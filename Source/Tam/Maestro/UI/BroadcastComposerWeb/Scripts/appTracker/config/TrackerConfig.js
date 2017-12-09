var TrackerConfig = {

    //TBD - for a normal configuration object or below for specific
    //set util property deep to false to avoid merging arrays
    getCfg: function(name, obj) {
        var item = this[name];
        if (item) return util.copyData(item, obj, null, true);
        return null;
    },

    //copy a object deep and optionally add additional properties to
    copyCfgItem: function(item, xtra) {
        return util.copyData(item, xtra, null, true);
    },

    formatDate: function (dateString) {
        if (!dateString)
            return "";

        dateString = dateString.replace(/-/g, '\/').replace(/T.+/, '');
        return moment(new Date(dateString)).format('MM/DD/YY');
    },

    //RENDERER related: also see global renderers as needed (in config.js)

    //correspond to class and icon for BVS status in grid: 1 in spec, 2 needs attention , 3 officiall out of spec
    bvsStatusStates: [
        ['status-green', 'thumbs-up', 'in spec'], // InSpec = 1
        ['status-warning', 'alert', 'out of spec'], // OutOfSpec = 2 
        ['status-red', 'thumbs-down', 'officially out of spec']
    ], // OfficialOutOfSpec = 3

    redRenderer: function(val, isRed) {
        return isRed ? ('<strong style="color: red;">' + val + '<strong>') : val;
    },

    //GRID configuration

    getScheduleGridCfg: function(view) {
        var me = view;
        var gridCfg = {
            name: 'ScheduleGrid',
            show: {
                header: false,
                footer: true,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            menu: [
                { id: 1, text: 'Schedule Report' },
                { id: 2, text: 'Client Report' },
                { id: 3, text: '3rd Party Provider Report' },
                { id: 4, text: 'Schedule Settings' },
                { id: 5, text: 'Rerun Tracking' }
        ],

            columns: [
                { field: 'Name', caption: 'Schedule', size: '15%', sortable: true, searchable: true },
                { field: 'Advertiser', caption: 'Advertiser', size: '8%', sortable: true },
                { field: 'Estimate', caption: 'Estimate', sortable: true, searchable: true, size: '8%' },
                {
                    field: 'StartDate',
                    caption: 'Start Date',
                    sortable: true,
                    size: '8%',
                    render: function(record, index, column_index) {
                        return moment(record.StartDate).format("M/D/YYYY");
                    }
                },
                {
                    field: 'EndDate',
                    caption: 'End Date',
                    sortable: true,
                    size: '8%',
                    render: function(record, index, column_index) {
                        return moment(record.EndDate).format("M/D/YYYY");
                    }
                },
                { field: 'SpotsBooked', caption: 'Spots Booked', size: '8%', sortable: true },
                { field: 'SpotsDelivered', caption: 'Spots Delivered', size: '8%', sortable: true },
                { field: 'OutOfSpec', caption: 'Out of Spec', size: '8%', sortable: true },
               // { field: 'PostingBook', caption: 'Posting Book', size: '8%', sortable: true },
                {
                     field: 'PostingBookDate',
                     caption: 'Posting Book',
                     sortable: true,
                     render: function (record) {
                         return record.PostingBook;
                     },
                     size: '8%'
                 },

                {
                    field: 'PrimaryDemoBooked',
                    caption: 'Primary Demo Booked Imp. (000)',
                    hidden: false,
                    render: function (record) {
                        return record.PrimaryDemoBooked ? numeral(util.divideImpressions(record.PrimaryDemoBooked)).format('0,0.[000]') : '-';
                    },
                    size: '10%',
                    sortable: true
                },
                {
                    field: 'PrimaryDemoDelivered',
                    caption: 'Primary Demo Delivered Imp. (000)',
                    hidden: false,
                    render: function (record) {
                        return record.PrimaryDemoDelivered ? numeral(util.divideImpressions(record.PrimaryDemoDelivered)).format('0,0.[000]') : '-';
                    },
                    size: '10%',
                    sortable: true
                }
            ],

            sortData: [{ field: 'recid', direction: 'asc' }]
        };

        //get copy so original object not mutated
        //return this.copyCfgItem(gridCfg);
        return gridCfg;
    },

    getScrubGridCfg: function (view) {
        var me = this;

        var gridCfg = {
            name: 'ScrubGrid',
            show: {
                header: false,
                footer: true,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            multiSelect: true,
            columns: [
                {
                    field: 'Status',
                    caption: '',
                    sortable: true,
                    attr: 'align=center',
                    size: '60px',
                    render: function (record, index, column_index) {
                        var stat = record.Status;
                        var display = '';

                        if (stat) {
                            var states = me.bvsStatusStates[stat - 1];
                            display = '<span title="' + states[2] + '" style="font-size: 14px;" id="bvs_status_item_' + record.recid + '" class="' + states[0] + ' glyphicon glyphicon-' + states[1] + '" aria-hidden="true"></span>';

                            if (record.HasLeadInScheduleMatches && !record.LinkedToLeadin) {
                                display += ' <span title="timeslot mismatch" style="font-size: 14px; display: inline;" id="bvs_status_item_' + record.recid + '" class="status-red fa fa-clock-o" aria-hidden="true"></span>';
                            }
                        }

                        return display;
                    }
                },
                {
                    field: 'RunTime',
                    caption: 'Time Aired',
                    sortable: true,
                    size: '10%',
                    render: function (record, index, column_index) {
                        var display = moment(record.RunTime).format("M/D/YYYY h:mm:ss A");
                        return me.redRenderer(display, !record.MatchAirtime);
                    }
                },
                {
                    field: 'SpotLength',
                    caption: 'Ad Length',
                    size: '5%',
                    sortable: true,
                    render: function (record, index, column_index) {
                        return me.redRenderer(record.SpotLength, !record.MatchSpotLength);
                    }
                },
                {
                    field: 'Isci',
                    caption: 'ISCI',
                    sortable: true,
                    size: '10%',
                    render: function (record, index, column_index) {
                        return me.redRenderer(record.Isci, !record.MatchIsci);
                    }
                },
                {
                    field: 'Program',
                    caption: 'Program',
                    sortable: true,
                    size: '10%',
                    render: function (record, index, column_index) {
                        var programIssue = !record.MatchProgram && !record.LinkedToBlock && !record.LinkedToLeadin;
                        return me.redRenderer(record.Program, programIssue);
                    }
                },
                {
                    field: 'Affiliate',
                    caption: 'Affiliate',
                    sortable: true,
                    size: '5%'
                },
                {
                    field: 'Market',
                    caption: 'Market',
                    sortable: true,
                    size: '5%',
                    render: function (record, index, column_index) {
                        //return me.redRenderer(record.Market, record.MarketAlerted);
                        return record.Market;
                    }
                },
                {
                    field: 'Station',
                    caption: 'Station',
                    sortable: true,
                    size: '5%',
                    render: function (record, index, column_index) {
                        return me.redRenderer(record.Station, !record.MatchStation);
                    }
                },
                {
                    field: 'Cost',
                    caption: 'Cost',
                    size: '10%',
                    sortable: true,
                    render: function (record, index, column_index) {
                        // allow zero, if null will be dash -- test for null to dash -- if (index == 3) record.Cost = null;
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cost, true);
                    }
                },
                {
                    field: 'Impressions',
                    caption: 'Impressions (000)',
                    render: function (record) {
                        return record.Impressions ? numeral(util.divideImpressions(record.Impressions)).format('0,0.[000]') : '-';
                    },
                    size: '10%',
                    sortable: true
                },
                {
                    field: 'Loading',
                    caption: '',
                    sortable: true,
                    size: '60px',
                    render: function (record, index, column_index) {
                        return record.isUpdating ? '<span class="text-loading animated-background" style="width: 40px;"><span></span></span>' : '';
                    }
                }
            ],

            sortData: [{ field: 'recid', direction: 'asc' }],

            onDblClick: function (event) {
                view.onDoubleClick(event);
            },

            onContextMenu: function (event) {
                view.onContextMenu(event);
            },

            onMenuClick: function (event) {
                view.onMenuClick(event);
            }
        };

        //get copy so original object not mutated
        //return this.copyCfgItem(gridCfg);
        return gridCfg;
    },

    //config for manage mappings grid - either type: "Station" or "Program"
    getMappingsGridCfg: function (view, type) {
        var me = view;
        var gridCfg = {
            name: 'Mappings' + type + 'Grid',
            header: type + ' Mappings',
            show: {
                header: true,
                //lineNumbers: true,
                footer: true,
                toolbar: true,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: true
            },
            menu: [{ id: 1, text: 'Delete' }],

            columns: [
                { field: 'ScheduleValue', caption: 'Schedule Value', size: '50%', sortable: true, searchable: true},
                { field: 'BvsValue', caption: 'Bvs Value', size: '50%', sortable: true, searchable: true }
            ],

            sortData: [{ field: 'ScheduleValue', direction: 'asc' }]
        };

        //get copy so original object not mutated
        //return this.copyCfgItem(gridCfg);
        return gridCfg;
    },

    getRatingsBooksGridCfg: function (view) {
        var me = this;
        var gridCfg = {
            name: 'RatingsBooksGrid',
            show: {
                header: false,
                footer: false,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },

            menu: [
                { id: 1, text: 'Delete' }
            ],

            //change overall height to fit input fields, select
            recordHeight: 32,

            columns: [
                {
                    field: 'MediaMonthDisplay',
                    caption: 'Posting Book',
                    sortable: false,
                    size: '15%',
                    render: function (record, index, column_index) {
                        if (record.addNew === true) {
                            return '<div style="width: 200px;"><select id="add_book_select"></select></div>';
                        }
                        return (record.MediaMonthDisplay) ? '<div>' + record.MediaMonthDisplay + '</div>' : '<div></div>';

                    }
                },
                
                {
                    field: 'AnnualAdjustment',
                    caption: 'Year on Year Loss',
                    sortable: false,
                    size: '15%',
                    //test as built in editable
                    //render: 'percent:2',
                    render: function (record, index, column_index) {
                        //for add new button - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "book_loss_" + record.recid;
                        //var yearLoss = record.AnnualAdjustment ? record.AnnualAdjustment.toFixed(2) + '%' : '';
                        //use getCellValue if not updating manually
                        var yearLoss = this.getCellValue(index, column_index);
                        if (yearLoss) yearLoss = yearLoss.toFixed(2) + '%';
                        //may not need store id if no validation etc
                        var cell = '<div bookRecidEdit="' + record.recid + '" id="' + cellId + '" class="flex-container-1 editable-cell">' + yearLoss + '</div>';
                        return cell;
                    },

                    editable: { type: 'percent', autoFormat: true, precision: 2, min: -100, max: 100 }
                },
                {
                    field: 'NtiAdjustment',
                    caption: 'NTI/NSI Conversion',
                    sortable: false,
                    size: '15%',
                    render: function (record, index, column_index) {
                        //for add new button - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "book_conversion_" + record.recid;
                        //var conversion = record.NtiAdjustment ? record.NtiAdjustment.toFixed(2) + '%' : '';
                        //use getCellValue if not updating manually
                        var conversion = this.getCellValue(index, column_index);
                        if (conversion) conversion = conversion.toFixed(2) + '%';
                        //may not need store id if no validation etc
                        var cell = '<div bookRecidEdit="' + record.recid + '" id="' + cellId + '" class="flex-container-1 editable-cell">' + conversion + '</div>';
                        //var cell = "<div bookRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        //cell += "<input name=\"book_conversion_input_" + record.recid + "\" type='text' class='edit-input' style='display: none !important'/>";
                        //cell += '<div>' + conversion + '</div>';
                        //cell += "</div>";
                        return cell;
                    },

                    editable: { type: 'percent', autoFormat: true, precision: 2, min: -100, max: 100 }
                }
            ]
        };

        return gridCfg;
    },

    getBvsFileListingGridCfg: function (view) {
        var self = view;
        var gridCfg = {
            name: 'BvsFileListing',
            show: {
                header: false,
                footer: false,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            menu: [{ id: 1, text: 'Delete' }],
            columns: [
                { field: 'FileName', caption: 'File Name', size: '25%', sortable: true, searchable: true },
                {
                    field: 'StartDate',
                    caption: 'Start Date',
                    size: '5%',
                    sortable: true,
                    searchable: true,
                    render: function (record, index, column_index) {
                        return TrackerConfig.formatDate(record.StartDate)
                    }
                },
                {
                    field: 'EndDate',
                    caption: 'End Date',
                    size: '5%',
                    sortable: true,
                    searchable: true,
                    render: function (record, index, column_index) {
                        return TrackerConfig.formatDate(record.EndDate)
                    }
                },
                {
                    field: 'UploadDate',
                    caption: 'Upload Date',
                    size: '5%',
                    sortable: true,
                    searchable: true,
                    render: function (record, index, column_index) {
                        return TrackerConfig.formatDate(record.UploadDate)
                    }
                },
                { field: 'RecordCount', caption: 'Record Count', size: '5%', sortable: true, searchable: true },
            ],
            sortData: [{ field: 'UploadDate', direction: 'desc' }]
        };

        return gridCfg;
    }
};