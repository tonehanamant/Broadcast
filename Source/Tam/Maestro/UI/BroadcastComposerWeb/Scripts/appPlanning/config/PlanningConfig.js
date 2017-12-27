var PlanningConfig = {

    /*** HELPERS ***/

    greyRenderer: function (val, isGrey) {
        var cls = isGrey ? 'status-gray' : '';
        return '<div class="' + cls + '">' + val + '</div>';
    },

    formatDate: function (dateString) {
        if (!dateString)
            return "";

        dateString = dateString.replace(/-/g, '\/').replace(/T.+/, '');
        return moment(new Date(dateString)).format('MM/DD/YY');
    },

    impressionRenderer: function (val) {
        return val ? val.toFixed(3) : '-';
    },

    //text, selected; active, dashed, IsContinuation
    //use prepared data in the record - order below of checks determines handling overall
    contractRenderer: function (rec, isBack) {
        if (!rec.active) return '-';
        var contract = isBack ? rec.back : rec.front;
        //just show dash?
        if (rec.isHiatus) return contract.name ? contract.name : '-';
        if (!contract.active) return '-';
        
        if (contract.available) return '<strong>Available</strong>';
        if (contract.selected) {
            var html = '<span class="label label-success"><span class="glyphicon glyphicon-ok" aria-hidden="true"></span></span> <strong style="color: green;">Selected</strong>';
            //if selected, name and changed - show tip
            //change: do not show tip if currentProposal
            if (contract.isChanged && contract.name && !contract.isCurrentProposal) {
                var mrk = '<span class="glyphicon glyphicon-info-sign" style="" aria-hidden="false"></span>';
                html += '<span data-container="body" data-html="true" data-toggle="tooltip" title="' + contract.name + '">&nbsp;' + mrk + '</span>';
            }
            return html;
        }

        //show name or continuation - use inventory_display_text for elipsis tip check - see view
        var nameDisplay = (isBack && contract.isContinuation) ? ('<span class="label label-default">30s contd...</span> ' + contract.name) : contract.name;
        //return '<div title="' + nameDisplay + '">' + nameDisplay + '</div>';
        return '<div class="inventory_display_text">' + nameDisplay + '</div>';
    },

    /*** PROPOSAL GRID CONFIG ***/

    getProposalsGridCfg: function (view) {
        var gridCfg = {
            name: 'ProposalsGridCfg',
            show: {
                header: false,
                footer: true,
                toolbar: true,
                toolbarReload: false,
                toolbarColumns: true,
                toolbarSearch: false,
                toolbarInput: false
            },
            //header: 'Proposals',
            columns: [
                { field: 'Id', caption: 'Id', sortable: true, size: '5%' },
                { field: 'ProposalName', caption: 'Name', sortable: true, size: '20%' },
                { field: 'Advertiser', caption: 'Advertiser', sortable: true, size: '20%' },
                { field: 'DisplayStatus', caption: 'Status', sortable: true, size: '10%' },
                {
                    field: 'FlightStartDate',
                    caption: 'Flight',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        var $scope = this;

                        var hiatusWeeks = [];
                        var html = '<span>' + PlanningConfig.formatDate(record.FlightStartDate) + " - " + PlanningConfig.formatDate(record.FlightEndDate) + '</span>';
                        var tooltipText = '<div><span>Hiatus Weeks</span><br>';

                        $.each(record.Flights, function (idx, week) {
                            if (week.IsHiatus == true) {
                                var hiatusWeekFormated = PlanningConfig.formatDate(week.StartDate) + ' - ' + PlanningConfig.formatDate(week.EndDate);
                                tooltipText += '<div>' + hiatusWeekFormated + '</div>';
                                hiatusWeeks.push(hiatusWeekFormated);
                            }
                        });

                        if (hiatusWeeks.length > 0) {
                            tooltipText += '</div>';
                            var mrk = '<span class="glyphicon glyphicon-info-sign" style="" aria-hidden="false"></span>';
                            html = '<span class="proposal-hiatus span-block" data-container="body" data-html="true" data-toggle="tooltip" title="' + tooltipText + '">' + html + '&nbsp;' + mrk + '</span>';
                        }

                        return html;
                    }
                },
                { field: 'Owner', caption: 'Owner', sortable: true, size: '15%' },
                {
                    field: 'LastModified',
                    caption: 'Last Modified',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return PlanningConfig.formatDate(record.LastModified)
                    }
                }
            ],

            sortData: [{ field: 'Id', direction: 'desc' }],

            /*** Events ***/

            onRefresh: function (event) {
                event.onComplete = function () {
                    $('body').tooltip({
                        selector: '[data-toggle="tooltip"]'
                    });
                }
            },

            onDblClick: function (event) {
                var proposal = this.get(event.recid);
                view.controller.editProposal(proposal.Id);
            }
        };

        return gridCfg;
    },

    //for multiple sets of detail grids - copy; nested structure
    getProposalDetailGridCfg: function (id, set) {
        var gridCfg = {
            name: 'ProposalDetailGrid_' + id,
            //multiSelect: false, //seting false makes the grid scroll to top on selections if many records
            fixedBody: false, //tbd
            columns: [
                {
                    field: 'Week',
                    caption: 'Week',
                    sortable: false,
                    size: '20%',
                    //Quarter or Week
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) return record.displayText;
                        if (record.isQuarter) {
                            return '<strong>' + (record.QuarterText ? record.QuarterText : '') + '</strong></div>';
                            //return '<div style="position: absolute; z-index: 2000; background-color: #eee; top: 0; right: 0; left: 0; height: 24px;">Stephans DIV | CPM...</div>';
                        } else {
                            return record.IsHiatus ? ('<span class="status-gray">' + record.Week + '</span></div>') : record.Week + '</div>';
                        }
                        
                    }
                },
                {
                    field: 'Units',
                    caption: 'Units',
                    sortable: false,
                    size: '20%',
                    //Quarter (CPM as Units); Week (Units)
                    render: function (record, index, column_index) {
                        //allow for comma 1000s
                        if (record.w2ui && record.w2ui.summary) return record.TotalUnits ? numeral(record.TotalUnits).format('0,0') : '-';
                        var id = 'editable_units_' + record.recid;
                        //editable fields
                        if (record.isQuarter) {
                            //use Units as Cpm
                            //change only show decimals if present but with alway 2 decimals ($5.70); use commas
                            var formattedCpm = numeral(record.Units).format('$0,0[.]00');
                            //return '<div class="editable-cell" id="' + id + '"><span class="grid-label">CPM ' + '</span>' + w2uiConfig.renderers.toMoneyOrDash(record.Units, false) + '</div>';
                            return '<div class="editable-cell" id="' + id + '"><span class="grid-label">CPM ' + '</span>' + formattedCpm + '</div>';
                        } else {
                            //allow for comma 1000s
                            var formatUnits = numeral(record.Units).format('0,0');
                            return '<div class="editable-cell" id="' + id + '">' + formatUnits + '</div>';
                        }
                    },
                    //default: will change based on quarter or week
                    //editable: { type: 'int', min: 1}
                    editable: { type: 'money', min: 0.01, autoFormat: false, prefix: 'CPM $' }
                },
                {
                    field: 'Impressions',
                    caption: 'Imp (000)',
                    sortable: false,
                    size: '20%',
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) {
                            //allow for comma 1000s; only show decimal if not 000
                            return record.TotalImpressions ? numeral(util.divideImpressions(record.TotalImpressions)).format('0,0.[000]'): '-';
                        }

                        var id = 'editable_impressions_' + record.recid;
                        //var roundedImpressions = Math.round(Number(record.Impressions));
                        //no longer rounding; allow for comma 1000s; only show decimal if not 000
                        var convertedImpressions = numeral(util.divideImpressions(record.Impressions)).format('0,0.[000]');
                        if (record.isQuarter) {
                            return '<div class="editable-cell" id="' + id + '"><span class="grid-label">Imp. Goal (000) ' + '</span>' + convertedImpressions + '</div>';
                        } else {
                            return '<div class="editable-cell" id="' + id + '">' + convertedImpressions + '</div>';
                        }
                    },
                    editable: { type: 'float', precision: 3, prefix: 'IMP Goal </br>'  }
                },
                {
                    field: 'Cost',
                    caption: 'Cost',
                    sortable: false,
                    size: '40%',
                    //Cost
                    render: function (record, index, column_index) {
                        //only show decimal if not 000
                        if (record.w2ui && record.w2ui.summary) return w2uiConfig.renderers.toMoneyOrDash(record.TotalCost, false);
                        if (record.isQuarter) {
                            return '';
                        } else {
                            return w2uiConfig.renderers.toMoneyOrDash(record.Cost, false);
                        }
                    }
                }
            ]
        };
        //return copy
        return util.copyData(gridCfg, null, null, true);
    },

    getSwitchProposalVersionGridCfg: function (view) {
        var gridCfg = {
            name: 'SwitchProposalVersionGridCfg',
            multiSelect: false,
            columns: [
                { field: 'Version', caption: 'Version', sortable: true, size: '15%' },
                { field: 'DisplayStatus', caption: 'Status', sortable: true, size: '40%' },
                { field: 'Advertiser', caption: 'Advertiser', sortable: true, size: '30%' },
                {
                    field: 'Flight',
                    caption: 'Flight',
                    sortable: true,
                    size: '30%',
                    render: function (record, index, column_index) {
                        return PlanningConfig.formatDate(record.StartDate) + " - " + PlanningConfig.formatDate(record.EndDate);
                    }
                },

                { field: 'GuaranteedAudience', caption: 'Guaranteed Demo', sortable: true, size: '30%' },
                { field: 'Owner', caption: 'Owner', sortable: true, size: '30%' },
                {
                    field: 'DateModified',
                    caption: 'Date Modified',
                    sortable: true,
                    size: '30%',
                    render: function (record, index, column_index) {
                        return PlanningConfig.formatDate(record.DateModified)
                    }
                },
                { field: 'Notes', caption: 'Notes', sortable: true, size: '30%' }
            ],

            /*** Events ***/

            onDblClick: function (event) {
                var item = this.get(event.recid);
                view.openVersion(item.Version);
            }
        };

        return gridCfg;
    },

    //for multiple weeksinventory grids - copy; provide columnGroups
    //columns variable as well as the column group spans
    //spans do not work proper if try setting non shown column hidden - change to get only needed columns
    //try to simplify column sizes - set non fariable to fixed px and contracts to percentages
    getInventoryWeekGridCfg: function (id, group, isFrontBack, view) {
        var span = isFrontBack ? 6 : 5;
        var gridCfg = {
            name: group ? ('InventoryWeekHeaderGrid_' + id) : ('InventoryWeekGrid_' + id),
            //multiSelect: false,
            fixedBody: false,
            columnGroups: group ? [{ caption: group, span: span }] : [],
            show: { columnHeaders: group ? true : false },
            onSelect: function (event) {
                //to prevent selection (styling on grids)
                event.preventDefault();
            }
        };

        var columns = isFrontBack ? [
                {
                    field: 'ContractFront',
                    caption: 'Front Spot',
                    sortable: false,
                    resizable: false,
                    size: '50%',
                    //tbd selectable editable cell: <div class="editable-cell" id="' + id + 
                    render: function (record, index, column_index) {
                        //if (record.w2ui && record.w2ui.summary) return record.displayText;
                        if (record.isGroup) {
                            return '';
                        } else {
                            //tbd
                            // return record.ContractFront;
                            return PlanningConfig.contractRenderer(record, false);
                        }
                    }
                },
                {
                    field: 'ContractBack',
                    caption: 'Back Spot',
                    sortable: false,
                    resizable: false,
                    size: '50%',
                    //tbd selectable editable cell: <div class="editable-cell" id="' + id + 
                    render: function (record, index, column_index) {
                        //if (record.w2ui && record.w2ui.summary) return record.displayText;
                        if (record.isGroup) {
                            return '';
                        } else {
                            //tbd
                            //return record.ContractBack;
                            return PlanningConfig.contractRenderer(record, true);
                        }
                    }
                }] :
                [{
                    field: 'Contract',
                    caption: 'Contract',
                    sortable: false,
                    resizable: false,
                    size: '100%',
                    //tbd selectable editable cell: <div class="editable-cell" id="' + id + 
                    render: function (record, index, column_index) {
                        //if (record.w2ui && record.w2ui.summary) return record.displayText;
                        if (record.isGroup) {
                            return '';
                        } else {
                            return PlanningConfig.contractRenderer(record, false);
                        }
                    }
                }];

        gridCfg.columns = columns.concat(
            [{
                field: 'Impressions',
                caption: 'Imp (000)',
                sortable: false,
                resizable: false,
                size: '100px',
                render: function (record, index, column_index) {
                    if (record.isGroup) {
                        return '';
                    } else {
                        if (!record.active) return '-';

                        
                        var formattedImpresions = numeral(util.divideImpressions(record.Impressions)).format('0,0.[000]');

                        if (record.HasWastage) {
                            return '<div>' + formattedImpresions + ' <span class="glyphicon glyphicon-info-sign" title="wastage"></span></div>';
                        } else {
                            return '<div>' + formattedImpresions + '</div>';
                        }
                    }
                }
            },
            {
                field: 'CPM',
                caption: 'CPM',
                sortable: false,
                resizable: false,
                size: '80px',
                //Cost
                render: function (record, index, column_index) {
                    //if (record.w2ui && record.w2ui.summary) return w2uiConfig.renderers.toMoneyOrDash(record.TotalCost, false);
                    if (record.isGroup) {
                        return '';
                    } else {
                        if (!record.active) return '-';
                        //todo show green if Selected?
                        return numeral(record.CPM).format('$0,0[.]00');
                    }
                }
            },
            {
                field: 'Cost',
                caption: 'Cost',
                sortable: false,
                resizable: false,
                size: '120px',
                render: function (record, index, column_index) {
                    //if (record.w2ui && record.w2ui.summary) return w2uiConfig.renderers.toMoneyOrDash(record.TotalCost, false);
                    if (record.isGroup) {
                        return '';
                    } else {
                        if (!record.active) return '-';
                        //todo show green if Selected?
                        return numeral(record.Cost).format('$0,0[.]00');
                    }
                }
            }]);
        //return copy
        return util.copyData(gridCfg, null, null, true);
    },

    getInventoryDaypartsGridCfg: function (view, isHeader) {
        var gridCfg = {
            name: isHeader ? 'InventoryDaypartsHeaderGrid' : 'InventoryDaypartsGrid',
            //multiSelect: false,
            fixedBody: false,
            show: { columnHeaders: isHeader ? true : false },
            columns: [
                {
                    field: 'daypartDisplay',
                    caption: 'Daypart',
                    sortable: false,
                    resizable: false,
                    size: '100%',
                    render: function (record, index, column_index) {
                        //if (record.w2ui && record.w2ui.summary) return record.displayText;
                        return record.daypartDisplay;
                    }
                }
            ],
            onSelect: function (event) {
                //to prevent selection (styling on grids)
                event.preventDefault();
            }
        };

        return gridCfg;
    },

    
    //OPEN MARKETS

    //get open market grid cfg (or columns) - sets dynamic week column sets based on  weeksLength
    getOpenMarketGridCfg: function (view, weeksLength) {
        var gridCfg = {
            name: 'OpemMarketGrid',
            show: { footer: true },
            onSelect: function (event) {
                //to prevent selection (styling on grids)
                event.preventDefault();
            }
        };
        //combined columns change - arrays for Dayparts, ProgramNames - Friends/Seinfeld | 8a-9p/11-12p
        var columns = [
                {
                    field: 'ProgramName',
                    caption: 'Program | Airing Time',
                    resizable: false,
                    frozen: true,
                    sortable: false,
                    size: '480px',
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) return '';
                        if (record.isMarket) {
                            //use numeral?
                            return '<strong>' + record.MarketRank + '.  ' + record.MarketName + '</strong> (' + w2utils.formatNumber(record.MarketSubscribers.toFixed(0)) + ')';
                        } else if (record.isStation) {
                            return '<strong>' + record.StationName + ' (' + record.Affiliation + ')</strong>';
                        } else {
                            //return (record.Daypart && record.Daypart.Display) ? record.Daypart.Display : ' - ';
                            //ProgramNames, can be empty like [null] OR [name, null, name]
                            // var programDisplay = (record.ProgramNames.length && record.ProgramNames[0]) ? record.ProgramNames.join('/') : ' - ';
                            var programDisplay = ' - ';
                            if (record.ProgramNames.length) {
                                var resPrograms = '';
                                var lastProgram = record.ProgramNames.length - 1;
                                $.each(record.ProgramNames, function (idx, program) {
                                    if (program) {
                                        resPrograms += program;
                                        if (idx < lastProgram) resPrograms += '/';
                                    }
                                });
                                programDisplay = resPrograms;
                            }

                            var airtimeDisplay = ' - ';
                            if (record.Dayparts.length) {
                                var resAir = '';
                                var lastAir = record.Dayparts.length - 1;
                                $.each(record.Dayparts, function (idx, air) {
                                    resAir += air.Display;
                                    if (idx < lastAir) resAir += '/';
                                });
                                airtimeDisplay = resAir;
                            }
                                
                            return programDisplay + ' | ' + airtimeDisplay;
                        }
                    }
                },
				//should be empty for market/station
                /*
                {
                    field: 'ProgramName',
                    caption: 'Program',
                    frozen: true,
                    resizable: false,
                    sortable: false,
                    size: '200px'
                },
                */
                {
                    field: 'TargetCpm',
                    //TBD? toggle between w2ui-sort-up / w2ui-sort-down
                    //caption: 'CPM <div class="sort_indicator" id="TargetCpm_sort"></div>',
                    caption: 'CPM',
                    frozen: true,
                    sortable: false,
                    resizable: false,
                    size: '80px',
                    render: function (record, index, column_index) {
                        if (record) {

                            var val = record.TargetCpm ? numeral(record.TargetCpm).format('$0,0[.]00') : '-';
                            if (record.isProgram) {
                                return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                            } else {
                                return '';
                            }
                        }
                    }
                }
        ];

        var me = this;
        for (var i = 0; i < weeksLength; i++) {
            columns = columns.concat(me.getOpenMarketsWeekColumnsCfg(i));
        }

        gridCfg.columns = columns;

        return gridCfg;
    },
    
    //dynamic week  column sets
    getOpenMarketsWeekColumnsCfg: function (weekIdx) {
        var getWeekData = function (record) { return record['week' + weekIdx] };
        var columns = [
                {
                    field: 'week' + weekIdx,
                    caption: 'Spots',
                    sortable: false,
                    resizable: false,
                    size: '200px',
                    render: function (record, index, column_index) {
                        if (record.isStation) return '';
                        var week = getWeekData(record);
                        var spot = week.Spots ? week.Spots : '-';
                        if (record.isProgram) {
                            //now need to deal with greying here
                            if (!week.active) return PlanningConfig.greyRenderer('Unavailable', true);
                            if (week.isHiatus) return PlanningConfig.greyRenderer(spot, true);
                            var cellId = 'program_week_spot_' + record.recid + '_' + weekIdx;
                            var changedCls = week.isChanged ? 'w2ui-changed' : '';
                            var cell = '<div id="' + cellId + '" data-weekidx="' + weekIdx + '" data-recid="' + record.recid + '" class="flex-container-1 editable-cell program_week_spot_click ' + changedCls + '">';
                            cell += '<input type="number" class="edit-input" step="1" style="display: none !important" />';
                            cell += '<div>' + spot + '</div>';
                            cell += '<div style="color: #bbbaba" class="glyphicon glyphicon-edit" aria-hidden="true"></div>';
                            cell += "</div>";
                            //this is not dry
                            if (!week.TotalImpressions || week.TotalImpressions === 0 || week.TotalImpression === '0') {
                                spot = PlanningConfig.greyRenderer('Unavailable', true);
                                var cell = '<div id="' + cellId + '" data-weekidx="' + weekIdx + '" data-recid="' + record.recid + '" class="flex-container-1' + changedCls + '">';
                                cell += '<div>' + spot + '</div>';
                                cell += "</div>";
                            }
                            return cell;
                        } else {
                            return spot;
                        }
                    }
                },
                {
                    field: 'week' + weekIdx,
                    caption: 'Imp (000)',
                    sortable: false,
                    resizable: false,
                    size: '180px',
                    render: function (record, index, column_index) {
                       
                        //format of impressions and dash
                        if (record.isStation) return '';
                        var week = getWeekData(record);
                        if (record.isProgram) {//display val including 0 unless not active
                            if (!week.active) return PlanningConfig.greyRenderer('-', true);
                            //TODO hiatus needs set here
                            var val = numeral(util.divideImpressions(week.TotalImpressions)).format('0,0.[000]');
                            //return grey for 0 or hiatus
                            var grey = week.Spots === 0 || week.isHiatus;
                            return PlanningConfig.greyRenderer(val, grey);
                        } else {//market display val or dash
                            return week.Impressions ? numeral(util.divideImpressions(week.Impressions)).format('0,0.[000]') : '-';
                        }

                    }
                },

                {
                    field: 'week' + weekIdx,
                    caption: 'Cost',
                    sortable: false,
                    resizable: false,
                    size: '180px',
                    render: function (record, index, column_index) {
                        if (record.isStation) return '';
                        var week = getWeekData(record);
                        if (record.isProgram) {//display val including 0 unless not active
                            if (!week.active) return PlanningConfig.greyRenderer('-', true);
                            var val = week.Cost ? numeral(week.Cost).format('$0,0[.]00') : '-';
                            //return grey for 0 or hiatus
                            var grey = week.Spots === 0 || week.isHiatus;
                            return PlanningConfig.greyRenderer(val, grey);
                        } else {//market display val or dash
                            return numeral(week.Cost).format('$0,0[.]00');
                        }
                    }
                }

        ];

        //return copy
        return util.copyData(columns, null, null, true);
    },
};