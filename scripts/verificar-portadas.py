"""Verifica archivos y asociaciones; solo lee datos y hace GET a localhost.

python scripts/verificar-portadas.py
python scripts/verificar-portadas.py --base-url http://localhost:5172
Sin dependencias externas; Python 3.10 o posterior.
"""
import argparse
import csv
import hashlib
import re
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import urlencode, urlsplit
from urllib.request import urlopen

ROOT = Path(__file__).resolve().parents[1]
WEB = ROOT / "PlataformaEditorial.Web/wwwroot"


class Pagina(HTMLParser):
    def __init__(self):
        super().__init__()
        self.imagenes = []
        self.detalles = []

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "img" and {"portada-catalogo", "portada-detalle"}.intersection(attrs.get("class", "").split()):
            self.imagenes.append(attrs)
        if tag == "a" and re.fullmatch(r"/Catalogo/Detalles/\d+", attrs.get("href", "")):
            self.detalles.append(attrs["href"])


def verificar(base_url=None):
    with (ROOT / "docs/PORTADAS-LIBROS.csv").open(encoding="utf-8-sig") as f:
        libros = list(csv.DictReader(f))
    with (ROOT / "docs/FUENTES-LIBROS.csv").open(encoding="utf-8-sig") as f:
        fuentes = {r["ISBN"]: r["Titulo"] for r in csv.DictReader(f) if r["Formato"] == "Papel"}
    assert len(libros) == len(fuentes) == 20
    assert {r["ISBN"] for r in libros} == set(fuentes)
    esperadas = {}
    for libro in libros:
        isbn, ruta = libro["ISBN"], libro["Ruta"]
        assert libro["Titulo"] == fuentes[isbn]
        assert re.fullmatch(r"/images/portadas/" + isbn + r"\.(jpg|jpeg|png|webp)", ruta, re.I)
        archivo = ROOT / libro["Archivo"]
        assert archivo.resolve() == (WEB / ruta.lstrip("/")).resolve()
        datos = archivo.read_bytes()
        assert len(datos) == int(libro["Bytes"])
        assert hashlib.sha256(datos).hexdigest() == libro["SHA256"], archivo
        esperadas[isbn] = ruta
    archivos = {p.name for p in (WEB / "images/portadas").iterdir() if p.suffix.lower() in {".jpg", ".jpeg", ".png", ".webp"}}
    assert archivos == {Path(r).name for r in esperadas.values()}, "Imagenes faltantes o sin correspondencia"
    for nombre in ("libros-reales.sql", "asociar-portadas-locales.sql"):
        sql = (ROOT / "scripts" / nombre).read_text(encoding="utf-8-sig")
        asociaciones = re.findall(r"N'(\d{13})'[^\n]*?N'(/images/portadas/[^']+)'", sql)
        assert len(asociaciones) == 20 and dict(asociaciones) == esperadas, nombre
    print("OK: 20 archivos, hashes originales y asociaciones identicas en ambos scripts; 0 faltantes y 0 sin correspondencia.")
    if not base_url:
        return
    assert urlsplit(base_url).hostname in {"localhost", "127.0.0.1"}, "Utilice el servidor local"
    base_url = base_url.rstrip("/")
    recursos, paginas, referencias = set(), 0, 0

    def comprobar(ruta, libro=None):
        nonlocal paginas, referencias
        with urlopen(base_url + ruta, timeout=30) as respuesta:
            assert respuesta.status == 200
            pagina = Pagina()
            pagina.feed(respuesta.read().decode("utf-8"))
        assert pagina.imagenes, ruta
        for img in pagina.imagenes:
            src = img["src"]
            assert src.startswith("/images/") and ".." not in src and not urlsplit(src).netloc
            if src not in recursos:
                with urlopen(base_url + src, timeout=15) as respuesta:
                    assert respuesta.status == 200 and respuesta.headers["Content-Type"].startswith("image/")
                    assert respuesta.read() == (WEB / src.lstrip("/")).read_bytes()
                recursos.add(src)
        if libro:
            assert any(i["src"] == libro["Ruta"] and i.get("alt") == "Portada de " + libro["Titulo"] for i in pagina.imagenes), ruta
        paginas += 1
        referencias += len(pagina.imagenes)
        return pagina

    for ruta in ("/", "/Catalogo", "/Catalogo?Pagina=2"):
        comprobar(ruta)
    for libro in libros:
        resultado = comprobar("/Catalogo?" + urlencode({"Texto": libro["ISBN"]}), libro)
        assert len(set(resultado.detalles)) == 1, libro["ISBN"]
        comprobar(resultado.detalles[0], libro)
    assert set(esperadas.values()).issubset(recursos)
    print(f"OK HTTP: {paginas} paginas, {referencias} referencias locales, 20 portadas servidas con sus bytes originales.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-url")
    verificar(parser.parse_args().base_url)
