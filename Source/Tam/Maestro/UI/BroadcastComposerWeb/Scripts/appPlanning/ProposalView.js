

//Proposal View - with detailSets handling
//detail sets are index based and not removed overall - running sets some with detail, one always not and removals are emptied but part of the overal set array
//consider refactor to separate overall from the active sets
var ProposalView = BaseView.extend({
    controller: null,
    formValidator: null,

    activeProposalData: null,
    activeSelectedId: null, //for position state changes

    activeEditingRecord: null,
    readOnly: false, //tbd future status

    $ProposalModal: null,
    activeProposalId: null,

    detailSets: [],
    $InventoryView: null,
    $OpenMarketView: null,
    openMarketInventory: null,

    initView: function (controller) {
        var $scope = this;

        this.controller = controller;

        this.initValidationRules();

        this.initProposalCollapse();
        this.initModal();
    },

    //MODAL//
    initModal: function () {
        this.$ProposalModal = $('#proposal_modal');
        //prevent click outside closing (todo: possibly add properties to KO custom modal)
        this.$ProposalModal.modal({ backdrop: 'static', show: false, keyboard: false });
        var $scope = this;
        //hide event
        //intercept - before hide to check unsaved on view - not using may add back later;
        //this.$ProposalModal.on('hide.bs.modal', function (e) {
        //    if ($scope.checkUnsaved()) {
        //        e.preventDefault();
        //        e.stopImmediatePropagation();
        //        return false;
        //    }

        //});

        //hidden event
        this.$ProposalModal.on('hidden.bs.modal', function (e) {
            //tbd - unlock
            if ($scope.activeProposalId) {
                $scope.controller.apiGetUnlock($scope.activeProposalId, function (unlockResponse) {
                    if (!unlockResponse.Success) {
                        util.notify("Error unlocking the proposal", "danger");
                    }
                });
            }
            //reset collapse state in header
            $scope.resetCollapse();
            //clear VM
            $scope.controller.proposalViewModel.onHideModal();
            //do separately 
            $scope.clearDetailSets();
            $scope.activeProposalData = null;
            //refresh main
            $scope.controller.planningController.view.loadGrid();
        });

        //shown event
        this.$ProposalModal.on('shown.bs.modal', function (e) {
            $scope.clearValidation();

            $scope.controller.proposalViewModel.onShowModal();
            //if set then load - always set one empty
            if ($scope.activeProposalData) {
                $scope.loadDetailSets($scope.activeProposalData.Details);
            }
            $scope.createDetailSet();
        });

    },

    showModal: function (isHide) {
        if (isHide) {
            this.$ProposalModal.modal('hide');
        } else {

            this.$ProposalModal.modal('show');
        }
    },

    /*** COLLAPSE HEADER ***/

    //collapse events
    initProposalCollapse: function () {
        //need event combination timed to prevent scroll bars from flickering
        $('#proposal_header').on('hidden.bs.collapse', this.onCollapseHeader.bind(this, false));
        $('#proposal_header').on('show.bs.collapse', this.onCollapseHeader.bind(this, true));
    },

    //change icon; REVISE - might resize lower area
    onCollapseHeader: function (show) {
        //var height = show ? 500 : 688;
        //$("#proposal_programs_grid").height(height);
        //this.programsGrid.resize();
        var icon = show ? 'glyphicon glyphicon-triangle-bottom' : 'glyphicon glyphicon-triangle-top';
        $("#proposal_header_collapse").removeClass().addClass(icon);
    },

    //on modal close - reset (see VM)
    resetCollapse: function () {
        $('#proposal_header').collapse('show');
    },

    //PROPOSAL DETAIL SET/LOAD Proposal

    //set proposal including detail after save/load; for now called from proposalViewModel on load/create (change)
    setProposal: function (proposal) {
        this.activeProposalId = proposal.Id;
        this.activeProposalData = proposal;

    },

    //any after save mehanisms (called from VM save/save version)
    //need to sync the details sent with the proposal details returned to add/change the detail.Id that does not exist before save and changes on every save
    onAfterProposalSave: function (proposal) {
        var newDetails = proposal.Details;
        // clear any details dirty, add/change the real BE ID
        var active = this.getActiveSets();
        //console.log('on after proposal save', active, newDetails);
        $.each(active, function (idx, set) {
            var me = this;
            //assuming here that the proposal detail will exactly match the hasDetail sets (active); may need refactor
            set.setDetail(newDetails[idx], false);//reset all detail
            set.setGridDirty(false);
            set.clearVMDirty(false);
        });
    },

    //gets the active details from a set - 
    getProposalDetailForSave: function () {
        var ret = [];
        $.each(this.detailSets, function (idx, set) {
            var me = this;
            if (set && set.hasDetail) {
                //get the data for BE
                ret.push(set.getSaveSet());
            }
        });
        return ret;
    },

    //set custom events (set object) triggered from the detail set
    setCustomDetailSetEvents: function (set) {
        //set custom event//might need to remove with off on reset/remove
        $(set).on('flightSelect', this.controller.onProposalDetailFlightSelect.bind(this.controller));
        //todo future stories
        // $(set).on('aduChecked', this.controller.onAduChange.bind(this.controller));//not needed
        $(set).on('removeSet', this.onRemoveSet.bind(this));
        $(set).on('openInventory', this.onOpenDetailInventory.bind(this));
        $(set).on('openMarket', this.onOpenDetailOpenMarket.bind(this));
        $(set).on('openManageRatings', this.onOpenManageRatings.bind(this));
    },

    onOpenManageRatings: function (event, detailSetId, sharePostingBookId, hutPostingBookId, playbackType) {
        this.controller.manageRatingsViewModel.open(detailSetId, sharePostingBookId, hutPostingBookId, playbackType);
    },

    createDetailSet: function () {
        var id = this.detailSets.length + 1;//index based id
        var set = new ProposalDetailSet(this, id);
        var detailVM = set.setVM();
        this.controller.proposalViewModel.addProposalDetail(detailVM);
        set.initSet();
        this.setCustomDetailSetEvents(set);
        this.detailSets.push(set);
        return set;
    },

    //load from existing
    loadDetailSets: function (details) {
        var me = this;
        $.each(details, function (idx, item) {
            var set = me.createDetailSet();
            set.setDetail(item, false);
            //set is newly loaded so not dirty
            set.setGridDirty(false);
            set.clearVMDirty(false);
        });
        //create new item for end
        //this.createDetailSet();
    },

    //index based so need to make sure sets are not removed - deactivate them
    getDetailSet: function (id) {
        return this.detailSets[id - 1] || null;
    },

    //gets all the sets that are active - has detail
    getActiveSets: function (asId) {
        var active = [];
        $.each(this.detailSets, function (idx, set) {
            if (set && set.hasDetail) {
                var item = asId ? set.id : set;
                active.push(item);
            }
        });
        return active;
    },

    //get the index of active sets only based on set id(idx + 1)
    getIndexActiveSets: function (setId) {
        var active = this.getActiveSets(true);
        return active.indexOf(setId);
    },

    //from initial flight selection on a set - 
    setDetailSetInitial: function (id, detail) {
        var set = this.getDetailSet(id);
        if (set) {
            if (!set.hasDetail) this.createDetailSet();

            this.checkDefaultBookWarning(detail);

            var defaultShareBook = detail.DefaultPostingBooks.DefaultShareBook.PostingBookId;
            detail.SharePostingBookId = defaultShareBook;
            set.vm.SharePostingBookId(defaultShareBook);

            var defaultHutPostingBookId = detail.DefaultPostingBooks.DefaultHutBook.PostingBookId;
            detail.HutPostingBookId = defaultHutPostingBookId;
            set.vm.HutPostingBookId(defaultHutPostingBookId);

            var defaultPlaybackType = detail.DefaultPostingBooks.DefaultPlaybackType;
            detail.PlaybackType = defaultPlaybackType;
            set.vm.PlaybackType(defaultPlaybackType);

            set.setDetail(detail, true);
        }
    },

    checkDefaultBookWarning: function(detail) {
        if (detail.DefaultPostingBooks.DefaultShareBook.HasWarning || detail.DefaultPostingBooks.DefaultHutBook.HasWarning) {
            var warning = '<ul>';

            if (detail.DefaultPostingBooks.DefaultShareBook.HasWarning) {
                warning += '<li>' + detail.DefaultPostingBooks.DefaultShareBook.WarningMessage + '</li>';
            }

            if (detail.DefaultPostingBooks.DefaultHutBook.HasWarning) {
                warning += '<li>' + detail.DefaultPostingBooks.DefaultHutBook.WarningMessage + '</li>';
            }

            warning += '</ul>';

            util.confirm('Warning', warning, function () { });
        }
    },

    //update detail from set quarter/week change - changes are marked in the set.activeDetail
    //the api is passed all details from here (not just set)
    //proposal header/ specific set needs to update on response (proposal)
    //if not a set then just update static fields
    updateProposalDetail: function (set) {
        var $scope = this;
        var details = this.getProposalDetailForSave();

        $scope.controller.apiProposalDetailUpdate(details, function (proposal) {
            $scope.controller.proposalViewModel.loadStaticFields(proposal);

            if (set) {
                var idx = $scope.getIndexActiveSets(set.id);
                var detail = proposal.Details[idx];

                set.setDetail(detail, false);

                if (!set.flightBookNotified) {
                    $scope.checkDefaultBookWarning(detail);
                    set.flightBookNotified = true;
                }

                //set dirty - assumption is that if a set this will be called from a set update (grid specific edit or bulk change)
                set.setGridDirty(true);
                set.checkAfterEdit();
            }
        });
    },

    //from event trigger - remove the set - call the BE to update header
    onRemoveSet: function (event, setId, set) {
        var $scope = this;
        
        var removeFn = function () {
            if ($scope.removeSet(setId)) {
                $scope.updateProposalDetail();
            }
        };

        util.confirm('Delete Proposal Detail', 'Are you sure you wish to Delete the proposal detail?', removeFn);
    },

    //remove a set but keep placheolder in array
    removeSet: function (id) {
        //remove event - off etc
        var set = this.getDetailSet(id);
        if (set && set.hasDetail) {
            set.onRemove();
            this.controller.proposalViewModel.removeProposalDetail(set.vm);
            //this.detailSets.splice(id - 1, 1);
            //set = null; //just make null so keep in array (index based id)?
            this.detailSets[id - 1] = null;
            // console.log('remove', this.detailSets);
            return true;

        }
        return false;
    },

    //tbd clear all sets - remmove any dependencies
    clearDetailSets: function () {
        //handle individually
        this.controller.proposalViewModel.removeAllProposalDetails();
        var me = this;
        $.each(this.detailSets, function (idx, set) {
            if (set && set.id) set.onRemove();//destroy, etc?
        });
        this.detailSets = [];
    },

    //DETAIL Inventory per a set - open/set modal
    //custom trigger from set
    onOpenDetailInventory: function (event, set) {
        var $scope = this;
        var readOnly = this.controller.proposalViewModel.status() === 1 || this.controller.proposalViewModel.status() === 4;

        if ($scope.checkInventorySaved(set)) {
            if (!$scope.$InventoryView) {
                $scope.$InventoryView = new ProposalDetailInventoryView();
                $scope.$InventoryView.initView($scope);
            }

            var detailId = set.activeDetail.Id;
            var inventoryApiFn = function () {
                $scope.controller.apiGetProposalInventory(detailId, function (inventory) {
                    $scope.$InventoryView.setInventory(set, inventory, readOnly);
                });
            }.bind($scope);
            if (readOnly) {
                var status = this.controller.proposalViewModel.getStatusDisplay();
                util.confirm('Inventory Read Only', 'Proposal Status of "' + status + '", you will not be able to save inventory.  Press "Continue" to go to Inventory.', inventoryApiFn);
            } else {
                inventoryApiFn();
            }
        }
    },

    //DETAIL Open Market Inventory per a set - open/set modal
    //custom trigger from set
    onOpenDetailOpenMarket: function (event, set) {
        var $scope = this;
        var readOnly = this.controller.proposalViewModel.status() === 1 || this.controller.proposalViewModel.status() === 4;;
        if ($scope.checkInventorySaved(set)) {
            if (!$scope.$OpenMarketView) {
                $scope.$OpenMarketView = new ProposalDetailOpenMarketView();
                $scope.$OpenMarketView.initView($scope);
            }

            var detailId = set.activeDetail.Id;
            var inventoryApiFn = function () {
                $scope.controller.apiGetProposalOpenMarketInventory(detailId, function (inventory) {
                    $scope.openMarketInventory = inventory;
                    //$scope.$OpenMarketView.setInventory(set, inventory, readOnly);
                    $scope.$OpenMarketView.loadInventory(set, inventory, readOnly);

                });
            }.bind($scope);

            if (readOnly) {
                var status = this.controller.proposalViewModel.getStatusDisplay();
                util.confirm('Open Market Inventory Read Only', 'Proposal Status of "' + status + '", you will not be able to save inventory.  Press "Continue" to go to Inventory.', inventoryApiFn);
            } else {
                inventoryApiFn();
            }
        }
    },

    //status or unsaved check
    checkInventorySaved: function (set) {
        var check = true;
        //check saved/invalid
        var valid = this.formValidator.form();
        var dirty = this.controller.proposalViewModel.proposalHeaderDirty();
        var detailDirty = set.detailDirty();
        if (!this.controller.proposalViewModel.isReadOnly() && (!valid || dirty || detailDirty)) {
            check = false;
            util.alert('Proposal Not Saved', 'To access Inventory Planner you must save proposal first');
        }
        //check status
        return check;

    },


    // needed? on hide?
    checkUnsaved: function () {
        return false;
        //console.log('check unsaved');
        //var $scope = this;
        //if (this.pendingUnsavedSpots) {
        //    var continueFn = function () {
        //        $scope.pendingUnsavedSpots = false;
        //        $scope.controller.proposalViewModel.showModal(false);
        //    };
        //    util.confirm('Unsaved Spot Edits', 'There are edited program spots that are not saved. Continuing will lose these changes.', continueFn);
        //    return true;
        //}
        //return false;
    },


    /*** MASKING AND VALIDATION ***/
    //REVISE BELOW AS NEEDED NEW HEADER - not needed
    //initInputMasks: function () {
    //    $('input[name="proposal_units"]').w2field('int', { autoFormat: false, min: 1 });
    //    $('input[name="proposal_budget"]').w2field('money');
    //    $('input[name="proposal_impressions"]').w2field('float', { precision: 3 });
    //},

    initValidationRules: function () {

        this.formValidator = $('#proposal_form').validate({
            rules: {
                proposal_name: {
                    required: true,
                    maxlength: 100
                },
                proposal_advertiser: {
                    required: true
                }
            }
        });

    },

    clearValidation: function () {
        this.formValidator.resetForm();
    },

    //validate across headers and indivdual detail sets (see set)
    isValid: function () {
        this.clearValidation();
        var valid = this.formValidator.form();
        //TODO: Uncomment when storey ready
        if (valid) {
            $.each(this.detailSets, function (idx, set) {
                if (set && set.id) {
                    //valid = set.isValid();
                    //this will stop further checking - per discussion show all?
                    //if (!valid) return false;
                    //process all
                    var setValid = set.isValid();
                    if (!setValid) valid = false;

                }
            });
        }
        //console.log('valid', valid);
        if (!valid) util.notify("Proposal cannot be saved: Required Inputs Incomplete (in red)", "warning");
        return valid;
    },

    // informed keyword should match the start of the option
    select2MatchStart: function (params, data) {
        if (!params.term) {
            return data;
        }

        if (data.text.toUpperCase().indexOf(params.term.toUpperCase()) == 0) {
            return data;
        }

        return false;
    }
});