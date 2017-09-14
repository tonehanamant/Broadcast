var httpService = {
    get: function (url, success, error, args) {
        this.ajax(url, success, error, 'GET', args);
    },

    remove: function (url, success, error, args) {
        this.ajax(url, success, error, 'DELETE', args);
    },

    post: function (url, success, error, data, args) {
        args = args || {};
        args.Data = data;

        this.ajax(url, success, error, 'POST', args);
    },

    put: function (url, success, error, data, args) {
        args = args || {};
        args.Data = data;

        this.ajax(url, success, error, 'PUT', args);
    },

    ajax: function (url, success, error, method, args) {
        if (args.$ViewElement) {
            this.showProcessing(args.$ViewElement);
        }

        var me = this;
        var ajaxObj = {
            url: url,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            method: method,
            async: true,
            cache: false,
            success: function (result, status, xhr) {
                me.debug('[' + url + '] -', result);
                if (result.Success) {
                    if (result.Data || result.Data === false || result.Data === 0) {
                        state = 'success';
                        if (me.clearProcessing) me.clearProcessing();

                        if (result.Message)
                            me.showMessage(result.Message);

                        if (success)
                            success(result.Data);
                    } else {
                        state = 'error';
                        var msg = me.getApiErrorMsg(xhr, args.ErrorMessage);
                        me.showDefaultError(msg, args.TitleErrorMessage);
                       // console.log('AJAX error', error);
                        if (error) error(xhr);
                    }
                } else {
                    state = 'error';

                    if (error) {
                        error(xhr);
                        //dont bypass default error message
                        me.showDefaultError(result.Message);
                    } else {
                        me.showDefaultError(result.Message);
                    }
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                state = 'error';
                var msg = me.getApiErrorMsg(xhr, args.ErrorMessage);
                me.showDefaultError(msg, args.TitleErrorMessage);
               // console.log('AJAX error', error);
                if (error) error(xhr);
                me.debug('error', xhr);
            },
            complete: function (xhr, textStatus) {
                if (state == 'error' && me.clearProcessing) {
                    me.clearProcessing();
                }
                me.setStatus(state, args.StatusMessage);
            }
        };
        if (method == 'POST' || method == 'PUT') {
            ajaxObj.processData = false;
            ajaxObj.data = args.Data;
        }

        if (method == 'GET' && args.data) {
            console.log('Get', args.data);
            ajaxObj.processData = true;
            ajaxObj.data = args.data;
        }

        $.ajax(ajaxObj);
    },

    clearProcessing: null,

    showProcessing: function (el, msg) {
        msg = msg || config.processingMsg;
        w2utils.lock(el, msg, true);
        this.clearProcessing = function () {
            w2utils.unlock(el);
        };
    },

    //show default error modal - dont neeed refresh option?
    showDefaultError: function (msg, headtxt, showRefresh) {
        msg = msg || config.defaultErrorMsg;
        headtxt = headtxt || config.headError;
        var mrkup = '<p>' + msg + '</p>';
        if (showRefresh) mrkup += '<p>' + config.refreshMessage + '</p>';
        $('#default_error_modal').modal({ backdrop: false });
        $('#default_error_modal').modal('show');
        if (headtxt) $("#default_error_text").html(headtxt);
        $("#default_error_message").html(mrkup).css("white-space", "pre-wrap");//css force returned messages to recognize line breaks
    },

    showMessage: function (msg, headtxt, showRefresh) {
        var mrkup = '<p>' + msg + '</p>';
        if (showRefresh) mrkup += '<p>' + config.refreshMessage + '</p>';
        $('#default_success_modal').modal({ backdrop: false });
        $('#default_success_modal').modal('show');
        if (headtxt) $("#default_message_text").html(headtxt);
        $("#default_message").html(mrkup).css("white-space", "pre-wrap");
    },

    getApiErrorMsg: function (xhr, api, txt) {
        txt = txt || 'Problem with API Request';
        var ret = txt + ' - ' + api + '<br>';
        var message = config.defaultErrorMsg;
        if (xhr.responseJSON) {
            if (xhr.responseJSON.InnerException && xhr.responseJSON.InnerException.ExceptionMessage) {
                message = xhr.responseJSON.InnerException.ExceptionMessage;
            } else {
                if (xhr.responseJSON.ExceptionMessage) {
                    message = xhr.responseJSON.ExceptionMessage;
                }
            }
        } else {
            //connection errors return all html - remove responseText
            //if (xhr.responseText) message += ('<br>' + xhr.responseText);
            if (xhr.status) message += ' | Message:  ' + xhr.status;
        }
        ret += message;
        return ret;
    },

    debug: function (msg, prop) {
        if (this.appDebug) util.log(msg, prop);
    },

    statusCls: 'alert-info',

    setStatus: function (status, msg) {
        //add option not to set if not 'error'?
        var statusCfg = config.statusStates[status];
        var mrk = '<strong>Status: ' + statusCfg[1] + '</strong>';
        if (msg) mrk += ' - ' + msg;
        var cls = 'alert-' + statusCfg[0];
        $("#broadcast_status").removeClass(this.statusCls).addClass(cls).html(mrk);
        this.statusCls = cls;
    },
};