var AppController = BaseController.extend({

    pageController: null,
    user: null,
    initControllerPage: function (pageController) {
        var $scope = this;

        $scope.apiGetEmployeeInfo();
        $scope.apiGetEnvironment();

        $scope.pageController = pageController;
        $scope.pageController.initController();

    },

    apiGetEmployeeInfo: function () {
        //this.View.resetInitialAppStates();
        var me = this;
        var url = baseUrl + 'api/employee';
        httpService.get(url,
            me.onEmployeeApi.bind(this),
            null,
            {
                $ViewElement: null,
                ErrorMessage: 'Getting Employee Info',
                TitleErrorMessage: 'No employee info returned',
                StatusMessage: 'Employee Info'
        });
    },

    //after authenticate
    onEmployeeApi: function (user) {
        this.user = user;
        this.displayApiUser(user);
    },

    displayApiUser: function (user) {
        if (!user) {
            $("#user_info").html('');
        } else {
            var name = user._Username;
            $("#user_info").html(name);
        }
    },

    displayeEvironment: function (environment) {
        if (!environment) {
            $("#app_environment_name").hide();
        } else {
            $("#app_environment_name").html(environment);
        }
    },

 
    //authentication to get user
    apiGetEnvironment: function () {
        var state;
        var me = this;
        var url = baseUrl + 'api/environment';
        httpService.get(url,
            me.onGetEnvironmentCompleteApi.bind(this),
            null,
            {
                $ViewElement: null,
                ErrorMessage: 'Getting Environment Info',
                TitleErrorMessage: 'No environment info returned',
                StatusMessage: 'Environment Info'
            });
    },

    //after authenticate
    onGetEnvironmentCompleteApi: function (data) {
        this.displayeEvironment(data);
    }

});