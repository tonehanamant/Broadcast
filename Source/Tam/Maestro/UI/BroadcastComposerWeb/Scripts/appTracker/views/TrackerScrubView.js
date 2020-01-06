
//scrubbing view: grid inside modal
var TrackerScrubView = BaseView.extend({
    isActive: false,
    $ScrubGrid: null,
    $ScrubModal: null,
    activeScrubData: null,
    mappable: [''],
    isLocked: false,
    lockedRows: [],

    activeItemRecid: null,
    activeItemIndex: null,

    //prior to modal open
    initView: function(controller) {
        this.controller = controller;
        this.$ScrubGrid = $('#scrub_grid').w2grid(TrackerConfig.getScrubGridCfg(this));
    },

    //set the active scrub session
    //add option to refresh only (reload when modal is open already)
    setActiveScrub: function(scrubData, refreshOnly) {
        var me = this;
        this.activeScrubData = scrubData;
        this.lockedRows = [];
        this.lockScrubGrid(false);
        if (!this.$ScrubModal) {
            this.$ScrubModal = $('#scrubModal');
            //scope is the modal within event
            this.$ScrubModal.on('shown.bs.modal', function(event) {
                if (!me.isActive) {
                    // Handler for search input
                    $('#scrub_status_filter_input').on('change', function() {
                        me.onGridStatusFilter($(this).val());
                    });
                    me.isActive = true;
                }

                me.setScrubGrid();
            });
            this.$ScrubModal.on('hidden.bs.modal', function (event) {
                //when closing either after a save or cancel - refresh the schedules grid as may be other changes (like mappings)
                me.controller.refreshSchedulesAfterSaveCancel();
            });

            //add the refresh event to the grid after renders and view model creation (not in grid cfg); scope within event is grid
            this.$ScrubGrid.on('refresh', function (event) {
                event.onComplete = function () {
                    me.resetAllOutSpec();
                }
            });

            this.$ScrubModal.modal({
                backdrop: 'static',
                show: false,
                keyboard: false
            });
        }

        //if refreshing reset the grid only; else show the modal and will refresh on shown
        if (refreshOnly) {
            this.setScrubGrid();
        } else {
            this.$ScrubModal.modal('show');
        }
    },

    // prepare scrub grid data with non mutaing copy as needed
    prepareScrubGridData: function (data) {
        var displayData = util.copyData(data);

        var ret = [];
        $.each(displayData, function (index, value) {
            var item = value;
            item.recid = item.Id;
            ret.push(item);
        });

        return ret;
    },

    // prepares W2UI grid add and clear its filters
    setScrubGrid: function () {
        var scrubData = this.prepareScrubGridData(this.activeScrubData.DetectionDetails);
        var filterInputValue = $('#scrub_status_filter_input').val();
        this.$ScrubGrid.searchReset();
        this.$ScrubGrid.clear(false);
        this.$ScrubGrid.add(scrubData);
        this.$ScrubGrid.resize();
        //resets filter input to all, which is not needed as long as the grid isn't being reset
        //$('#scrub_status_filter_input').val('all');
        this.onGridStatusFilter(filterInputValue);
    },

    //filter grid by status
    onGridStatusFilter: function (statusId) {
        if (!statusId || statusId == 'all') {
            this.$ScrubGrid.searchReset();
        } else {
            this.$ScrubGrid.search('Status', statusId);
        }
    },

    //update a row from BE data
    updatePendingMappedRows: function (bvsItems) {
        var me = this;

        $.each(bvsItems, function (index, item) {
            me.$ScrubGrid.set(item.Id, item); //setting will change data and rerender
            me.unlockRow(item.Id);
        });
    },

    /*** LOCKING/UNLOCKING ***/

    // lock the grid editing: set isLocked
    lockScrubGrid: function (isLocked) {
        this.isLocked = isLocked;
    },

    // lock multiple rows by IDs- array of record ids
    lockScrubGridMultipleRows: function (recIds) {
        var me = this;
        $.each(recIds, function (index, recid) {
            me.lockScrubGridSingleRow(recid);
        });
    },

    // lock grid row editing
    lockScrubGridSingleRow: function (recid) {
        var rec = this.$ScrubGrid.get(recid);
        rec.w2ui = { style: 'opacity:0.7' };
        rec.isUpdating = true;
        this.$ScrubGrid.refreshRow(recid);
        this.lockedRows.push(parseInt(recid));
    },

    // updates/unlocks with response data or just ids
    // TODO determine if can rely on the response BVS details matching the ids OR check?
    unlockUpdateScrubGridMultipleRows: function (recIds, updateData) {
        var me = this;
        $.each(recIds, function (index, recid) {
            //this may be overkill? 
            var item = updateData ? util.objectFindByKey(updateData, 'Id', recid) : null;
            me.unlockUpdateScrubGridSingleRow(recid, item);
        });
    },

    // unlock grid row editing -  set properties and data to record depending on updated data
    unlockUpdateScrubGridSingleRow: function (recid, updateItem) {
        var recObj = updateItem || {};
        recObj.w2ui = { style: 'opacity:0.7' };
        recObj.isUpdating = false;
        this.lockedRows.pop(parseInt(recid));
    },

    /*** GRID EVENTS ***/

    // checks if schedule item has errors -- if it does, than enable mapping
    needToMap: function (bvsItem) {
        return (!bvsItem.MatchStation || (!bvsItem.MatchProgram && !bvsItem.LinkedToBlock && !bvsItem.LinkedToLeadin));
    },

    // can be set to 'Officially Out of Spec (status 3)' if any of the selected items is 'Out of Spec (status 2)'
    canOfficiallyOutSpec: function (selectedIds) {
        var me = this;

        if (!selectedIds) {
            return false;
        }

        for (var i = 0; i < selectedIds.length; i++) {
            var estimate = me.$ScrubGrid.get(selectedIds[i]);
            if (estimate.Status == 2) {
                return true;
            }
        }

        return false;
    },

    // double click on a row -- checks conditions for opening the mapping modal
    onDoubleClick: function (event) {
        if (this.isLocked || this.lockedRows.indexOf(parseInt(event.recid)) != -1) {
            event.preventDefault();
        } else {
            var record = this.$ScrubGrid.get(event.recid);
            if (this.needToMap(record)) {
                this.saveScrollInfo(event.recid);
                this.controller.apiGetScheduleProgram(record);
            }   
        }
    },

    // context menu handler
    onContextMenu: function (event) {
        var me = this;

        var selectedIds = me.$ScrubGrid.getSelection();

        if (me.isLocked) {
            event.preventDefault();
        }
        else {
            // multiple rows selected
            if (selectedIds.length > 1) {
                me.$ScrubGrid.menu = [
                    { id: 1, text: 'Map Values', disabled: true },
                    { id: 2, text: 'Officially Out of Spec...', disabled: !me.canOfficiallyOutSpec(selectedIds) }
                ];
            } else {
                // single row selected
                if (me.lockedRows.indexOf(selectedIds[0]) != -1) {
                    event.preventDefault();
                } else {
                    var estimate = me.$ScrubGrid.get(selectedIds[0]);

                    me.$ScrubGrid.menu = [
                        { id: 1, text: 'Map Values', disabled: !me.needToMap(estimate) },
                        { id: 2, text: 'Officially Out of Spec...', disabled: !me.canOfficiallyOutSpec(selectedIds) }
                    ];
                }
            }
        }
    },

    // context menu item selected handler
    onMenuClick: function (event) {
        this.saveScrollInfo(event.recid);

        switch(event.menuItem.id) {
            case 1:
                this.controller.apiGetScheduleProgram(this.$ScrubGrid.get(event.recid));
                break;

            case 2:
                this.setOutSpecItem(event.recid);
                break;
        }
    },

    //scrub grid refresh (sort, filter, change, etc)
    onScrubGridRefresh: function (event) {
        this.resetAllOutSpec();
    },

    //out of spec handling; store ids or records in VM; handle state
    setOutSpecItem: function (recid, displayOnly) {
        var me = this;
        var selectedIds = me.$ScrubGrid.getSelection();

        for (var i = 0; i < selectedIds.length; i++) {
            var rec = this.$ScrubGrid.get(selectedIds[i]);

            if (rec && rec.Status == 2) {
                this.$ScrubGrid.set(selectedIds[i], { Status: 3 });
                var td = $("#bvs_status_item_" + selectedIds[i]).closest('td').addClass('w2ui-changed');
                if (!displayOnly) this.controller.viewModel.addOutSpec(selectedIds[i]);
            }
        }
    },

    //reset out of spec items; from grid refresh operations; get items from view model
    resetAllOutSpec: function () {

        var me = this;
        var specs = this.controller.viewModel.getAllOutSpecs();
        //console.log('resetAllOutSpec', specs);
        if (specs && specs.length) {
            $.each(specs, function (index, id) {
                me.setOutSpecItem(id, true);
            });
        }
    },

    saveScrollInfo: function(recid) {
        this.activeItemRecid = recid;
        this.activeItemIndex = this.$ScrubGrid.get(recid, true);
    },

    scrollToActiveItemIndex: function() {
        if (this.activeItemIndex) {
            this.$ScrubGrid.select(this.activeItemRecid);
            this.$ScrubGrid.scrollIntoView(this.activeItemIndex);
            this.activeItemIndex = null;
        }
    }
});