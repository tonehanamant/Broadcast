var ProposalView = BaseView.extend({
    controller: null,
    programsGrid: null,

    formValidator: null,
    formValidatorManage: null,

    activeProgramsData: null,
    activeSelectedId: null, //for position state changes

    activeEditingRecord: null,
    pendingUnsavedSpots: false, //unsaved
    unsavedPrograms: true,  //based on BE return set in VM - not currently active here
    readOnly: false, //tbd future status
    resetView: true,

    sortStates: {
        isMarketRank: true,

        getField: function (name) {
            return this[name] || false;
        },

        //provide logic to alternate direction
        setActiveField: function (field) {
            if (!field) {
                this.activeField = null;
            } else {
                if ( this.activeField && (this.activeField.name == field) ) {
                    this.activeField.isActive = true;//needed?
                    this.activeField.asc = this.activeField.asc ? false : true;//reverse
                } else {
                    this.activeField = util.copyData(this.getField(field));
                }
            }
            //console.log('sort states - set active field', this.activeField);
            return this.activeField;
        },

        activeField: null,

        //defaults
        TargetCpm: {
            name: 'TargetCpm',
            indicator: '#TargetCpm_sort',
            asc: true,
            isActive: true

        },
        HHeCPM: {
            name: 'HHeCPM',
            indicator: '#HHeCPM_sort',
            asc: true,
            //desc: false,
            isActive: true
        },
        
        AdditonalAudienceCPM: {
            name: 'AdditonalAudienceCPM',
            indicator: '#AdditonalAudienceCPM_sort',
            asc: true,
            isActive: true
        }

    },

    initView: function (controller) {
        var $scope = this;

        $scope.controller = controller;
        $scope.programsGrid = $('#proposal_programs_grid').w2grid(PlanningConfig.getProgramsGridCfg($scope));

        $scope.initInputMasks();
        $scope.initValidationRules();
        $scope.initGridEvents();
        $scope.initProposalCollapse();

        // unlock station on window/tab close
        //$(window).on('beforeunload', function () {
        //    if ($scope.controller.proposalViewModel.proposalId && $scope.controller.proposalViewModel.proposalId()) {
        //        $scope.controller.apiGetUnlock($scope.controller.proposalViewModel.proposalId());
        //    }
        //});
    },

    /*** COLLAPSE HEADER ***/

    //collapse events
    initProposalCollapse: function () {
        //need event combination timed to prevent scroll bars from flickering
        $('#proposal_header').on('hidden.bs.collapse', this.onCollapseHeader.bind(this, false));
        $('#proposal_header').on('show.bs.collapse', this.onCollapseHeader.bind(this, true));
    },

    //change icon; resize grid
    onCollapseHeader: function (show) {
        var height = show ? 500 : 688;
        $("#proposal_programs_grid").height(height);
        this.programsGrid.resize();
        var icon = show ? 'glyphicon glyphicon-triangle-bottom' : 'glyphicon glyphicon-triangle-top';
        $("#proposal_header_collapse").removeClass().addClass(icon);     
    },

    //on modal close - reset (see VM)
    resetCollapse: function () {
        $('#proposal_header').collapse('show');
    },

    /*** GRID ***/

    initGridEvents: function () {
        var $scope = this;
        this.programsGrid.on('sort', function (event) {
            //console.log('sort event', event);
            event.preventDefault();
            //only activatable fields should have sort true
            $scope.setGridColumnSort(event.field, true);
        });

        this.programsGrid.on('select', function (event) {
            $scope.activeSelectedId = event.recid;
        });

        this.programsGrid.onClick = this.onProgramsGridClick.bind(this);
    },

    //get a market row
    getMarketRow: function (market) {
        var ret = {
            recid: 'market_' + market.MarketId,
            isMarket: true,
            w2ui: { "style": "background-color: #dedede" },
            MarketName: market.MarketName,
            MarketSubscribers: market.MarketSubscribers,
            Rank: market.Rank,
            TotalSpots: market.MarketTotals.TotalSpots,
            TotalCost: market.MarketTotals.TotalCost,
            TargetImpressions: market.MarketTotals.TotalTargetImpressions,
            TargetCpm: market.MarketTotals.TotalTargetCPM,
            TRP: market.MarketTotals.TotalTRP,
            HHeCPM: market.MarketTotals.TotalHHCPM,
            HHImpressions: market.MarketTotals.TotalHHImpressions,
            GRP: market.MarketTotals.TotalGRP,
            AdditionalAudienceImpressions: market.MarketTotals.TotalAdditionalAudienceImpressions,
            AdditonalAudienceCPM: market.MarketTotals.TotalAdditionalAudienceCPM
        };
        return ret;
    },

    //get a station row
    getStationRow: function (station) {
        var ret = {
            recid: 'station_' + station.Station.StationCode,
            isStation: true,
            style: 'background: #DCDCDC;',
            w2ui: { "style": "background-color: #E9E9E9" },
            Station: station.Station,
            TotalSpots: station.StationTotals.TotalSpots,
            TotalCost: station.StationTotals.TotalCost,
            TargetImpressions: station.StationTotals.TotalTargetImpressions,
            TargetCpm: station.StationTotals.TotalTargetCPM,
            TRP: station.StationTotals.TotalTRP,
            HHeCPM: station.StationTotals.TotalHHCPM,
            HHImpressions: station.StationTotals.TotalHHImpressions,
            GRP: station.StationTotals.TotalGRP,
            AdditionalAudienceImpressions: station.StationTotals.TotalAdditionalAudienceImpressions,
            AdditonalAudienceCPM: station.StationTotals.TotalAdditionalAudienceCPM
        };
        return ret;
    },

    //get total summary rows as - array
    //allow to set the row indivdually for update only
    getTotalRows: function (target, total) {
        var ret = [{
            recid: 'summary_target',
            w2ui: { summary: true, style: 'font-weight: bold;' },
            displayText: 'Total per Target Unit',
            TotalSpots: ' - ', //this will have no data//target.TotalSpots,
            TotalCost: target.TotalCost,
            TargetImpressions: target.TotalTargetImpressions,
            TargetCpm: target.TotalTargetCPM,
            TRP: target.TotalTRP,
            HHeCPM: target.TotalHHCPM,
            HHImpressions: target.TotalHHImpressions,
            GRP: target.TotalGRP,
            AdditionalAudienceImpressions: target.TotalAdditionalAudienceImpressions,
            AdditonalAudienceCPM: target.TotalAdditionalAudienceCPM
        }, {
            recid: 'summary_total',
            w2ui: { summary: true, style: 'font-weight: bold;' },
            displayText: 'Totals',
            TotalSpots: total.TotalSpots,
            TotalCost: total.TotalCost,
            TargetImpressions: total.TotalTargetImpressions,
            TargetCpm: total.TotalTargetCPM,
            TRP: total.TotalTRP,
            HHeCPM: total.TotalHHCPM,
            HHImpressions: total.TotalHHImpressions,
            GRP: total.TotalGRP,
            AdditionalAudienceImpressions: total.TotalAdditionalAudienceImpressions,
            AdditonalAudienceCPM: total.TotalAdditionalAudienceCPM
        }];
        return ret;
    },

    //return stations or modified to active field sort
    checkStationsSortData: function (stations) {
        var active = this.sortStates.activeField;
        if ( active && active.isActive ) {
            //sort by programs to activeField asc and then sort the stations to match high or low in each
            var dir = active.asc ? 'asc' : 'desc';
            $.each(stations, function (index, station) {
                //sort programs (will there always be programs?); add additional secondary sort to Programname
                var programs = _.orderBy(station.ProposalPrograms, [active.name, 'ProgramName'], [dir, 'asc']);
                //add sortParam based on top item
                station.sortParam = programs[0][active.name];
                station.ProposalPrograms = programs;
                //console.log('sort programs', station);
            });
            //sort stations
            stations = _.orderBy(stations, ['sortParam'], [dir]);
            return stations;
        } else {
        return stations;
        }
    },

    //process programs data object for flat groupings/totals in grid - normalize data to grid column renderers, etc
    //todo: handle other needed items like arrays of data for filter criteria
    prepareGridData: function (programsData) {
        //if isMarketRank in  sortStates use data else use the data sorteed by name
        var marketPrograms = this.sortStates.isMarketRank ?
            programsData.DisplayProposalMarketPrograms :
            _.sortBy(programsData.DisplayProposalMarketPrograms, [function (o) { return o.MarketName; }]);
        
        var $scope = this;
        var ret = [];
        $.each(marketPrograms, function (mIdx, market) {
            ret.push($scope.getMarketRow(market));
            //intercept this data if activeField sorting is on
            var stationPrograms = $scope.checkStationsSortData(market.DisplayProposalStationPrograms);
            $.each(stationPrograms, function (sIdx, station) {
                ret.push($scope.getStationRow(station));
                $.each(station.ProposalPrograms, function (pIdx, program) {
                    //Id will represent proposalProgram id for editing but null initially
                    //use generated based on market, station program indexes for now
                    //program.recid = program.ProgramId;
                    program.recid = market.MarketId + '_' + station.Station.StationCode + '_' + (pIdx + 1);
                    program.isProgram = true;
                    ret.push(program);
                });
            });
        });
        var sums = $scope.getTotalRows(programsData.TargetUnitTotals, programsData.OverallTotals);
        ret = ret.concat(sums);
        return ret;
    },

    //load by existing data that will be resorted based on sortStates
    loadGridFromSort: function () {
        this.setResetState(false);
        if (this.activeProgramsData) this.loadGrid(this.activeProgramsData);
    },

    //Load grid from loaded data or existing (sorts) - adjust overall states per resetView
    loadGrid: function (programsData) {
        var resetView = this.resetView;
        this.programsGrid.clear(false);
        if (resetView) this.resetGridStates();
        //get unsaved programs status from VM
        this.unsavedPrograms = this.controller.proposalViewModel.unsavedPrograms();

        this.activeProgramsData = programsData;

        if (programsData && programsData.DisplayProposalMarketPrograms && programsData.DisplayProposalMarketPrograms.length) {
            var gridData = this.prepareGridData(util.copyData(programsData, null, null, true));
            this.programsGrid.add(gridData);
        }

        //reset or set states
        if (resetView) {
            this.resetGridDisplay();
        } else {
            this.setExistingGridDisplay();
        }
    },

    //GRID/VIEW STATES

    //change state from view or view model to reflect pending spots unsaved
    setPendingUnsavedSpots: function (unsaved) {
        this.pendingUnsavedSpots = unsaved;
    },

    //change state from view or view model to reflect loadGrid state handling
    setResetState: function (isReset) {
        this.resetView = isReset;
    },

    //when reset view - clear out states
    resetGridStates: function () {
        this.setPendingUnsavedSpots(false);
        this.setGridColumnSort();
        this.sortStates.isMarketRank = true; 
        this.activeSelectedId = null;
    },

    //reset displays after add - based on current states
    resetGridDisplay: function () {
        this.controller.proposalViewModel.setSortByMarketRank(true);
        this.displayAdditionalDemoColumns(false);
        this.setGridColumnSortDisplay();
    },

    //setExisting displays after add - based on current states
    setExistingGridDisplay: function () {
        //todo: deal with position; update the demo column display directly here?
        if (this.controller.proposalViewModel.selectedAdditionalDemographicId()) {
            this.displayAdditionalDemoColumns(true);
        } else {
            this.displayAdditionalDemoColumns(false);
        }
        this.setGridColumnSortDisplay();
        //try to reset position
        if (this.activeSelectedId) {
            //can either select and scroll into view or scroll to index
            var record = this.programsGrid.get(this.activeSelectedId); //second arg true to to get index
            if (record) {
                this.programsGrid.select(this.activeSelectedId);
                this.programsGrid.scrollIntoView();
            }       
        }
    },

    displayGridHHcolumns: function (display) {
        var $scope = this;

        if (display) {
            $scope.programsGrid.showColumn('HHeCPM', 'HHImpressions');
        } else {
            $scope.programsGrid.hideColumn('HHeCPM', 'HHImpressions');
        }
    },

    displayAdditionalDemoColumns: function (display) {
        var $scope = this;

        if (display) {
            $scope.programsGrid.showColumn('AdditionalAudienceImpressions', 'AdditonalAudienceCPM');
            $(".additional_audience_name").html($scope.controller.proposalViewModel.additionalDemographic()); 
            $("#additional_option_menu_dropdown").dropdown('toggle');
        } else {
            $scope.programsGrid.hideColumn('AdditionalAudienceImpressions', 'AdditonalAudienceCPM');
            $(".additional_audience_name").html('');
        }

        $scope.programsGrid.resize();
    },

    /*** SORTING ***/

    //from VM click change the market sort
    sortMarket: function (isRank) {
        this.sortStates.isMarketRank = isRank;
        this.loadGridFromSort();
    },

    //set sortStates - clear if resetView; activeSort = reload grid after setting sorts
    setGridColumnSort: function (fieldName, activeSort) {
        var field = this.sortStates.setActiveField(fieldName);
        if(field && activeSort) {
            this.loadGridFromSort(); //mechanism to load grid using current copied data and sort states
        }

    },

    //setDisplay based on sortStates
    setGridColumnSortDisplay: function () {
        //clear any initial - does not seem to clear all per documents
        $(".sort_indicator").removeClass();
        var field = this.sortStates.activeField;
        if (field && field.isActive) {
            var dir = field.asc ? 'up' : 'down';
            $(field.indicator).addClass('sort_indicator w2ui-sort-' + dir);
           // console.log('setGridColumnSortDisplay', dir);
        }

    },

    /*** INLINE SPOTS EDITING***/

    //user clicks grid: enable edit if spots column and not active/readOnly - has id
    onProgramsGridClick: function (event) {
        if (event.column === null || this.readOnly || this.activeEditingRecord) {
            return;
        }

        if (event.column == 4) {//spots column
            var record = this.programsGrid.get(event.recid);
            //record needs to be program with a Id (saved)
            if (record && record.isProgram && record.Id) {
                //console.log('edit grid click', record, event);
                this.setEditSpot(record, event);
            } else {
                return;
            }

        } else {
            return;
        }
    },

    //set spot in row for inline editing; handle eveents
    setEditSpot: function (record, clickevent) {
        
        var editTarget = $('#program_spot_' + record.recid);
        
        if (editTarget.length && !editTarget.hasClass('is-editing')) {
            this.activeEditingRecord = record;
            editTarget.addClass("is-editing");
            var $scope = this;
            var input = editTarget.find(".edit-input");
            input.show();
            var spotval = record.TotalSpots || 0;
            input.val(spotval);
            input.focus();
            
            //keyup or keypress?
            input.keypress(function (event) {
                // value = this.value.replace(/[^0-9\.]/g, '');
                //value = value >= 999 ? 999 : value < 0 ? 0 : value;
                // (this.value != value) {
                   // this.value = value;
                //}
                //console.log('press', event);
                //some browsers use keyCode - deprecating charCode?
                return (event.charCode == 8 || event.charCode == 0) ? null : event.charCode >= 48 && event.charCode <= 57;
                //return (event.keyCode == 8 || event.keyCode == 0) ? null : event.keyCode >= 48 && event.keyCode <= 57;
            });
            input.keydown(function (event) {
                if (event.keyCode === 9) {//TAB
                    event.preventDefault();
                    //var nextCell = $("#rate-" + record.recid);
                    //nextCell.click();
                    //nextCell.find("input").focus();
                    input.blur();
                } else if (event.keyCode === 13) { //ENTER
                    event.preventDefault();
                    //use blur event so not called twice
                    input.blur();

                }
            });
            input.blur(function (event) {
                setTimeout(function () {
                    this.onEditSpot(editTarget, input, record);
                    //input.off("keyup");
                    input.off("keypress");
                    input.off("keydown");
                    input.off("blur");
                }.bind(this), 50);
            }.bind(this));

        }

    },

    //after a spot is edites send to api; handle return to update row and states
    onEditSpot: function (editTarget, input, record) {
        var spotVal = parseInt(input.val());
        var currentVal = record.TotalSpots;
        //console.log('onEditSpot', spotVal);
        //if val is same as record or empty then end the edit - no API call//else make the api call and end the edit after success/failure
        if ( (spotVal || (spotVal === 0) ) && (spotVal !== currentVal)) {
            //console.log('call api');
            var $scope = this;
            $scope.controller.apiEditProposalProgramSpot(record.Id, spotVal,
				//success
				function (programsSpotData) {
				    $scope.onAfterEditSpotSuccess(programsSpotData, editTarget, input);
				},
				//error
				function () {
				    $scope.endEditSpot(editTarget, input);
				}
			);
        } else {
            this.endEditSpot(editTarget, input);
        }
    },

    //end spot editing; hide input; remove class and allow editing again
    endEditSpot: function (editTarget, input) {
        input.hide();
        editTarget.removeClass("is-editing");
        this.activeEditingRecord = null;
    },

    //after success - end editing - update the row and totals data; set pending; notify
    onAfterEditSpotSuccess: function (programsSpotData, editTarget, input) {
        //console.log('onAfterEditSpotSuccess', programsSpotData);
        //set the record data which will also re-render (so end first?)
        var recid = this.activeEditingRecord.recid;
        this.endEditSpot(editTarget, input);
        this.updateGridEditSpot(recid, programsSpotData);
        //mark pending to provide warnings
        this.setPendingUnsavedSpots(true);
        util.notify("Program Spot Edited (please save proposal to commit)");
    },

    //update specific row and totals based on response
    updateGridEditSpot: function (recid, programsSpotData) {
        //var programId = this.activeEditingRecord.recid;//existing recid to update - will be cleared
        var totals = this.getTotalRows(programsSpotData.TargetUnitTotals, programsSpotData.OverallTotals);//array returns recid(s) same as updating
        var market = this.getMarketRow(programsSpotData.DisplayProposalMarketPrograms[0]);//return recid same as updating
        var station = this.getStationRow(programsSpotData.DisplayProposalMarketPrograms[0].DisplayProposalStationPrograms[0]);//return recid same as updating
        var program = programsSpotData.DisplayProposalMarketPrograms[0].DisplayProposalStationPrograms[0].ProposalPrograms[0];//new program data
        var grid = this.programsGrid;
        grid.set(recid, program);
        grid.set(market.recid, market);
        grid.set(station.recid, station);
        //summary_target  summary_total
        grid.set('summary_target', totals[0]);
        grid.set('summary_total', totals[1]);
        //the row style gets removed when set even though property set - so force?
        $("#grid_ProgramsGrid_rec_summary_target").css("font-weight", "bold");
        $("#grid_ProgramsGrid_rec_summary_total").css("font-weight", "bold");
        //sync active programs
        this.syncActiveProgramsData(programsSpotData);
    },

    //syncs stored activeProgramsData following spot edits - so that sorting reflects changed data
    syncActiveProgramsData: function (programsSpotData) {
        var active = this.activeProgramsData;
        //sync overall totals
        active.TargetUnitTotals = programsSpotData.TargetUnitTotals;
        active.OverallTotals = programsSpotData.OverallTotals;
        //find and sync market totals, station totals, program
        var newMarket = programsSpotData.DisplayProposalMarketPrograms[0];
        var activeMarket = util.objectFindByKey(active.DisplayProposalMarketPrograms, 'MarketId', newMarket.MarketId);
        if (activeMarket) {
            activeMarket.MarketTotals = newMarket.MarketTotals;
            //this needs adjustment as Station is sub object
            //var activeStation = util.objectFindByKey(activeMarket.DisplayProposalStationPrograms, 'Station', programsSpotData.DisplayProposalMarketPrograms[0].DisplayProposalStationPrograms[0].Station.StationCode);
            var newStation = newMarket.DisplayProposalStationPrograms[0];
            var activeStation = _.find(activeMarket.DisplayProposalStationPrograms, function (o) { return o.Station.StationCode === newStation.Station.StationCode; });
            if (activeStation) {
                activeStation.StationTotals = newStation.StationTotals;
                //assumes that active data and new data both have Id (saved)
                var newProgram = newStation.ProposalPrograms[0];
                var activeProgram = util.objectFindByKey(activeStation.ProposalPrograms, 'Id', newProgram.Id);
                if (activeProgram) {
                    //activeProgram = newProgram;//apply it so entire object gets changed
                    $.extend(true, activeProgram, newProgram);
                }
            }
           // console.log('changed programs data', this.activeProgramsData);
        }

    },

    //called from VM - check pending before close modal
    checkUnsavedSpots: function () {
        //console.log('check unsaved');
        var $scope = this;
        if (this.pendingUnsavedSpots) {
            var continueFn = function () {
                $scope.pendingUnsavedSpots = false;
                $scope.controller.proposalViewModel.showModal(false);
            };
            util.confirm('Unsaved Spot Edits', 'There are edited program spots that are not saved. Continuing will lose these changes.', continueFn);
            return true;
        }
        return false;
    },

    // updates grid totals and programs with the group object -- already existing programs that are not part of the group object are kept unchanged
    updateGridWithPartialGroup: function (groupedProposalPrograms) {
        var $scope = this;

        if (groupedProposalPrograms && groupedProposalPrograms.DisplayProposalMarketPrograms) {
            groupedProposalPrograms.DisplayProposalMarketPrograms.forEach(function (groupedDisplayProposalMarketProgram) {
                if (groupedDisplayProposalMarketProgram.DisplayProposalStationPrograms) {
                    groupedDisplayProposalMarketProgram.DisplayProposalStationPrograms.forEach(function (proposalStationProgram) {
                        if (proposalStationProgram.ProposalPrograms) {
                            proposalStationProgram.ProposalPrograms.forEach(function (proposalProgram) {
                                $scope.programsGrid.records.map(function (record) {
                                    if (record.Id == proposalProgram.Id) {
                                        $scope.programsGrid.set(record.recid, proposalProgram); 
                                    }
                                })
                            })
                        }

                        var station = $scope.getStationRow(proposalStationProgram);
                        $scope.programsGrid.set(station.recid, station);
                    })
                }

                var market = $scope.getMarketRow(groupedDisplayProposalMarketProgram);
                $scope.programsGrid.set(market.recid, market);
            })
        }
       
        var totals = this.getTotalRows(groupedProposalPrograms.TargetUnitTotals, groupedProposalPrograms.OverallTotals);
        $scope.programsGrid.set('summary_target', totals[0]);
        $scope.programsGrid.set('summary_total', totals[1]);
        $("#grid_ProgramsGrid_rec_summary_target").css("font-weight", "bold");
        $("#grid_ProgramsGrid_rec_summary_total").css("font-weight", "bold");
    },

    /*** MASKING AND VALIDATION ***/

    initInputMasks: function () {
        $('input[name="proposal_units"]').w2field('int', { autoFormat: false, min: 1 });
        $('input[name="proposal_budget"]').w2field('money');
        $('input[name="proposal_impressions"]').w2field('float', { precision: 3 });
    },

    initValidationRules: function () {
        var $scope = this;

        $scope.formValidator = $('#proposal_form').validate({
            rules: {
                proposal_name: {
                    required: true
                },
                proposal_advertiser: {
                    required: true
                }
            }
        });

        $scope.formValidatorManage = $('#proposal_form_manage').validate({
            rules: {
                proposal_flight: {
                    required: true
                },
                proposal_spot: {
                    required: true
                },
                proposal_market: {
                    required: true
                },
                proposal_demo: {
                    required: true
                }
            }
        });
    },

    clearValidation: function () {
        var $scope = this;

        $scope.formValidator.resetForm();
        $scope.formValidatorManage.resetForm();
    },

    isValid: function () {
        return $('#proposal_form').valid();
    },

    isValidForCriteria: function () {
        return $('#proposal_form_manage').valid();
    },

    // informed keyword should match the start of the option
    select2MatchStart: function (params, data) {
        if (!params.term) {
            return data;
        }

        if (data.text.toUpperCase().indexOf(params.term.toUpperCase()) == 0) {
            return data;
        }

        return false;
    }
});