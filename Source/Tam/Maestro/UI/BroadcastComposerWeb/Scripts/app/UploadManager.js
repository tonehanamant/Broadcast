//common Upload Management base - extended versions should handle custom handling specific to file types and logic
//this version processes xml files (i.e rate)
//uses some features from:
// Custom cross-browser implementation for binding a handler to an event 
// http://www.htmlgoodies.com/html5/javascript/drag-files-into-the-browser-from-the-desktop-HTML5.html#fbid=KCLVDJr8yhi
//could provide element properties for dropzone/overlay/button/ file type etc

//adding dragEnabled

var UploadManager = Class.extend({

    view: null,
    dragEnabled: true, //determines if show drag overlay and allow drop (in most modal modes will not)
    //this is called by Class
    init: function (view) {
        this.view = view;
        this.handlerExtensions();
        this.initDropzone();
        this.initViewEvents();
    },

    //initilaize events in view that are handled here; overide for specific
    initViewEvents: function () {
        $('#uploadButton').on('change', this.handleUploadSelectedFile.bind(this));
        $('.overlay').on('click', function () { $('.overlay').hide(); });
    },

    handlerExtensions: function () {
        Function.prototype.bindToEventHandler = function bindToEventHandler() {
            var handler = this;
            var boundParameters = Array.prototype.slice.call(arguments);

            //create closure
            return function (e) {
                e = e || window.event; // get window.event if e argument missing (in IE)   
                boundParameters.unshift(e);
                handler.apply(this, boundParameters);
            }
        };
    },

    addEventHandler: function (obj, evt, handler) {
        if (obj.addEventListener) {
            obj.addEventListener(evt, handler, false);
        } else if (obj.attachEvent) {
            obj.attachEvent('on' + evt, handler);
        } else {
            obj['on' + evt] = handler;
        }
    },

    //default - override for specific
    initDropzone: function () {
        var me = this;
        $(window).bind('dragover', dragover);
        $(window).bind('drop', drop);
        $('.overlay').bind('dragleave', dragleave);

        function dragover(event) {
            event.stopPropagation();
            event.preventDefault();
            if (me.dragEnabled) $('.overlay').show();
        }

        function dragleave(event) {
            event.stopPropagation();
            if (me.dragEnabled) $('.overlay').hide();
        }

        function drop(event) {
            if (me.dragEnabled) {
                me.handleLoadedFiles(event.originalEvent.dataTransfer.files);

                event.preventDefault();
                event.stopPropagation();

                $('.overlay').hide();
            }
        }
    },

    //upload button handler
    handleUploadSelectedFile: function () {

        //this event is only called if the file changes (not reselected)
        var fileInput = document.getElementById("uploadButton");
        //console.log('handleUploadSelectedFile', fileInput.value);
        var files = fileInput.files;
        this.handleLoadedFiles(files);
        //remove value so is set again for change
        fileInput.value = null;
    },

    //process files with specific logic as needed - override for specific
    handleLoadedFiles: function (files) {
        var me = this;
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            var reader = new FileReader();
            var allLoaded = false;
            if (i == files.length - 1) {
                allLoaded = true;
            }

            reader.readAsDataURL(file);
            this.addEventHandler(reader,
                'loadend',
                function (e, file) {
                    var bin = this.result;
                    var b64 = bin.split("base64,")[1];
                    var ext = file.name.substr((file.name.lastIndexOf('.') + 1)).toLowerCase();//allow various cases

                    switch (ext) {
                        case 'xml':
                            var rateRequest = {
                                UserName: "user",
                                FileName: file.name,
                                RawData: b64,
                                BvsStream: null
                            }
                            me.view.processUploadFileRequest(rateRequest);
                            break;

                        default:
                            util.notify('Invalid file format. Please, provide an .xml File', 'danger');
                            break;
                    }
                }.bindToEventHandler(file));
        }
    }

});