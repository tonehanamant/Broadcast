var PostingUpoadView = BaseView.extend({
    controller: null,
    formValidator: null,
    postingUploadModal: null,

    initView: function (controller) {
        var self = this;
        self.controller = controller;
        self.initValidationRules;
        self.initModal;
    },

    initModal: function () {
        var self = this;
        self.postingUploadModal = $("posting-upload-modal");
        self.postingUploadModal.modal({ backdrop: 'static', show: false, keyboard: false });
    }
})