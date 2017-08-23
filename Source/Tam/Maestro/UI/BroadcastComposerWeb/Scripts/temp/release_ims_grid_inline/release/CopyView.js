
//Copy  view - revise to NEW UX
var CopyView = BaseView.extend({
        //viewModel: controller.viewModel,
        isActive: false,
        //controller: controller,
        activeCopyData: null,
        $CopyGrid: null,
        actionsSet: false,
        isMarried: false,
        //hasActiveNetworksAllSelected: false,

        initView: function (controller) {
            this.controller = controller;
            var me = this;
            
            this.$CopyGrid = $('#copy_grid').w2grid(this.getCopiesGridConfig());
            this.isActive = true;

            window.onbeforeunload = function () {
                return me.hasActiveEdit ? 'Are you sure you want to close? Your changes have not been saved.' : undefined;
            }
        },

        getUseCaseId: function () {
            return 1; // Copy management
        },

        //no need probably - for w2ui lists
        prepareDataForDDL: function (inArray) {
            var resultData = [];
            $.each(inArray, function (idx, val) {
                resultData.push({ id: val.Id, text: val.Display });
            });
            return resultData;
        },

        setTrafficDisplay: function (data) {
            var traffic = '<strong>' + data.TrafficName + '</strong> (' + data.TrafficId + ')';
            $("#traffic_display").html(traffic);
        },
    });
