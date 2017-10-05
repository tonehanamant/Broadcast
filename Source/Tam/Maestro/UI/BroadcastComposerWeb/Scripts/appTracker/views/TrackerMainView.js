//tracker main view
//init will be called by Class if exists
//Changing SCX related for upload to generic schedule

var TrackerMainView = BaseView.extend({

    isActive: false,
    $ScheduleGrid: null,
    activeQuarterId: null,
    activeScheduleData: null,
    scheduleComponents: null,
    activeScheduleRequest: null,  // upload file request or existing schedule data
    SchedulesTextSearch: null, //now dependent if source was previously active

    $ScheduleModal: null,

    uploadManager: null,

    MappingsView: null,
    RatingsView: null,
    BvsFilesListingView: null,

    initView: function (controller) {
        this.controller = controller;
        this.isActive = true;

        this.$ScheduleGrid = $('#schedule_grid').w2grid(TrackerConfig.getScheduleGridCfg(this));
        //mappings
        $("#tracker_mappings_btn").on('click', this.manageMappings.bind(this));

        //ratings books
        $("#tracker_ratings_books_btn").on('click', this.manageRatings.bind(this));

        //BVS Files
        $("#bvs_files_btn").on('click', this.manageBvsFiles.bind(this));

        //Add advertiser
        $("#tracker_add_adverisers_btn").on('click', this.addAdvertiser.bind(this));

        this.setupGridEventHandlers();
        this.initUpload();
    },

    //UPLOAD handling - see tracker specific upload Manager 

    initUpload: function () {
        this.uploadManager = new TrackerUploadManager(this);//will call init

    },

    //process BVS - passed from upload manager
    processUploadBvsFileRequest: function (bvsRequest) {
        this.controller.apiUploadBVSFile(bvsRequest);

    },

    //fileRequest now single object
    processUploadScheduleFileRequest: function (fileRequest) {
        this.setScheduleModal('upload', fileRequest);

    },

    //uploadFtp: function () {
    //    this.controller.apiUploadFtp();
    //},

    //displayFtpMessages: function (message) {
    //    message = message.replace(/(?:\r\n|\r|\n)/g, '<br />');
    //    $("#ftpmsg").html(message);
    //    $("#ftpResultModel").modal({ show: true });
    //},

    /*** SCHEDULE GRID RELATED ***/

    // Binds several event handlers to the main grid
    setupGridEventHandlers: function () {
        var me = this;

        // double click - opens scrubber; call controller
        this.$ScheduleGrid.on('dblClick', function (event) {
            var rec = me.$ScheduleGrid.get(event.recid);
            //console.log('$ScheduleGrid double click', event, rec);
            me.controller.onScrubRecordSelect(rec);
        });

        // Click on the search button - executes the search
        $("#schedule_view").on("click", '#schedule_search_btn', this.scheduleGridTextSearch.bind(this));

        // Enter on the search input - executes the search
        $("#schedule_view").on("keypress", '#schedule_search_filter_input', function (e) {
            var key = e.which;
            if (key == 13) {
                me.scheduleGridTextSearch();
            }
        });

        // Click on clear search
        $("#schedule_view").on("click", '#schedule_search_clear_btn', this.clearScheduleGridSearch.bind(this, false));

        // Quarter filter change
        $("#schedule_view").on("change", '#quarter_filter_input', function (e) {
            var val = $(this).val();
            me.scheduleGridQuarterFilter(val);
        });

        // Advertiser filter change
        $("#schedule_view").on("change", '#advertiser_filter_input', function (e) {
            var val = $(this).val();
            me.scheduleGridAdvertiserFilter(val);
        });

        //handle menu clicks
        this.$ScheduleGrid.on('menuClick', this.onScheduleMenuClick.bind(this));

    },

    // Initializes the W2UI grid active data, and configures its filters
    setSchedule: function (data, initialComponents) {
        this.setScheduleGrid(data.Schedules);
        this.activeScheduleData = data;
        var setQtrSelects = false;
        if (initialComponents) {
            this.scheduleComponents = initialComponents;
            this.activeQuarterId = initialComponents.CurrentQuarter.Id;
            setQtrSelects = true;
        } else {
            this.activeQuarterId = this.controller.activeQuarterId;
        }

        this.setScheduleFilterSelects(data, setQtrSelects);
        //this.clearScheduleGridSearch(true);
    },

    // Modifies the data for use in W2UI grid (i.e. adds the 'recid' field to each source element)
    prepareScheduleGridData: function (data) {
        var displayData = util.copyData(data);

        var ret = [];
        $.each(displayData, function (index, value) {
            var item = value;
            item.recid = item.Id;
            ret.push(item);
        });

        return ret;
    },

    // Clears the W2UI grid and adds the new data
    setScheduleGrid: function (data) {
        var scheduleData = this.prepareScheduleGridData(data);
        this.$ScheduleGrid.clear(false);
        this.$ScheduleGrid.add(scheduleData);
        this.$ScheduleGrid.resize();

        if (this.SchedulesTextSearch) {
            //console.log('filter on load', this.StationsTextSearch);
            this.$ScheduleGrid.search(this.SchedulesTextSearch, 'OR');
        }

        $('[data-toggle="tooltip"]').tooltip({
            container: 'body'
        });
    },

    // Populate the grids filters (Quarters and Advertisers) 
    //may be from a filter all on quarters so no reset
    setScheduleFilterSelects: function (scheduledata, qtrReset) {
        var me = this;
        var quarters = this.scheduleComponents.Quarters;
        var advertisers = scheduledata.Advertisers;
        var qSelect = $("#quarter_filter_input");
        var aSelect = $("#advertiser_filter_input");

        if (qtrReset) {
            qSelect.empty();
            var qopts = '<option value="all">All</option>';
            $.each(quarters, function (index, qt) {
                qopts += '<option value="' + qt.Id + '">' + qt.Display + '</option>';
            });
            qSelect.html(qopts).val(me.activeQuarterId);
        }

        aSelect.empty();
        var aopts = '<option value="all">All</option>';
        $.each(advertisers, function (index, ad) {
            aopts += '<option value="' + ad.Id + '">' + ad.Display + '</option>';
        });

        aSelect.html(aopts).val('all');
    },

    // Executes a search in the W2UI grid
    scheduleGridTextSearch: function (event) {
        var val = $("#schedule_search_filter_input").val();
        if (val && val.length) {
            val = val.toLowerCase();
            var search = [{ field: 'Estimate', type: 'text', value: [val], operator: 'contains' }, { field: 'Name', type: 'text', value: [val], operator: 'contains' }];
            this.$ScheduleGrid.search(search, 'OR');
            $("#schedule_search_clear_btn").show();
            this.SchedulesTextSearch = search;
        } else {
            this.clearScheduleGridSearch();
        }
    },

    // Clear search - either specific or all
    clearScheduleGridSearch: function (clearAll) {
        if (clearAll) {
            this.$ScheduleGrid.searchReset();
        } else {
            this.$ScheduleGrid.removeSearch('Estimate', 'Name'); //this is not working
            this.$ScheduleGrid.search();
        }
        this.SchedulesTextSearch = null;
        $("#schedule_search_filter_input").val('');
        $("#schedule_search_clear_btn").hide();
    },

    // Filter by current sdvertisers: change to AdvertiserId to match selects: can be all
    // AND/OR searches do not work in W2UI to coordinate with advertisers/text search; changing to find advertisers and reset grid
    scheduleGridAdvertiserFilter: function (advertiser) {
        this.clearScheduleGridSearch(true);
        var recs = [];
        var displayData = util.copyData(this.activeScheduleData.Schedules);
        if (advertiser == 'all') {
            recs = displayData;
        } else {
            $.each(displayData, function (index, val) {
                if (val.AdvertiserId == advertiser) {
                    recs.push(val);
                }
            });
        }

        recs = this.prepareScheduleGridData(recs);
        this.$ScheduleGrid.clear(false);
        this.$ScheduleGrid.add(recs);
        this.$ScheduleGrid.refresh();
    },

    // Filter by api Call; can be all
    scheduleGridQuarterFilter: function (quarter) {
        this.clearScheduleGridSearch(true);

        // call API pass QuarterId and controller will get by Id
        this.controller.setScheduleByQuarterFilter(quarter);
    },


    /*** SCHEDULE  MODAL - use KO ***/
    //based on type/mode

    //set modal - formType (upload, advertiser) - use schedule Object if upload or editing; isEdit - set VM mode to edit
    setScheduleModal: function (formType, schedule, isEdit) {
        var me = this;
        var mode = isEdit ? 'edit' : 'create';
        //set VM ype/mode
        this.controller.viewModel.setFormTypeMode(formType, mode);
        this.activeScheduleRequest = schedule ? schedule : null;
        this.initScheduleValidationRules(formType, isEdit);

        if (!this.$ScheduleModal) {
            this.$ScheduleModal = $('#scheduleModal');
            this.$ScheduleModal.on('shown.bs.modal', this.onScheduleModalShown.bind(this));
            this.$ScheduleModal.on('hidden.bs.modal', this.onScheduleModalHidden.bind(this));
        }

        if (!this.activeModal) {
            this.$ScheduleModal.modal({
                backdrop: 'static',
                show: false,
                keyboard: false
            });
            this.activeModal = true;
        }

        this.$ScheduleModal.modal("show");
    },

    onScheduleModalShown: function () {
        this.uploadManager.dragEnabled = false;
        this.controller.viewModel.setSchedule(this.activeScheduleRequest);
    },

    onScheduleModalHidden: function () {
        this.uploadManager.dragEnabled = true;
        this.activeScheduleRequest = null;
        this.controller.viewModel.resetSchedule(); //generic reset all?

    },

    //after upload save - hide modal, etc - for all saves?
    onAfterScheduleUploadSave: function () {
        this.$ScheduleModal.modal("hide");
    },

    //grid menu click id 1 through 4
    onScheduleMenuClick: function (event) {
        //console.log('onScheduleMenuClick', event);
        var id = event.menuItem.id;
        var scheduleId = event.recid;
        switch (id) {
            case 1: //client report
                this.controller.downloadScheduleReport(scheduleId);
                break;
            case 2: //3rd party client report
                this.controller.downloadClientReport(scheduleId);
                break;
            case 3: //3rd party provider report
                this.controller.downloadProviderReport(scheduleId);
                break;
            case 4: //settings
                var rec = this.$ScheduleGrid.get(event.recid);
                if (rec.IsBlank) {
                    //console.log('Settings - Is Blank', rec.Id);
                    this.controller.apiGetSchedule(rec.Id, 'advertiser');
                } else {
                    //console.log('Settings - NOT Blank', rec.Id);
                    this.controller.apiGetSchedule(rec.Id, 'upload');
                }
                break;
            case 5: //Rerun Tracking
                this.controller.apiTrackSchedule(event.recid);
                util.notify("Tracking Re-running", 'info');
                break;
        }
    },

    onApiTrackSchedule: function () {
        this.controller.refreshApiLoadSchedules();
    },

    //adjust per type/is Edit
    initScheduleValidationRules: function (type, isEdit) {
        //undocumented - clear out current rules
        $('#schedule_form').removeData('validator');

        var rules = { //for all forms - tbd

            schedule_input_name: {
                required: true
            },
            schedule_input_advertiser: {
                required: false
            },
            schedule_input_posting: {
                required: true
            },
            schedule_input_iscis: {
                required: true
            },
            schedule_input_posttype: {
                required: true
            }
        };

        if (type == 'upload') {

            rules.schedule_input_estimate = {
                required: true,
                min: 1
            };

            rules.schedule_input_inventory = {
                required: true
            };

        } else if (type == 'advertiser') {
            //add custom check on save for start versus end validation
            rules.schedule_input_start_date = {
                required: true
            };

            rules.schedule_input_end_date = {
                required: true
            };

        }

        if (type == 'advertiser' || isEdit) {

            rules.schedule_input_demos = {
                required: true
            };
        }

        var scheduleValidator = $('#schedule_form').validate({ rules: rules });
    },

    //ADVERTISER

    addAdvertiser: function () {
        this.setScheduleModal('advertiser');
    },

    //MAPPINGS

    manageMappings: function () {
        this.controller.apiManageMappings();
    },

    onMappingsSet: function (program, station) {
        if (!this.MappingsView) {
            this.MappingsView = new TrackerManageMappings(this);
            this.MappingsView.initView();
        }
        this.MappingsView.setMappings(program, station);
    },

    //RATINGS BOOKS

    manageRatings: function () {
        this.controller.apiManageRatingsBooks();
    },

    onRatingsSet: function (ratingsData) {
        if (!this.RatingsView) {
            this.RatingsView = new TrackerManageRatingsBooks(this);
            this.RatingsView.initView();
        }
        this.RatingsView.setRatings(ratingsData);
    },

    //BVS Files

    manageBvsFiles: function () {
        this.controller.apiBvsFileListing();
    },

    onBvsFileListingSet: function (bvsFileListingData) {
        if (!this.BvsFilesView) {
            this.BvsFilesView = new BvsFileListingView(this);
            this.BvsFilesView.initView();
        }
        this.BvsFilesView.setBvsFiles(bvsFileListingData);
    }
});