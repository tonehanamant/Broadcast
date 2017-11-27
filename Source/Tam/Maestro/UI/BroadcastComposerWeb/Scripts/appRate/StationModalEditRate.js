/*** UPDATE RATES PROGRAM MODAL  RELATED ***/
//NOTE: Genres processing currently not active (commented out)
var StationModalEditRate = function (view) {
    var _view = view;

    return {

        $EditModal: null,
        activeRecord: null,
        lastInput30Val: null,
        initView: function () {
            $("#update_program_save_btn").on('click', this.saveRate.bind(this));
            //$("#update_program_cancel_btn").on('click', this.cancelEdit.bind(this));

            this.setValidationRulesToForm();
        },

        // set edit program modal
        setEditRate: function (activeRecord) {
            var me = this;
            //set active record each time; refer to it later after events called
            this.activeRecord = activeRecord;
            if (!this.$EditModal) {
                this.$EditModal = $('#station_edit_rates_program_modal');
                this.$EditModal.on('shown.bs.modal', function (event) {
                    $("#update_program_form").validate().resetForm();
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

        // set modal fields with active station rate data; set change listener on spot30
        populateForm: function () {
            var me = this;
            var input30 = $('#update_program_spot30_input');
            //remove previous listeners and reset
            //input30.off('blur keydown');
            input30.off('change');

            $('#update_program_name_input').val(me.activeRecord.ProgramName);
            $('#update_program_airtime_input').val(me.activeRecord.AirtimePreview);
            //Flight is now constructed in the program renderer based on dates
            $('#update_program_flight_input').val(me.activeRecord.Flight);
            $('#update_program_spot15_input').val(me.activeRecord.Rate15);
            $('#update_program_spot30_input').val(me.activeRecord.Rate30);
            $('#update_program_hhimpressions_input').val(util.divideImpressions(me.activeRecord.HouseHoldImpressions));
            $('#update_program_hhrating_input').val(me.activeRecord.Rating);
            $('#update_program_effective_date_input').val(null);
           // $('#update_program_audience_id_input').val(me.activeRecord.AudienceId);

            me.loadGenres();
            me.applyMasksToForm();
            me.setFormEffectiveDatePicker($('#update_program_effective_date_input'));

            this.lastInput30Val = null;
            input30.on('change', this.onSpot30change.bind(this, input30));
            //input30.on('blur', this.onSpot30change.bind(this, input30));
            //input30.keydown(function (event) {
            //    if (event.keyCode === 13) { //ENTER
            //        event.preventDefault();
            //        input30.blur();
            //    }
            //});
        },

        loadGenres: function () {
            var me = this;

            _view.controller.apiLoadGenres(function (genres) {
                genres = genres.map(function (originalGenre) {
                    return {
                        id: originalGenre.Id,
                        text: originalGenre.Display
                    }
                });

                $('#update_program_genre_input').select2({
                    tags: true,
                    data: genres
                });
                //not processing per current
                /*
                var selected = me.activeRecord.Genres.map(function (selectedGenre) {
                    return selectedGenre.Id;
                });

                $('#update_program_genre_input').val(selected).trigger('change');
                */
            });
        },

        saveRate: function () {
            var me = this;

            if ($("#update_program_form").valid()) {

                // unmasking form values
                var rate15 = $('#update_program_spot15_input').val() ? parseFloat($('#update_program_spot15_input').val().replace(/[$,]+/g, "")) : null;
                var rate30 = $('#update_program_spot30_input').val() ? parseFloat($('#update_program_spot30_input').val().replace(/[$,]+/g, "")) : null;

                var impressions = parseFloat($('#update_program_hhimpressions_input').val().replace(/,/g, ''));
                    impressions = util.multiplyImpressions(impressions);

                var rating = parseFloat($('#update_program_hhrating_input').val().replace(/,/g, ''));
                //var audienceId = $('#update_program_audience_id_input').val();

                var getEffectiveDate = $('#update_program_effective_date_input').val();
                var effectiveDate = moment(new Date(getEffectiveDate)).isValid() ? moment(new Date(getEffectiveDate)).format('YYYY-MM-DD' + 'T00:00:00') : null;
                
                //not currently processing genres
                //var genres = me.processGenres();

                //adjust to BE specifications
                var updatedProgram = {
                    //UpdatedProgramId: me.activeRecord.Id,
                    //Genres: genres,
                    Id: this.activeRecord.Id,
                    RateSource: _view.controller.getSource(),
                    Rate15: rate15,
                    Rate30: rate30,
                    HouseHoldImpressions: impressions,
                    Rating: rating,
                    EffectiveDate: effectiveDate,
                    Airtime: this.activeRecord.Airtime,
                    EndDate: this.activeRecord.EndDate
                   // AudienceId: audienceId
                };

                _view.controller.apiUpdateRatesProgram(updatedProgram);
            }
        },

        // process the selectec genre from the select2 plugin
        processGenres: function () {
            var genres = $("#update_program_genre_input").select2('data').map(function (item) {
                return {
                    Id: item.id == item.text ? 0 : item.id,
                    Display: item.text
                };
            });

            return genres;
        },

        //cancelEdit: function () {

        //},

        //called from _view after controller saves
        onAfterSaveRate: function () {
            this.$EditModal.modal('hide');
            this.activeRecord = null;

        },

        applyMasksToForm: function () {
            $('#update_program_hhimpressions_input').w2field('float', { precision: 3 });
            $('#update_program_hhrating_input').w2field('float', { precision: 2 });
            $('#update_program_spot15_input').w2field('money');
            $('#update_program_spot30_input').w2field('money');
        },

        setFormEffectiveDatePicker: function (getElement) {
            // updates to monday (needed for manual input)
            $('#update_program_effective_date_input').on('keyup blur', function () {
                if ($('#update_program_form').valid()) {
                    var value = $(this).val()
                    if (value) {
                        var effectiveDate = moment(value, 'YYYY/MM/DD');                        if (effectiveDate.day() != 1) {
                            var monday = effectiveDate.day() == 0 ? effectiveDate.add(-1, "week").startOf('week').weekday(1) : effectiveDate.startOf('week').weekday(1);
                            $("#update_program_effective_date_input").val(monday.format('YYYY/MM/DD'));
                        }
                    }
                }
            });

            // daterangepicker config
            var currentDay = moment();
            var lastDay = this.activeRecord.EndDate;
            var beginningOfWeek = currentDay.startOf('week').weekday(1);

            getElement.daterangepicker({
                "autoUpdateInput": false,
                "singleDatePicker": true,
                "startDate": beginningOfWeek,
                "minDate": beginningOfWeek,
                "maxDate": lastDay,
                "locale": {
                    "format": "YYYY/MM/DD",
                    "firstDay": 1
                }
            });

            // daterangepicker apply btn handler
            getElement.on('apply.daterangepicker', function (ev, picker) {
                var monday = picker.endDate.day() == 0 ? picker.endDate.add(-1, "week").startOf('week').weekday(1) : picker.endDate.startOf('week').weekday(1);
                getElement.data('daterangepicker').setStartDate(monday);

                //picker has a bug that leaves the selection CSS - active end-date - on the slected item so the change does not clear
                //set the end date to the start date so state is maintained
                getElement.data('daterangepicker').setEndDate(monday);
                getElement.val(monday.format('YYYY/MM/DD'));
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
                var effectiveDate = moment(value, 'YYYY/MM/DD').toDate();
                var flightEndDate = (new Date(me.activeRecord.EndDate));
                flightEndDate.setDate(flightEndDate.getDate() + 1);

                if (effectiveDate > flightEndDate) {
                    return false;
                }

                // check if not before Monday
                var beginningOfWeek = moment().startOf('week').weekday(0);
                return (effectiveDate > beginningOfWeek);

            }, "Invalid date");

            $('#update_program_form').validate({
                rules: {
                    update_program_genre_input: {
                        required: false
                    },

                    update_program_spot15_input: {
                        require_from_group: [1, ".update-spot-cost"]
                    },

                    update_program_spot30_input: {
                        require_from_group: [1, ".update-spot-cost"]
                    },

                    update_program_hhimpressions_input: {
                        required: false
                    },

                    update_program_hhrating_input: {
                        required: false
                    },

                    update_program_effective_date_input: {
                        required: true,
                        effective_date: true
                    }
                },

                messages: {
                    update_program_spot15_input: {
                        require_from_group: "One spot required."
                    },

                    update_program_spot30_input: {
                        require_from_group: "One spot required."
                    }
                }
            });
        },

        //on user entered spot 30 change - get then set default values for spot 15
        //issue - W2ui calls change event twice if input initially empty and formatted
        //check not existing and val !existing
        onSpot30change: function (input30) {
            //var val = parseFloat(input30.val().replace(/[$,]+/g, ""));
            var val = input30.val().replace(/[$,]+/g, "");//allow 0.00
            if (val && (!this.lastInput30Val || (this.lastInput30Val != val))) {
                //console.log('spot 30 change', val);
                //call api and set
                _view.controller.apiConvertRate(parseFloat(val), this.setSpot15Change.bind(this), false);
            }
            this.lastInput30Val = val;
        },

        //set spot/rate 15 from 30 change api
        setSpot15Change: function (newVal) {
            //console.log('setSpot15Change', newVal);
            $('#update_program_spot15_input').val(newVal);
            //setting val removes the formatting (W2Ui field does not update) so reset
            $('#update_program_spot15_input').w2field('money');
        }
    };

};