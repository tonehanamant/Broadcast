
/*Detail vm for a detail set - handles set header*/
function DetailVM(set, start, end) {
    var $scope = this;

    $scope.set = set;
    $scope.id = ko.observable(set.id);
    $scope.hasDetail = ko.observable(false);

    //FlIGHTS: determine initial start/end (EA vagues so may need to adjust: start - next Monday from today; end - Sunday 4 weeks from today
    var today = moment();
    var startDate = today.day() > 0 ? today.add(1, "week").startOf('week').weekday(1) : today.startOf('week').weekday(1);
    var endDate = today.clone().add(4, "week").startOf('week').weekday(0);
    startDate = startDate.format('YYYY-MM-DD');
    endDate = endDate.format('YYYY-MM-DD');
    $scope.FlightStartDate = ko.observable(startDate);
    $scope.FlightEndDate = ko.observable(endDate);
    $scope.FlightWeeks = ko.observable();

    //callback picker apply: initial or flight changes
    $scope.onFlightSelect = function (picker) {
        var params = { StartDate: $scope.FlightStartDate(), EndDate: $scope.FlightEndDate(), FlightWeeks: $scope.FlightWeeks() };
        $scope.set.setFlight(params);

    };

    //DAYPART
    //on daypart selection; wrapper based converted for observable/BE
    $scope.onDaypartSelect = function (dateValues) {
        $scope.Daypart(dateValues);
    };

    $scope.Daypart = ko.observable();
    $scope.daypartWrap = null;

    $scope.SpotLength = ko.observable();
    $scope.DaypartCode = ko.observable();
    $scope.Adu = ko.observable(false);
    $scope.checkedAdu = ko.observable(false);//Adu checkbox State
    $scope.aduCheckedSubscriber = null;

    // sweeps
    $scope.SharePostingBookId = ko.observable();
    $scope.HutPostingBookId = ko.observable();
    $scope.PlaybackType = ko.observable();

    $scope.setStart = function () {
        if (!$scope.daypartWrap) $scope.daypartWrap = new wrappers.daypartWrapper($("#proposal_detail_daypart_" + $scope.id()), '#proposal_modal', $scope.onDaypartSelect.bind($scope));
        if (!$scope.aduCheckedSubscriber) {
            //use the checkedAdu so can compare to the actual Adu - if difference then set Adu to new
            $scope.aduCheckedSubscriber = $scope.checkedAdu.subscribe(function (checked) {
                if ($scope.Adu() != checked) {
                    $scope.Adu(checked);
                    $scope.set.onEditAdu(checked);
                }
            });
        }
    };

    //initial setting/change (flights) versus loading// todo: future flight changes
    $scope.setInitialDetail = function () {
        $scope.checkDirty(false);
        //distinguish between initial SharePostingBookIdload and after getDetail by flight?
        $scope.hasDetail(true);
        $scope.setStart();
        $scope.Daypart(null);
        $scope.daypartWrap.remove(); //remove in case already set
        $scope.daypartWrap.init();
        $scope.SpotLength(null);
        $scope.DaypartCode(null);
        $scope.Adu(false); //tbd BE prop
        //set also the checked for subscribe
        $scope.checkedAdu(false);
        $scope.checkDirty(true);
        $scope.isDirty(false);
    };

    //load detail - existing/update
    $scope.loadDetail = function (detail) {
        $scope.checkDirty(false);
        $scope.hasDetail(true);
        $scope.setStart();
        $scope.Daypart(detail.Daypart);
        $scope.daypartWrap.remove();
        $scope.daypartWrap.init(detail.Daypart, true);
        $scope.SpotLength(detail.SpotLengthId);
        $scope.DaypartCode(detail.DaypartCode);
        $scope.FlightStartDate(detail.FlightStartDate);
        $scope.FlightEndDate(detail.FlightEndDate);
        $scope.FlightWeeks(detail.FlightWeeks);
        $scope.Adu(detail.Adu || false);
        $scope.checkedAdu(detail.Adu || false);
        $scope.checkDirty(true);
        $scope.isDirty(false);
        $scope.SharePostingBookId(detail.SharePostingBookId);
        $scope.HutPostingBookId(detail.HutPostingBookId);
        $scope.PlaybackType(detail.PlaybackType);
    };

    //tbd needed? - any clear mechanism on modal close, etc when all detail sets removed from proposalView OR set removed
    //as is would need to be called before the array removal in peoposal view of this item - else does not exist
    //$scope.clearDetail = function () {
    //    $scope.daypartWrap.remove();//remove the plugin
    //    //destroy flights picker?
    //};

    $scope.onRemoveDetail = function (setData) {
        $($scope.set).trigger('removeSet', [setData.id(), $scope.set]);
    };

    $scope.getSaveDetail = function () {
        // force daypart to close to trigger handlers
        $scope.daypartWrap.input.webuiPopover('hide');

        var ret = {
            FlightStartDate: $scope.FlightStartDate(),
            FlightEndDate: $scope.FlightEndDate(),
            FlightWeeks: $scope.FlightWeeks(),
            Daypart: $scope.Daypart(),
            DaypartCode: $scope.DaypartCode(),
            SpotLengthId: $scope.SpotLength(),
            Adu: $scope.Adu()
        };
        return ret;
    };

    $scope.openInventory = function () {
        $($scope.set).trigger('openInventory', [$scope.set]);
    };

    $scope.openOpenMarket = function () {
        $($scope.set).trigger('openMarket', [$scope.set]);
    };

    //DIRTY handling - just for base header changes - flight, adu will already change on update

    $scope.checkDirty = ko.observable(false);
    $scope.isDirty = ko.observable(false);

    $scope.setDirty = function (val) {
        if ($scope.checkDirty()) {
            //console.log('setDirty', $scope.checkDirty(), val);
            $scope.isDirty(true);
        }
    };

    $scope.detailHeaderDirty = function () {
        return $scope.checkDirty() ? $scope.isDirty() : false;
    };

    $scope.Daypart.subscribe($scope.setDirty);
    $scope.DaypartCode.subscribe($scope.setDirty);
    $scope.SpotLength.subscribe($scope.setDirty);

    $scope.openManageRatings = function () {
        $($scope.set).trigger('openManageRatings', [$scope.id(), $scope.SharePostingBookId(), $scope.HutPostingBookId(), $scope.PlaybackType()]);
    }
};

/*Detail Set - Handles set grid andd coordinates set changes to view; detail VM
    handle the storage of set data (activeDetail) and changes for BE update in context of the detail set data and quarters grouping structure
    overall sets coordination is handled in the ProposalView and set.vm instantiation from the array in ProposalViewModel
*/
var ProposalDetailSet = function (proposalView, id) {

    return {
        view: proposalView,
        grid: null,
        vm: null,
        id: id,
        storedDetail: null, //original stored detail data (BE format)
        activeDetail: null, //modified copy when altered
        hasDetail: false, //flag: has detail - set is active
        formValidator: null,
        isGridEditing: false,//flag: grid is in edit mode
        isGridDirty: false, //flag determine if grid has unsaved changes (inventory access)
        activeEditingRecord: null,
        lastScrollTop: null,
        isAdu: false, //flag - adu specific handling in grid
        flightBookNotified: true,

        setVM: function (flight) {
            this.vm = new DetailVM(this);
            return this.vm;
        },

        //call after setting VM
        //start up options 
        initSet: function () {
            //set the flight picker initially showing empty value (initial start/end are set)
            var flight = $('input[name=proposal_detail_flight_' + this.id + ']');
            flight.val('');
            this.initGrid();
        },

        initGrid: function () {
            var el = '#proposal_detail_grid_' + this.id;
            this.grid = $(el).w2grid(PlanningConfig.getProposalDetailGridCfg(id, this));
            this.initGridEvents();
        },

        initGridEvents: function () {
            //grid event set up			
            this.grid.on('click', this.onGridClick.bind(this));
            this.grid.on('dblClick', this.onGridDoubleClick.bind(this));
            this.grid.on('change', this.onGridEditChange.bind(this));
            this.grid.on('editField', this.onGridEditStart.bind(this));
        },

        //set validation (VM fields) based on id structure (after hasDetail when in DOM)
        initFormValidationRules: function () {
            //use markup setings for rules (id based names)
            //determine how to not display error messages - override defaults not working - using css intead to hide
            this.formValidator = $('#proposal_detail_form_' + this.id).validate({
                //not working 
                errorPlacement: function (error, element) {
                }
            });
            //check alphanumeric for daypart code
            $("#proposal_detail_daypart_code_" + this.id).rules("add", {
                alphanumeric: true
            });
        },

        // check original versus new date ranges (including hiatus) - if old effected 
        checkFlightChange: function (params) {
            var oldStart = this.activeDetail.FlightStartDate,
                oldEnd = this.activeDetail.FlightEndDate,
                oldFlights = this.activeDetail.FlightWeeks;

            var change = false;
            this.flightBookNotified = false;

            // check date change that would effect current weeks
            if (moment(params.StartDate).isAfter(oldStart, 'day') || moment(params.EndDate).isBefore(oldEnd, 'day')) {
                change = true;
            } else {
                // added MediaWeekId to params FlightWeeks in custom KO plug-in (if existing so can check - MediaWeekId or 0
                $.each(oldFlights, function (idx, flight) {
                    var compare = util.objectFindByKey(params.FlightWeeks, 'MediaWeekId', flight.MediaWeekId);
                    if (compare) {
                        if (compare.IsHiatus != flight.IsHiatus) change = true;
                    }
                });
            }

            return change;
        },

        //set flights - either initial or an update (if has detail)
        setFlight: function (params) {
            if (this.hasDetail) {
                var fn = function () {
                    this.activeDetail.FlightEdited = true;
                    this.view.updateProposalDetail(this);
                }.bind(this);

                if (this.checkFlightChange(params)) {
                    util.confirm('Flight Change', ' Existing data will be affected by this Flight change. Click Continue to proceed', fn);
                } else {
                    fn();
                }

            } else {
                $(this).trigger('flightSelect', [params, this]);
            }
        },

        //set detail - in context of isInitial
        setDetail: function (detail, isInitial) {
            this.storedDetail = detail;
            this.activeDetail = util.copyData(detail, null, null, true);
            this.hasDetail = true;
            this.isAdu = detail.Adu || false;
            if (isInitial) {
                this.vm.setInitialDetail();
            } else {
                this.vm.loadDetail(detail);
            }
            this.setDetailGrid(this.activeDetail);
            this.initFormValidationRules();

            // load books or default options
            this.view.controller.manageRatingsViewModel.selectedShareBook(detail.SharePostingBookId || detail.DefaultPostingBooks.DefaultShareBook.PostingBookId);

            var shareBookId = -1;
            if (detail.DefaultPostingBooks && detail.DefaultPostingBooks.UseShareBookOnlyId) {
                shareBookId = detail.DefaultPostingBooks.UseShareBookOnlyId;
            }

            // adds 'Use Share Only' once, if not added yet
            var shareOnlyOption = _.find(this.view.controller.manageRatingsViewModel.hutBookOptions(), function(option) {
                return option.Display == "Use Share Only";
            });

            if (!shareOnlyOption) {
                this.view.controller.manageRatingsViewModel.hutBookOptions.unshift({ Id: shareBookId, Display: "Use Share Only" });
            }

            this.view.controller.manageRatingsViewModel.selectedHutBook(detail.HutPostingBookId || detail.DefaultPostingBooks.DefaultHutBook.PostingBookId);
            this.view.controller.manageRatingsViewModel.selectedPlaybackType(detail.PlaybackType || detail.DefaultPostingBooks.DefaultPlaybackType);
        },

        //prepare data in context of grid grouped structure from nested BE data; add indexes, ids
        //NOTE: to sync with grid column field (editing) - Quarter ImpressionGoal is set as Impressions; Quarter Cpm as Units
        //issues with unique id - at quarter week level - there in no uniquness in the ids from BE - add an additional set id to
        prepareDetailGridData: function (data) {
            var ret = [];
            var me = this;
            $.each(data, function (idx, qtr) {
                var qtrRow = {
                    isQuarter: true,
                    w2ui: { "style": "background-color: #dedede" },
                    recid: util.generateUUID(),
                    Id: qtr.Id,
                    quarterIdx: idx,
                    QuarterText: qtr.QuarterText,
                    //Cpm: qtr.Cpm,
                    //ImpressionGoal: qtr.ImpressionGoal,
                    //use similar value to sync with field - Units is CPM; Impresion is ImpressionGoal
                    Units: qtr.Cpm,
                    Impressions: qtr.ImpressionGoal

                };
                ret.push(qtrRow);

                $.each(qtr.Weeks, function (weekIdx, week) {
                    week.recid = util.generateUUID();
                    week.quarterId = qtr.Id;
                    week.quarterIdx = idx;
                    week.weekIdx = weekIdx;
                    ret.push(week);
                });
            });
            return ret;
        },

        getTotalRow: function (setId, data) {
            var ret = {
                recid: 'summary_detail_' + setId,
                w2ui: { summary: true, style: 'font-weight: bold;' },
                displayText: 'Totals',
                TotalUnits: data.TotalUnits, //this will have no data//target.TotalSpots,
                TotalCost: data.TotalCost,
                TotalImpressions: data.TotalImpressions
            };
            return ret;
        },

        setDetailGrid: function (detail) {
            var detailData = this.prepareDetailGridData(detail.Quarters);
            this.grid.clear();
            var total = this.getTotalRow(this.id, detail);
            detailData.push(total);
            this.grid.add(detailData);
            this.grid.resize();
        },

        //any pre removal mechanisms
        onRemove: function () {
            //grid needs to be removed from the w2ui cache as will be similar id
            this.grid.destroy();
            //this.vm.clearDetail();
        },

        //check for unsaved chnages to access inventory - tbd; can use valid as the initial check?
        detailDirty: function () {
            var dirty = false;
            if (!this.hasDetail) return true;
            if (!this.isValid()) {
                dirty = true;
            }
            if (this.isGridDirty || this.vm.detailHeaderDirty()) dirty = true;
            return dirty;
        },

        //check validation: todo handle inner grid changes?
        isValid: function () {
            //test validation
            var valid = true;//assume true unless has detail and forms not valid
            if (this.hasDetail) {
                //handle separately but display all issues
                var formValid = this.formValidator.form();
                var gridValid = this.validateGrid();
                valid = formValid && gridValid;
            }
            return valid;
        },

        validateGrid: function () {
            var valid = true;

            var me = this;
            if (this.hasDetail) {
                //determine if is 0 or null within valid states isAdu, isHiatus etc; set red class if invalid
                var gridEditEls = '#proposal_detail_grid_' + this.id + ' .editable-cell';
                //reset initially
                $(gridEditEls).removeClass('has-error');
                $.each(me.grid.records, function (idx, rec) {
                    var hasCpmOrUnits = (rec.Units > 0), hasGoalOrImpressions = (rec.Impressions > 0);
                    var unitsEl = '#editable_units_' + rec.recid, ImpressionsEl = '#editable_impressions_' + rec.recid;
                    if (rec.isQuarter) {
                        if (!hasCpmOrUnits && !me.isAdu) {
                            valid = false;
                            $(unitsEl).addClass('has-error');
                        }
                        if (!hasGoalOrImpressions) {
                            valid = false;
                            $(ImpressionsEl).addClass('has-error');
                        }
                    } else { //Weeks ?
                        if (!hasCpmOrUnits && !rec.IsHiatus) {
                            valid = false;
                            $(unitsEl).addClass('has-error');
                        }
                        if (!hasGoalOrImpressions && !rec.IsHiatus) {
                            valid = false;
                            $(ImpressionsEl).addClass('has-error');
                        }
                    }
                });
                //console.log('valid grid', valid, this);
                return valid;
            }

        },
        //get the data for save 
        getSaveSet: function () {
            var ret = this.activeDetail;
            var vmChanges = this.vm.getSaveDetail();
            ret = $.extend({}, ret, vmChanges);
            return ret;
        },

        //set/unset the grid dirty (inventory access unsaved) - called from View updateDetail for now
        setGridDirty: function (dirty) {
            this.isGridDirty = dirty;
        },

        clearVMDirty: function (dirty) {
            this.vm.isDirty(dirty);
        },

        //INLINE EDITING GRID

        //determine if can edit - intercept
        canGridEdit: function (rec, column) {
            if (this.view.controller.proposalViewModel.isReadOnly())
                return false;

            var can = true;
            if (rec.isQuarter) {//Quarter: CPM or Goal (goal can always edit?)
                if (column === 1) {//CPM - check ADU
                    if (this.isAdu) can = false;
                }
            } else {//Week: Units or Impressions
                //cannot edit directly if hiatus
                if (rec.IsHiatus) can = false;
            }
            //console.log('can edit', can, this.isAdu)
            return can;
        },

        //attempt to go to position of last record - but does not work as not in grid scroll - only highlights
        //use reset of scroll position

        checkAfterEdit: function () {
            //console.log('checkAfterEdit', this.activeEditingRecord);
            if (this.activeEditingRecord) {
                //this.grid.select(this.activeEditingRecord.recid);
                //this.grid.scrollIntoView(this.grid.get(this.activeEditingRecord.recid, true));

                //better to just maintain exact position of last scroll
                if (this.lastScrollTop) {
                    $("#proposal_view_body").scrollTop(this.lastScrollTop);
                    this.lastScrollTop = null;
                }
            }
            this.activeEditingRecord = null;
        },

        //on grid click per column - handle possible states that prevent editing: IsHiatus, isAdu
        //TODO: FOCUS not being handled  correctly after reload - try to capture new field if applicable - revise isGridEditing
        onGridClick: function (event) {
            var record = this.grid.get(event.recid);
            if ((event.column === null) || !this.canGridEdit(record, event.column)) {
                return;
            }

            //quarter cpm goal or week unit
            if (event.column === 1) {
                //try to determine diff between go to click when already editing versus completion of editing?
                //if (this.isGridEditing) {
                //    console.log('grid editing on click', this.isGridEditing);
                //    this.isGridEditing = false;
                //    return;
                //}
                this.isGridEditing = false;
                //set/change the editor based on quarter/week; for money format the prefix (then non editable) and set to autoFormat false 
                //IF SET min vals on each then change event will be thrown ie. for 0 to the min - changing the value even if set to 0
                //this.grid.columns[1].editable = record.isQuarter ? { type: 'money', autoFormat: false, min: 0.01, prefix: 'CPM $' } : { type: 'int', min: 1 };
                this.grid.columns[1].editable = record.isQuarter ? { type: 'money', autoFormat: false, prefix: 'CPM $' } : { type: 'int' };
                //new - need to set the editing to no decimals if .00 or blank '' if 0 value overall
                var units = record.Units ? record.Units : '';
                this.grid.editField(event.recid, 1, units);
                this.isGridEditing = true;
            }

            //week ImpressionGoal or Impressions
            if (event.column === 2) {
                this.isGridEditing = false;
                this.grid.columns[2].editable = record.isQuarter ? { type: 'float', autoFormat: true, precision: 3, prefix: 'IMP Goal </br>' } : { type: 'float', precision: 3 };
                //editing mode
                //new - need to set the editing to no decimals if .00 or blank '' if 0 value overall - just use not to fixed and editor should handle?
                var editImpressions = record.Impressions ? util.divideImpressions(record.Impressions) : '';
                this.grid.editField(event.recid, 2, editImpressions);
                this.isGridEditing = true;
            }
        },

        //prevent default behavior for edit fields (may be disabled or wrong editor) - let the grid click dictate
        onGridDoubleClick: function (event) {
            event.preventDefault();
        },

        //for now prevent the grid from going to next editable field on tab/enter if is Editing
        onGridEditStart: function (event) {
            if (this.isGridEditing) {
                console.log('grid is editing', event);
                event.preventDefault();
                this.isGridEditing = false; // todo change after api calls?
            }
        },

        //on edit change: specific to column/type - check for valid value and not same as current
        onGridEditChange: function (event) {
            //wait until after complete
            // console.log('onGridEditChange', event);
            var me = this;
            //do another check just in case got through
            if (!this.canGridEdit) return;
            event.onComplete = function () {
                var record = me.grid.get(event.recid);
                var newVal = event.value_new;
                //check for empty string and reset to 0 (from editing hack)
                if (!$.isNumeric(newVal)) newVal = 0;
                if (event.column === 1) {//CPM or Unit
                    //me.isGridEditing = true;
                    if (record.isQuarter) {//Cpm                       
                       // console.log('onGridEditChange CPM Quarter', event, newVal);
                        //Quarter CPM ($)- Units represents CPM in record
                        newVal = newVal.toFixed(2);
                        if ((newVal > 0) && (newVal != record.Units)) {
                            me.onEditQuarterCpm(record, newVal);
                        }

                    } else {
                        //console.log('onGridEditChange Unit Week', event, newVal);
                        //Week Units
                        if ((newVal > 0) && (newVal != record.Units)) {
                            me.onEditWeekUnits(record, newVal);
                        }
                    }
                }
                if (event.column === 2) {//ImpressionGoal or Impressions
                    //me.isGridEditing = true;
                    newVal = newVal.toFixed(3);
                    newVal = util.multiplyImpressions(newVal); // returns value *1000
                    if (record.isQuarter) {
                        //console.log('onGridEditChange ImpressionGoal Quarter', event, newVal);
                        //Quarter Impression Goal (float) - Impressions represents ImpressionGoal
                        if ((newVal > 0) && (newVal != record.TotalImpressions)) {
                            me.onEditQuarterGoal(record, newVal);
                        }
                    } else {
                        //console.log('onGridEditChange Impressions Week', event, newVal);
                        //Week Impressions  
                        if ((newVal > 0) && (newVal != record.TotalImpressions)) {
                            me.onEditWeekImpressions(record, newVal);
                        }
                    }
                }
            };
        },

        //Editing apis - find in activeDetail and change value/flag - reset storedDetail prior to use

        setActiveToStored: function () {
            this.activeDetail = util.copyData(this.storedDetail, null, null, true);
        },

        //set active editing - pending until api response - set rec and lastScrollTop
        setActiveEditing: function (rec) {
            this.activeEditingRecord = rec;
            this.lastScrollTop = $("#proposal_view_body").scrollTop();
        },
        //quarter cpm
        onEditQuarterCpm: function (rec, value) {
            this.setActiveToStored();
            var qtr = this.activeDetail.Quarters[rec.quarterIdx];
            if (qtr) {
                qtr.Cpm = value;
                qtr.DistributeGoals = false;
                this.setActiveEditing(rec);
                this.view.updateProposalDetail(this);
            }
        },

        //quarter impresssion goals; change flag
        //check for change in weeks and warn; if cancel reset goal
        onEditQuarterGoal: function (rec, value) {
            this.setActiveToStored();
            var qtr = this.activeDetail.Quarters[rec.quarterIdx];
            if (qtr) {
                var fn = function () {
                    qtr.ImpressionGoal = value;
                    qtr.DistributeGoals = true; //todo change name per BE
                    this.setActiveEditing(rec);
                    this.view.updateProposalDetail(this);
                }.bind(this);
                var confirm = false;
                $.each(qtr.Weeks, function (idx, week) {
                    if (!week.IsHiatus && week.Impressions > 0) confirm = true;
                });
                if (confirm) {
                    util.confirm('', 'Warning: You will loose one or more existing weekly goals for this quarter.', fn);
                } else {
                    fn();
                }
            }
        },


        //week Units
        onEditWeekUnits: function (rec, value) {
            this.setActiveToStored();
            var qtr = this.activeDetail.Quarters[rec.quarterIdx];
            if (qtr) {
                var week = qtr.Weeks[rec.weekIdx];
                if (week) {
                    qtr.DistributeGoals = false; //todo change name per BE
                    week.Units = value;
                    this.setActiveEditing(rec);
                    this.view.updateProposalDetail(this);
                }
            }
        },

        //week impresssion
        onEditWeekImpressions: function (rec, value) {
            this.setActiveToStored();
            var qtr = this.activeDetail.Quarters[rec.quarterIdx];
            if (qtr) {
                var week = qtr.Weeks[rec.weekIdx];
                if (week) {
                    qtr.DistributeGoals = false; //todo change name per BE
                    week.Impressions = value;
                    this.setActiveEditing(rec);
                    this.view.updateProposalDetail(this);
                }
            }
        },
        //the adu should already be changed from the vm s0 just call the update
        onEditAdu: function (checked) {
            this.view.updateProposalDetail(this);
            this.isAdu = checked;
        },

    };
};