using System.Web.Optimization;

namespace BroadcastComposerWeb
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Enable this when debugging to see the bundles
            //BundleTable.EnableOptimizations = true;

            var librariesBundle = new ScriptBundle("~/bundles/libraries").Include(
                "~/Scripts/library/jquery-2.2.3.min.js",
                "~/Scripts/library/jquery-validate/jquery.validate.min.js",
                "~/Scripts/library/jquery-validate/require_from_group.js",
                "~/Scripts/library/bootstrap-3.3.5/js/bootstrap.js",
                "~/Scripts/library/knockout/knockout-3.3.0.js",
                "~/Scripts/library/knockout/knockout.mapping.js",
                "~/Scripts/library/w2ui/w2ui-1.5.rc1.js",
                "~/Scripts/library/lodash.min.js",
                "~/Scripts/library/moment.js",
                "~/Scripts/library/bootstrap-3.3.5/js/bootstrap-notify.min.js",
                "~/Scripts/library/daterangepicker/daterangepicker_modified.js",
                "~/Scripts/library/webui-popover/jquery.webui-popover.min.js",
                "~/Scripts/library/timepicker/jquery.timepicker.js",
                "~/Scripts/library/select2/select2.min.js",
                "~/Scripts/library/daypart-dropdown/daypart-dropdown-broadcast.js",
                //"~/Scripts/library/jquery.requestanimationframe.min.js",
                "~/Scripts/library/numeral.js",
                "~/Scripts/library/bootstrap-tagsinput/bootstrap-tagsinput.js");

            var appBundle = new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/app/Class.js",
                "~/Scripts/app/config.js",
                "~/Scripts/app/BaseController.js",
                "~/Scripts/app/BaseView.js",
                "~/Scripts/app/wrappers.js",
                "~/Scripts/app/util.js",
                "~/Scripts/app/httpService.js",
                "~/Scripts/app/UploadManager.js",
                "~/Scripts/app/appController.js",
                "~/Scripts/library/knockout/custom-bindings/daypart-dropdown.js",
                "~/Scripts/library/knockout/custom-bindings/flightweek-picker.js",
                "~/Scripts/library/knockout/custom-bindings/select2.js",
                "~/Scripts/library/knockout/custom-bindings/modal.js",
                "~/Scripts/library/knockout/custom-bindings/bootstrap-tagsinput.js",
                "~/Scripts/library/knockout/custom-bindings/stopBubble.js");

            var trackerBundle = new ScriptBundle("~/bundles/tracker").Include(
                "~/Scripts/appTracker/TrackerConfig.js",
                "~/Scripts/appTracker/TrackerMainController.js",
                "~/Scripts/appTracker/TrackerScheduleViewModel.js",
                "~/Scripts/appTracker/TrackerMainView.js",
                "~/Scripts/appTracker/TrackerManageMappings.js",
                "~/Scripts/appTracker/TrackerManageRatingsBooks.js",
                "~/Scripts/appTracker/TrackerUploadManager.js",
                "~/Scripts/appTracker/BvsFileListingView.js");

            var rateBundle = new ScriptBundle("~/bundles/rate").Include(
                "~/Scripts/library/daypart-dropdown/daypart-dropdown-broadcast.js",
                "~/Scripts/appRate/RateConfig.js",
                "~/Scripts/appRate/RateMainController.js",
                "~/Scripts/appRate/RateMainView.js",
                "~/Scripts/appRate/RateUploadManager.js",
                "~/Scripts/appRate/RateStationController.js",
                "~/Scripts/appRate/RateStationView.js",
                "~/Scripts/appRate/RateStationViewModel.js",
                "~/Scripts/appRate/StationModalEditRate.js",
                "~/Scripts/appRate/StationModalEditRateThirdparty.js",
                "~/Scripts/appRate/StationModalEndFlight.js",
                "~/Scripts/appRate/StationModalNewRate.js",
                "~/Scripts/appRate/ImportThirdPartyViewModel.js");

            var planningBundle = new ScriptBundle("~/bundles/planning").Include(
                "~/Scripts/appPlanning/PlanningConfig.js",
                "~/Scripts/appPlanning/PlanningMainController.js",
                "~/Scripts/appPlanning/PlanningMainView.js",
                "~/Scripts/appPlanning/PlanningSearchViewModel.js",
                "~/Scripts/appPlanning/ProposalController.js",
                "~/Scripts/appPlanning/ProposalView.js",
                "~/Scripts/appPlanning/ProposalViewModel.js",
                "~/Scripts/appPlanning/ProposalDetailSet.js",
                "~/Scripts/appPlanning/CustomMarketsViewModel.js",
                "~/Scripts/appPlanning/CustomMarketsSelectorViewModel.js",
                "~/Scripts/appPlanning/ProposalDetailInventoryView.js",
                "~/Scripts/appPlanning/ProposalDetailInventoryViewModel.js",
                "~/Scripts/appPlanning/ProposalDetailOpenMarketView.js",
                "~/Scripts/appPlanning/ProposalDetailOpenMarketViewModel.js",

                "~/Scripts/appPlanning/CriteriaBuilderViewModel.js",
                "~/Scripts/appPlanning/FilterViewModel.js",
                "~/Scripts/appPlanning/SwitchProposalVersionView.js",
                "~/Scripts/appPlanning/SwitchProposalVersionViewModel.js",
                "~/Scripts/appPlanning/VersionCreatedOptionsViewModel.js",
                "~/Scripts/appPlanning/ProposalUpdateWarningViewModel.js",
                "~/Scripts/appPlanning/ManageRatingsViewModel.js");

            var trackerScrubBundle = new ScriptBundle("~/bundles/trackerScrub").Include(
                "~/Scripts/appTracker/TrackerScrubController.js",
                "~/Scripts/appTracker/TrackerScrubViewModel.js",
                "~/Scripts/appTracker/TrackerScrubView.js");

            var postingBundle = new ScriptBundle("~/bundles/posting").Include(
                "~/Scripts/appPosting/PostingConfig.js",
                "~/Scripts/appPosting/PostingMainController.js",
                "~/Scripts/appPosting/PostingMainView.js",
                "~/Scripts/appPosting/PostingUploadController.js",
                "~/Scripts/appPosting/PostingUploadManager.js",
                "~/Scripts/appPosting/PostingUploadView.js",
                "~/Scripts/appPosting/PostingUploadViewModel.js");

            var trafficBundle = new ScriptBundle("~/bundles/traffic").Include(
                "~/Scripts/appTraffic/TrafficConfig.js",
                "~/Scripts/appTraffic/TrafficController.js",
                "~/Scripts/appTraffic/TrafficView.js");

            var cssBundle = new StyleBundle("~/content/css").Include(
                "~/Scripts/library/w2ui/w2ui-1.5.rc1.css",
                "~/Scripts/library/bootstrap-3.3.5/css/bootstrap.css",
                "~/Scripts/library/bootstrap-3.3.5/css/bootstrap-theme.css",
                "~/Scripts/library/font-awesome/css/font-awesome.css",
                "~/Scripts/library/daterangepicker/daterangepicker.css",
                "~/Scripts/library/webui-popover/jquery.webui-popover.min.css",
                "~/Scripts/library/timepicker/jquery.timepicker.css",
                "~/Scripts/library/select2/select2.min.css",
                "~/Scripts/library/select2/select2-bootstrap.min.css",
                "~/Scripts/library/daypart-dropdown/daypart-dropdown.css",
                "~/Scripts/library/bootstrap-tagsinput/bootstrap-tagsinput.css",
                "~/Styles/css/libraries/animate.css",
                "~/Styles/css/fonts/stack.css",
                "~/Styles/css/app.css");

            bundles.Add(librariesBundle);
            bundles.Add(appBundle);
            bundles.Add(trackerBundle);
            bundles.Add(rateBundle);
            bundles.Add(planningBundle);
            bundles.Add(trackerScrubBundle);
            bundles.Add(postingBundle);
            bundles.Add(trafficBundle);
            bundles.Add(cssBundle);
        }
    }
}
