//Rate Upload Management - extended version of app/UploadManager to handle different types of Rate Files

// - initially will block certain types until each is set up

//see UploadManager  dragEnabled

var RateUploadManager = UploadManager.extend({

    // isUploadEnabled: true, //temporary - prevent uploads for certain types


    sourceTypes: {
        OpenMarket: {
            name: 'OpenMarket',
            enabled: true,
            isSingleFile: false,
            fileType: 'xml',
            checkType: function (fileExt) {
                return fileExt == 'xml';
            }
        },
        Assembly: {
            name: 'Assembly',
            enabled: true,
            isSingleFile: false,
            fileType: 'csv',
            checkType: function (fileExt) {
                //return fileExt == ('xls' || 'xlsx');
                return fileExt == 'csv';
            }
        },
        TVB: {
            name: 'TVB',
            enabled: true,
            isSingleFile: true,//single file only
            fileType: 'csv',//TBD
            checkType: function (fileExt) {
                return fileExt == 'csv';
            }
        },
        //todo
        CNN: {
            name: 'CNN',
            enabled: true,
            isSingleFile: true,//single file only
            fileType: 'xls',
            checkType: function (fileExt) {
                return fileExt == 'xls' || fileExt == 'xlsx';
            }
        },
        TTNW: {
            name: 'TTNW',
            enabled: true,
            isSingleFile: true, //single file only
            fileType: 'xls',
            checkType: function (fileExt) {
                return fileExt == 'xls' || fileExt == 'xlsx';
            }
        }

    },

    activeSourceType: null,

    //add change of file input multiple or remove attribute
    setActiveSourceType: function (type) {
        this.activeSourceType = this.sourceTypes[type] || null;
        var multiple = this.activeSourceType.isSingleFile ? false : true;
        this.setMultipleFileUpload(multiple);
    },


    //override version - process different types
    //TBD based on types - initially prevent
    //REVISED - for some send as multiple files
    handleLoadedFiles: function (files) {
        var source = this.activeSourceType;
        var validRequests = [];
        if (source && source.enabled) {
            var canProceed = true;
            //if single then use first file only
            if (source.isSingleFile) {
                files = [files[0]];
            }
            var me = this;
            //determine last file and compare below to ensure last loded by reader
            var lastFile = files[files.length - 1];
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                var reader = new FileReader();
                /*
                var allLoaded = false;//not used this context
                if (i == files.length - 1) {
                    allLoaded = true;
                }
                */

                //reader.readAsDataURL(file);
                this.addEventHandler(reader,
                    'loadend',
                    function (e, file) {
                        var bin = this.result;
                        var b64 = bin.split("base64,")[1];
                        var ext = file.name.substr((file.name.lastIndexOf('.') + 1)).toLowerCase();//allow various cases
                        //console.log('file Ext', ext);
                        if (source.checkType(ext)) {

                            var rateRequest = {
                                UserName: "user",
                                InventorySource: source.name, //add here or use controller?
                                FileName: file.name,
                                RawData: b64
                            };

                            //handle as multiple
                            validRequests.push(rateRequest);
                            // me.view.processUploadFileRequest(rateRequest, source.name);

                        } else {
                            canProceed = false;
                            util.notify('Invalid file format. Please, provide a .' + source.fileType + ' File', 'danger');
                        }
                        var done = (file == lastFile);
                        //console.log('last', done);
                        if (done && canProceed) me.view.processUploadFileRequest(validRequests, source.name);
                    }.bindToEventHandler(file));
                reader.readAsDataURL(file);
            }

        } else {
            util.notify('File Upload is not yet available for this Rate Type.', 'warning');
        }
    },

    setMultipleFileUpload: function (isMultiple) {

        if (isMultiple) {
            $('#uploadButton').attr('multiple', true);
        } else {

            $('#uploadButton').removeAttr('multiple');
        }

    }


});
