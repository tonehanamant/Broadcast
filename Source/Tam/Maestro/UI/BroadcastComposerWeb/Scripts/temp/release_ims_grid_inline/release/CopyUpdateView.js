var CopyUpdateView = CopyBulkUpdateModalView.extend({

    hasStoredChangesForSave: false,
   
    initView: function (controller) {
        //Fixing Select2 problem inside a modal
        //http://stackoverflow.com/questions/18487056/select2-doesnt-work-when-embedded-in-a-bootstrap-modal
        $.fn.modal.Constructor.prototype.enforceFocus = function () { };

        this.controller = controller;
        this._enableEditingOnReleasedOrders = true;
        this._bulkEditOrders = [];
        this._currentOrderIndex = undefined;
        this.effectiveDate = new Date();
      
        this.initializeSelects();

        var me = this;
        var cfg = config.getOrdersGridConfig(this);
        cfg.onDblClick = this.onDblClickRow.bind(this);
        //cfg.onClick = this.onGridClick.bind(this);
        cfg.onMenuClick = this.onGridOrdersMenuClick.bind(this);
        cfg.onSelect = function () {
            me.$editOrdersBtn.removeAttr("disabled");
        };
        cfg.onUnselect = function () {
            me.$editOrdersBtn.attr("disabled", true);
        };

        //BUTTON EVENTS
        $("#saveAlertModalBtn").on("click", this.createTrafficAlertAndClose.bind(this));
        $("#saveAlertNext").on("click", this.createTrafficAlertAndGoNext.bind(this));
        $("#backModalButton").on("click", this.createTrafficAlertAndGoBack.bind(this));
        $(".close-modal1").on("click", this.onCloseModal.bind(this))

        this.$editOrdersBtn = $("#editOrdersBtn");
        this.$editOrdersBtn.on("click", this.onEditOrders.bind(this));

        this.$OrdersGrid = $('#orders_grid').w2grid(cfg);

        this.$copiesGridModal = $("#copiesGridModal");
        var gridConfig = this.getCopiesGridConfig();
        gridConfig.header = undefined;
        gridConfig.show.header = false;
        this.$CopyGrid = this.$copiesGridModal.find("#copy_grid").w2grid(gridConfig);

        this.hasChangedEffectiveDate = false;
        var $effectiveDatepicker = $('#effective_date_datepicker');
        this.$effectiveDatepicker = $effectiveDatepicker;
        $effectiveDatepicker.daterangepicker({
            "singleDatePicker": true,
        }, function (start, end, label) {
            //console.log('$effectiveDatepicker select...', start, this);
            this.hasChangedEffectiveDate = true;
            this.$effectiveDatepicker.parent().removeClass('has-error');
            this.effectiveDate = start.toDate();
        }.bind(this));

        this.$copiesGridModal.modal({
            show: false,
            backdrop: 'static',
        });

        window.onbeforeunload = function () {
            return me.hasActiveEdit ? 'Are you sure you want to close? Your changes have not been saved.' : undefined;
        }
    },

    getUseCaseId: function () {
        return 2; // Copy Update
    },

    initializeSelects: function () {
        var _this = this;
        var url = baseUrl + '/api/CopyUpdate/Iscis';

        var $isciSelect = $("#select-isci");
        $isciSelect.select2({
            placeholder: "ISCI",
            ajax: {
                url: url,
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        IsciLike: params.term, // search term
                        PageIndex: params.page,
                        PageSize: 40,
                    };
                },
                processResults: function (response, params) {
                    params.page = params.page || 1;
                    return {
                        results: response.Data.IsciList.map(function (x) { return { id: x, text: x } }),
                        pagination: {
                            more: response.Data.MoreRowsExists
                        }
                    };
                },
                cache: true
            },
        });
        $isciSelect.on('select2:select', function (evt) {
            var isci = evt.params.data;
            _this.lastISCISelected = isci.id;
            _this.controller.apiLoadOrders(isci.id, _this.onLoadOrders.bind(_this));
        });
    },

    attachGridEvents: function() {
        this.$copiesGridModal.off("hidden.bs.modal");
        this.$copiesGridModal.off("shown.bs.modal");
        this.$copiesGridModal.on("shown.bs.modal", function (e) {
            this.$CopyGrid.resize();
        }.bind(this));

        var _this = this;
        this.$copiesGridModal.on("hidden.bs.modal", function (e) {
            this.controller.apiUnlockTrafficOrders(this._bulkEditOrders, function (response) {
                _this.controller.apiLoadOrders(_this.lastISCISelected, _this.onLoadOrders.bind(_this));
            });
            this.$OrdersGrid.selectNone();
            //$("#editOrdersBtn").attr("disabled", true);
        }.bind(this));
    },

    onEditMultipleOrders: function (ids) {
        this.attachGridEvents();
        this._currentOrderIndex = 0;
        if (ids.length > 1) {
            $("#releasecopytitle").html("1 of " + ids.length + " - ");
            $("#saveAlertNext").show();
            $("#saveAlertModalBtn").hide();
            this._bulkEditOrders = ids;
            this._currentOrderIndex = 0;
        }

        var _this = this;
        this.controller.apiLockTrafficOrders(ids, function (response) {
            if (response.Success) {
                _this.startEditModal(_this._bulkEditOrders[_this._currentOrderIndex]);
            } else {
                util.alert("Error", response.ErrorMessage);
            }
        });
    },

    onEditSingleOrder: function (id) {
        this._currentOrderIndex = 0;
        this.attachGridEvents();
        $("#saveAlertNext").hide();
        $("#saveAlertModalBtn").show();
        $("#releasecopytitle").html("1 of 1 - ");

        var _this = this;
        this._bulkEditOrders = [id];
        this.controller.apiLockTrafficOrders(this._bulkEditOrders, function (response) {
            if (response.Success) {
                _this.startEditModal(id);
            } else {
                util.alert("Error", response.ErrorMessage);
            }
        });
    },

    onGridOrdersMenuClick: function (event) {
        if (event.menuIndex === 0) { //Bulk Editing
            this.onEditOrders();
        };
    },

    onEditOrders: function() {
        var selectedRecids = this.$OrdersGrid.getSelection();
        
        if (selectedRecids.length > 1) {
            this.onEditMultipleOrders(selectedRecids);
        } else if (selectedRecids.length == 1) {
            this.onEditSingleOrder(selectedRecids[0]);
        };
    },

    onDblClickRow: function (event) {
        this.onEditSingleOrder(event.recid);
    },

    closeCopyModal: function () {
        if (this.activeEditRecord) {
            this.cleanISCIEditor(this.activeEditRecord.recid);
            this.clearEditorsInstances();
        }
        this.clearEditingData();
        //this.controller.apiLoadOrders(this.lastISCISelected, this.onLoadOrders.bind(this));
        this.$copiesGridModal.modal("hide");
    },

    onLoadOrders: function (data) {
        this.LocalData = data.OrderDetails.map(function (x) { x.recid = x.TrafficId; return x; });
        this.$OrdersGrid.clear();
        this.$OrdersGrid.add(this.LocalData);

        var _this = this;

        //ADVERTISERS
        var $advertiserSelect = $("#select-advertiser");
        if ($advertiserSelect.data('select2')) {
            //Select 2 bug to clean oldvalues https://github.com/select2/select2/issues/2830
            $advertiserSelect.html('');
            $advertiserSelect.select2('destroy');
        }
        $advertiserSelect.select2({
            placeholder: 'Advertiser',
            allowClear: true,
            data: data.Advertisers.map(function (x) { return { id: x.Id, text: x.Display } })
        }).select2('val', undefined);
        $advertiserSelect.on('select2:select', function (evt) {
            _this.selectedAdvertiser = evt.params.data;
            _this.filterGrid();
        });
        $advertiserSelect.on('select2:unselect', function (evt) {
            _this.selectedAdvertiser = null;
            _this.filterGrid();
        });


        //PRODUCTS
        var $productsSelect = $("#select-products");
        if ($productsSelect.data('select2')) {
            //Select 2 bug to clean oldvalues https://github.com/select2/select2/issues/2830
            $productsSelect.html('');
            $productsSelect.select2('destroy');
        }
        $productsSelect.select2({
            placeholder: 'Product',
            allowClear: true,
            data: data.Products.map(function (x) { return { id: x.Id, text: x.Display } })
        }).select2('val', undefined);
        $productsSelect.on('select2:select', function (evt) {
            _this.selectedProduct = evt.params.data;
            _this.filterGrid();
        });
        $productsSelect.on('select2:unselect', function (evt) {
            _this.selectedProduct = null;
            _this.filterGrid();
        });
    },

    filterGrid: function () {
        var records = this.LocalData.concat();
        var _this = this;

        if (this.selectedAdvertiser) {
            records = records.filter(function (x) {
                return x.Advertiser.Id == _this.selectedAdvertiser.id;
            });
        }

        if (this.selectedProduct) {
            records = records.filter(function (x) {
                var hasProduct = false;
                for (var i = 0; i < x.Products.length; i++) {
                    if (_this.selectedProduct.id == x.Products[i].Id) {
                        hasProduct = true;
                        break;
                    }
                }
                return hasProduct;
            });
        }

        this.$OrdersGrid.clear();
        this.$OrdersGrid.add(records);
    },
});