//Proposal Detail Open Market View - handle the Open Market Planning for active Detail Set with programs and weeks
//revision to 1 combined grid with dynamic columns/weeks
var ProposalDetailOpenMarketView = BaseView.extend({
    activeInventoryData: null,
    //separate storage of edited records so can reset grid record state after changes (sort, filter); use for saving independent of what is displayed in grids
    activeEditWeekRecords: [], //flat array all weeks
    //activeDetailSet: null,
    //isActive: false,
    openMarketsGrid: null,
    weeksLength: null,
    //programsGrid: null,
    //programsHeaderGrid: null,
    //inventoryWeekGrids: [],
    //inventoryWeekHeaderGrids: [],
    //ProposalView: null,
    //OpenMarketVM: null,
    //CriteriaBuilderVM: null,
    //FilterVM: null,
    isReadOnly: false,
    //bypassCheckSave: false,
    $Modal: null,
    //isScrollSet: false,
    //scrollPositions: null, //stored positions for resetting user poistion after refresh
    weekColumnGroupTpl: _.template('<div class="openmarket-column-group"><div>${ quarter } Week of ${ week }</div>' +
                                   '<div class="cadent-dk-blue goals">' +
                                        '<i class="fa fa-bullhorn" aria-hidden="true"></i> ${ weekImpressions }/${ impressions } <span class="label ${ impressionsMarginClass } custom-label">${ impressionsPercent }%</span>&nbsp;&nbsp;&nbsp;&nbsp;' +
                                        '<i class="fa fa-money" aria-hidden="true"></i> ${ weekBudget }/${ budget } <span class="label ${ budgetMarginClass } custom-label">${budgetPercent} %</span>' +
                                   '</div>'),
    isMarketSortName: false,
    //marketSortIndexMap: [], //index of active markets sort - determined from programs market data; used for sorting weeks (name or rank)

    initView: function (view) {
        var self = this;

        self.ProposalView = view;

        self.OpenMarketVM = new ProposalDetailOpenMarketViewModel(this);
        ko.applyBindings(this.OpenMarketVM, document.getElementById("proposal_openmarket_view"));

        self.CriteriaBuilderVM = new CriteriaBuilderViewModel(self);
        ko.applyBindings(this.CriteriaBuilderVM, document.getElementById("criteria_builder_modal"));

        self.FilterVM = new FilterViewModel(self);
        ko.applyBindings(this.FilterVM, document.getElementById("filter_modal"));
        self.initModal();
    },

    //MODAL//
    initModal: function () {
        this.$Modal = $('#proposal_detail_openmarket_modal');
        this.$Modal.modal({ backdrop: 'static', show: false, keyboard: false });

        //hide event
        //intercept - before hide to check unsaved on view
        this.$Modal.on('hide.bs.modal', this.onCheckInventoryCancel.bind(this));

        //hidden event
        this.$Modal.on('hidden.bs.modal', this.onClearInventory.bind(this, true));

        //shown event
        this.$Modal.on('shown.bs.modal', this.onSetInventory.bind(this));
    },

    showModal: function (isHide) {
        var $scope = this;

        if (isHide) {
            $scope.ProposalView.controller.planningController.apiGetPrimaryProposal($scope.activeInventoryData.ProposalId, function (proposal) {
                $scope.ProposalView.controller.proposalViewModel.load(proposal);
                $scope.$Modal.modal('hide');
            });
        } else {
            $scope.$Modal.modal('show');
        }

        //$scope.FilterVM.clearFilters();
        // $scope.OpenMarketVM.selectedSpotFilterOption(1);
        //$scope.activeEditWeekRecords = [];
    },

    //set grid with dynamic columns via the weeks length
    //TBD to recreate on open or adjust columns in current
    //on load
    initGrid: function () {
        if (!this.openMarketsGrid) {
            this.openMarketsGrid = $('#openmarkets_grid').w2grid(PlanningConfig.getOpenMarketGridCfg(this, this.weeksLength));
        } else {
            var columns = PlanningConfig.getOpenMarketGridCfg(this, this.weeksLength).columns;
            this.openMarketsGrid.columns = columns;
            //normally would refresh but wait for setGrid to handle
        }
    },

    //separate mechanism for initial loads - init/reset Grid columns with weeks each time
    loadInventory: function (detailSet, inventory, readOnly) {
        this.activeDetailSet = detailSet;
        this.isReadOnly = readOnly;
        this.weeksLength = inventory.Weeks.length;
        this.initGrid();

        this.FilterVM.clearFilters();
        this.OpenMarketVM.selectedSpotFilterOption(1);
        this.activeEditWeekRecords = [];
        this.setInventory(inventory);
    },

    //TBD
    refreshInventory: function (inventory, reset, checkEdits) {
        this.onClearInventory(reset);
        this.setInventory(inventory, true, checkEdits);
    },

    //REVISE
    //set allinitial inventory data
    setInventory: function (inventory, isRefresh, checkEdits) {
        this.activeInventoryData = util.copyData(inventory, null, null, true);
        this.OpenMarketVM.setInventory(inventory, this.isReadOnly);
        this.FilterVM.setAvailableValues(inventory.DisplayFilter);
        this.CriteriaBuilderVM.ProgramNamesOptions(inventory.RefineFilterPrograms);

        if (isRefresh) {
            this.onSetInventory(checkEdits);
        } else {
            this.showModal();
        }
    },

    //REVISE - sey active back on close?
    //after modal shown/refresh - set inventory views
    onSetInventory: function (checkEdits) {
        //initial render after shown
        //if (!this.isActive) {
        //    this.initGrid();
        //    this.isActive = true;
        //}
        this.setGrid();

        //will only reset if recorded
        //this.scrollToLastPosition();
        if (checkEdits) {
            // this.resetEditedGridRecords();
        }
    },

    //set Grid - combined
    setGrid: function () {
        //console.log(this.openMarketsGrid);
        //var gridData = this.prepareProgramsGridData(markets);
        this.openMarketsGrid.clear();
        var gridRecs = this.setProgramsWeeksRecords();//will also set column groups
        console.log('setProgramsWeeksRecords', gridRecs);

        this.openMarketsGrid.add(gridRecs);
        this.openMarketsGrid.resize();
        //hack this to change the outer column background to distinguish from weeks - W2ui dom has no way to isolate alone in CSS
        var programsColumnGroup = $("#openmarket_programs_column_inner").closest("td").addClass('openmarket-program-column-group');
        //console.log('programsColumnGroup', programsColumnGroup);

    },

    //SORTING strategy is to change activeInventory (markets sort in Markets and Weeks/Markets) and reset grids; store changed records separately from grids for reset states
    /*
    setMarketSort: function (isName) {
        this.isMarketSortName = isName;//need? just check vm?

        //check to see needs sorting if more than 1 market
        if (this.activeInventoryData.Markets.length > 1) {
            var inventory = this.changeInventoryDataForSort(this.activeInventoryData, isName);
            //console.log('setMarketSort Name', inventory, this.marketSortIndexMap);
            //this.recordLastScrollPosition(); position should reset to top on sort?
            this.refreshInventory(inventory, false, true);//pass last param true to checkEditSpots for reset
        }
    },

    
    //to change inventory Markets - Weeks Markets sorted
    //will need top process as filteredInventoryData or as activeInventoryData in the future; rank or name?
    changeInventoryDataForSort: function (inventory, isName) {

        var sortedProgramMarkets = this.getProgramsMarketsSort(inventory.Markets, isName);//also creates map
        inventory.Markets = sortedProgramMarkets;
        var $scope = this;
        $.each(inventory.Weeks, function (idx, week) {
            week.Markets = $scope.getWeekMarketsSort(week.Markets);
        });
        return inventory;
    },

    //get markets sorted; create index map for use in week markets
    getProgramsMarketsSort: function (markets, isName) {
        var sortedProgramMarkets = isName ? _.sortBy(markets, [function (o) { return o.MarketName; }]) : _.sortBy(markets, [function (o) { return o.MarketRank; }]);
        var map = [];
        for (var i = 0; i < sortedProgramMarkets.length; i++) {
            map[i] = markets.indexOf(sortedProgramMarkets[i]);
        }
        this.marketSortIndexMap = map;
        return sortedProgramMarkets;
    },

    //from mapped market programs - sort week markets by name/rank (indexes)
    getWeekMarketsSort: function (weekMarkets) {
        var sortedMarkets = [];
        for (var i = 0; i < weekMarkets.length; i++) {
            sortedMarkets[i] = weekMarkets[this.marketSortIndexMap[i]];
        }
        return sortedMarkets;
    },

    */


    //REVISE combine

    //PROGRAMS GRID - grouped

    //get a market row
    getProgramsMarketRow: function (market) {
        var ret = {
            recid: 'market_' + market.MarketId,
            isMarket: true,
            w2ui: { "style": "background-color: #dedede" },
            MarketName: market.MarketName,
            MarketSubscribers: market.MarketSubscribers,
            MarketRank: market.MarketRank
        };
        return ret;
    },

    //get a station row
    getProgramsStationRow: function (station) {
        var ret = {
            recid: 'station_' + station.StationCode,
            isStation: true,
            w2ui: { "style": "background-color: #E9E9E9" },
            Affiliation: station.Affiliation,
            StationName: station.LegacyCallLetters
        };
        return ret;
    },

    //prepare programs data for initial records by groupings
    prepareProgramsGridData: function (markets) {
        var $scope = this;
        var ret = [];
        $.each(markets, function (mIdx, market) {
            ret.push($scope.getProgramsMarketRow(market));
            //intercept this data if activeField sorting is on
            //var stationPrograms = $scope.checkStationsSortData(market.DisplayProposalStationPrograms);
            var stationPrograms = market.Stations;
            $.each(stationPrograms, function (sIdx, station) {
                ret.push($scope.getProgramsStationRow(station));
                $.each(station.Programs, function (pIdx, program) {
                    //this should now be unique ProgramId
                    //program.recid = program.ProgramId;
                    program.recid = 'program_' + program.ProgramId;
                    program.isProgram = true;
                    ret.push(program);
                });
            });
        });
        //var sums = $scope.getTotalRows(programsData.TargetUnitTotals, programsData.OverallTotals);
        //ret = ret.concat(sums);
        return ret;
    },

    //WEEKs and Column Groups
    //try to set each setGrid by inserting?
    setAllColumnGroups: function (weekGroups) {
        var $scope = this;
        //if weekGroups.length?
        //set as empty headet to match height of weeks: 26 px = 12px height + top7 bottom7 padding
        var colGroups = [{ span: 3, caption: '<div id="openmarket_programs_column_inner" style="height: 12px; padding-right: 5px; text-align: right;">' + this.weeksLength + ' WEEKS:</div>' }];
        //var colGroups = [{ span: 1, caption: 'Airing Time', master:true }, { span: 1, caption: 'Program', master:true }, { span: 1, caption: 'CPM', master:true }]; //this does not work
        $.each(weekGroups, function (idx, group) {
            colGroups.push({ caption: group, span: 3 })
        });

        this.openMarketsGrid.columnGroups = colGroups;
        //may need refresh?
    },

    //for use in grid column settings and for updates
    getWeekColumnGroup: function (week) {
        var convertedGoalImpressions = week.ImpressionsGoal ? week.ImpressionsGoal/1000 : null;
        var convertedWeekImpressions = week.ImpressionsTotal ? week.ImpressionsTotal/1000 : null;

        var goal = convertedGoalImpressions ? numeral(convertedGoalImpressions).format('0,0.[000]') : '-';
        var budget = week.Budget ? numeral(week.Budget).format('$0,0[.]00') : '-';
        var impressionsPercent = week.ImpressionsPercent ? week.ImpressionsPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";
        var budgetPercent = week.BudgetPercent ? week.BudgetPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";

        var vals = {
            quarter: week.QuarterText,
            week: week.Week,
            weekImpressions: convertedWeekImpressions ? numeral(convertedWeekImpressions).format('0,0.[000]') : "-",
            impressions: goal,
            impressionsPercent: impressionsPercent,
            weekBudget: week.BudgetTotal ? numeral(week.BudgetTotal).format('$0,0[.]00') : "-",
            budget: budget,
            budgetPercent: budgetPercent,
            impressionsMarginClass: week.ImpressionsMarginAchieved ? "label-success" : "label-danger",
            budgetMarginClass: week.BudgetMarginAchieved ? "label-danger" : "label-success"
        };

        return this.weekColumnGroupTpl(vals);
    },

    //update column group
    updateWeekColumnGroups: function (week) {
        var el = $('#openmarket_week_column_group_' + week.MediaWeekId);
        var summary = this.getWeekColumnGroup(week);
        if (el) el.html(summary);
    },



    //TBD - get the market part for a week record
    getWeekMarketItem: function (market, isHiatus) {
        var ret = {
            Cost: market.Cost,
            Spots: market.Spots,
            //EFF: market.EFF,
            Impressions: market.Impressions,
            isHiatus: isHiatus
        };
        //need to set style color per columns - use renderer?
        //ret.w2ui = isHiatus ? { "style": "color: #8f8f8f; background-color: #dedede;" } : { "style": "background-color: #dedede" };
        return ret;
    },

    getWeekProgramItem: function (program, week, market, indexes) {
        var ret;
        if (!program) {//is null so not available throughout; 
            ret = { active: false };//isProgram?
        } else {
            //pass week index or convention to identify?
            program.isHiatus = week.IsHiatus;
            program.active = true;//overall active
            program.isChanged = false;//overall edited
            program.initialSpots = program.Spots; //to determine if real change on edit/save

            program.MediaWeekId = week.MediaWeekId;
            //program.StationCode = station.StationCode;
            program.MarketId = market.MarketId;
            //for accessing stored data
            program.marketDataIdx = indexes[0]; //mIdx;
            program.stationDataIdx = indexes[1]; //sIdx;
            program.programDataIdx = indexes[2]; //pIdx;
            ret = program;
        }

        return ret;
    },

    setProgramsWeeksRecords: function () {

        var recs = this.prepareProgramsGridData(this.activeInventoryData.Markets);
        var $scope = this;
        var groups = [];  //test setting column groups
        $.each(this.activeInventoryData.Weeks, function (weekIdx, week) {
            var group = $scope.getWeekColumnGroup(week);
            //append a div with Id to use for updates later          
            groups.push('<div id="openmarket_week_column_group_' + week.MediaWeekId + '">' + group + '</div>');
            var markets = week.Markets;
            var recIdx = 0;
            $.each(markets, function (mIdx, market) {
                recs[recIdx]['week' + weekIdx] = $scope.getWeekMarketItem(market, week.IsHiatus);
                recIdx++;
                var stationPrograms = market.Stations;
                $.each(stationPrograms, function (sIdx, station) {
                    //station does not need week data?
                    recIdx++;
                    $.each(station.Programs, function (pIdx, program) {
                        recs[recIdx]['week' + weekIdx] = $scope.getWeekProgramItem(program, week, market, [mIdx, sIdx, pIdx]);
                        recIdx++
                    });
                });
            });
        });
        this.setAllColumnGroups(groups);
        return recs;
    },


    /////////////original








    /*
    //tbd- editing etc
    setWeekGridEvents: function (grid) {
        grid.on('click', this.onSpotsGridClick.bind(this, grid));
    },

    //SPOT EDITING RELATED - adapt from original implementation
    //Need to separate storage of changes on grid records as may be reset on sort/filter; store separately and reset as needed (grid records now primarilly set for display state)

    //user clicks grid: enable edit if spots column and not active/already editing
    onSpotsGridClick: function (grid, event) {

        if (event.column === null || this.activeEditingRecord) {
            return;
        }

        if (event.column == 0) {//spots column
            var record = grid.get(event.recid);
            //record needs to be program with a Id (saved)
            if (record && record.isProgram && record.active && !record.isHiatus) {
                console.log('edit grid click', record, event);
                this.setEditSpot(record, event, grid);
            } else {
                return;
            }

        } else {
            return;
        }
    },

    //set spot in row for inline editing; handle eveents
    setEditSpot: function (record, clickevent, grid) {
        //make selector specific to grid?
        var editTarget = $('#program_week_spot_' + record.recid);

        if (editTarget.length && !editTarget.hasClass('is-editing')) {
            this.activeEditingRecord = record;
            editTarget.addClass("is-editing");
            var $scope = this;
            //timeout to prevent blur event call initially
            //TODO change this out to new w2ui format used elsewhere
            setTimeout(function () {
                var input = editTarget.find(".edit-input");
                input.show();
                var spotval = record.Spots || 0;
                input.val(spotval);
                input.focus();

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
                    // setTimeout(function () {
                    this.onEditSpot(editTarget, input, record, grid);
                    //input.off("keyup");
                    input.off("keypress");
                    input.off("keydown");
                    input.off("blur");
                    // }.bind($scope), 50);
                }.bind($scope));

            }, 300);

            //input.focus();//focus here else cause issues calling event too soon - but only works chrome
        }

    },

    //after a spot is edited send to api; handle return to update row and states
    //set back if user sets no value
    onEditSpot: function (editTarget, input, record, grid) {
        var spotVal = parseInt(input.val());
        var currentVal = record.Spots;
        // console.log('onEditSpot', spotVal);
        //if val is same as record or empty then end the edit - no API call//else make the api call and end the edit after success/failure
        if ((spotVal || (spotVal === 0)) && (spotVal !== currentVal)) {
            //console.log('call api');
            var $scope = this;
            //temporary
            this.updateEditSpot(record, spotVal, grid);
            this.endEditSpot(editTarget, input);

        } else {
            this.endEditSpot(editTarget, input);
        }
    },


    //set active data with change, record with changes - call update totals
    //check of actual change versus initial spot
    updateEditSpot: function (rec, spotVal, grid) {
        var $scope = this;

        var weekDataItem = $scope.activeInventoryData.Weeks[rec.weekGridIndex];
        var marketDataItem = weekDataItem.Markets[rec.marketDataIdx];
        var stationDataItem = marketDataItem.Stations[rec.stationDataIdx];
        var programDataItem = stationDataItem.Programs[rec.programDataIdx];

        if (programDataItem) {
            programDataItem.Spots = spotVal;
            var changes = { Spots: spotVal, isChanged: true };

            if (spotVal == rec.initialSpots) {
                changes.isChanged = false;
            }

            grid.set(rec.recid, changes);

            //separated storage with check if changed
            this.storeEditedSpotRecord(rec);

            // update entry on original inventory
            var originalWeek = _.find($scope.ProposalView.openMarketInventory.Weeks, ['MediaWeekId', weekDataItem.MediaWeekId]);
            $.each(originalWeek.Markets, function (mIdx, market) {
                $.each(market.Stations, function (sIdx, station) {
                    $.each(station.Programs, function (pIdx, program) {
                        if (program) {
                            if (program.ProgramId == programDataItem.ProgramId) {
                                program.Spots = rec.Spots;
                            }
                        }
                    });
                });
            });

            //general flag to look for changes
            grid.checkChanges = true;
            var params = $scope.activeInventoryData;
            this.ProposalView.controller.apiUpdateInventoryOpenMarketTotals(params, function (response) {
                $scope.onUpdateEditSpot(response, rec, grid);
            });
        }
    },

    //update totals, rows etc - set new activeInventoryData
    onUpdateEditSpot: function (inventoryData, rec, grid) {
        var week = inventoryData.Weeks[rec.weekGridIndex];
        var marketDataItem = week.Markets[rec.marketDataIdx];
        var programDataItem = marketDataItem.Stations[rec.stationDataIdx].Programs[rec.programDataIdx];

        //update header totals
        this.OpenMarketVM.setInventory(inventoryData, this.isReadOnly);

        //update grid week header
        this.updateWeekColumnGroups(week);

        //update market row
        grid.set('market_week_' + rec.MarketId, { Spots: marketDataItem.Spots, Cost: marketDataItem.Cost, Impressions: marketDataItem.Impressions });

        //update program row (spots already set)
        grid.set(rec.recid, { Cost: programDataItem.Cost, TotalImpressions: programDataItem.TotalImpressions });

        //reset with new data
        this.activeInventoryData = inventoryData;
    },

    //end spot editing; hide input; remove class and allow editing again
    endEditSpot: function (editTarget, input) {
        input.hide();
        editTarget.removeClass("is-editing");
        this.activeEditingRecord = null;
    },

    ///////////// EDITNG STORAGE/CHECK/RESET

    // update 'activeEditWeekRecords' array
    storeEditedSpotRecord: function (rec) {
        var match = _.find(this.activeEditWeekRecords, ['recid', rec.recid]);

        if (match) {
            if (match.isChanged) {
                var index = _.indexOf(this.activeEditWeekRecords, match);
                this.activeEditWeekRecords.splice(index, 1, rec);
            }
        } else {
            this.activeEditWeekRecords.push(rec);
        }
    },

    //on grid refreshes (sort, onClearInventoryfilter, etc) set back the states of edited records - if filtered out of grids will not change
    resetEditedGridRecords: function () {
        if (this.checkUnsavedSpots) {
            var $scope = this;
            $.each(this.activeEditWeekRecords, function (idx, programRec) {
                var grid = $scope.inventoryWeekGrids[programRec.weekGridIndex];
                var gridRec = grid.get(programRec.recid);
                if (gridRec) {
                    //restore initialSpots, isChanged
                    grid.set(gridRec.recid, { initialSpots: programRec.initialSpots, isChanged: programRec.isChanged }); //force true?
                }
            });
        }
    },
    */
    ///////////// SAVE INVENTORY
    /*
    checkUnsavedSpots: function () {
        return this.activeEditWeekRecords.length >= 1;
    },

    getParamsForSave: function () {
        var params = {
            ProposalVersionDetailId: this.activeInventoryData.DetailId,
            Weeks: []
        };

        if (this.checkUnsavedSpots) {
            //partitions: breaks into week groups map and then values to arrays [[program, program], [program]]
            //use values array or each on the groupBy?
            var partitionedByWeek = _.values(_.groupBy(this.activeEditWeekRecords, 'MediaWeekId'));
            $.each(partitionedByWeek, function (pidx, items) {
                var week = {
                    MediaWeekId: items[0].MediaWeekId,
                    Programs: []
                };
                $.each(items, function (ridx, rec) {
                    //should be all isChanged
                    if (rec.isChanged) {
                        week.Programs.push({ ProgramId: rec.ProgramId, Spots: rec.Spots, Impressions: rec.TotalImpressions });
                    }
                });
                params.Weeks.push(week);
            });
        }

        params.Filter = this.ProposalView.openMarketInventory.Filter;
        return params;
    },

    //handle after save apply - refresh based on potential sorting; clear activeEditWeekRecords, filtering etc
    onAfterSaveApply: function (detailId) {
        var $scope = this;
        $scope.recordLastScrollPosition();
        $scope.ProposalView.controller.apiGetProposalOpenMarketInventory(detailId, function (inventory) {
            //check if needs sorting (filtering future)
            if ($scope.isMarketSortName) {
                inventory = $scope.changeInventoryDataForSort(inventory, true);
            }
            this.activeEditWeekRecords = [];//clear
            $scope.refreshInventory(inventory, false, false);
        }, true);
    },

    //save from apply or save context - 
    //if apply - record scroll positions, call api to refresh, and reset
    //apply sorting; revise to handle future filtering/post processing
    saveInventory: function (isApply) {
        var $scope = this;

        var continueSaveFn = function () {
            var request = $scope.getParamsForSave();

            $scope.ProposalView.controller.apiSaveInventoryOpenMarket(request, function (inventory) {
                $scope.activeEditWeekRecords = [];

                if (isApply) {
                    $scope.recordLastScrollPosition();

                    if ($scope.isMarketSortName) {
                        inventory = $scope.changeInventoryDataForSort(inventory, true);
                    }

                    $scope.refreshInventory(inventory, true, false, true);
                } else {
                    $scope.showModal(true); //close open market modal
                }

                util.notify('Inventory saved successfully', 'success');
            });
        };

        if ($scope.OpenMarketVM.hasFiltersApplied()) {
            util.confirm('Filters are set', 'You have filtered your Proposal. The totals displayed are for the filtered items only. Are you sure you would like to save?', continueSaveFn, null, 'Save');
        } else {
            continueSaveFn();
        }
    },

    */


    //REVISE
    //clear inventory ; reset
    onClearInventory: function (reset) {

        this.activeInventoryData = null;
        this.bypassCheckSave = false;
        if (reset) {
            this.isMarketSortName = false;
            this.marketSortIndexMap = [];
            this.OpenMarketVM.setSortByMarketName(false);
        }
    },

    //modal hide event - check unsaved if not read only
    onCheckInventoryCancel: function (e) {
        /*
        if (!this.isReadOnly && !this.bypassCheckSave && this.checkUnsavedSpots()) {
            var $scope = this;
            e.preventDefault();
            e.stopImmediatePropagation();
            var continueFn = function () {
                $scope.bypassCheckSave = true;
                $scope.showModal(true);//hide modal
            };
            util.confirm('You have unsaved spots allocated.', 'Select "Continue" to loose the spots. Select "Cancel" to return to the grid.', continueFn);
            return false;
        }

        this.bypassCheckSave = false;
        */

    },

    /*
    hasFiltersApplied: function () {
        return this.FilterVM.criteriaList().length > 0 || this.OpenMarketVM.selectedSpotFilterOption() != 1;
    },

    applyFilter: function () {
        var $scope = this;

        var inventoryWithFilters = $scope.ProposalView.openMarketInventory;

        // update request with filter criteria from FilterVM and spot filter dropdown
        inventoryWithFilters.Filter = $scope.FilterVM.formattedFilterCriteria();

        if (inventoryWithFilters.Filter) {
            inventoryWithFilters.Filter.SpotFilter = $scope.OpenMarketVM.selectedSpotFilterOption();
        } else {
            inventoryWithFilters.Filter = {
                SpotFilter: $scope.OpenMarketVM.selectedSpotFilterOption()
            }
        }

        $scope.ProposalView.controller.apiApplyOpenMarketInventoryFilter(inventoryWithFilters, function (filteredInventory) {
            $scope.refreshInventory(filteredInventory, true, true, true);

            // filter indicator
            $scope.OpenMarketVM.hasFiltersApplied($scope.hasFiltersApplied());

            $scope.FilterVM.showModal(false);
        });
    }

    */
});