var TrafficController = BaseController.extend({
    trafficView: null,
    trafficViewModel: null,

    initController: function () {
        var $scope = this;

        $scope.trafficView = new TrafficView();
        $scope.trafficView.initView(this);
    },

    apiGetTraffic: function (callback, errorCallback) {
        var url = baseUrl + 'api/Traffic/GetTraffic';

        httpService.get(
            url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            {
                $ViewElement: $('#traffic_view'),
                ErrorMessage: 'Error Fetching Traffic Data',
                TitleErrorMessage: 'Error Fetching Traffic Data',
                StatusMessage: 'Error Fetching Traffic Data'
            });
    }
});