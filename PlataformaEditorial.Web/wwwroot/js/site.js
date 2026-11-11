// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sustituye portadas ausentes o enlaces externos que ya no estén disponibles.
document.querySelectorAll('.portada-catalogo, .portada-detalle').forEach(function (imagen) {
    imagen.addEventListener('error', function () {
        if (!imagen.src.endsWith('/images/sin-portada.svg')) imagen.src = '/images/sin-portada.svg';
    });
});
