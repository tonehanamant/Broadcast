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
            fileType: 'csv',
            checkType: function (fileExt) {
                return fileExt == 'csv';
            }
        },
        TTNW: {
            name: 'TTNW',
            enabled: true,
            isSingleFile: true, //single file only
            fileType: 'csv',
            checkType: function (fileExt) {
                return fileExt == 'csv';
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
    handleLoadedFiles: function (files) {
        var source = this.activeSourceType;
        if (source && source.enabled) {
            //if single then use first file only
            if (source.isSingleFile) {
                files = [files[0]];
            }
            var me = this;
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                var reader = new FileReader();
                var allLoaded = false;//not used this context
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
                        //console.log('file Ext', ext);
                        if (source.checkType(ext)) {

                            var rateRequest = {
                                UserName: "user",
                                RateSource: source.name, //add here or use controller?
                                FileName: file.name,
                                RawData: b64
                            };
                            me.view.processUploadFileRequest(rateRequest, source.name);

                        } else {
                            util.notify('Invalid file format. Please, provide a .' + source.fileType + ' File', 'danger');
                        }
                    }.bindToEventHandler(file));
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
