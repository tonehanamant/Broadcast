//rates Station COntroller - handles Stations and Station Details (Rates/Contacts)
var RateStationController = BaseController.extend({
    appController: null,
    activeStation: null,

    //init the controller; create view, viewModel; get initial data
    initController: function (appController) {
        this.appController = appController;

        // W2UI Grid
        this.view = new RateStationView(this);
        this.view.initView(this);

        // Knockout View Model
        this.viewModel = new RateStationViewModel(this);
        ko.applyBindings(this.viewModel, $('#station_view')[0]);
    },

    getSource: function () {
        return this.appController.activeRateSource;
    },

    //initilaize from selected schedule
    initializeActiveStation: function (station) {
        //console.log('Scrub Controller >> initializeActiveScrub', station);
        this.activeStation = station;
        this.apiGetStation(station.Code);        
    },

    //on view close - refresh the main controller stations maintaining states
    onStationClose: function () {
        this.appController.refreshApiLoadStations(true);
        this.appController.apiGetStationUnlock(this.activeStation.Code, function (unlockResponse) {
            if (!unlockResponse.Success) {
                util.notify("Station could not be unlocked", "danger");
            }
        });
    },

    //API get station details (object with Rates array)
    apiGetStation: function (stationId) {
        var me = this;

        var url = baseUrl + 'api/RatesManager/' + this.getSource() + '/Stations/' + stationId;
        //var queryData = { stationId: stationId };
        httpService.get(url,
            me.onApiGetStation.bind(this),
            me.appController.apiGetStationUnlock.bind(this, stationId, null),
            {
                $ViewElement: $('#rate_view'),
               // data: queryData,
                ErrorMessage: 'Station Data',
                TitleErrorMessage: 'No Station Data Returned',
                StatusMessage: 'Station Data'
            });
    },

    //set the data in view and view model : include isThirdParty
    onApiGetStation: function (data) {
        var thirdParty = this.appController.isThirdParty;
        this.view.setActiveStation(data, thirdParty);
        this.viewModel.setActiveStation(data, thirdParty);
    },

    //delete a Station Rate
    apiDeleteStationRate: function (rec) {
       // console.log('apiDeleteStationRate', rec);
        //BE will need Id, FlightEndDate and FlightStartDate passed to POST
        //on response - remove the record from grid
        //jquery will not pass query as data to a delete
        //var queryData = { startDate: rec.FlightStartDate, endDate: rec.FlightEndDate };
        var queryData = jQuery.param({ startDate: rec.FlightStartDate, endDate: rec.FlightEndDate });
        var url = baseUrl + 'api/RatesManager/Programs/' + rec.Id + '?' + queryData;
        
        httpService.remove(url,
            this.onApiDeleteStationRate.bind(this, rec),
            null,
            {
                $ViewElement: $('#station_rates_view'), //process in context of the inner rates tab view in modal
                //data: queryData,
                ErrorMessage: 'Delete Program',
                TitleErrorMessage: 'No Delete Program Data Returned',
                StatusMessage: 'Delete Program'
            });

    },

    onApiDeleteStationRate: function (rec, data) {
       // console.log('onApiDeleteStationRate', rec, data);
        this.view.removeStationRateFromGrid(rec);
    },

    //API end flight as delete; chnaging to POST - todo move data to body
    apiEndFlightRate: function (programId, endDate, callback) {
        //...api/RatesManager/Programs/2551/Flight?enddate=2016-10-03
        var queryData = jQuery.param({ enddate: endDate });
        var url = baseUrl + 'api/RatesManager/Programs/' + programId + '/Flight?' + queryData;
       
        httpService.post(url,
            this.onApiEndFlightRate.bind(this, callback),
            null,
            {
                $ViewElement: $('#rate_view'),
                ErrorMessage: 'Error End Program Flight ',
                TitleErrorMessage: 'No End Program Flight Data Returned',
                StatusMessage: 'End Program Flight'
            });
    },

    //callback and refresh rates
    onApiEndFlightRate: function (callback, data) {
        //console.log('onApiEndFlightRate', data);
        callback();
        this.refreshApiProgramRates();
    },

    apiUpdateRatesProgram: function (programId, data) {
        var jsonObj = JSON.stringify(data);
        var url = baseUrl + 'api/RatesManager/Programs/' + programId;

        httpService.put(url, this.onApiUpdateRatesProgram.bind(this), null, jsonObj, {
            $ViewElement: $('#update_program_form'), //show processing to form
            ErrorMessage: 'Error Updating Program Rate',
            TitleErrorMessage: 'Program Rate Not Updated',
            StatusMessage: 'Program Rate Updated'
        });
    },

    onApiUpdateRatesProgram: function (data) {
        //console.log('onApiUpdateRatesProgram', data);
        util.notify('Program Updated Successfully');
        this.view.onAfterSaveUpdateRatesProgram();
        this.refreshApiProgramRates();
    },

    // save new program with callback to sub view
    apiSaveNewRatesProgram: function (data, callback) {
        data.RateSource = this.getSource();
        data.StationCode = this.activeStation.Code;
        var jsonObj = JSON.stringify(data);
        //console.log('apiSaveNewRatesProgram', jsonObj);
        var url = baseUrl + 'api/RatesManager/Programs';

        httpService.post(url, this.onApiSaveNewRatesProgram.bind(this, callback), null, jsonObj, {
            $ViewElement: $('#new_program_form'), //show processing to form
            ErrorMessage: 'Error Saving New Program Rate',
            TitleErrorMessage: 'New Program Rate Not Saved',
            StatusMessage: 'New Program Rate Save'
        });
    },

    onApiSaveNewRatesProgram: function (callback, data) {
        //console.log('onApiSaveNewRatesProgram', data);
        //this.view.onAfterSaveNewRatesProgram();
        util.notify('Program Created Successfully');
        callback();
        this.refreshApiProgramRates();
    },

    //pass reset as using filter
    refreshApiProgramRates: function () {
        this.apiFilterPrograms('All', this.activeStation.Code, true);
    },

    //allow reset so can be set to all
    apiFilterPrograms: function (period, stationCode, reset) {
        var me = this,
            url = baseUrl + 'api/RatesManager/' + this.getSource() + '/Stations/' + stationCode + '/Rates',
            //checks if option is within array of possibilities 
            validate = (function(options) {
                return function(option) {
                    return options.indexOf(option) > -1;
                };
            }(['All', 'ThisQuarter', 'LastQuarter', 'Today'])),
            isValid = false,
            dateRange = null;
        //there is not All period
        if (period != 'All') {
            if (typeof period === 'object') {
                dateRange = {
                    startDate: period.start.toISOString(),
                    endDate: period.end.toISOString()
                };

                isValid = true;
            } else if (typeof period === 'string' && validate(period)) {
                url += '/' + period.split(" ").join("");
                isValid = true;
            }
        } else {
            isValid = true
        }

        if (isValid) {
            httpService.get(url, this.onApiFilterPrograms.bind(this, reset), null, {
                $ViewElement: $('#station_modal_view'),
                data: dateRange,
                ErrorMessage: 'Program Period',
                TitleErrorMessage: 'No Program Data Returned at the Selected Period',
                StatusMessage: 'Program Period'
            });
        }
    },

    //pass reset if going back to initial all state
    onApiFilterPrograms: function (reset, data) {
        this.view.updateRatesGrid(data, reset);

    },

    /* CONFLICT */
    // passed stationCode, conflictProgramObj - { StartTime, EndTime, Airtime: {} }
    apiGetProgramConflicts: function (stationCode, conflictProgramObj, callback) {
        conflictProgramObj.RateSource = this.getSource();
        conflictProgramObj.StationCode = stationCode;
        var jsonObj = JSON.stringify(conflictProgramObj);
        var url = baseUrl + 'api/RatesManager/Conflicts'; 

        httpService.post(url, this.onApiGetProgramConflict.bind(this, callback), null, jsonObj, {
            $ViewElement: $('#new_program_form'), //show processing to form
            ErrorMessage: 'Error Getting Program Conflicts',
            TitleErrorMessage: 'Get Program Conflicts Error',
            StatusMessage: 'New Program Conflicts'
        });

    },

    //conflicts response - pass callback to subview - update conflicts
    onApiGetProgramConflict: function (callback, data) {
        //console.log('onApiGetProgramConflict', data);
        //UNCOMMENT after testing
        //this.view.setProgramConflictsGrid(data);
        callback(data);
    },

    //revise POST remove query string, etc
    apiCheckSingleConflict: function (programId, conflictObj, callback) {
        var jsonObj = JSON.stringify(conflictObj);
        var url = baseUrl + 'api/RatesManager/Conflicts/' + programId;

        httpService.post(url, this.onApiCheckSingleConflict.bind(this, programId, callback), null, jsonObj, {
            $ViewElement: $('#new_program_form'), //show processing to form
            ErrorMessage: 'Error Check Program Conflict',
            TitleErrorMessage: 'Check Program Conflict Error',
            StatusMessage: 'Check Program Conflict'
        });
    },

    onApiCheckSingleConflict: function (programId, callback, data) {
       // console.log('onApiCheckSingleConflict', data);
        callback(programId, data);

    },

    //API get contacts only for refesh
    apiRefreshStationContacts: function (stationCode) {
        var url = baseUrl + 'api/RatesManager/' + this.getSource() + '/Stations/' + stationCode + '/Contacts';
        httpService.get(url, this.onApiRefreshStationContacts.bind(this), null,
            {
                $ViewElement: $('#station_contacts_view'),
                ErrorMessage: 'Refresh Station Contacts',
                TitleErrorMessage: 'No Station Contacts Data Returned',
                StatusMessage: 'Refresh Station Contacts'
            });
    },

    //reset contacts after refresh
    onApiRefreshStationContacts: function (data) {
        this.view.setContactsGrid(data);
    },

    apiDeleteStationContact: function (contact) {
        var url = baseUrl + 'api/RatesManager/Contacts/' + contact.Id;    
        
        httpService.remove(url, this.onApiDeleteStationContact.bind(this, contact), null, {
            $ViewElement: $('#station_contacts_view'), //show processing to form
            ErrorMessage: 'Error Deleting Station Contact',
            TitleErrorMessage: 'Error Deleting Station Contact',
            StatusMessage: 'Station Contact Deleted'
        });
    },

    onApiDeleteStationContact: function (rec, data) {
        //console.log('onApiDeleteStationContact', rec, data);
        this.view.removeStationContactFromGrid(rec);
    },

    //add new contact (Id: 0) or edit existing
    apiAddEditStationContact: function (contact, isNew) {
        var url = baseUrl + 'api/RatesManager/Contacts';
        //@VINI - remove this line below when you are ready
        contact.RateSource = this.getSource();

        var jsonObj = JSON.stringify(contact);
        var type = isNew ? 'Create' : 'Edit';
        //send contact info, req body
        httpService.post(url, this.onApiAddEditStationContact.bind(this), null, jsonObj, {
            $ViewElement: $('#station_contacts_view'), //show processing to grid
            ErrorMessage: 'Station Contact ' + type,
            TitleErrorMessage: 'Station Contact ' + type + ' Error',
            StatusMessage: 'Station Contact ' + type
        });
    },

    //after create/update - refresh contacts
    onApiAddEditStationContact: function (data) {
        //refresh grid
       // console.log('onApiAddEditStationContact', data);
        util.notify('Station Contact Edited Successfully');
        this.apiRefreshStationContacts(this.activeStation.Code);
        this.view.onAfterSaveContact();
    },

    apiLoadGenres: function (callback) {
        var url = baseUrl + 'api/RatesManager/Genres';
        httpService.get(url,
           callback.bind(this),
           null,
           {
               //$ViewElement: $('#rate_view'),//this conflicts with save lock if add another
               ErrorMessage: 'Error Loading Genres',
               TitleErrorMessage: 'Error Loading Genres',
               StatusMessage: 'Error Loading Genres'
           });
    },

    //converts 15 spot/rate based on 30 (may have other conversions in future)
    apiConvertRate: function (rate30, callback, isNew) {
        var jsonObj = JSON.stringify({ Rate30: rate30, SpotLength: 15 });
        var url = baseUrl + 'api/RatesManager/ConvertRate';
        var el = isNew ? $('#new_program_form') : $('#update_program_form');
        httpService.post(url, callback.bind(this), null, jsonObj, {
            $ViewElement: el, //show processing to modal based on update/new
            ErrorMessage: 'Convert Rate',
            TitleErrorMessage: 'Convert Rate',
            StatusMessage: 'Convert Rate'
        });
    }
});