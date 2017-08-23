//Proposal Detail Inventory View - handle the Inventory for active Detail Set
var ProposalDetailInventoryView = BaseView.extend({
    activeInventoryData: null,
    activeDetailSet: null,
    daypartsGrid: null,
    daypartsHeaderGrid: null,
    inventoryWeekGrids: [], //or object?
    inventoryWeekHeaderGrids: [],
    ProposalView: null,
    InventoryVM: null,
    $Modal: null,
    isScrollSet: false,
    userName: null,
    scrollPositions: null, //stored positions for resetting user poistion after refresh
    weekColumnGroupTpl: _.template('<div class="openmarket-column-group"><div>${ quarter } Week of ${ week }</div>' +
                                   '<div class="cadent-dk-blue goals">' +
                                        '<i class="fa fa-bullhorn" aria-hidden="true"></i> ${ weekImpressions }/${ impressions } <span class="label ${ impressionsMarginClass } custom-label">${ impressionsPercent }%</span>&nbsp;&nbsp;&nbsp;&nbsp;' +
                                        '<i class="fa fa-money" aria-hidden="true"></i> ${ weekBudget }/${ budget } <span class="label ${ budgetMarginClass } custom-label">${budgetPercent} %</span>' +
                                   '</div>'),

    takenWarningTpl: _.template('<br/>Week <strong>${ WeekDisplay } ${ SlotDaypartCode }</strong> reserved by User <strong>${ UserName }</strong> for Proposal <strong>${ ProposalName }</strong>'),

    initView: function (view) {
        this.ProposalView = view;
        this.InventoryVM = new ProposalDetailInventoryViewModel(this);
        this.getUser();

        ko.applyBindings(this.InventoryVM, document.getElementById("proposal_inventory_view"));
        this.daypartsGrid = $('#inventory_daypart_grid').w2grid(PlanningConfig.getInventoryDaypartsGridCfg(this));
        this.daypartsHeaderGrid = $('#inventory_daypart_header_grid').w2grid(PlanningConfig.getInventoryDaypartsGridCfg(this, true));
        this.initModal();
        //elipsis check on inventory fron/back column - show tip
        $('.inventory-container').on('mouseenter', '.inventory_display_text', function () {
            var $this = $(this);       
            if (this.offsetWidth < this.scrollWidth && !$this.attr('title')) {
                //console.log($this.html);
                $this.attr('title', $this.html());
            }
        });
    },

    //store the user for use in saves - todo move to controller or main view?
    getUser: function () {
        this.userName = appController.user ? appController.user._Username : null;
        //console.log('user', this.userName);
    },

    //top or bottom scroller to align header scroll - probably set once?
    //https://stackoverflow.com/questions/3934271/horizontal-scrollbar-on-top-and-bottom-of-table
    setScrollers: function () {
        /*
        if (!this.isScrollSet) {
            $(".scroll_wrapper1").scroll(function () {
                $(".scroll_wrapper2").scrollLeft($(".scroll_wrapper1").scrollLeft());
            });
            $(".scroll_wrapper2").scroll(function () {
                $(".scroll_wrapper1").scrollLeft($(".scroll_wrapper2").scrollLeft());
            });
            this.isScrollSet = true;
        }*/
        //adjust to scroll based on 1 visible scroller
        if (!this.isScrollSet) {
            $(".scroll_wrapper3").scroll(function () {
                $(".scroll_wrapper1").scrollLeft($(".scroll_wrapper3").scrollLeft());
                $(".scroll_wrapper2").scrollLeft($(".scroll_wrapper3").scrollLeft());
            });
            this.isScrollSet = true;
        }
    },

    //store positions
    recordLastScrollPosition: function () {
        this.scrollPositions = {
            lastScrollTop: $("#inventory_container_scrollable").scrollTop(),
            lastScrollLeft: $("#inventory_weeks_bottom").scrollLeft(),
        };

    },

    //scroll vertically and horizontally based on the last position (i.e. after a refresh)
    scrollToLastPosition: function () {
        var positions = this.scrollPositions;
        if (positions) {
            if (positions.lastScrollTop) $("#inventory_container_scrollable").scrollTop(positions.lastScrollTop);
            if (positions.lastScrollLeft) $("#inventory_weeks_bottom").scrollLeft(positions.lastScrollLeft);
            this.scrollPositions = null;
        }

    },

    //MODAL//
    initModal: function () {
        this.$Modal = $('#proposal_detail_inventory_modal');
        this.$Modal.modal({ backdrop: 'static', show: false, keyboard: false });

        //hidden event
        this.$Modal.on('hidden.bs.modal', this.onClearInventory.bind(this));

        //shown event
        this.$Modal.on('shown.bs.modal', this.onSetInventory.bind(this));
    },

    showModal: function (isHide) {
        if (isHide) {
            this.$Modal.modal('hide');
        } else {
            this.$Modal.modal('show');
        }
    },

    //refresh after Apply save
    refreshInventory: function () {
        var $scope = this;
        //call with varied view El context
        var set = this.activeDetailSet;
        //console.log('refreshInventory', set);
        this.ProposalView.controller.apiGetProposalInventory(set.activeDetail.Id, function (inventory) {
            $scope.onClearInventory(); //reset states before reloading; if do this will clear activeDetailSet
            $scope.setInventory(set, inventory, false, true);//readOnly should always be false as refresh is from save?
        }, true);
    },


    //set all initial inventory data - open modal; isRefresh - refresh already open
    setInventory: function (detailSet, inventory, readOnly, isRefresh) {
        this.activeInventoryData = util.copyData(inventory, null, null, true);
        this.activeDetailSet = detailSet;
        this.InventoryVM.setInventory(inventory, readOnly);
        if (isRefresh) {
            this.onSetInventory(true);
        } else {
            this.showModal();
        }
    },

    //after modal shown - set inventory views
    onSetInventory: function (isRefresh) {
        this.setDaypartsGrid(this.activeInventoryData.Dayparts);
        this.insertWeekGrids(this.activeInventoryData.Weeks, this.activeInventoryData.DetailSpotLength);
        //dayparts grid and weeks dont always line up does not align so resize again
        this.realignAllGrids();
        this.setScrollers();
        this.updateSummary();
        if (isRefresh) this.scrollToLastPosition();
    },

    //for use in grid column settings and for updates
    getWeekColumnGroup: function (week) {
        var goal = PlanningConfig.impressionRenderer(week.ImpressionsGoal);
        var budget = config.renderers.toMoneyOrDash(week.Budget, true);

        var impressionsPercent = week.impressionsPercent ? week.impressionsPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";
        var budgetPercent = week.budgetPercent ? week.budgetPercent.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0] : "-";

        var vals = {
            quarter: week.QuarterText,
            week: week.Week,

            weekImpressions: week.weekImpressions ? week.weekImpressions.toFixed(3) : "-",
            impressions: goal,
            impressionsPercent: impressionsPercent,
            
            weekBudget: week.weekBudget ? config.renderers.toMoneyOrDash(week.weekBudget, true) : "-",
            budget: budget,
            budgetPercent: budgetPercent,
            impressionsMarginClass: week.ImpressionsMarginAchieved ? "label-danger" : "label-success",
            budgetMarginClass: week.BudgetMarginAchieved ? "label-danger" : "label-success"
        };

        return this.weekColumnGroupTpl(vals); //use lodash template
    },

    //update column group
    updateWeekColumnGroups: function (week) {
        var el = $('#inventory_week_column_group_' + week.MediaWeekId);
        var summary = this.getWeekColumnGroup(week);
        if (el) el.html(summary);
    },

    //insert multiple grids for scrolling structure - headers and data
    //handle 1 or 2 contact columns
    insertWeekGrids: function (weeks, detailSpotLength) {
        var $scope = this;
        $.each(weeks, function (idx, week) {
            var id = idx + 1;
            var group = $scope.getWeekColumnGroup(week);
            //append a div with Id to use for updates later
            group = '<div id="inventory_week_column_group_' + week.MediaWeekId + '">' + group + '</div>';
            //determine 1 or 2 columns (30s or 15s)
            //columns : MaxSlotSpotLength/DetailSpotLength (in Root) 1 or 2
            var isFrontBack = (week.MaxSlotSpotLength / detailSpotLength) > 1;
            //elements
            var el = 'inventory_week_grid_' + id;
            var el2 = 'inventory_week_header_grid_' + id; //header
            $('#inventory_weeks').append('<div class="week-item"><div id="' + el + '"></div></div>');
            $('#inventory_weeks_headers').append('<div class="week-item"><div id="' + el2 + '"></div></div>');
            //set bottom scroller
            $('#inventory_weeks_bottom').append('<div class="week-item"></div>');
            //data grid
            var grid = $('#' + el).w2grid(PlanningConfig.getInventoryWeekGridCfg(id, null, isFrontBack, this));
            //header grid with grouping
            var grid2 = $('#' + el2).w2grid(PlanningConfig.getInventoryWeekGridCfg(id, group, isFrontBack, this));//header with group
            //set properties on grid for access
            grid.weekGridId = id;
            grid.weekGridIndex = idx;
            grid.mediaWeekId = week.MediaWeekId;
            grid.isFrontBack = isFrontBack;//2 columns
            grid.isHiatus = week.IsHiatus;
            grid.checkChanges = false;
            //set week grid
            $scope.setWeekGrid(grid, week, week.DaypartGroups);//need overall week as is nested structure
            //set week grid events
            $scope.setWeekGridEvents(grid);
            //store grids
            $scope.inventoryWeekGrids.push(grid);
            $scope.inventoryWeekHeaderGrids.push(grid2);
        });

    },

    //allocation determination to record: separation of allocation - so can be reused in update taken
    //slot may be prepared item or existing record; separate allocations as may be changed from update context
    //return {front: front, back: back}

    setFrontBackSlotFromAllocations: function (allocations, slot) {
        var isFrontBack = slot.isFrontBack
        var front = {
            active: true, //specific active
            isFront: true,
            isInitiallyAvailable: true,
            isInitiallyTaken: false,
            allocation: null,
            name: null, //dervied name
            selected: false, //is selected
            available: true, //is specifically available - initially empty
            isChanged: false, //is specifically changed
            isCurrentProposal: false,
            isMultiAllocation: false, //has 2 alloccations but 1 column based
            isSingle15Allocation: false // has a single allocation that is 15 (in a detail 30) -/name OR name/- : depending on allocation/order
        };

        var back = {
            active: true, //specific active
            isFront: false,
            isInitiallyAvailable: true,
            isInitiallyTaken: false,
            allocation: null,
            name: null, //dervied name
            selected: false, //is selected
            available: true, //is specifically available
            isChanged: false, //is specifically changed
            isCurrentProposal: false,
            isContinuation: false,  //is a continuation in the back
            isSelectedFromContinue: false //a continuuation that was selected at front or back (can revert back)
        };

        if (allocations && allocations.length) {
            //determine allocations based on order
            var allocation = allocations[0];
            var allocation2 = allocations[1];
            var order = allocation.Order;
            var order2 = allocation2 ? allocation2.Order : null;
            //WRONG!
            //var frontAllocation = (order == 1 || (order2 && order2 == 1)) ? allocation : null;
            // var backAllocation = (order == 2 || (order2 && order2 == 2)) ? allocation2 : null;
            var frontAllocation = null, backAllocation = null;
            if (order == 1) frontAllocation = allocation;
            if (order == 2) backAllocation = allocation;
            if (order2 && (order2 == 1)) frontAllocation = allocation2;
            if (order2 && (order2 == 2)) backAllocation = allocation2;
            //set back
            if (backAllocation) {
                back.available = false;
                back.isInitiallyAvailable = false;
                back.allocation = backAllocation;
                back.name = backAllocation.ProposalName;
                if (backAllocation.IsCurrentProposal) {
                    back.selected = true;//display selected
                    back.isCurrentProposal = true;
                } else {
                    back.selected = false;
                    back.isCurrentProposal = false;//display name
                    back.isInitiallyTaken = true;
                }
            }
            //set front
            if (frontAllocation) {
                front.available = false;
                front.isInitiallyAvailable = false;
                front.allocation = frontAllocation;
                front.name = frontAllocation.ProposalName;

                if (frontAllocation.IsCurrentProposal) {
                    front.selected = true;//display selected
                    front.isCurrentProposal = true;
                } else {
                    front.selected = false;
                    front.isCurrentProposal = false;//display name
                    front.isInitiallyTaken = true;
                }
            }
            //deal with specific cases
            if (isFrontBack) {
                //deal with back active; continuation based on the slot.SpotLength
                var spot = slot.SpotLength;
                if (spot == 15) {
                    back.active = false;
                    back.isInitiallyAvailable = false;
                    //deal Front also?
                } else if (!backAllocation && (spot == 30)) {  //adjust - need to check the allocation is 30
                    if (frontAllocation && (frontAllocation.SpotLength == 30)) {
                        back.isContinuation = true;
                        back.available = false;
                        back.isInitiallyAvailable = false;
                        back.name = front.name;
                        back.isInitiallyTaken = true;//??
                    }
                }

            } else {
                //isMultiAllocation: dual allocation in 1 column
                if (frontAllocation && backAllocation) {
                    front.name += ('/' + backAllocation.ProposalName);
                    front.isInitiallyAvailable = false;
                    front.isMultiAllocation = true;//assume allocation speaks for both
                    front.isInitiallyTaken = true;
                }

                //isSingle15Allocation: single allocations in fron OR back that are 15s -/name or name/-

                if (frontAllocation && !backAllocation && (frontAllocation.SpotLength == 15)) {
                    front.selected = false;
                    front.isInitiallyAvailable = false;
                    //front.isCurrentProposal = false;
                    front.name = front.name + '/-';
                    front.isInitiallyTaken = true;
                    front.isSingle15Allocation = true;
                }

                if (backAllocation && !frontAllocation && (backAllocation.SpotLength == 15)) {
                    front = back;
                    front.selected = false;
                    front.isInitiallyAvailable = false;
                    //front.isCurrentProposal = false;
                    front.name = '-/' + front.name;
                    front.isInitiallyTaken = true;
                    front.isSingle15Allocation = true;
                }
            }
        } else {
            //deal with no allocations but spot 15
            var spot = slot.SpotLength;
            if (isFrontBack && (spot == 15)) {
                back.active = false;
                back.isInitiallyAvailable = false;
                back.available = false;
            }
        }

        return { front: front, back: back };
    },

    //flatten with empty group row and details with Week DaypartSlots
    //refactor new BE  structure - set more explicit properties at this level in record; handle null
    //SEPARATING allocation calcs to allow for updates
    prepareWeekGridData: function (data, week, grid) {
        data = util.copyArray(data);//needed?
        var ret = [];
        var isFrontBack = grid.isFrontBack;
        var $scope = this;
        //may need spacer row
        $.each(data, function (idx, source) {
            //insert group row
            ret.push({ recid: 'inventory_source_' + week.MediaWeekId, isGroup: true, w2ui: { "style": "background-color: #D3D3D3" } });
            $.each(source.Value.DaypartSlots, function (idx2, slot) {
                if (!slot) {//is null so not available throughout; no slot.Id so use index hybrid ID
                    var slotId = 'inventory_slot_' + week.MediaWeekId + '_notavailable_' + idx + '_' + idx2;
                    ret.push({ recid: slotId, active: false });
                } else {
                    //var slotId = 'inventory_slot_' + week.MediaWeekId + '_' + slot.DaypartSlotId;
                    var slotId = 'inventory_slot_' + week.MediaWeekId + '_' + slot.InventoryDetailSlotId;  //change to use InventoryDetailSlotId
                    slot.recid = slotId;
                    //grid identifiers
                    slot.weekGridId = grid.weekGridId;
                    slot.weekGridIndex = grid.weekGridIndex;
                    slot.MediaWeekId = week.MediaWeekId;
                    slot.ProposalVersionDetailQuarterWeekId = week.ProposalVersionDetailQuarterWeekId;
                    //InventoryDetailSlotId for save
                    //state
                    slot.isFrontBack = isFrontBack;//2 columns if true
                    slot.isHiatus = week.IsHiatus;
                    slot.active = true;//overall active
                    slot.isChanged = false;//overall edited
                    if (week.IsHiatus) slot.w2ui = { "style": "color: #8f8f8f;" };
                    //prepare front/back from allocations - return {front: front, back: back}
                    var frontBackObj = $scope.setFrontBackSlotFromAllocations(slot.ProposalsAllocations, slot);

                    slot.front = frontBackObj.front;
                    if (isFrontBack || (frontBackObj.front && frontBackObj.front.isMultiAllocation)) {
                        slot.back = frontBackObj.back;
                    }
                    ret.push(slot);
                }
            });
        });
        //console.log('prepare week grid records - week index:' + grid.weekGridIndex, ret);
        return ret;
    },

    //set a week from nested structure to grouped and spacer
    setWeekGrid: function (grid, week, weekDaypartsData) {
        var gridData = this.prepareWeekGridData(weekDaypartsData, week, grid);
        //grid.clear();
        grid.add(gridData);
        grid.resize();
    },

    //add off? to clear events?
    setWeekGridEvents: function (grid) {
        grid.on('contextMenu', this.onWeekGridContextMenu.bind(this, grid));
        grid.on('menuClick', this.onWeekGridMenuClick.bind(this, grid));
    },


    //DAYPART

    //dayparts grid data
    prepareDaypartsGridData: function (data) {
        data = util.copyArray(data);
        var ret = [];
        $.each(data, function (idx, source) {
            //group row then data
            source.recid = 'inventory_source_' + source.Id;
            source.daypartDisplay = source.InventorySource;
            source.isGroup = true;
            source.w2ui = { "style": "background-color: #D3D3D3" };
            ret.push(source);
            $.each(source.Details, function (idx2, spot) {
                var spotId = 'inventory_daypart_slot_' + source.Id + '_' + spot.Id;
                //ret.push({ recid: spotId, daypartDisplay: spot.InventorySpot, isGroup: false });
                spot.recid = spotId;
                spot.daypartDisplay = spot.InventorySpot;
                ret.push(spot);
            });
        });
        return ret;
    },

    //set from nested structure to grouped and spacer
    setDaypartsGrid: function (daypartsData) {
        var gridData = this.prepareDaypartsGridData(daypartsData);
        this.daypartsGrid.clear();
        this.daypartsGrid.add(gridData);
        this.daypartsGrid.resize();
    },

    //realign wehn all grids set
    realignAllGrids: function () {
        this.daypartsGrid.resize();
        this.daypartsHeaderGrid.resize();
        $.each(this.inventoryWeekHeaderGrids, function (idx, grid) {
            grid.resize();
        });
        $.each(this.inventoryWeekGrids, function (idx, grid) {
            grid.resize();
        });

    },

    //clear inventory at modal hide and destroy week grids, etc
    onClearInventory: function () {
        this.daypartsGrid.clear(); //clear dayparts?
        //need clear method in view Model?
        $.each(this.inventoryWeekGrids, function (idx, grid) {
            grid.destroy();
        });
        $('#inventory_weeks').empty();
        $.each(this.inventoryWeekHeaderGrids, function (idx, grid) {
            grid.destroy();
        });
        $('#inventory_weeks_headers').empty();
        $('#inventory_weeks_bottom').empty();
        this.activeDetailSet = null;
        this.activeInventoryData = null;
        this.inventoryWeekGrids = [];
        this.inventoryWeekHeaderGrids = [];

    },

    //grid selection
    //set context menu custom for column 1 or 2 only
    //refactor based on new data - calculated based on front/back in prepareData
    onWeekGridContextMenu: function (grid, event) {
        //does not pick up column - have to find from target
        grid.menu = [];//clear existing menu
        var rec = grid.get(event.recid);
        if (rec && !rec.isGroup && rec.active && !rec.isHiatus) {//not a group and generally active
            //find column from inner attributes of td as event does not return
            var td = $(event.originalEvent.target).parents('td').get(0), tdCol = null;
            if (td) tdCol = td.getAttribute("col");
            // var tdCol = $(event.originalEvent.target).parent('td');
            //is column 1 or 2
            if (tdCol && (tdCol == 0 || tdCol == 1)) {
                var isBack = false;
                //has 2 columns - back is right click
                if (rec.isFrontBack && tdCol == 1) {
                    isBack = true;
                } else {//has 1 or 2 columns front right click
                    isBack = false;
                }
                var contract = isBack ? rec.back : rec.front;
                //found contract and it is active
                if (contract && contract.active) {
                    //pass colContext so menu click will have reference
                    if (contract.selected) {//selected so allow free
                        grid.menu = [{ id: 1, text: 'Free Up', disabled: false, colContext: parseInt(tdCol) }];
                    } else { //taken or available so select
                        grid.menu = [{ id: 2, text: 'Select', disabled: false, colContext: parseInt(tdCol) }];
                    }
                }
            }
        }
    },

    //set needed changes on record contract base on type select/free isSelectedFromContinue handles reset state
    //need to handle cases that were previosly named versus empty available - for now just check to see if allocation? need further check so that front shows available on initial continuation
    setRecordContract: function (contract, isFree, isSelectedFromContinue, overrideFree) {
        //intercept free from current proposal
        var takenAllocation = (contract.allocation && !contract.isCurrentProposal);
        var reallyAvailable = overrideFree ? true : (isSelectedFromContinue || takenAllocation) ? false : true;
        if (isFree) {
            contract.selected = false;
            contract.available = reallyAvailable;
        } else {
            contract.selected = true;
            contract.available = false;
        }
        contract.isChanged = true;
        return contract;
    },

    //on right click change to either free or select - also passing allocation column context to menuItem
    //Refactor based on new BE data and prepare data structure - to contract front/back based
    onWeekGridMenuClick: function (grid, event) {
        //{ id: 1, text: 'Free Up', disabled: false, colContext: parseInt(tdCol) }
        //{ id: 2, text: 'Select', disabled: false, colContext: parseInt(tdCol) }
        var rec = grid.get(event.recid);
        var menuItem = event.menuItem;
        //console.log('onWeekGridMenuClick', event, menuItem);
        if (rec && menuItem) {
            var isFrontBack = rec.isFrontBack;
            //can operate on both 
            var front = rec.front, back = rec.back;
            if (menuItem.id === 1) {//Free Up
                var changes = { isChanged: true };
                //can be set back to original state of continuatuion if selected then freed up
                //in this case cannot make available as was already taken - so set available false

                if (isFrontBack && back.isContinuation && back.isSelectedFromContinue) { //2 column continuation was set before
                    back = this.setRecordContract(back, true, true);
                    front = this.setRecordContract(front, true, true);
                    back.isSelectedFromContinue = false;
                    changes.front = front;
                    changes.back = back;
                } else {
                    if (isFrontBack && (menuItem.colContext == 1)) {//change back only
                        back = this.setRecordContract(back, true);
                        changes.back = back;
                    } else { //change front only
                        front = this.setRecordContract(front, true);
                        changes.front = front;
                    }
                }
                grid.set(event.recid, changes);
                grid.checkChanges = true; //check - may not have changes if set back
                console.log('on Free', rec);
                this.updateSummary(rec);
            } else if (menuItem.id === 2) {//Select
                var changes = { isChanged: true };
                //continuation If select Front then show that Selected with tooltip. Show Back as Available (and vice versa). 
                //but only on first go - then other can be selected.  then if either free set back to original state (above)
                if (isFrontBack && back.isContinuation && !back.isSelectedFromContinue) { //2 column continutation
                    if (menuItem.colContext == 1) {
                        back = this.setRecordContract(back, false);
                        front = this.setRecordContract(front, true, false, true);//special case override free
                        back.isSelectedFromContinue = true;
                    } else {
                        back = this.setRecordContract(back, true);
                        front = this.setRecordContract(front, false);
                        back.isSelectedFromContinue = true;
                    }
                    changes.front = front;
                    changes.back = back;
                } else {
                    if (isFrontBack && (menuItem.colContext == 1)) {//change back only
                        back = this.setRecordContract(back, false);
                        changes.back = back;
                    } else { //change front only
                        front = this.setRecordContract(front, false);
                        changes.front = front;
                    }
                }
                grid.set(event.recid, changes);
                grid.checkChanges = true;
                console.log('on Select', rec);
                this.updateSummary(rec);
            }
        }
    },

    //SAVING

    //set initial params for save
    getSaveItemsParams: function () {
        var detailId = this.activeDetailSet.activeDetail.Id;
        return {
            UserName: this.userName,
            ForceSave: false,
            ProposalDetailId: detailId,
            SlotAllocations: []
        };
    },

    //get an item for a delete or add;
    getSaveItem: function (rec, order, spot, quarterId) {
        //should provide the allocation spot if available else use the default
        spot = spot || this.activeInventoryData.DetailSpotLength;
        //for delete need the allocation ProposalVersionDetailQuarterWeekId (not the week)
        quarterId = quarterId || rec.ProposalVersionDetailQuarterWeekId;
        return {
            //InventoryDetailSlotId: rec.InventoryDetailSlotId,
            QuarterWeekId: quarterId,
            Order: order,
            SpotLength: spot
        };
    },

    //adjust record for saves
    //adds always from the DetailSpotLength, deletes from the allocation.SpotLength
    //CHANGE: return specific object for each slot
    checkRecordForSave: function (rec) {
        var front = rec.front, back = rec.back, isFrontBack = rec.isFrontBack;
        var ret = {
            InventoryDetailSlotId: rec.InventoryDetailSlotId,
            Adds: [],
            Deletes: []
        };

        if (rec.isChanged) {
            //all fronts? deal with specific cases: 
            //if isFrontBack and continuation 1 delete (30) if selected - adds for either/both selection
            //!isFrontBack: isSingle15Allocation delete order(1 or 2 depending on allocation add front 1; isMultiAllocation: 1 add 2 deletes if selected
            var continuation = isFrontBack && back && back.isContinuation;
            //30 with 2 allocations name/name - selected
            if (!isFrontBack && front.isMultiAllocation && front.selected) {
                ret.Deletes.push(this.getSaveItem(rec, 1, front.allocation.SpotLength, front.allocation.ProposalVersionDetailQuarterWeekId));
                ret.Deletes.push(this.getSaveItem(rec, 2, back.allocation.SpotLength, back.allocation.ProposalVersionDetailQuarterWeekId));
                ret.Adds.push(this.getSaveItem(rec, 1));//use default spot
                return ret;
            }
            //based on front (can be an allocation with order 1 or order 2 so use the order in the allocation)
            if (!isFrontBack && front.isSingle15Allocation && front.selected) {
                ret.Deletes.push(this.getSaveItem(rec, front.allocation.Order, front.allocation.SpotLength, front.allocation.ProposalVersionDetailQuarterWeekId));
                ret.Adds.push(this.getSaveItem(rec, 1));//use default spot
                return ret;
            }

            if (front.isChanged) {
                //available to selected - add
                if (front.isInitiallyAvailable && front.selected) {
                    ret.Adds.push(this.getSaveItem(rec, 1));//use default spot
                }
                //current and now not selected - remove
                if (front.isCurrentProposal && !front.selected && !continuation) {
                    ret.Deletes.push(this.getSaveItem(rec, 1, front.allocation.SpotLength, front.allocation.ProposalVersionDetailQuarterWeekId));
                }
                //was taken and now selected - add
                //delete also
                if (front.isInitiallyTaken && front.selected) {
                    ret.Adds.push(this.getSaveItem(rec, 1));//use default spot
                    //delete as well - handle continuation in the back
                    if (!continuation) ret.Deletes.push(this.getSaveItem(rec, 1, front.allocation.SpotLength, front.allocation.ProposalVersionDetailQuarterWeekId));
                }
            }
            if (isFrontBack && back && back.isChanged) {
                //available to selected - add
                if (back.isInitiallyAvailable && back.selected) {
                    ret.Adds.push(this.getSaveItem(rec, 2));//use default spot
                }
                //current and now available - remove
                if (back.isCurrentProposal && !back.selected && !continuation) {
                    ret.Deletes.push(this.getSaveItem(rec, 2, back.allocation.SpotLength, back.allocation.ProposalVersionDetailQuarterWeekId));
                }
                //was taken and now selected - add
                //delete also
                if (back.isInitiallyTaken && back.selected) {
                    ret.Adds.push(this.getSaveItem(rec, 2));//use default spot
                    //delete as well
                    if (!continuation) ret.Deletes.push(this.getSaveItem(rec, 2, back.allocation.SpotLength, back.allocation.ProposalVersionDetailQuarterWeekId));
                }
            }

            if (continuation && (front.selected || back.selected)) ret.Deletes.push(this.getSaveItem(rec, 1, 30, front.allocation.ProposalVersionDetailQuarterWeekId));
            return ret;
        }
        return null;
    },

    //params for save
    getParamsForSave: function (forceSave) {
        var params = this.getSaveItemsParams();
        var $scope = this;
        $.each(this.inventoryWeekGrids, function (idx, grid) {
            if (grid.checkChanges) {
                $.each(grid.records, function (idx, slot) {
                    if (slot.active) {
                        var slot = $scope.checkRecordForSave(slot);
                        if (slot) params.SlotAllocations.push(slot);
                    }
                });
            }
        });
        params.ForceSave = forceSave ? true : false;
        return params;
    },

    //get week grid from media Id
    getWeekGridByMediaWeekId: function (mediaWeekId) {
        var weekGrid;
        $.each(this.inventoryWeekGrids, function (idx, grid) {
            if (mediaWeekId == grid.mediaWeekId) {
                weekGrid = grid;
            }
        });
        return weekGrid;
    },

    //find grid and row based on MediaWeekId and InventoryDetailSlotId ; set record
    processTakenChanges: function (takenItems) {
        var self = this;
        //get grid and construct the id
        $.each(takenItems, function (idx, item) {
            var grid = self.getWeekGridByMediaWeekId(item.MediaWeekId);
            var recid = 'inventory_slot_' + item.MediaWeekId + '_' + item.InventoryDetailSlotId;
            var rec = grid.get(recid);

            //set the grid rec which will reset the rendering, etc
            var frontBackObj = self.setFrontBackSlotFromAllocations(item.ProposalAllocations, rec);
            frontBackObj.ProposalAllocations = item.ProposalAllocations;
            grid.set(recid, frontBackObj);
        });
    },

    //get warning items - refresh if continue- future handle taken
    getSaveTakenWarning: function (params, response, isApply) {
        var $scope = this;
        var ret = '<p>Inventory spots have been taken by one or more User(s)</p>' +
            '<p>By selecting "Continue" - You will take all the spots you have selected.<br/>' +
            'By selecting "Cancel" - you will lose spots listed below in the grid; free spots selected will remain but still need to be saved.</p>' +
            '<p>Spot(s):';
        $.each(response, function (idx, item) {
            ret += '<div>' + item.Messages[0] + '</div>';
        });
        var confirmFn = function () {
            params.ForceSave = true;
            this.saveInventory(isApply, params);
        }.bind(this);
        var cancelFn = function () {
            //do not close modal
            this.processTakenChanges(response);
        }.bind(this);

        util.confirm('Inventory Taken', ret, confirmFn, cancelFn);
    },

    //saving - check all grids and get changes
    //optional pendingParams from second call with forceSave
    saveInventory: function (isApply, pendingParams) {
        var $scope = this;
        var params = pendingParams ? pendingParams : this.getParamsForSave(false);
        //console.log('saveInventory', params);
        this.ProposalView.controller.apiSaveInventoryProprietary(params, function (response) {
            //console.log('apiSaveInventoryProprietary', response);
            if (response.length) {//has conflicts
                $scope.getSaveTakenWarning(params, response, isApply);
                return;
            }
            if (isApply) {
                $scope.recordLastScrollPosition();
                $scope.refreshInventory();
            } else {
                $scope.showModal(true);//close if from save
            }
            util.notify('Inventory saved successfully', 'success');
        });
    },

    // updates overall totals and the week(s)
    updateSummary: function () {
        var self = this;

        // gets week info and selected slots
        var weeks = [];
        for (var i = 0; i < this.inventoryWeekGrids.length; i++) {
            var week = this.activeInventoryData.Weeks[i];
            var grid = this.inventoryWeekGrids[i];

            if (!grid.isHiatus && week) {
                var selectedSlots = grid.records.reduce(function (selection, record) {
                    var isSelected = !record.isGroup && ((record.front && record.front.selected) || (record.back && record.back.selected));

                    if (isSelected) {
                        selection.push({
                            Impressions: record.Impressions,
                            Cpm: record.CPM,
                            Cost: record.Cost
                        });
                    }

                    return selection;

                }, []);

                if (selectedSlots) {
                    weeks.push({
                        MediaWeekId: week.MediaWeekId,
                        ImpressionGoal: week.ImpressionsGoal,
                        Budget: week.Budget,
                        Slots: selectedSlots
                    });
                }
            }
        }

        // assembles the API request
        var request = {
            ProposalDetailId: this.activeDetailSet.activeDetail.Id,
            DetailTargetBudget: this.InventoryVM.detailTargetBudget(),
            DetailTargetImpressions: this.InventoryVM.detailTargetImpressions(),
            DetailCpm: this.InventoryVM.detailCpm(),
            Weeks: weeks
        }

        // sends to BE and use response to update detail header and week/grid header(s)
        this.ProposalView.controller.apiPostDetailTotals(request, function (response) {
            self.InventoryVM.totalCost(response.TotalCost);
            self.InventoryVM.budgetPercent(response.BudgetPercent);
            self.InventoryVM.detailBudgetMarginAchieved(response.BudgetMarginAchieved);

            self.InventoryVM.totalImpressions(response.TotalImpressions);
            self.InventoryVM.impressionsPercent(response.ImpressionsPercent);
            self.InventoryVM.detailImpressionsMarginAchieved(response.ImpressionsMarginAchieved);

            self.InventoryVM.totalCpm(response.TotalCpm);
            self.InventoryVM.cpmPercent(response.CpmPercent);
            self.InventoryVM.detailCpmMaginAchieved(response.CpmMarginAchieved);

            // updates week(s) header(s)
            $.each(response.Weeks, function (idx, updatedWeek) {
                var filteredWeeks = $.grep(self.activeInventoryData.Weeks, function (w) {
                    return w.MediaWeekId == updatedWeek.MediaWeekId;
                });

                var filteredWeek = filteredWeeks[0];

                filteredWeek.weekImpressions = updatedWeek.Impressions;
                filteredWeek.impressionsPercent = updatedWeek.ImpressionsPercent;
                filteredWeek.weekBudget = updatedWeek.Budget;
                filteredWeek.budgetPercent = updatedWeek.BudgetPercent;
                filteredWeek.ImpressionsMarginAchieved = updatedWeek.ImpressionsMarginAchieved;
                filteredWeek.BudgetMarginAchieved = updatedWeek.BudgetMarginAchieved;

                self.updateWeekColumnGroups(filteredWeek);
            });
        });
    }
});