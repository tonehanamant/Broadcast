//Tracker Scrubbing Controller - 
//tbd inheritance from base; use of separate view/controller structures
var TrackerScrubController = BaseController.extend({
    appController: null,
    scrubData: null,
    scrubComponents: null,
    activeEstimateId: null,
    activeSchedule: null, //store activeSchedule record from TrackerMainController (i.e. the Id for schedule)

    //init the controller; create view, viewModel; get initial data
    initController: function(appController) {
        this.appController = appController;

        // W2UI Grid
        this.view = new TrackerScrubView(this);
        this.view.initView(this);

        // Knockout View Model
        this.viewModel = new TrackerScrubViewModel(this);
        ko.applyBindings(this.viewModel, $('#scrub_view')[0]);
    },

    //initilaize from selected schedule
    initializeActiveScrub: function(schedule) {
        //console.log('Scrub Controller >> initializeActiveScrub', schedule);
        this.apiGetScrub(schedule.Estimate, false);
        this.activeSchedule = schedule;
    },

    apiGetScrub: function(estimateId, refreshOnly) {
        var url = baseUrl + 'api/Tracker/GetBvsScrubbingData';
        var queryData = { estimateId: estimateId };
        httpService.get(url,
            this.onApiGetScrub.bind(this, refreshOnly),
            null,
            {
                $ViewElement: $('#schedule_view'),
                data: queryData,
                ErrorMessage: 'Scrubbing Data',
                TitleErrorMessage: 'No Scrubbing Data Returned',
                StatusMessage: 'Scrubbing Data'
            });
    },

    onApiGetScrub: function (refreshOnly, data) {

        this.activeEstimateId = data.EstimateId;
        this.scrubData = data;
        this.view.setActiveScrub(data, refreshOnly);
        this.viewModel.setActiveScrub(data);
        this.view.scrollToActiveItemIndex();
    },

    //updates schedule entry (after mapping)
    apiUpdateScrubbed: function(mappingsToSave) {
        var recIds = mappingsToSave.DetailIds;
        this.view.lockScrubGridMultipleRows(recIds);

        var url = baseUrl + 'api/Tracker/SaveScrubbingMappings';
        var jsonObj = JSON.stringify(mappingsToSave);

        httpService.post(url, this.onApiUpdateScrubbed.bind(this, recIds), this.onApiUpdateScrubbedError.bind(this, recIds), jsonObj, {
            ErrorMessage: 'Error Saving Scrubbing Mappings',
            TitleErrorMessage: 'Scrubbing Mappings Not Updated',
            StatusMessage: 'Save Scrubbing Mappings'
        });
    },

    // scrubbing save success callback - remove locks, update grid rows; viewModel process save
    onApiUpdateScrubbed: function(recIds, data) {
        util.notify('Mappings Saved Successfully');

        this.view.unlockUpdateScrubGridMultipleRows(recIds, data);
        this.refreshScrubAfterAction();
    },

    //scrubbing save error callback - remove locks
    onApiUpdateScrubbedError: function(recIds, xhr) {
        //unlock rows
        this.view.unlockUpdateScrubGridMultipleRows(recIds);
        util.notify('Mappings Save Error', 'danger');
    },

    //accept a mapping as a block
    apiAcceptAsBlock: function (data, callback) {
        var url = baseUrl + 'api/Tracker/AcceptScheduleBlock';
        var jsonObj = JSON.stringify(data);

        httpService.post(url, this.onApiAcceptAsBlock.bind(this, callback), null, jsonObj, {
            ErrorMessage: 'Error Saving Block Acceptance',
            TitleErrorMessage: 'Block Acceptance Not Saved',
            StatusMessage: 'Save Block Acceptance'
        });
    },

    // callback for apiAcceptAsBlock
    onApiAcceptAsBlock: function (callback) {
        util.notify('Updated as block successfully');
        callback();
        this.refreshScrubAfterAction();
    },

    // replaces current bvs entry program with one of its program lead in options
    apiAcceptScheduleLeadin: function (acceptScheduleLeadinRequest) {
        var url = baseUrl + 'api/Tracker/AcceptScheduleLeadin';
        var jsonObj = JSON.stringify(acceptScheduleLeadinRequest);

        httpService.post(url, this.onApiAcceptScheduleLeadin.bind(this), null, jsonObj, {
            ErrorMessage: 'Error Verifying Lead In',
            TitleErrorMessage: 'Error Verifying Lead In',
            StatusMessage: 'Verifying Lead In'
        });
    },

    // callback for apiAcceptScheduleLeadin
    onApiAcceptScheduleLeadin: function () {
        util.notify('Lead in successfully verified');
        this.refreshScrubAfterAction();
    },

    //GET schedule program that maps to program for program display
    //pass Id from the record as bvsDetailId
    apiGetScheduleProgram: function (detailRec) {
        var url = baseUrl + 'api/Tracker/GetProgramMapping';
        var queryData = { bvsDetailId: detailRec.Id };

        httpService.get(url,
            this.onApiGetScheduleProgram.bind(this, detailRec),
            null,
            {
                $ViewElement: $('#scrub_modal_view'),//show processing on modal
                data: queryData,
                ErrorMessage: 'Scheduled Program',
                TitleErrorMessage: 'No Scheduled Program Data Returned',
                StatusMessage: 'Scheduled Program'
            });
    },

    //response: scheduled program (STRING) used to map - re-call the view model openMappingModal with return scheduled program and the original record
    onApiGetScheduleProgram: function (detailRec, data) {
        this.viewModel.openMappingModal(detailRec, data);
    },

    // Updates Schedule
    apiUpdateSchedule: function(updatedSchedule, closeModal) {
        var url = baseUrl + 'api/Tracker/ScrubSchedule';
        var jsonObj = JSON.stringify(updatedSchedule);

        httpService.post(url, this.onApiUpdateSchedule.bind(this, updatedSchedule.EstimateId, closeModal), null, jsonObj, {
            $ViewElement: $('#scrub_modal_view'),//show processing on modal
            ErrorMessage: 'Error Updating Schedule',
            TitleErrorMessage: 'Schedule Not Updated',
            StatusMessage: 'Schedule Updated'
        });
    },

    onApiUpdateSchedule: function (estimateId, closeModal) {
        if (closeModal) {
            this.view.$ScrubModal.modal('hide');
        } else {
            // update modal -- get updated data and refresh scrubbing grid
            this.apiGetScrub(estimateId, true);
        }

        //refresh the main API
        //CHANGE this to always refresh main on modal hidden event so that other items like mappings are reflected in the main grid
        //this.appController.refreshApiLoadSchedules();
        util.notify('Schedule Updated Successfully');
    },

    //refresh the entire grid in situations where data returned does not allow refresh of row only
    refreshScrubAfterAction: function () {
        this.apiGetScrub(this.activeEstimateId, true);
    },

    //called from view to refresh following scrubbing
    refreshSchedulesAfterSaveCancel: function () {
        this.appController.refreshApiLoadSchedules();
    }
});