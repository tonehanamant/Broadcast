var CopyBulkUpdateModalView = BaseView.extend({

    //override Base CopyUpdate need to increment the controller newId and use here for Id by controller
    getNewCopyRecordId: function () {
        var id = 'N-' + this.controller.newCopyCounter;
        this.controller.incrementNewCopyCounter();
        return id;

    },

    //overide base - set the row with updated data
    //change to handle response - if data update
    //need to set properties then add a new record for add as not calling setGrid
    onAfterSaveCopy: function (recid, newData) {
        this.endActiveEdit();
        if (newData) {
            //the record sate is wrong when a new record
            newData.creatingCopy = false;
            newData.newCopy = false;
            this.$CopyGrid.set(recid, newData);
            //if new item so add another new record - but need to determine if reediting before save of a new; check recid 0 (Add New) as well

            if ((jQuery.type(recid) === 'string') && (recid.indexOf('N-') != -1)) {
                //console.log('onAfterSave recid = 0', this.$CopyGrid.get(0));
                var hasNewAdd = this.$CopyGrid.get(0);
                if (!hasNewAdd) this.$CopyGrid.add({ recid: 0, newCopy: true, RootCopy: { SpotLengthDisplay: 'new' } });
            }
        }
        //this.clearEditorsInstances();
    },

    //in bulk storage  - remove the copy from the grid (non-binding to BE), prior to save
    onGridDeleteCopy: function (copyId) {
        this.$CopyGrid.remove(copyId);
    },


    //////////////Copy update Common

    startEditModal: function (id) {
        this.controller.apiLoadCopies(id);
    },

//only call if initial not after save copy
setTrafficDisplay: function (data) { //SET TRAFFIC VALUES AND OPEN THE MODAL ONCE THE COPIES WERE LOADED
    var $trafficAlertTypeSelect = $("#select-traffic-alert-type");
    this.$trafficAlertTypeSelect = $trafficAlertTypeSelect;
    this.cleanErrorsClasses();

    var _this = this;
    if ($trafficAlertTypeSelect.data('select2')) {
        $trafficAlertTypeSelect.off('select2:select');
        $trafficAlertTypeSelect.off('select2:unselect');
        //Select 2 bug to clean oldvalues https://github.com/select2/select2/issues/2830
        $trafficAlertTypeSelect.html('');
        $trafficAlertTypeSelect.select2('destroy');
    };

    var alertTypesOptions = data.ValidAlertTypes.map(function (x) { return { id: x.Id, text: x.Display } });

    $trafficAlertTypeSelect.select2({
        placeholder: 'Traffic Alert Type',
        allowClear: true,
        data: alertTypesOptions
    });

    this.selectedTrafficAlertType = data.TrafficAlertTypeId;
    $trafficAlertTypeSelect.on('select2:select', function (evt) {
        $trafficAlertTypeSelect.parent().removeClass('has-error');
        _this.selectedTrafficAlertType = evt.params.data.id;
    });
    $trafficAlertTypeSelect.on('select2:unselect', function (evt) {
        _this.selectedTrafficAlertType = null;
    });

    $trafficAlertTypeSelect.val(data.TrafficAlertTypeId);
    $trafficAlertTypeSelect.trigger("change.select2");


    var effectiveDate = new Date(data.EffectiveDate);
    if (effectiveDate.getFullYear() <= 1970) {
        this.effectiveDate = undefined;
        this.$effectiveDatepicker.val('');
    } else {
        this.effectiveDate = effectiveDate;
        this.$effectiveDatepicker.data('daterangepicker').setStartDate(effectiveDate);
        this.$effectiveDatepicker.data('daterangepicker').setEndDate(effectiveDate);
    }
    this.hasChangedEffectiveDate = false;
    $("#alert_comments").val(data.AlertComment);
    $("#copy_comments").val(data.CopyComment);

    $("#id-order").html("Order " + data.TrafficId);
    $("#title-order").html(" - " + data.TrafficName + " - ");
    $("#flightweek-order").html(data.FlightWeekString);
    if (!this._currentOrderIndex) {
        $("#backModalButton").hide();
        this.$copiesGridModal.modal('show');
    } else {
        $("#backModalButton").show();
    }
},

cleanErrorsClasses: function () {
    this.$effectiveDatepicker.parent().removeClass('has-error');
    this.$trafficAlertTypeSelect.parent().removeClass('has-error');
},

hasChanged: function () {
    if (this.hasStoredChangesForSave) return true;
    var changedAlertComments = this.activeCopyData.AlertComment != $("#alert_comments").val();
    var changedCopyComments = this.activeCopyData.CopyComment != $("#copy_comments").val();
    var changedEffectiveDate = this.hasChangedEffectiveDate;
    var changedAlertType = this.activeCopyData.TrafficAlertTypeId != this.selectedTrafficAlertType;

    return changedAlertComments || changedCopyComments || changedEffectiveDate || changedAlertType;
},

onCloseModal: function () {
    if (this.hasChanged()) {
        util.confirm("Warning", "Are you sure you want to close? Your changes have not been saved.", this.closeCopyModal.bind(this));
    } else {
        this.closeCopyModal();
    }
},

clearEditingData: function () {
    this.activeEditRecord = null;
    this.hasActiveEdit = false;
},

createTrafficAlertAndClose: function () {
    this.createTrafficAlert(this.closeCopyModal.bind(this));
},

createTrafficAlertAndGoNext: function () {
    this.createTrafficAlert(this.checkNextOrder.bind(this));
},

createTrafficAlertAndGoBack: function () {
    this.createTrafficAlert(this.checkPreviousOrder.bind(this));
},

checkNextOrder: function () {
    this._currentOrderIndex++;
    if ((this._currentOrderIndex) < this._bulkEditOrders.length) {
        $("#releasecopytitle").html("" + (this._currentOrderIndex + 1) + " of " + this._bulkEditOrders.length + " - ");
        this.startEditModal(this._bulkEditOrders[this._currentOrderIndex]);
    };

    if (this._currentOrderIndex == this._bulkEditOrders.length - 1) {
        $("#saveAlertNext").hide();
        $("#saveAlertModalBtn").show();
    };
},

checkPreviousOrder: function () {
    this._currentOrderIndex--;
    if (this._currentOrderIndex >= 0) {
        $("#releasecopytitle").html("" + (this._currentOrderIndex + 1) + " of " + this._bulkEditOrders.length + " - ");
        this.controller.apiLoadCopies(this._bulkEditOrders[this._currentOrderIndex]);
    };

    if (this._currentOrderIndex == 0) {
        $("#backModalButton").hide();
    };
},


createTrafficAlert: function (callback) {

    this.effectiveDate = this.$effectiveDatepicker.val();

    if (!this.effectiveDate) {
        this.$effectiveDatepicker.parent().addClass('has-error');
    }
    if (!this.selectedTrafficAlertType) {
        this.$trafficAlertTypeSelect.parent().addClass('has-error');
    }
    if (this.effectiveDate && this.selectedTrafficAlertType) {
        if (this.hasActiveEdit) {
            util.alert("Warning", "Please save your current work before proceeding.");
            return;
        }

        var _this = this;
        var alertObject = {
            Id: this.activeCopyData.TrafficAlertId,
            //Copies: controller will add
            AlertComment: $("#alert_comments").val(),
            CopyComment: $("#copy_comments").val(),
            EffectiveDate: this.effectiveDate,
            TrafficId: this.activeCopyData.TrafficId,
            TrafficAlertTypeId: this.selectedTrafficAlertType,
        };

        if (this.activeEditRecord) {
            this.cleanISCIEditor(this.activeEditRecord.recid);
            this.clearEditorsInstances();
        }
        this.clearEditingData();
        this.controller.apiSaveTrafficAlert(alertObject, callback);
    }
},

});