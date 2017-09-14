$(function () {

    // jq validate defaults - bootstrap error styles
    $.validator.setDefaults({
        errorElement: "span",
        errorClass: "help-block",

        highlight: function (element, errorClass, validClass) {
            $(element).closest('.form-group').addClass('has-error');
        },

        unhighlight: function (element, errorClass, validClass) {
            $(element).closest('.form-group').removeClass('has-error');
        },

        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length || element.prop('type') === 'checkbox' || element.prop('type') === 'radio' || element.prop('type').indexOf('select') > -1) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        },
    });

    // Custom validator default messages
    $.extend(jQuery.validator.messages, {
        required: "Required."
    });

    jQuery.validator.addMethod("alphanumeric", function (value, element) {
        return this.optional(element) || /^[a-zA-Z0-9]+$/.test(value);
    });
});