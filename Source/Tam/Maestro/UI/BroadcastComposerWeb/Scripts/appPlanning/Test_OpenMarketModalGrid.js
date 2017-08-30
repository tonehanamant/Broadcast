//test open market as one combined grid

var Test_OpenMarketModalGrid = BaseView.extend({
    activeInventoryData: null,
    //separate storage of edited records so can reset grid record state after changes (sort, filter); use for saving independent of what is displayed in grids
    //activeEditWeekRecords: [], //flat array all weeks
    //activeDetailSet: null,
    isActive: false,
    openMarketsGrid: null,
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

        //self.OpenMarketVM = new ProposalDetailOpenMarketViewModel(this);
        //ko.applyBindings(this.OpenMarketVM, document.getElementById("proposal_openmarket_view"));

        //self.CriteriaBuilderVM = new CriteriaBuilderViewModel(self);
        //ko.applyBindings(this.CriteriaBuilderVM, document.getElementById("criteria_builder_modal"));

        //self.FilterVM = new FilterViewModel(self);
        //ko.applyBindings(this.FilterVM, document.getElementById("filter_modal"));
        //self.initGrid();
        self.initModal();
    },

    initGrid: function () {
        this.openMarketsGrid = $('#test_open_market_grid').w2grid(PlanningConfig.getTestOpenMarketGridCfg(this));
        console.log(this.openMarketsGrid);
    },



    //MODAL//
    initModal: function () {
        this.$Modal = $('#test_proposal_detail_openmarket_modal');
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
        if (isHide) {
            this.$Modal.modal('hide');
        } else {
            this.$Modal.modal('show');
        }

        //this.FilterVM.clearFilters();
        //this.OpenMarketVM.selectedSpotFilterOption(1);

        //this.activeEditWeekRecords = [];
    },

    //TBD
    refreshInventory: function (inventory, reset, checkEdits, keepActiveSet) {
        this.onClearInventory(reset, keepActiveSet);
        this.setInventory(this.activeDetailSet, inventory, this.isReadOnly, true, checkEdits);
    },

    //REVISE
    //set allinitial inventory data
    setInventory: function (detailSet, inventory, readOnly, isRefresh, checkEdits) {
        this.activeInventoryData = util.copyData(inventory, null, null, true);
        this.activeDetailSet = detailSet;
        this.isReadOnly = readOnly;
        //this.OpenMarketVM.setInventory(inventory, readOnly);
        //this.FilterVM.setAvailableValues(inventory.DisplayFilter);
        //this.CriteriaBuilderVM.ProgramNamesOptions(inventory.RefineFilterPrograms);

        if (isRefresh) {
            this.onSetInventory(checkEdits);
        } else {
            this.showModal();
        }
    },

    //REVISE
    //after modal shown/refresh - set inventory views
    onSetInventory: function (checkEdits) {
        //initial render after shown
        if (!this.isActive) {
           this.initGrid();
            this.isActive = true;
        }
        this.setGrid(this.activeInventoryData.Markets);

        //will only reset if recorded
        //this.scrollToLastPosition();
        if (checkEdits) {
           // this.resetEditedGridRecords();
        }
    },

    //test
    setGrid: function (markets) {
        console.log(this.openMarketsGrid);
        var gridData = this.prepareProgramsGridData(markets);
        this.openMarketsGrid.clear();
        this.openMarketsGrid.add(gridData);
        this.openMarketsGrid.resize();
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

    //prepare programs data for records by groupings - will need to handle sorting in future (no totals here)
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

    //Programs grid data at left - grouped structure - market/station - tbd - handle states/sorting/reset etc for future
    setProgramsGrid: function (markets) {
        var gridData = this.prepareProgramsGridData(markets);
        this.programsGrid.clear();
        this.programsGrid.add(gridData);
        this.programsGrid.resize();
    },

    //WEEK GRIDS - multiple grouped

    //for use in grid column settings and for updates
    getWeekColumnGroup: function (week) {
        var goal = week.ImpressionsGoal ? numeral(week.ImpressionsGoal).format('0,0.[000]') : '-';
        var budget = week.Budget ? numeral(week.Budget).format('$0,0[.]00') : '-';
        var impressionsPercent = week.ImpressionsPercent ? week.ImpressionsPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";
        var budgetPercent = week.BudgetPercent ? week.BudgetPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";

        var vals = {
            quarter: week.QuarterText,
            week: week.Week,
            weekImpressions: week.ImpressionsTotal ? numeral(week.ImpressionsTotal).format('0,0.[000]') : "-",
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

    //insert multiple grids for scrolling structure - headers and data
    //handle 1 or 2 contact columns
    insertWeekGrids: function (weeks) {
        var $scope = this;
        $.each(weeks, function (idx, week) {
            var id = idx + 1;
            var group = $scope.getWeekColumnGroup(week);
            //append a div with Id to use for updates later
            group = '<div id="openmarket_week_column_group_' + week.MediaWeekId + '">' + group + '</div>';

            //elements
            var el = 'openmarket_week_grid_' + id;
            var el2 = 'openmarket_week_header_grid_' + id; //header
            $('#openmarket_weeks').append('<div class="openmarket-week-item"><div id="' + el + '"></div></div>');
            $('#openmarket_weeks_headers').append('<div class="openmarket-week-item"><div id="' + el2 + '"></div></div>');
            //set bottom scroller
            $('#openmarket_weeks_bottom').append('<div class="openmarket-week-item"></div>');
            //data grid
            var grid = $('#' + el).w2grid(PlanningConfig.getOpenMarketWeekGridCfg(id, null, this));

            if (grid) {
                //header grid with grouping
                var grid2 = $('#' + el2).w2grid(PlanningConfig.getOpenMarketWeekGridCfg(id, group, this));//header with group
                //set properties on grid for access
                grid.weekGridId = id;
                grid.weekGridIndex = idx;
                grid.mediaWeekId = week.MediaWeekId;
                grid.isHiatus = week.IsHiatus;
                grid.checkChanges = false;
                //set week grid
                $scope.setWeekGrid(grid, week, week.Markets);//need overall week as is nested structure
                //set week grid events
                $scope.setWeekGridEvents(grid);
                //store grids
                $scope.inventoryWeekGrids.push(grid);
                $scope.inventoryWeekHeaderGrids.push(grid2);
            }

        });

    },

    //get week market row (contains data)
    getWeekMarketRow: function (market, isHiatus) {
        var ret = {
            recid: 'market_week_' + market.MarketId,
            isMarket: true,
            // w2ui: { "style": "background-color: #dedede" },
            //TBD BE
            Cost: market.Cost,
            Spots: market.Spots,
            EFF: market.EFF,
            Impressions: market.Impressions
        };
        ret.w2ui = isHiatus ? { "style": "color: #8f8f8f; background-color: #dedede;" } : { "style": "background-color: #dedede" };
        return ret;
    },

    //get week station row
    getWeekStationRow: function (station) {
        var ret = {
            recid: 'station_Week_' + station.StationCode,
            isStation: true,
            w2ui: { "style": "background-color: #E9E9E9" }
        };
        return ret;
    },

    prepareWeekGridData: function (markets, week, grid) {
        markets = util.copyArray(markets);//needed?
        var $scope = this;
        var ret = [];
        $.each(markets, function (mIdx, market) {
            ret.push($scope.getWeekMarketRow(market, week.IsHiatus));
            //intercept this data if activeField sorting is on
            //var stationPrograms = $scope.checkStationsSortData(market.DisplayProposalStationPrograms);
            var stationPrograms = market.Stations;
            $.each(stationPrograms, function (sIdx, station) {
                ret.push($scope.getWeekStationRow(station));
                $.each(station.Programs, function (pIdx, program) {
                    var style = week.IsHiatus ? { "style": "color: #8f8f8f;" } : {};
                    if (!program) {//is null so not available throughout; no ProgramId so use index hybrid ID
                        var uniqueId = station.StationCode + pIdx;
                        var slotId = 'program_week_' + week.MediaWeekId + '_notavailable_' + uniqueId;
                        ret.push({ recid: slotId, active: false, isProgram: true, w2ui: style });//isProgram?
                    } else {
                        //TBD - this may effect spot editing
                        //programId should be unique now but could be same in another week
                        program.recid = 'program_week_' + week.MediaWeekId + '_' + program.ProgramId;
                        //state
                        program.isProgram = true;
                        program.isHiatus = week.IsHiatus;
                        program.active = true;//overall active
                        program.isChanged = false;//overall edited
                        program.initialSpots = program.Spots; //to determine if real change on edit/save
                        //grid identifiers
                        program.weekGridId = grid.weekGridId;
                        program.weekGridIndex = grid.weekGridIndex;
                        program.MediaWeekId = week.MediaWeekId;
                        program.StationCode = station.StationCode;
                        program.MarketId = market.MarketId;
                        //for accessing stored data
                        program.marketDataIdx = mIdx;
                        program.stationDataIdx = sIdx;
                        program.programDataIdx = pIdx;
                        program.w2ui = style;
                        ret.push(program);
                    }
                });
            });
        });
        return ret;
    },

    //set a week from nested structure to grouped and spacer
    setWeekGrid: function (grid, week, markets) {
        var gridData = this.prepareWeekGridData(markets, week, grid);
        //grid.clear();
        grid.add(gridData);
        grid.resize();
    },


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


    //REVISE - either clear/update columns on load OR  remove altogehter
    //clear inventory at modal hidden and destroy week grids, etc
    onClearInventory: function (reset, clearActiveDetailSet) {
        //destroy or clear?

        /*
        this.programsGrid.clear(); //clear programs

        $.each(this.inventoryWeekGrids, function (idx, grid) {
            grid.destroy();
        });

        $('#openmarket_weeks').empty();
        $.each(this.inventoryWeekHeaderGrids, function (idx, grid) {
            grid.destroy();
        });

        $('#openmarket_weeks_headers').empty();
        $('#openmarket_weeks_bottom').empty();

        if (clearActiveDetailSet)
            this.activeDetailSet = null;

        this.activeInventoryData = null;
        this.inventoryWeekGrids = [];
        this.inventoryWeekHeaderGrids = [];
        this.bypassCheckSave = false;
        if (reset) {
            this.isMarketSortName = false;
            this.marketSortIndexMap = [];
            this.OpenMarketVM.setSortByMarketName(false);
        }
        */
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