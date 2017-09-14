//Import Third party: TVB/CNN etc  View Model - modal form

var ImportThirdPartyViewModel = function (controller) {
    //private controller
    var $scope = this;
    var controller = controller; //leave private

    //tbd to distuinguish types TVB/CNN (display, etc)
    $scope.RateSource = ko.observable();

    /*** FILE FORM RELATED ***/
    $scope.FileName = ko.observable();
    $scope.FlightStartDate = ko.observable();
    $scope.FlightEndDate = ko.observable();
    $scope.FlightWeeks = ko.observable();//array
    // $scope.Daypart = ko.observableArray([]);
    $scope.BlockName = ko.observable();
    //active request from upload manager - not observed //file reader: object{FileName: file.name,  RawData: b64, UserName: "user"}
    $scope.ActiveFileRequest = null;
    $scope.showModal = ko.observable(false);

    $scope.ratingBookOptions = ko.observableArray();
    $scope.selectedRatingBook = ko.observable();

    $scope.playbackTypeOptions = ko.observableArray();
    $scope.selectedPlaybackType = ko.observable();

   // modal shown event: disable upload dragging
    $scope.onOpenModal = function () {
        controller.view.setUploadDragEnabled(false);
    };

    //modal hidden event: enable upload dragging
    $scope.onHideModal = function () {
        controller.view.setUploadDragEnabled(true);
    };

    //set based on file request - with initial defaults
    $scope.setActiveImport = function (fileRequest, rateSource) {
        controller.view.clearThirdPartyValidation();
        $scope.ActiveFileRequest = fileRequest;
        $scope.RateSource(rateSource);
        $scope.FileName(fileRequest.FileName);
        //Default start date to today, end date to one month from today. 
        //to get current minus offsets - moment has changed
        var current = moment().startOf('day').format('MM/DD/YYYY');
        var future = moment(current).add(1, 'M').format('MM/DD/YYYY');
        //console.log(current, future);
        $scope.FlightStartDate(current);
        $scope.FlightEndDate(future);
        $scope.FlightWeeks([]); //tbd base on start /end?
        //$scope.Daypart([]);//set empty
        $scope.BlockName($scope.RateSource() + ' NEWS BLOCK');//defaults
        $scope.showModal(true);
        //force apply for flight - so that bound values get updated initially (for save)
        var flightPicker = $("#import_thirdparty_form input[name='import_thirdparty_flights']").data('daterangepicker');
        //console.log(flightPicker);
        //flightPicker.trigger('apply.daterangepicker');
        flightPicker.clickApply();
    };

    $scope.uploadFile = function () {
        if (controller.view.isThirdPartyValid()) {
            var file = $scope.ActiveFileRequest;
            file.FlightWeeks = $scope.FlightWeeks();
            file.FlightStartDate = $scope.FlightStartDate();
            file.FlightEndDate = $scope.FlightEndDate();
            file.BlockName = $scope.BlockName();
            file.RatingBook = $scope.selectedRatingBook();
            file.PlaybackType = $scope.selectedPlaybackType();
            
            controller.apiUploadRateFile(file, 
                function (data) {
                    $scope.showModal(false);
                }
            );
        }
    };
};