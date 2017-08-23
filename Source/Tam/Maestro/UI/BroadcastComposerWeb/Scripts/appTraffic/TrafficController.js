var TrafficController = BaseController.extend({
    trafficView: null,
    trafficViewModel: null,

    initController: function () {
        var $scope = this;

        $scope.trafficView = new TrafficView();
        $scope.trafficView.initView(this);
    }
});