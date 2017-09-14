//Main Post Controller
var PostingMainController = BaseController.extend({
    view: null,
    viewModel: null,
    postingData: null,
    apiUrl: baseUrl + "api/Post/",

    initController: function () {
        this.viewModel = new PostingUploadViewModel(this);
        ko.applyBindings(this.viewModel, document.getElementById("posting-upload-view"));

        this.view = new PostingMainView(this);
        this.view.initView(this);

        this.apiGetComponents();
    },

    ////////////////////////// API
    apiGetComponents: function () {
        var self = this;
        var url = self.apiUrl + 'InitialData';

        httpService.get(url,
            self.onApiGetComponents.bind(self),
            null,
            {
                $ViewElement: $('#posting_view'),
                ErrorMessage: 'Initial Data',
                TitleErrorMessage: 'No Components Data Returned',
                StatusMessage: 'Initial Data'
            });
    },

    onApiGetComponents: function (data) {
        var self = this;
        if (data) {
            self.viewModel.initComponents(data);
        }
    },

    //get all posts for grid
    apiGetInitialPostings: function (callback, errorCallback) {
        var self = this;

        httpService.get(self.apiUrl,
           callback.bind(this),
           errorCallback ? errorCallback.bind(this) : null,
           {
               $ViewElement: $("#posting_view"),
               ErrorMessage: "Load Posts",
               TitleErrorMessage: "No Post Data Returned",
               StatusMessage: "Load Posts"
           });
    },

    apiGetPostingById: function (postingId) {
        var self = this;
        var url = self.apiUrl + postingId;

        httpService.get(url,
            this.onApiGetPostingById.bind(this),
            null,
            {
                $ViewElement: $("#posting_view"),
                ErrorMessage: "Load Posts",
                TitleErrorMessage: "No Post Data Returned",
                StatusMessage: "Load Posts"
            });
    },

    onApiGetPostingById: function (data) {
        var self = this;
        self.viewModel.preparePostingEditForm(data);
        self.viewModel.preparePostingRemoval(data);
    },

    //upload Posting file
    apiUploadPostingFile: function (postingFileRequest) {
        var self = this;
        var jsonObj = JSON.stringify(postingFileRequest);
        httpService.post(self.apiUrl,
            self.onApiUploadPostingFile.bind(self),
            null,
            jsonObj,
            {
                $ViewElement: $("#posting_view"),
                ErrorMessage: "Upload Posts",
                TitleErrorMessage: "No Post Data Returned",
                StatusMessage: "Upload Posts"
            });
    },

    //manage Posting upload return
    onApiUploadPostingFile: function () {
        var self = this;
        this.view.onAfterPostingEvent();
        util.notify("Posting File Uploaded");
    },

    //save edits from File Settings
    apiSavePostingFile: function (posting, callback, errorCallback) {
        var self = this;
        var JsonObj = JSON.stringify(posting);

        httpService.put(self.apiUrl,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            JsonObj,
            {
                $ViewElement: $("#posting_view"),
                ErrorMessage: "Save Posting",
                TitleErrorMessage: "No Posting Data Returned",
                StatusMessage: "Save Posting"
            });
    },

    //remove a file from the grid
    apiDeletePostingFile: function (postingId, callback) {
        var self = this;
        var url = baseUrl + "api/Post/" + postingId;
        httpService.remove(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $("#posting_view"),
                ErrorMessage: "Remove Posting",
                TitleErrorMessage: "No Posting Data Removed",
                StatusMessage: "Posting Removed"
            });
    },

    apiDownloadPostingReport: function (postingId) {
        var self = this;
        var url = self.apiUrl + "Report/" + postingId;
        window.open(url);
    }
})