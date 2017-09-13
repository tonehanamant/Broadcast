(function () {
    ko.bindingHandlers.tagsinput = {
        init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            var options = ko.utils.unwrapObservable(allBindingsAccessor().tagsinput);
            options = (typeof options === 'object') ? options : {}

            // adds select2 CSS class
            var select2Class = "select2-selection__choice";
            if (options.tagClass) {
                options.tagClass = options.tagClass.concat(" " + select2Class);
            } else {
                options.tagClass = select2Class;
            }

            $(el).tagsinput(options);
        }
    };
})();