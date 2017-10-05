//demo sub items with formatted price; selectedDemo value; active/canDelete for state handling
function DemoItem(canDelete, active) {
    var $scope = this;
    $scope.selectedDemo = ko.observable(null);
    $scope.cpm = ko.observable(null);
    //scope.cpm.extend({ notify: 'always' });//does not work in computed context without subscriber
    $scope.canDelete = ko.observable(canDelete);
    $scope.active = ko.observable(active);
    $scope.formattedPrice = ko.pureComputed({
        read: function () {
            //console.log($scope.cpm());
            if ($scope.cpm()) {
                return numeral($scope.cpm()).format('$0,0[.]00');
            } else {
                return null;
            }
        },

        //revise need to strip completely if not number/float
        //does not evaluate initially as returns null - need to force a chagne
        write: function (value) {
            // Strip out unwanted characters, parse as float, then write the 
            // raw data back to the underlying "cpm" observable
            value = parseFloat(value.replace(/[^\.\d]/g, ""));
            //console.log('write', value);
            //$scope.cpm(isNaN(value) ? null : value); // does not reevaluate if no original value
            //hack: issues at https://github.com/knockout/knockout/issues/1019
            if (isNaN(value)) {
                $scope.cpm(-1);
                $scope.cpm(null);
            } else {
                $scope.cpm(value);
            }
        },
        owner: $scope
    });

};

//1780 todo: 
//handle demoWithFixedOptions, demoOptions form initialData from BE (currently mocked)
//hook up to API whjen BE completes
//upload disabled for testing - renable

//Import Third party: TVB/CNN etc  View Model - modal form

var ImportThirdPartyViewModel = function (controller) {
    //private controller
    var $scope = this;
    var controller = controller; //leave private

    $scope.RateSource = ko.observable();

    /*** FILE FORM RELATED ***/
    $scope.FileName = ko.observable();
    $scope.EffectiveDate = ko.observable();
    //TODO Remove Flights; block name
   // $scope.FlightStartDate = ko.observable();
    //$scope.FlightEndDate = ko.observable();
    //$scope.FlightWeeks = ko.observable();//array
    // $scope.Daypart = ko.observableArray([]);
    //$scope.BlockName = ko.observable();
    //active request from upload manager - not observed //file reader: object{FileName: file.name,  RawData: b64, UserName: "user"}
    $scope.ActiveFileRequest = null;
    $scope.showModal = ko.observable(false);

    $scope.ratingBookOptions = ko.observableArray();
    $scope.selectedRatingBook = ko.observable();

    $scope.playbackTypeOptions = ko.observableArray();
    $scope.selectedPlaybackType = ko.observable();

    //DEMO/CPM TABLE

    $scope.demos = ko.observableArray();
    //test mock
    $scope.demoWithFixedOptions = ko.observableArray([{ Id: 1, Display: 'Fixed' }, { Id: 2, Display: 'A25-55' }, { Id: 3, Display: 'A55-65' }]);
    $scope.demoOptions = ko.observableArray([{ Id: 2, Display: 'A25-55' }, { Id: 3, Display: 'A55-65' }]);
    
    $scope.isDemoFixed = ko.observable(false);
    $scope.allowDemoFixed = ko.observable(true);

    //start initial demo item based on allowDemoFixed
    $scope.initDemoItem = function () {
        var demoItem = new DemoItem(false, true);
        $scope.demos.push(demoItem);
        var parent = $scope;
        //if allwing fixed handle changes
        if ($scope.allowDemoFixed()) {
            demoItem.selectedDemo.subscribe(function (demo) {
                //if demo 1 set isDemoFixed trues - remove any others below
                //console.log('subscribed Demo', demo);
                if (demo === 1) {
                    parent.isDemoFixed(true);
                    //remove all except first !canDelete
                    parent.demos.remove(function (item) {
                        //console.log(item)
                        return item.canDelete();
                    });
                } else {
                    parent.isDemoFixed(false);
                    parent.checkActive();
                }
            });
        } else {
            demoItem.selectedDemo.subscribe(function (demo) {
                 parent.checkActive();
            });
        }
    };

    //add a demo item; subcribe to set active/checkActive
    $scope.addDemo = function () {
        var demoItem = new DemoItem(true, false);
        $scope.demos.push(demoItem);
        var parent = $scope;
        demoItem.selectedDemo.subscribe(function (demo) {
            demoItem.active(true);
            //console.log('demo item select', demoItem);
            parent.checkActive();

        });
    };

    //remove a demo item
    $scope.removeDemo = function (demo) {
        $scope.demos.remove(demo);
    };

    //check active to add additinal inactive demo when needed
    $scope.checkActive = function () {
        var last = $scope.demos()[$scope.demos().length - 1];
        //console.log('check active', last.active());
        if (last.active()) {
            $scope.addDemo();
        }
    };

    //effective date wrapper - to set EffectiveDate

    $scope.effectiveDateWrap = null;

    $scope.initEffectiveDateWrapper = function () {
        $scope.effectiveDateWrap = new wrappers.datePickerSingleWrapper($("#import_thirdparty_effective_date"), $scope.onEffectiveDateChange.bind($scope));
        $scope.effectiveDateWrap.init();
    };

    //callback from wrap picker apply
    $scope.onEffectiveDateChange = function (start) {
        //console.log('onEffectiveDateChange', start);
        $scope.EffectiveDate(start);
    };

   // modal shown event: disable upload dragging
    $scope.onOpenModal = function () {
        controller.view.setUploadDragEnabled(false);
    };

    //modal hidden event: enable upload dragging
    $scope.onHideModal = function () {
        controller.view.setUploadDragEnabled(true);
    };

    //init from initial data api
    //TODO : set demo options
    $scope.initOptions = function (options) {
        $scope.ratingBookOptions(options.RatingBooks);
        $scope.playbackTypeOptions(options.PlaybackTypes);
        $scope.selectedPlaybackType(options.DefaultPlaybackType);
        $scope.initEffectiveDateWrapper();
    };

    //set based on file request - with initial defaults
    //TODO no flights/block name
    $scope.setActiveImport = function (fileRequest, rateSource) {
        $("#import_thirdparty_form").valid();
        $scope.ActiveFileRequest = fileRequest;
        $scope.RateSource(rateSource);
        //console.log('rateSource', $scope.RateSource());
        //set fixed functionaly by source
        $scope.allowDemoFixed(($scope.RateSource() == 'CNN') ? true : false);

        $scope.FileName(fileRequest.FileName);
        //Default start date to today, end date to one month from today. 
        //to get current minus offsets - moment has changed
        //var current = moment().startOf('day').format('MM/DD/YYYY');
        //var future = moment(current).add(1, 'M').format('MM/DD/YYYY');
        //console.log(current, future);
        //$scope.FlightStartDate(current);
        //$scope.FlightEndDate(future);
        //$scope.FlightWeeks([]); //tbd base on start /end?
        //$scope.Daypart([]);//set empty
        //$scope.BlockName($scope.RateSource() + ' NEWS BLOCK');//defaults
        //demo todo set context; clear subscribers?
        $scope.EffectiveDate(null);
        $scope.demos([]);
        $scope.initDemoItem();
        $scope.showModal(true);
        //force apply for flight - so that bound values get updated initially (for save)
        //var flightPicker = $("#import_thirdparty_form input[name='import_thirdparty_flights']").data('daterangepicker');
        //console.log(flightPicker);
        //flightPicker.trigger('apply.daterangepicker');
        //flightPicker.clickApply();

        $scope.effectiveDateWrap.updateDisplay();
        //date picker does not clear previsously validate error class
        $scope.effectiveDateWrap.input.closest('.form-group').removeClass('has-error');
    };

    //upload a file - with form data
    $scope.uploadFile = function () {
        if (controller.view.isThirdPartyValid()) {
            var fileData = $scope.ActiveFileRequest;
            //fileData.FlightWeeks = $scope.FlightWeeks();
            //fileData.FlightStartDate = $scope.FlightStartDate();
            //fileData.FlightEndDate = $scope.FlightEndDate();
            //fileData.BlockName = $scope.BlockName();
            fileData.RatingBook = $scope.selectedRatingBook();
            fileData.PlaybackType = $scope.selectedPlaybackType();
            fileData.EffectiveDate = $scope.EffectiveDate().format('MM-DD-YYYY'); //convert from moment
            var activeDemos = [];
            $scope.demos().forEach(function (item) {
                //only set active items
                if (item.active()) {  
                    var ret = { DemoId: item.selectedDemo(), Price: item.cpm() };
                    activeDemos.push(ret);
                }
            });
            fileData.Demos = activeDemos;
            //console.log('uploadFile', JSON.stringify(fileData));
            console.log('uploadFile', fileData);
            //TODO REENABLE WHEN BE READY
            //controller.apiUploadRateFile(fileData, 
            //    function (data) {
            //        $scope.showModal(false);
            //    }
            //);
        }
    };
};