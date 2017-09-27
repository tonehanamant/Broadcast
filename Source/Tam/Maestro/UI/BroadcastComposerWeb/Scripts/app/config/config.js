//global app config items - shared across views/controllers
var config = {

    //global status states
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
};