//Tracker Upload Management - extended version of app/UploadManager to handle SCX and BVS files
//REVISE: will handle additional types; remove drag on modal (single only); remove isScxMode - see UploadManager  dragEnabled

var TrackerUploadManager = UploadManager.extend({

    //isScxMode: false, //set by view when in scxMode (modal)

    //initilaize events in view that are handled here; overide version
    initViewEvents: function () {

        $("#uploadButton").on("change", this.handleUploadSelectedFile.bind(this));
        $('.overlay').on('click', function () { $('.overlay').hide(); });
        //provides trigger to upload button from scx Modal - deprecate
        //$("#scx_upload_files").on("click", function () {
        //    $("#uploadButton").trigger('click');
        //});

        //$('#ftpUploadButton').on('click', this.handleFtpUploadFile.bind(this));
    },

    //handleFtpUploadFile: function () {
    //    $('#ftpUploadButton').attr("disabled", "disabled");
    //    this.view.uploadFtp();
    //},

    //override version - scx
    //deprecated - let base class handle as dragEnabled
    /*initDropzone: function () {
        var me = this;

        $(window).bind('dragover', dragover);
        $(window).bind('drop', drop);
        $('.overlay').bind('dragleave', dragleave);

        function dragover(event) {
            event.stopPropagation();
            event.preventDefault();
            if (!me.isScxMode) $('.overlay').show();
        }

        function dragleave(event) {
            event.stopPropagation();
            if (!me.isScxMode) $('.overlay').hide();
        }

        function drop(event) {
            me.handleLoadedFiles(event.originalEvent.dataTransfer.files);

            event.preventDefault();
            event.stopPropagation();

            if (!me.isScxMode) $('.overlay').hide();
        }
    },
    */


    //override version - process as DAT, SCX/CSV, XLS/XLSX
    handleLoadedFiles: function (files) {
        var me = this;
        var scheduleRequestList = [];//store as array but only process first
        var bvsRequestList = {
            UserName: "user",
            BvsFiles: []
        };

        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            var reader = new FileReader();
            var allLoaded = false;
            if (i === files.length - 1) {
                allLoaded = true;
            }

            reader.readAsDataURL(file);
            this.addEventHandler(reader,
                'loadend',
                function (e, file) {
                    var bin = this.result;
                    var b64 = bin.split("base64,")[1];
                    var ext = file.name.substr((file.name.lastIndexOf('.') + 1));
                    ext = ext.toLowerCase();

                    switch (ext) {
                        case 'xls':
                        case 'xlsx':
                            var bvsRequest = {
                                BvsFileName: file.name,
                                RawData: b64,
                                BvsStream: null
                            }

                            bvsRequestList.BvsFiles.push(bvsRequest);

                            if (bvsRequestList.BvsFiles.length >= files.length) {
                                me.view.processUploadBvsFileRequest(bvsRequestList);
                            }
                            break;
                        case 'csv':
                        case 'scx':
                            var scheduleFileData = {
                                FileName: file.name,
                                FileType: ext,  //add in for use in VM/BE?
                                UserName: "user",
                                RawData: b64
                            }
                            //TODO allow only 1 - pass one file only
                            scheduleRequestList.push(scheduleFileData);

                            if (allLoaded) {

                                //if (scxRequestList.length > 0) {
                                //    if (me.isScxMode) {
                                //        me.view.processUploadScxFileRequest(false, scxRequestList);
                                //    } else {
                                //        me.view.processUploadScxFileRequest(true, scxRequestList);
                                //    }
                                //}
                                //only process first
                                me.view.processUploadScheduleFileRequest(scheduleRequestList[0]);
                            }
                            break;
                        default:
                            util.notify('Invalid file format. Please, provide .dat, .scx .csv or an Excel file.', 'danger');
                            break;
                    }
                }.bindToEventHandler(file));
        }
    }
});
