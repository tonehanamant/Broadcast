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
};