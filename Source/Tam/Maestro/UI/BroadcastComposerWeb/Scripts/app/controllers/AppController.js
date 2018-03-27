
//change to synchronized on load - before initializing page controller
var AppController = BaseController.extend({

    pageController: null,
    pageControllerActivated: false,
    user: null,
    environment: null,
    initControllerPage: function (pageController) {
        var $scope = this;
        $scope.pageController = pageController;
        $scope.apiGetEmployeeInfo();
        $scope.apiGetEnvironment();

       //wait until processed
        //$scope.pageController.initController();

    },

    //when both are ready - proceeed
    checkUserEnvironmentReady: function () {
        if (this.user && this.environment) {
            if (!this.pageControllerActivated) this.pageController.initController();
            this.pageControllerActivated = true;
        }
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
        this.checkUserEnvironmentReady();
    },

    displayApiUser: function (user) {
        if (!user) {
            $("#user_info").html('');
        } else {
            var name = user.Username;
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
        this.environment = data;
        this.displayeEvironment(data);
        this.checkUserEnvironmentReady();
    }

});