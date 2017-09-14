var util = {

    appNotify: null,
    //return reusable function closure to show/hide element (el should be JQ object not string selector);
    //called function will return el
    toggleDisplay: function (el) {
        return function (showOrHide) {
            return el.toggle(showOrHide);
        };
    },

    alertModal: null,
    confirmModal: null,
    busyModal: null,

    alert: function (title, message) {
        if (!this.alertModal) {
            this.alertModal = $("#alertModal").modal({ show: false });
        }
        this.alertModal.find('.modal-title').html(title);
        this.alertModal.find('.modal-body').html(message);
        this.alertModal.modal('show');
    },

    confirm: function (title, message, confirmCommand, cancelCommand, confirmBtnText, cancelBtnText) {
        if (!this.confirmModal) {
            this.confirmModal = $("#confirmModal").modal({ show: false });
        }
        this.confirmModal.find('.modal-title').html(title);
        this.confirmModal.find('.modal-body').html(message);

        var confirmBtn = this.confirmModal.find('#confirmButton');
        var cancelBtn = this.confirmModal.find('#confirmCancelButton');

        confirmBtn.off('click').on('click', confirmCommand);
        if (cancelCommand) cancelBtn.off('click').on('click', cancelCommand);

        if (cancelBtnText) {
            cancelBtn.html(cancelBtnText);
        }

        if (confirmBtnText) {
            confirmBtn.html(confirmBtnText);
        }

        this.confirmModal.modal('show');
    },

    notify: function (msg, type, options, settings) {
        if (this.appNotify) this.appNotify.close();
        var types = { success: 'glyphicon glyphicon-thumbs-up', warning: 'glyphicon glyphicon-warning-sign', info: 'glyphicon glyphicon-info-sign', danger: 'glyphicon glyphicon-warning-sign' };
        type = (type && types[type]) ? type : 'success';
        var opts = {
            message: msg,
            icon: types[type]
        };
        var sets = {
            type: type,
            delay: 3000,
            z_index: 9999
        };
        if (options) $.extend(opts, options);
        if (settings) $.extend(sets, settings);
        this.appNotify = $.notify(opts, sets);
    },

    createEnumSelectObject: function (id, text) {
        var obj = {
            id: id,
            text: text || id,
            hidden: false
        }

        return obj;
    },

    sortEnumSelectArray: function (a, b) {
        if (a.text < b.text) return -1;
        if (a.text > b.text) return 1;
        return 0;
    },

    //scroll to - pass jquery el, container
    scrollToElement: function ($el, $container) {
        //https://stackoverflow.com/questions/2905867/how-to-scroll-to-specific-item-using-jquery
        var offset = $el.offset().top - $container.offset().top + $container.scrollTop();
        var offset2 = $el.offset().top;
        //account for centering
        var offset3 = $el.offset().top - $container.offset().top + $container.scrollTop() - ($container.height() / 2);
        //console.log('scrollToElement', offset, offset2, offset3, $el, $container);
        $container.scrollTop(offset3);

    },

    formatNumber: function (number) {
        return number.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
    },
    //recursive copy/ apply merge
    //optional prop
    //allow quick copy array. Array.isArray (ES5) JS $.isArray([])
    //add deep option
    copyData: function (data, xtra, prop, deep) {
        deep = deep ? true : false;
        if ($.isArray(data)) return util.copyArray(data);
        //pass true first argument for recursive merge
        var ret = $.extend(deep, {}, data, xtra || {});
        return prop ? (ret[prop] || null) : ret;
    },

    copyArray: function (arr) {
        //this does not return deep copy
        //return arr.slice(0);
        return $.extend(true, [], arr);
    },

    //check has array items - return boolean, else optional count
    hasDataArray: function (arr, count) {
        var cnt = (arr && arr.length);
        //!!cnt ? or !==0 ?
        return count ? cnt : !!cnt;
    },

    //add a property (name) to array items based on (prop) value elsewhere in array
    //used to add proprietary ids to data like grid plug in which requires recid prop as id
    addValToArray: function (data, name, prop) {
        $.each(data, function (index, value) {
            value[name] = value[prop];
        });
        return data;
    },

    //source is array you want values from; target is to compare and if match - add to new array which can be empty
    compareArrays: function (source, sourceName, target, targetName) {
        var ret = [];
        $.each(source, function (index, value) {
            var id = value[sourceName];
            if (id && util.objectFindByKey(target, targetName, id)) {
                ret.push(value);
            }
        });
        return ret;
    },

    getIndexByKey: function (array, key, value) {
        for (var i = 0; i < array.length; i++) {
            if (array[i][key] === value) {
                return i;
            }
        }
        return -1;
    },

    //find item in array by key/value
    objectFindByKey: function (array, key, value) {
        for (var i = 0; i < array.length; i++) {
            if (array[i][key] === value) {
                return array[i];
            }
        }
        return null;
    },

    //remove item in array by key/value - return removed item;
    removeFromArray: function (array, key, value) {
        for (var i = 0; i < array.length; i++) {
            if (array[i][key] === value) {
                var ret = array[i];
                array.splice(i, 1);
                return ret;
            }
        }
        return null;
    },

    //save file to local computer; pass full filename and stringified JSON
    saveAsJSON: function (filename, json) {
        var pom = document.createElement('a');
        pom.setAttribute('href', 'data:text/json;charset=utf8,' + encodeURIComponent(json));
        pom.setAttribute('download', filename);
        pom.click();
    },

    //JSON.stringify with formatting
    jsonDisplay: function (item, format) {
        format = format || 4;
        var ret = JSON.stringify(item, null, format);
        return ret;
    },

    //localStorage - get
    getLocalStorage: function (name, parse) {
        var ret = localStorage[name];
        if (ret) return parse ? JSON.parse(ret) : ret;
        return null;
    },

    //localStorage - save
    saveLocalStorage: function (name, item) {
        var ret = JSON.stringify(item);
        localStorage[name] = ret;
        return ret;
    },

    //localStorage - clear
    clearLocalStorage: function (name) {
        if (name) return localStorage.removeItem(name);
        localStorage.clear();//returns anything?
    },

    //sessionStorage - get
    getSessionStorage: function (name, parse) {
        var ret = sessionStorage[name];
        if (ret) return parse ? JSON.parse(ret) : ret;
        return null;
    },

    //sessionStorage - save
    saveSessionStorage: function (name, item) {
        var ret = JSON.stringify(item);
        sessionStorage[name] = ret;
        return ret;
    },

    //sessionStorage - clear
    clearSessionStorage: function (name) {
        if (name) return sessionStorage.removeItem(name);
        sessionStorage.clear();//returns anything?
    },

    // Read a page's GET URL variables and return them as an associative array.
    //http://www.example.com/?me=myValue&name2=SomeOtherValue
    // var first = getUrlVars()["me"];
    getUrlVars: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },

    //get file extension ("xml")
    getFileExtension: function (path) {
        var ext = path.slice(path.lastIndexOf('.') + 1).toLowerCase();
        return ext;
    },

    //get filename either unix/windows
    getFilenameFromPath: function (path) {
        //path = path.split("\\");
        return path.replace(/^.*[\\\/]/, '');
    },

    //alternate
    fileName: function (path, stripExt) {
        var nameStart = Math.max(path.lastIndexOf("/") + 1, path.lastIndexOf("\\") + 1, 0);
        var fname = path.substring(nameStart);
        if (stripExt) return fname.substr(0, fname.lastIndexOf('.'));
        return fname;
    },

    stripHttp: function (url) {
        var protomatch = /^(https?|ftp):\/\//;
        return url.replace(protomatch, '');
    },

    //console msg
    log: function (msg, prop) {
        prop = prop || '>>>';
        return console.log(msg, prop);
    },

    generateUUID: function () {
        var d = new Date().getTime();
        if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
            d += performance.now();
        }

        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }
};