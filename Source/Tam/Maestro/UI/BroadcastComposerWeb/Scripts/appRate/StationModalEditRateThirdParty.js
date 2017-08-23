/*** UPDATE RATES PROGRAM THIRD PARTY (Specific) MODAL  RELATED ***/
//Genre & Effective Date (required) only editable - all other read only
//read only demo CPM grid
var StationModalEditRateThirdParty = function (view) {
    var _view = view;

    return {

        $EditModal: null,
        $DemoGrid: null,
        activeRecord: null,

        initView: function () {
            $("#edit_program_thirdparty_save_btn").on('click', this.saveRate.bind(this));
            //TODO
            this.$DemoGrid = $('#rate_demo_thirdparty_grid').w2grid(RateConfig.getDemoThirdPartyGridCfg(this));
            this.setValidationRulesToForm();
        },

        // set edit program modal
        setEditRate: function (activeRecord) {
            var me = this;
            //set active record each time; refer to it later after events called
            this.activeRecord = activeRecord;
            if (!this.$EditModal) {
                this.$EditModal = $('#station_edit_rate_thirdparty_modal');
                this.$EditModal.on('shown.bs.modal', function (event) {
                    $("#edit_program_form_thirdparty").validate().resetForm();
                    me.populateForm();
                    me.setDemoGrid(me.activeRecord.Audiences);
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
            var me = this;

            $('#edit_program_name_thirdparty_input').val(me.activeRecord.Program);
            $('#edit_program_airtime_thirdparty_input').val(me.activeRecord.Airtime);
            $('#edit_program_flight_thirdparty_input').val(me.activeRecord.Flight);
            $('#edit_program_effective_thirdparty_input').val(null);
            me.loadGenres();
            me.setFormEffectiveDatePicker($('#edit_program_effective_thirdparty_input'));
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

                $('#edit_program_genre_thirdparty_input').select2({
                    tags: true,
                    data: genres
                });

                var selected = me.activeRecord.Genres.map(function (selectedGenre) {
                    return selectedGenre.Id;
                });

                $('#edit_program_genre_thirdparty_input').val(selected).trigger('change');
            });
        },

        //Save Third Party rate
        saveRate: function () {

            if ($("#edit_program_form_thirdparty").valid()) {


                var getEffectiveDate = $('#edit_program_effective_thirdparty_input').val();
                var effectiveDate = moment(new Date(getEffectiveDate)).isValid() ? moment(new Date(getEffectiveDate)).format('YYYY-MM-DD' + 'T00:00:00') : null;

                var genres = this.processGenres();

                var updatedProgram = {
                    AudienceId: this.activeRecord.AuidenceId,
                    Genres: genres,
                    FlightStartDate: effectiveDate,
                    FlightEndDate: this.activeRecord.FlightEndDate
                };

                //TBD - new API?
                _view.controller.apiUpdateRatesProgram(this.activeRecord.Id, updatedProgram);
            }
        },

        // process the selected genre from the select2 plugin
        processGenres: function () {
            var genres = $("#edit_program_genre_thirdparty_input").select2('data').map(function (item) {
                return {
                    Id: item.id == item.text ? 0 : item.id,
                    Display: item.text
                };
            });

            return genres;
        },

        //called from _view after controller saves
        onAfterSaveRate: function () {
            this.$EditModal.modal('hide');
            this.activeRecord = null;

        },


        //Effective date picker single - set to Monday/Sunday etc
        setFormEffectiveDatePicker: function (getElement) {
            // updates to monday (needed for manual input)
            $('#edit_program_effective_thirdparty_input').on('keyup blur', function () {
                if ($('#edit_program_form_thirdparty').valid()) {
                    var value = $(this).val()
                    if (value) {
                        var effectiveDate = moment(value, 'YYYY/MM/DD');                        if (effectiveDate.day() != 1) {
                            var monday = effectiveDate.day() == 0 ? effectiveDate.add(-1, "week").startOf('week').weekday(1) : effectiveDate.startOf('week').weekday(1);
                            $("#edit_program_effective_thirdparty_input").val(monday.format('YYYY/MM/DD'));
                        }
                    }
                }
            });

            // daterangepicker config
            var currentDay = moment();
            var lastDay = this.activeRecord.FlightEndDate;
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

        //validation effective date
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
                var flightEndDate = (new Date(me.activeRecord.FlightEndDate));
                flightEndDate.setDate(flightEndDate.getDate() + 1);

                if (effectiveDate > flightEndDate) {
                    return false;
                }

                // check if not before Monday
                var beginningOfWeek = moment().startOf('week').weekday(0);
                return (effectiveDate > beginningOfWeek);

            }, "Invalid date");

            $('#edit_program_form_thirdparty').validate({
                rules: {

                    edit_program_effective_thirdparty_input: {
                        required: true,
                        effective_date: true
                    }
                }
            });
        },

        //tbd - ID, etc
        prepareDemoGridData: function (data) {
            var ret = [];
            $.each(data, function (index, value) {
                var item = value;
                item.recid = index + 1;
                item.Target = value.Audience.AudienceString;
                ret.push(item);
            });

            return ret;
        },

        setDemoGrid: function (demoData) {
            var gridData = this.prepareDemoGridData(demoData);
            this.$DemoGrid.clear(false);
            this.$DemoGrid.add(gridData);
            this.$DemoGrid.resize();
        }
    };

};