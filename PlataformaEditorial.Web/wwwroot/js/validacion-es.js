if (window.jQuery && jQuery.validator) {
    jQuery.extend(jQuery.validator.messages, {
        required: 'Este campo es obligatorio.', email: 'Escribe un correo electrónico válido.',
        number: 'Escribe un número válido.', digits: 'Escribe solo dígitos.', equalTo: 'Los valores deben coincidir.',
        minlength: jQuery.validator.format('Escribe al menos {0} caracteres.'), maxlength: jQuery.validator.format('Escribe como máximo {0} caracteres.'),
        range: jQuery.validator.format('Escribe un valor entre {0} y {1}.'), min: jQuery.validator.format('El mínimo es {0}.'), max: jQuery.validator.format('El máximo es {0}.'),
        date: 'Escribe una fecha válida.', url: 'Escribe una dirección web válida.'
    });
}
