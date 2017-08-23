var bulkDateObject;
var bulkTarget;
//spot limit specific view using W2UI layouts/components
var ProposalView = function (controller) {
    this.controller = controller;

    return {
        viewOptions: {
            sorting: {
                spotSortingByHealth:false,      // Flag that determines if the "Spots" Column is sorted by number or health
                spotSortByHealthDirection: 1,   // Flag that determines the direction of the Health Sorting (1 is ascending, -1 is descending)
            }
        },
        viewModel: controller.viewModel,
        isActive: false,
        controller: controller,
        aux: 0,
        addNetworkActionsSet: false,
        lastRecidSelected: 0,
        //hasActiveNetworksAllSelected: false,

        initView: function () {
            var cfg = config.getGridConfig(this);
            var me = this;
            //console.log(config);
            cfg.grid.onClick = this.onGridClick.bind(this);
            cfg.grid.onDblClick = this.onGridDoubleClick.bind(this);
            cfg.grid.onMenuClick = this.onGridMenuClick.bind(this);
            cfg.grid.onContextMenu = this.beforeContextMenuShown.bind(this);
            cfg.grid.onSort = this.onGridSort.bind(this);                       // This is used for the custom sort logic
            cfg.grid.show.selectColumn = false;
            //cfg.grid.onContextMenu = this.onGridContextMenuClick.bind(this);
            this.$InventoryGrid = $('#main').w2grid(cfg.grid);
            //this.$SpotLimitsGrid = w2ui['ProposalsGrid'];
            this.$NetworkDataModal = $("#networkDataModal").modal({ show: false });
            this.$BulkEditModal = $('#bulkDaypartModal').modal({ show: false });
            this.$BulkEditModal.find('#bulkDaypart').on("focusin", $.proxy(this.onBulkDaypart, this));
            this.$BulkEditForm = null;
            //this.setEndLimitForm();
            this.isActive = true;
            this.$NetworkDataGrid = null;
            this.recordsLoadingIMSData = {};
            this.recordsAlreadyRefreshed = {};
            $("body").on("click", "#daypart-info-btn", this.onClickDaypartInfo.bind(this));
            $("body").on("click", "#cancel-button", function () {
                if (confirm("Your changes will be lost, proceed without saving?")) {
                    setTimeout(function () {
                        $("#cancelBtn")[0].click();
                    }, 20);
                }
            });
            // Handler for the options modal
            $('body').on('click', '#options-button', function () {
                var optionsModal = util.showModal(
                    '#optionsModal',
                    "<h4>Proposal Options</h4>",
                    "<form class='form-horizontal' onsubmit='return false;'><fieldset><div class='form-group'><label class='col-md-4 control-label' for='RateRoundingSelect'>Rounding Option</label><div class='col-md-4'><select id='RateRoundingSelect' name='RateRoundingSelect' class='form-control'><option value='false'>Nearest Cent</option><option value='true' " + (proposalController.proposalOptions.RoundProposalRate == true ? "selected" : "") + ">Nearest Dollar</option></select></div></div></fieldset></form>",
                    function () {
                        // need a function to call the apiProposalReffresh
                    }
                    );   // Uses the Show Modal Method to construct a custom modal
                $('#optionsModal').off().on('change', '#RateRoundingSelect', function (e) {
                    var optionSelected = $("option:selected", this);
                    var valueSelected = this.value;
                    proposalController.apiChangeRoundingRate(this.value);
                    optionsModal.modal('hide');
                    proposalController.view.refreshGridComplete();
                });
            });
        },

        prepareGridData: function (data, totals) {
            var header = this.viewModel.Header();

            var gridData = data.map(function (x) {
                return Object.assign({}, x, {
                    recid: x.UniqueId,
                    ImpressionsPerc: x.ContractedHhEqImpressionsTotal / totals.HhEqImpressions * 100,
                    ContractedTotalCostPerc: (x.ContractedTotalCost / totals.GrossCost * 100),
                    DaypartSortDisplay: x.Daypart.FullString
                });
            });
            var addNetworkRow = this.getAddNetworkRowRecord();

            var totalRow = {
                summary: true,
                recid: 'S-1',
                Daypart: { FullString: "<span style='text-align: float;'>Totals</span>" },
                GdEqCpm: totals.GdEqCpm,
                HhEqCpm: totals.HhEqCpm,
                ContractedUnits: totals.Units,
                ContractedHhEqImpressionsTotal: totals.HhEqImpressions,
                ContractedTotalCost: totals.GrossCost,
                Trp: totals.Trp,
                ContractedTotalCostPerc: (totals.GrossCost / header.Budget * 100),
                ImsInfo: {}
            };
            return gridData.concat(addNetworkRow, totalRow);
        },

        getAddNetworkRowRecord: function () {
            return { addNetwork: true, recid: 'N-1' };
        },

        getGridDetailRecords: function () {
            var details = this.$InventoryGrid.records.filter(function (x) {
                return x.recid !== "N-1" && x.recid !== "S-1";
            });
            return details;
        },

        setGridData: function (data, totals) {
            var gridData = this.prepareGridData(data, totals);
            this.totals = totals;

            this.$InventoryGrid.add(gridData);
        },

        refreshGridData: function (data) {
            var gridData = this.prepareGridData(data, this.totals);
            //this.$InventoryGrid.clear();
            //this.$InventoryGrid.add(gridData)

            //Update IMS content and remove is-loading class
            //var $editableCells = $('#main').find(".ims-cell")
            var _this = this;
            gridData.forEach(function (record, i) {
                if (!_this.recordsAlreadyRefreshed[record.recid]) {
                    _this.renderRowIMSData(record);
                }
            });
            this.controller.apiLoadRefreshTotalsRow(this.totals, data);
        },

        renderRowIMSData: function (record, refreshTotals) {
            var rowCells = $('*[data-recid="' + record.recid + '"]');

            var returnIndex = this.$InventoryGrid.get(record.recid, true);
            if (record.summary)
                this.$InventoryGrid.summary[returnIndex] = record;
            else
                this.$InventoryGrid.records[returnIndex] = record;
            this.$InventoryGrid.refresh();

            rowCells.removeClass("is-loading"); //CPM
            if (record.ImsRecommendedCpm && !record.summary) {
                var $cpmCell = $(rowCells[0]);
                var cpmValue = '$' + record.ImsRecommendedCpm.HhEqCpmStart.toFixed(2);
                $cpmCell.find(".ims-content").html(cpmValue);
            }

            //SPOTS
            if (record.ImsInfo && record.ImsInfo.IsSet) {
                var $spotsCell = $(rowCells[1]);
                var healthRangeCode = record.ImsInfo.ImsAllocatedUnits > 0 ? record.ImsInfo.ImsHealthRangeCode : 0;
                var className = config.getHealthGradeClassName(healthRangeCode);
                var won = config.convertSpotsTagNumber(record.ImsInfo.ImsAllocatedUnits);
                var inv = config.convertSpotsTagNumber(record.ImsInfo.ImsAvailableUnits);

                $spotsCell.find(".won").find(".s-won").remove();
                $spotsCell.find(".won")
                    .addClass(className)
                    .prepend("<span class='s-won'>" + won + "</span>");
                $spotsCell.find(".inventory")
                    .addClass(className)
                    .html(inv);
            }
            if (refreshTotals) {
                this.refreshTotalsAfterApiAction();
            }
        },

        getContainerElement: function (element) {
            var $elem = $(element);
            return $elem.hasClass("editable-cell") ? $elem : this.getContainerElement($elem.parent());
        },

        onClickDaypartInfo: function (e) {
            var daypartBreakdown = this.DaypartBreakdown;
            var cfg = config.getGridConfig(this);
            var daypartInfoBtn = $("#daypart-info-btn");
            var $parent = daypartInfoBtn.parent();
            if (!daypartBreakdown) {
                return;
            }

            var header = '<div style="text-align: right;"><a id="close-button" type="button" class="btn btn-link" aria-label=Close"><span class="glyphicon glyphicon-remove" aria-hidden="true"></span></a></div>'; //if (!this.$DaypartInfoDataGrid) {
            daypartInfoBtn.popover({
                html: true,
                trigger: "manual",
                placement: "bottom",
                content: header + "<div id='daypart-grid-elem' style='width: 500px; height: 200px;'></div>"
            });
            daypartInfoBtn.popover('show'); //}

            var totalRow = { recid: "S-1", summary: true };
            for (var key in daypartBreakdown) {
                if (key !== "Dayparts") {
                    totalRow[key.replace("Total", "")] = daypartBreakdown[key];
                }
            }
            var dayparts = daypartBreakdown.Dayparts.map(function (x, i) {
                x.recid = i;
                x.TotalHhEqImpressions = totalRow.HhEqImpressions;
                return x;
            });
            var gridInstance;
            var gridContainer;
            daypartInfoBtn.on("shown.bs.popover", function () {
                gridContainer = $("#daypart-grid-elem");
                var hash = 'Daypart-' + Date.now();
                gridInstance = gridContainer.w2grid($.extend({}, cfg.daypartPopoverGrid, { name: hash }));
                gridInstance.add(dayparts.concat(totalRow));
                $("#close-button").on("click", function () { daypartInfoBtn.popover('hide'); });
            }.bind(this));
            daypartInfoBtn.on("hidden.bs.popover", function () {
                gridInstance.destroy();
                $(".daypart-btn").remove();
                $parent.append('<a tabindex="1" id="daypart-info-btn" role="button" class="btn btn-primary btn-xxs daypart-btn" aria-label="Daypart Breakdown"><i class="fa fa-info" style="padding: 0px 3px;"></i></a>'); //$("#close-button").off("click")
            }.bind(this));
        },

        onGridMenuClick: function (event) {
            console.log(event);
            // Sort by Spots
            if (event.menuIndex == 3) {
                this.viewOptions.sorting.spotSortingByHealth = false;
                return;
            }
            // Sort by Health Score
            if (event.menuIndex == 4) {
                this.viewOptions.sorting.spotSortingByHealth = true;
                this.viewOptions.sorting.spotSortByHealthDirection = 1;
                return;
            }
            //1 is display Network, 2 is bulk edit, 3 is delete row
            var typeId = event.menuItem.id;
            var record = this.$InventoryGrid.get(event.recid);
            var headerInfo = this.viewModel.Header();
            var cfg = config.getGridConfig(this);
            if (typeId == 1) {
               
                var titleProposal = "<span class='modal-title proposal-title'>" + headerInfo.Title + " - </span>";
                var titleNetwork = "<span class='modal-title network-title'>" + record.Network + "</span>";
                var titleFlight = "<span class='modal-title flight-title'> - " + headerInfo.FlightText + "</span>";
                this.$NetworkDataModal
                    .find(".modal-header")
                    .html("<div>" + titleProposal + titleNetwork + titleFlight + "</div>");

                if (!this.$NetworkDataGrid) {
                    var $gridElement = this.$NetworkDataModal.find("#networkDataGrid");
                    this.$NetworkDataGrid = $gridElement.w2grid(cfg.networkModalGrid);
                }
                this.$NetworkDataGrid.clear();
                this.$NetworkDataGrid.menu = []; // needed for mun functionality
                var competingProposals = record.Competition.CompetingProposals.map(function (x, i) {
                    x.recid = i;
                    x.HhEqCpmStart = record.ImsRecommendedCpm.HhEqCpmStart.toFixed(2);
                    return x;
                });
                competingProposals.push({
                    recid: 'S-1',
                    summary: true,
                    TotalContractedUnits: record.Competition.TotalContractedUnits,
                    TotalRivalContractedUnits: record.Competition.TotalRivalContractedUnits,
                    TotalContractedCost: record.Competition.TotalContractedCost,
                });

                this.$NetworkDataModal.modal("show");
                this.$NetworkDataModal.off('shown.bs.modal');
                this.$NetworkDataModal.on('shown.bs.modal', function () {
                    this.$NetworkDataGrid.add(competingProposals);
                    this.$NetworkDataGrid.onContextMenu = this.beforeContextMenuShown.bind(this);
                    this.$NetworkDataGrid.onSort = this.onGridSort.bind(this);
                }.bind(this));
            }
            if (typeId == 2) {
                var bulkTitle = "<span class='modal-title bulk-title'>Bulk Edit Daypart: " + this.$InventoryGrid.getSelection().length + " record(s) selected.</span>";
                this.$BulkEditModal
                    .find(".modal-header")
                    .html("<div>" + bulkTitle + "</div>");

                var $bulkSave = this.$BulkEditModal.find("#bulkSave");
                $bulkSave.on("click",$.proxy(this.bulkDaypartSave, this));
                this.$BulkEditModal.modal("show");
                this.$BulkEditModal.off('shown.bs.modal');

                                }
            if (typeId == 3) {
                //delete record; make sure not add network record
                if (record.recid != 'N-1') {

                    this.cleanRowPlugins(record.recid);
                    this.removeGridRecord(record.recid);
                }
            }
        },


        getSelections: function () {
            var selections = $InventoryGrid.getSelection();
            var ret = "";
            for (var i = 0; i < selections.length; i++) {
                var sel = selections[i];
                ret += sel.Id + ", ";
            }
            return ret;
        },

        bulkDaypartEdit: function (rec) {
            var record = rec;
            this.controller.apiChangeDaypart(record, bulkDateObject);
        },
        bulkDaypartSave: function () {
            var bulkModal = this.$BulkEditModal;
            var bulkInput = this.$BulkEditModal.find("#bulkDaypart");
            if (bulkInput.hasClass("is-editing")) {
                bulkInput.removeClass("is-editing");
                bulkModal.focus().click();
                $().delay(1000);
            }
            var selections = this.$InventoryGrid.getSelection();
            for (i = 0; i < selections.length; i++) {
                var record = this.$InventoryGrid.get(selections[i]);
                this.bulkDaypartEdit(record);
            }
            bulkDateObject = null;
        },

        cleanRowPlugins: function (rowId) {
            //Remove possible dropdowna
            var daypartInput = $("#daypart-" + rowId).find(".edit-input");
            daypartInput.webuiPopover('destroy');
            daypartInput.removeData('plugin_daypartDropdown');

            var cpmInput = $("#cpm-" + rowId).find(".edit-input");
            cpmInput.webuiPopover('destroy');
            cpmInput.removeData('plugin_cpmDropdown');
        },

        onGridDoubleClick: function (event) {
            event.preventDefault();
            var record = this.$InventoryGrid.get(event.recid);

            if (event.column === 3) { //CPM
                var elementClass = event.originalEvent.target.className;
                if (elementClass.indexOf("label-cpm") >= 0) {
                    var newCpmValue = parseFloat(record.ImsRecommendedCpm.HhEqCpmStart);
                    if (newCpmValue !== parseFloat(record.HhEqCpm.toFixed(2))) {
                        var newRecord = $.extend({}, record, {
                            HhEqCpm: newCpmValue
                        });
                        this.controller.apiChangeHhCpm(newRecord, newCpmValue);
                    }
                }
            }
        },

        onGridClick: function (event) {
            if (event.column === null) {
                return;
            }

            var record = this.$InventoryGrid.get(event.recid);
            if (this.recordsLoadingIMSData[record.recid]) {
                return;
            }
            this.cleanRowPlugins(this.lastRecidSelected);
            this.lastRecidSelected = event.recid;

            if (event.column === 0) {  //NETWORK
                if (record.addNetwork) {
                    var target = this.getContainerElement(event.originalEvent.target);
                    var input = $(target).find(".edit-input");
                    $(input).select2({
                        placeholder: "Add Network",
                        data: this.Networks,
                    }).select2("val", null);
                    //this.onSelectAddNetwork = this.onSelectAddNetwork.bind(this, record);//then e is not bound
                    if (!this.addNetworkActionsSet) {
                        $(input).on("select2:select", this.onSelectAddNetwork.bind(this, record));
                        $(input).on("select2:close", this.onSelectAddNetworkClose.bind(this, input));
                        this.addNetworkActionsSet = true;
                    }

                    setTimeout(function () {
                        $(input).select2("open");
                        $("#add_network_grid_btn").hide();
                    }, 100);
                }
                return;
            }
            //return if not column 0 is addRecord click
            if (record.addNetwork) return;

                //$(target).parent().addClass("cell-focused")
            else if (event.column === 2) { //DAYPART
                var target = this.getContainerElement(event.originalEvent.target);
                target.addClass("is-editing");
                var input = target.find(".edit-input");
                $(input).daypartDropdown({
                    record: record,
                    dateObject: record.Daypart,
                    onClosePopup: this.onSelectDaypart.bind(this, target, record)
                });
                setTimeout(function () {

                    $(input)
                        .focus()
                        .click();
                }, 100);
            }
            else if (event.column === 3) { //CPM
                if (event.originalEvent.target.className.indexOf("label-cpm") >= 0) {
                    return;
                }
                var target = this.getContainerElement(event.originalEvent.target);
                target.addClass("is-editing");
                var input = target.find(".edit-input");
                var optimalBid;

                if (record.ImsRecommendedCpm) {
                    optimalBid = {
                        id: 0,
                        text: record.ImsRecommendedCpm.HhEqCpmStart,
                        units: record.ImsRecommendedCpm.ImsAvailableUnits.toFixed(0),
                        status: record.ImsRecommendedCpm.ImsHealthRangeCode,
                        quantifier: record.ImsRecommendedCpm.ImsQuantifier
                    };
                }

                var configObj = {
                    record: record,
                    initialValue: record.HhEqCpm,
                    data: record.ImsAvailabilityByCpm.map(function (item, i) {
                        item.text = item.HhEqCpmStart,
                        item.id = i + 1,
                        item.units = item.ImsAvailableUnits.toFixed(0),
                        item.status = item.ImsHealthRangeCode,
                        item.quantifier = item.ImsQuantifier;
                        return item;
                    }),
                    optimalBid: optimalBid,
                    onSelectCpm: this.onSelectCpm.bind(this, target, record)
                };
                input.cpmDropdown(configObj);
                setTimeout(function () {
                    input
                        .focus()
                        .click();
                }, 200); 1
            }
            else if (event.column === 5) { //SPOTS
                //this.cleanRowPlugins(record.recid)
                var target = this.getContainerElement(event.originalEvent.target);
                if (target.hasClass('is-editing') || target.hasClass('has-error')) return;

                target.addClass("is-editing");
                var _this = this;
                var input = target.find(".edit-input");
                var spotval = record.ContractedUnits || 0;
                input.val(spotval);
                input.focus();
                input.keyup(function () {
                    var value = this.value.replace(/[^0-9\.]/g, '');
                    value = value >= 999 ? 999 : value < 0 ? 0 : value;
                    if (this.value != value) {
                        this.value = value;
                    }
                });
                input.keydown(function (event) {
                    if (event.keyCode === 9) {//TAB
                        event.preventDefault();
                        var nextCell = $("#rate-" + record.recid);
                        nextCell.click();
                        nextCell.find("input").focus();
                    } else if (event.keyCode === 13) { //ENTER
                        event.preventDefault();
                        //use blur event so not called twice
                        input.blur();

                    }
                });
                input.blur(function (event) {
                    setTimeout(function () {
                        this.onSelectSpot(target, record, input);
                        input.off("keyup");
                        input.off("keydown");
                        input.off("blur");
                    }.bind(this), 50);
                }.bind(this));
            }

            else if (event.column === 8) { //Rate
                //this.cleanRowPlugins(record.recid)
                var target = this.getContainerElement(event.originalEvent.target);
                if (target.hasClass('is-editing') || target.hasClass('has-error')) return;
                target.addClass("is-editing");
                var _this = this;
                var input = target.find(".edit-input");

                var rateVal = record.ContractedRate || 0;
                /*var options = {};
                if (this.controller.proposalOptions.RoundProposalRate) {
                    options = {
                        prefix: "$",
                        precision: 0
                    };
                } else {
                    options = {
                        prefix: "$",
                        precision: 2
                    }
                }*/
                // money mask options
                input.focus();
                input.val(rateVal)
                input.keydown(function (event) {
                    var periods = [190, 110];
                    var bkspace_del = [8, 46];
                    var keyArray = [37, 38, 39, 40, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 46, 8, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57];
                    if (_this.controller.proposalOptions.RoundProposalRate == false) {
                        keyArray = keyArray.concat(periods);
                    }
                    if ($.inArray(event.keyCode, keyArray) == -1) {
                        if (util.numberOfDecimalPlaces(input.val()) >= (_this.controller.proposalOptions.RoundProposalRate ? 0 : 2)) event.preventDefault();
                        if ($.inArray(event.keyCode, bkspace_del) == -1) event.preventDefault();
                    }
                    if (event.keyCode === 9) {//TAB
                        event.preventDefault();
                        _this.selectNextRecord(record);
                    } else if (event.keyCode === 13) { //ENTER
                        event.preventDefault();
                        //use blur event so not called twice
                        input.blur();
                    }
                });
                input.blur(function (event) {
                    var complete = this.onSelectRate(target, record, input);
                    if (complete) {
                        input.off("keyup");
                        input.off("keydown");
                        input.off("blur");
                    }
                }.bind(this));
            }

        },

        selectNextRecord: function (record) {
            var records = this.$InventoryGrid.records.filter(function (x) {
                return x.recid !== "S-1" && x.recid !== "N-1";
            });
            var nextRecord;
            for (var i = 0; i < records.length; i++) {
                var rec = records[i];
                if (rec.recid === record.recid && i < records.length - 1) {
                    nextRecord = records[i + 1];
                }
            }
            if (nextRecord) {
                var nextCell = $("#daypart-" + nextRecord.recid);
                nextCell.click();
                nextCell.find("input").focus();
            } else {
                this.onToolbarAddNetwork();
            }
        },

        delayRemoveEditing: function (target) {
            setTimeout(function () { target.removeClass("is-editing"); }, 100);
        },

        onSelectAddNetworkClose: function (input, e) {
            //this is called before select
            var me = this;
            setTimeout(function () {
                $(".is-editing").removeClass("is-editing");
                $(input).select2('destroy');
                $("#add_network_grid_btn").show();

            }, 200);

        },

        onSelectAddNetwork: function (record, e) {
            var netId = e.params.data.Id;
            this.controller.apiAddNetwork(netId);
            this.addNetworkActionsSet = false;
        },

        onSelectCpm: function (target, record, newValue) {
            //target.removeClass("is-editing");
            this.delayRemoveEditing(target);

            if (record.HhEqCpm.toFixed(2) != newValue) {
                var newRecord = $.extend({}, record, {
                    HhEqCpm: parseFloat(newValue)
                });
                this.controller.apiChangeHhCpm(newRecord, parseFloat(newValue));
            }
        },

        onSelectDaypart: function (target, record, dateObject) {
            //target.removeClass("is-editing");
            this.delayRemoveEditing(target);
            var hasChanged = false;
            for (var key in dateObject) {
                if (record.Daypart[key] !== dateObject[key]) {
                    hasChanged = true;
                    break;
                }
            }

            if (hasChanged) {
                this.controller.apiChangeDaypart(record, dateObject);
            }
        },
        onBulkDaypart: function () {
            var selections = this.$InventoryGrid.getSelection();
            var bulkModal = this.$BulkEditModal.find("#bulkDaypart");
            var rec = this.$InventoryGrid.get(selections[0]);
            bulkModal.addClass("is-editing");
            var input = bulkModal.find(".edit-input");
            var dateObj;
            if (bulkDateObject != null) {
                dateObj = bulkDateObject;
            } else {
                dateObj = rec.Daypart;
            }

            bulkModal.daypartDropdown({
                record: rec,
                dateObject: dateObj,
                onClosePopup: function (value) {
                    bulkDateObject = value;
            }
            });
            
        },

        onSelectSpot: function (target, record, input) {
            this.delayRemoveEditing(target);
            var spotValue = parseInt($(input).val());
            //check is changed
            if (spotValue >= 0 && record.ContractedUnits != spotValue) {
                this.controller.apiChangeUnits(record, spotValue);
            }
        },

        onSelectRate: function (target, record, input) {
            var value = parseFloat(input.val());
            //target.removeClass("is-editing");
            this.delayRemoveEditing(target);
            if (value > 100000) {
                value = 100000; // max value = $ 100,000.00
            }

            //check is changed
            if (record.ContractedRate != value) {
                if (isNaN(value)) {
                    input.val(record.ContractedRate);
                    return true;
                }
                this.controller.apiChangeRate(record, value.toFixed(2));
            }
            return true;
        },

        onToolbarAddNetwork: function () {
            this.$InventoryGrid.scrollIntoView(this.$InventoryGrid.get('N-1', true));
            $('#add_network_grid_btn').click();
            //this.$InventoryGrid.select(data.recid);
        },

        //Add a record from detail
        //set new record above Add Network - either splice in and refreshgrid OR remove network record and readd?
        addRecordToGrid: function (data) {
            data.recid = data.UniqueId;
            data.ContractedTotalCostPerc = data.ContractedTotalCost / this.totals.GrossCost * 100;
            data.ImpressionsPerc = data.ContractedHhEqImpressionsTotal / this.totals.HhEqImpressions * 100;

            this.$InventoryGrid.remove('N-1');
            var addNetworkRow = this.getAddNetworkRowRecord();
            this.$InventoryGrid.add([data, addNetworkRow]);
            //get index and scroll to?
            this.$InventoryGrid.scrollIntoView(this.$InventoryGrid.get(data.recid, true));
            this.$InventoryGrid.select(data.recid);
            var nextCell = $("#daypart-" + data.recid);
            nextCell.click();
            nextCell.find("input").focus();
            this.refreshTotalsAfterApiAction();
            this.updateGridAfterRefresh();
        },

        updateGridAfterRefresh: function () {
            this.$InventoryGrid.records.forEach(function (record) {
                if (!this.recordsLoadingIMSData[record.recid]) {
                    this.renderRowIMSData(record);
                }
            }.bind(this));
        },

        removeGridRecord: function (recid) {
            this.$InventoryGrid.remove(recid);
            this.refreshTotalsAfterApiAction();
            this.updateGridAfterRefresh();
        },

        //from controller - refresh r=totals after add/delete, etc
        refreshTotalsAfterApiAction: function () {
            var details = this.getGridDetailRecords();
            var totals = this.$InventoryGrid.get("S-1");
            this.controller.apiLoadRefreshTotalsRow(totals, details);
        },

        refreshGridRow: function (data, refreshTotals) {
            this.cleanRowPlugins(data.recid); //handle specialized daypart for sorting; need ContractedTotalCostPerc & ImpressionsPerc calc as well as not done in renderer
            data.DaypartSortDisplay = data.Daypart.FullString;
            data.ContractedTotalCostPerc = data.ContractedTotalCost / this.totals.GrossCost * 100;
            data.ImpressionsPerc = data.ContractedHhEqImpressionsTotal / this.totals.HhEqImpressions * 100;

            var returnIndex = this.$InventoryGrid.get(data.recid, true);
            if (data.summary)
                this.$InventoryGrid.summary[returnIndex] = data;
            else
                this.$InventoryGrid.records[returnIndex] = data;
            this.$InventoryGrid.refresh();

            if (refreshTotals) {
                this.refreshTotalsAfterApiAction();
            }
        },

        refreshGridTotalsRow: function (data) {
            this.totals = data;
            var details = this.$InventoryGrid.records.filter(function (x) {
                return x.recid !== "N-1" && x.recid !== "S-1";
            });
            var totalrow = this.$InventoryGrid.get("S-1");
            var hasImpressionsChanged = totalrow.ContractedHhEqImpressionsTotal !== data.HhEqImpressions;
            var hasCostChanged = totalrow.ContractedTotalCost !== data.GrossCost;
            for (var i = 0; i < details.length; i++) {
                var record = details[i];
                if (hasImpressionsChanged) {
                    record.ImpressionsPerc = record.ContractedHhEqImpressionsTotal / data.HhEqImpressions * 100;
                    this.$InventoryGrid.refreshCell(record.recid, "ContractedHhEqImpressionsTotal");
                }
                if (hasCostChanged) {
                    record.ContractedTotalCostPerc = record.ContractedTotalCost / data.GrossCost * 100;
                    this.$InventoryGrid.refreshCell(record.recid, "ContractedTotalCost");
                }
            }
            this.DaypartBreakdown = data.DaypartBreakdown;

            var header = this.viewModel.Header();
            totalrow.ContractedHhEqImpressionsTotal = data.HhEqImpressions;
            totalrow.ContractedTotalCost = data.GrossCost;
            totalrow.ContractedUnits = data.Units;
            totalrow.ContractedTotalCostPerc = (data.GrossCost / header.Budget) * 100;

            totalrow.Trp = data.Trp;
            totalrow.GdEqCpm = data.GdEqCpm;
            totalrow.HhEqCpm = data.HhEqCpm;
            totalrow.HhEqImpressions = data.HhEqImpressions;
            totalrow.ImsInfo = data.ImsInfo;
            totalrow.NetCost = data.NetCost;

            this.updateHeader(data);
            this.$InventoryGrid.refreshRow("S-1");
            this.renderRowIMSData(totalrow);
        },

        updateHeader: function (totals) {
            var header = this.viewModel.Header();
            header.HhEqImpressions = totals.HhEqImpressions;
            header.NetCost = totals.NetCost;
            header.GrossCost = totals.GrossCost;
            header.CostPercentage = header.Budget ? (header.GrossCost / header.Budget) * 100 : 0;
            header.ImsHealthScore = Math.floor(totals.ImsInfo.ImsHealthScore * 100);
            var healthAdditionalClass = totals.ImsInfo.ImsHealthRangeCode === 1 ? 'label-danger' : totals.ImsInfo.ImsHealthRangeCode === 2 ? 'label-warning' : 'label-success';
            header.healthClass = 'label-health ' + healthAdditionalClass;
            header.ImsHealthText = totals.ImsInfo.ImsHealthRangeText;
            this.viewModel.Header(header);
        },

        refreshGridComplete: function () {
            try {
                this.$InventoryGrid.records.forEach(function (record) {
                    this.controller.apiChangeRate(record, record.ContractedRate); // Reload Rate Api Calls
                    }.bind(this)).promise().done(function () {
                    this.refreshTotalsAfterApiAction();
                }
                );
            } catch (err) {
            }
        },

        // 6/28/2016
        // This is bound as the onSort Method (line 23) and Allows for a sort to be custom implemented
        // http://w2ui.com/web/docs/w2grid.onSort
        // Custom  Propagation: event -> onGridSort(event) -default-> w2grid.sort()
        // Custom  Propagation: event -> onGridSort(event) { event.stopPropagation()) }
        // Regular Propagation: event -> w2grid.sort()
        onGridSort: function (event) {
            console.log(event);
            var $grid = w2ui[event.target];             // Reference to the grid that is being sorted; event.target is the grid target
            var _this = this;                           // a static this reference to be utilized in our custom sort function below
            switch (event.field) {                      // This switch statment splits the event by the field (i.e. the specific column) so a custom sort can be implmented for a specific column                
                case "ContractedUnits": {               // If this is Spots row implement a custom sort
                    // Begin Custom Sorting Code
                    if (_this.viewOptions.sorting.spotSortingByHealth == true) {    // Only do a custom sort if that is enabled
                        if (event.target == "ProposalsGrid") {
                            var first = $grid.records[$grid.records.length - 1];        // save the "Add Network" row, if it exists
                            $grid.records = $grid.records.slice(0, -1).sort(
                            function (a, b) {
                                return (a.ImsInfo.ImsHealthScore - b.ImsInfo.ImsHealthScore) * _this.viewOptions.sorting.spotSortByHealthDirection;
                            }
                            ).concat(first);
                        } else {
                            $grid.records = $grid.records.sort(
                            function (a, b) {
                                return (a.ImsInfo.ImsHealthScore - b.ImsInfo.ImsHealthScore) * _this.viewOptions.sorting.spotSortByHealthDirection;
                            }
                            );
                        }                        
                        $grid.refresh();
                        _this.viewOptions.sorting.spotSortByHealthDirection = -_this.viewOptions.sorting.spotSortByHealthDirection; // Reverse the sorting direction
                        event.preventDefault();         // Canceling the propagation of the default sort event chain allows our custom sort to no be re-sorted by the value: if this isn't here the custom sort code code is null*/
                    }
                    // End Custom Sorting Code
                    break;
                }
                default: { break; }
            }
            event.onComplete = function () {
                setTimeout(function () {
                    $grid.scrollIntoView(10);
                }, 10);
            };
        },

        beforeContextMenuShown: function (event) {
            console.log(event);
            var target = event.target;
            var sortBySpotsText = ' Sort by Spots';
            var sortByHealthText = ' Sort by Health Score';
            if (this.viewOptions.sorting.spotSortingByHealth == true) {
                sortByHealthText = '<i class="fa fa-check" aria-hidden="true"></i>' + sortByHealthText;
            } else {
                sortBySpotsText = '<i class="fa fa-check" aria-hidden="true"></i>' + sortBySpotsText;
            }
            var menu = w2ui[target].menu;
            w2ui[target].menu = w2ui[target].menu.concat([{ id: 7781, text: sortBySpotsText }, { id: 5233, text: sortByHealthText }]);
            event.onComplete = function () {
                setTimeout(function () { w2ui[target].menu = menu; }, 150);
            }
        }
    };
};