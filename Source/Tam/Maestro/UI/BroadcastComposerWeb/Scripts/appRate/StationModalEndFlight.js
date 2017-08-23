/*** END PORGRAM FLIGHT RATES  MODAL  RELATED ***/
//set flight end in DRP - set date to end of week (Sunday)
var StationModalEndFlight = function (view) {
    //private
    var _view = view;

    return {

        $EditModal: null,
        activeRecord: null,

        initView: function () {
            $("#end_flight_save_btn").on('click', this.saveEndFlight.bind(this));
            this.setValidationRulesToForm();
        },

        // set end flight program modal
        setEndFlight: function (activeRecord) {
            var me = this;
            //set active record each time; refer to it later after events called
            this.activeRecord = activeRecord;
            if (!this.$EditModal) {
                this.$EditModal = $('#station_end_flight_modal');
                this.$EditModal.on('shown.bs.modal', function (event) {
                    $("#end_flight_form").validate().resetForm();
                    me.populateForm();
                });

                this.$EditModal.modal({
                    backdrop: 'static',
                    show: false,
                    keyboard: false
                });
            }

            this.$EditModal.modal('show');
        },

        // set modal fields with active station rate data
        populateForm: function () {
            //set program name ? somewhere? title
            $('#end_flight_program_name_input').val(this.activeRecord.Program);
            this.setFormFlightDatePicker($('#end_flight_date_input'));
        },


        saveEndFlight: function () {
           // console.log('valid', $("#end_flight_form").valid());
            if ($("#end_flight_form").valid()) {

                var getEffectiveDate = $('#end_flight_date_input').val();
                var endDate = moment(new Date(getEffectiveDate)).isValid() ? moment(new Date(getEffectiveDate)).format('YYYY-MM-DD') : null;
                //pass callback
                var callback = this.onAfterSaveEndFlight.bind(this);
                if (endDate) _view.controller.apiEndFlightRate(this.activeRecord.Id, endDate, callback);
            }
        },

        //called from callback after controller saves
        onAfterSaveEndFlight: function () {
            this.$EditModal.modal('hide');
            this.activeRecord = null;
        },

        //Sunday basis (end of week)
        //endDate = endDate.day() == 0 ? endDate : endDate.add(1, "week").startOf('week').weekday(0);
        setFormFlightDatePicker: function (getElement) {
            $("#end_flight_date_input").val('');
            // updates to sunday (needed for manual input); set to sunday of the week
            $('#end_flight_date_input').on('keyup blur', function () {
                if ($('#end_flight_form').valid()) {
                    var value = $(this).val()
                    if (value) {
                        var effectiveDate = moment(value, 'YYYY/MM/DD');                        //if (effectiveDate.day() != 1) {
                            var sunday = effectiveDate.day() == 0 ? effectiveDate : effectiveDate.add(1, "week").startOf('week').weekday(0);
                            $("#end_flight_date_input").val(sunday.format('YYYY/MM/DD'));
                        //}
                    }
                }
            });

            // daterangepicker config
            var currentDay = moment();
            // clone moment objects to avoid changing original values
            var endCurrent = currentDay.clone();
            var lastDay = this.activeRecord.FlightEndDate;
            //var endOfWeek = currentDay.startOf('week').weekday(0);
            var endOfWeek = endCurrent.day() == 0 ? endCurrent : endCurrent.add(1, "week").startOf('week').weekday(0);
            var startOfWeek = currentDay.startOf('week').weekday(1);
            //console.log('start, endOfWeek', startOfWeek, endOfWeek);
            //set to Sunday but allow setting any part of a current week (if within now to end)
            getElement.daterangepicker({
                "autoUpdateInput": false,
                "singleDatePicker": true,
                "startDate": endOfWeek,
                "minDate": startOfWeek,
                "maxDate": lastDay,
                "locale": {
                    "format": "YYYY/MM/DD",
                    "firstDay": 1
                }
            });

            // daterangepicker apply btn handler
            getElement.on('apply.daterangepicker', function (ev, picker) {
                var sunday = picker.startDate.day() == 0 ? picker.startDate : picker.startDate.add(1, "week").startOf('week').weekday(0);
                getElement.data('daterangepicker').setStartDate(sunday);

                //picker has a bug that leaves the selection CSS - active end-date - on the slected item so the change does not clear
                //set the end date to the start date so state is maintained
                getElement.data('daterangepicker').setEndDate(sunday);
                getElement.val(sunday.format('YYYY/MM/DD'));
            });
        },

        setValidationRulesToForm: function () {
            var me = this;

            // validation for effective date -- needed for manual input
            $.validator.addMethod("effective_date", function (value, element) {
                // valid format
                if (!moment(value, 'YYYY/MM/DD', true).isValid()) {
                    return false;
                }

                // check if before FlightEndDate
                var effectiveDate = moment(value, 'YYYY/MM/DD');
                var flightEndDate = (new Date(me.activeRecord.FlightEndDate));
                flightEndDate.setDate(flightEndDate.getDate() + 1);

                if (effectiveDate > flightEndDate) {
                    return false;
                }

               //check not in past but allow from Monday of the current week
                
                var monnow = moment().startOf('week').weekday(1);;
                //isSameOrAfter is not available in our version
                //console.log((effectiveDate.isSame(monnow, 'day') || effectiveDate.isAfter(monnow, 'day')));
                //could also to check before now to allowable monday?
                return (effectiveDate.isSame(monnow, 'day') || effectiveDate.isAfter(monnow, 'day'));

            }, "Invalid date");

            $('#end_flight_form').validate({
                rules: {

                    end_flight_date_input: {
                        required: true,
                        effective_date: true
                    }
                }
            });
        }
    };

};