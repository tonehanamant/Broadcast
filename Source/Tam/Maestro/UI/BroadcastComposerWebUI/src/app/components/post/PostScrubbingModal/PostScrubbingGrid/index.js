import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Grid } from 'react-redux-grid';
import Sorter from 'Utils/react-redux-grid-sorter';

// import CustomPager from 'Components/shared/CustomPager';
import { getDateInFormat, getDay } from '../../../../utils/dateFormatter';

/* eslint-disable */
export class PostScrubbingGrid extends Component {
    constructor(props, context) {
        super(props, context);
        this.context = context;
    }

   /*  shouldComponentUpdate(nextProps) {
        // return nextProps.ClientScrubs !== this.props.ClientScrubs;
    } */

    componentDidUpdate(prevProps) {
       if (prevProps.ClientScrubs !== this.props.ClientScrubs) {
           /*  this.props.setOverlayLoading({
                id: 'gridPostScrubbingGrid',
                loading: true,
            }); */
          // evaluate column sort direction
        setTimeout(() => {
            const cols = this.props.grid.get('gridPostScrubbingGrid').get('columns');
            let sortCol = cols.find(x => x.sortDirection);
            if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);

            if (sortCol) {
                const datasource = this.props.dataSource.get('gridPostScrubbingGrid');
                const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);

                this.props.doLocalSort({
                    data: sorted,
                    stateKey: 'gridPostScrubbingGrid',
                });
            }

          /*   this.props.setOverlayLoading({
                id: 'gridPostScrubbingGrid',
                loading: false,
            }); */
        }, 0);
        }
    }

    /* ////////////////////////////////// */
    /* // GRID ACTION METHOD BINDINGS
    /* ////////////////////////////////// */

    selectRow(ref) {
        this.props.selectRow(ref);
    }

    deselectAll(ref) {
        this.props.deselectAll(ref);
    }

    render() {
        const style = { color: '#FF0000' };
        const stateKey = 'gridPostScrubbingGrid';
        const { activeScrubbingData } = this.props;
        //const { Details = [] } = activeScrubbingData;
        const { ClientScrubs = [] } = activeScrubbingData;

        // let clientScrubs = [];

       /*  Details.forEach(details => {
            details.ClientScrubs.forEach((item) => {
                clientScrubs.push(item);
            });
        }); */

        const columns = [
            {
                name: 'Week Start',
                dataIndex: 'WeekStart',
                width: '8%',
                renderer: ({ row }) => {
                    const weekStart = <span>{(row.WeekStart && getDateInFormat(row.WeekStart)) || '-'}</span>
                    return (
                        weekStart
                    )
                },
            },
            {
                name: 'Date',
                dataIndex: 'TimeAired',
                width: '8%',
                renderer: ({ row }) => {
                    const date = <span>{getDateInFormat(row.TimeAired) || '-'}</span>
                    return (
                        date
                    )
                },
            },
            {
                name: 'Time Aired',
                dataIndex: 'MatchTime',
                width: '10%',
                renderer: ({ row }) => {
                    const TimeAired = row.MatchTime ? <span>{getDateInFormat(row.TimeAired, false, true) || '-'}</span> : <span style={style}>{getDateInFormat(row.TimeAired, false, true) || '-'}</span>
                    return (
                        TimeAired
                    )
                },
            },
            {
                name: 'Day',
                dataIndex: 'DayOfWeek',
                width: '8%',
                renderer: ({ row }) => {
                    const DayOfWeek = <span>{getDay(row.DayOfWeek) || '-'}</span>
                    return (
                        DayOfWeek
                    )
                },
            },
            {
                name: 'Ad Length',
                dataIndex: 'SpotLength',
                width: '4%',
                renderer: ({ row }) => (
                    <span>{row.SpotLength || '-'}</span>
                ),
            },
            {
                name: 'ISCI',
                dataIndex: 'ISCI',
                defaultSortDirection: 'ASC',
                width: '10%',
                renderer: ({ row }) => {
                    const ISCI = row.MatchISCI ? <span>{row.ISCI || '-'}</span> : <span style={style}>{row.ISCI || '-'}</span>
                    return (
                        ISCI
                    )
                }
            },
            {
                name: 'Program',
                dataIndex: 'ProgramName',
                width: '12%',
                renderer: ({ row }) => {
                    const programName = row.MatchProgram ? <span>{row.ProgramName || '-'}</span> : <span style={style}>{row.ProgramName || '-'}</span>
                    return (
                        programName
                    )
                },
            },
            {
                name: 'Genre',
                dataIndex: 'GenreName',
                width: '10%',
                renderer: ({ row }) => {
                    const GenreName = row.MatchGenre ? <span>{row.GenreName || '-'}</span> : <span style={style}>{row.GenreName || '-'}</span>
                    return (
                        GenreName
                    )
                },
            },
            {
                name: 'Affiliate',
                dataIndex: 'Affiliate',
                width: '10%',
                renderer: ({ row }) => (
                    <span>{row.Affiliate || '-'}</span>
                ),
            },
            {
                name: 'Market',
                dataIndex: 'Market',
                width: '10%',
                renderer: ({ row }) => {
                    const Market = row.MatchMarket ? <span>{row.Market || '-'}</span> : <span style={style}>{row.Market || '-'}</span>
                    return (
                        Market
                    )
                }
            },
            {
                name: 'Station',
                dataIndex: 'Station',
                width: '10%',
                renderer: ({ row }) => {
                    const Station = row.MatchStation ? <span>{row.Station || '-'}</span> : <span style={style}>{row.Station || '-'}</span>
                    return (
                        Station
                    )
                },
            },
        ];

        const plugins = {
            COLUMN_MANAGER: {
                resizable: false,
                moveable: false,
                sortable: {
                    enabled: true,
                    method: 'local',
                },
            },
            LOADER: {
                enabled: false,
            },
            SELECTION_MODEL: {
                mode: 'single',
                enabled: true,
                allowDeselect: true,
                activeCls: 'active',
                selectionEvent: 'singleclick',
            },
        };

        const events = {
            HANDLE_BEFORE_SORT: () => {
              this.deselectAll({ stateKey });
            },
        };

        const grid = {
            columns,
            plugins,
            events,
            stateKey,
        };

        return (
            <Grid {...grid} data={ClientScrubs} store={this.context.store} height={380}/>
        );
    }
}

PostScrubbingGrid.PropTypes = {
    grid: PropTypes.object.isRequired,
    dataSource: PropTypes.object.isRequired,
    // ClientScrubs: PropTypes.object.isRequired,
    activeScrubbingData: PropTypes.object.isRequired,
    // setOverlayLoading: PropTypes.func.isRequired,
    selectRow: PropTypes.func.isRequired,
    deselectAll: PropTypes.func.isRequired,
    doLocalSort: PropTypes.func.isRequired,
};

export default PostScrubbingGrid;
