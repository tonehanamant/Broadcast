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
                /* Foundation Libraries & Utility Libraries */
                "~/Scripts/library/jquery-2.2.3.min.js",                                    // JQUERY, Javascript Library
                "~/Scripts/library/jquery-validate/jquery.validate.min.js",
                "~/Scripts/library/jquery-validate/require_from_group.js",

                "~/Scripts/library/bootstrap-3.3.5/js/bootstrap.js",                        // BOOTSTRAP, JS,HTML,CSS Component Library
                "~/Scripts/library/bootstrap-3.3.5/js/bootstrap-notify.min.js",
                "~/Scripts/library/bootstrap-tagsinput/bootstrap-tagsinput.js",

                "~/Scripts/library/knockout/knockout-3.3.0.js",                             // KNOCKOUT,  MVVM Library
                "~/Scripts/library/knockout/knockout.mapping.js",

                /* Utilitiy Libraries */
                "~/Scripts/library/w2ui/w2ui-1.5.rc1.js",                                   // W2UI, UI Library (Grid)
                "~/Scripts/library/lodash.min.js",                                          // LODASH, Utility Library
                "~/Scripts/library/moment.js",                                              // MOMENTJS, Date and Time Library
                "~/Scripts/library/numeral.js",                                             // NUMERALJS, 

                /* Component Extensions */
                "~/Scripts/library/daterangepicker/daterangepicker_modified.js",            // DATERANGEPICKER, Bootstrap Component Extension
                "~/Scripts/library/webui-popover/jquery.webui-popover.min.js",              // WEBUI-POPOVER, JQuery Component Extension
                "~/Scripts/library/timepicker/jquery.timepicker.js",                        // TIMEPICKER, JQuery Component Extension
                "~/Scripts/library/select2/select2.min.js",                                 // SELECT2, JQuery Component Extension (Select Boxes)
                "~/Scripts/library/cadent-daypart-dropdown/daypart-dropdown-broadcast.js"   // DAYPART-DROPDOWN-BROADCAST, Cadent Component Extension
                );

            var appBundle = new ScriptBundle("~/bundles/app").Include(
                /* Vendor Imports */
                "~/Scripts/app/vendor/Class.js",

                /* Configuration */
                "~/Scripts/app/config/config.js",
                "~/Scripts/app/config/jqvalidate.js",
                "~/Scripts/app/config/w2ui.js",

                /* Utilities */
                //"~/Scripts/app/utilities/array.js",         // Prototype - pending removal
               // "~/Scripts/app/utilities/date.js",          // Prototype - pending removal
                "~/Scripts/app/utilities/scroll.js",
                "~/Scripts/app/utilities/global.js",        // Defines util 
                "~/Scripts/app/utilities/impressions.js",   // Prototypes onto util


                /* Services - Data*/
                "~/Scripts/app/services/httpService.js",

                /* Factories - Function */
                "~/Scripts/app/factories/wrappers.js",
                
                /* Controllers */
                "~/Scripts/app/controllers/BaseController.js",
                "~/Scripts/app/controllers/appController.js",

                /* View */
                "~/Scripts/app/views/BaseView.js",
                "~/Scripts/app/views/UploadManager.js", // closely coupled with view

                /* Knockout: Custom Bindings */
                "~/Scripts/library/cadent-knockout/custom-bindings/daypart-dropdown.js",
                "~/Scripts/library/cadent-knockout/custom-bindings/flightweek-picker.js",
                "~/Scripts/library/cadent-knockout/custom-bindings/select2.js",
                "~/Scripts/library/cadent-knockout/custom-bindings/modal.js",
                "~/Scripts/library/cadent-knockout/custom-bindings/bootstrap-tagsinput.js",
                "~/Scripts/library/cadent-knockout/custom-bindings/stopBubble.js"
                );

            var trackerBundle = new ScriptBundle("~/bundles/tracker").Include(
                /* Config */
                "~/Scripts/appTracker/config/TrackerConfig.js",

                /* Controllers */
                "~/Scripts/appTracker/controllers/TrackerMainController.js",

                /* View Models */
                "~/Scripts/appTracker/viewmodels/TrackerScheduleViewModel.js",
                
                /* Views */
                "~/Scripts/appTracker/views/TrackerMainView.js",
                "~/Scripts/appTracker/views/BvsFileListingView.js",

                /* Hybrid */
                "~/Scripts/appTracker/TrackerManageMappings.js",
                "~/Scripts/appTracker/TrackerManageRatingsBooks.js",
                "~/Scripts/appTracker/TrackerUploadManager.js"
                );



            /* See revise directory - revisions for React intgration */
            var planningBundle = new ScriptBundle("~/bundles/planning").Include(
                /* Config */
                "~/Scripts/appPlanning/config/PlanningConfig.js",

                /* Controllers */
                //"~/Scripts/appPlanning/controllers/PlanningMainController.js",
                 "~/Scripts/appPlanning/revise/PlanningController.js",
                //"~/Scripts/appPlanning/controllers/ProposalController.js",

                /* View Models */
                "~/Scripts/appPlanning/viewmodels/PlanningSearchViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/ProposalViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/CustomMarketsViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/CustomMarketsSelectorViewModel.js",
                "~/Scripts/appPlanning/viewmodels/ProposalDetailInventoryViewModel.js",
                "~/Scripts/appPlanning/viewmodels/ProposalDetailOpenMarketViewModel.js",
                 "~/Scripts/appPlanning/revise/CriteriaBuilderViewModel.js",
                "~/Scripts/appPlanning/revise/FilterViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/CriteriaBuilderViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/FilterViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/SwitchProposalVersionViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/VersionCreatedOptionsViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/ProposalUpdateWarningViewModel.js",
                //"~/Scripts/appPlanning/viewmodels/ManageRatingsViewModel.js",

                /* Views */
                "~/Scripts/appPlanning/views/PlanningMainView.js",
                //"~/Scripts/appPlanning/views/ProposalView.js",
                //"~/Scripts/appPlanning/views/ProposalDetailInventoryView.js",
                "~/Scripts/appPlanning/revise/ProposalDetailInventoryView.js",
                //"~/Scripts/appPlanning/views/ProposalDetailOpenMarketView.js",
                 "~/Scripts/appPlanning/revise/ProposalDetailOpenMarketView.js"
                //"~/Scripts/appPlanning/views/SwitchProposalVersionView.js",

                /* Hybrid */
                //"~/Scripts/appPlanning/ProposalDetailSet.js"
                );

            var trackerScrubBundle = new ScriptBundle("~/bundles/trackerScrub").Include(
                /* Config */
                /* Controllers */
                "~/Scripts/appTracker/controllers/TrackerScrubController.js",

                /* View Models */
                "~/Scripts/appTracker/viewmodels/TrackerScrubViewModel.js",

                /* Views */
                "~/Scripts/appTracker/views/TrackerScrubView.js"

                /* Hybrid */
                );

            var postingBundle = new ScriptBundle("~/bundles/posting").Include(
                /* Config */
                "~/Scripts/appPosting/config/PostingConfig.js",

                /* Controllers */
                "~/Scripts/appPosting/controllers/PostingMainController.js",
                "~/Scripts/appPosting/controllers/PostingUploadController.js",

                /* View Models */
                "~/Scripts/appPosting/viewmodels/PostingUploadViewModel.js",

                /* Views */
                "~/Scripts/appPosting/views/PostingMainView.js",
                "~/Scripts/appPosting/views/PostingUploadView.js",

                /* Hybrid */
                "~/Scripts/appPosting/PostingUploadManager.js"
                );

            var trafficBundle = new ScriptBundle("~/bundles/traffic").Include(
                /* Config */
                "~/Scripts/appTraffic/config/TrafficConfig.js",

                /* Controllers */
                "~/Scripts/appTraffic/controllers/TrafficController.js",

                /* View Models */

                /* Views */
                "~/Scripts/appTraffic/views/TrafficView.js"

                /* Hybrid */
                );

            var cssBundle = new StyleBundle("~/content/css").Include(
                /* Bootstrap */
                "~/Scripts/library/bootstrap-3.3.5/css/bootstrap.css",
                "~/Scripts/library/bootstrap-3.3.5/css/bootstrap-theme.css",
                "~/Scripts/library/bootstrap-tagsinput/bootstrap-tagsinput.css",
                "~/Scripts/library/select2/select2-bootstrap.min.css",

                /* Libraries & Toolkits */
                "~/Scripts/library/w2ui/w2ui-1.5.rc1.css",
                "~/Scripts/library/font-awesome/css/font-awesome.css",
                "~/Styles/css/library/animate.css",

                /* Component Extensions */
                "~/Scripts/library/bootstrap-tagsinput/bootstrap-tagsinput.css",
                "~/Scripts/library/daterangepicker/daterangepicker.css",
                "~/Scripts/library/webui-popover/jquery.webui-popover.min.css",
                "~/Scripts/library/timepicker/jquery.timepicker.css",
                "~/Scripts/library/select2/select2.min.css",
                "~/Scripts/library/cadent-daypart-dropdown/daypart-dropdown.css",

                /* App */
                "~/Styles/css/fonts/stack.css",
                "~/Styles/css/app.css"
                );

            bundles.Add(librariesBundle);
            bundles.Add(appBundle);
            bundles.Add(trackerBundle);
            bundles.Add(planningBundle);
            bundles.Add(trackerScrubBundle);
            bundles.Add(postingBundle);
            bundles.Add(trafficBundle);
            bundles.Add(cssBundle);
        }
    }
}
