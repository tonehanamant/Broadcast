var config = {
    devMode: true,  //if true then apps will: show console logging; 

    //file status map: text color style
    fileStatusStates: {
        QUEUED: ['status-blue', 'Queued'],
        IN_REVIEW: ['status-blue', 'In Review'],
        READY_FOR_APPROVAL: ['status-orange', 'Ready For Approval'],
        APPROVED: ['status-green', 'Approved'],
        COMPLETED: ['status-green', 'Completed'],
        CANCELLED: ['status-orange', 'Cancelled'],
        ERROR_PRODUCTION_LOAD: ['status-red', 'Error In Maestro Update'],
        ERROR: ['status-red', 'Error'],
        DEFAULT: ['status-blue', 'Ready']
    },
    //tbd
    statusStates: {
        ready: ['info', 'Ready'],
        error: ['danger', 'Error'],
        success: ['success', 'Success'],
        alert: ['warning', 'Alert']
    },

    processingMsg: 'Processing...',

    defaultErrorMsg: 'The server encountered an error processing the request.  Please try again or contact your administrator to review error logs.',
    refreshMessage: 'Data has been refreshed to sync with changes on the server.',
    headError: 'An error was encountered Processing the Request',

    //cancel/save icon - only show if edited
    copyActionsRenderFunction: function (record, index, column_index) {
        var id = record.recid;
        var cell = '<div id="actions_' + id + '" class="edit-actions" style="display: none;">';
        if (!record.creatingCopy) {
            cell += '<a  data-recid="' + id + '" name="save" title="Save edit Copy" style="padding: 2px 4px 0 2px; color: green;" class="btn btn-link"><span class="glyphicon glyphicon-check" aria-hidden="true"></span></a>';
        }
        cell += '<a  data-recid="' + id + '" name="cancel" title="Cancel edit Copy" style="padding: 2px 0 0 4px;  color: red;" class="btn btn-link"><span class="glyphicon glyphicon-remove" aria-hidden="true"></span></a>';
        cell += '</div>'

        return cell;
    },

    getIsciTags: function (rec, id) {
        var ret = '<div>';
        if (rec.Hd) ret += '<span class="label label-primary">HD</span> ';
        if (rec.Esp) ret += '<span class="label label-primary">ESP</span> ';
        if (rec.Married) ret += '<span data-id="' + id + '" class="label label-primary label-married"><span class="glyphicon glyphicon-link" aria-hidden="true"></span></span> ';
        if (rec.House) ret += '<span class="label label-primary"><span class="glyphicon glyphicon-home" aria-hidden="true"></span></span> ';
        if (rec.SensitiveContent) ret += '<span class="label label-primary"><span class="glyphicon glyphicon-ban-circle" aria-hidden="true"></span></span>';

        ret += '</div>';
        return ret;
    },

    getCopyGridCfg: function (view) {
        var me = this;
        var gridCfg = {
            name: 'CopyGrid',
            header: 'Attached Copy',
            //multiSelect: true,
            show: {
                header: true,
                footer: true,
                //toolbar: true,
                //lineNumbers: true,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            menu: [
                { id: 1, text: 'Delete Copy' }
            ],
            onSelect: function(e) {
                e.preventDefault();
            },
            columns: [

                {
                    field: 'Isci', caption: 'ISCI', sortable: true, size: '20%',
                    //tbd - labels/icons - display and checkable (create)
                    render: function (record, index, column_index) {
                        var cell = "<div class='flex-container-1'>";
                        if (record.newCopy === true && !record.creatingCopy) {
                            //return "<div>New Copy</div>";
                            cell += '<button id="isci_btn" class="btn btn-xs btn-link">Add Copy <span class="glyphicon glyphicon-plus" aria-hidden="true"></span></button>';
                        }
                        else if (record.creatingCopy === true) {
                            var cellId = "isci-" + record.recid;
                            cell += "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='flex-container-1 editable-cell is-editing'>"
                            cell += "<input id='isci_input' class='edit-input'></select>";
                            cell += "</div>";
                        }
                        else {
                            cell += "<div>" + record.RootCopy.Isci + "</div>"
                            cell += me.getIsciTags(record.RootCopy, record.recid);
                        }

                        //cell += "<div>" + record.Isci + "</div>";
                        //var testRec = { "Married": true, "Hd": true, "Esp": true, "House": true, "SensitiveContent": true };
                        //cell += me.getIsciTags(record);
                        cell += "</div>"
                        return cell;
                    }
                },
                {
                    field: 'RootCopy.SpotLengthDisplay', caption: 'Length', size: '60px', sortable: true,
                    render: function (record, index, column_index) {
                        return record.RootCopy.SpotLengthDisplay === 'new' ? '' : record.RootCopy.SpotLengthDisplay;
                    }
                },
                { field: 'ProductString', caption: 'Product', size: '15%', sortable: true },
                {
                    field: 'ReelName', caption: 'Reel', sortable: true, size: '13%',
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }
                        var cell = '';
                        //GU: only editable if NOT IsCopyReleased
                        if (record.IsCopyReleased === true) {
                            cell += "<div class='flex-container-1'>" + record.ReelName + "</div>";
                        } else {
                            var cellId = "reel-" + record.recid;
                            cell += "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='flex-container-1 editable-cell reel'>";
                            cell += "<select id='reel-select' class='edit-input'></select>";
                            cell += "<div>" + record.ReelName + "</div>";
                            cell += "</div>";
                        }

                        return cell;
                    }
                },
                {
                    field: 'DispositionString', caption: 'Disposition', sortable: true, size: '75px',
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }
                        var cell = '';
                        var label = record.DispositionId === 1 ? "Current" : record.DispositionId === 2 ? "Prior" : "New";
                        //GU: only editable if NOT IsCopyReleased
                        if (record.IsCopyReleased === true) {
                            cell += "<div class='flex-container-1'><div tooltip='" + record.DispositionString + "' class='reel-disposition-label'>" + label + "</div></div>";
                        } else {
                            var cellId = "reel-disposition-" + record.recid;
                            cell += "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='flex-container-1 editable-cell reel'>";
                            cell += "<select id='reel-disposition-select' class='edit-input reel-disposition-select'></select>";
                            cell += "<div tooltip='" + record.DispositionString + "' class='reel-disposition-label'>" + label + "</div>";
                            cell += "</div>";                            
                        }
                        return cell;
                    }
                },
                {
                    field: 'FlightWeekString', caption: 'Flight', sortable: true, size: '260px',
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }

                        var cellId = "flight-" + record.recid;
                        var cell = "<div recidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input type='text' class='edit-input' />"
                        cell += '<i class="calendar-icon glyphicon glyphicon-calendar fa fa-calendar"></i>'
                        cell += "<div>" + record.FlightWeekString;
                        //test
                        //record.HasHiatusWeeks = true;
                        if (record.HasHiatusWeeks) cell += '<span class="" data-html="true" data-toggle="tooltip" title="' + record.LongFlightWeekString + '"><span class="glyphicon glyphicon-info-sign" style="margin-left: 2px;" aria-hidden="false"></span></span>';
                        cell += "</div></div>";
                        return cell;
                    }
                },
                //{ field: 'Rotation', caption: 'Rotation', size: '80px', sortable: true, render: function (record) { return record.Rotation + '%'; } },
                {
                    field: 'Rotation', caption: 'Rotation', size: '70px', sortable: true,
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }

                        var cellId = "rotation-" + record.recid;
                        var cell = "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='flex-container-1 editable-cell'>";
                        cell += "<input id='rotation-input' type='number' min='1' max='100' class='edit-input' />";
                        cell += "<div>" + record.Rotation + "</div>";

                        cell += "</div>";
                        return cell;
                    }
                },
                {
                    field: 'MvpdFormattedName', caption: 'Targeting', size: '135px', sortable: true,
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }
                        var cell = '';
                        //GU: only editable if NOT IsCopyReleased
                        if (record.IsCopyReleased === true) {
                            cell += "<div class='flex-container-1'>" + record.MvpdFormattedName + "</div>";
                        } else {
                            var cellId = "mvpd-" + record.recid;
                            cell += "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='flex-container-1 editable-cell'>";
                            cell += "<select id='mvpd-select' class='edit-input'></select>";
                            cell += "<div>" + record.MvpdFormattedName + "</div>";
                            cell += "</div>";
                        }
                        return cell;
                    }
                },
                {
                    field: 'SdComment', caption: 'Comments', size: '15%', sortable: true,
                    render: function (record, index, column_index) {
                        if (record.newCopy === true) {
                            return null;
                        }

                        var cellId = "comments-" + record.recid;
                        var cell = "<div recidEdit='" + record.recid + "' id='" + cellId + "' class='editable-cell' style='position: relative; height: 24px; padding-top: 5px;'>";

                        cell += "<div class='edit-input-container' style='width: 100%;'>"
                        //CRMWRC-355 bug - display of icon overlap - adjust size, flex - add elipsis; minimize icon footprint (css)
                        if (record.RootCopy.Hd) {
                            cell += "<div id='comments-div-popover' class='edit-input' style='flex: .9; -ms-flex: .9; width:100%; height: 100%;'>" + (record.SdComment || '') + "</div>";
                            cell += "</div>"
                            cell += "<div title='" + (record.HdComment || '') + "' class='hd-comment-icon'>HD</div>";
                        } else {
                            cell += "<input id='SDCommentsInput' type='text' maxlength='255' class='edit-input' />";
                            cell += "</div>"
                        }
                        cell += "<div style='width: 90%; overflow: hidden; text-overflow: ellipsis; height: 100%;'>" + (record.SdComment || '') + "</div>";

                        cell += "</div>";
                        return cell;
                    }
                },
                { field: 'actions', caption: 'Actions', hidden: false, size: '65px', sortable: false, render: me.copyActionsRenderFunction }
            ],

            sortData: [{ field: 'recid', direction: 'ASC' }]
            //wait for on complete as selections do not change immediately
            /* onSelect: function (event) {
                // console.log(event);

                event.onComplete = function () {
                    view.onUpdateSetLimitsState();
                };
            },

            onUnselect: function (event) {
                //console.log(event);
                event.onComplete = function () {
                    view.onUpdateSetLimitsState();
                };
            } */
        };

        return gridCfg;
    },

    getOrdersGridConfig: function (view) {
        return {
            name: 'OrdersGrid',
            menu: [
                { id: 1, text: 'Edit order(s)' }
            ],
            columns: [
                { field: 'TrafficName', caption: 'Order', hidden: false, size: '15%', sortable: true },
                { field: 'TrafficId', caption: 'ID', hidden: false, size: '5%', sortable: true },
                { field: 'AdvertiserString', caption: 'Advertiser', hidden: false, size: '15%', sortable: true },
                { field: 'ProductString', caption: 'Product', hidden: false, size: '20%', sortable: true },
                { field: 'ReleaseName', caption: 'Release', hidden: false, size: '10%', sortable: true },
                { field: 'FlightWeekString', caption: 'Flight', hidden: false, size: '15%', sortable: true },
                { field: 'LastUpdatedTime', caption: 'Last Updated', hidden: false, size: '7%', sortable: true, render: 'date' },
                { field: 'LastUpdatedBy', caption: 'Last Updated by', hidden: false, size: '7%', sortable: true },
                { field: 'Status', caption: 'Status', hidden: false, size: '6%', sortable: true },
            ],
            //sortData: [{ field: 'recid', direction: 'ASC' }]
        }
    },

    getGrid1Config: function () {
        //var btnNewMaster = "<div style='width: 40%; display: inline-block;'><button class='btn btn-xs btn-default' style='margin: 1px 0 0 3px;'>New Master</button></div>";
        //var toolbarSelect = "";
        //var searchInput = "<div><input placeholder='Search Master Alerts' /><span class='glyphicon glyphicon-search' style='right: 20px;' aria-hidden='true'></span></div>"
        //var html = "<div style='width: 100%;'>" + btnNewMaster + toolbarSelect + "</div>";
        return {
            name: 'grid1',
            show: {
                toolbar: true,
                toolbarReload: false,
                toolbarColumns: false,
                //toolbarSearch: false,
            },
            toolbar: {
                style: 'width: 100%',
                items: [
                    { type: 'break' },
                    //{ type: 'button', id: 'new-master', caption: 'New Master' },
                    //{ type: 'html', id: 'item6', html: html }
                    { type: 'html', id: 'item6', html: '<div class="grid-toolbar-select"><select id="master-type-select"></select></div>' }
                ],
                right: "<button id='new-master-btn' type='button' style='margin: -3px 5px 0 0;' class='btn btn-sm btn-default'>New Master</button>",
                //right: searchInput,
                onClick: function (target, data) {
                    console.log(target);
                }
            },
            //searches: [
            //    { field: 'masterAlert', caption: 'Master Alert', type: 'text' }
            //],
            columns: [
                { field: 'Name', caption: 'Master Alert', size: '35%' },
                { field: 'NoOfAlerts', caption: 'Alerts', size: '60px' },
                { field: 'Status', caption: 'Status', size: '20%' },
                { field: 'progressDisplay', caption: 'PDF Progress', hidden: true, size: '30%' }
                //{
                //    field: 'Progress', caption: 'Progress', size: '25%',
                //    render: function (record, index, column_index) {
                //        var elemId = "generatePdfProgressBar-" + record.Id;
                //        var cell = '<div id="' + elemId + '" class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%; padding: 0; height: 24px;">';
                //        cell += '</div>';

                //        return cell;
                //    }
                //}
            ],
            multiSelect: false
        }
    },

    grid2: {
        name: 'grid2',
        show: {
            toolbar: true,
            toolbarReload: false,
            toolbarColumns: false,
            lineNumbers: true
        },
        dragSelect: false,
        menu: [
         { id: 1, text: 'Edit order' }
        ],
        columns: [
            { field: 'TrafficName', caption: 'Alerts', size: '15%' },
            { field: 'TrafficId', caption: 'Order ID', size: '70px' },
            { field: 'Release', caption: 'Release', size: '15%' },
            { field: 'AlertType', caption: 'Type', size: '15%' },
            { field: 'EffectiveDate', caption: 'Copy End Date', size: '130px', render: 'date:mm/dd/yyyy' },
            { field: 'AlertComment', caption: 'Comments', size: '35%' },
        ],
    },

    grid3: {
        name: 'grid3',
        //header: 'Unassigned Alerts',
        dragSelect: false,
        show: {
            //header: true,
            toolbar: true,
            toolbarReload: false,
            toolbarColumns: false,
        },
        menu: [
            { id: 1, text: 'Edit order' },
            { id: 2, text: 'Delete order' }
        ],
        columns: [
            { field: 'TrafficName', caption: 'Unassigned Alerts', size: '15%', sortable: true },
            { field: 'TrafficId', caption: 'Order ID', size: '70px' },
            { field: 'Release', caption: 'Release', size: '15%', sortable: true },
            { field: 'AlertType', caption: 'Type', size: '15%', sortable: true },
            { field: 'EffectiveDate', caption: 'Copy End Date', size: '130px', render: 'date:mm/dd/yyyy', sortable: true },
            { field: 'AlertComment', caption: 'Comments', size: '35%' },
        ],
    }
};