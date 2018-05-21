import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Grid } from 'react-redux-grid';
// import Sorter from 'Utils/react-redux-grid-sorter';

// import CustomPager from 'Components/shared/CustomPager';
import { getDateInFormat, getSecondsToTimeString, getDay } from '../../../../utils/dateFormatter';

import './index.scss';

export class PostScrubbingGrid extends Component {
    constructor(props, context) {
      super(props, context);
      this.context = context;
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
        // const { Details = [] } = activeScrubbingData;
        const { ClientScrubs = [] } = activeScrubbingData;

        // let clientScrubs = [];

       /*  Details.forEach(details => {
            details.ClientScrubs.forEach((item) => {
                clientScrubs.push(item);
            });
        }); */

        const columns = [
            {
                name: 'Status',
                dataIndex: 'Status',
                // width: '3%',
                // test specific width
                width: 44,
                renderer: ({ row }) => {
                    const iconClassName = (row.Status === 2) ? 'fa-check-circle' : 'fa-times-circle';
                    return (
                        <i className={`status-icon fa ${iconClassName}`} />
                    );
                },
            },
            {
                name: 'Week Start',
                dataIndex: 'WeekStart',
                width: 100,
                renderer: ({ row }) => {
                    const weekStart = <span>{(row.WeekStart && getDateInFormat(row.WeekStart)) || '-'}</span>;
                    return (
                        weekStart
                    );
                },
            },
            {
                name: 'Date',
                dataIndex: 'DateAired',
                width: 100,
                renderer: ({ row }) => {
                    const date = row.MatchDate ? <span>{getDateInFormat(row.DateAired) || '-'}</span> : <span style={style}>{getDateInFormat(row.DateAired) || '-'}</span>;
                    return (
                        date
                    );
                },
            },
            {
                name: 'Time Aired',
                dataIndex: 'TimeAired',
                width: 100,
                renderer: ({ row }) => {
                    const TimeAired = row.MatchTime ? <span>{getSecondsToTimeString(row.TimeAired) || '-'}</span> : <span style={style}>{getSecondsToTimeString(row.TimeAired) || '-'}</span>;
                    return (
                        TimeAired
                    );
                },
            },
            {
                name: 'Day',
                dataIndex: 'DayOfWeek',
                width: 80,
                renderer: ({ row }) => {
                    const DayOfWeek = row.MatchIsciDays ? <span>{getDay(row.DayOfWeek) || '-'}</span> : <span style={style}>{getDay(row.DayOfWeek) || '-'}</span>;
                    // const DayOfWeek = <span>{getDay(row.DayOfWeek) || '-'}</span>
                    return (
                        DayOfWeek
                    );
                },
            },
            {
                name: 'Spot Length',
                dataIndex: 'SpotLength',
                width: 80,
                renderer: ({ row }) => (
                    <span>{row.SpotLength || '-'}</span>
                ),
            },
            {
                name: 'House ISCI',
                dataIndex: 'ISCI',
                // defaultSortDirection: 'ASC',
                width: 150,
                renderer: ({ row }) => (<span>{row.ISCI || '-'}</span>),
            },
            {
              name: 'Client ISCI',
              dataIndex: 'ClientISCI',
              // defaultSortDirection: 'ASC',
              width: 150,
              renderer: ({ row }) => (<span>{row.ClientISCI || '-'}</span>),
            },
            {
                name: 'Program',
                dataIndex: 'ProgramName',
                width: 150,
                renderer: ({ row }) => {
                    const programName = row.MatchProgram ? <span>{row.ProgramName || '-'}</span> : <span style={style}>{row.ProgramName || '-'}</span>;
                    return (
                        programName
                    );
                },
            },
            {
                name: 'Genre',
                dataIndex: 'GenreName',
                width: 100,
                renderer: ({ row }) => {
                    const GenreName = row.MatchGenre ? <span>{row.GenreName || '-'}</span> : <span style={style}>{row.GenreName || '-'}</span>;
                    return (
                        GenreName
                    );
                },
            },
            {
                name: 'Show Type',
                dataIndex: 'ShowType',
                width: 100,
                renderer: ({ row: { ShowTypeName, MatchShowType } }) => {
                    const showTypeRow = <span style={MatchShowType ? {} : style}>{ShowTypeName || '-'}</span>;
                    return (
                        showTypeRow
                    );
                },
            },
            {
                name: 'Affiliate',
                dataIndex: 'Affiliate',
                width: 60,
                renderer: ({ row }) => (
                    <span>{row.Affiliate || '-'}</span>
                ),
            },
            {
                name: 'Market',
                dataIndex: 'Market',
                width: 150,
                renderer: ({ row }) => {
                    const Market = row.MatchMarket ? <span>{row.Market || '-'}</span> : <span style={style}>{row.Market || '-'}</span>;
                    return (
                        Market
                    );
                },
            },
            {
                name: 'Station',
                dataIndex: 'Station',
                width: 60,
                renderer: ({ row }) => {
                    const Station = row.MatchStation ? <span>{row.Station || '-'}</span> : <span style={style}>{row.Station || '-'}</span>;
                    return (
                        Station
                    );
                },
            },
            {
              name: 'Comments',
              dataIndex: 'Comments',
              // width: 150,
              width: '100%',
              renderer: ({ row }) => (
                  <span>{row.Comments || '-'}</span>
              ),
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
            <Grid {...grid} data={ClientScrubs} store={this.context.store} height={340} />
        );
    }
}

PostScrubbingGrid.propTypes = {
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
