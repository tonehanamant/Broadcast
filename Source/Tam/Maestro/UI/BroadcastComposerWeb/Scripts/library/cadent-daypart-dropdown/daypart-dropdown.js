// the semi-colon before function invocation is a safety net against concatenated
// scripts and/or other plugins which may not be closed properly.

; (function ($, window, document, undefined) {

    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than global
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    // Create the defaults once
    var pluginName = "daypartDropdown",
        defaults = {
            record: null,
            dateObject: null,
            onClosePopup: null,
        };

    function dateToString(dateObj) {
        var builder = [];
        var daysBuilder = [];
        var dayAbbreviationStrings = ["M", "TU", "W", "TH", "F", "SA", "SU"];
        var days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        var startDay = 0;
        var endDay = 0;
        var currentDay = 0;

        while (currentDay <= 6) {
            if (dateObj[days[currentDay]]) {
                startDay = currentDay;
                while (++currentDay <= 6 && dateObj[days[currentDay]])
                    endDay = currentDay;

                var aux = daysBuilder.length > 0 ? "," : "";
                if (endDay > startDay) {
                    daysBuilder.push(aux + dayAbbreviationStrings[startDay] + "-" + dayAbbreviationStrings[endDay]);
                } else {
                    daysBuilder.push(aux + dayAbbreviationStrings[startDay]);
                }
            }
            else
                currentDay++;
        }

        builder.push(daysBuilder.join(""));
        builder.push(timeToString(dateObj.StartTime) + "-" + timeToString(dateObj.EndTime));
        return builder.join(" ");
    }

    function timeToString(time) {
        var seconds = parseInt(time % 60),
            minutes = parseInt((time / 60) % 60),
            hours = parseInt((time / (60 * 60)) % 24);

        var time = new Date(1970, 0, 2, hours, minutes, seconds, 0);

        return moment(time).format("h:mmA").replace(":00", "");
    }

    function renderCustomElement(option) {
        var itemContent = "<div class='custom-option-element' data-key=" + option.key + ">";
        itemContent += "<span>" + option.description + "</span>";
        itemContent += "</div>";

        return itemContent;
    }

    function renderDayElement(day) {
        var itemContent = "<div class='day-element'>";
        itemContent += "<div class='checkbox'>";
        itemContent += "<label>";
        itemContent += "<input id='" + day.key + "' type='checkbox'>" + day.description;
        itemContent += "</label>";
        itemContent += "</div>";
        itemContent += "</div>";
        return itemContent;
    }

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;

        // jQuery has an extend method which merges the contents of two or
        // more objects, storing the result in the first object. The first object
        // is generally empty as we don't want to alter the default options for
        // future instances of the plugin
        this.settings = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._selected = undefined;
        this._isOpen = false;
        this.init();
    }

    // Avoid Plugin.prototype conflicts
    $.extend(Plugin.prototype, {
        init: function () {
            var _this = this;
            var days = [
                { key: 'Mon', description: 'Monday' },
                { key: 'Tue', description: 'Tuesday' },
                { key: 'Wed', description: 'Wednesday' },
                { key: 'Thu', description: 'Thursday' },
                { key: 'Fri', description: 'Friday' },
                { key: 'Sat', description: 'Saturday' },
                { key: 'Sun', description: 'Sunday' },
            ];
            var customOptions = [
                { key: 1, description: 'Everyday' },
                { key: 2, description: 'Weekdays' },
                { key: 3, description: 'Weekends' },
            ];
            this.clickCustomOption = this.clickCustomOption.bind(this);
            this.handleOutsideClick = this.handleOutsideClick.bind(this);
            this.handleKeyDown = this.handleKeyDown.bind(this);
            this.handleBodyKeyDown = this.handleBodyKeyDown.bind(this);
            this.onChangeCheckbox = this.onChangeCheckbox.bind(this);
            this.onChangeTime = this.onChangeTime.bind(this); // Place initialization logic here
            // You already have access to the DOM element and
            // the options via the instance, e.g. this.element
            // and this.settings
            // you can add more functions like the one below and
            // call them like the example below

            $(this.element).webuiPopover({
                arrow: false,
                multiple: true,
                animation: 'pop',
                style: 'custom',
                offsetTop: 1,
                offsetLeft: 0,
                dismissible: false,
                padding: 0,
                placement: 'auto-bottom',
                width: 300,
                onShow: function ($popoverElement) {
                    _this._isOpen = true;
                    //$(".timepicker").timepicker();
                    $(".timepicker")
                        .timepicker(
                        {
                            'step': 60
                        });;
                    $(".timepicker")
                        .on("changeTime",
                            function () {

                                $(this).data('valid-time', $(this).value);
                                _this.onChangeTime();

                            }).on('timeRangeError timeFormatError', function () {

                                $(this).val($(this).data('valid-time'));
                                // alert the user here
                            });
                    if (_this.settings.dateObject) {
                        _this.setValues();
                    }

                    $(".custom-option-element").on("click", _this.clickCustomOption);
                    $("body").on("click", _this.handleOutsideClick);
                    //$(document).on("keydown", _this.handleBodyKeyDown);
                    $(".checkbox input").on("change", _this.onChangeCheckbox);
                },
                onHide: function ($popoverElement) {
                    _this._isOpen = false;

                    $(".custom-option-element").off("click", _this.clickCustomOption); //$("body").off("click", _this.handleOutsideClick)
                    $(document).off("keydown", _this.handleBodyKeyDown);
                    $(_this.element).off("keydown", _this.handleKeyDown);
                    $(".timepicker").timepicker('hide');
                    $(".timepicker").off("changeTime", _this.onChangeTime);
                    $(".checkbox input").off("change", _this.onChangeCheckbox);
                    $("body").off("click", _this.handleOutsideClick);
                    if (_this.settings.onClosePopup) {
                        _this._onClosePopup();
                    }
                    _this.destroy();
                },
                content: function () {
                    var content = "<div id='daypart-dropdown'>";

                    //LeftContainer
                    content += "<div class='daypart-left-container'>";
                    //Custom options
                    content += "<div class='custom-options-container'>";
                    content += customOptions.map(renderCustomElement).join('');
                    content += "</div>"; //Days
                    content += "<div class='days-container'>";
                    content += days.map(renderDayElement).join('');
                    content += "</div>";
                    content += "</div>";

                    //Right Container
                    content += "<div class='daypart-right-container'>";
                    content += "<div class='time-pickers-container'>";
                    content += "<input id='starttime' type='text' class='timepicker' />";
                    content += "<span>to</span>";
                    content += "<input id='endtime' type='text' class='timepicker' />";
                    content += "</div>";
                    content += "</div>";

                    content += "</div>";
                    return content;
                }
            });
            $(this.element).on("keydown", this.handleKeyDown);
        },

        onChangeCheckbox: function (event) {
            this.updateInputText();
        },

        onChangeTime: function (event) {
            this.updateInputText();
        },

        updateInputText: function() {
            var dateObject = this.generateDateObject();
            var dateString = dateToString(dateObject);
            this.element.value = dateString;
        },

        handleBodyKeyDown: function (event) {
            event.preventDefault();
            if (event.keyCode === 13) { //ENTER
                if (this.settings.onClosePopup) {
                    this._onClosePopup();
                }
                this.destroy();
            }
        },

        handleKeyDown: function (event) {
            if (event.keyCode === 9) {//TAB
                var _this = this;
                event.preventDefault(); //this._onClosePopup();
                this.destroy();
                setTimeout(function () {
                    var nextCell = $("#cpm-" + _this.settings.record.recid);
                    nextCell.click();
                    nextCell.find("input").focus();
                }, 400);
            }
            else if (event.keyCode === 13) { //ENTER
                if (this.settings.onClosePopup) {
                    this._onClosePopup();
                }
                this.destroy();
            }
        },

        setValues: function() {
            var dateObj = this.settings.dateObject;
            var $daysContainer = $(".days-container");
            var days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
            days.forEach(function (d, i) {
                if (dateObj[d]) {
                    $($daysContainer.find("#" + d)).prop("checked", true);
                }
            });
            var $container = $("#daypart-dropdown");
            var startTimepicker = $($container.find("#starttime"));
            var endTimepicker = $($container.find("#endtime"));
            
            startTimepicker.timepicker("setTime", dateObj.StartTime);
            endTimepicker.timepicker("setTime", dateObj.EndTime);
        },

        clickCustomOption: function (event) {
            var key = $(event.currentTarget).data("key");
            var days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
            var daysContainer = $(".days-container");
            function toogleCheckbox(checked, day) {
                var elem = $(daysContainer.find("#" + day));
                elem.prop("checked", checked);
            }
            function isWeekDay(x) { return x !== "Sat" && x !== "Sun"; }
            function isWeekendDay(x) { return x === "Sat" || x === "Sun"; }

            if (key === 1) { //Everyday
                days.forEach(toogleCheckbox.bind(null, true));
            }
            else if (key === 2) { //Weekdays
                days
                    .filter(isWeekDay)
                    .forEach(toogleCheckbox.bind(null, true));
                days
                    .filter(isWeekendDay)
                    .forEach(toogleCheckbox.bind(null, false));
            }
            else { //Weekends
                days
                    .filter(isWeekendDay)
                    .forEach(toogleCheckbox.bind(null, true));
                days
                    .filter(isWeekDay)
                    .forEach(toogleCheckbox.bind(null, false));
            }

            this.updateInputText();
        },

        handleOutsideClick: function (e) {
            var clientX = e.clientX;
            var clientY = e.clientY;

            var $container = $("#daypart-dropdown");
            var clientRect = $container[0].getBoundingClientRect();
            var isInside =
                (clientX >= clientRect.left && clientX <= clientRect.right) &&
                (clientY >= clientRect.top && clientY <= clientRect.bottom);
            if (!isInside) {
                this.destroy();
            }
        },

        generateDateObject: function() {
            function hourToSeconds(h, m) {
                return h * 3600 + m * 60;
            }
            var $container = $("#daypart-dropdown");
            var $daysContainer = $(".days-container");
            var days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
            var startTime = $($container.find("#starttime")).timepicker("getTime");
            var endTime = $($container.find("#endtime")).timepicker("getTime");
            var endTimeSeconds = endTime && hourToSeconds(endTime.getHours(), endTime.getMinutes());
            if (endTimeSeconds === 0) {
                endTimeSeconds = 86400;
            }
            var dateValue = {
                StartTime: hourToSeconds(startTime.getHours(), startTime.getMinutes()),
                EndTime: endTimeSeconds,
            };
            days.forEach(function (day, i) {
                dateValue[day] = $($daysContainer.find("#" + day)).prop("checked");
            });
            return dateValue;
        },

        _onClosePopup: function () {
            var dateValue = this.generateDateObject();
            this.settings.onClosePopup(dateValue);
        },

        destroy: function () {
            $(this.element).webuiPopover('destroy');
            $(this.element).removeData('plugin_' + pluginName);
        }
    });

    // A really lightweight plugin wrapper around the constructor,
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" +
                    pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);