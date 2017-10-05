(function () {
    ko.bindingHandlers.select2 = {
        init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            var allBindings = allBindingsAccessor();

            var select2Config = ko.utils.unwrapObservable(allBindings.select2);
            select2Config = (typeof select2Config === 'object') ? select2Config : {}

            // configures data property for multiple selection (tags)
            if (allBindings.select2.multiple || el.multiple) {
                var options = allBindings.options;
                if (options && options.length > 0) {
                    var select2Options = [];
                    for (var i = 0; i < options.length; i++) {
                        var item = options[i];

                        select2Options.push({
                            id: item[allBindings.optionsValue],
                            text: item[allBindings.optionsText]
                        });
                    }
                }

                select2Config.data = select2Options;
            }

            select2Config.theme = select2Config.theme || "bootstrap";
            $(el).select2(select2Config);

            /*** event handlers ***/
            //workaround for select 2 and jquery validation
            select2Config.validate = select2Config.validate || false;
            if (select2Config.validate) {
                $(el).on("select2:select", function (event) {
                    $(this).valid(); // trigger validation on this element
                });
            }

            if (select2Config.stopPropagation) {
                $(el).on("change select2:open select2:close select2:select select2:unselect", function (event) {
                    event.stopPropagation();
                });

                $("body").on("click", ".select2-search__field", function (event) {
                    event.stopPropagation();
                });
            }

            // prevents sorting when inserting a new element
            $(el).on('select2:select', function (e) {
                var element = $(this).find('[value="' + e.params.data.id + '"]');
                $(this).append(element);
                $(this).trigger('change');
            });

            ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
                $(el).select2('destroy');
            });
        },

        update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            var allBindings = allBindingsAccessor();

            if ("selectedOptions" in allBindings && (allBindings.select2.multiple || el.multiple)) {
                var selectedOptions = allBindings.selectedOptions();
                if (selectedOptions.constructor === Array) {
                    var element;

                    for (var i = 0; i < selectedOptions.length; i++) {
                        var item = selectedOptions[i];
                        if (typeof item === 'object') {
                            element = $(el).find('[value="' + item[allBindings.optionsValue] + '"]');
                        } else {
                            element = $(el).find('[value="' + item + '"]');
                        }

                        $(el).append(element);
                    }
                }
            } else if ("value" in allBindings && allBindings.value && allBindings.value()) {
                if ((allBindings.select2.multiple || el.multiple) && allBindings.value().constructor != Array) {
                    $(el).val(allBindings.value().split(','));
                }
                else {
                    $(el).val(allBindings.value());
                }
            }

            $(el).trigger("change");
        } 
    };
})();