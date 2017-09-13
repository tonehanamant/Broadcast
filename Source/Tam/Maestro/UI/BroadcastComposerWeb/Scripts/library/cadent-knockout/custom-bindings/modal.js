(function () {
    ko.bindingHandlers.modal = {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            var showObservable = valueAccessor().show;

            var initCallback = valueAccessor().open;
            $(element).on('shown.bs.modal', function (e) {
                showObservable(true);

                if (typeof initCallback === 'function') {
                    initCallback();
                }


            });

            var hideCallback = valueAccessor().hide;
            $(element).on('hidden.bs.modal', function (e) {
                showObservable(false);

                if (typeof hideCallback === 'function') {
                    hideCallback();
                }
            });
        },

        update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            var showObservable = valueAccessor().show;
            if (showObservable()) {
                $(element).modal('show');
            } else {
                $(element).modal('hide');
            }
        }
    }
})();