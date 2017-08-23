//Manage Mappings (TrackerMainView context) - Modal with Program/Station tab and grids for mappings; allows search and delete of a mapping

var TrackerManageMappings = function (view) {
    //private
    var _view = view;//TrackerMainView

    return {

        $Modal: null,
        $ProgramGrid: null,
        $StationGrid: null,
        activeMappingsProgram: null,
        activeMappingsStation: null,

        initView: function () {
            //grids
            this.$ProgramGrid = $('#mappings_program_grid').w2grid(TrackerConfig.getMappingsGridCfg(this, 'Program'));
            this.$StationGrid = $('#mappings_station_grid').w2grid(TrackerConfig.getMappingsGridCfg(this, 'Station'));
            this.setGridEventHandlers();
            //tab listeners
            var me = this;
            $('#manage_mappings_tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                //e.target // newly activated tab
                //e.relatedTarget // previous active tab
                //console.log('tab', e);
                if (e.target.name == 'program') {
                    //me.$ProgramGrid.resize();
                    //in IE grid sometimes shows horizontal scroll when 2 columns (full refresh seenms to fix)
                    me.$ProgramGrid.refresh();
                }

                if (e.target.name == 'station') {
                    //me.$StationGrid.resize();
                    //in IE grid sometimes shows horizontal scroll when 2 columns (full refresh seenms to fix)
                    me.$StationGrid.refresh();
                }
            });
        },

        // load mappings; open modal
        setMappings: function (program, station) {
            this.activeMappingsProgram = program;
            this.activeMappingsStation = station;
            var me = this;
            if (!this.$Modal) {
                this.$Modal = $('#manage_mappings_modal');
                this.$Modal.on('shown.bs.modal', function (event) {
                    me.onSetMappings();
                });

                this.$Modal.modal({
                    backdrop: 'static',
                    show: false,
                    keyboard: false
                });
            }

            this.$Modal.modal('show');
        },

        setGridEventHandlers: function () {
            var me = this;
            //grid default search forces to "begins" - need to use "contains" - intercept here
            this.$ProgramGrid.on('search', function (event) {
                //console.log(event);
                $.each(event.searchData, function (index, search) {
                    search.operator = 'contains';
                });
            });

            this.$StationGrid.on('search', function (event) {
                //console.log(event);
                $.each(event.searchData, function (index, search) {
                    search.operator = 'contains';
                });
            });

            //menu, delete

            this.$ProgramGrid.on('menuClick', function (event) {
                //console.log(event);
                if (event.menuItem.id === 1) { //delete
                    me.handleMappingsDelete('Program', me.$ProgramGrid.get(event.recid));
                }
            });

            this.$StationGrid.on('menuClick', function (event) {
                //console.log(event);
                if (event.menuItem.id === 1) { //delete
                    me.handleMappingsDelete('Station', me.$StationGrid.get(event.recid));
                }
            });

        },

        //after modal shown - set grids; move to first tab
        onSetMappings: function () {
            this.setProgramGrid();
            this.setStationGrid();
            $('#manage_mappings_tabs a[href="#mappings_program_view"]').tab('show')
        },

        //use index based ID
        prepareGridData: function (data) {
            var displayData = util.copyData(data);
            var ret = [];
            $.each(displayData, function (index, item) {
                item.recid = index + 1;
                ret.push(item);
            });

            return ret;
        },

        // set Program Grid
        setProgramGrid: function () {
            var programData = this.prepareGridData(this.activeMappingsProgram);
            this.$ProgramGrid.searchReset();//reset searches
            this.$ProgramGrid.clear(false);
            this.$ProgramGrid.add(programData);
            //this.$ProgramGrid.resize();
        },

        //set Station Grid
        setStationGrid: function () {
            var stationData = this.prepareGridData(this.activeMappingsStation);
            this.$StationGrid.searchReset();//reset searches
            this.$StationGrid.clear(false);
            this.$StationGrid.add(stationData);
            //this.$StationGrid.resize();
        },

        //delete a mapping by type
        handleMappingsDelete: function (type, rec) {
            //confirm the delete?
            var title = 'Delete ' + type + ' Mapping',
                message = 'Are you sure you want to delete ' + rec.ScheduleValue + '/' + rec.BvsValue + '?';
            var callback = this.onMappingsDelete.bind(this, rec.recid, type);
            util.confirm(title, message, _view.controller.apiDeleteMapping.bind(_view.controller, type, rec, callback));
            //_view.controller
        },

        //successful delete callback - remove rec from grid by type; notify
        onMappingsDelete: function (id, type) {
            //console.log('onMappingsDelete', id, type);
            if (type == 'Program') {
                this.$ProgramGrid.remove(id);
            } else if (type == 'Station') {
                this.$StationGrid.remove(id);
            }

            util.notify(type + ' Mapping Deleted');
        }

    }

}