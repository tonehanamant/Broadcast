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
    onApiLoadStationsError: function() {
        this.view.setStations(null);
    },

    //refresh stations
    //called from StationController after close modal - mainatain states; OR after Upload - reset States
    refreshApiLoadStations: function (withFilter) {
        var filter = (withFilter && this.activeStationsDataFilter) ? this.activeStationsDataFilter : false;
        this.apiLoadStations(filter);
        if(!withFilter) this.view.StationsTextSearch = null;//reset state to clear text search
    },

    //upload Rate file
    apiUploadInventoryFile: function (rateFileRequest, callback) {
        var url = baseUrl + 'api/RatesManager/UploadInventoryFile';
        var jsonObj = JSON.stringify(rateFileRequest);
        httpService.post(url,
            this.onApiUploadInventoryFile.bind(this, callback),
            null,
            jsonObj,
            {
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Error Uploading Rate File',
                TitleErrorMessage: 'Rate File Not Uploaded',
                StatusMessage: 'Upload Rate File'
            });
    },

    //handle Rate upload return; show problems in view if present
    onApiUploadInventoryFile: function (callback, data) {
        var hasProblems = data.Problems && data.Problems.length;
        if (hasProblems) {
            this.view.showUploadFileIssues(data.Problems);
        }
        //reset states
        if (callback) callback(data);
        if (!hasProblems) {
            util.notify("Rate File Uploaded");
        }
        this.refreshApiLoadStations(false);
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

    apiGetFileImportOptions: function(callback) {
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