import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Grid, Actions } from 'react-redux-grid';
import Sorter from 'Utils/react-redux-grid-sorter';

import { setOverlayLoading } from 'Ducks/app';
import CustomPager from 'Components/shared/CustomPager';
import { getDateInFormat, getDay } from '../../../../utils/dateFormatter';

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({ post: { proposalDetail: { ClientScrubs } }, grid, dataSource, menu }) => ({
    ClientScrubs,
    grid,
    dataSource,
    menu,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
    {
      showMenu,
      hideMenu,
      selectRow,
      deselectAll,
      doLocalSort,
      setOverlayLoading,
    }, dispatch)
);

/* eslint-disable */
export class PostScrubbingGrid extends Component {
    constructor(props, context) {
        super(props, context);
        this.context = context;
    }

    shouldComponentUpdate(nextProps) {
        return nextProps.ClientScrubs !== this.props.ClientScrubs;
    }

    componentDidUpdate(prevProps) {
        if (prevProps.ClientScrubs !== this.props.ClientScrubs) {
            this.props.setOverlayLoading({
                id: 'gridPostScrubbingGrid',
                loading: true,
            });
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

            this.props.setOverlayLoading({
                id: 'gridPostScrubbingGrid',
                loading: false,
            });
        }, 0);

        // Hide Context Menu (assumes visible)
          this.props.hideMenu({ stateKey: 'gridPostScrubbingGrid' });
        }
    }

    /* ////////////////////////////////// */
    /* // GRID ACTION METHOD BINDINGS
    /* ////////////////////////////////// */
    hideContextMenu(ref) {
        this.props.hideMenu(ref);
    }

    showContextMenu(ref) {
        this.props.showMenu(ref);
    }

    selectRow(ref) {
        this.props.selectRow(ref);
    }

    deselectAll(ref) {
        this.props.deselectAll(ref);
    }

    render() {
        const style = { color: '#FF0000' };
        const stateKey = 'gridPostScrubbingGrid';
        const columns = [
            {
                name: 'Week Start',
                dataIndex: 'WeekStart',
                width: '8%',
                renderer: ({ row }) => {
                    const weekStart = <span>{getDateInFormat(row.WeekStart) || '-'}</span>
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
                renderer: ({ row }) => (
                    <span>{row.ISCI || '-'}</span>
                ),
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
                    const Market = row.MatchMarket ? <span>{Market || '-'}</span> : <span style={style}>{Market || '-'}</span>
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
                resizable: true,
                moveable: false,
                sortable: {
                    enabled: true,
                    method: 'local',
                },
            },
            EDITOR: {
                type: 'inline',
                enabled: false,
            },
            PAGER: {
                enabled: false,
                pagingType: 'local',
                pagerComponent: (
                    <CustomPager stateKey={stateKey} idProperty="Id" />
                ),
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
              this.hideContextMenu({ stateKey });
            },
        };

        const grid = {
            columns,
            plugins,
            events,
            stateKey,
        };

        return (
            <Grid {...grid} data={this.props.ClientScrubs} store={this.context.store} />
        );
    }
}

PostScrubbingGrid.PropTypes = {
    grid: PropTypes.object.isRequired,
    dataSource: PropTypes.object.isRequired,
    menu: PropTypes.object.isRequired,
    ClientScrubs: PropTypes.object.isRequired,

    setOverlayLoading: PropTypes.func.isRequired,

    showMenu: PropTypes.func.isRequired,
    hideMenu: PropTypes.func.isRequired,
    selectRow: PropTypes.func.isRequired,
    deselectAll: PropTypes.func.isRequired,
    doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostScrubbingGrid);
