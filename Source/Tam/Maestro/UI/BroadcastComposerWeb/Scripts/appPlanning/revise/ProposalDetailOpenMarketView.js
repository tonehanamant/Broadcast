//REVISED version for open from React - activeDetail set will be params, no proposalView, use PlanningController and params

//Proposal Detail Open Market View - handle the Open Market Planning for active Detail Set with programs and weeks
//revision to 1 combined grid with dynamic columns/weeks
var ProposalDetailOpenMarketView = BaseView.extend({
    activeInventoryData: null,
    storedOpenMarketInventory: null,  //moving here for proposal view - pristine original data
    //separate storage of edited records so can reset grid record state after changes (sort, filter); use for saving independent of what is displayed in grids
    activeProgramEditItems: [], //array of pending edit items
    activeDetailSet: null, //now params - modal, detailId, proposalId, readOnly
    openMarketsGrid: null,
    weeksLength: null,
    PlanningController: null,
    //ProposalView: null,
    OpenMarketVM: null,
    CriteriaBuilderVM: null,
    FilterVM: null,
    isReadOnly: false,
    bypassCheckSave: false,
    $Modal: null,
    scrollPositions: null, //stored positions for resetting user poistion after refresh
    weekColumnGroupTpl: _.template('<div class="openmarket-column-group"><div>${ quarter } Week of ${ week }</div>' +
                                   '<div class="cadent-dk-blue goals">' +
                                        '<i class="fa fa-bullhorn" aria-hidden="true"></i> ${ weekImpressions }/${ impressions } <span class="label ${ impressionsMarginClass } custom-label">${ impressionsPercent }%</span>&nbsp;&nbsp;&nbsp;&nbsp;' +
                                        '<i class="fa fa-money" aria-hidden="true"></i> ${ weekBudget }/${ budget } <span class="label ${ budgetMarginClass } custom-label">${budgetPercent} %</span>' +
                                   '</div>'),
    isMarketSortName: false,
    marketSortIndexMap: [], //index of active markets sort - determined from programs market data; used for sorting weeks (name or rank)

    //change to use controller
    initView: function (controller) {
        var self = this;

        self.PlanningController = controller;

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

        //hidden event - revised
        this.$Modal.on('hidden.bs.modal', this.onHideInventory.bind(this));

        //shown event
        this.$Modal.on('shown.bs.modal', this.onSetInventory.bind(this));
    },

    //revise - hide will go back to React - call controller - in clear function
    showModal: function (isHide) {
        var $scope = this;

        if (isHide) {
            $scope.$Modal.modal('hide');
        } else {
            $scope.$Modal.modal('show');
        }
    },

    //go back to react proposal
    returnToProposal: function (proposalId) {
        this.PlanningController.editProposal(proposalId);
    },


    //store grid scroll positions
    recordLastScrollPosition: function () {
        this.scrollPositions = {
            lastScrollTop: $("#grid_OpemMarketGrid_records").scrollTop(),
            lastScrollLeft: $("#grid_OpemMarketGrid_records").scrollLeft(),
        };

    },

    //scroll vertically and horizontally based on the last position (i.e. after a refresh)
    scrollToLastPosition: function () {
        var positions = this.scrollPositions;

        if (positions) {
            //console.log('scrollToLastPosition', positions);
            if (positions.lastScrollTop) $("#grid_OpemMarketGrid_records").scrollTop(positions.lastScrollTop);
            if (positions.lastScrollLeft) $("#grid_OpemMarketGrid_records").scrollLeft(positions.lastScrollLeft);
            this.scrollPositions = null;
        }

    },

    //set grid with dynamic columns via the weeks length
    //recreate on open or adjust columns in reload
    //on load
    initGrid: function () {
        if (!this.openMarketsGrid) {
            this.openMarketsGrid = $('#openmarkets_grid').w2grid(PlanningConfig.getOpenMarketGridCfg(this, this.weeksLength));
            //set up spot events dynamic reuse - do not bind as need the context of the jq el
            var $scope = this;
            $('#openmarkets_grid').on('click', '.program_week_spot_click', function (evt) {
                var recid = $(this).data('recid');
                var weekIdx = $(this).data('weekidx');
                $scope.onClickEditSpot(recid, weekIdx, evt)
            });
        } else {
            var columns = PlanningConfig.getOpenMarketGridCfg(this, this.weeksLength).columns;
            this.openMarketsGrid.columns = columns;
            //normally would refresh but wait for setGrid to handle
        }
    },


    //revise to store storedOpenMarketInventory here instead of removed ProposalView - params detailSet is from planning
    //separate mechanism for initial loads - init/reset Grid columns with weeks each time
    loadInventory: function (detailSet, inventory) {
        this.activeDetailSet = detailSet;
        this.storedOpenMarketInventory = inventory;
        this.isReadOnly = detailSet.readOnly;
        this.weeksLength = inventory.Weeks.length;
        this.initGrid();

        this.FilterVM.clearFilters();
        this.OpenMarketVM.selectedSpotFilterOption(1);
        this.activeProgramEditItems = [];
        this.scrollPositions = null;
        this.setInventory(inventory);
    },

    //reset inventory - filter, refine, sorts, etc
    refreshInventory: function (inventory, reset, checkEdits, checkSorts) {
        this.onClearInventory(reset);
        if (checkSorts) {
            if (this.isMarketSortName) {
                inventory = this.changeInventoryDataForSort(inventory, true);
            }
        }
        this.setInventory(inventory, true, checkEdits);
    },

    //revise store separate copy of inventory whcihc is curentlty stored in removed ProposalView
    //set all initial inventory data
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

    //after modal shown/refresh - set inventory views
    onSetInventory: function (checkEdits) {
        this.setGrid();
        //will only reset if recorded
        this.scrollToLastPosition();
        if (checkEdits) {
            this.resetEditedGridRecords();
        }
    },

    //set Grid - combined
    setGrid: function () {
        this.openMarketsGrid.clear();
        var gridRecs = this.setProgramsWeeksRecords();//will also set column groups
        //console.log('setProgramsWeeksRecords', gridRecs);

        this.openMarketsGrid.add(gridRecs);
        this.openMarketsGrid.resize();
        //hack this to change the outer column background to distinguish from weeks - W2ui dom has no way to isolate alone in CSS
        var programsColumnGroup = $("#openmarket_programs_column_inner").closest("td").addClass('openmarket-program-column-group');
        //console.log('programsColumnGroup', programsColumnGroup);

    },

    //PROGRAMS section - grouped

    //get a market row
    getProgramsMarketRow: function (market) {
        var ret = {
            recid: 'market_' + market.MarketId,
            MarketId: market.MarketId,
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

            var stationPrograms = market.Stations;
            $.each(stationPrograms, function (sIdx, station) {
                ret.push($scope.getProgramsStationRow(station));
                $.each(station.Programs, function (pIdx, program) {
                    //this should now be unique ProgramId
                    program.recid = program.ProgramId;
                    //program.recid = 'program_' + program.ProgramId;
                    program.isProgram = true;
                    program.MarketId = market.MarketId;
                    ret.push(program);
                });
            });
        });
        return ret;
    },

    //WEEKs and Column Groups
    setAllColumnGroups: function (weekGroups) {
        var $scope = this;
        //if weekGroups.length?
        //set as empty headet to match height of weeks: 26 px = 12px height + top7 bottom7 padding
        var colGroups = [{ span: 2, caption: '<div id="openmarket_programs_column_inner" style="height: 12px; padding-right: 5px; text-align: right;">' + this.weeksLength + ' WEEKS:</div>' }];
        //var colGroups = [{ span: 1, caption: 'Airing Time', master:true }, { span: 1, caption: 'Program', master:true }, { span: 1, caption: 'CPM', master:true }]; //this does not work
        $.each(weekGroups, function (idx, group) {
            colGroups.push({ caption: group, span: 4 })
        });

        this.openMarketsGrid.columnGroups = colGroups;
        //needs refresh but the final resize should handle
    },

    //for use in grid column settings and for updates
    getWeekColumnGroup: function (week) {
        var convertedGoalImpressions = week.ImpressionsGoal ? week.ImpressionsGoal / 1000 : null;
        var convertedWeekImpressions = week.ImpressionsTotal ? week.ImpressionsTotal / 1000 : null;

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

    //get the market part for a week record
    getWeekMarketItem: function (market, isHiatus) {
        var ret = {
            Cost: market.Cost,
            Spots: market.Spots,
            //EFF: market.EFF,
            Impressions: market.Impressions,
            isHiatus: isHiatus
        };
        return ret;
    },

    //program with indexes: [mIdx, sIdx, pIdx, weekIdx]
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
            program.weekIdx = indexes[3]; //weekIdx
            ret = program;
        }

        return ret;
    },

    //provides all combined methodas to get full records for grid
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
                        recs[recIdx]['week' + weekIdx] = $scope.getWeekProgramItem(program, week, market, [mIdx, sIdx, pIdx, weekIdx]);
                        recIdx++
                    });
                });
            });
        });
        this.setAllColumnGroups(groups);
        return recs;
    },

    //SORTING strategy is to change activeInventory (markets sort in Markets and Weeks/Markets) and reset grids; store changed records separately from grids for reset states

    setMarketSort: function (isName) {
        this.isMarketSortName = isName;

        //check to see needs sorting if more than 1 market
        if (this.activeInventoryData.Markets.length > 1) {
            var inventory = this.changeInventoryDataForSort(this.activeInventoryData, isName);
            //console.log('setMarketSort Name', inventory, this.marketSortIndexMap);
            //this.recordLastScrollPosition(); position should reset to top on sort?
            this.refreshInventory(inventory, false, true);//pass param true to checkEditSpots for reset
        }
    },


    //to change inventory Markets - Weeks Markets sorted

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


    //EDITING SPOTS

    //from program_week_spot_click class on the rendered element - see initGrid

    onClickEditSpot: function (recid, weekIdx, evt) {
        if (this.activeEditingRecord) return; //prevent further editing while processing?
        //get record and process
        var record = this.openMarketsGrid.get(recid);
        if (record && record.isProgram) {
            var week = record['week' + weekIdx];
            if (week && week.active && !week.isHiatus) {
                //console.log('onClickEditSpot', week, record, evt);
                this.setEditSpot(record, week, evt);
            }

        } else {
            return;
        }
    },


    //set spot in row for inline editing; handle eveents
    setEditSpot: function (record, week, evt) {

        var editTarget = $('#program_week_spot_' + record.recid + '_' + week.weekIdx);

        if (editTarget.length && !editTarget.hasClass('is-editing')) {
            this.activeEditingRecord = record;
            editTarget.addClass("is-editing");
            var $scope = this;
            //timeout to prevent blur event call initially
            //TODO change this out to new w2ui format used elsewhere?
            setTimeout(function () {
                var input = editTarget.find(".edit-input");
                input.show();
                var spotval = week.Spots || 0;
                input.val(spotval);
                input.focus();

                input.keypress(function (event) {
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
                    this.onEditSpot(editTarget, input, record, week);
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
    onEditSpot: function (editTarget, input, record, week) {
        var spotVal = parseInt(input.val());
        var currentVal = week.Spots;
        // console.log('onEditSpot', spotVal);
        //if val is same as record or empty then end the edit - no API call//else make the api call and end the edit after success/failure
        //check initialSpots here?
        if ((spotVal || (spotVal === 0)) && (spotVal !== currentVal)) {
            var $scope = this;
            this.updateEditSpot(record, spotVal, week);
            this.endEditSpot(editTarget, input);

        } else {
            this.endEditSpot(editTarget, input);
        }
    },

    //update the spot record and stored data
    updateEditSpot: function (rec, spotVal, week) {
        var $scope = this;

        var weekDataItem = $scope.activeInventoryData.Weeks[week.weekIdx];
        var marketDataItem = weekDataItem.Markets[week.marketDataIdx];
        var stationDataItem = marketDataItem.Stations[week.stationDataIdx];
        var programDataItem = stationDataItem.Programs[week.programDataIdx];

        if (programDataItem) {
            programDataItem.Spots = spotVal;
            var isChanged = (spotVal != week.initialSpots);
            var changes = {};
            changes['week' + week.weekIdx] = { Spots: spotVal, isChanged: isChanged };

            $scope.openMarketsGrid.set(rec.recid, changes);

            //separated storage with check if changed
            this.storeEditedSpotWeek(rec, week);
            //console.log('updateEditSpot', rec, this.activeProgramEditItems);
            // update entry on original inventory
            var originalWeek = _.find($scope.storedOpenMarketInventory.Weeks, ['MediaWeekId', weekDataItem.MediaWeekId]);
            $.each(originalWeek.Markets, function (mIdx, market) {
                $.each(market.Stations, function (sIdx, station) {
                    $.each(station.Programs, function (pIdx, program) {
                        if (program) {
                            if (program.ProgramId == programDataItem.ProgramId) {
                                program.Spots = week.Spots;
                            }
                        }
                    });
                });
            });

            var params = $scope.activeInventoryData;
            this.PlanningController.apiUpdateInventoryOpenMarketTotals(params, function (response) {
                $scope.onUpdateEditSpot(response, rec, week);
            });
        }
    },

    //update totals, rows etc - set new activeInventoryData
    onUpdateEditSpot: function (inventoryData, rec, week) {
        var weekDataItem = inventoryData.Weeks[week.weekIdx];
        var marketDataItem = weekDataItem.Markets[week.marketDataIdx];
        var programDataItem = marketDataItem.Stations[week.stationDataIdx].Programs[week.programDataIdx];

        //update header totals
        this.OpenMarketVM.setInventory(inventoryData, this.isReadOnly);

        //update grid week header
        this.updateWeekColumnGroups(weekDataItem);

        //update market row
        var marketChanges = {};
        marketChanges['week' + week.weekIdx] = { Spots: marketDataItem.Spots, Cost: marketDataItem.Cost, Impressions: marketDataItem.Impressions };
        this.openMarketsGrid.set('market_' + rec.MarketId, marketChanges);

        //update program row (spots already set)
        var weekProgramChanges = {};
        weekProgramChanges['week' + week.weekIdx] = { Cost: programDataItem.Cost, TotalImpressions: programDataItem.TotalImpressions };
        this.openMarketsGrid.set(rec.recid, weekProgramChanges);

        //reset with new data
        this.activeInventoryData = inventoryData;
    },

    //end spot editing; hide input; remove class and allow editing again
    endEditSpot: function (editTarget, input) {
        input.hide();
        editTarget.removeClass("is-editing");
        this.activeEditingRecord = null;
    },

    // change to more specific item with weeks (not overall record)
    storeEditedSpotWeek: function (rec, week) {
        var matchItem = _.find(this.activeProgramEditItems, ['recid', rec.recid]);
        var change = week.isChanged;

        if (matchItem) {
            var matchWeek = matchItem.weeks['week' + week.weekIdx];
            //if matchWeek and new week is changed - update; else delete; if not matchWeek add it to matchItem
            if (matchWeek && change) {
                matchWeek = week;
            } else if (matchWeek) {
                delete matchItem.weeks['week' + week.weekIdx];
                //now check matchItem overall for size - if empty remove from array
                if (_.size(matchItem.weeks) === 0) {
                    _.pull(this.activeProgramEditItems, matchItem);
                }
            } else {
                matchItem.weeks['week' + week.weekIdx] = week;
            }

            //new so add to activeProgramEditItems
        } else {
            var editItem = {
                recid: rec.recid,
                weeks: {}
            };
            editItem.weeks['week' + week.weekIdx] = week;
            this.activeProgramEditItems.push(editItem);
        }
    },

    //on grid refreshes (sort, onClearInventoryfilter, etc) set back the states of edited records - if filtered out of grid will not change
    resetEditedGridRecords: function () {
        if (this.checkUnsavedSpots) {
            var $scope = this;
            $.each(this.activeProgramEditItems, function (idx, programItem) {
                var gridRec = $scope.openMarketsGrid.get(programItem.recid);
                if (gridRec) {
                    //restore initialSpots, isChanged
                    //get all changes from programItem weeks
                    var changes = {};
                    $.each(programItem.weeks, function (key, wk) {
                        changes[key] = { initialSpots: wk.initialSpots, isChanged: wk.isChanged }; //force isChanged True?
                    });
                    $scope.openMarketsGrid.set(gridRec.recid, changes);
                }
            });
        }
    },

    ///////////// SAVE INVENTORY

    checkUnsavedSpots: function () {
        return this.activeProgramEditItems.length >= 1;
    },

    //parameters from saved edits - in BE structure by weeks
    getParamsForSave: function () {
        var params = {
            ProposalVersionDetailId: this.activeInventoryData.DetailId,
            Weeks: []
        };

        if (this.checkUnsavedSpots) {
            //partition - break down all items flat then partition by MediaWeekId
            var programWeeks = [];
            $.each(this.activeProgramEditItems, function (idx, programItem) {
                $.each(programItem.weeks, function (key, wk) {
                    programWeeks.push(wk);
                });
            });

            //partitions: breaks into week groups map and then values to arrays [[program, program], [program]]
            var partitionedByWeek = _.values(_.groupBy(programWeeks, 'MediaWeekId'));
            $.each(partitionedByWeek, function (pidx, items) {
                var week = {
                    MediaWeekId: items[0].MediaWeekId,
                    Programs: []
                };
                $.each(items, function (didx, data) {
                    //should be all isChanged
                    if (data.isChanged) {
                        week.Programs.push({
                            ProgramId: data.ProgramId,
                            Spots: data.Spots,
                            TotalImpressions: data.TotalImpressions,
                            UnitCost: data.UnitCost,
                            UnitImpressions: data.UnitImpression
                        });
                    }
                });
                params.Weeks.push(week);
            });
        }
        //why stored on this higher object?
        params.Filter = this.storedOpenMarketInventory.Filter;
        return params;
    },

    //save from apply or save context - 
    //if apply - record scroll positions, call api to refresh, and reset
    //apply sorting; revise to handle future filtering/post processing
    saveInventory: function (isApply) {
        var $scope = this;

        //if request.length etc?
        var continueSaveFn = function () {
            var request = $scope.getParamsForSave();
            //console.log('saveInventory', request);
            //if request.Weeks.length etc?
            $scope.PlanningController.apiSaveInventoryOpenMarket(request, function (inventory) {
                $scope.activeProgramEditItems = [];

                if (isApply) {
                    //scroll, sorting reset
                    $scope.recordLastScrollPosition();
                    $scope.refreshInventory(inventory, false, false, true);
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


    //Revised  modal close event - added to distinguish between clear and hide modal and got0 to react proposal
    onHideInventory: function () {
        var proposalId = this.activeDetailSet.proposalId;  //or from data
        this.onClearInventory(true);
        this.returnToProposal(proposalId);

    },

    //clear inventory ; reset
    onClearInventory: function (reset) {

        this.activeInventoryData = null;
        this.bypassCheckSave = false;
        if (reset) {
            this.isMarketSortName = false;
            this.marketSortIndexMap = [];
            this.OpenMarketVM.setSortByMarketName(false);
            this.activeProgramEditItems = [];
        }
    },

    //modal hide event - check unsaved if not read only
    onCheckInventoryCancel: function (e) {

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


    },

    hasFiltersApplied: function () {
        return this.FilterVM.criteriaList().length > 0 || this.OpenMarketVM.selectedSpotFilterOption() != 1;
    },

    applyFilter: function () {
        var $scope = this;

        var inventoryWithFilters = $scope.storedOpenMarketInventory;

        // update request with filter criteria from FilterVM and spot filter dropdown
        inventoryWithFilters.Filter = $scope.FilterVM.formattedFilterCriteria();

        if (inventoryWithFilters.Filter) {
            inventoryWithFilters.Filter.SpotFilter = $scope.OpenMarketVM.selectedSpotFilterOption();
        } else {
            inventoryWithFilters.Filter = {
                SpotFilter: $scope.OpenMarketVM.selectedSpotFilterOption()
            }
        }

        $scope.PlanningController.apiApplyOpenMarketInventoryFilter(inventoryWithFilters, function (filteredInventory) {
            $scope.refreshInventory(filteredInventory, false, true, true);//check edits and sorts

            // filter indicator
            $scope.OpenMarketVM.hasFiltersApplied($scope.hasFiltersApplied());

            $scope.FilterVM.showModal(false);
        });
    }

});