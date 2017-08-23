//65094, 54423, 58362

$.fn.enterKey = function (fnc) {
    return this.each(function () {
        $(this).keypress(function (ev) {
            var keycode = (ev.keyCode ? ev.keyCode : ev.which);
            if (keycode == '13') {
                fnc.call(this, ev);
            }
        });
    });
};
var config = {                                                              // The configuration for the Proposal Viewer, contains classes and definitions that are called or used in the controller
    devMode: true,  //if true then apps will: show console logging; 

    //file status map: text color style
    fileStatusStates: {                                                     // Status classes and strings for files
        QUEUED: ['status-blue', 'Queued'],                                  // format [class, Info String]
        IN_REVIEW: ['status-blue', 'In Review'],                            // ex HTML <... class="... status-blue" >...In Review...</...>
        READY_FOR_APPROVAL: ['status-orange', 'Ready For Approval'],
        APPROVED: ['status-green', 'Approved'],
        COMPLETED: ['status-green', 'Completed'],
        CANCELLED: ['status-orange', 'Cancelled'],
        ERROR_PRODUCTION_LOAD: ['status-red', 'Error In Maestro Update'],
        ERROR: ['status-red', 'Error'],
        DEFAULT: ['status-blue', 'Ready']
    },
    //tbd
    statusStates: {                                                         // Status classes and String definitions for the Alert Bubble on the top right of the IMS page
        ready: ['info', 'Ready'],                                           // format [alert-subclass, Info String]
        error: ['danger', 'Error'],                                         // ex HTML for success: <div id="traffic_status" class="alert alert-success" role="alert" style="padding: 0 4px; margin: 8px 0px 0px 0px;"><strong>Status: Success</strong> - Refreshing IMS Totals</div>
        success: ['success', 'Success'],
        alert: ['warning', 'Alert']
    },
    // Begin UI Messages
    processingMsg: 'Processing...',                                         // The message that appears on the Loading screen when the page first loads or the grid data changes
    defaultErrorMsg: 'The server encountered an error processing the request.  Please try again or contact your administrator to review error logs.',   // Default Error Message - Shown in a bootstrap modal
    refreshMessage: 'Data has been refreshed to sync with changes on the server.',                                                                      // Refresh Error Message - Called when a refresh throws an error and is shown in the modal
    headError: 'An error was encountered Processing the Request',           // General HTTP error message when the httpservice.js module does not function for a variety of reasons

    //converter for spot tags display
    convertSpotsTagNumber: function (n) {
        var ret;
        if (n < 1 && n > 0) {
            ret = n.toFixed(2).toString().replace(/^0+/, '');
            //if converts to 1, switch
            if (ret == 1) {
                ret = n.toFixed(0);
            }
        } else if (n > 999) {
            ret = (n / 1000).toFixed(0) + 'k';
        } else {
            ret = n.toFixed(0);
        }
        return ret;
    },

    getHealthGradeClassName: function (healthRangeCode) {                   // Returns the class for the ISM health meter as displayed in the Spots column
        var className =                                                     // This name is added as a class to the trophy [<span>] element
            healthRangeCode === 0 ? 'none' :                                // Displays a grey of a value of 0
            healthRangeCode === 1 ? 'danger' :                              // Danger is red
            healthRangeCode === 2 ? 'warning' :                             // Warning ie orange-yellow
            'success';                                                      // Success is green
        return className;
    },

    getGridConfig: function (view) {                                        // Returns the w2grid [in w2ui] configuration
        var me = view;                                                      // The View that is getting the grid configuration (for IMS is it ProposalView.js), referenced for styling data
        var cfg = {                                                         // The actual config variable that is fed into w2grid
            grid: {
                name: 'ProposalsGrid',                                      // Grid name for referencing e.g. var grid = w2grid['ProposalsGrid'];
                header: 'Inventory',                                        // Header Name
                multiSelect: true,                                          // Flag that detmines whether multiple rows can be selected at once for operations
                //multiSearch: false,
                show: {                                                     // Grid elements to display from headers, to footers
                    header: true,                                           // Flag [bool]: Determines header visibility
                    footer: false,                                          // Flag [bool]: Determines footer visibility
                    toolbar: true,                                          // Flag [bool]: Determines toolbar visibility
                    //lineNumbers: true,
                    toolbarReload: false,                                   // Flag [bool]: Determines whether the toolbar contains a refresh button.                                           Dependent: [toolbar: true]
                    toolbarColumns: true,                                   // Flag [bool]: Determines whether to display a columns option button. i.e. checkboxes to toggle column visibility. Dependent: [toolbar: true]
                    toolbarSearch: false                                    // Flag [bool]: Determines whether to display a search bar                                                          Dependent: [toolbar: true]
                },

                toolbar: {                                                                                                                              // Tool bar items and options
                    items: [                                                                                                                            // Non-default toolbar members to display           
                        { type: 'break' },                                                                                                              // A visual divider
                        { type: 'button', id: 'add_networks_toolbar_btn', caption: 'Add Network', icon: ''/*'fa fa-caret-down'*/ }                      // Button to add a network; event caught and processed by toolbar.onClick()
                    ],
                    onClick: function (target, data) {                      // Event handler to handle clicks on the toolbar, done via html id filtering
                        if (target == 'add_networks_toolbar_btn') {         // Handle a click from the Add Network Button [id=''add_networks_toolbar_btn'']
                            me.onToolbarAddNetwork();                       // Call the add network method
                        }
                        }
                },

                /*searches : [
                    { field: 'Network', caption: 'Network', type: 'text' },
                    { field: 'SpotLength', caption: 'Length', type: 'text' },
                    { field: 'Daypart', caption: 'Daypart', type: 'text' }
                ],

                onSearch: function(event) {
                    console.log('grid search', event);
                },   */

                menu: [                                                     // Context Menu item definitions; handled via w2grid.onMenuClick http://w2ui.com/web/docs/w2grid.onMenuClick
                    { id: 1, text: 'Display Network Data' },                // Option 1: Displays relevent network data
                    { id: 2, text: 'Bulk Edit Daypart'},                    // Option 2: Bulk edit dayparts
                    { id: 3, text: 'Delete Row' }                           // Option 3: Delete a network row
                    //icon?
                    //{ id: 2, text: 'Delete Row', icon: 'fa-minus' }
                ],

                columns: [
                    {                                                       // Definition for the Network Row
                        field: 'Network',                                   // Field name in JSON (Network)
                        caption: 'Network',                                 // Caption to be displayed at row head
                        size: '200px',                                      // Width of 200px to allow for proper spacing
                        sortable: true,                                     // Can sort by networks
                        searchable: true,                                   // Can search but doesn't matter if there isn't a search bar
                        hideable: false,                                    // Can't hide this column because it is important and hide changes are persistent over reloads
                        render: function (record, index, column_index) {    // Custom render function so we cn play with our data
                            if (record.summary === true) {
                                return null;                                // If this is the totals row (summary record) then don't diplay anything
                            }
                            var cell = '';                                  // Cell HTML data because why not have a throwback to the old CGI-BIN days [looking at you MXD]
                            if (record.addNetwork === true) {                                                                                                                       // If this is the AddNetwork Row
                                cell += '<div class="flex-container-1 editable-cell">';                                                                                             // Add a editable container so the user can enter some data
                                cell += '<select id="add_network_grid_select" class="edit-input"></select>';                                                                        // What data you may ask? A Network from a dropdown list
                                cell += '<button id="add_network_grid_btn" class="btn btn-xs btn-link">Add Network <i class="fa fa-caret-down" aria-hidden="true"></i></button>';   // Add a button so we confirm selections
                                cell += '</div>';                                                                                                                                   // *drops mic*
                            } else {
                                cell += "<div>" + record.Network + "</div>";                                                                                                        // If this is a regular row, show the user what network they are looking at
                            }
                            return cell;                                    // Returns the html content of our custom cell
                        }
                    },
                    {
                        field: 'SpotLength',                                // Field for duration of the spot (15s, 30s, 60s, etc...)
                        caption: 'Length',                                  // Caption for the row title
                        size: '100px',                                      // Width of column
                        sortable: true,                                     // Sortable: yes
                        searchable: true,                                   // You can search this but that doesn't matter because there is not damn search bar
                        hideable: false,                                    // Can't hide this, user needs to see
                        render: function (record, index, column_index) {    // Render function, the returned HTML will be displayed in the Length column
                            if ((record.summary === true) || (record.addNetwork === true)) {    // If it isn't a row with actual data
                                return null;                                                    // Send back nothing because this row is empty
                            }
                            var cell = "<span>:" + record.SpotLength + "</span>";               // Show the spotlength in terms of seconds in the row
                            return cell;                                                        // Return HTML data to render in the cell
                        }
                    },
                    {
                        field: 'DaypartSortDisplay',                        // Display for the daypart that this network will be run on ('DaypartSortDisplay' is the fieldname in JSON)
                        caption: 'Daypart',                                 // Caption for the row title
                        size: '200px',                                      // Width of the column
                        sortable: true,                                     // Sortable: yes
                        searchable: true,                                   // You can search this but that doesn't matter because there is not damn search bar
                        hideable: false,                                    // Can't hide this, user needs to see
                        render: function (record, index, column_index) {    // Render function, returned HTML will be put in this row's cell contents
                            if (record.Daypart) {                           // if the Daypart is returned in the record
                                var daypart = record.Daypart.FullString;                                        // get the daypart from record object
                                var id = "daypart-" + record.recid;                                             // custom build an element id for ease of future reference
                                var cell = "<div id='" + id + "' class='flex-container-1 editable-cell'>";      // Make an editable cell
                                cell += "<input type='text' class='edit-input'></input>";                       // add a text input
                                cell += "<div>" + daypart + "</div>";                                           // Display the daypart so the user can see it
                                if (!record.summary)                                                            // If this isn't a totals row
                                    cell += "<i class='pull-right fa fa-caret-down' aria-hidden='true'></i>";   // Add an icon so the user knows this is a dropdown; this is the icon http://fontawesome.io/icon/caret-down/
                                cell += "</div>";                                                               // End this cell
                                return cell;                                                                    // Return the cell so we can see it
                            } else {
                                return null;                                                                    // Since there is no Daypart, We know this row is not a data row and can display nothing (event for the summary, because how do you sum dayparts)
                            }
                        }
                    },
                    {                                                       // Row for the Equalized Household CPM data
                        field: 'HhEqCpm',                                   // Field for the Equalized Household CPM metric (fieldname in the JSON model)
                        caption: 'HH CPM',                                  // Caption to Display as the Row Title
                        size: '150px',                                      // Width of the column
                        sortable: true,                                     // Sortable: yes
                        searchable: true,                                   // You can search this but that doesn't matter because there is not damn search bar
                        hideable: false,                                    // Can't hide this, user needs to see
                        render: function (record, index, column_index) {    // Render function, returned HTML will be put in this row's cell contents
                            if (record.addNetwork === true) {               // If this the Add Network Row (A special row in the data)
                                return null;                                // Return nothing because there is no HhEqCPM for an empty row
                            }
                            var cell = '';                                  // HTML contents of this cell
                            var editClassses = '';                                                                                                                          // classes that enable editing of the cell, changed further in the code to allow for dynamic loading
                            var hasImsLoaded = record.ImsInfo !== null && typeof record.ImsInfo.IsSet !== 'undefined' ? record.ImsInfo.IsSet : false;                       // Look at the presence of objects to determine if IMS data is loaded (these objects only exist during the loading phase)
                            var id = "cpm-" + record.recid;                                                                                                                 // Custom id of the cell for easier future reference
                            if (!record.summary) {                                                                                                                          // If this is not the last row
                                editClassses = "editable-cell cpm-cell" + (hasImsLoaded ? "" : " is-loading");                                                              // Classes that enable editing of the Cell
                                cell += "<div id='" + id + "' data-recid='" + record.recid + "' class='flex-container-1 ims-cell " + editClassses + "'>";                   // Add a flex container and custom classes for IMS data display
                                cell += "<input type='text' class='edit-input edit-cpm-input'></input>";                                                                    // Add an input for ability to get user input
                                cell += "$" + record.HhEqCpm.toFixed(2);                                                                                                    // Only render to a possible currency amt e.g XX.XX not XX.XXXXXXXX...
                                cell += "<div class='ims-content label-cpm'>" + (hasImsLoaded ? "$" + record.ImsRecommendedCpm.HhEqCpmStart.toFixed(2) : "") + "</div>";    // If the IMS data has loaded then display, if not show nothing
                                cell += "<div class='text-loading animated-background' style='width: 40px;'><span></span></div>";                                           // Show a loading background class, this will be removed when IMS is finished loading
                                cell += "</div>";                                                                                                                           // End this cell
                            }
                            else {                                                                                                                                          // If this is the last cell, then show totals
                                editClassses = (hasImsLoaded ? "" : "is-loading");                                                                                          // Only show summary data if it is the summary row
                                cell += "<div id='" + id + "' data-recid='" + record.recid + "' class='flex-container-1 ims-cell " + editClassses + "'>";                   // Add a data display cell
                                cell += "$" + record.HhEqCpm.toFixed(2);                                                                                                    // Show the final amount of spots
                                cell += "</div>";                                                                                                                           // End this cell
                            }
                            return cell;
                        },
                    },
                    {
                        field: 'GdEqCpm',
                        caption: 'Demo CPM',
                        size: '100px',
                        sortable: true,
                        searchable: false,
                        render: function (record, index, column_index) {
                            if (record.addNetwork === true) {
                                return null;
                            }
                            return "<div>$" + record.GdEqCpm.toFixed(2) + "</div>";
                        },
                    },
                    {
                        field: 'ContractedUnits',
                        caption: 'Spots',
                        size: '150px',
                        sortable: true,
                        searchable: false,
                        hideable: false,
                        render: function (record, index, column_index) {
                            if (record.addNetwork === true) {
                                return null;
                            }
                            var hasImsLoaded = record.ImsInfo !== null && typeof record.ImsInfo.IsSet !== 'undefined' ? record.ImsInfo.IsSet : false;
                            var id = "spot-" + record.recid;

                            var editClassses = !record.summary ? " editable-cell spot-cell" : "";
                            var cell = "<div id='" + id + "' data-recid='" + record.recid + "' class='flex-container-1 ims-cell" + (hasImsLoaded ? "" : " is-loading ") + editClassses + "'>";
                            if (!record.summary) {
                                cell += "<input type='text' min='0' max='999' maxlength='3' placeholder='0-999' class='edit-input edit-spot-input'></input>";
                            }
                            cell += "<div>" + record.ContractedUnits + "</div>";
                            if (hasImsLoaded) {
                                var healthRangeCode = record.ImsInfo.ImsAllocatedUnits > 0 ? record.ImsInfo.ImsHealthRangeCode : 0;
                                var className = config.getHealthGradeClassName(healthRangeCode);
                                var won = config.convertSpotsTagNumber(record.ImsInfo.ImsAllocatedUnits);
                                var inv = config.convertSpotsTagNumber(record.ImsInfo.ImsAvailableUnits);

                                cell += '<div class="ims-content wrap-spot"><span class="spots-net won ' + className + '"><span class="s-won">' + won + '</span><i class="fa fa-trophy" style="margin-left: 1px;"></i></span><span class="spots-net inventory ' + className + '">' + inv + '</span></div>';
                                cell += "<span class='text-loading animated-background' style='width: 40px;'><span></span></span>";
                                cell += "</div>";
                            }
                            else {
                                cell += '<div class="ims-content wrap-spot"><span class="spots-net won"><i class="fa fa-trophy" style="margin-left: 1px;"></i></span><span class="spots-net inventory"></span></div>';
                                cell += "<span class='text-loading animated-background' style='width: 40px;'><span></span></span>";
                                cell += "</div>";
                            }
                            return cell;
                        },
                    },
                    {
                        field: 'ContractedHhEqImpressionsTotal',
                        caption: 'Imp (000)',
                        sortable: true,
                        size: '150px',
                        searchable: false,
                        hideable: false,
                        render: function (record, index, column_index) {
                            if (record.addNetwork === true) {
                                return null;
                            }
                            var impressions = (record.ContractedHhEqImpressionsTotal / 1000).toFixed(0);
                            //var hasIMSLoaded = record.ImsInfo.IsSet;
                            var cell = "<div class='flex-container-1'>";
                            cell += "<span>" + parseInt(impressions).toLocaleString('en') + "</span>";
                            if (!record.summary && record.ImpressionsPerc) {
                                //if (hasIMSLoaded) {
                                cell += "<span class='text-perc'>" + (record.ImpressionsPerc).toFixed(0) + "%</span>"; //} else {
                                //    cell += "<span class='text-loading animated-background'><span></span></span>"
                                //}
                            }
                            cell += "</div>";
                            return cell;
                        },
                    },
                    {
                        field: 'Trp',
                        caption: "TRP",
                        sortable: true,
                        size: '100px',
                        render: function (record, index, column_index) {
                            if (record.addNetwork === true) {
                                return null;
                            }
                            var cell = "";
                            cell += "<div class='flex-container-1'>";
                            cell += "<span>" + record.Trp.toFixed(2) + "</span>";
                            cell += "</div>";
                            return cell;
                        }
                    },
                    {
                        field: 'ContractedRate', caption: 'Rate', size: '100px', sortable: true, hideable: false,
                        render: function (record, index, column_index) {
                            if ((record.summary === true) || (record.addNetwork === true)) {
                                return null;
                            } else {
                                var rate = record.ContractedRate || 0;
                                var rateDisplay = '$' + w2utils.formatNumber(rate);
                                var id = "rate-" + record.recid;
                                var cell = "<div id='" + id + "' class='flex-container-1 editable-cell'>";
                                cell += "<input class='edit-input' type='text' maxLength='11' ></input>";
                                cell += "<div>" + rateDisplay + "</div>";
                                cell += "</div>";
                                return cell;
                            }
                        }
                    },
                    {
                        field: 'ContractedTotalCost', caption: 'Cost', sortable: true, size: '150px',
                        searchable: false,
                        hideable: false,
                        render: function (record, index, column_index) {
                            if (record.addNetwork === true) {
                                return null;
                            }
                            var cell = "<div class='flex-container-1'>";
                            cell += "<span>$" + record.ContractedTotalCost.toLocaleString('en') + "</span>";
                            if (record.ContractedTotalCostPerc) {
                                if (!record.summary) {
                                    cell += "<span style='color: #8e8e8e'>" + record.ContractedTotalCostPerc.toFixed(1) + "%</span>";
                                }
                            }

                            cell += "</div>";
                            return cell;
                        }
                    }
                ]
            },

            getBulkEditFormCfg: function (_view) {
                var vw = _view;
                var confg = {
                    //header: 'Create Division',
                    name: 'BulkEditForm',
                    focus: -1,
                    fields: [
                        {
                            name: 'daypart', type: 'text', required: false, html: { caption: 'Selected Daypart', attr: 'size="30"' },
                            options: {}
                        }
                    ]

                };
                return confg;

            },

            daypartPopoverGrid: {
                name: 'DaypartDataGrid',
                header: 'Daypart Breakdown',
                show: {
                    header: true
                },
                columns: [
                    {
                        field: 'Daypart', caption: 'Daypart', size: '35%', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                return "<div style='font-weight: bold; text-align: right;'>Totals</div>";
                            }

                            return "<div>" + record.Daypart + "</div>";
                        }
                    },
                    {
                        field: 'HhEqImpressions', caption: 'Imp. (000)', size: '25%', sortable: true,
                        render: function (record, index, column_index) {
                            var value = parseFloat((record.HhEqImpressions / 1000).toFixed(2)).toLocaleString("en", { minimumFractionDigits: 2 });
                            var percentage = ((record.HhEqImpressions / record.TotalHhEqImpressions) * 100).toFixed(1);
                            var percentageElem = !record.summary ? "<div>" + percentage + "%</div>" : "";

                            return "<div class='flex-container-1'><div>" + value + "</div>" + percentageElem + "</div>";
                        }
                    },
                    {
                        field: 'Trp',
                        caption: 'TRP',
                        size: '60px',
                        sortable: true,
                        render: function (record, index, column_index) {
                            return record.Trp.toFixed(2);
                        }
                    },
                    { field: 'Cpp', caption: 'CPP', size: '100px', sortable: true, render: 'money' }
                ]
            },
            bulkDaypartModal: {
                name: 'BulkDaypartModal',
                fields: [
                        {
                            name: 'daypart', type: 'text', required: false, html: { caption: 'Selected Daypart', attr: 'size="30"' },
                            options: {}
                        }
                ]
            },
            networkModalGrid: {
                name: 'NetworkDataGrid',
                columns: [
                    {
                        field: 'Advertiser', caption: 'Advertiser', size: '20%', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                return;
                            }
                            var circleElem = record.ActiveProposal ? "<i class='fa fa-circle' style='color: blue; margin: 0 10px 0 5px' aria-hidden='true'></i>" : "";
                            return "<div style='position: relative;'>" + circleElem + record.Advertiser + "</div>";
                        }
                    },
                    { field: 'Product', caption: 'Product', size: '20%', sortable: true },
                    { field: 'Status', caption: 'Status', size: '70px', sortable: true },
                    {
                        field: 'ContractedHhEqCpm', caption: 'CPM', size: '120px', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                return;
                            }
                            var cell = "<div class='flex-container-1'>";
                            cell += "<div>$" + record.ContractedHhEqCpm.toFixed(2) + "</div>";
                            if (record.ActiveProposal) {
                                cell += "<div class='label-cpm'>$" + record.HhEqCpmStart + "</div>";
                            }
                            cell += "</div>";
                            return cell;
                        }
                    },
                    {
                        field: 'RivalDaypart', caption: 'Rival Daypart', size: '120px', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                return "<div style='font-weight: bold; text-align: right;'>Totals</div>";
                            }
                            return record.RivalDaypart;
                        }
                    },
                    {
                        field: 'RivalContractedUnits', caption: 'Rival Spots', size: '120px', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                var healthRangeCode = record.TotalAllocatedUnits > 0 ? record.ImsHealthRangeCode : 0;
                                var className = config.getHealthGradeClassName(healthRangeCode);
                                var cell = '<div class="flex-container-1 ims-cell spot-cell">';
                                cell += '<div>' + record.TotalRivalContractedUnits.toFixed(1) + '</div>';
                                cell += '</div>';
                                return cell;
                            }

                            var healthRangeCode = record.ImsInfo.ImsAllocatedUnits > 0 ? record.ImsInfo.ImsHealthRangeCode : 0;
                            var className = config.getHealthGradeClassName(healthRangeCode);
                            var won = config.convertSpotsTagNumber(record.ImsInfo.ImsAllocatedUnits);
                            var inv = config.convertSpotsTagNumber(record.ImsInfo.ImsAvailableUnits);

                            var cell = '<div class="flex-container-1 ims-cell spot-cell">';
                            cell += '<div>' + (record.ActiveProposal ? record.RivalContractedUnits.toFixed(0) : record.RivalContractedUnits.toFixed(1)) + '</div>';
                            cell += '<div class="ims-content wrap-spot">';
                            cell += '<span class="spots-net won ' + className + '">';
                            cell += '<span class="s-won">' + won + '</span>';
                            cell += '<i class="fa fa-trophy" style="margin-left: 1px;"></i>';
                            cell += '</span>';
                            if (record.ActiveProposal)
                                cell += '<span class="spots-net inventory success">' + inv + '</span>';
                            cell += '</div></div>';
                            return cell;
                        }
                    },
                    { field: 'Daypart', caption: 'Booked Daypart', size: '120px', sortable: true },
                    {
                        field: 'ContractedUnits', caption: 'Booked Spots', size: '100px', sortable: true,
                        render: function (record, index, column_index) {
                            var cell = "";
                            if (record.summary === true) {
                                cell += "<div>" + record.TotalContractedUnits.toFixed(0) + "</div>";
                            } else {
                                cell += "<div>" + record.ContractedUnits.toFixed(0) + "</div>";
                            }
                            return cell;
                        }
                    },
                    {
                        field: 'ContractedCost', caption: 'Booked Value', size: '100px', sortable: true,
                        render: function (record, index, column_index) {
                            if (record.summary === true) {
                                var value = parseInt(record.TotalContractedCost.toFixed(0)).toLocaleString("en");
                                return "<div>$" + value + "</div>";
                            }
                            var value = parseInt(record.ContractedCost.toFixed(0)).toLocaleString("en");
                            return "<div>$" + value + "</div>";
                        }
                    }
                ],
                sortData: [
                    {
                        field: 'ContractedHhEqCpm',
                        direction: 'asc'
                    }
                ]
            }
        };
        return cfg;
    }

};


if (!Object.assign) {
    Object.defineProperty(Object, 'assign', {
        enumerable: false,
        configurable: true,
        writable: true,
        value: function (target) {
            'use strict';
            if (target === undefined || target === null) {
                throw new TypeError('Cannot convert first argument to object');
            }

            var to = Object(target);
            for (var i = 1; i < arguments.length; i++) {
                var nextSource = arguments[i];
                if (nextSource === undefined || nextSource === null) {
                    continue;
                }
                nextSource = Object(nextSource);

                var keysArray = Object.keys(Object(nextSource));
                for (var nextIndex = 0, len = keysArray.length; nextIndex < len; nextIndex++) {
                    var nextKey = keysArray[nextIndex];
                    var desc = Object.getOwnPropertyDescriptor(nextSource, nextKey);
                    if (desc !== undefined && desc.enumerable) {
                        to[nextKey] = nextSource[nextKey];
                    }
                }
            }
            return to;
        }
    });
}