var BvsFileListingView = function (view) {

    var _view = view;


    return {
        modal: null,
        bvsFilesGrid: null,
        controller: null,
        bvsFileData: null,

        initView: function () {
            this.bvsFilesGrid = $("#bvs_files_grid").w2grid(TrackerConfig.getBvsFileListingGridCfg(this));
            this.controller = new TrackerMainController();
            this.setGridEventHandlers();
            this.initBvsFiles();
        },

        setBvsFiles: function (data) {
            this.bvsFileData = data;
            this.modal.modal('show');
        },

        initBvsFiles: function () {
            var self = this;

            self.modal = $('#bvs_file_listing_modal');
            self.modal.on('shown.bs.modal', function (event) {
                self.onSetBvsFiles(self.bvsFileData);
            });

            self.modal.modal({
                backdrop: 'static',
                show: false,
                keyboard: false
            });
        },

        onSetBvsFiles: function (listing) {
            this.setBvsFilesGrid(listing);
        },

        setBvsFilesGrid: function (listing) {
            if (listing) {
                var listData = this.prepareGridData(listing);

                this.bvsFilesGrid.clear(false);
                this.bvsFilesGrid.add(listData);
                this.bvsFilesGrid.localSort();
                this.bvsFilesGrid.resize();
            }
        },

        prepareGridData: function (data) {
            var gridItem = [];
            $.each(data, function (index, item) {
                //set grid with recid
                item.recid = item.Id;
                gridItem.push(item);
            });

            return gridItem;
        },

        setGridEventHandlers: function () {
            var self = this;
            //search button
            $("#bvs_file_listing_modal_view").on("click", "#bvsfiles_grid_search_btn", self.bvsfilesGridTextSearch.bind(self));

            //keystroke search
            $("#bvs_file_listing_modal_view").on("keypress", "#bvsfiles_grid_search_input", function (e) {
                var key = e.which;
                if (key == 13) {
                    self.bvsfilesGridTextSearch();
                }
            });

            //clear search
            $("#bvs_file_listing_modal_view").on("click", "#bvsfiles_grid_search_clear_btn", self.clearBvsfilesGridSearch.bind(self, false));

            self.bvsFilesGrid.on("menuClick", function (event) {
                if (event.menuItem.id === 1) {//1 should be delete in the menu
                    self.bvsFileDelete(self.bvsFilesGrid.get(event.recid));
                }
            });
        },

        bvsfilesGridTextSearch: function (event) {
            var self = this;
            var val = $("#bvsfiles_grid_search_input").val();
            if (val && val.length) {
                val = val.toLowerCase();
                var search = [
                    { field: 'FileName', type: 'text', value: [val], operator: 'contains' }];
                self.bvsFilesGrid.search(search, 'OR');
                $("#bvsfiles_grid_search_clear_btn").show();
            }
            else {
                self.clearBvsfilesGridSearch();
            }
        },

        clearBvsfilesGridSearch: function () {
            var self = this;
            self.bvsFilesGrid.searchReset();

            $("#bvsfiles_grid_search_input").val('');
            $("#bvsfiles_grid_search_clear_btn").hide();
        },

        bvsFileDelete: function (rec) {
            var title = "Delete BVS File",
                message = "Are you sure you want to delete " + rec.FileName + "?";
            var callback = this.onBvsFileDelete.bind(this, rec.recid);
            util.confirm(title, message, view.controller.apiDeleteBvsFile.bind(view.controller, rec, callback));
        },

        onBvsFileDelete: function (id) {
            this.bvsFilesGrid.remove(id);
            util.notify("BVS File Deleted.");
        }
    }
}