//tracker scrubbing view model

var TrackerScrubViewModel = function (controller) {
    //private controller
    var $scope = this;
    var controller = controller; //leave private

    /*** Main scrub grid properties ***/
    var initialPostingBookId;
    var initialIscis;
    $scope.ScheduleName = ko.observable();
    $scope.PostingBooks = ko.observableArray();
    $scope.ActivePostingBookId = ko.observable();
    $scope.HasPostingBookIdChange = ko.observable(false);
    $scope.ActiveIscis = ko.observable();
    $scope.HasIscisChange = ko.observable(false);
    $scope.PostingBooks = ko.observableArray();
    $scope.ActiveOutSpecs = ko.observableArray();
    $scope.IsGridLocked = ko.observable(false);
    $scope.EstimateId = ko.observable();

    /*** Mapping station properties ***/
    $scope.StationMappingOptions = ko.observableArray();
    $scope.ShowMappingStation = ko.observable(false);
    $scope.SelectedStation = ko.observable();

    /*** Mapping program properties ***/
    $scope.ProgramMappingOptions = ko.observableArray();
    $scope.ShowMappingProgram = ko.observable(false);
    $scope.SelectedProgram = ko.observable();

    /*** Mapping confirmation properties ***/
    $scope.ProgramMappingConfirmed = ko.observable(false);

    /*** Verify timeslot properties ***/
    $scope.SelectedNextContractedTimeslot = ko.observable();

    /*** Mapped item  ***/
    $scope.MappedItem = ko.observable();

    $scope.SingleProgramMapping = ko.computed(function () {
        var result = null;

        if ($scope.MappedItem() != undefined && $scope.MappedItem().SchedulePrograms != null) {
            result = $scope.MappedItem().SchedulePrograms.length == 1
        }

        return result;
    }, $scope);

    $scope.SingleFollowingScheduleMatches = ko.computed(function () {
        var result = null;

        if ($scope.MappedItem() != undefined && $scope.MappedItem().FollowingScheduleMatches != null) {
            result = $scope.MappedItem().FollowingScheduleMatches.length == 1
        }

        return result;
    }, $scope);

    /*** Main scrub grid methods ***/

    $scope.setActiveScrub = function (data) {
        initialPostingBookId = data.CurrentPostingBookId;
        initialIscis = data.ISCIs;
        $scope.ScheduleName(data.ScheduleName);
        $scope.PostingBooks(data.PostingBooks);
        $scope.ActivePostingBookId(data.CurrentPostingBookId);
        $scope.HasPostingBookIdChange(false);
        $scope.ActiveIscis(data.ISCIs);
        $scope.HasIscisChange(false);
        $scope.ActiveOutSpecs([]);
        //stations/programs for mapping iniialized here
        $scope.setScrubMappingOptions(data);

        $scope.EstimateId(data.EstimateId);
    };

    //add out of spec item (rec or id?) to array
    $scope.addOutSpec = function (id) {
        $scope.ActiveOutSpecs.push(id);
    };

    //get all active out of spec ids as array (used by view for refresh)
    $scope.getAllOutSpecs = function (id) {
        return $scope.ActiveOutSpecs();
    };

    // Update scrubbed data 
    $scope.updateSchedule = function (closeModal) {
        var schedule = {
            EstimateId: controller.activeSchedule.Estimate,
            PostingBookId: $scope.ActivePostingBookId(),
            ISCIs: $scope.ActiveIscis(),
            OfficiallyOutOfSpecIds: $scope.ActiveOutSpecs()
        };

        controller.apiUpdateSchedule(schedule, closeModal);
    };

    // Loads options for the mapping modal (from initial scrub data: format: {Display, Id} Id not used)
    $scope.setScrubMappingOptions = function (data) {
        var programOptions = data.SchedulePrograms || [], stationOptions = data.ScheduleNetworks || [];
        programOptions.unshift({ Display: 'Do Not Map', Id: null });
        stationOptions.unshift({ Display: 'Do Not Map', Id: null });
        $scope.ProgramMappingOptions(programOptions);
        $scope.StationMappingOptions(stationOptions);
    };

    // Updates not only the selected item but every other item with the incorrect mapped value
    $scope.SaveMappings = function (mapStation, mapProgram) {
        var filteredBvsDetails = [];

        //filter all values dedpendent on mappings
        filteredBvsDetails = controller.view.$ScrubGrid.records.filter(function (item) {
            return mapProgram ? (item.Program == $scope.MappedItem().Program) : false || mapStation ? (item.Station == $scope.MappedItem().Station) : false;
        });

        if (filteredBvsDetails.length > 0) {
            //extract the ids of the filtered bvs
            var bvsIds = filteredBvsDetails.map(function (item) {
                return item.recid;
            });

            // TODO - SelectedProgram() should have the correct value here, regardless of SingleProgramMapping()
            var scheduleProgram = null;
            if (mapProgram) {
                scheduleProgram = $scope.SingleProgramMapping() ? $scope.MappedItem().SchedulePrograms[0].ProgramName : $scope.SelectedProgram().ProgramName;
            }

            //assemble object for response per BE; only send Bvs parts if active in mapping
            var mappingsToSave = {
                BvsProgram: mapProgram ? $scope.MappedItem().Program : null,
                ScheduleProgram: mapProgram ? scheduleProgram : null,
                BvsStation: mapStation ? $scope.MappedItem().Station : null,
                ScheduleStation: $scope.SelectedStation(),
                DetailIds: bvsIds,
                EstimateId: $scope.EstimateId()
            };

            controller.apiUpdateScrubbed(mappingsToSave);
        }
    };

    // Opens the mapping modal and sets the variables that control which mapping options will be displayed
    $scope.openMappingModal = function (item, schedulePrograms) {
        // clear previous value
        $scope.SelectedStation(null);
        $scope.SelectedProgram(null);
        $scope.ProgramMappingConfirmed(false);

        // processing primary schedule list
        var primarySchedulePrograms = null;
        if (schedulePrograms) {
            primarySchedulePrograms = schedulePrograms.PrimaryScheduleMatches.map(function (item) {
                return {
                    ProgramName: item.ProgramName,
                    ScheduleDaypart: item.ScheduleDaypart,
                    ScheduleDetailWeekId: item.ScheduleDetailWeekId,
                    DisplayName: item.ProgramName + ", " + item.ScheduleDaypart
                }
            });
        };

        // processing following schedule matches (next timeslot)
        var followingScheduleMatches = null;
        if (schedulePrograms) {
            followingScheduleMatches = schedulePrograms.FollowingScheduleMatches.map(function (item) {
                return {
                    ProgramName: item.ProgramName,
                    ScheduleDaypart: item.ScheduleDaypart,
                    ScheduleDetailWeekId: item.ScheduleDetailWeekId,
                    DisplayName: item.ProgramName + ", " + item.ScheduleDaypart
                }
            });
        }

        // create object used for the bindings
        $scope.MappedItem({
            recid: item.recid,
            Program: item.Program,
            SchedulePrograms: primarySchedulePrograms,
            FollowingScheduleMatches: followingScheduleMatches,
            Station: item.Station
        });

        if (!item.MatchStation) {
            $("#scrub_mapping_station_modal").modal('show');
        } else if (item.HasLeadInScheduleMatches) {
            $("#scrub_timeslot_modal").modal('show');
        } else if (!item.MatchProgram) {
            if (schedulePrograms && schedulePrograms.PrimaryScheduleMatches) {
                $("#scrub_mapping_program_modal").modal('show');
            } else {
                util.notify('No programs available for the selected detail', 'danger');
            }
        } else {
            util.notify('Nothing to map in the selected detail', 'danger');
        }
    };

    // Determine if posting, iscis, or ther changes to enable master save; out of spec
    $scope.hasChanges = ko.computed(function () {
        var hasPosting = ($scope.ActivePostingBookId() && $scope.ActivePostingBookId() != initialPostingBookId);
        var hasIscis = ($scope.ActiveIscis() && $scope.ActiveIscis() != initialIscis);
        var hasOutSpeck = $scope.ActiveOutSpecs().length > 0;

        if (hasPosting || hasIscis) {
            controller.view.lockScrubGrid(true); // blocks W2UI grid context menu while the grid isn't saved
            $scope.IsGridLocked(true);

        } else {
            controller.view.lockScrubGrid(false); // change back if re-allowed from initial
            $scope.IsGridLocked(false);
        }
        //set form states
        $scope.HasIscisChange(hasIscis);
        $scope.HasPostingBookIdChange(hasPosting);
        if (hasPosting || hasIscis || hasOutSpeck) {
            return true;
        }
        return false;
    });

    /*** Mapping station methods ***/

    $scope.enableSaveStationMapping = ko.computed(function () {
        return $scope.SelectedStation() != undefined;
    });

    $scope.saveStationMappings = function () {
        $scope.SaveMappings(true, false)
        $("#scrub_mapping_station_modal").modal('hide');
    };

    /*** Mapping program methods ***/

    $scope.canMap = ko.computed(function () {
        var isSingleMapping = ($scope.SingleProgramMapping() !== null && $scope.SingleProgramMapping() !== undefined) ? $scope.SingleProgramMapping() : false;
        var isValidSelectedProgram = $scope.SelectedProgram() !== null && $scope.SelectedProgram() !== undefined;
        return isSingleMapping || isValidSelectedProgram
    }, $scope);

    $scope.acceptAsBlock = function () {
        if ($scope.canMap()) {
            var scheduleProgram = $scope.SingleProgramMapping() ? $scope.MappedItem().SchedulePrograms[0] : $scope.SelectedProgram();

            var acceptScheduleBlockRequest = {
                DetectionDetailId: $scope.MappedItem().recid,
                ScheduleDetailWeekId: scheduleProgram.ScheduleDetailWeekId
            };

            controller.apiAcceptAsBlock(acceptScheduleBlockRequest, function () {
                $("#scrub_mapping_program_modal").modal('hide');
            });
        }
    };

    $scope.openConfirmation = function () {
        if ($scope.canMap()) {
            $("#scrub_mapping_program_modal").modal('hide');
            $("#scrub_mapping_program_modal_confirmation").modal('show');
        }
    };

    $scope.setOutOfSpec = function () {
        controller.view.setOutSpecItem($scope.MappedItem().recid);
    };

    /*** Mapping confirmation methods ***/

    $scope.backToMappingProgram = function () {
        $("#scrub_mapping_program_modal_confirmation").modal('hide');
        $("#scrub_mapping_program_modal").modal('show');
    };

    $scope.confirmMapping = function () {
        if ($scope.ProgramMappingConfirmed()) {
            $scope.SaveMappings(false, true);
            $("#scrub_mapping_program_modal_confirmation").modal('hide');
        }
    },

    /*** Mapping timeslot methods ***/

    $scope.canVerifyLeadIn = ko.computed(function () {
        var isSingleMapping = ($scope.SingleFollowingScheduleMatches() !== null && $scope.SingleFollowingScheduleMatches() !== undefined) ? $scope.SingleFollowingScheduleMatches() : false;
        var isValidSelectedProgram = $scope.SelectedNextContractedTimeslot() !== null && $scope.SelectedNextContractedTimeslot() !== undefined;
        return isSingleMapping || isValidSelectedProgram
    }, $scope);

    $scope.openProgramMapping = function () {
        if ($scope.canMap()) {
            $("#scrub_timeslot_modal").modal('hide');
            $("#scrub_mapping_program_modal").modal('show');
        }
    }

    $scope.openVerifyLeadIn = function () {
        if ($scope.canVerifyLeadIn()) {
            $scope.SelectedNextContractedTimeslot($scope.MappedItem().FollowingScheduleMatches[0]);

            $("#scrub_timeslot_modal").modal('hide');
            $("#scrub_verify_lead_in").modal('show');
        }
    };

    /*** Verify lead in methods ***/

    $scope.verifySpot = function () {
        var acceptScheduleLeadinRequest = {
            DetectionDetailId: $scope.MappedItem().recid,
            ScheduleDetailWeekId: $scope.SelectedNextContractedTimeslot().ScheduleDetailWeekId
        };

        controller.apiAcceptScheduleLeadin(acceptScheduleLeadinRequest);

        $("#scrub_verify_lead_in").modal('hide');
    };
};