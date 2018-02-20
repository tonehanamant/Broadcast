import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Grid, Actions } from 'react-redux-grid';
import Sorter from 'Utils/react-redux-grid-sorter';

import { setOverlayLoading } from 'Ducks/app';
import CustomPager from 'Components/shared/CustomPager';

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({ post: { post }, grid, dataSource, menu }) => ({
    post,
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
export class PostScrubbingDetail extends Component {
    constructor(props, context) {
        super(props, context);
        this.context = context;
        this.state = {
            activeDateThingy: '',
        };
    }

    componentDidUpdate(prevProps) {
        if (prevProps.post !== this.props.post) {
            this.props.setOverlayLoading({
                id: 'gridPostMain',
                loading: true,
            });
          // evaluate column sort direction
        setTimeout(() => {
            const cols = this.props.grid.get('gridPostScrubbingDetail').get('columns');
            let sortCol = cols.find(x => x.sortDirection);
            if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);

            if (sortCol) {
                const datasource = this.props.dataSource.get('gridPostScrubbingDetail');
                const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);

                this.props.doLocalSort({
                    data: sorted,
                    stateKey: 'gridPostScrubbingDetail',
                });
            }

            this.props.setOverlayLoading({
                id: 'gridPostScrubbingDetail',
                loading: false,
            });
        }, 0);

        // Hide Context Menu (assumes visible)
          this.props.hideMenu({ stateKey: 'gridPostScrubbingDetail' });
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
        const stateKey = 'gridPostScrubbingDetail';
        const columns = [
            {
                name: 'Time Aired',
                dataIndex: 'AiredTime',
                width: '20%',
            },
            {
                name: 'Ad Length',
                dataIndex: 'AdLength',
                width: '10%',
            },
            {
                name: 'ISCI',
                dataIndex: 'Isci',
                defaultSortDirection: 'ASC',
                width: '15%',
            },
            {
                name: 'Program',
                dataIndex: 'Program',
                width: '15%',
            },
            {
                name: 'Genre',
                dataIndex: 'Genre',
                width: '10%',
            },
            {
                name: 'Affiliate',
                dataIndex: 'Affiliate',
                width: '10%',
            },
            {
                name: 'Market',
                dataIndex: 'Market',
                width: '10%',
            },
            {
                name: 'Station',
                dataIndex: 'Station',
                width: '10%',
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
            <Grid {...grid} data={this.props.postSrubbingDetail} store={this.context.store} />
        );
    }
}

PostScrubbingDetail.defaultProps = {
    postSrubbingDetail: [
        {
          "AdLength": 30,
          "Program": "Goldie Mejia",
          "Genre": "News",
          "Isci": "INTERLOO",
          "AiredTime": "08/15/2015 03:44:49",
          "Affiliate": "MNTV",
          "Market": "Maimi",
          "Station": "KSWB"
        },
        {
          "AdLength": 30,
          "Program": "Carrillo Horton",
          "Genre": "Reality TV",
          "Isci": "ELITA",
          "AiredTime": "08/18/2017 12:22:23",
          "Affiliate": "MNTV",
          "Market": "Las Vegas",
          "Station": "WT TV"
        },
        {
          "AdLength": 30,
          "Program": "Shari Heath",
          "Genre": "Reality TV",
          "Isci": "SOLGAN",
          "AiredTime": "06/25/2014 11:50:35",
          "Affiliate": "MNTV",
          "Market": "Las Vegas",
          "Station": "KSWB"
        },
        {
          "AdLength": 30,
          "Program": "Mae Tucker",
          "Genre": "Reality TV",
          "Isci": "COMFIRM",
          "AiredTime": "04/27/2014 12:12:51",
          "Affiliate": "FOX",
          "Market": "San Diago",
          "Station": "WT TV"
        },
        {
          "AdLength": 30,
          "Program": "Francisca Macias",
          "Genre": "Reality TV",
          "Isci": "MICRONAUT",
          "AiredTime": "12/01/2017 08:14:56",
          "Affiliate": "MNTV",
          "Market": "Las Vegas",
          "Station": "WT TV"
        }
    ],
};

PostScrubbingDetail.PropTypes = {
    grid: PropTypes.object.isRequired,
    dataSource: PropTypes.object.isRequired,
    menu: PropTypes.object.isRequired,
    postSrubbingDetail: PropTypes.array.isRequired,

    setOverlayLoading: PropTypes.func.isRequired,

    showMenu: PropTypes.func.isRequired,
    hideMenu: PropTypes.func.isRequired,
    selectRow: PropTypes.func.isRequired,
    deselectAll: PropTypes.func.isRequired,
    doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostScrubbingDetail);
