var TrafficView = BaseView.extend({
    trafficController: null,

    initView: function (controller) {
        var $scope = this;

        $scope.trafficController = controller;
        $scope.postingGrid = $("#traffic_grid").w2grid(TrafficConfig.getTrafficGridConfig(this));

        $scope.loadGrid();
    },

    loadGrid: function () {
        // TODO - load grid   
    }
});