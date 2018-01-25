//Main Rate Controller - 
//tbd inheritance from base; use of separate view/controller structures
var RateMainController = BaseController.extend({
    view: null,
    thirdPartyViewModel: null,
    stationController: null,
    stationsData: null,
    activeRateSource: 'OpenMarket',  //active source for loading/uploading rates
    activeStationsDataFilter: null, //store the active filter for refresh (with state) after edit

    isThirdParty: false, //on tab change in main view set this property so other sections can use

    //handle open market imports as queue and aggregate errors
    importFileQueue: null,
    importFileQueueOriginalLength: 0,
    importFileErrors: [], // {fileName, message, problems}

    //init the controller; create view, get initial data
    initController: function () {
        this.view = new RateMainView(this);
        this.view.initView(this);
        this.apiLoadStations();
        //TVB view model for TVB imports
        this.thirdPartyViewModel = new ImportThirdPartyViewModel(this);
        ko.applyBindings(this.thirdPartyViewModel, document.getElementById("import_thirdparty_modal"));

    },

    //load stations data - filter (optional filter), set source (rate source), 
    apiLoadStations: function (filter, source) {
        source = source || this.activeRateSource;

        var data = null,
            url = baseUrl + 'api/RatesManager/' + source + '/Stations',
            validate = (function (options) {
                return function (option) {
                    return options.indexOf(option) > -1;
                };
            }(['WithTodaysData', 'WithoutTodaysData']));

        if (validate(filter)) url += '?filter=' + filter;

        this.activeRateSource = source;
        this.activeStationsDataFilter = filter ? filter : null;

        this.isThirdParty = (source != 'OpenMarket') && (source != 'Assembly');
        //console.log('isThirdParty', source, this.isThirdParty);

        httpService.get(url,
            this.onApiLoadStations.bind(this),
            this.onApiLoadStationsError.bind(this),
            {
                data: data,
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Load Stations',
                TitleErrorMessage: 'No Stations Data Returned',
                StatusMessage: 'Load Stations'
            });
    },

    //handle stations data return - view may need to know refresh for states
    onApiLoadStations: function (data) {
        this.stationsData = data;
        var copyData = util.copyData(data, null, null, true);
        this.view.setStations(copyData, this.activeStationsDataFilter);
    },

    //clear data in view else data will remain in old state - via rate Source
    onApiLoadStationsError: function () {
        this.view.setStations(null);
    },

    //refresh stations
    //called from StationController after close modal - mainatain states; OR after Upload - reset States
    refreshApiLoadStations: function (withFilter) {
        var filter = (withFilter && this.activeStationsDataFilter) ? this.activeStationsDataFilter : false;
        this.apiLoadStations(filter);
        if (!withFilter) this.view.StationsTextSearch = null;//reset state to clear text search
    },

    //REVISED from upload manager to view process multiple files
    apiQueueInventoryFiles: function (fileRequests) {
        var queue = [];
        //this.importFileQueue.length = fileRequests.length;
        $.each(fileRequests, function (idx, req) {
            //console.log('req', req, idx);
            queue.push(req.FileName);
        });
        this.importFileQueue = queue;
        //console.log(queue, fileRequests, this.importFileQueue);
        this.importFileQueueOriginalLength = fileRequests.length;
        var $scope = this;
        $.each(fileRequests, function (index, rateFileRequest) {
            $scope.apiUploadInventoryFile(rateFileRequest);
        });
    },

    //upload Rate file
    //REVISING to queue uploads and handle aggregated errors/ success - remove callback?
    apiUploadInventoryFile: function (rateFileRequest, callback) {
        var url = baseUrl + 'api/RatesManager/UploadInventoryFile';
        var jsonObj = JSON.stringify(rateFileRequest);
        httpService.post(url,
            this.onApiUploadInventoryFile.bind(this, callback),
            this.onApiUploadInventoryFileErrorProblems.bind(this, rateFileRequest.FileName),  //add the fileName
            jsonObj,
            {
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Error Uploading Rate File',
                TitleErrorMessage: 'Rate File Not Uploaded',
                StatusMessage: 'Upload Rate File',
                bypassErrorShow: true //test bypass
            });
    },

    checkImportFileQueue: function () {
        //console.log('check queue', this.importFileQueue);
        this.importFileQueue.pop();
        if (this.importFileQueue.length == 0) {
            return true;
        } else {
            return false;
        }
    },

    //handdle aggregated erros if needed and success responses to notify
    resolveFileImportsComplete: function () {
        var notifyType = 'success';
        var notifyMessage = 'Rate File(s) Uploaded';
        var hasErrors = this.importFileErrors.length > 0;
        var allFailed = false;
        if (hasErrors) {
            //aggregate and show
            //resolve notify if some success
            allFailed = this.importFileQueueOriginalLength == this.importFileErrors.length;
            notifyType = allFailed ? 'danger' : 'warning';
            notifyMessage = allFailed ? 'Error Uploading Rate File(s)' : 'Error Uploading Some Rate File(s)';
            this.view.showUploadFileIssues(this.importFileErrors, this.importFileQueueOriginalLength);
        }
        util.notify(notifyMessage, notifyType);
        this.importFileErrors = [];
        this.importFileQueue = null;
        this.importFileQueueOriginalLength = 0;
        if (!hasErrors || !allFailed) this.refreshApiLoadStations(false);
    },

    //handle Rate upload return; show problems in view if present
    //CHANGE: handle problems as error status false - intercept as error - view will handle modal display text
    //REVISE - queued
    onApiUploadInventoryFile: function (callback, data) {
        if (callback) callback(data);
        var check = this.checkImportFileQueue();
        if (check) this.resolveFileImportsComplete();
        //util.notify("Rate File Uploaded");
        //this.refreshApiLoadStations(false);
    },

    //error callback if Problems or NOW handle all errors - response.Problems or response as message with file name bind
    //REVISE queued
    onApiUploadInventoryFileErrorProblems: function (fileName, xhr, response) {
        var errorItem = { fileName: fileName, message: response, problems: null };
        var hasProblems = response && response.Problems && response.Problems.length;
        if (hasProblems) {
            errorItem.message = null;
            errorItem.problems = response.Problems;
        }
        this.importFileErrors.push(errorItem);
        var check = this.checkImportFileQueue();
        if (check) this.resolveFileImportsComplete();
        //console.log('onApiUploadInventoryFileErrorProblems', check, fileName, response, xhr);
    },

    //TBD - add this in when creating Station Specifics C/V/VM
    //Station detail Initialization
    //from view select rate for station
    onStationRecordSelect: function (rec) {
        if (!this.stationController) {
            this.stationController = new RateStationController();
            this.stationController.initController(this);
        }
        this.stationController.initializeActiveStation(rec); //pass schedule record
    },

    apiGetStationLock: function (stationCode, callback) {
        var url = baseUrl + 'api/RatesManager/Stations/' + stationCode + '/Lock';

        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Stations Lock',
                TitleErrorMessage: 'Stations Lock',
                StatusMessage: 'Stations Lock'
            });
    },

    apiGetStationUnlock: function (stationCode, callback) {
        var url = baseUrl + 'api/RatesManager/Stations/' + stationCode + '/UnLock';

        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Stations Lock',
                TitleErrorMessage: 'Stations Lock',
                StatusMessage: 'Stations Lock'
            });
    },

    apiGetFileImportOptions: function (callback) {
        var url = baseUrl + 'api/RatesManager/InitialData';

        httpService.get(url,
           callback ? callback.bind(this) : null,
           null,
           {
               $ViewElement: $('#rate_view'),
               ErrorMessage: 'File Import Options',
               TitleErrorMessage: 'File Import Options',
               StatusMessage: 'File Import Options'
           });
    }
});