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

    getSwitchProposalVersionGridCfg: function (view) {
        var gridCfg = {
            name: 'SwitchProposalVersionGridCfg',
            multiSelect: false,
            columns: [
                { field: 'Version', caption: 'Version', sortable: true, size: '15%' },
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

    getProgramsGridCfg: function (view) {
        var gridCfg = {
            name: 'ProgramsGrid',
            multiSelect: false,
            show: {
                header: false,
                footer: false,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false,
                selectColumn: false
            },

            columns: [
                {
                    field: 'DayPart',
                    caption: 'Airing Time',
                    sortable: false,
                    size: '14%',
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) return '';
                        if (record.isMarket) {
                            return '<strong>' + record.Rank + '.  ' + record.MarketName + '</strong> (' + w2utils.formatNumber(record.MarketSubscribers.toFixed(0)) + ')';
                        } else if (record.isStation) {
                            return '<strong>' + record.Station.LegacyCallLetters + ' (' + record.Station.Affiliation + ')</strong>';
                        } else {
                            return (record.DayPart && record.DayPart.Display) ? record.DayPart.Display : ' - ';
                        }
                    }
                },
				//should be empty for market/station
                {
                    field: 'ProgramName',
                    caption: 'Program',
                    sortable: false,
                    size: '10%'
                },
                {
                    field: 'FlightWeeks',
                    caption: 'Program Flight',
                    sortable: true,
                    size: '14%',
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) return '';
                        var $scope = this;
                        var hiatusWeeks = [];
                        var html = '<span>' + PlanningConfig.formatDate(record.StartDate) + " - " + PlanningConfig.formatDate(record.EndDate) + '</span>';
                        var tooltipText = '<div><span>Hiatus Weeks</span><br>';

                        $.each(record.FlightWeeks, function (idx, week) {
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
				//should be empty for market/station; add text for summary
                {
                    field: 'Genres',
                    caption: 'Genre',
                    sortable: false,
                    size: '12%',
                    //will be array of Genres  - Id, Display
                    render: function (record, index, column_index) {
                        if (record.w2ui && record.w2ui.summary) {
                            return record.displayText;

                        }
                        if (record.Genres && record.Genres.length) {
                            var out = [];
                            $.each(record.Genres, function (index, value) {
                                out.push(value.Display);
                            });
                            return out.join(', ');
                        }
                        return '';
                    }
                },
                {
                    field: 'TotalSpots',
                    caption: 'Total Spots',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        if (record.isProgram) {
                            var cellId = 'program_spot_' + record.recid;
                            var cell = '<div id="' + cellId + '" class="flex-container-1 editable-cell">';
                            cell += '<input type="number" class="edit-input" step="1" style="display: none !important" />';
                            cell += '<div>' + record.TotalSpots + '</div>';
                            cell += "</div>";
                            return cell;
                        } else {
                            return record.TotalSpots;
                        }
                    }
                },
                {
                    field: 'TotalCost',
                    caption: 'Total Cost',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = config.renderers.toMoneyOrDash(record.TotalCost, true);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'TargetImpressions',
                    caption: 'Target Imp. (000)',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = record.TargetImpressions.toFixed(1);

                        if (record.isProgram) {
                            if (record.IsOverlapping) {
                                val = val + '  <span class="glyphicon glyphicon-info-sign" title="overlapping"></span>';
                            }

                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'TargetCpm',
                    //toggle between w2ui-sort-up / w2ui-sort-down
                    caption: 'Target Cpm <div class="sort_indicator" id="TargetCpm_sort"></div>',
                    sortable: true,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = config.renderers.toMoneyOrDash(record.TargetCpm, true);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'TRP',
                    caption: 'TRP',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = record.TRP.toFixed(2);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'HHeCPM',
                    //toggle between w2ui-sort-up / w2ui-sort-down
                    caption: 'HH Cpm <div class="sort_indicator" id="HHeCPM_sort"></div>',
                    sortable: true,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = config.renderers.toMoneyOrDash(record.HHeCPM, true);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'HHImpressions',
                    caption: 'HH Imp. (000)',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = record.HHImpressions.toFixed(1);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'GRP',
                    caption: 'GRP',
                    sortable: false,
                    size: '8%',
                    render: function (record, index, column_index) {
                        var val = record.GRP.toFixed(2);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'AdditionalAudienceImpressions',
                    caption: '<span class="additional_audience_name"></span> imp (000)',
                    sortable: false,
                    size: '18%',
                    hidden: true,
                    render: function (record, index, column_index) {
                        var val = record.AdditionalAudienceImpressions.toFixed(1);
                        if (record.isProgram) {
                            return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                        } else {
                            return val;
                        }
                    }
                },
                {
                    field: 'AdditonalAudienceCPM',
                    caption: '<span class="additional_audience_name"></span> CPM<div class="sort_indicator" id="AdditonalAudienceCPM_sort"></div>',
                    sortable: true,
                    size: '18%',
                    hidden: true,
                    render: function (record, index, column_index) {
                        if (record) {
                            var val = config.renderers.toMoneyOrDash(record.AdditonalAudienceCPM, true);
                            if (record.isProgram) {
                                return PlanningConfig.greyRenderer(val, record.TotalSpots === 0);
                            } else {
                                return val;
                            }
                        }
                    }
                }
            ],

            /*** Events ***/

            onRefresh: function (event) {
                event.onComplete = function () {
                    $('body').tooltip({
                        selector: '[data-toggle="tooltip"]'
                    });
                }
            }
        };

        return gridCfg;
    }
};