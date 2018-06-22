import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Grid, Actions } from 'react-redux-grid';
// import Sorter from 'Utils/react-redux-grid-sorter';
import { overrideStatus } from 'Ducks/post';
import SwapDetailModal from './SwapDetailModal';
// import CustomPager from 'Components/shared/CustomPager';
import { getDateInFormat, getSecondsToTimeString, getDay } from '../../../../utils/dateFormatter';

import './index.scss';

const { MenuActions } = Actions;
const { showMenu, hideMenu } = MenuActions;

const mapStateToProps = ({ grid, selection, dataSource, menu }) => ({
  // Grid
  grid,
  selection,
  dataSource,
  menu,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
  {
    showMenu,
    hideMenu,
    overrideStatus,
  }, dispatch)
);

export class PostScrubbingGrid extends Component {
    constructor(props, context) {
      super(props, context);
      this.context = context;
      this.getScrubbingSelections = this.getScrubbingSelections.bind(this);
      this.processManualOverrides = this.processManualOverrides.bind(this);
      this.hideContextMenu = this.hideContextMenu.bind(this);
      this.showContextMenu = this.showContextMenu.bind(this);
      this.openSwapDetailModal = this.openSwapDetailModal.bind(this);
    }

    componentDidUpdate(prevProps) {
      if (prevProps.activeScrubbingData !== this.props.activeScrubbingData) {
        // Hide Context Menu (assumes visible)
        this.props.hideMenu({ stateKey: 'PostScrubbingGrid' });
      }
    }

   /*  shouldComponentUpdate(nextProps) {
        // return nextProps.ClientScrubs !== this.props.ClientScrubs;
    } */

    getScrubbingSelections() {
      const stateKey = 'PostScrubbingGrid';
     // const selections = this.props.selection.toJS()[stateKey];
      const selectedIds = this.props.selection.get(stateKey).get('indexes');
       // const selectedIds = selectMap.get('indexes');
      const rowData = this.props.dataSource.get(stateKey).toJSON(); // currentRecords or data - array
      // const list = this.props.dataSource.get(stateKey);
      // const activeSelections = rowData.data.filter(item => selectedIds.indexOf(item._id) > -1);
      const activeSelections = [];

      selectedIds.forEach((idx) => {
        activeSelections.push(rowData.data[idx]);
      });

      // console.log('getScrubbing Selections', activeSelections, selectedIds, rowData);

      return activeSelections;
    }
    // tbd call api with params - slections to Ids, filterKey, etc
    processManualOverrides(overrideType, selections) {
      const activeFilterKey = this.props.activeScrubbingData.filterKey;
      const proposalId = this.props.activeScrubbingData.Id;
      const selectionIds = selections.map(({ ScrubbingClientId }) => ScrubbingClientId);
      const params = { ProposalId: proposalId, ScrubIds: selectionIds, ReturnStatusFilter: activeFilterKey, OverrideStatus: overrideType };
      // console.log('manual overrides', params);
      this.props.overrideStatus(params);
    }

    hideContextMenu(ref) {
      this.props.hideMenu(ref);
    }
    showContextMenu(ref) {
      this.props.showMenu(ref);
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

    openSwapDetailModal(selections) {
      this.props.toggleModal({
        modal: 'swapDetailModal',
        active: true,
        properties: { selections },
      });
    }

    render() {
        const style = { color: '#FF0000' };
        const stateKey = 'PostScrubbingGrid';
        const { activeScrubbingData, details } = this.props;
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
                width: 59,
                renderer: ({ row }) => {
                  const override = row.StatusOverride;
                  let iconClassName = '';
                  if (override) {
                    iconClassName = (row.Status === 2) ? 'fa-check-circle-o' : 'fa-times-circle-o';
                  } else {
                    iconClassName = (row.Status === 2) ? 'fa-check-circle' : 'fa-times-circle';
                  }
                  return (
                      <i className={`status-icon fa ${iconClassName}`} />
                  );
                },
            },
            {
              name: 'Detail ID',
              dataIndex: 'Sequence',
              width: 75,
              renderer: ({ row }) => (
                <span>{row.Sequence || '-'}</span>
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
              width: 75,
              renderer: ({ row }) => {
                  const Station = row.MatchStation ? <span>{row.Station || '-'}</span> : <span style={style}>{row.Station || '-'}</span>;
                  return (
                      Station
                  );
              },
            },
            {
              name: 'Affiliate',
              dataIndex: 'Affiliate',
              width: 75,
              renderer: ({ row }) => (
                  <span>{row.Affiliate || '-'}</span>
              ),
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
              dataIndex: 'ShowTypeName',
              width: 100,
              renderer: ({ row: { ShowTypeName, MatchShowType } }) => {
                  const showTypeRow = <span style={MatchShowType ? {} : style}>{ShowTypeName || '-'}</span>;
                  return (
                      showTypeRow
                  );
              },
            },
            {
                name: 'Spot Length',
                dataIndex: 'SpotLength',
                width: 95,
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
                mode: 'multi',
                enabled: true,
                allowDeselect: true,
                activeCls: 'active',
                selectionEvent: 'singleclick',
            },
            GRID_ACTIONS: {
              iconCls: 'action-icon',
              /* onMenuShow: ({ columns, rowData }) => {
                console.log('This event fires before menushow', columns, rowData);
                if (rowData.isDisabled) {
                    return ['menu-item-key']; // this field will now be disabled
                }
                // this works but does not actually show disabled
                // return ['menu-post-override-out'];
                return [];
              }, */
              menu: [
                {
                  text: 'Override In Spec',
                  key: 'menu-post-override-in',
                  /* eslint-disable no-unused-vars */
                  EVENT_HANDLER: ({ metaData }) => {
                    // todo process as just Ids? or need to handle response
                    const selections = this.getScrubbingSelections();
                    // console.log('override in spec', selections, metaData, metaData.rowData);
                    this.processManualOverrides('InSpec', selections);
                  },
                },
                {
                  text: 'Override Out of Spec',
                  key: 'menu-post-override-out',
                  EVENT_HANDLER: ({ metaData }) => {
                    // todo process as just Ids? or need to handle response
                    const selections = this.getScrubbingSelections();
                    // console.log('override in spec', selections, metaData, metaData.rowData);
                    this.processManualOverrides('OutOfSpec', selections);
                  },
                },
                {
                  text: 'Swap Proposal Detail',
                  key: 'menu-post-swap-detail',
                  EVENT_HANDLER: ({ metaData }) => {
                    // todo process as just Ids? or need to handle response
                    const selections = this.getScrubbingSelections();
                    // console.log('override in spec', selections, metaData, metaData.rowData);
                    this.openSwapDetailModal(selections);
                  },
                },
              ],
            },
            ROW: {
              enabled: true,
              renderer: ({ rowProps, cells }) => {
                const stateKey = cells[0].props.stateKey;
                const rowId = cells[0].props.rowId;
                const updatedRowProps = {
                  ...rowProps,
                  tabIndex: 1,
                  /* onClick: (e) => {
                    rowProps.onClick(e);
                    this.hideContextMenu({ stateKey });
                  }, */
                  onBlur: () => {
                    if (rowId) {
                      this.hideContextMenu({ stateKey });
                    }
                  },
                  onContextMenu: (e) => {
                    e.preventDefault();
                    // only show if is an active selected row
                    const isSelected = rowProps.className.includes('active');
                    if (isSelected) {
                      const rowElement = e.target.closest('.react-grid-row');
                      const contextMenuContainer = rowElement.querySelector('.react-grid-action-icon');
                      contextMenuContainer.setAttribute('style', `right: ${(rowElement.clientWidth - e.clientX) - 102}px`); // 102 contextMenu width
                     // console.log('on context', isSelected, rowProps, cells, rowId, rowElement, contextMenuContainer);
                      // this.deselectAll({ stateKey });
                      // this.selectRow({ rowId, stateKey });
                      this.showContextMenu({ id: rowId, stateKey });
                    }
                  },
                };
                return (
                  <tr {...updatedRowProps}>{ cells }</tr>
                );
              },
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
            <div>
            <Grid {...grid} data={ClientScrubs} store={this.context.store} height={340} />
            <SwapDetailModal details={details} />
            </div>
        );
    }
}

PostScrubbingGrid.propTypes = {
    grid: PropTypes.object.isRequired,
    dataSource: PropTypes.object.isRequired,
    // ClientScrubs: PropTypes.object.isRequired,
    activeScrubbingData: PropTypes.object.isRequired,
    // setOverlayLoading: PropTypes.func.isRequired,
    selection: PropTypes.object.isRequired,
    selectRow: PropTypes.func.isRequired,
    deselectAll: PropTypes.func.isRequired,
    doLocalSort: PropTypes.func.isRequired,
    showMenu: PropTypes.func.isRequired,
    hideMenu: PropTypes.func.isRequired,
    overrideStatus: PropTypes.func.isRequired,
    details: PropTypes.array.isRequired,
    toggleModal: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostScrubbingGrid);
