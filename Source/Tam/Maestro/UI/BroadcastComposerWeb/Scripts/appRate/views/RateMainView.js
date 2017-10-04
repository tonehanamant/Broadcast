//rate main view
//init will be called by Class if exists

var RateMainView = BaseView.extend({

    isActive: false,
    $StationsGrid: null,
    activeRateData: null,
    StationsTextSearch: null, //now dependent if source was previously active
    //rateComponents: null, //needed?
    uploadManager: null,

    initView: function (controller) {
        this.controller = controller;
        this.isActive = true;
        this.initRatesTabs();
        this.$StationsGrid = $('#stations_grid').w2grid(RateConfig.getStationsGridCfg(this));
        this.setupGridEventHandlers();
        this.initUpload();
        this.initializeStationFilters();
        this.initThirdPartyUploadValidationRules();
        this.initializeFileImportOptions();

        //set dynamic tips on
        $('body').tooltip({
            selector: '[data-toggle="tooltip"]'
        });
    },

    //New Rates based Tabs - source

    initRatesTabs: function () {
        var me = this;
        $('#rates_tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {

            //set controler with type, upload manager source type

            var type = e.target.name;
            me.uploadManager.setActiveSourceType(type);
            //load and reset
            me.resetStationDataFilterAll();
            me.clearStationsGridSearch(true);
            me.StationsTextSearch = null;
            me.controller.apiLoadStations(false, type);
            
        });
    },

    //UPLOAD handling - see upload Manager (uses default)

    initUpload: function () {
        this.uploadManager = new RateUploadManager(this);//will call init
        this.uploadManager.setActiveSourceType('OpenMarket');
    },

    //passed from upload manager
    //intercept via InventorySource if TVB/CNN (todo) send to VM for input - add source to request or handle in controller?
    processUploadFileRequest: function (rateRequest, rateSource) {
        if ((rateSource == 'TVB') || (rateSource == 'CNN') || (rateSource == 'TTNW')) {
            this.controller.thirdPartyViewModel.setActiveImport(rateRequest, rateSource);
        } else {
            this.controller.apiUploadInventoryFile(rateRequest);
        }
    },

    //show upload issues if success but response includes "Problems" array (see controller)
    showUploadFileIssues: function (problems) {
        var programProblems = problems.filter(function(problem) {
            return !problem.AffectedProposals;
        });

        var affectedProposalProblems = problems.filter(function (problem) {
            return problem.AffectedProposals && problem.AffectedProposals.length > 0;
        });

        var ret = '';
        if (programProblems.length > 0) {
            ret = '<div><h5><strong>The Following Issues Reported:</strong></h5><dl>';

            $.each(programProblems, function (index, val) {
                var item = '<dt>' + val.ProgramName + ' | ' + val.StationLetters + '</dt><dd>' + val.ProblemDescription + '</dd>';
                ret += item;
            });

            ret += '</dl></div>';
        }

        $.each(affectedProposalProblems, function (index, val) {
            var item = '<dt>' + val.ProblemDescription + '</dt><ul>';

            $.each(val.AffectedProposals, function(i, proposal) {
                item += '<li>' + proposal + '</li>';
            });

            ret += item;
        });
        ret += '</ul></div>';

        var title = programProblems.length > 0 ? 'Rate File Uploaded Successfully - With Issues' : 'Reserved Inventory Updated';
        util.alert(title, ret);
    },

    // TODO - adjust after BE is done
    showSuccessUploadFileWarnings: function (warningObj) {
        var ret = '<div><h5><strong>' + warningObj.Title +  '</strong></h5><dl>';
        $.each(warningObj.Proposals, function (index, val) {
            var item = '<dt>' + val + '</dd>';
            ret += item;
        });
        ret += '</dl></div>';
        util.alert("Reserved Inventory Updated", ret);
    },

    //VM calls modal shown/hidden to disable drag while modal
    setUploadDragEnabled: function (enable) {
        this.uploadManager.dragEnabled = enable;
    },

    initThirdPartyUploadValidationRules: function () {

        var thirdPartyUploadformValidator = $('#import_thirdparty_form').validate({
             ignore: [],
            rules: {
                //import_thirdparty_daypart: {
                //    required: true
                //},
                import_thirdparty_flights: {
                    required: true
                },
                import_thirdparty_blockname: {
                    required: true
                }
            }
        });
    },

    clearThirdPartyValidation: function () {
        $("#import_thirdparty_form").validate().resetForm();
    },

    isThirdPartyValid: function () {
        return $("#import_thirdparty_form").valid();
    },

    /*** STATIONS GRID RELATED ***/
    //TBD
    // Binds several event handlers to the main grid
    setupGridEventHandlers: function () {
        var me = this;

        // double click - opens scrubber; call controller
        this.$StationsGrid.on('dblClick', function (event) {
            var rec = me.$StationsGrid.get(event.recid);

            me.controller.apiGetStationLock(rec.Code, function (lockResponse) {
                if (lockResponse.Success) {
                    me.controller.onStationRecordSelect(rec);
                } else {
                    util.notify("Station Locked", "danger");
                    var msg = 'This Station is currently in use by ' + lockResponse.LockedUserName + '. Please try again later.';
                    util.alert('Station Locked', msg);
                }
            });
        });

        // Click on the search button - executes the search
        $("#rate_view").on("click", '#stations_search_btn', this.stationsGridTextSearch.bind(this));

        // Enter on the search input - executes the search
        $("#rate_view").on("keypress", '#stations_search_input', function (e) {
            var key = e.which;
            if (key == 13) {
                me.stationsGridTextSearch();
            }
        });

        // Click on clear search
        $("#rate_view").on("click", '#stations_search_clear_btn', this.clearStationsGridSearch.bind(this, false));
        //NOT here - event gets set multiple times this way
        //this.initializeStationFilters();
    },

    // Stations: Initializes the W2UI grid view and active data
    //!filter indicates need to sync state of controller api filter with select here
    setStations: function (data, dataFilter) {
        this.setStationsGrid(data);
        this.activeRateData = data;
        if (!dataFilter) this.resetStationDataFilterAll();
        if (!this.StationsTextSearch) {
            this.clearStationsGridSearch(true);
        }
    },

    // Modifies the data for use in Stations grid 
    prepareStationsGridData: function (data) {
        var displayData = util.copyData(data);
        var ret = [];
        $.each(displayData, function (index, value) {
            var item = value;
            item.LastUpdate = moment(item.ModifiedDate, "MM/DD/YYYY h:mm:ss a").valueOf();
            item.recid = item.Code;
            ret.push(item);
        });

        return ret;
    },

    // Main Stations Grid - Clears data in grid; adds new data; called on error to clear data (previous active  rate source)
    setStationsGrid: function (data) {
        if (data) {
            var stationsData = this.prepareStationsGridData(data);
            this.$StationsGrid.clear(false);
            this.$StationsGrid.add(stationsData);
            this.$StationsGrid.resize();

            if (this.StationsTextSearch) {
                //console.log('filter on load', this.StationsTextSearch);
                this.$StationsGrid.search(this.StationsTextSearch, 'OR');
            }
        } else {
            this.$StationsGrid.clear(false);
        }
    },

    // Executes a search in the Stations grid
    stationsGridTextSearch: function (event) {
        var val = $("#stations_search_input").val();
        if (val && val.length) {
            val = val.toLowerCase();
            var search = [{ field: 'LegacyCallLetters', type: 'text', value: [val], operator: 'contains' }, { field: 'Affiliation', type: 'text', value: [val], operator: 'contains' }, { field: 'OriginMarket', type: 'text', value: [val], operator: 'contains' }];
            this.$StationsGrid.search(search, 'OR');
            $("#stations_search_clear_btn").show();
            this.StationsTextSearch = search;
        } else {
            this.clearStationsGridSearch();
        }
    },

    // Clear search
    clearStationsGridSearch: function (clearAll) {
        //if (clearAll) {
        //    this.$StationsGrid.searchReset();
        //} else {
        //    this.$StationsGrid.removeSearch('Estimate', 'Name'); 
        //    this.$StationsGrid.search();
        //}
        this.$StationsGrid.searchReset();
        this.StationsTextSearch = null;
        $("#stations_search_input").val('');
        $("#stations_search_clear_btn").hide();
    },

    //reset the API based filter select to all
    resetStationDataFilterAll: function () {
        $('#stations_filter_input').val('all');
    },

    initializeStationFilters: function () {
        var me = this;

        $('#stations_filter_input').on('change', function () {
            var selected = $(this).val();
            //me.StationsTextSearch = me.$StationsGrid.searchData;
            me.controller.apiLoadStations(selected);
            
        });
    },

    initializeFileImportOptions: function () {
        var me = this;
        me.controller.apiGetFileImportOptions(function (options) {
            me.controller.thirdPartyViewModel.ratingBookOptions(options.RatingBooks);
            me.controller.thirdPartyViewModel.playbackTypeOptions(options.PlaybackTypes);
            me.controller.thirdPartyViewModel.selectedPlaybackType(options.DefaultPlaybackType);
        });
    }
});
