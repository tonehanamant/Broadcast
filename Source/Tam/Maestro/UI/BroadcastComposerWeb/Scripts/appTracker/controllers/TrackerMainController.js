
//Main Tracker Controller - 
//tbd inheritance from base; use of separate view/controller structures


//Revise - schedule files (SCX, CSV) are now single - no array : remove queue - check one, upload one
var TrackerMainController = BaseController.extend({
    view: null,
    viewModel: null,
    scrubController: null,
    scheduleData: null,
    scheduleComponents: null,
    activeQuarterId: null,
    activeQuarter: null,


    //init the controller; create view, viewModel; get initial data
    initController: function () {
        this.viewModel = new TrackerScheduleViewModel(this);
        ko.applyBindings(this.viewModel, document.getElementById('schedule_modal_view'));

        this.view = new TrackerMainView(this);
        this.view.initView(this);
        this.apiGetInitialData();
    },

    //get reussable data components
    apiGetInitialData: function () {
        var url = baseUrl + 'api/Tracker/GetInitialData';
        httpService.get(url,
            this.onApiGetInitialData.bind(this),
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Initial Data',
                TitleErrorMessage: 'No Initial Data Returned',
                StatusMessage: 'Initial Data'
            });
    },

    //get current quarter; store components; call load api
    onApiGetInitialData: function (data) {
        this.scheduleComponents = data;
        if (data && data.CurrentQuarter) {
            this.viewModel.initComponents(this.scheduleComponents);
            this.apiLoadSchedules(data.CurrentQuarter.StartDate, data.CurrentQuarter.EndDate, true);
            this.activeQuarterId = data.CurrentQuarter.Id;
            this.activeQuarter = data.CurrentQuarter;
        } else {
            //load with all or error?
        }
    },

    //filter data by quarter: called from view quarter change - can be 'all'
    //change: if "all" - set activeQuarter and activeQuarterId to null - so sends no params when refresh schedules later
    setScheduleByQuarterFilter: function (quarterId) {
        if (!quarterId || quarterId === 'all') {
            this.activeQuarterId = null;
            this.activeQuarter = null;
            return this.apiLoadSchedules();
        }
        //test for already active? set active quarter (and reset last on error)?
        var qtr = util.objectFindByKey(this.scheduleComponents.Quarters, 'Id', parseInt(quarterId));
        //console.log('setScheduleByQuarterFilter', qtr);
        if (qtr) {
            this.apiLoadSchedules(qtr.StartDate, qtr.EndDate);
            //todo may need to reset on API error
            this.activeQuarterId = quarterId;
            this.activeQuarter = qtr;
        }
    },

    //load schedules data
    apiLoadSchedules: function (startDate, endDate, initial) {
        var $scope = this;

        initial = initial || false;
        var url = baseUrl + 'api/Tracker/LoadSchedules';
        var queryData = {};
        if (startDate) {
            queryData.startDate = startDate;
            if (endDate) {
                queryData.endDate = endDate;
            }
        }

        httpService.get(url,
            this.onApiLoadSchedules.bind(this, initial),
            null,
            {
                data: queryData,
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Load Schedules',
                TitleErrorMessage: 'No Schedule Data Returned',
                StatusMessage: 'Load Schedules'
            });
    },

    //handle schedules data return
    onApiLoadSchedules: function (initial, data) {
        this.scheduleData = data;
        var copyData = util.copyData(data, null, null, true);
        //this.view.setScheduleGrid(data);
        var components = initial ? util.copyData(this.scheduleComponents, null, null, true) : null;
        this.view.setSchedule(copyData, components);
    },

    //refresh schedules
    refreshApiLoadSchedules: function () {
        if (this.activeQuarter) {
            this.apiLoadSchedules(this.activeQuarter.StartDate, this.activeQuarter.EndDate, false);
        } else {
            this.apiLoadSchedules();
        }
    },
    //apiUploadFtp : function() {
    //    //console.log('inside the function');
    //    var url = baseUrl + 'api/Tracker/UploadBvsFtp';
    //    httpService.post(url,
    //        this.onApiUploadFtp.bind(this),
    //        this.onApiUploadFtpError.bind(this),
    //        null,
    //        {
    //            ErrorMessage: 'Error with FTP Process',
    //            TitleErrorMessage: 'FTP Error',
    //            StatusMessage: 'FTP Process'
    //        });
    //},

    //onApiUploadFtpError: function (data) {
    //    $('#ftpUploadButton').removeAttr("disabled");
    //},

    //onApiUploadFtp: function (data) {
    //    this.view.displayFtpMessages(data);
    //    $('#ftpUploadButton').removeAttr("disabled");
    //},

    //upload SIGMA file
    apiUploadSigmaFile: function (sigmaFileRequest) {
        var url = baseUrl + 'api/Tracker/UploadSigmaFile ';
        var jsonObj = JSON.stringify(sigmaFileRequest);
        //console.log('apiupload sigma', jsonObj);
        httpService.post(url,
            this.onApiUploadSigmaFile.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Error Uploading File',
                TitleErrorMessage: 'File Not Uploaded',
                StatusMessage: 'Upload Sigma File'
            });
    },

    //handle Sigma upload return
    onApiUploadSigmaFile: function (data) {
        util.notify("Sigma File Uploaded");
        this.refreshApiLoadSchedules();
    },

    //upload BVS file
    apiUploadBVSFile: function (bvsFileRequest) {
        var url = baseUrl + 'api/Tracker/UploadBvsFile';
        var jsonObj = JSON.stringify(bvsFileRequest);
        httpService.post(url,
            this.onApiUploadBVSFile.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Error Uploading File',
                TitleErrorMessage: 'File Not Uploaded',
                StatusMessage: 'Upload BVS File'
            });
    },

    //handle BVS upload return
    onApiUploadBVSFile: function (data) {
        util.notify("BVS File Uploaded");
        this.refreshApiLoadSchedules();
    },

    //SCHEDULE related 

    apiGetScheduleHeader: function (estimateId, callback) {
        var url = baseUrl + 'api/Tracker/ScheduleHeader/' + estimateId;

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#schedule_modal_view'),
                ErrorMessage: 'Schedule Header Data',
                TitleErrorMessage: 'No Schedule Header Data Returned',
                StatusMessage: 'Schedule Header Data'
            });
    },

    apiCheckUploadSchedule: function (schedule) {
        var me = this;
        var estimateId = schedule.EstimateId;
        var url = baseUrl + 'api/Tracker/ScheduleExists';
        httpService.get(url,
            function (exists) {
                if (exists) {
                    me.viewModel.showSchedulesWarning(schedule);
                } else {
                    me.apiUploadSchedule(schedule);
                }
            },
            null,
            {
                $ViewElement: $('#schedule_modal_view'),//use the modal view so processing over 
                data: { estimateId: estimateId },
                ErrorMessage: 'Check Schedule Exists',
                TitleErrorMessage: 'No Check Schedule Returned',
                StatusMessage: 'Check Schedule Exists'
            });

    },


    //schedule upload
    apiUploadSchedule: function (schedule) {
        var url = baseUrl + 'api/Tracker/UploadScheduleFile';
        var jsonObj = JSON.stringify({ Schedule: schedule });

        httpService.post(url, this.onApiUploadSchedule.bind(this), null, jsonObj, {
            $ViewElement: $('#schedule_modal_view'),//use the modal view so processing over 
            ErrorMessage: 'Error Uploading Schedule File',
            TitleErrorMessage: 'Schedule File Not Uploaded',
            StatusMessage: 'Upload Schedule File'
        });
    },

    //handle schedules upload return
    onApiUploadSchedule: function (data) {
        //reset view
        this.view.onAfterScheduleUploadSave(); //close modal
        util.notify("Schedule File Uploaded");
        this.refreshApiLoadSchedules();
    },

    //add/edit blank schedule
    apiSaveSchedule: function (schedule) {
        var url = baseUrl + 'api/Tracker/UploadScheduleFile';
        var jsonObj = JSON.stringify({ Schedule: schedule });

        httpService.post(url, this.onApiSaveSchedule.bind(this), null, jsonObj, {
            $ViewElement: $('#schedule_modal_view'),//use the modal view so processing over 
            ErrorMessage: 'Save Schedule',
            TitleErrorMessage: 'Schedule Not Saved',
            StatusMessage: 'Save Schedule'
        });
    },

    //handle add schedule add return
    onApiSaveSchedule: function (data) {
        //reset view
        this.view.onAfterScheduleUploadSave(); //close modal
        util.notify("Schedule Saved");
        this.refreshApiLoadSchedules();
    },

    // get existing schedule data by scheduleId
    //determine by type
    apiGetSchedule: function (scheduleId, type) {
        var url = baseUrl + 'api/Tracker/GetSchedule/' + scheduleId;
        httpService.get(url,
            //pass ScheduleId?
            this.onApiGetSchedule.bind(this, type),
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Get Schedule Data',
                TitleErrorMessage: 'No Get Schedule Data Returned',
                StatusMessage: 'Get Schedule  Data'
            });
    },

    //tbd: use type??
    onApiGetSchedule: function (type, schedule) {
        //may need to pass ScheduleId if not in BE
        //schedule.ScheduleId = scheduleId;
        this.view.setScheduleModal(type, schedule, true);
    },

    //SCRUB Initialization
    //from view select schedule for scrub
    onScrubRecordSelect: function (rec) {
        if (!this.scrubController) {
            this.scrubController = new TrackerScrubController();
            this.scrubController.initController(this);
        }
        this.scrubController.initializeActiveScrub(rec); //pass schedule record
    },

    //REPORTS

    downloadScheduleReport: function (scheduleId) {
        var url = baseUrl + 'api/Tracker/ScheduleReport/' + scheduleId;
        console.log(url);
        window.open(url);
    },

    downloadClientReport: function (scheduleId) {
        var url = baseUrl + 'api/Tracker/ClientReport/' + scheduleId;
        window.open(url);
    },

    downloadProviderReport: function (scheduleId) {
        var url = baseUrl + 'api/Tracker/ProviderReport/' + scheduleId;
        window.open(url);
    },

    //MAPPINGS

    //get all data for mappings - program, station
    //call both apis and return to view if both success
    apiManageMappings: function () {
        var $scope = this;
        var url = baseUrl + 'api/Tracker/Mappings/Program';
        //program
        httpService.get(url,
            function (program) {
                var url = baseUrl + 'api/Tracker/Mappings/Station';
                //Station
                httpService.get(url,
                    $scope.onApiManageMappings.bind($scope, program),
                    null,
                     {
                         $ViewElement: $('#schedule_view'),
                         ErrorMessage: 'Mappings Station Data',
                         TitleErrorMessage: 'No Mappings Station Data Returned',
                         StatusMessage: 'Mappings Station Data'
                     });
            },
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Mappings Program Data',
                TitleErrorMessage: 'No Mappings Program Data Returned',
                StatusMessage: 'Mappings Program Data'
            });
    },

    onApiManageMappings: function (program, station) {
        //console.log('onApiManageMappings', program, station);
        this.view.onMappingsSet(program.TrackingMapValues, station.TrackingMapValues);
    },

    //Uses POST - delete mapping by type (Program, Station) - callback from TrackerManageMappings
    //body: schedule and bvs vals from rec
    apiDeleteMapping: function (type, rec, callback) {
        //url and params; callback
        var jsonObj = JSON.stringify({ BvsValue: rec.BvsValue || '', ScheduleValue: rec.ScheduleValue || '' });
        var url = baseUrl + 'api/Tracker/Mappings/' + type + '/Delete';
        httpService.post(url,
            callback.bind(this),//needs to pass back id and type (todo: check callee)
            null,
            jsonObj,
            {
                $ViewElement: $('#manage_mappings_modal_view'), //process in context of modal or tab?
                ErrorMessage: 'Delete Mapping',
                TitleErrorMessage: 'No Delete Mapping Data Returned',
                StatusMessage: 'Delete Mapping'
            });
    },

    //RATINGS BOOKS

    //load ratings books with master list of weeks
    apiManageRatingsBooks: function () {
        var $scope = this;
        var url = baseUrl + 'api/Tracker/RatingAdjustments';
        httpService.get(url,
            function (ratingsData) {
                $scope.view.onRatingsSet(ratingsData);
                //console.log('apiManageRatingsBooks', ratingsData);
            },
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Ratings Books',
                TitleErrorMessage: 'No Ratings Books Data Returned',
                StatusMessage: 'Ratings Books'
            }
        );
    },

    //save ratings books
    apiSaveRatingsBooks: function (ratings, callback) {
        var jsonObj = JSON.stringify(ratings);
        var $scope = this;
        var url = baseUrl + 'api/Tracker/RatingAdjustments';
        httpService.post(url, callback.bind(this), null, jsonObj,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Ratings Books Save',
                TitleErrorMessage: 'No Ratings Books Save Data Returned',
                StatusMessage: 'Ratings Books Save'
            }
        );
    },

    //BVS Files

    //call the API to load the list of files
    apiBvsFileListing: function () {
        var self = this;
        var url = baseUrl + 'api/tracker/BvsFileSummaries';

        httpService.get(url, function (bvsFilesData) {
            self.view.onBvsFileListingSet(bvsFilesData);
        },
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'BVS Files',
                TitleErrorMessage: 'No BVS File data returned',
                StatusMessage: 'BVS Files'
            });
    },

    apiTrackSchedule: function (scheduleId) {
        var $scope = this;
        var url = baseUrl + 'api/Tracker/TrackSchedule/' + scheduleId;
        httpService.put(url,
            function () {
                $scope.view.onApiTrackSchedule();
            },
            null,
            {
                $ViewElement: $('#schedule_view'),
                ErrorMessage: 'Track Schedule',
                TitleErrorMessage: 'Could not track schedule',
                StatusMessage: 'Track Schedule'
            });
    },

    apiDeleteBvsFile: function (rec, callback) {
        var jsonObj = JSON.stringify(rec);
        var url = baseUrl + 'api/Tracker/BvsFile/' + rec.Id;
        httpService.remove(url,
            callback.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#manage_mappings_modal_view'),
                ErrorMessage: 'Delete Mapping',
                TitleErrorMessage: 'No Delete Mapping Data Returned',
                StatusMessage: 'Delete Mapping'
            });
    },
});
