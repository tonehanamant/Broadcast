
//begins wrappers for plug-in factory (i.e use in VMs where binding is problematic)
var wrappers = {
    //wraps single date pickers  - i.e. for form binding use in VM
    //provide mechanism to set start for an inital value (edit)
    datePickerSingleWrapper: function (input, callBack) {

        return {
            input: input,
            init: function () {
                var me = this;
                input.daterangepicker(
                    {
                        autoUpdateInput: false,
                        singleDatePicker: true,
                        //showDropdowns: true,
                        locale: {
                            format: "MM/DD/YYYY",
                            firstDay: 1
                        },
                        opens: "right",
                    }
                );
                input.on('apply.daterangepicker', function (ev, picker) {
                    var start = picker.startDate;
                    me.updateDisplay(start);
                    callBack(start);
                });
            },
            updateDisplay: function (start) {
                var formattedStartDate = start ? start.format('MM/DD/YYYY') : '';
                this.input.val(formattedStartDate);
                if (!start) {

                    this.input.data('daterangepicker').setStartDate(moment());
                    //reset end Date or does not clear display
                    this.input.data('daterangepicker').setEndDate(moment());
                    //console.log(this.input.data('daterangepicker'));
                }
            },
            //start - moment start date
            setStart: function (start) {
                if (start) {
                    this.input.data('daterangepicker').setStartDate(start);
                    //reset end Date or does not clear display
                    this.input.data('daterangepicker').setEndDate(start);
                }
                this.updateDisplay(start);

            }
        };

    },

    //wraps daypart plugin (i.e.form binding in VM)
    daypartWrapper: function (input, popContainer, callBack) {

        return {
            input: input,
            popContainer: popContainer,
            init: function (dateValues, convert) {
                if (dateValues && convert) {
                    //set the value to input if dateValues; convert values for daypart plugin
                    this.input.val(dateValues.Text);
                    dateValues = this.convertFor(dateValues);
                }
                this.input.daypartDropdown({
                    record: null,
                    dateObject: dateValues || { StartTime: 0, EndTime: 0, Sun: true, Mon: true, Tue: true, Wed: true, Thu: true, Fri: true, Sat: true },//needs defaults
                    onClosePopup: this.onClose.bind(this),
                    popoverContainer: this.popContainer
                    //important - set to avoid input focus issues - needs to be in the modal container not the form
                });
            },

            onClose: function (dateValues) {
                //need to store values and then either reinitilize new values (after destroy)
                var me = this;
                var dateObjectDto = this.convertTo(dateValues);
                callBack(dateObjectDto);
                //timeout is needed or the picker does not get detroyed in time for the reset
                setTimeout(function () {
                    me.init(dateValues);
                }, 500);
            },

            remove: function () {
                this.input.webuiPopover('destroy');
                this.input.removeData('plugin_daypartDropdown');
                this.input.val('');
            },
            //convert for use in daypart
            convertFor: function (values) {
                var dateObject = {
                    StartTime: values.startTime,
                    EndTime: values.endTime,
                    Sun: values.sun,
                    Mon: values.mon,
                    Tue: values.tue,
                    Wed: values.wed,
                    Thu: values.thu,
                    Fri: values.fri,
                    Sat: values.sat
                };

                return dateObject;
            },
            //convert for VM
            convertTo: function (dateValues) {
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
                return dateObjectDto;
            }
        };

    }

};