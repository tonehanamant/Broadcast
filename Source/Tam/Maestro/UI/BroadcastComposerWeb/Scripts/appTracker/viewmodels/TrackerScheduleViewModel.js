
//tracker schedule view model: handle Form Modal scenarios
//variable form: Upload scx or csv - Update Import Schedules (Todo); Create/Edit Advertiser (Schedule);
//see global wrappers - using daypart, date wrapper here
var TrackerScheduleViewModel = function (controller) {
    var $scope = this;

    var controller = controller;
    var scheduleValidator = null;
    $scope.activeSchedule = null;  //active schedule upload (non observable)
    $scope.activeFormType = ko.observable('upload');  //'upload', 'advertiser'
    $scope.activeFormMode = ko.observable('create'); // 'create', 'edit'

    //from InitialData
    $scope.Advertisers = ko.observableArray();
    $scope.PostingBooks = ko.observableArray();
    $scope.InventorySources = ko.observableArray();
    $scope.PostTypes = ko.observableArray();
    $scope.Markets = ko.observableArray();
    $scope.Demos = ko.observableArray();//tbd

    //stored observables - schedule shared -  upload/advertisers
    $scope.ScheduleName = ko.observable('');

    $scope.IscisBrand = ko.observableArray();
    $scope.Iscis = ko.computed({
        read: function () {
            if (!$scope.IscisBrand()) {
                return "";
            }

            var iscisFormatted = $scope.IscisBrand().reduce(function (aggregated, iscisBrand) {
                var house = iscisBrand.House || "";
                var client = iscisBrand.Client || "";
                var brand = iscisBrand.Brand || "";

                return aggregated + house.trim() + "," + client.trim() + "," + brand.trim() + "\n";
            }, "");

            return iscisFormatted;
        },

        write: function (value) {
            var result = value;

            if (value && value.constructor !== Array) {
                result = value.match(/([^\r\n]+)/g).map(function (entry) {
                    var splitted = entry.split(",");
                    var ret = {
                        House: splitted[0] ? splitted[0].trim() : null,
                        Client: splitted[1] ? splitted[1].trim() : null,
                        Brand: splitted[2] ? splitted[2].trim() : null
                    };
                    //add if over valid limit
                    if (splitted[3]) ret.errorLength = true;

                    return ret;
                });
            }

            $scope.IscisBrand(result);
        }
    });

    $scope.AdvertiserId = ko.observable('');
    $scope.PostingBookId = ko.observable('');
    $scope.MarketRestrictions = ko.observableArray([]);

    //on daypart selection; converted for observable/BE
    $scope.onDaypartSelect = function (dateValues) {
        //just update the observable to change value?
        //console.log('onDaypartSelect', dateValues);
        $scope.DaypartRestriction(dateValues);
    };

    //remove daypart restriction
    $scope.onDaypartRestrictionRemove = function () {
        $scope.DaypartRestriction(null);
        $scope.daypartWrap.remove();//remove
        $scope.daypartWrap.init();//reinit so has default values
    };
    $scope.daypartWrap = new wrappers.daypartWrapper($("#schedule_input_daypart"), '#scheduleModal', $scope.onDaypartSelect.bind($scope));
    $scope.DaypartRestriction = ko.observable(null);

    //TODO call to change specifically
    //$scope.daypartWrap.init();

    $scope.PostTypeId = ko.observable(1);//default 1 NSI
    $scope.Equivalized = ko.observable(true);//default true

    $scope.ScheduleId = ko.observable(null);//used for existing schedule - edit mode

    $scope.DemoInputs = ko.observableArray([]);//dependent on context (upload file: no, upload edit settings: yes, advertiser: yes both create/edit 

    //subcribe to check validation - Validation does not auto update with Select2
    $scope.DemoInputs.subscribe(function (values) {
        //console.log('DemoInputs change', values);
        if (values && values.length) {
            scheduleValidator.element("#schedule_input_demos");
        }
    });

    //storedObservables - upload only
    $scope.FileName = ko.observable('');//not in save
    $scope.InventorySourceId = ko.observable(4); //default 4 Open Market TBD
    $scope.EstimateId = ko.observable();

    //EstimateId change - in upload mode get prepopulated data
    //todo: need to set only by mode or prevent - if another context(edit)
    $scope.estimateIdChange = function () {
        var id = $scope.EstimateId();
        if (id) {
            controller.apiGetScheduleHeader(id, function (scheduleHeader) {
                $scope.Iscis(scheduleHeader.ISCIs);
            });
        } else {
            $scope.Iscis([]);
        }
    };

    //stored observables - advertiser only

    $scope.startDateWrap = null;//start date picker wrapper
    $scope.StartDate = ko.observable('');
    //callback from wrap picker apply
    $scope.onStartDateChange = function (start) {
        //just update the observable to change value?
        console.log('onStartDateChange', start);
        $scope.StartDate(start);
    };

    $scope.endDateWrap = null;//start date picker wrapper
    $scope.EndDate = ko.observable('');
    //callback from wrap picker apply
    $scope.onEndDateChange = function (end) {
        //just update the observable to change value?
        console.log('onEndDateChange', end);
        $scope.EndDate(end);
    };

    $scope.initDateWrappers = function () {
        $scope.startDateWrap = new wrappers.datePickerSingleWrapper($("#schedule_input_start_date"), $scope.onStartDateChange.bind($scope));
        $scope.startDateWrap.init();
        $scope.endDateWrap = new wrappers.datePickerSingleWrapper($("#schedule_input_end_date"), $scope.onEndDateChange.bind($scope));
        $scope.endDateWrap.init();
    };

    //STATE
    //title based on type and create/edit
    $scope.scheduleTitle = ko.computed(function () {
        if ($scope.activeFormType() === 'upload') {
            if ($scope.activeFormMode() === 'edit') {
                return $scope.ScheduleName();
            } else {
                return 'Upload Schedule File';
            }
        } else if ($scope.activeFormType() === 'advertiser') {
            return 'Schedule Preferences';
        } else {
            return '';
        }
    });

    //from view set form mode/type before open modal
    $scope.setFormTypeMode = function (type, mode) {
        $scope.activeFormType(type); //'upload', 'advertiser'
        $scope.activeFormMode(mode); //'create', 'edit'
    };

    //set Schedule (from View) on modal shown - dependent on type/mode
    $scope.setSchedule = function (scheduleRequest) {
        var type = $scope.activeFormType(), mode = $scope.activeFormMode();
        scheduleValidator = $("#schedule_form").validate();
        $scope.resetSchedule();//reset schedule
        $scope.activeSchedule = scheduleRequest;
        if (type === 'upload') {
            (mode === 'create') ? $scope.initScheduleUpload() : $scope.editScheduleUpload();
        } else if (type === 'advertiser') {
            (mode === 'create') ? $scope.initScheduleAdvertiser() : $scope.editScheduleAdvertiser();
        }
    };

    //set components
    $scope.initComponents = function (components) {
        $scope.Advertisers(components.Advertisers);
        $scope.PostingBooks(components.PostingBooks);
        $scope.Markets(_.sortBy(components.Markets, ['Display']));
        $scope.InventorySources(components.InventorySources);
        $scope.PostTypes(components.SchedulePostTypes);
        $scope.Demos(components.Audiences);
        //set date wrapper inputs
        $scope.initDateWrappers();
    };

    //UPLOAD
    //initial schedule (create) - for upload
    $scope.initScheduleUpload = function () {
        var schedule = $scope.activeSchedule;
        $scope.FileName(schedule.FileName);
        $scope.daypartWrap.init();
    };

    //prepare for post; check schedule exists via controller 
    $scope.uploadSchedule = function () {
        if (scheduleValidator.form() && $scope.checkIscis()) {
            var schedule = $scope.prepareScheduleForUpload();
            controller.apiCheckUploadSchedule(schedule);
        }
    };

    //for file upload - (Create)
    $scope.prepareScheduleForUpload = function () {
        var schedule = $scope.activeSchedule;
        schedule.ScheduleName = $scope.ScheduleName();       
        schedule.Iscis = $scope.IscisBrand();
        schedule.EstimateId = parseInt($scope.EstimateId());
        schedule.AdvertiserId = $scope.AdvertiserId();
        schedule.PostingBookId = $scope.PostingBookId();

        schedule.MarketRestrictions = $scope.MarketRestrictions();
        schedule.DaypartRestriction = $scope.DaypartRestriction();
        schedule.PostType = $scope.PostTypeId();
        schedule.InventorySource = $scope.InventorySourceId();
        schedule.Equivalized = $scope.Equivalized();
        return schedule;
    };

    //show a warning if controller determines there are existing schedule that will overwrite
    $scope.showSchedulesWarning = function (schedule) {
        var message = "The following schedule already exist (Estimate Id): " + schedule.EstimateId + ".  Would you like to overwrite?";
        //confirm - if continue then controller will upload/overwrite;
        util.confirm('Schedule Exists', message, controller.apiUploadSchedule.bind(controller, schedule));
    };


    //edit existing schedule upload
    $scope.editScheduleUpload = function () {
        var schedule = $scope.activeSchedule;
        $scope.ScheduleName(schedule.Name);
        $scope.Iscis(schedule.Iscis);//get Iscis for Brand update
        $scope.EstimateId(schedule.Estimate);
        $scope.AdvertiserId(schedule.AdvertiserId);
        $scope.PostingBookId(schedule.PostingBookId);
        $scope.MarketRestrictions(schedule.MarketRestrictions);

        $scope.DaypartRestriction(schedule.DaypartRestriction);
        //set with converted values if exist; else empty
        $scope.daypartWrap.init(schedule.DaypartRestriction, true);

        $scope.PostTypeId(schedule.PostType);
        $scope.InventorySourceId(schedule.InventorySource);
        $scope.DemoInputs(schedule.Audiences);
        $scope.Equivalized(schedule.IsEquivalized);
    };

    //ADVERTISER

    $scope.initScheduleAdvertiser = function () {
        $scope.daypartWrap.init();//init daypart
    };

    //check dates -show custom error as needed
    $scope.checkStartEnd = function () {
        //console.log($scope.StartDate().isBefore($scope.EndDate()));
        if ($scope.StartDate().isBefore($scope.EndDate())) {
            return true;
        } else {
            scheduleValidator.showErrors({
                "schedule_input_start_date": "Start date must be before end date.",
                "schedule_input_end_date": "End date must be after start date."
            });
            return false;
        }
    };

    $scope.editScheduleAdvertiser = function () {
        var schedule = $scope.activeSchedule;

        $scope.ScheduleName(schedule.Name);
        $scope.Iscis(schedule.Iscis);
        $scope.AdvertiserId(schedule.AdvertiserId);
        $scope.PostingBookId(schedule.PostingBookId);
        $scope.MarketRestrictions(schedule.MarketRestrictions);
        $scope.DaypartRestriction(schedule.DaypartRestriction);
        $scope.daypartWrap.init(schedule.DaypartRestriction, true);
        $scope.PostTypeId(schedule.PostType);
        
        // sort by rank before initializing observable
        schedule.Audiences.sort(function (audience1, audience2) {
            return audience1.Rank - audience2.Rank;
        });

        var rankedScheduleIds = schedule.Audiences.map(function (audience) {
            return audience.audienceId;
        });

        $scope.DemoInputs(rankedScheduleIds);
        $scope.Equivalized(schedule.IsEquivalized);

        if (schedule.StartDate) {
            var start = moment(schedule.StartDate);
            $scope.startDateWrap.setStart(start);
            $scope.StartDate(start);
        }

        if (schedule.EndDate) {
            var end = moment(schedule.EndDate);
            $scope.endDateWrap.setStart(end);
            $scope.EndDate(end);
        }
    };

    ////////used for both types

    //prep advertiser create/edit; upload edit
    $scope.prepareSchedule = function () {
        var isBlank = $scope.activeFormType() === 'advertiser';
        var isEdit = $scope.activeFormMode() === 'edit';
        var schedule = {
            //common to all
            IsBlank: isBlank,
            ScheduleName: $scope.ScheduleName(),
            ISCIs: $scope.IscisBrand(),
            AdvertiserId: $scope.AdvertiserId(),
            PostingBookId: $scope.PostingBookId(),
            MarketRestrictions: $scope.MarketRestrictions(),
            DaypartRestriction: $scope.DaypartRestriction(),
            PostType: $scope.PostTypeId(),
            Equivalized: $scope.Equivalized()
        };

        if (isEdit) {
            schedule.Id = $scope.activeSchedule.Id;
        }
        
        if (isBlank || isEdit) {
            schedule.Audiences = $scope.DemoInputs().map(function(demo, index) {
                return {
                    AudienceId: demo,
                    Rank: index + 1
                }
            });
        }

        //blank advertiser specific
        if (isBlank) {
            schedule.StartDate = $scope.StartDate().format('MM-DD-YYYY'); //convert from moment
            schedule.EndDate = $scope.EndDate().format('MM-DD-YYYY'); //convert from moment
        } else { //upload specific
            schedule.EstimateId = parseInt($scope.EstimateId());
            schedule.InventorySource = $scope.InventorySourceId();
        }
        return schedule;
    };


    //check Iscis - if errorLength then too many on line; check here for valid entries
    $scope.checkIscis = function () {
        var isValid = true, errors = '';
        $.each($scope.IscisBrand(), function (index, item) {
            var msg = 'Error line ' + (index + 1) + ': ';
            var lineValid = true;
            if (!item.House) {
                msg += 'House ISCI cannot be empty; ';
                lineValid = false;
            }

            if (!item.Client) {
                msg += 'Client ISCI cannot be empty; ';
                lineValid = false;
            }

            if (item.errorLength) {
                msg += 'Too many values entered (limit 3);';
                lineValid = false;
            }

            if (!lineValid) {
                errors += (msg + '<br />');
                isValid = false;
            }
        });
        console.log('checkIscis', $scope.IscisBrand(), $('textarea[name=schedule_input_iscis]').val());
        if (isValid) {
            return true;
        } else {
            scheduleValidator.showErrors({
                "schedule_input_iscis": errors
            });
            return false;
        }
    };

    //save Schedule - advertiser create/edit; upload edit
    $scope.saveSchedule = function () {
        var isValid = false;
        //isBlank - check validation with StartEnd
        var isBlank = $scope.activeFormType() === 'advertiser';
        if (isBlank) {
            if (scheduleValidator.form() && $scope.checkStartEnd() && $scope.checkIscis()) {
                isValid = true;
            }
        } else { //upload edit - check only basic validation
            if (scheduleValidator.form() && $scope.checkIscis()) {
                isValid = true;
            }
        }

        if (isValid) {
            var schedule = $scope.prepareSchedule();
            console.log('saveSchedule', JSON.stringify({ Schedule: schedule }));
            controller.apiSaveSchedule(schedule);
        }
    };

    /////////////////

    //reset schedules/clear
    $scope.resetSchedule = function () {
        //shared
        $scope.ScheduleId(null);
        $scope.ScheduleName('');

        $scope.Iscis([]);

        $scope.AdvertiserId('');
        $scope.PostingBookId('');
        $scope.MarketRestrictions([]);
        $scope.DaypartRestriction(null);
        $scope.daypartWrap.remove();//remove the plugin
        $scope.PostTypeId(1);//default 1 NSI
        $scope.Equivalized(true);//default true

        $scope.DemoInputs([]); //depending context 
        //upload
        $scope.FileName('');
        $scope.EstimateId('');
        $scope.InventorySourceId(1); //default 4 Open Market
        //advertiser
        $scope.StartDate('');
        $scope.EndDate('');
        //reset inputs
        $scope.startDateWrap.updateDisplay();
        $scope.endDateWrap.updateDisplay();
        //reset vars
        $scope.activeSchedule = null;
        //temp:
        scheduleValidator.resetForm();
    };

};