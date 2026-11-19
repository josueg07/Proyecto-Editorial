# Fuentes y portadas

Revisión del 19/11/2026: [inventario y resultados de las 20 portadas](REVISION-PORTADAS.md), con [CSV por ISBN](PORTADAS-LIBROS.csv). No se encontró autorización verificable de redistribución de las cubiertas editoriales; se conserva la imagen predeterminada local. El informe distingue disponibilidad pública, descarga personal y redistribución.

`FUENTES-LIBROS.csv` registra 20 títulos, 20 ISBN impresos y tres ISBN digitales. Las fuentes enlazadas son fichas de las editoriales, consultadas el 13/11/2026. Se comprobó además el dígito de control de cada ISBN: esto complementa, pero no sustituye, la correspondencia editorial de la ficha.

La fecha de la edición de *El fin de la eternidad* se corroboró también en [Casa del Libro](https://www.casadellibro.com/libro-el-fin-de-la-eternidad/9788497933537/970833). El año mostrado por el modelo corresponde a la edición impresa; no hay una columna de año por presentación para las ediciones digitales. Las categorías son clasificaciones del proyecto y las sinopsis se redactaron para este catálogo, sin copiar las descripciones editoriales.

Los precios e inventarios son exclusivamente académicos y no representan precios ni existencias de las editoriales. Se incorporan 120 ejemplares por edición impresa nueva. Repetir el script no repone inventario ni sobrescribe precios. Los formatos digitales no incluyen archivos de las obras ni una licencia de distribución; este proyecto registra su pedido para coordinación manual, sin simular descargas.

No se copiaron imágenes editoriales sin autorización comprobada. Para colocar una portada autorizada, guarda el archivo en `PlataformaEditorial.Web/wwwroot/images/portadas/{ISBN}.jpg` y usa `/images/portadas/{ISBN}.jpg` en Editar libro. Comprueba que el ISBN coincida con la edición de la imagen. Mientras tanto se utiliza `/images/sin-portada.svg`.

La fuente de Orgullo y prejuicio corresponde a la edición de septiembre de 2026 con ilustraciones de C. E. Brock; la autoría literaria sigue siendo Jane Austen. Se registran autores literarios y se conservan los traductores e ilustradores en las fichas enlazadas, porque el modelo actual no distingue esas funciones.
