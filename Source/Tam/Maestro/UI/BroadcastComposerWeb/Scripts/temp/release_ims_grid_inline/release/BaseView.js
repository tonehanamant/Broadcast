var BaseView = Class.extend({

    _enableEditingOnReleasedOrders: false,
    activeEditRecord: null,
    hasActiveEdit: false,
    selectedReel: null,
    selectedReelDisposition: null,
    selectedProduct: null,
    reels: [],
    selectizeInstances: [],

    //GRID EDITING CODE

    init: function() {
        $("body").on("click", "#apply-comment-popover", this.saveComments.bind(this));
        $("body").on("click", "#cancel-comment-popover", this.cancelComments.bind(this));
        $("body").on("click", this.handleOutsideClick.bind(this));
    },

    getUseCaseId: function () {
        return 1; // Copy management
    },

    getCopiesGridConfig: function () {
        var me = this;
        var cfg = config.getCopyGridCfg(this);
        cfg.onClick = this.onGridClick.bind(this);
        cfg.onRefresh = this.onRefreshGrid.bind(this);
        cfg.onSort = function (event) {
            if (me.hasActiveEdit) {
                event.preventDefault();
                return;
            };
        };
        //GU handle exclusion showing delete if IsCopyReleased
        cfg.onContextMenu = function (e) {
            if (this.activeEditRecord && e.recid === this.activeEditRecord.recid) {
                e.preventDefault();
                return;
            }
            var rec = me.$CopyGrid.get(e.recid);
            if (rec && rec.IsCopyReleased) {
                e.preventDefault();
                return;
            }
        }.bind(this)
        cfg.onMenuClick = this.onGridMenuClick.bind(this);

        return cfg;
    },

    onLoadCopies: function (data, noResetDisplay) {
        this.activeCopyData = data;
        this.isMarried = data.IsMarried;
        this.Mvpds = data.Mvpds || [];
        this.Products = data.Products || [];
        this.SpotLengths = data.SpotLengths || [];
        this.ValidDispositions = data.ValidDispositions || [];
        this.setCopyGrid(data);
        //only call this if initial oad not after save copy (CopyUpdate)
        if (noResetDisplay) {

        } else {
            this.setTrafficDisplay(data);
        }

        if (this.isMarried && data.MarriedRotation) this.setMarriedDisplay(data.MarriedRotation);

        if (data.IsReadOnly && !this._enableEditingOnReleasedOrders) {
            $("#readOnlyWarning").html("Read Only");
        } else {
            $("#readOnlyWarning").hide();
        }
    },

    prepareCopyGridData: function (displayCopies) {
        var copy = util.copyData(displayCopies);
        var me = this;
        var ret = [];
        $.each(copy, function (index, value) {
            var item = value;
            item = me.setCopyGridRecord(item);
            ret.push(item);
        });
        return ret;
    },

    setCopyGridRecord: function (copy, isNew) {
        copy.recid = copy.TrafficCopyId;
        if (isNew) copy.isNew = true;
        return copy;
    },

    setCopyGrid: function (data) {
        var copyData = this.prepareCopyGridData(data.DisplayCopies);
        this.$CopyGrid.clear(false);
        var gridData = copyData.concat();
        if (this.activeEditRecord) {
            gridData.push(this.activeEditRecord);
        } else {
            gridData.push({ recid: 0, newCopy: true, RootCopy: { SpotLengthDisplay: 'new' } });
        }
        this.$CopyGrid.add(gridData);
        //do we nee resort, resize refresh?
        this.$CopyGrid.resize();
        this.setViewCopyActions();
        $('[data-toggle="tooltip"]').tooltip({
            container: 'body'
        });
    },

    onRefreshGrid: function (event) {
        var _this = this;
        event.onComplete = function () {
            if (this.activeEditRecord) {
                this.startActiveEdit(this.activeEditRecord);
                this.initializeEditors();
            }

            $('.hd-comment-icon').tooltip({
                container: 'body',
            });
            var marriedLabels = $('#copy_grid').find(".label-married");
            $.each(marriedLabels, function (index, element) {
                var $elem = $(element);
                var recid = $elem.data("id");
                var record = _this.$CopyGrid.get(recid);

                _this.renderTooltipElem(record, $elem);
            });

            var readOnly = this.activeCopyData && !this._enableEditingOnReleasedOrders && this.activeCopyData.IsReadOnly === true;
            if (readOnly) {
                $("#isci_btn").addClass("disabled").attr("disabled", "disabled");
            }
        }.bind(this);
    },

    renderTooltipElem: function (record, $elem) {
        // We use a table here to show the isci and brand nicely in a tooltip
        var table = $(document.createElement('table'));     // Create Table
        table.addClass('table table-hover table-striped table-condensed');  // Sets Table class for bootstrap
        table.css('margin-bottom', '0px');

        var tableBody = $(document.createElement('tbody')); // Create the table body so we can add stuff
        if (record.MarriedCopies && record.MarriedCopies.length >= 2) { // Only execute if there are MarriedCopies (Married status requires more than one copy per record)
            for (var i = 0; i < record.MarriedCopies.length; ++i) {         // For all the married copies --> add an element to the table with each set of copy info
                var product = this.Products.filter(function (x) { return x.Id == record.MarriedCopies[i].ProductId; }); // get the product ID
                var copyIsci = record.MarriedCopies[i].Isci;        // get the product ISCI
                var tableRow = $(document.createElement('tr'));     // Create a row for the table
                var productCol = $(document.createElement('td'));     // Create a column for the product name
                var isciCol = $(document.createElement('td'));     // Create a column for the copy ISCI
                var iconsCol = $(document.createElement('td'));     // Create a column for icons
                if (product.length) productCol.append(product[0].Display);  // If the product has a display name, show it
                isciCol.append(copyIsci);   // Add the isci to the table
                if (record.MarriedCopies[i].Hd) iconsCol.append('<span class="label label-primary">HD</span>');
                if (record.MarriedCopies[i].Esp) iconsCol.append('<span class="label label-primary">ESP</span>');
                if (record.MarriedCopies[i].House) iconsCol.append('<span class="label label-primary"><span class="glyphicon glyphicon-home" aria-hidden="true"></span></span>');
                if (record.MarriedCopies[i].SensitiveContent) iconsCol.append('<span class="label label-primary"><span class="glyphicon glyphicon-ban-circle" aria-hidden="true"></span></span>');
                tableRow.append(productCol, isciCol, iconsCol);
                tableBody.append(tableRow);
            }
        }
        table.append(tableBody);
        $elem.popover({
            html: true,
            container: 'body',
            //content: table.html(),          ==> This does not include the table wrapper (we need that)
            content: table[0].outerHTML,    //==> This includes the outer wrapper :)
            trigger: "hover",
            placement: "bottom"
        });
    },

    onGridMenuClick: function (event) {
        if (event.menuIndex === 0) { //Delete option
            var thisOuter = this;
            util.confirm(
                "Confirm",
                "Are you sure you would like to delete this Copy?",
                function () {
                    //thisOuter.controller.apiDeleteCopy(event.recid);
                    thisOuter.controller.deleteCopy(event.recid);
                }
            );
        }
    },

    showGridActionColumn: function (show) {
        if (show) {
            this.$CopyGrid.showColumn('actions');
        } else {
            //hide all inner editors?
            $('.edit-actions').hide();
            this.$CopyGrid.hideColumn('actions');
        }
    },

    showEditActions: function (show, recid) {
        var edit = $('#actions_' + recid);
        if (show) {
            edit.show();
        } else {
            edit.hide();
        }
    },

    startActiveEdit: function (rec) {
        this.hasActiveEdit = true;
        var original = util.copyData(rec, null, null, true);//deep?
        this.activeEditRecord = original;
        //this.showGridActionColumn(true);
        this.showEditActions(true, rec.recid);
    },

    endActiveEdit: function () {
        this.hasActiveEdit = false;
        this.showEditActions(false, this.activeEditRecord.recid);
        //this.showGridActionColumn(false);
        this.activeEditRecord = null;
    },

    onAfterSaveCopy: function (data) {
        //refresh grid after save - just reset here initially
        this.endActiveEdit();
        //this.setCopyGrid(data);
    },

    setMarriedDisplay: function (marriedData) {
        var list = '<dl style="margin-bottom: 6px;" class="dl-horizontal">';
        $.each(marriedData, function (idx, val) {
            list += ('<dt>' + val.Product + ':</dt><dd>' + val.Rotation + '%</dd>');
        });
        list += '</dl>';
        $("#married_display").show().html(list);
    },

    // 0 - "all' - pass to server as NULL
    //get saved Data;
    getSaveCopyData: function (rec) {
        //this creates new instance this way? store the item
        var isciId;
        if (rec.creatingCopy === true) {
            var isciItem = $("#isci-" + rec.recid).find(".edit-input").data('plugin_isciDropdown').getValue();
        }
        // GU: if record - IsCopyReleased then do not check mvpd input value; do not test for reels or disposition
        if (!rec.IsCopyReleased) {
            //var mvpdId = parseInt($("#mvpd-" + rec.recid).find(".edit-input").selectize()[0].selectize.getValue());
            var mvpdId = parseInt($("#mvpd-" + rec.recid).find(".edit-input")[0].selectize.getValue());
            if (mvpdId == 0) mvpdId = null;
            rec.MvpdId = mvpdId;
            //GU: move here from below
            var _this = this;
            var selectedReels = this.reels.filter(function (x) { return x.Id == _this.selectedReel; });
            var selectedReelDispositions = this.ValidDispositions.filter(function (x) { return x.Id == _this.selectedReelDisposition; });
            if (selectedReels.length) { //Existing Reel
                rec.ReelId = selectedReels[0].Id;
                rec.ReelName = selectedReels[0].Display;
            } else { //New Reel
                rec.ReelId = null;
                rec.ReelName = this.selectedReel;
            }
            if (selectedReelDispositions.length) {
                rec.DispositionId = selectedReelDispositions[0].Id;
                rec.DispositionString = selectedReelDispositions[0].Display;
            }
        }

        var $cell = $("#comments-" + rec.recid);
        rec.StartDate = $("#flight-" + rec.recid).find(".edit-input").data('daterangepicker').startDate.format("YYYY-MM-DDT00:00:00");
        rec.EndDate = $("#flight-" + rec.recid).find(".edit-input").data('daterangepicker').endDate.format("YYYY-MM-DDT00:00:00");
        rec.Rotation = parseInt($("#rotation-" + rec.recid).find(".edit-input").val());
        

        if (rec.RootCopy.Hd) {
            rec.HdComment = rec.HdComment;
            rec.SdComment = rec.SdComment;
        } else {
            rec.SdComment = $cell.find("#SDCommentsInput").val();
        }

        var _this = this;

        var selectedProducts = this.Products.filter(function (x) { return x.Id == _this.selectedProduct; });

        if (selectedProducts.length) {
            rec.ProductId = selectedProducts[0].Id;
            rec.ProductString = selectedProducts[0].Display;
        } else {

        }
        var copy = rec;
        return copy;
    },

    cancelCopyRecord: function (rec) {
        //reset to original
        if (rec.isNew) {
            //remove from grid and editing
            this.$CopyGrid.remove(rec.recid);
            this.endActiveEdit();
        } else {
            var original = this.activeEditRecord;
            //restore and remove from editing
            if (original) {
                this.$CopyGrid.set(rec.recid, original);
                this.endActiveEdit();
            }
        }
    },

    getContainerElement: function (element) {
        var $elem = $(element);
        return $elem.hasClass("editable-cell") ? $elem : this.getContainerElement($elem.parent());
    },

    onGridClick: function (event) {
        var readOnly = !this._enableEditingOnReleasedOrders && this.activeCopyData.IsReadOnly === true;
        if (event.column === null || readOnly) {
            return;
        }
        if (this.activeEditRecord) { //&& (this.activeEditRecord.recid != event.recid)
            return;
        }
        var record = this.$CopyGrid.get(event.recid);
        if (event.column === 0 && record.newCopy === true) {
            //var newRecordObj = $.extend({}, record, )
            this.$CopyGrid.set(record.recid, { newCopy: true, creatingCopy: true }); //this.$CopyGrid.add({ recid: 'NEW', newCopy: true })
        } else if (record.newCopy === true) {
            return;
        }

        //by doing this column show - refreshes/rerenders grid - thus does not work
        this.startActiveEdit(record);
        //var reelselectize = this.setReelEditing(event, record);
        this.initializeEditors(event, event.column);
    },

    initializeEditors: function (event, selectedColumn) {
        var _this = this;
        var record = this.activeEditRecord;
        if (record.creatingCopy === true) {
            var $isciElem = $("#isci-" + record.recid);
            var $isciInput = $isciElem.find(".edit-input");

            $isciElem.addClass("is-editing");

            $isciInput.isciDropdown({
                data: this.activeCopyData.ProductCopies,
                products: this.Products,
                lengthOptions: this.SpotLengths,
                loadData: function (query, pageIndex, cb, onlySingle, onlyHd) {
                    _this.controller.apiLoadCopiesIscis(query, pageIndex, cb, onlySingle, onlyHd);
                },
                onSelectISCI: this.onSelectISCI.bind(this),
            });
            $isciInput.click();
            $isciInput.focus();

            return;
        }

        var $editElements = $("[recidEdit='" + record.recid + "']");
        $editElements.addClass("is-editing");
        if ($editElements.length === 0) {
            return;
        }
        //GU: in extended instances (copy update and alert management - do not initialize some editors (!IsCopyReleased)
        if (!record.IsCopyReleased) {
            var $reelElem = $("#reel-" + record.recid);
            var $reelSelect = $reelElem.find("#reel-select");

            var firstLoad = true;
            $reelSelect.selectize({
                load: function (query, callback) {
                    firstLoad = !query.length;

                    //if (!record.CopyId) return callback();
                    $.ajax({
                        url: baseUrl + '/api/TrafficCopy/AvailableReels/' + (record.RootCopy.CopyId || 0) + '/' + +(record.ReelId || 0),
                        type: 'GET',
                        dataType: 'json',
                        data: {
                            q: query,
                            //page_limit: 10,
                        },
                        error: function () {
                            callback();
                        },
                        success: function (res) {
                            var existing = res.Data.Existing.map(function (reel) {
                                reel.type = "Existing";
                                return reel;
                            });
                            var notLocked = res.Data.NotLocked.map(function (reel) {
                                reel.type = "NotLocked";
                                return reel;
                            });
                            var reels = existing.concat(notLocked);
                            _this.reels = reels;
                            callback(reels);
                        }
                    });
                },
                onItemAdd: function (value, $item) {
                    this.selectedReel = value;
                }.bind(this),
                optgroupField: 'type',
                optgroups: [
                    { value: 'Existing', label: 'Existing', label_scientific: 'Existing' },
                    { value: 'NotLocked', label: 'Not Locked', label_scientific: 'NotLocked' },
                ],
                preload: true,
                labelField: 'Display',
                searchField: ['Display'],
                valueField: 'Id',
                create: true,
                createFilter: function (text) { return text.length <= 63 },
                copyClassesToDropdown: false,
                dropdownParent: "body",
                dropdownClass: "selectize-dropdown reel-selectize",
                onLoad: function (data) {
                    if (firstLoad) {
                        var initialValue = record.editingNewCopy ? this.selectedReel : record.ReelId;
                        $reelSelect[0].selectize.setValue(initialValue + "", true);
                        if (selectedColumn === 3) {
                            $reelSelect[0].selectize.open();
                        }
                    }
                }.bind(this)
            });
            var selectizeReel = $reelSelect[0].selectize;
            if (selectedColumn === 3) {
                selectizeReel.open();
            }
            this.selectizeInstances.push(selectizeReel);


            //Reel Disposition
            var $reelDispositionElem = $("#reel-disposition-" + record.recid);
            var $reelDispositionSelect = $reelDispositionElem.find("#reel-disposition-select");

            $reelDispositionSelect.selectize({
                options: this.ValidDispositions,
                labelField: 'Display',
                valueField: 'Id',
                searchField: ['Display'],
                //create: true,
                copyClassesToDropdown: false,
                dropdownParent: "body",
                dropdownClass: "selectize-dropdown mvpd-selectize",
                render: {
                    item: function (data, escape) {
                        var label = data.Id === 1 ? "Current" : data.Id === 2 ? "Prior" : "New";
                        return "<div tooltip='" + record.DispositionString + "' class='reel-disposition-label'>" + label + "</div>";
                    }
                },
                onItemAdd: function (value, $item) {
                    this.selectedReelDisposition = value;
                }.bind(this),
            });
            $reelDispositionSelect[0].selectize.setValue((record.DispositionId || 3) + "", true);
            this.selectizeInstances.push($reelDispositionSelect[0].selectize);
            if (selectedColumn === 4) {
                $reelDispositionSelect[0].selectize.open();
            }


            //MVPD
            var $mvpdElem = $("#mvpd-" + record.recid);
            var $mvpdSelect = $mvpdElem.find(".edit-input");

            var mvpds = this.Mvpds.concat([]);
            mvpds.unshift({ Id: 0, Display: "All" });
            $mvpdSelect.selectize({
                options: mvpds,
                labelField: 'Display',
                valueField: 'Id',
                searchField: ['Display'],
                //create: true,
                copyClassesToDropdown: false,
                dropdownParent: "body",
                dropdownClass: "selectize-dropdown mvpd-selectize"
            }); //TOD0 - no MvpdId in record - get from BE and remove find below
            //BE change - added MvpdId - null represents All
            var mvpdId = record.MvpdId || 0;
            //if (mvpdvalitem) $mvpdSelect[0].selectize.setValue(mvpdvalitem.Id)
            var mvpdSelectize = $mvpdSelect[0].selectize;
            mvpdSelectize.setValue(mvpdId);
            if (selectedColumn === 7) {
                mvpdSelectize.open();
            }
            this.selectizeInstances.push(mvpdSelectize);
        }

        //FLIGHT
        var $flightElem = $("#flight-" + record.recid);
        var $flightInput = $flightElem.find(".edit-input");
        var startDate = record.StartDate ? moment(record.StartDate) : moment(this.activeCopyData.TrafficStartDate);
        var endDate = record.EndDate ? moment(record.EndDate) : moment(this.activeCopyData.TrafficEndDate);
        var flightWeeks = record.FlightWeeks;
        //Build the histus weeks into the daterangepicker
        var buildHiatusCheckbox = function (FlightWeek) {
            var isChecked = FlightWeek["Selected"];
            var isTrafficHiatusWeek = FlightWeek["HiatusWeek"];
            var weekString = FlightWeek["StartDate"].substring(0, 10);
            var num = FlightWeek["MediaWeekId"];
            return '<li' + (isTrafficHiatusWeek == true ? ' class="hiatus_strikeout-label"' : '') + '>' + '<input value="' + weekString + '" type="checkbox" id="checkbox' + num + '"' + (isChecked == true ? 'checked' : '') + '>' + weekString + '</li>';
        };
        // Get some Hiatus weeks from data, this is sample data atm
        var hiatusHTML = '<h5>Flight Weeks</h4>';
        for (var entry in flightWeeks) {
            hiatusHTML += buildHiatusCheckbox(flightWeeks[entry]);
        }
        hiatusHTML += '';
        var daterangePicker = $flightInput.daterangepicker({
            locale: {
                // Extra settings that the daterangepicker uses; we specify one custom field here
                firstDay: 1     // This is the only custom field we need as of right now (06/13/2016) and it specifies what day of the week to start on
                // "firstDay": 1 displays monday as the first day of the week
            },
            template: '<div class="daterangepicker dropdown-menu">' +
            '<div class="calendar left">' +
                '<div class="daterangepicker_input">' +
                  '<input class="input-mini form-control" type="text" name="daterangepicker_start" value="" />' +
                  '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                  '<div class="calendar-time">' +
                    '<div></div>' +
                    '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                  '</div>' +
                '</div>' +
                '<div class="calendar-table"></div>' +
            '</div>' +
            '<div class="calendar right">' +
                '<div class="daterangepicker_input">' +
                  '<input class="input-mini form-control" type="text" name="daterangepicker_end" value="" />' +
                  '<i class="fa fa-calendar glyphicon glyphicon-calendar"></i>' +
                  '<div class="calendar-time">' +
                    '<div></div>' +
                    '<i class="fa fa-clock-o glyphicon glyphicon-time"></i>' +
                  '</div>' +
                '</div>' +
                '<div class="calendar-table"></div>' +
            '</div>' +
            '<div class="ranges hiatus">' +
                hiatusHTML +
                '<div class="range_inputs">' +
                    '<button class="applyBtn" disabled="disabled" type="button"></button> ' +
                    '<button class="cancelBtn" type="button"></button>' +
                '</div>' +
            '</div>' +
        '</div>',
            startDate: startDate,
            endDate: endDate,
            minDate: moment(this.activeCopyData.TrafficStartDate).format("M/D/YYYY"),
            maxDate: moment(this.activeCopyData.TrafficEndDate).format("M/D/YYYY"),

        },
        function (start, end, label) {
            var startDt = start.format('YYYY-MM-DD');
            var endDt = end.format('YYYY-MM-DD');
            var weeks = _this.getFlightWeeksForDateRange(startDt, endDt);
            daterangePicker.data('daterangepicker').container.find('.ranges.hiatus').find("input[type='checkbox']").each(
                function (i, val) {
                    var inRange = false;
                    for (var i = 0; i < weeks.length; ++i) {
                        if (weeks[i].StartDate.substring(0, 10) == $(this).val()) {
                            inRange = true;
                        }
                    }
                    if (inRange == true) {
                        $($(this).context.parentElement).removeClass("hidden");
                    } else if (inRange == false) {
                        $($(this).context.parentElement).addClass("hidden");
                    }
                }
            );
        }
    );
        // This Code is needed to disable the event handlers that the daterangepicker uses and add our own
        daterangePicker.data('daterangepicker').container.find('.ranges.hiatus')
            .off('click.daterangepicker', 'li')
            .off('mouseenter.daterangepicker', 'li')
            .off('mouseleave.daterangepicker', 'li')
        // Our custom event handler for the flightweek checkboxes
            .on('change', ':checkbox', function (event) {
                var mediaweekId = event.originalEvent.target.id.replace("checkbox", "");
                var recid = _this.activeEditRecord.recid;
                var record = _this.$CopyGrid.get(recid);
              
                for (var i = 0; i < record.FlightWeeks.length; ++i) {
                    if (record.FlightWeeks[i]["MediaWeekId"] == mediaweekId) {
                        record.FlightWeeks[i]["Selected"] = $(this).is(":checked");
                    }
                }
            });
        if (selectedColumn === 5) {
            setTimeout(function () {
                $flightInput.click();
            });
        }
        this.$dateRangeElem = $flightInput;

        //ROTATION
        var $rotationElem = $("#rotation-" + record.recid);
        var $rotationInput = $rotationElem.find(".edit-input");
        var rotval = record.Rotation || 100;
        $rotationInput.val(rotval);
        $rotationInput.keyup(function () {
            var newValue = this.value.replace(/[^0-9\.]/g, '');
            //if (newValue === "0") {
            //    newValue = 1;
            //}
            if (newValue > 100) {
                newValue = 100;
            }
            if (newValue != this.value) {
                this.value = newValue;
            }
        });
        if (selectedColumn === 6) {
            $rotationInput.focus();
        }



        //COMMENTS
        var $commentsElem = $("#comments-" + record.recid);
        var $commentsDivInp = $commentsElem.find(".edit-input");
        var _this = this;

        if (record.RootCopy.Hd) {
            $commentsDivInp.popover({
                html: true,
                title: "Comments",
                container: "body",
                placement: "bottom",
                trigger: "manual",
                content: this.getCommentsPopoverContent(record.SdComment, record.HdComment),
            });

            $commentsDivInp.on("shown.bs.popover", function () {
                var $popover = $(".comments-popover");
                var record = _this.$CopyGrid.get(_this.activeEditRecord.recid);
                $popover.find("#SDCommentsInput").val(record.SdComment || '');
                $popover.find("#HDCommentsInput").val(record.HdComment || '');
            });
            $commentsDivInp.on("click", function () {
                $commentsDivInp.popover('show');
            });
            
            if (selectedColumn === 8) {
                _.delay(function () {
                    $commentsDivInp.popover("show");
                }, 100);
            }

            this.$commentsDiv = $commentsDivInp;
        } else {
            $commentsDivInp.val(record.SdComment);
             if (selectedColumn === 8) {
                 $commentsDivInp.focus();
             }
        }
    },

    getCommentsPopoverContent: function () {
        var html = "<div class='comments-popover'>";

        // SD comments
        html += '<div class="form-group" style="width: 450px">';
        html += '<label for="SDCommentsInput">SD</label>'
        html += '<input type="text" class="form-control" id="SDCommentsInput" placeholder="SD comments">';
        html += '</div>';

        // HD comments
        html += '<div class="form-group">';
        html += '<label for="HDCommentsInput">HD</label>'
        html += '<input type="text" class="form-control" id="HDCommentsInput" placeholder="HD comments">';
        html += '</div>';

        html += '<div>';
        html += '<button id="apply-comment-popover" class="btn btn-success">Apply</btton>';
        html += '<button id="cancel-comment-popover" class="btn" style="margin-left: 10px;">Cancel</btton>';
        html += '</div>';

        html += "</div>";
        return html;
    },

    saveComments: function () {
        var $popover = $(".comments-popover");
        var sdComments = $popover.find("#SDCommentsInput").val();
        var hdComments = $popover.find("#HDCommentsInput").val();

        var record = this.$CopyGrid.get(this.activeEditRecord.recid);
        record.SdComment = sdComments;
        record.HdComment = hdComments;

        var $cell = $("#comments-" + this.activeEditRecord.recid);
        $cell.find("#comments-div-popover").html(sdComments);
        $cell.find(".hd-comment-icon").tooltip('destroy');
        $cell.find(".hd-comment-icon").attr("title", hdComments)
            .tooltip({ container: 'body' });

        this.$commentsDiv.popover('hide');
    },

    cancelComments: function() {
        this.$commentsDiv.popover('hide');
    },

    handleOutsideClick: function (e) {
        var clientX = e.clientX;
        var clientY = e.clientY;
        if (e.target.id === 'comments-div-popover') {
            return;
        }

        var $container = $(".popover");
        if (!$container[0]) {
            return;
        }
        var clientRect = $container[0].getBoundingClientRect();
        var isInside =
            (clientX >= clientRect.left && clientX <= clientRect.right) &&
            (clientY >= clientRect.top && clientY <= clientRect.bottom);

        if (!isInside) {
            this.$commentsDiv.popover('hide')
        }
    },

    //implemented to provided alternative Id system in CopyUpdate instance for bulk handling
    getNewCopyRecordId: function () {
        return 'N-1';

    },

    onSelectISCI: function (isci) {
        var recid = this.getNewCopyRecordId();
        var newRecord = $.extend({}, isci, {
            recid: recid,
            editingNewCopy: true,
            MarriedCopies: [],
            FlightWeeks: this.activeCopyData.TrafficFlightWeeks
        });

        if (isci.ChildCopy1 && isci.ChildCopy2) {
            newRecord.MarriedCopies = [isci.ChildCopy1, isci.ChildCopy2];
        }

        if (isci.newIsci) {
            newRecord.RootCopy.CopyId = null;
        } else {
            this.selectedReel = isci.RootCopy.LatestReelId;
            newRecord.ReelId = isci.RootCopy.LatestReelId;
        }

        //371 bug - set the default comment initially if HD
        if (isci.RootCopy.Hd) {
            newRecord.HdComment = 'RUN ON HD NETWORKS WITHIN ORDER.';
        }
        this.$CopyGrid.remove(0);
        this.$CopyGrid.add(newRecord);

        this.startActiveEdit(newRecord);
        this.initializeEditors(null, 3);
        //console.log('onSelectISCI', newRecord);
    },

    getFlightWeeksForDateRange: function (startDate, endDate) {
        return this.activeCopyData.TrafficFlightWeeks.filter(function (x) {
            return ((x.StartDate >= startDate && x.EndDate <= endDate) ||
            (x.StartDate < startDate && x.EndDate > startDate) ||
            (x.EndDate > endDate && x.StartDate < endDate));
        });
    },

    cleanISCIEditor: function (rowId) {
        var $isciInput = $("#isci-" + rowId).find(".edit-input");
        if ($isciInput.length) {
            $isciInput.data("plugin_isciDropdown").destroy();
        }
    },

    clearEditorsInstances: function () {
        if (this.$dateRangeElem) {
            this.$dateRangeElem.data('daterangepicker').remove();
            this.$dateRangeElem = null;
        }
        this.selectizeInstances.forEach(function (elem) {
            elem.destroy();
        })
        this.selectizeInstances = [];
    },

    setViewCopyActions: function () {
        var me = this;
        if (!this.actionsSet) {
            $("#copy_grid").on("click", "a[name='save']", function () {
                var id = me.activeEditRecord.recid; // $(this).data('recid');
                var rec = me.$CopyGrid.get(id);
                var copy = me.getSaveCopyData(rec);
                if (copy) me.controller.saveCopy(copy);
                if (me.$commentsDiv) {
                    me.$commentsDiv.popover("destroy");
                }
            });
            $("#copy_grid").on("click", "a[name='cancel']", function () {
                var id = me.activeEditRecord.recid;
                var rec = me.$CopyGrid.get(id);
                if (rec) {
                    me.selectedReel = null; //cleaning value
                    me.cleanISCIEditor(id);
                    me.clearEditorsInstances();
                    if (me.$commentsDiv) {
                        me.$commentsDiv.popover("destroy");
                    }
                    if (rec.editingNewCopy || rec.creatingCopy) {
                        me.endActiveEdit();
                        me.$CopyGrid.remove(id);

                        me.$CopyGrid.add({ recid: 0, newCopy: true, RootCopy: { SpotLengthDisplay: 'new' } });
                    } else {
                        me.cancelCopyRecord(rec);
                    }
                }
            });

            this.actionsSet = true;
        }
    },
});