(function () {
    function InitializeDayPartDatePicker(daypartInput, observable, containerId, dateValues) {
        if (!dateValues) {
            daypartInput.webuiPopover('destroy');
            daypartInput.removeData('plugin_daypartDropdown');
        }

        daypartInput.daypartDropdown({
            record: null,
            dateObject: dateValues || { StartTime: 0, EndTime: 0, Sun: true, Mon: true, Tue: true, Wed: true, Thu: true, Fri: true, Sat: true },
            popoverContainer: '#' + containerId,
            onClosePopup: function (dateValues) {
                // convert fromto plugin object to BE DTO
                var dateObjectDto = {
                    startTime: dateValues.StartTime,
                    endTime: dateValues.EndTime,
                    sun: dateValues.Sun,
                    mon: dateValues.Mon,
                    tue: dateValues.Tue,
                    wed: dateValues.Wed,
                    thu: dateValues.Thu,
                    fri: dateValues.Fri,
                    sat: dateValues.Sat,
                    Text: dateValues.toString()
                }

                if (observable().constructor == Array) {
                    observable([dateObjectDto]);
                } else {
                    observable(dateObjectDto);
                }

                daypartInput.val(dateObjectDto.Text);

                setTimeout(function () {
                    InitializeDayPartDatePicker(daypartInput, observable, containerId, dateValues);
                }, 500);
            }
        });
    };

    ko.bindingHandlers.dayPartDatePicker = {
        init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            var allBindings = allBindingsAccessor();
            var dayPartDatePickerConfig = ko.utils.unwrapObservable(allBindings.dayPartDatePicker);

            // convert from BE DTO to plugin object
            var value = allBindings.value();
            var dateObjectDto = value.constructor != Array ? value : value[0];
            if (dateObjectDto) {
                var dateObject = {
                    StartTime: dateObjectDto.startTime,
                    EndTime: dateObjectDto.endTime,
                    Sun: dateObjectDto.sun,
                    Mon: dateObjectDto.mon,
                    Tue: dateObjectDto.tue,
                    Wed: dateObjectDto.wed,
                    Thu: dateObjectDto.thu,
                    Fri: dateObjectDto.fri,
                    Sat: dateObjectDto.sat
                };

                $(el).val(dateObjectDto.Text);
            }

            InitializeDayPartDatePicker($(el), allBindings.value, dayPartDatePickerConfig.containerId, dateObject);
        }
    };
})();