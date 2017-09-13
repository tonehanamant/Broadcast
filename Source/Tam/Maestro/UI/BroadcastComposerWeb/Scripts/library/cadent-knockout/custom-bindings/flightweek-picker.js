(function () {
    function InitializeFlightWeekPicker(flightWeekInput, startDate, endDate, weekSelection, noMinimum, applyCallback, drop, guid) {
        var me = this;
        noMinimum = noMinimum || false;
        drop = drop || 'down';
        flightWeekInput.val('');

        var dateRangeconfig = {
            autoUpdateInput: false,
            drops: drop,
            startDate: startDate() ? moment(startDate()).format('MM/DD/YYYY') : "",
            endDate: endDate() ? moment(endDate()).format('MM/DD/YYYY') : "",
            locale: {
                format: "MM/DD/YYYY",
                firstDay: 1
            },
            opens: "right",           
            template:
            '<div class="daterangepicker dropdown-menu">' +
                '<div class="calendar left">' +
                    '<div class="daterangepicker_input">' +
                        '<input class="input-mini form-control mini_' + guid + '" type="text" name="daterangepicker_start" value="" />' +
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
                    '<input class="input-mini form-control mini_' + guid + '" type="text" name="daterangepicker_end" value="" />' +
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
                '<ul id="flight_week_list_' + guid + '" style="width: 220px; padding-left:20px; max-height: 180px; overflow-y: auto; margin-bottom: 20px;"></ul>' +

                '<div class="range_inputs">' +
                    '<button class="applyBtn" disabled="disabled" type="button"></button> ' +
                    '<button class="cancelBtn" data-dismiss="modal" type="button"></button>' +
                '</div>' +
            '</div>'
        };

        flightWeekInput.daterangepicker(dateRangeconfig);

        /*** HANDLERS ***/

        flightWeekInput.on('show.daterangepicker', function (ev, picker) {
            if (!noMinimum) picker.minDate = moment();
            picker.updateView();

            // always updates picker with observable values on show
            if (startDate() && (endDate())) {
                picker.setStartDate(moment(startDate()));
                picker.setEndDate(moment(endDate()));
                updateWeekSelection(picker, weekSelection(), guid);
            }
            
            // handler: on selecting a new range (doesn't update the observables)
            picker.container.find('.calendar')
                .on('mousedown.daterangepicker', 'td.available', $.proxy(function (e) {
                    updateWeekSelection(picker, weekSelection(), guid);
                }, this));

            // disable handlers to turn off daterangepicker hoverRange errors
            picker.container.find('.ranges')
                .off('mouseenter.daterangepicker', 'li')
                .off('mouseleave.daterangepicker', 'li')
                .off('click.daterangepicker', 'li');
        });

        // restores initial display when user closes in an invalid state
        flightWeekInput.on('hide.daterangepicker', function (ev, picker) {
            if (startDate() && endDate()) {
                var validInput = moment(startDate()).isAfter(picker.minDate) && moment(endDate()).isAfter(picker.minDate);
                if (!validInput) {
                    var originalStart = moment(startDate());
                    var originalEnd = moment(endDate());
                    updateDisplay(picker, originalStart, originalEnd);
                }
            }

            picker.minDate = null;
        });

        // handler: APPLY button -- updates observables
        flightWeekInput.on('apply.daterangepicker', function (ev, picker) {
            var flights = [];
            $('#flight_week_list_' + guid + ' input').each(function (item) {
                var isChecked = $(this).context.checked;
                var id = parseInt($(this).data('mediaweek')); // id if existing (from BE) else 0
                var flight = {
                    StartDate: $(this).data('start'),
                    EndDate: $(this).data('end'),
                    IsHiatus: !($(this).prop('checked')),
                    MediaWeekId: id
                };

                flights.push(flight);
            });

            var formattedStartDate = picker.startDate.format('MM/DD/YYYY');
            var formattedEndDate = picker.endDate.format('MM/DD/YYYY');

            // update observables and input element
            weekSelection(flights);
            startDate(formattedStartDate);
            endDate(formattedEndDate);
            flightWeekInput.val(formattedStartDate + ' - ' + formattedEndDate);
            flightWeekInput.valid();
            if (applyCallback && typeof applyCallback === 'function') {
                applyCallback(picker);
            }
        });

        // handler: manual input
        //make sure unique
        //$(".daterangepicker").on("change", ".input-mini", function (event) {
        $(".daterangepicker").on("change", ".mini_" + guid, function (event) {
            var picker = flightWeekInput.data('daterangepicker');
            var name = $(this).attr("name");
            var value = moment($(this).val());
            if (value.isValid()) {
                if (name == "daterangepicker_start") {
                    picker.setStartDate(value);
                } else {
                    picker.setEndDate(value);
                }

                updateWeekSelection(picker, weekSelection(), guid);
            }
        });
    };

    /*** HELPERS ***/

    function updateWeekSelection(picker, weekSelection, guid) {
        var startDate = picker.startDate;
        var endDate = picker.endDate;

        if (startDate && startDate.isValid() && endDate && endDate.isValid()) {
            var flightWeeksEl = $('#flight_week_list_'+ guid);
            flightWeeksEl.html('');

            // checks if the range is within the same week
            var durationDays = moment.duration(endDate.diff(startDate)).asDays();
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
            //add MediaWeekId so can determine change
            for (var week = 0; week < amountWeeks; week++) {
                var startWeek = startDate.clone().add(week, "week");
                var endWeek = startWeek.clone().add(1, "week").startOf('week').weekday(0);

                // Find if week already exists in the observable and determine if should be rendered as checked
                var checked = true;
                var weekId = 0;
                if (weekSelection) {
                    for (var i = 0; i < weekSelection.length; i++) {
                        var preexistingWeek = moment(weekSelection[i].StartDate);
                        if (startWeek.isSame(preexistingWeek) && weekSelection[i].IsHiatus) {
                            checked = false;
                            break;
                        }
                        //add id if existing
                        if (startWeek.isSame(preexistingWeek)) {
                            weekId = (weekSelection[i] && weekSelection[i].MediaWeekId) ? weekSelection[i].MediaWeekId : 0;
                        }
                    }
                    
                }

                var weekInterval = startWeek.format('MM/DD/YYYY') + " - " + endWeek.format('MM/DD/YYYY');

                if (checked) {
                    flightWeeksEl.append('<li style="width: 200px; position: relative; right: 25px "><input data-mediaweek="'+ weekId + '" data-start="' + startWeek.format('YYYY-MM-DD') + '" data-end="' + endWeek.format('YYYY-MM-DD') + '"type="checkbox" id="checkbox_' + week + '" checked> ' + weekInterval + '</li>');
                } else {
                    flightWeeksEl.append('<li style="width: 200px; position: relative; right: 25px "><input data-mediaweek="' + weekId + '" data-start="' + startWeek.format('YYYY-MM-DD') + '" data-end="' + endWeek.format('YYYY-MM-DD') + '"type="checkbox" id="checkbox_' + week + '"> ' + weekInterval + '</li>');
                }
            }

            // adjust start and end dates
            startDate.startOf('week').weekday(1);
            endDate.day() == 0 ? endDate : endDate.add(1, "week").startOf('week').weekday(0);
            picker.updateView();

            // update display
            updateDisplay(picker, picker.startDate, picker.endDate);
        };
    };

    // updates inputs with formatted values
    function updateDisplay(picker, startMoment, endMoment) {
        var formattedStartDate = startMoment.format('MM/DD/YYYY');
        var formattedEndDate = endMoment.format('MM/DD/YYYY');
        picker.element.val(formattedStartDate + ' - ' + formattedEndDate);
        //make unique
        $(".mini_" + guid + ".input[name='daterangepicker_start']").val(formattedStartDate);
        $(".mini_" + guid + ".input[name='daterangepicker_end']").val(formattedEndDate);
    }

    /*** KO custom binding definition ***/
    //adding noMinimum option - if defined (true) then do not set min date; applyCallback - optional callback passed back to VM on apply (with picker argument);
    //fix use of static id/clases in template ('#flight_week_list' input-mini) - define timestamp based guid
    //fix use of static id/clases in template ('#flight_week_list' input-mini) - define timestamp based guid
    //added: drop accessor defaults to down - set to "up" to drop up (drops property in picker)
    //example usage all options: data-bind="flightWeekPicker: { startDate: FlightStartDate, endDate: FlightEndDate, weekSelection: FlightWeeks, noMinimum: true, drop: "up", applyCallback: onFlightSelect }"
    ko.bindingHandlers.flightWeekPicker = {
        init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            //this.guid = moment().unix();//not specific enough - moment clips the milliseconds
            this.guid = moment().valueOf();
            
            InitializeFlightWeekPicker($(el), valueAccessor().startDate, valueAccessor().endDate, valueAccessor().weekSelection, valueAccessor().noMinimum, valueAccessor().applyCallback, valueAccessor().drop, this.guid);
        },

        update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
            var allBindings = allBindingsAccessor();
            var startDate = ko.utils.unwrapObservable(allBindings.flightWeekPicker.startDate);
            var endDate = ko.utils.unwrapObservable(allBindings.flightWeekPicker.endDate);
            var weekSelection = ko.utils.unwrapObservable(allBindings.flightWeekPicker.weekSelection);
            var picker = $(el).data('daterangepicker');
            //console.log('guid', this.guid, moment().valueOf());

            if (startDate && endDate) {
                var startMoment = moment(startDate);
                var endMoment = moment(endDate);

                picker.setStartDate(startMoment);
                picker.setEndDate(endMoment);

                updateWeekSelection(picker, weekSelection, this.guid);
            } else {
                picker.setStartDate(moment());
                picker.setEndDate(moment());
                $(el).val('');
                $("#flight_week_list_" + this.guid).empty();
            }
        }
    };
})();