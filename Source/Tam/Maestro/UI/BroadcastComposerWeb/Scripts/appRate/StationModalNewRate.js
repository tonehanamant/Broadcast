/*** NEW RATES PROGRAM MODAL  RELATED ***/
//New Program form handling with custom airtime plug-in, flights drp
//Conflicts handling: get conflicts on flights/airtime; edit conflicts - custom locked DRP (id start is in past); check conflict api
//Store/Save data per BE requirements; maintain changed conflicts

var StationModalNewRate = function (view) {
    //private
    var _view = view;

    return {

        $NewModal: null,
        $ProgramConflictGrid: null,
        newProgram: null,
        activeStationCode: null,
        activeConflictsRecord: null,
        $ConflictFlightRangeElem: null,
        hasPendingConflictsChanges: false,
        lastInput30Val: null,

        initView: function () {
            this.$ProgramConflictGrid = $('#program_conflict_grid').w2grid(RateConfig.getProgramConflictGridCfg(this));
            $("#new_program_add_btn").on('click', this.saveRate.bind(this, false));
            $("#new_program_add_another_btn").on('click', this.saveRate.bind(this, true));
            this.initProgramConflictsGridEvents();
            this.setValidationRulesToForm();
        },

        //set new program modal - form
        setNewRate: function (stationCode) {
            this.activeStationCode = stationCode;
            var me = this;
            this.resetNewProgram();
            
            if (!this.$NewModal) {
                this.$NewModal = $('#station_new_rates_program_modal');
                //scope within event is modal
                //handle any post render items
                this.$NewModal.on('shown.bs.modal', function (event) {
                    $("#new_program_form").validate().resetForm();
                    me.populateForm();
                    me.setProgramConflictsGrid(); //set to empty
                });

                this.$NewModal.modal({
                    backdrop: 'static',
                    show: false,
                    keyboard: false
                });
            }

            this.$NewModal.modal('show');
        },

        //reset new Program object
        resetNewProgram: function () {
            this.activeConflictsRecord = null;
            this.hasPendingConflictsChanges = false;
            this.newProgram = {
                //StationCode: null, //StationCode ?
                Program: null,
                Rate15: null,
                Rate30: null,
                Impressions: null,
                Rating: null,
                FlightStartDate: null,
                FlightEndDate: null,
                Genres: [], //currently one item but will change
                Flights: [], //StartDate, EndDate, IsHiatus
                Airtime: null, //airtime object
                Conflicts: [] //flights in conflicts should only show changes
            };
        },

        // set modal fields with cleared data; airtime plugin and flights DRP; set change listener on spot30
        populateForm: function () {
            var me = this;
            var input30 = $('#new_program_spot30_input');
            //remove previous listeners and reset
            input30.off('change');

            $('#new_program_name_input').val('');
            $('#new_program_airtime_input').val('');
            $('#new_program_flight_input').val(null);
            $('#new_program_spot15_input').val('');
            $('#new_program_spot30_input').val('');
            $('#new_program_hhimpressions_input').val('');
            $('#new_program_hhrating_input').val('');

            me.loadGenres();
            me.applyMasksToForm();
            me.setFormFlightsPicker($('#new_program_flight_input'));
            me.setFormAirtimeDropdown();
            this.lastInput30Val = null;
            input30.on('change', this.onSpot30change.bind(this, input30));
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

                $('#new_program_genre_input').select2({
                    tags: true,
                    data: genres
                });

                $('#new_program_genre_input').val([]).trigger('change');
            });
        },

        // save rate from newProgram including new converted/ 'unmasked' data to server; Conflicts Data handling; addAnother
        saveRate: function (addAnother) {
            var me = this;
            if ($("#new_program_form").valid()) {
                me.newProgram.Conflicts = this.getConflictsForSave(this.$ProgramConflictGrid.records);
                me.newProgram.Program = $('#new_program_name_input').val();
                me.newProgram.Genres = me.processGenres();
                me.newProgram.Rate15 = $('#new_program_spot15_input').val() ? parseFloat($('#new_program_spot15_input').val().replace(/[$,]+/g, "")) : null;
                me.newProgram.Rate30 = $('#new_program_spot30_input').val() ? parseFloat($('#new_program_spot30_input').val().replace(/[$,]+/g, "")) : null;

                me.newProgram.Impressions = parseFloat($('#new_program_hhimpressions_input').val().replace(/[$,]+/g, ""));
                me.newProgram.Rating = parseFloat($('#new_program_hhrating_input').val().replace(/[$,]+/g, ""));

                var callback = this.onAfterSaveNewRate.bind(this, addAnother);
                _view.controller.apiSaveNewRatesProgram(this.newProgram, callback);
            }
        },

        // process the selectec genre from the select2 plugin
        processGenres: function () {
            var genres = $('#new_program_genre_input').select2('data').map(function (item) {
                return {
                    Id: item.id == item.text ? 0 : item.id,
                    Display: item.text
                };
            });

            return genres;
        },

        //callback after save: addAnother resets
        onAfterSaveNewRate: function (addAnother) {
            this.resetNewProgram();
            if (addAnother) {
                this.setProgramConflictsGrid(); //clear
                this.populateForm();//reset
            } else {
                this.$NewModal.modal('hide');
            }       
        },

        //get Conflict items from grid records; adapt to BE requirements; get only conflicts that are edited
        //send all flights - changed or not
        getConflictsForSave: function (conflicts) {
            var ret = [];
            $.each(conflicts, function (index, item) {
                if (item.isEdited) {
                    var obj = {
                        Id: item.Id,
                        FlightStartDate: item.FlightStartDate,
                        FlightEndDate: item.FlightEndDate,
                        Flights: item.Flights
                    };
                    ret.push(obj);
                }
                    
             });
            return ret;

        },

        //apply masks to form fields
        applyMasksToForm: function () {
            $('#new_program_hhimpressions_input').w2field('float', { precision: 3 });
            $('#new_program_hhrating_input').w2field('float', { precision: 2 });
            $('#new_program_spot15_input').w2field('money');
            $('#new_program_spot30_input').w2field('money');
        },

        // Flights daterangepicker
        setFormFlightsPicker: function (getElement) {
            var start = moment();
            var me = this;

            getElement.daterangepicker({
                autoUpdateInput: false,
                minDate: start,
                locale: {
                    format: "MM/DD/YYYY",
                    firstDay: 1
                },
                opens: "left",
                template:
                '<div class="daterangepicker dropdown-menu">' +
                    '<div class="calendar left">' +
                        '<div class="daterangepicker_input">' +
                            '<input class="input-mini form-control" type="text" name="daterangepicker_start" value="" />' +
                            '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                            '<div class="calendar-time">' +
                                '<div></div>' +
                                '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                            '</div>' +
                        '</div>' +

                    '<div class="calendar-table"></div>' +
                '</div>' +

                '<div class="calendar right">' +
                    '<div class="daterangepicker_input">' +
                        '<input class="input-mini form-control" type="text" name="daterangepicker_end" value="" />' +
                        '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                        '<div class="calendar-time">' +
                            '<div></div>' +
                            '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                        '</div>' +
                    '</div>' +

                    '<div class="calendar-table"></div>' +
                '</div>' +

                '<div class="ranges" style="width: 220px;" >' +
                    '<h5>Flight Weeks</h5>' +
                    '<ul id="flight_week_list" style="width: 220px; padding-left:20px; max-height: 180px; overflow-y: auto; margin-bottom: 20px;"></ul>' + // weeks checkbox list goes in here

                    '<div class="range_inputs">' +
                        '<button class="applyBtn" disabled="disabled" type="button"></button> ' +
                        '<button class="cancelBtn" data-dismiss="modal" type="button"></button>' +
                    '</div>' +
                '</div>'
            },

            function (start, end) {

            });

            // save picker data for BE post - change date format appropriately
            getElement.on('apply.daterangepicker', function (ev, picker) {
                var adjustStart = picker.startDate.startOf('week').weekday(1);
                me.newProgram.FlightStartDate = adjustStart.format('MM-DD-YYYY');
                //need to set to Sunday
                var adjustEnd = picker.endDate.day() == 0 ? picker.endDate : picker.endDate.add(1, "week").startOf('week').weekday(0);
                me.newProgram.FlightEndDate = adjustEnd.format('MM-DD-YYYY');

                getElement.val(adjustStart.format('MM/DD/YYYY') + ' - ' + adjustEnd.format('MM/DD/YYYY'));
                //fix validation change not displaying
                getElement.valid();
                //REVISE - get all the flights - parse from value - with IsHiatus - not checked
                var flights = [];
                $('#flight_week_list input').each(function () {
                    var flight = { StartDate: $(this).data('start'), EndDate: $(this).data('end') };
                    flight.IsHiatus = !$(this).prop('checked');
                    flights.push(flight);
                });
                me.newProgram.Flights = flights;
               
                //check conflicts
                me.checkNewProgramConflicts();
            });

            // reevaluates the list of weeks for hiatus selection
            getElement.on('show.daterangepicker', function (ev, picker) {
                picker.container.find('.calendar')
                    .on('mousedown.daterangepicker', 'td.available', $.proxy(function (e) {
                        var flightWeeksEl = $('#flight_week_list');
                        flightWeeksEl.html('');

                        if (picker.startDate && picker.endDate) {

                            // clone moment objects to avoid changing original values
                            var startDate = picker.startDate.clone();
                            var endDate = picker.endDate.clone();
                            var durationDays = moment.duration(endDate.diff(startDate)).asDays();

                            // checks if the range is within the same week
                            var mondayToSunday = durationDays > 7 && durationDays < 8 && startDate.day() == 1 && endDate.day() == 0;
                            var sameDate = durationDays < 2;
                            var sameWeek = sameDate || mondayToSunday || (durationDays < 7 && startDate.day() != 0 && (startDate.day() < endDate.day() || endDate.day() == 0));

                            // initial and final dates for the whole interval (first day of first week and last day of last week)
                            startDate = startDate.day() != 0 ? startDate.startOf('week').weekday(1) : startDate.add(-1, "week").startOf('week').weekday(1);
                            endDate = endDate.day() == 0 ? endDate : endDate.add(1, "week").startOf('week').weekday(0);

                            var amountWeeks = 1;
                            if (!sameWeek) {
                                var durationWeeks = moment.duration(endDate.diff(startDate)).asWeeks();
                                amountWeeks = Math.ceil(durationWeeks);
                            }

                            // generates the checkboxes for each week in the selected range
                            for (var week = 0; week < amountWeeks; week++) {
                                var startWeek = startDate.clone().add(week, "week");
                                var endWeek = startWeek.clone().add(1, "week").startOf('week').weekday(0)
                                var weekInterval = startWeek.format('YYYY/MM/DD') + " - " + endWeek.format('YYYY/MM/DD');
                                //change use data for start and end  for Flights
                                flightWeeksEl.append('<li style="width: 200px; position: relative; right: 25px "><input data-start="' + startWeek.format('YYYY-MM-DD') + '" data-end="' + endWeek.format('YYYY-MM-DD') + '"type="checkbox" id="checkbox_' + week + '" checked> ' + weekInterval + '</li>');
                            }
                        }
                    }, this));

                //Disable handlers to turn off daterangepicker hoverRange errors
                picker.container.find('.ranges')
                    .off('mouseenter.daterangepicker', 'li')
                    .off('mouseleave.daterangepicker', 'li')
                    .off('click.daterangepicker', 'li')
            });
        },

        //set custom plugin - daypart dropdown
        setFormAirtimeDropdown: function (dateValues) {
            //console.log('setNewFormAirtimeDropdown', dateValues);
            //reset so uses defaults
            if (!dateValues) {
                this.cleanNewFormDaypart();
            }
            $("#new_program_airtime_input").daypartDropdown({
                record: null,
                dateObject: dateValues || { StartTime: 0, EndTime: 0, Sun: true, Mon: true, Tue: true, Wed: true, Thu: true, Fri: true, Sat: true },//needs defaults
                onClosePopup: this.onSelectFormDaypart.bind(this),
                popoverContainer: '#station_new_rates_program_modal' //important - set to avoid input focus issues - needs to be in the modal container not the form
            });

        },

        //on select daypart airtime 
        //destroy/sets after hide popoover - so need to reinitialize to make work
        onSelectFormDaypart: function (dateObject) {
            //console.log('onSelectFormDaypart', dateObject);
            //need to store values and then either reinitilize with new values (after destroy)
            this.newProgram.Airtime = dateObject;
            this.checkNewProgramConflicts();
            var me = this;
            //timeout is needed or the picker does not get detroyed in time for the reset
            setTimeout(function () {
                me.setFormAirtimeDropdown(dateObject);
            }, 500);
        },

        //clear/reset airtime daypart
        cleanNewFormDaypart: function () {
            //Remove possible dropdowna
            var daypartInput = $("#new_program_airtime_input");//$("#daypart-" + rowId).find(".edit-input");
            daypartInput.webuiPopover('destroy');
            daypartInput.removeData('plugin_daypartDropdown');

        },

       //validations
        setValidationRulesToForm: function () {
            $('#new_program_form').validate({
                rules: {
                    new_program_name_input: {
                        required: true
                    },
                    new_program_airtime_input: {
                        required: true
                    },
                    new_program_flight_input: {
                        required: true
                    },
                    new_program_hhimpressions_input: {
                        required: true
                    },
                    new_program_hhrating_input: {
                        required: true
                    },
                    new_program_spot15_input: {
                        require_from_group: [1, ".new-spot-cost"]
                    },
                    new_program_spot30_input: {
                        require_from_group: [1, ".new-spot-cost"]
                    }
                },
                messages: {
                    new_program_spot15_input: {
                        require_from_group: "One spot required."
                    },
                    new_program_spot30_input: {
                        require_from_group: "One spot required."
                    }
                }
            });
        },

        /*** CONFLICTS PROGRAM ***/

        //check airtime and flights vals - if both: get data from api call that will populate grid; show warning if conflicts changes unsaved
        //if none or just one then clear the grid data
        checkNewProgramConflicts: function () {
            if (this.newProgram.Airtime && this.newProgram.FlightStartDate && this.newProgram.FlightEndDate) {
                var checkObj = {
                    Airtime: this.newProgram.Airtime,
                    StartDate: this.newProgram.FlightStartDate,
                    EndDate: this.newProgram.FlightEndDate
                };
                var callback = this.setProgramConflictsGrid.bind(this);
                if (this.hasPendingConflictsChanges) {
                    util.confirm('Pending Conflict Changes', 'This action will clear all pending Conflict edits.', _view.controller.apiGetProgramConflicts.bind(_view.controller, this.activeStationCode, checkObj, callback));

                } else {
                    _view.controller.apiGetProgramConflicts(this.activeStationCode, checkObj, callback);
                }
            };
        },

        
        //prepare conflicts grid data add hasConflict; store ActiveFlights; store original start and end for use in ranges
        prepareProgramConflictGridData: function (data) {
            var displayData = util.copyData(data);
            var ret = [];

            $.each(displayData, function (index, value) {
                var item = value;
                item.recid = item.Id;
                //attempt to store condition that has original hiatus (in both Flights and ActiveFlights
                $.each(item.Flights, function (idx, flight) {
                    flight.hasOriginalHiatus = flight.IsHiatus;
                });
                item.ActiveFlights = util.copyArray(item.Flights);
                item.hasConflict = true;
                item.OriginalFlightStartDate = item.FlightStartDate;
                item.OriginalFlightEndDate = item.FlightEndDate;
                ret.push(item);
            });

            return ret;
        },

        //conflicts grid
        setProgramConflictsGrid: function (conflicts) {
            this.$ProgramConflictGrid.clear(false);
            if (conflicts) {
                var programConflictData = this.prepareProgramConflictGridData(conflicts);
                this.$ProgramConflictGrid.add(programConflictData);
            }

            this.$ProgramConflictGrid.resize();
            this.hasPendingConflictsChanges = false;//reset flag
        },

        //set grid events - tbd if need refresh
        initProgramConflictsGridEvents: function () {
            var me = this;
            this.$ProgramConflictGrid.onClick = this.onProgramConflictsGridClick.bind(this);
            //this.$ProgramConflictGrid.onRefresh = this.onProgramConflictsGridRefresh.bind(this);
            //prevent sorting if editing
            this.$ProgramConflictGrid.onSort = function (event) {
                if (me.activeConflictsRecord) {
                    event.preventDefault();
                    return;
                };
            };
        },

        //conflicts grid click: if the click is on flights (and editing not active)  then go to editing mode 
        onProgramConflictsGridClick: function (event) {
            if (this.activeConflictsRecord) { 
                return;
            }
            
            if (event.column === 3) {
                var record = this.$ProgramConflictGrid.get(event.recid);
                this.startConflictsActiveEdit(record);

                var $flightElem = $("#flight-" + this.activeConflictsRecord.recid);
                $flightElem.addClass("is-editing");

                var $flightInput = $flightElem.find(".edit-input");
                this.$ConflictFlightRangeElem = $flightInput;
                this.$ConflictFlightRangeElem.show();
                this.setConflictsFlightsPicker($flightInput, this.activeConflictsRecord);
                
                //open when set focus - allow set up time
                setTimeout(function () {
                    $flightInput.focus();
                });   
            }
        },

        //set activeConflictsRecord for flight editing 
        startConflictsActiveEdit: function (rec) {
            var original = util.copyData(rec, null, null, true);
            this.activeConflictsRecord = original;
        },

        //end editing - attempt to destroy picker - but depending on context may no longer exist to remove drp flights items (removing below on reset)
        endConflictsActiveEdit: function () {
            var $flightElem = $("#flight-" + this.activeConflictsRecord.recid);
            $flightElem.removeClass("is-editing");

            if (this.$ConflictFlightRangeElem && this.$ConflictFlightRangeElem.data('daterangepicker')) {
                this.$ConflictFlightRangeElem.data('daterangepicker').remove();
                this.$ConflictFlightRangeElem.hide();
                this.$ConflictFlightRangeElem = null;
            }
            this.activeConflictsRecord = null; //just reset each start?
        },

        //determine flights/hiatus changes - alter record Flights to most curent

        getConflictFlightsChanges: function (recid, flights) {
            var newFlights = [];
            $('#conflicts_flight_week_list_' + recid + ' input').each(function () {
                var originalFlight = util.objectFindByKey(flights, 'Id', parseInt($(this).val()));
                if (originalFlight) {
                    var notHiatus = $(this).prop('checked');
                    //originalFlight.isEdited = (originalFlight.IsHiatus === notHiatus);
                    originalFlight.IsHiatus = !notHiatus;
                    newFlights.push(originalFlight);
                }

            });
            return newFlights;
        },

        //set the grid record; change editing state; call the check single api; getconflicts format date per use for save/display
        setConflictFlightItem: function (start, end) {
            //this should update the stored FLights in each and retun a new version for UI handling (altered)
            var flightChanges = this.getConflictFlightsChanges(this.activeConflictsRecord.recid, this.activeConflictsRecord.Flights);
            //console.log('conflict flightChanges', flightChanges);
            start = start.startOf('week').weekday(1);
            end = (end.day() == 0) ? end : end.add(1, "week").startOf('week').weekday(0);
            var startSave = start.format('MM-DD-YYYY'),
                endSave = end.format('MM-DD-YYYY');
            var callback = this.setSingleProgramConflictStatus.bind(this);
            //change to single object per API change
            _view.controller.apiCheckSingleConflict(this.activeConflictsRecord.Id, { ConflictedProgramNewStartDate: startSave, ConflictedProgramNewEndDate: endSave, StartDate: this.newProgram.FlightStartDate, EndDate: this.newProgram.FlightEndDate }, callback);
            var displayFlight = start.format('MM/DD/YYYY') + ' - ' + end.format('MM/DD/YYYY');
            //this.activeConflictsRecord.Flights = flightChanges;//set here and then wehn extend on set call will add the changes to (extend)
            this.activeConflictsRecord.Flight = displayFlight;
            this.activeConflictsRecord.FlightStartDate = startSave;
            this.activeConflictsRecord.FlightEndDate = endSave;
            this.activeConflictsRecord.isEdited = true; //so can set indicator
            this.hasPendingConflictsChanges = true;
            this.$ProgramConflictGrid.set(this.activeConflictsRecord.recid, this.activeConflictsRecord);
            this.$ProgramConflictGrid.get(this.activeConflictsRecord.recid).ActiveFlights = flightChanges;//have to manually set this as the set does not change all
            //console.log('setConflictFlightItem new rec', this.$ProgramConflictGrid.get(this.activeConflictsRecord.recid));
            this.endConflictsActiveEdit();
            
        },

        //changes conflict status indicator in grid
        setSingleProgramConflictStatus: function (recid, status) {
            this.$ProgramConflictGrid.set(recid, {hasConflict: status});

        },

        //set the inputs in drp picker based on the actual flights from the record; handle disabled/checked
        //revised: use changed ActiveFlights if initial else use original flights if user change (start, end)
        //revised: need to disable hiatus if in original flights but not if changed in sessions - see hasOriginalHiatus
        setConflictFlightsInternalPicker: function (startDate, endDate, flightWeeksEl) {
            flightWeeksEl.html('');
            //if notInitial - just display all
            var notInitial = startDate ? true : false;
            var conflictFlights = notInitial ? this.activeConflictsRecord.Flights : this.activeConflictsRecord.ActiveFlights;
           // console.log('picker which flights - notInitial, flights', notInitial, conflictFlights);

            $.each(conflictFlights, function (index, item) {
                var endWeek = moment(item.EndDate);
                var startWeek = moment(item.StartDate);
                //show start not before selected start ; end not after selected end
                if (notInitial && (endWeek.diff(endDate, 'day') > 0) || (startWeek.diff(startDate, 'day') < 0)) {
                } else {
                    //need to also disable items in the past
                    var now = moment();
                    var inFuture = endWeek.diff(now, 'day') > 0;
                    var checked = item.IsHiatus ? '' : 'checked';
                    //var disabled = (item.IsHiatus || !inFuture) ? 'disabled' : '';
                    var disabled = (item.hasOriginalHiatus || !inFuture) ? 'disabled' : '';
                    var weekInterval = startWeek.format('YYYY/MM/DD') + " - " + endWeek.format('YYYY/MM/DD');
                   
                    flightWeeksEl.append('<li style="width: 200px; position: relative; right: 25px "><input value="' + item.Id + '" type="checkbox" id="checkbox_' + item.Id + '" ' + checked + ' ' + disabled + '> ' + weekInterval + '</li>');
                }
            });

        },

        // Conflicts Flight DPR - similar to Flights daterangepicker but different logic: set values; change;  apply remove editing max to new program flight; handle conflicts/flights, etc)
        //revise event handlers to deal with calling issues (i.e. remove hide as called first)
        //revise to store original flight start/end so can maintain those ranges
        setConflictsFlightsPicker: function (getElement, rec) {
            //this fix removes old if present- as context of destroy DRP has issues
            $('#conflicts_flight_week_list_' + rec.recid).remove();
            //USING NEW DATE - changes to GMT based so can be off a day
            //var today = moment();
            //var minStart = moment(new Date(rec.OriginalFlightStartDate));
            //var maxEnd = moment(new Date(rec.OriginalFlightEndDate));
            //var start = moment(new Date(rec.FlightStartDate));
            //var end = moment(new Date(rec.FlightEndDate));
            //var inPast = minStart.diff(today, 'day') < 0;

            var today = moment();
            var minStart = moment(rec.OriginalFlightStartDate);
            var maxEnd = moment(rec.OriginalFlightEndDate);
            var start = moment(rec.FlightStartDate);
            var end = moment(rec.FlightEndDate);
            var inPast = minStart.diff(today, 'day') < 0;

            
            //console.log('setConflictsFlightsPicker', inPast, minStart);
            var me = this;

            getElement.daterangepicker({
                autoUpdateInput: true,
                //if set this to TODAY cannot set the right start date
                // minDate: today,
                minDate: inPast ? today : minStart,
                maxDate: maxEnd,
                startDate: start,
                endDate: end,
                lockedInput: inPast ? 'daterangepicker_start' : '',
                locale: {
                    format: "MM/DD/YYYY",
                    firstDay: 1
                },
                opens: "left",
                template:
                '<div class="daterangepicker dropdown-menu">' +
                    '<div class="calendar left">' +
                        '<div class="daterangepicker_input">' +
                            '<input class="input-mini form-control" type="text" name="daterangepicker_start" value="" />' +
                            '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                            '<div class="calendar-time">' +
                                '<div></div>' +
                                '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                            '</div>' +
                        '</div>' +

                    '<div class="calendar-table"></div>' +
                '</div>' +

                '<div class="calendar right">' +
                    '<div class="daterangepicker_input">' +
                        '<input class="input-mini form-control" type="text" name="daterangepicker_end" value="" />' +
                        '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                        '<div class="calendar-time">' +
                            '<div></div>' +
                            '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                        '</div>' +
                    '</div>' +

                    '<div class="calendar-table"></div>' +
                '</div>' +

                '<div class="ranges" style="width: 220px;" >' +
                    '<h5>Flight Weeks</h5>' +
                    '<ul id="conflicts_flight_week_list_' + rec.recid + '" style="width: 220px; padding-left:20px; max-height: 180px; overflow-y: auto; margin-bottom: 20px;"></ul>' + // weeks checkbox list goes in here

                    '<div class="range_inputs">' +
                        '<button class="applyBtn" disabled="disabled" type="button"></button> ' +
                        '<button class="cancelBtn" data-dismiss="modal" type="button"></button>' +
                    '</div>' +
                '</div>'
            });

            //function (start, end) {
            //    //this returns before the hide but not on apply if no date change
            //   /// console.log('callback conflicts picker', start, end);
            //    me.setConflictFlightItem(start, end);
            //});


            //drp events - hide happens before other events get called so not good canidate
            // use apply (not handler as does not register unless dates change); cancel, and undocumented outsideClick


            getElement.on('apply.daterangepicker', function (ev, picker) {
                //console.log('apply event conflict picker', ev, picker);
                me.setConflictFlightItem(picker.startDate, picker.endDate);
                
            });

            getElement.on('outsideClick.daterangepicker', function (ev, picker) {
                //console.log('outsideClick event conflict picker', ev, picker);
                me.endConflictsActiveEdit();
            });

            getElement.on('cancel.daterangepicker', function (ev, picker) {
                //console.log('cancel event conflict picker', ev, picker);
                me.endConflictsActiveEdit();
            });

            //problematic as called before other events
            //getElement.on('hide.daterangepicker', function (ev, picker) {
            //    console.log('hide conflicts picker', ev, picker);
            //    me.endConflictsActiveEdit();
            //});

            // reevaluates the list of weeks for hiatus selection - adapt to use flights
            getElement.on('show.daterangepicker', function (ev, picker) {
                //set initially
                me.setConflictFlightsInternalPicker(null, null, $('#conflicts_flight_week_list_' + me.activeConflictsRecord.recid));
                picker.container.find('.calendar')
                    .on('mousedown.daterangepicker', 'td.available', $.proxy(function (e) {
                        var flightWeeksEl = $('#conflicts_flight_week_list_' + me.activeConflictsRecord.recid);
                        //flightWeeksEl.html('');

                        if (picker.startDate && picker.endDate) {
                            //console.log('drp mouse down initial: ', picker.startDate.format('YYYY/MM/DD'), picker.endDate.format('YYYY/MM/DD'), e);
                            // clone moment objects to avoid changing original values
                            var startDate = picker.startDate.clone();
                            var endDate = picker.endDate.clone();
                            //if end date is in past then change to today 
                            //ISSUE- when setEndDate input updates - the display (selected calendar) is not updated; use updateView sets the proper end date active
                            //but does not show the correct calendar if in different month
                            if (endDate.diff(moment(), 'day') < 0) {
                                picker.setEndDate(moment());
                                endDate = moment();
                                //setso view selection will update
                                picker.updateView();
                            }

                            startDate = startDate.day() != 0 ? startDate.startOf('week').weekday(1) : startDate.add(-1, "week").startOf('week').weekday(1);
                            endDate = endDate.day() == 0 ? endDate : endDate.add(1, "week").startOf('week').weekday(0);

                            me.setConflictFlightsInternalPicker(startDate, endDate, flightWeeksEl);
                        }
                    }, this));

                //Disable handlers to turn off daterangepicker hoverRange errors
                picker.container.find('.ranges')
                    .off('mouseenter.daterangepicker', 'li')
                    .off('mouseleave.daterangepicker', 'li')
                    .off('click.daterangepicker', 'li')
            });
        },

        //on user entered spot 30 change - get then set default values for spot 15
        //issue - W2ui calls change event twice if input initially empty and formatted
        //check not existing and val !existing
        onSpot30change: function (input30) {
            //var val = parseFloat(input30.val().replace(/[$,]+/g, ""));
            var val = input30.val().replace(/[$,]+/g, "");//allow 0.00
            if (val && (!this.lastInput30Val || (this.lastInput30Val != val))) {
                console.log('spot 30 change', val);
                //call api and set
            }
            this.lastInput30Val = val;
        }

    };

};