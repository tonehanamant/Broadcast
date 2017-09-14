
var PostingUploadViewModel = function (controller) {
    var self = this;
    var controller = controller;

    self.postingValidator = null;
    self.activePosting = null;
    self.activeFormType = ko.observable("upload");
    self.activeFormMode = ko.observable("edit");

    //declare components
    self.Equivalized = ko.observable(true);//set default
    self.PostingBooks = ko.observableArray();
    self.PlaybackTypes = ko.observableArray();
    self.PostingDemos = ko.observableArray();

    //stored values for upload
    self.PostingId = ko.observable();
    self.FileName = ko.observable();
    self.PostingBookId = ko.observable();
    self.selectedPostingDemos = ko.observableArray();
    self.selectedPlaybackType = ko.observable();

    //set posting modal with data, validator
    self.setPosting = function (postingRequest) {
        self.postingValidator = $("#posting_upload_form").validate();
        self.activePosting = postingRequest;
        self.FileName(postingRequest.FileName);
    }

    self.setFormMode = function (mode) {
        self.activeFormMode(mode);
    },

    //populate form fields with options
    self.initComponents = function (data) {
        self.PostingBooks(data.PostingBooks);
        self.PlaybackTypes(data.PlaybackTypes);
        self.PostingDemos(data.Demos);
    },

    //include form properties to selected posting file
    self.setPostingFormData = function () {
        var posting = self.activePosting;

        posting.Equivalized = self.Equivalized();
        posting.PostingBookId = self.PostingBookId();
        posting.PlaybackType = self.selectedPlaybackType();
        posting.Audiences = self.selectedPostingDemos();
        this.FileName(posting.FileName);

        return posting;
    },

    //load edit form with File Settings data from id
    self.preparePostingEditForm = function (posting) {
        self.postingValidator = $("#posting_upload_form").validate();
        self.PostingId(posting.Id);
        self.Equivalized(posting.Equivalized);
        self.selectedPostingDemos(posting.Demos);
        self.FileName(posting.FileName);
        self.selectedPlaybackType(posting.PlaybackType);
        self.PostingBookId(posting.PostingBookId);
    },

    //load data for deletion modal
    self.preparePostingRemoval = function (posting) {
        this.PostingId(posting.Id);
        this.FileName(posting.FileName);
    },

    //build object to send to server
    self.putPostingUpdate = function () {
        var postingUpdate = {
            FileId: self.PostingId(),
            Equivalized: self.Equivalized(),
            PostingBookId: self.PostingBookId(),
            PlaybackType: self.selectedPlaybackType(),
            Audiences: self.selectedPostingDemos()
        }

        return postingUpdate;
    },

    //called from upload form button
    self.uploadPosting = function () {
        if (self.postingValidator.form()) {
            var posting = self.setPostingFormData();
            controller.apiUploadPostingFile(posting);
        }
    },

    //called from Save form button
    self.savePosting = function () {
        if (this.postingValidator.form()) {
            var posting = self.putPostingUpdate();
            controller.apiSavePostingFile(posting, function(response) {
                controller.view.onAfterPostingEvent();
                util.notify("Posting File Saved");
            });
        }
    },

    self.deletePosting = function () {
        var postingId = self.PostingId();
        controller.apiDeletePostingFile(postingId);
    },

    //select2 Validation has to be forced
    self.selectedPostingDemos.subscribe(function (values) {
        if (values && values.length) {
            self.postingValidator.element("#post_input_demos");
        }
    });

    //reset the form
    self.resetPosting = function () {
        self.FileName("");
        self.PostingBookId(null);
        self.selectedPlaybackType("");
        self.selectedPostingDemos([]);
        self.Equivalized(true);
        activePosting = null;

        if (self.postingValidator) {
            self.postingValidator.resetForm();
        }

        $('.form-group').removeClass('has-error');
    }
}