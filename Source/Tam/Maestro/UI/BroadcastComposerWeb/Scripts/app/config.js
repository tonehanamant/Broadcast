$(function() {

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


//global app config items - shared across views/controllers

var config = {
    devMode: true,  //if true then apps will: show console logging; 

    //file status map: text color style
    fileStatusStates: {
        QUEUED: ['status-blue', 'Queued'],
        IN_REVIEW: ['status-blue', 'In Review'],
        READY_FOR_APPROVAL: ['status-orange', 'Ready For Approval'],
        APPROVED: ['status-green', 'Approved'],
        COMPLETED: ['status-green', 'Completed'],
        CANCELLED: ['status-orange', 'Cancelled'],
        ERROR_PRODUCTION_LOAD: ['status-red', 'Error In Maestro Update'],
        ERROR: ['status-red', 'Error'],
        DEFAULT: ['status-blue', 'Ready']
    },
    //tbd
    statusStates: {
        ready: ['info', 'Ready'],
        error: ['danger', 'Error'],
        success: ['success', 'Success'],
        alert: ['warning', 'Alert']
    },

    processingMsg: 'Processing...',

    defaultErrorMsg: 'The server encountered an error processing the request.  Please try again or contact your administrator to review error logs.',
    refreshMessage: 'Data has been refreshed to sync with changes on the server.',
    headError: 'An error was encountered Processing the Request',

    //globally used renderers (for grids, etc)
    renderers: {
        //returns val converted to money number format else dash if not value; option to allow 0 as value
        toMoneyOrDash: function (val, allowZero) {
            //test for val then allow zero test
           // return (val || (allowZero ? (val == 0) : false)) ? ('$' + (parseFloat(Math.round(val * 100) / 100).toFixed(2))) : '-';
            return (val || (allowZero ? (val == 0) : false)) ? ('$' + (w2utils.formatNumber(val.toFixed(2)))) : '-';
            
        },

        //returns val converted to number format else dash if not value; option to allow 0 as value
        toValueOrDash: function (val, allowZero) {
            //return val ? (parseFloat(Math.round(val * 100) / 100).toFixed(2)) : '-';
            //test for val then allow zero test
            return (val || (allowZero ? (val == 0) : false)) ? (parseFloat(Math.round(val * 100) / 100).toFixed(2)) : '-';
        }

    }
    
};