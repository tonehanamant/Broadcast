var PostingMainView = BaseView.extend({

    postingGrid: null,
    activePostingRequest: null,
    postingUploadModal: null,
    postingDeleteModal: null,
    uploadManager: null,

    initView: function (controller) {
        this.controller = controller;
        this.postingGrid = $("#posting_grid").w2grid(PostingConfig.getPostingGridConfig(this));

        this.loadGrid();

        this.setUpGridEventHandlers();
        this.initUpload();
    },

    loadGrid: function () {
        var self = this;

        self.postingGrid.clear(false);

        this.controller.apiGetInitialPostings(function (postings) {
            if (postings) {
                var gridPostings = postings.map(function(post) {
                    post.recid = post.Id;

                    post.DisplayDemo = post.DemoLookups.map(function (demo) {
                        return " " + demo.Display;
                    });

                    return post;
                });

                self.postingGrid.add(gridPostings);
            }
        });
    },

    //Manage uploading
    initUpload: function () {
        this.uploadManager = new PostingUploadManager(this);
    },

    //send files from upload manager
    processUploadPostingFile: function (postingFile) {
        var self = this;
        this.setPostingUploadModal(postingFile, false);
    },

    onAfterPostingEvent: function () {
        if (this.postingUploadModal) {
            this.postingUploadModal.modal("hide");
        }

        if (this.postingDeleteModal) {
            this.postingDeleteModal.modal("hide");
        }

        this.loadGrid();
    },

    //initialize modal
    setPostingUploadModal: function (postFile, isEdit) {
        var self = this;
        var mode = isEdit ? "edit" : "create";
        self.activePostingRequest = postFile ? postFile : null;
        self.controller.viewModel.setFormMode(mode);
        self.initPostingValidationRules();

        self.postingUploadModal = $("#posting_upload_modal");
        self.postingUploadModal.on("show.bs.modal", self.onPostingUploadModalShown.bind(this));
        self.postingUploadModal.on("hidden.bs.modal", self.onPostingUploadModalHidden.bind(this));

        self.postingUploadModal.modal("show");
    },

    onPostingUploadModalShown: function () {
        var self = this;

        self.uploadManager.dragEnabled = false;
        if (self.activePostingRequest) {
            self.controller.viewModel.setPosting(self.activePostingRequest);
        }
    },

    onPostingUploadModalHidden: function () {
        var self = this;

        self.uploadManager.dragEnabled = true;
        self.controller.viewModel.resetPosting();
        self.postingGrid.refresh();
    },

    initPostingValidationRules: function () {
        //jquery validator automatically adds 'validator' which needs to be reset when there are dynamic values
        $('#posting_upload_form').removeData('validator');

        rules = {
            post_input_posting: {
                required: true
            },
            post_input_playback: {
                required: true
            },
            post_input_demos: {
                required: true
            }
        };

        var postingUploadValidator = $("#posting_upload_form").validate({ rules: rules });
    },

    // w2ui event handlers
    setUpGridEventHandlers: function () {
        var self = this;

        // search button
        $("#posting_view").on("click", '#posting_grid_search_btn', self.postingGridTextSearch.bind(this));

        // keystroke search
        $("#posting_view").on("keypress", '#posting_grid_search_input', function (e) {
            var key = e.which;
            if (key == 13) {
                self.postingGridTextSearch();
            }
        });

        // Click on clear search
        $("#posting_view").on("click", '#posting_grid_search_clear_btn', this.clearPostingGridSearch.bind(this, false));

        //context menu clicks
        self.postingGrid.on("menuClick", this.onPostingMenuClick.bind(this));
    },

    // reset search field
    clearPostingGridSearch: function () {
        var self = this;
        self.postingGrid.searchReset();

        $("#posting_grid_search_input").val('');
        $("#posting_grid_search_clear_btn").hide();
    },

    // Search the w2ui grid
    postingGridTextSearch: function (event) {
        var self = this;
        var val = $("#posting_grid_search_input").val();
        if (val && val.length) {
            val = val.toLowerCase();
            var search = [{ field: 'FileName', type: 'text', value: [val], operator: 'contains' }, { field: 'DisplayDemo', type: 'text', value: [val], operator: 'contains' }];
            self.postingGrid.search(search, 'OR');
            $("#posting_grid_search_clear_btn").show();
        } else {
            this.clearPostingGridSearch();
        }
    },

    //Delete Post file from grid
    postingFileDelete: function (fileName, record) {
        var title = "Delete Post File",
            message = "Are you sure you want to delete " + fileName + "?";
        var callback = this.onPostingFileDelete.bind(this, record);
        util.confirm(title, message, this.controller.apiDeletePostingFile.bind(self.controller, record, callback));
    },

    onPostingFileDelete: function (id) {
        this.postingGrid.remove(id);
        util.notify("Post File Deleted");
    },

    onPostingMenuClick: function (event) {
        var id = event.menuItem.id;
        var postingId = event.recid;
        var self = this;

        switch (id) {
            case 1:
                self.controller.apiGetPostingById(postingId);
                self.setPostingUploadModal(null, true);
                break;
            case 2:
                this.controller.apiDownloadPostingReport(postingId);
                break;
            case 3:
                var postData = this.postingGrid.get(postingId);
                var fileName = postData.FileName;
                self.postingFileDelete(fileName, postingId);
                break;
        }
    },
})