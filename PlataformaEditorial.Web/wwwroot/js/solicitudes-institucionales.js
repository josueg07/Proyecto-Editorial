(function () {
    const form = document.getElementById('solicitud-institucional');
    const obras = document.getElementById('obras-solicitadas');
    const aviso = document.getElementById('aviso-obras');
    if (!form || !obras) return;
    function actualizar() {
        obras.querySelectorAll('.obra-solicitada').forEach(function (obra, indice) {
            obra.querySelector('legend').textContent = 'Obra ' + (indice + 1);
            obra.querySelectorAll('input').forEach(function (input) {
                input.name = input.name.replace(/Detalles\[\d+\]/g, 'Detalles[' + indice + ']');
                input.id = input.id.replace(/Detalles_\d+__/g, 'Detalles_' + indice + '__');
            });
            obra.querySelectorAll('label').forEach(function (label) {
                label.htmlFor = label.htmlFor.replace(/Detalles_\d+__/g, 'Detalles_' + indice + '__');
            });
        });
        if (window.jQuery && jQuery.validator) {
            jQuery(form).removeData('validator').removeData('unobtrusiveValidation');
            jQuery.validator.unobtrusive.parse(form);
        }
    }
    document.getElementById('agregar-obra').addEventListener('click', function () {
        if (obras.children.length >= 20) { aviso.textContent = 'El máximo es veinte obras.'; return; }
        const copia = obras.querySelector('.obra-solicitada').cloneNode(true);
        copia.querySelectorAll('input').forEach(function (input) {
            input.value = input.name.endsWith('.Cantidad') ? '1' : '';
            input.classList.remove('input-validation-error');
        });
        copia.querySelectorAll('.field-validation-error, label.error').forEach(function (elemento) { elemento.remove(); });
        obras.appendChild(copia); aviso.textContent = ''; actualizar();
    });
    obras.addEventListener('click', function (evento) {
        if (!evento.target.classList.contains('quitar-obra')) return;
        if (obras.children.length <= 1) { aviso.textContent = 'Debe conservar al menos una obra.'; return; }
        evento.target.closest('.obra-solicitada').remove(); aviso.textContent = ''; actualizar();
    });
})();
