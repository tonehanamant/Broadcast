
//rates view: grid inside modal and detail
//todo - use the object in checkSaveContact as stored object for all these types of forms - to process set, get, validate, save, etc?
var RateStationView = BaseView.extend({
    isActive: false,

    $StationsGrid: null,
    $RatesGrid: null,
    $ContactsGrid: null,
    $StationModal: null,
    EditRateView: null,
    EditRateThirdPartyView: null,
    NewRateView: null,
    EndFlightView: null,
    activeStationData: null,
    activeStationRateRecord: null, //the active record for a rate update
    activeContactEditRecord: null,
    activeRatesFilter: null,

    isThirdPartyMode: false, //flag to determine handling for various modes

    // prior to modal open
    initView: function (controller) {
        var me = this;
        me.controller = controller;
        me.$RatesGrid = $('#rates_grid').w2grid(RateConfig.getRatesGridCfg(this));
        me.$ContactsGrid = $('#contacts_grid').w2grid(RateConfig.getContactsGridCfg(this));
        me.initContactsGridEvents();

        // handlers
        $("#rates_new_program_btn").on('click', me.setNewRatesProgram.bind(this));

        // unlock station on window/tab close
        //$(window).on('beforeunload', function () {
        //    if (me.activeStationData) {
        //        me.controller.appController.apiGetStationUnlock(me.activeStationData.StationCode);
        //    }
        //});
    },

    // set the active station session
    //will now depen on thirdParty
    setActiveStation: function (stationData, isThirdParty) {
        var me = this;
        this.activeStationData = stationData;
        this.isThirdPartyMode = isThirdParty;

        if (!this.$StationModal) {
            this.$StationModal = $('#stationModal');
            //scope within event is modal
            this.$StationModal.on('shown.bs.modal', function (event) {
                if (!me.isActive) {
                    // handle any post modal open events/grid          
                    me.isActive = true;
                }
                $('#station_tabs a[href="#station_rates_view"]').tab('show');
                me.setStationGrids();
            });


            //on close modal - controller refresh all stations via main App controller
            this.$StationModal.on('hidden.bs.modal', function (event) {
                me.controller.onStationClose();
            });

            //change more specific as additional tabs in views
            $('#station_tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                //e.target // newly activated tab
                //e.relatedTarget // previous active tab
                //console.log('tab', e);
                if (e.target.name == 'rates') {
                    //resize only can cause bug when going back on reset
                    me.$RatesGrid.refresh();
                    // if rates tab - me.$RatesGrid,resize();
                }

                if (e.target.name == 'contacts') {
                    me.$ContactsGrid.refresh();
                    //reset - remove editing state if present
                    me.endActiveContactEdit(true);
                }
            });

            this.$StationModal.modal({
                backdrop: 'static',
                show: false,
                keyboard: false
            });
        }

        this.$StationModal.modal('show');
    },

    /*** STATION GRIDS RELATED ***/

    setStationGrids: function () {
        this.setRatesGrid();
        this.setContactsGrid();
    },

    // prepare rates grid data with non mutaing copy as needed - the Id from the BE is non-unique (multiple programs possible) , so use index based
    prepareRatesGridData: function (data) {
        var displayData = util.copyData(data);
        var ret = [];
        $.each(displayData, function (index, value) {
            var item = value;
            item.recid = index + 1;
            ret.push(item);
        });

        return ret;
    },

    // prepares Rates grid add data; clear filters
    setRatesGrid: function () {
        var ratesData = this.prepareRatesGridData(this.activeStationData.Rates);
        this.$RatesGrid.searchReset();
        this.$RatesGrid.clear(false);
        this.$RatesGrid.add(ratesData);
        this.showRatesGridThirdpartyColumns(this.isThirdPartyMode);
        this.$RatesGrid.resize();
        this.setRatesGridDateRangeFilter();
    },

    updateRatesGrid: function (programs, reset) {
        var ratesData = this.prepareRatesGridData(programs);
        this.$RatesGrid.clear(false);
        this.$RatesGrid.add(ratesData);
        this.$RatesGrid.resize();
        if (reset) this.resetRatesFilters();
    },


    showRatesGridThirdpartyColumns: function (show) {
        if (show) {
            this.$RatesGrid.hideColumn('Rate30', 'Rate15', 'Impressions', 'Rating');
            this.$RatesGrid.showColumn('Spots');

        } else {
            this.$RatesGrid.hideColumn('Spots');
            this.$RatesGrid.showColumn('Rate30', 'Rate15', 'Impressions', 'Rating');
        }
    },

    setRatesGridDateRangeFilter: function () {
        var me = this;    

        $('#station_rates_date_filter').daterangepicker({
            autoUpdateInput: false,
            opens: 'left',
            startDate: new Date()
            },
            function (start, end, label) {
                //dateRange.start = start;
                //dateRange.end = end;

                //me.controller.apiFilterPrograms(dateRange, stationCode);
        });

        this.setRatesFilterListeners();
        this.resetRatesFilters();
    },

    //drp events need to be reapplied - but not select change event
    //avoid use of hide as called before others and unreliable to set states; use cancel/outsideClick
    setRatesFilterListeners: function () {
        var me = this;

        $('#station_rates_date_filter').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $('#station_rates_quarter_filter').val('Custom');
            var dateRange = {start: picker.startDate, end: picker.endDate};
            me.controller.apiFilterPrograms(dateRange, me.activeStationData.StationCode);
            me.activeRatesFilter = 'Custom';
        });

        $('#station_rates_date_filter').on('outsideClick.daterangepicker', function (ev, picker) {
            me.checkRatesFilter();
        });

        $('#station_rates_date_filter').on('cancel.daterangepicker', function (ev, picker) {
            me.checkRatesFilter();
        });

        if (!this.activeRatesFilter) {
            $('#station_rates_quarter_filter').on('change', function (evt) {
                //console.log('select', this.value);
                switch (this.value) {
                    case 'Custom':
                        $("#station_rates_date_filter").focus();
                        break;
                    default:
                        $('#station_rates_date_filter').val('');
                        //change the start date in DRP back to defaults
                        $('#station_rates_date_filter').data('daterangepicker').setStartDate(new Date());
                        $('#station_rates_date_filter').data('daterangepicker').setEndDate(null);
                        me.controller.apiFilterPrograms(this.value, me.activeStationData.StationCode);
                        me.activeRatesFilter = this.value;
                }
               
            });
        }

    },
    //check when drp cancel/outside clicl to set appropriate state
    checkRatesFilter: function () {
        //console.log('checkRatesFilter', this.activeRatesFilter);
        if (this.activeRatesFilter == 'Custom') {

        } else {
            $('#station_rates_date_filter').val('');
            $('#station_rates_quarter_filter').val(this.activeRatesFilter);
        }

    },

    resetRatesFilters: function () {
        //clean values after reopening
        $('#station_rates_date_filter').val('');
        $('#station_rates_quarter_filter').val('All');
        this.activeRatesFilter = 'All'
    },

    //grid events - set in config
    //now based on isThirdPartyMode
    onRatesDoubleClick: function (event) {
        //Do not overwrite ; this.activeStationData is the full set
        this.activeStationRateRecord = this.$RatesGrid.get(event.recid);
        if (this.isThirdPartyMode) {
            this.setEditRatesProgramThirdParty();
        } else {
            this.setEditRatesProgram();
        }
    },

    onRatesContextMenu: function (event) {
        this.$RatesGrid.menu = [
            { id: 1, text: 'Delete', disabled: false },
            { id: 2, text: 'End Program Flight', disabled: false }
        ];
    },

    onRatesMenuClick: function (event) {
        var rec = this.$RatesGrid.get(event.recid),
            itemId = rec.Id,
            menuId = event.menuItem.id;

        switch (menuId) {
            case 1:
                var title = 'Delete this program',
                    message = 'Are you sure you want to delete ' + rec.Program + ' (' + itemId + ')' + '?';
                //change to pass record - as BE needs additional data
                util.confirm(title, message, this.controller.apiDeleteStationRate.bind(this.controller, rec));
                break;
            case 2:
                this.setEndFlightProgram(rec);
                break;
        }
    },

    // remove a record from grid (following api delete)
    removeStationRateFromGrid: function (rec) {
        if (rec && rec.recid) {
            this.$RatesGrid.remove(rec.recid);
        }

    },

    // END FLIGHT - PROGRAM RATE
    //from context menu pass rec to modal
    setEndFlightProgram: function (rec) {
        if (!this.EndFlightView) {
            this.EndFlightView = new StationModalEndFlight(this);
            this.EndFlightView.initView();
        }
        this.EndFlightView.setEndFlight(rec);
    },


    /*** UPDATE RATES PROGRAM RELATED ***/

    // set edit program modal class
    setEditRatesProgram: function () {
        if (!this.EditRateView) {
            this.EditRateView = new StationModalEditRate(this);
            this.EditRateView.initView();
        }
        this.EditRateView.setEditRate(this.activeStationRateRecord);
    },

    //from controller - call EditRateView
    onAfterSaveUpdateRatesProgram: function () {

        if (this.isThirdPartyMode) {
            this.EditRateThirdPartyView.onAfterSaveRate();
        } else {
            this.EditRateView.onAfterSaveRate();
        }

    },

    // set edit program modal class - third party version
    setEditRatesProgramThirdParty: function () {
        if (!this.EditRateThirdPartyView) {
            this.EditRateThirdPartyView = new StationModalEditRateThirdParty(this);
            this.EditRateThirdPartyView.initView();
        }
        this.EditRateThirdPartyView.setEditRate(this.activeStationRateRecord);
    },



    /*** NEW RATES PROGRAM RELATED ***/

    //set new program modal - class
    setNewRatesProgram: function () {
        if (!this.NewRateView) {
            this.NewRateView = new StationModalNewRate(this);
            this.NewRateView.initView();
        }
        this.NewRateView.setNewRate(this.activeStationData.StationCode);
    },


    /*** CONTACTS RELATED ***/

    // prepare contacts grid data with non mutaing copy as needed
    //tbd - ID property, etc
    prepareContactsGridData: function (data) {
        var displayData = util.copyData(data);
        var ret = [];
        $.each(displayData, function (index, value) {
            var item = value;
            item.recid = item.Id;
            ret.push(item);
        });

        return ret;
    },

    // prepares Rates grid add data; clear filters; allow refresh of existing
    setContactsGrid: function (newContactsData) {
        if (newContactsData) this.activeStationData.Contacts = newContactsData;
        var contactsData = this.prepareContactsGridData(this.activeStationData.Contacts);
        //add New button in grid: summary: true would add at very bottom
        contactsData.push({ recid: 'addNew', addNew: true});
        //clear search - unless should maintain?
        this.clearContactsGridSearch(true);
        this.$ContactsGrid.clear(false);
        this.$ContactsGrid.add(contactsData);
    },

    initContactsGridEvents: function () {
        var me = this;
        this.$ContactsGrid.onClick = this.onContactsGridClick.bind(this);
        this.$ContactsGrid.onSort = function (event) {
            if (me.activeContactEditRecord) {
                event.preventDefault();
                return;
            };
        };
        //probaly need refresh as well?

        //set dynamic delegated events for the actions items
        $("#contacts_grid").on("click", "a[name='save']", function () {
            var id = me.activeContactEditRecord.recid; 
            var rec = me.$ContactsGrid.get(id);
            //TBD - and whether edit or add
            me.checkSaveContact(rec);

        });
        $("#contacts_grid").on("click", "a[name='cancel']", function () {
            var id = me.activeContactEditRecord.recid;
            var rec = me.$ContactsGrid.get(id);
            //TBD - cancel the edit - deal with possible newly created (isNew) and remove
            if (rec) {
                me.endActiveContactEdit(true);
            }
        });

        // Contacts search button - executes the search
        $("#station_contacts_view").on("click", '#station_contacts_search_btn', this.contactsGridTextSearch.bind(this));

        // Contacts input Enter key - executes the search
        $("#station_contacts_view").on("keypress", '#station_contacts_search_input', function (e) {
            var key = e.which;
            if (key == 13) {
                me.contactsGridTextSearch();
            }
        });

        $("#station_contacts_view").on("click", '#station_contacts_search_clear_btn', this.clearContactsGridSearch.bind(this));

        $("#station_contacts_new_contact_btn").on('click', this.setNewContactRecord.bind(this));

    },

    onRatesContactContextMenu: function (event) {
        //no delete if editing mode or if the record is addNew button;
        // console.log(event.recid);
        if (this.activeContactEditRecord || (event.recid == 'addNew')) {
            event.preventDefault();
            return false;
        }
        this.$ContactsGrid.menu = [
            { id: 1, text: 'Delete', disabled: false }
        ];
    },

    onRatesContactMenuClick: function (event) {
        //should not be shown in this case but do additional check
        if (this.activeContactEditRecord || (event.recid == 'addNew')) {
            event.preventDefault();
            return false;
        }
        var rec = this.$ContactsGrid.get(event.recid),
            itemId = rec.Id,
            menuId = event.menuItem.id;

        switch (menuId) {
            case 1:
                var title = 'Delete this contact',
                    message = 'Are you sure you want to delete ' + rec.Name + '?';

                util.confirm(title, message, this.controller.apiDeleteStationContact.bind(this.controller, rec));
                break;
        }
    },

    //user clciks grid: enable editors if applicable (not editing, not "add new"); deal with action column
    onContactsGridClick: function (event) {
        if (event.column === null) {
            return;
        }
        if (this.activeContactEditRecord) { 
            return;
        }
        var record = this.$ContactsGrid.get(event.recid);
        //if Add New Contact button in grid
        if (event.column === 0 && record.addNew === true) {
            this.setNewContactRecord();
        } else {
            //console.log('onContactsGridClick valid', record);
            this.startActiveContactEdit(record);
        }
    },


    removeStationContactFromGrid: function (rec) {

        if (rec && rec.recid) {
            this.$ContactsGrid.remove(rec.recid);
        }

    },

    //need to exclude search of 'addNew' or 'N-1' record
    contactsGridTextSearch: function () {
        if (this.activeContactEditRecord) return;
        var val = $("#station_contacts_search_input").val();
        if (val && val.length) {
            val = val.toLowerCase();
            var search = [{ field: 'recid', type: 'text', value: ['addNew'], operator: 'is' },{ field: 'recid', type: 'text', value: ['N-1'], operator: 'is' },{ field: 'Type', type: 'text', value: [val], operator: 'contains' }, { field: 'Name', type: 'text', value: [val], operator: 'contains' }, { field: 'Company', type: 'text', value: [val], operator: 'contains' }, { field: 'Email', type: 'text', value: [val], operator: 'contains' }, { field: 'Phone', type: 'text', value: [val], operator: 'contains' }, { field: 'Fax', type: 'text', value: [val], operator: 'contains' }];
            //this.$ContactsGrid.search('all', val);
            this.$ContactsGrid.search(search, 'OR');
            $("#station_contacts_search_clear_btn").show();
        } else {
            this.clearContactsGridSearch();
        }
    },

    // Clear search
    clearContactsGridSearch: function () {
        if (this.activeContactEditRecord) return;
        this.$ContactsGrid.searchReset();
        $("#station_contacts_search_input").val('');
        $("#station_contacts_search_clear_btn").hide();
    },

    //edit/add related contacts



    showContactsGridActionColumn: function (show) {
        if (show) {
            this.$ContactsGrid.showColumn('actions');
        } else {
            //hide all inner editors?
            $('.contact-edit-actions').hide();
            this.$ContactsGrid.hideColumn('actions');
        }
    },

    showContactsEditActions: function (show, recid) {
        var edit = $('#contact_actions_' + recid);
        if (show) {
            edit.show();
        } else {
            edit.hide();
        }
    },

    //show/hide the inner grid button
    showContactsGridAddNew: function (show) {
        var btn = $("#contacts_grid_add_new_btn");
        if (show) {
            btn.show();
        } else {
            btn.hide();
        }

    },

    setNewContactRecord: function () {
        if (this.activeContactEditRecord) return;
        var rec = {
            recid: 'N-1',
            Id: 0,
            StationCode: this.activeStationData.StationCode,
            isNew: true
        };
        this.$ContactsGrid.add(rec);
        this.startActiveContactEdit(rec);
        this.showContactsGridAddNew(false);

    },

    //set disabled items when in edioting mode; enable when not
    disableContactEditingAllowedStates: function (disable) {
        if (disable) {
            $(".contacts_disabled_editing").prop('disabled', true);
        } else {
            $(".contacts_disabled_editing").prop('disabled', false);
        }
    },

    startActiveContactEdit: function (rec) {
        var me = this;

        this.activeContactEditRecord = rec;
        this.showContactsGridActionColumn(true);
        this.showContactsEditActions(true, rec.recid);
        var $editElements = $("[contactRecidEdit='" + rec.recid + "']");
        $editElements.addClass("is-editing");

        var $inputs = $editElements.find(".edit-input");
        $inputs.show();

        this.disableContactEditingAllowedStates(true);
        this.setContactEditData(rec);
    },

    endActiveContactEdit: function (removeNewCheck) {
        if (removeNewCheck) {
            var newrec = this.$ContactsGrid.get('N-1');
            if (newrec) this.$ContactsGrid.remove('N-1');
            this.showContactsGridAddNew(true);
        }
        
        this.showContactsGridActionColumn(false);//hide column? this effeciively hides after each edit or initially
        if (this.activeContactEditRecord) {
            this.showContactsEditActions(false);
            var $editElements = $("[contactRecidEdit='" + this.activeContactEditRecord.recid + "']");
            $editElements.removeClass("is-editing");
            this.disableContactEditingAllowedStates(false);
            this.activeContactEditRecord = null;
        }
    },

    //set the values for editin based on active record;  
    setContactEditData: function (rec) {
        var typeId = rec.Type === 'Station' ? 1 : (rec.Type === 'Rep' ? 2 : (rec.Type === 'Traffic') ? 3 : 0);
        $('[name="contact_type_select_' + rec.recid + '"]').val(typeId);
        $('[name="contact_name_input_' + rec.recid + '"]').val(rec.Name);
        $('[name="contact_company_input_' + rec.recid + '"]').val(rec.Company);
        $('[name="contact_email_input_' + rec.recid + '"]').val(rec.Email);
        $('[name="contact_phone_input_' + rec.recid + '"]').val(rec.Phone);
        $('[name="contact_fax_input_' + rec.recid + '"]').val(rec.Fax);
    },

    //validate, if ok get values from each input; pass to controller; 
    //adapt this to also get values for save and doContactSave if valid
    checkSaveContact: function (rec) {
        var canSend = true;
        var fields = [
            { selector: '[name="contact_type_select_' + rec.recid + '"]', required: true, key: 'Type' },
            { selector: '[name="contact_name_input_' + rec.recid + '"]', required: true, key: 'Name' },
            { selector: '[name="contact_company_input_' + rec.recid + '"]', required: false, key: 'Company' },
            { selector: '[name="contact_email_input_' + rec.recid + '"]', required: true, key: 'Email' },
            { selector: '[name="contact_phone_input_' + rec.recid + '"]', required: true, key: 'Phone' },
            { selector: '[name="contact_fax_input_' + rec.recid + '"]', required: false, key: 'Fax' }
        ];
        
        //Id/StationCode add/edit; 0 is for new
        var retObj = {
            Id: rec.isNew ? 0 : (rec.Id || 0),
            StationCode: rec.StationCode
        };

        fields.forEach(function (field) {
            var value = $(field.selector).val(),
                isRequired = field.required;

            $(field.selector).parent().removeClass('has-error');

            retObj[field.key] = value;
            // !!! coherces type from falsy/truthy (such as 0, undefined or '' to false (boolean))
            if (isRequired && !!!value) {
                canSend = false;
                $(field.selector).parent().addClass('has-error');
            }

            //if (field.selector.indexOf('email') > -1 && !(/^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+[a-z][a-z])|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i.test(value))) {
            //per bug discussion just changing to simple "@" check
            var emailpatt = new RegExp("@");
            if (field.selector.indexOf('email') > -1 && !emailpatt.test(value)) {
                canSend = false;
               
                $(field.selector).parent().addClass('has-error');
            } 
        });
        if (canSend) {
            this.doSaveContact(retObj, rec.isNew);
        }
        //return canSend;
    },

    //get values for be and save
    doSaveContact: function (contactObj, isNew) {
        contactObj.Type = parseInt(contactObj.Type);
        //console.log('doSaveContact', contactObj);
        this.controller.apiAddEditStationContact(contactObj, isNew);

    },

    //after save
    onAfterSaveContact: function (data) {
        this.endActiveContactEdit();
    }

});