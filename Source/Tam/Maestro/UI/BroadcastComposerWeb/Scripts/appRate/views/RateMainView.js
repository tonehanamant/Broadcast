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
        $("#unit_scx_button").on('click', this.controller.openUnitSCX.bind(this));
        $('#unit_scx_button').hide();
    },

    //New Rates based Tabs - source

    initRatesTabs: function () {
        var me = this;
        $('#rates_tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {

            //set controler with type, upload manager source type

            //Change to handle temporary Barter - no stations/grid - hide
            var type = e.target.name;
            me.uploadManager.setActiveSourceType(type);
            if (type === 'Barter') {
                $('.grid_hider').hide();
                $('#unit_scx_button').show();
                
            } else {
                $('#unit_scx_button').hide();
                $('.grid_hider').show();
                //load and reset
                me.resetStationDataFilterAll();
                me.clearStationsGridSearch(true);
                me.StationsTextSearch = null;
                me.controller.apiLoadStations(false, type);
            }
        });
    },

    //UPLOAD handling - see upload Manager (uses default)

    initUpload: function () {
        this.uploadManager = new RateUploadManager(this);//will call init
        this.uploadManager.setActiveSourceType('OpenMarket');
    },

    //passed from upload manager
    //intercept via InventorySource if TVB/CNN (todo) send to VM for input - add source to request or handle in controller?
    //REVISED = process as multiple files queue if applicable - rateRequest will be array
    processUploadFileRequest: function (rateRequest, rateSource) {
        if ((rateSource == 'TVB') || (rateSource == 'CNN') || (rateSource == 'TTWN')) {
            this.controller.thirdPartyViewModel.setActiveImport(rateRequest[0], rateSource);
        } else {
            //this.controller.apiUploadInventoryFile(rateRequest);
            this.controller.apiQueueInventoryFiles(rateRequest);  //will be array of files
        }
    },

    //REVISE to aggregated importFileErrors
    //with grouping
    showUploadFileIssues: function (errors, overallFileCount) {
        var title = 'Import Errors';
        var status = '<strong>' + errors.length + ' of ' + overallFileCount + '</strong> files failed.<br/>';
        var ret = status + '<p>Encountered errors uploading the following files:</p>';
        var $scope = this;
        $.each(errors, function (index, errorItem) {
            var msg = errorItem.message;
            if (errorItem.problems) msg = $scope.getUploadFileProblems(errorItem.problems);
            //var append = '<p><strong>' + errorItem.fileName + '</strong><br/>' + msg + '</p>';
            var append = '<div><strong>' + errorItem.fileName + '</strong></div>';
            var elId = 'error_' + (index + 1);
            append += '<div class="error-item""><a data-toggle="collapse" class="collapsed" href="#' + elId + '" aria-expanded="false" aria-controls="' + elId + '">Expand to view errors</a></div>';
            append += '<div class="collapse" id="' + elId + '">' + msg + '</div><p></p>';
            ret += append;
        });
        //console.log('issues', ret);
        httpService.showDefaultError(ret, title, false, true);
    },

    //single file version - error will contain message or problems and fileName
    showSingleUploadFileError: function (errorItem) {
        var title = 'Import Error';
        
        var ret = '<p>Encountered error uploading the following file: <strong>' + errorItem.fileName + '</strong></p>';
        var msg = errorItem.message;
        if (errorItem.problems) msg = this.getUploadFileProblems(errorItem.problems);
        ret += ('<p>' + msg + '</p>');
        httpService.showDefaultError(ret, title, false, true);
    },

    //show upload issues if success but response includes "Problems" array (see controller)
    //CHANGE - NO alert - will no longer be success. handle as success false and show httpService error modal - intercept as callback to api call
    //REVISED per aggregated errors - Problems and errors
    getUploadFileProblems: function (problems) {
        var programProblems = problems.filter(function (problem) {
            return !problem.AffectedProposals;
        });
        var affectedProposalProblems = problems.filter(function (problem) {
            return problem.AffectedProposals && problem.AffectedProposals.length > 0;
        });

        var ret = '';
        if (programProblems.length > 0) {
            ret = '<div><h5>The Following Issues were Reported:</h5><dl>';

            $.each(programProblems, function (index, val) {
                var item;

                if (val.ProgramName && val.StationLetters)
                    item = '<dt>' + val.ProgramName + ' | ' + val.StationLetters + '</dt><dd>' + val.ProblemDescription + '</dd>';
                else if (val.ProgramName)
                    item = '<dt>' + val.ProgramName + '</dt><dd>' + val.ProblemDescription + '</dd>';
                else if (val.StationLetters)
                    item = '<dt>' + val.StationLetters + '</dt><dd>' + val.ProblemDescription + '</dd>';
                else
                    item = '<dt></dt><dd>' + val.ProblemDescription + '</dd>';

                ret += item;
            });

            ret += '</dl></div>';
        }

        $.each(affectedProposalProblems, function (index, val) {
            var item = '<dt>' + val.ProblemDescription + '</dt><ul>';

            $.each(val.AffectedProposals, function (i, proposal) {
                item += '<li>' + proposal + '</li>';
            });

            ret += item;
        });
        ret += '</ul></div>';
        return ret;
    },


    //VM calls modal shown/hidden to disable drag while modal
    setUploadDragEnabled: function (enable) {
        this.uploadManager.dragEnabled = enable;
    },
    //uses required in fields not needed here?
    initThirdPartyUploadValidationRules: function () {

        var thirdPartyUploadformValidator = $('#import_thirdparty_form').validate({
            rules: {
                //import_thirdparty_daypart: {
                //    required: true
                //},
                //import_thirdparty_flights: {
                //    required: true
                //},
                //import_thirdparty_blockname: {
                //    required: true
                //}
                import_thirdparty_effective_date: {
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
            me.controller.thirdPartyViewModel.initOptions(options);
        });
    }
});
