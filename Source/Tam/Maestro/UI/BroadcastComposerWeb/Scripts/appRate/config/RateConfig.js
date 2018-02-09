var RateConfig = {

    //TBD - for a normal configuration object or below for specific
    //set util property deep to false to avoid merging arrays
    getCfg: function (name, obj) {
        var item = this[name];
        if (item) return util.copyData(item, obj, null, true);
        return null;
    },

    //copy a object deep and optionally add additional properties to
    copyCfgItem: function (item, xtra) {
        return util.copyData(item, xtra, null, true);
    },

    //cancel/save icon - only show if editing
    contactActionsRenderFunction: function (record, index, column_index) {
        //for add new button in Type - no display here
        if (record.addNew === true) {
            return null;
        }
        var id = record.recid;
        var cell = '<div id="contact_actions_' + id + '" class="contact-edit-actions" style="display: none;">';
        //tbd possibly for an add
        //if (!record.creatingCopy) {
            cell += '<a  data-recid="' + id + '" name="save" title="Save edit Contact" style="padding: 2px 4px 0 2px; color: green;" class="btn btn-link"><span class="glyphicon glyphicon-check" aria-hidden="true"></span></a>';
        //}
        cell += '<a  data-recid="' + id + '" name="cancel" title="Cancel edit Contact" style="padding: 2px 0 0 4px;  color: red;" class="btn btn-link"><span class="glyphicon glyphicon-remove" aria-hidden="true"></span></a>';
        cell += '</div>'

        return cell;
    },

    /*** GRID configuration ***/
    
    //Stations grid configuration on Main Rates page;
    getStationsGridCfg: function (view) {
        var gridCfg = {
            name: 'StationsGrid',
            show: {
                header: false,
                footer: true,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },

            columns: [
                { field: 'LegacyCallLetters', caption: 'Station', size: '20%', sortable: true, searchable: true },
                { field: 'Affiliation', caption: 'Affiliate', size: '20%', searchable: true, sortable: true },
                { field: 'OriginMarket', caption: 'Market', sortable: true, searchable: true, size: '20%' },
                {
                    field: 'RateDataThrough',
                    caption: 'Rate Data Through',
                    sortable: true,
                    size: '20%',

                    render: function (record, index, column_index) {
                        var hiatusWeeks = [];

                        var html = '<div style="display:inline-block;">' + record.RateDataThrough + '</div>';
                        var tooltipText = '<div><span>Hiatus Weeks</span> <br>';
                        $.each(record.FlightWeeks, function (idx, week) {
                            if (week.IsHiatus == true) {
                                var hiatusWeekFormated = moment(week.StartDate).format("MM/DD/YYYY") + " - " + moment(week.EndDate).format("MM/DD/YYYY");
                                hiatusWeeks.push(hiatusWeekFormated);
                                tooltipText += '<div>' + hiatusWeekFormated + '</div>';
                            }
                        });
                        tooltipText += '</div>';

                        if (hiatusWeeks.length > 0) {
                            var mrk = '<div style="display:inline-block;"><span class="glyphicon glyphicon-info-sign" aria-hidden="false"></span></div>';
                            html = '<div class="span-block" data-container="body" data-html="true" data-toggle="tooltip" title="' + tooltipText + '">' + html + '&nbsp;' + mrk + '</div>';
                        }
                        return html;
                    }
                },
                {
                    field: 'ModifiedDate',
                    caption: 'Last Update',
                    sortable: true,
                    size: '20%',
                    render: function (record, index, column_index) {
                        return moment(record.ModifiedDate).format("MM/DD/YYYY h:mm:ss a");
                    }
                }
            ],

            sortData: [{ field: 'LegacyCallLetters', direction: 'asc' }],
            onRefresh: function (event) {
                //is required for filtering grids to reset
                event.onComplete = function () {
                    $('[data-toggle="tooltip"]').tooltip({
                        container: 'body',
                    });
                }
            }
        };
        

        //get copy so original object not mutated (if reuse only)
        //return this.copyCfgItem(gridCfg);
        return gridCfg;
    },

    getContactsGridCfg: function (view) {
        var me = this;
        var gridCfg = {
            name: 'ContactsGrid',
            show: {
                header: false,
                footer: false,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },

            columns: [
                {
                    field: 'Type',
                    caption: 'Type',
                    sortable: true,
                    searchable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        if (record.addNew === true) {
                            //return "<div>New Copy</div>";
                            return '<div class="flex-container-1"><button id="contacts_grid_add_new_btn" class="btn btn-xs btn-link">Add Contact <span class="glyphicon glyphicon-plus" aria-hidden="true"></span></button></div>';
                        }
                        var cellId = "contact_type_" + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<select name=\"contact_type_select_" + record.recid + "\" class=\"edit-input\" style='display: none !important'>";
                            cell += "<option value=\"1\">Station</option>";
                            cell += "<option value=\"2\">REP</option>";
                            cell += "<option value=\"3\">Traffic</option>";
                        cell += "</select>";
                        cell += record.Type ? '<div>' + record.Type + '</div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                
                {
                    field: 'Name',
                    caption: 'Name',
                    sortable: true,
                    searchable: true,
                    size: '30%',
                    render: function (record, index, column_index) {
                        //for add new button in Type - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "contact_search_" + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<select name=\"contact_name_input_" + record.recid + "\" type='text' class='edit-input form-control select2' style='display: none !important'></select>";
                        cell += record.Name ? '<div>' + record.Name + '</div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                
                //{
                //    field: 'Name',
                //    caption: 'Name',
                //    sortable: true,
                //    searchable: true,
                //    size: '15%',
                //    render: function (record, index, column_index) {
                //        //for add new button in Type - no display here
                //        if (record.addNew === true) {
                //            return null;
                //        }
                //        var cellId = "contact_name_" + record.recid;
                //        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                //        cell += "<input name=\"contact_name_input_" + record.recid + "\" type='text' class='edit-input' style='display: none !important'/>";
                //        cell += record.Name ? '<div>' + record.Name + '</div>' : '<div></div>';
                //        cell += "</div>";
                //        return cell;
                //    }
                //},
                {
                    field: 'Company',
                    caption: 'Company',
                    sortable: true,
                    searchable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        //for add new button in Type - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "contact_company_" + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input name=\"contact_company_input_" + record.recid + "\" type='text' class='edit-input' style='display: none !important'/>";
                        cell += record.Company ? '<div>' + record.Company + '</div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                {
                    field: 'Email',
                    caption: 'Email',
                    sortable: true,
                    searchable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        //for add new button in Type - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = 'contact_email_' + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input name=\"contact_email_input_" + record.recid + "\" type='email' class='edit-input' style='display: none !important'/>";
                        cell += record.Email ? '<div><a href="mailto:' + record.Email + '">' + record.Email + '</a></div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                {
                    field: 'Phone',
                    caption: 'Phone',
                    sortable: true,
                    searchable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        //for add new button in Type - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "contact_phone_" + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input name=\"contact_phone_input_" + record.recid + "\" type='text' class='edit-input' style='display: none !important'/>";
                        cell += record.Phone ? '<div>' + record.Phone + '</div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                {
                    field: 'Fax',
                    caption: 'Fax',
                    sortable: true,
                    searchable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        //for add new button in Type - no display here
                        if (record.addNew === true) {
                            return null;
                        }
                        var cellId = "contact_fax_" + record.recid;
                        var cell = "<div contactRecidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input name=\"contact_fax_input_" + record.recid + "\" type='text' class='edit-input' style='display: none !important'/>";
                        cell += record.Fax ? '<div>' + record.Fax + '</div>' : '<div></div>';
                        cell += "</div>";
                        return cell;
                    }
                },
                {
                    field: 'actions',
                    caption: 'Actions',
                    searchable: false,
                    hidden: false,
                    size: '65px',
                    sortable: false,
                    render: me.contactActionsRenderFunction
                }
            ],

            onContextMenu: function (event) {
                view.onRatesContactContextMenu(event);
            },

            onMenuClick: function (event) {
                view.onRatesContactMenuClick(event);
            }
        };

        return gridCfg;
    },

    // RATES

    getRatesGridCfg: function (view) {
        var me = this;
        var gridCfg = {
            name: 'RatesGrid',
            show: {
                header: false,
                footer: false,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            menu: [
                //{ id: 1, text: 'Delete Schedule' }
            ],
            columns: [
                {
                    field: 'AirtimePreview',
                    caption: 'Air Time',
                    sortable: true,
                    //attr: 'align=center',
                    size: '15%'
                },
                {
                    field: 'ProgramName',
                    caption: 'Program',
                    //searchable: true,
                    sortable: true,
                    size: '15%'
                },
                {
                    field: 'Genre',
                    caption: 'Genre',
                    sortable: true,
                    size: '15%',
                    //will be array of Genres  - Id, Display
                    render: function (record, index, column_index) {
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
                    field: 'SpotsPerWeek',
                    caption: 'Spots/Week',
                    sortable: true,
                    hidden:true,
                    size: '7%',
                    //tbd
                   // render: function (record, index, column_index) {
                       // return w2uiConfig.renderers.toMoneyOrDash(record.Rate30, true);
                    //}
                },
                {
                    field: 'Rate30',
                    caption: '30 Rate',
                    sortable: true,
                    size: '7%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Rate30, true);
                    }
                },
                {
                    field: 'Rate15',
                    caption: '15 Rate',
                    sortable: true,
                    size: '7%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Rate15, true);
                    }
                },
                {
                    field: 'HouseHoldImpressions',
                    caption: 'HH Impressions (000)',
                    size: '10%',
                    sortable: true,
                    render: function (record) {
                        return record.HouseHoldImpressions ? numeral(util.divideImpressions(record.HouseHoldImpressions)).format('0,0.[000]') : '-';
                    }
                },
                {
                    field: 'Rating',
                    caption: 'Rating',
                    style: 'text-align: left',
                    render: 'float:2',
                    size: '10%',
                    sortable: true
                },
                {
                    field: 'EffectiveDate',
                    caption: 'Flight',
                    size: '15%',
                    sortable: true,
                    //remove tooltip; change from FLight to StartDate handle start and end only if present
                    render: function (record, index, column_index) {
                        var start = moment(record.EffectiveDate).format("MM/DD/YYYY");
                        var end = record.EndDate ? (' - ' + moment(record.EndDate).format("MM/DD/YYYY")) : '';
                        //var hiatusWeeks = [];
                        //var html = '<span>' + record.Flight + '</span>';
                        //var tooltipText = '<div><span>Hiatus Weeks</span> <br>';
                        //$.each(record.Flights, function (idx, week) {
                        //    if (week.IsHiatus == true) {
                        //        var hiatusWeekFormated = moment(week.StartDate).format("MM/DD/YYYY") + ' - ' + moment(week.EndDate).format("MM/DD/YYYY");
                        //        hiatusWeeks.push(hiatusWeekFormated);
                        //        tooltipText  += '<div>' + hiatusWeekFormated + '</div>';
                        //    }
                        //});

                        //if (hiatusWeeks.length > 0) {
                        //    tooltipText += '</div>';
                        //    var mrk = '<span class="glyphicon glyphicon-info-sign" style="" aria-hidden="false"></span>';
                        //    html = '<span class="span-block" data-container="body" data-html="true" data-toggle="tooltip" title="' + tooltipText + '">' + html + '&nbsp;' + mrk + '</span>';
                        //}
                        //try changing the record so can use in edit station form
                        record.Flight = start + end;
                        return start + end;
                    }
                }
            ],

            sortData: [{ field: 'ProgramName', direction: 'asc' }, { field: 'EffectiveDate', direction: 'asc' }],

            onDblClick: function (event) {
                view.onRatesDoubleClick(event);
            },

            onContextMenu: function (event) {
                view.onRatesContextMenu(event);
            },

            onMenuClick: function (event) {
                view.onRatesMenuClick(event);
            }

        };

        //get copy so original object not mutated
        //return this.copyCfgItem(gridCfg);
        return gridCfg;
    },

    getProgramConflictGridCfg: function () {
        var me = this;
        var gridCfg = {
            name: 'ProgramConflictGrid',
            fixedBody: true,
            show: {
                header: true,
                footer: true,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            header: 'Conflicts',
            columns: [
                {
                    field: 'Conflict',
                    caption: 'Conflict',
                    sortable: true,
                    attr: 'align=center',
                    size: '10%',
                    render: function (record, index, column_index) {
                        return record.hasConflict ? '<span class="glyphicon glyphicon-exclamation-sign" style="color: red; font-size: 14px;" aria-hidden="false"></span>' : '<span class="glyphicon glyphicon-ok" style="color: green; font-size: 14px;" aria-hidden="false"></span>';
                    }
                },
                {
                    field: 'AirtimePreview',
                    caption: 'Air Time',
                    sortable: true,
                    size: '30%'
                },
                {
                    field: 'ProgramName',
                    caption: 'Program',
                    sortable: true,
                    size: '30%'
                },             
                {
                    field: 'Flight',
                    caption: 'Flight',
                    size: '30%',
                    sortable: true,
                    render: function (record, index, column_index) {
                        //adjust - no FLight in data
                        var start = moment(record.EffectiveDate).format("MM/DD/YYYY");
                        var end = record.EndDate ? (' - ' + moment(record.EndDate).format("MM/DD/YYYY")) : '';
                        var flight = start + end;
                        var cellId = "flight-" + record.recid;
                        var cell = "<div recidEdit='" + record.recid + "' id='" + cellId + "'class='flex-container-1 editable-cell'>";
                        cell += "<input type='text' class='edit-input' style='display: none !important' />"
                        cell += '<i class="calendar-icon glyphicon glyphicon-calendar fa fa-calendar"></i>'
                        cell += record.isEdited ? "<div class='text-warning'>" + flight + '</div>' : "<div>" + flight + '</div>';
                        cell += "</div>";
                        return cell;
                    }
                }
            ],
            //prevent hover on selection
            onSelect: function (e) {
                e.preventDefault();
            },

            sortData: [{ field: 'Program', direction: 'asc' }],
        };

        return gridCfg;
    },

    getDemoThirdPartyGridCfg: function (view) {
        var me = this;
        var gridCfg = {
            name: 'DemoGrid',
            show: {
                header: true,
                footer: true,
                toolbar: false,
                toolbarReload: false,
                toolbarColumns: false,
                toolbarSearch: false
            },
            header: 'Demo CPM',
            columns: [
                {
                    field: 'Target',
                    caption: 'Target',
                    sortable: true,
                    //attr: 'align=center',
                    size: '25%'
                },
                {
                    field: 'Cpm30',
                    caption: ':30 CPM',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cpm30, false);
                    }
                },
                {
                    field: 'Cpm15',
                    caption: ':15 CPM',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cpm15, false);
                    }
                },
                {
                    field: 'Cpm60',
                    caption: ':60 CPM',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cpm60, false);
                    }
                },
                {
                    field: 'Cpm90',
                    caption: ':90 CPM',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cpm90, false);
                    }
                },
                {
                    field: 'Cpm120',
                    caption: ':120 CPM',
                    sortable: true,
                    size: '15%',
                    render: function (record, index, column_index) {
                        return w2uiConfig.renderers.toMoneyOrDash(record.Cpm120, false);
                    }
                }

            ]
            //??
            //sortData: [{ field: 'Target', direction: 'asc' }]

        };

        return gridCfg;
    }
};