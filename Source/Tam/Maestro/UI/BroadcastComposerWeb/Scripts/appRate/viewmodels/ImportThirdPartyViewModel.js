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

//Import Third party: TVB/CNN etc  View Model - modal form

var ImportThirdPartyViewModel = function (controller) {
    //private controller
    var $scope = this;
    var controller = controller; //leave private

    $scope.InventorySource = ko.observable();

    /*** FILE FORM RELATED ***/
    $scope.FileName = ko.observable();
    $scope.EffectiveDate = ko.observable();
    //active request from upload manager - not observed //file reader: object{FileName: file.name,  RawData: b64, UserName: "user"}
    $scope.ActiveFileRequest = null;
    $scope.showModal = ko.observable(false);

    $scope.ratingBookOptions = ko.observableArray();
    $scope.selectedRatingBook = ko.observable();

    $scope.playbackTypeOptions = ko.observableArray();
    $scope.selectedPlaybackType = ko.observable();

    //store variables for resetting - not observables
    $scope.defaultPlaybackType = null;
    $scope.defaultRatingBook = null;

    //DEMO/CPM TABLE

    $scope.demos = ko.observableArray();
    //test mock
    //$scope.demoWithFixedOptions = ko.observableArray([{ Id: 1, Display: 'Fixed' }, { Id: 2, Display: 'A25-55' }, { Id: 3, Display: 'A55-65' }]);
    //$scope.demoOptions = ko.observableArray([{ Id: 2, Display: 'A25-55' }, { Id: 3, Display: 'A55-65' }]);
    $scope.demoWithFixedOptions = ko.observableArray();
    $scope.demoOptions = ko.observableArray();
    
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
                //if demo 'Fixed' set isDemoFixed trues - remove any others below
                //console.log('subscribed Demo', demo);
                if (demo == 'Fixed') {
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
    $scope.initOptions = function (options) {
        $scope.ratingBookOptions(options.RatingBooks);
        $scope.playbackTypeOptions(options.PlaybackTypes);
        //problematic as only set once here - not on subsequent activations
        //store the defaults for defaultPlaybackType and defaultRatingBook (use first) to use when activate
        //$scope.selectedPlaybackType(options.DefaultPlaybackType);
        $scope.defaultPlaybackType = options.DefaultPlaybackType;
        $scope.defaultRatingBook = options.RatingBooks[0].Id;
        //sort alphabetically
        options.Audiences = _.sortBy(options.Audiences, ['Display']);
        $scope.demoOptions(options.Audiences);
        var fixedDemos = options.Audiences.slice(0);
        //add fixed option to (resolve to FixedPrice if applicable)
        fixedDemos.unshift({ Id: 'Fixed', Display: 'Fixed' });
        $scope.demoWithFixedOptions(fixedDemos);
        $scope.initEffectiveDateWrapper();
    };

    //set based on file request - with initial defaults
    $scope.setActiveImport = function (fileRequest, inventorySource) {
        $("#import_thirdparty_form").valid();
        $scope.ActiveFileRequest = fileRequest;
        $scope.InventorySource(inventorySource);
        //set fixed functionaly by source
        $scope.allowDemoFixed(($scope.InventorySource() == 'CNN') ? true : false);

        $scope.FileName(fileRequest.FileName);

        //reset these here per stored defaults
        $scope.selectedPlaybackType($scope.defaultPlaybackType);
        $scope.selectedRatingBook($scope.defaultRatingBook);

        $scope.EffectiveDate(null);
        $scope.demos([]);
        $scope.initDemoItem();
        $scope.showModal(true);
        //set to first monday from today
        var today = moment();
        var startDate = today.day() > 0 ? today.add(1, "week").startOf('week').weekday(1) : today.startOf('week').weekday(1);
        $scope.effectiveDateWrap.setStart(startDate);
        $scope.EffectiveDate(startDate);
        //date picker does not clear previsously validate error class
        $scope.effectiveDateWrap.input.closest('.form-group').removeClass('has-error');
    };

    //upload a file - with form data
    $scope.uploadFile = function () {
        if (controller.view.isThirdPartyValid()) {
            var fileData = $scope.ActiveFileRequest;
            fileData.RatingBook = $scope.selectedRatingBook();
            fileData.PlaybackType = $scope.selectedPlaybackType();
            fileData.EffectiveDate = $scope.EffectiveDate().format('MM-DD-YYYY'); //convert from moment
            //if Fixed allowed and set then use FixedPrice: price with empty array for AudiencePricing
            var activeDemos = [];
            var fixed = null;
            if ($scope.allowDemoFixed && ($scope.demos()[0].selectedDemo() == 'Fixed')) {
                fixed = $scope.demos()[0].cpm();
            } else {
                $scope.demos().forEach(function (item) {
                    //only set active items
                    if (item.active()) {
                        var ret = { AudienceId: item.selectedDemo(), Price: item.cpm() };
                        activeDemos.push(ret);
                    }
                });
            }
            fileData.AudiencePricing = activeDemos;
            fileData.FixedPrice = fixed;
            //console.log('uploadFile', JSON.stringify(fileData));
            console.log('uploadFile', fileData);
            controller.apiUploadInventoryFile(fileData,
                function (data) {
                    $scope.showModal(false);
                }
            );
        }
    };
};