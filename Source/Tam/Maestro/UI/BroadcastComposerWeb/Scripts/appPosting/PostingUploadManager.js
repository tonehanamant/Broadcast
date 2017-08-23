//Extend app/UploadManager to handle Posting specifics
//upload button and dragging supported

var PostingUploadManager = UploadManager.extend({
    initViewEvents: function () {

        $("#uploadButton").on("change", this.handleUploadSelectedFile.bind(this));
        $('.overlay').on('click', function () { $('.overlay').hide(); });
    },

    //override base version
    handleLoadedFiles: function (files) {
        var self = this;
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
                    var ext = file.name.substr((file.name.lastIndexOf('.') + 1)).toLowerCase();

                    switch (ext) {
                        case 'xlsx':
                            var postingFile = {
                                UserName: "user",
                                FileName: file.name,
                                RawData: b64,
                                BvsStream: null
                            }
                            self.view.processUploadPostingFile(postingFile);
                            break;

                        default:
                            util.notify('Invalid file format. Please provide an xlsx file', 'danger');
                            break;
                    }
                }.bindToEventHandler(file));
        }
    }
})