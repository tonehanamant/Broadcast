import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Button, Glyphicon, Tooltip, OverlayTrigger } from 'react-bootstrap';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';
import Sorter from 'Utils/react-redux-grid-sorter';

const { SelectionActions, GridActions } = Actions;
// const { showMenu, hideMenu } = MenuActions;
const { deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({ planning: { planningProposals }, grid, dataSource }) => ({
  planningProposals,
  grid,
  dataSource,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
  {
    deselectAll,
    doLocalSort,
  }, dispatch)
);

export class PlanningGrid extends Component {
  constructor(props, context) {
		super(props, context);
    this.context = context;
    this.showProposalDetail = this.showProposalDetail.bind(this);
  }

  componentDidUpdate(prevProps) {
    if (prevProps.planningProposals !== this.props.planningProposals) {
      // evaluate column sort direction
      setTimeout(() => {
        const cols = this.props.grid.get('gridPlanningHome').get('columns');
        let sortCol = cols.find(x => x.sortDirection);
        if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);
        if (sortCol) {
          const datasource = this.props.dataSource.get('gridPlanningHome');
          const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);

          this.props.doLocalSort({
            data: sorted,
            stateKey: 'gridPlanningHome',
          });
        }
      }, 0);
    }
  }
  /* eslint-disable class-methods-use-this */
  showProposalDetail(id) {
    // console.log('showProposalDetail', this, id);
    // this.props.history.push(`/proposal/${id}`);
    const url = `planning/proposal/${id}`;
    window.location.assign(url);
  }

  render() {
    const stateKey = 'gridPlanningHome';

    const columns = [
      // use displayId string for search or breaks Fuzzy; use Id displayed/sort (int)
      {
        name: 'Search Id',
        dataIndex: 'displayId',
        hidden: true,
        hideable: false,
        width: '5%',
      },
      {
        name: 'Id',
        dataIndex: 'Id',
        defaultSortDirection: 'ASC',
        width: '5%',
      },
      {
        name: 'Name',
        dataIndex: 'ProposalName',
        width: '20%',
      },
      {
        name: 'Advertiser',
        dataIndex: 'displayAdvertiser',
        width: '20%',
      },
      {
        name: 'Status',
        dataIndex: 'displayStatus',
        width: '10%',
      },
      {
        name: 'Flight',
        dataIndex: 'displayFlights',
        width: '20%',
        renderer: ({ row }) => {
          let hasTip = false;
          const checkFlightWeeksTip = (Flights) => {
            // console.log(Flights);
            if (Flights.length < 1) return '';
            const tip = [<div key="flight">Hiatus Weeks</div>];
            Flights.forEach((flight, idx) => {
              if (flight.IsHiatus) {
                hasTip = true;
                const key = `flight_ + ${idx}`;
                tip.push(<div key={key}><DateMDYYYY date={flight.StartDate} /><span> - </span><DateMDYYYY date={flight.EndDate} /></div>);
              }
            });
            const display = tip;
            return (
              <Tooltip id="flightstooltip">{display}</Tooltip>
            );
          };
          const tooltip = checkFlightWeeksTip(row.Flights);
          return (
            <div>
              <span>{row.displayFlights}</span>
              { hasTip &&
              <OverlayTrigger placement="top" overlay={tooltip}>
              <Button bsStyle="link"><Glyphicon style={{ color: 'black' }} glyph="info-sign" /></Button>
              </OverlayTrigger>
              }
            </div>
          );
        },
      },
      {
        name: 'Owner',
        dataIndex: 'Owner',
        width: '15%',
      },
      {
        name: 'Last Modified',
        dataIndex: 'displayLastModified',
        width: '10%',
      },
    ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: true,
        moveable: true,
        sortable: {
            enabled: true,
            method: 'local',
        },
      },
      PAGER: {
        enabled: false,
        pagingType: 'local',
        pagerComponent: (
            <CustomPager stateKey={stateKey} idProperty="Id" />
        ),
      },
      LOADER: {
        enabled: true,
      },
      SELECTION_MODEL: {
        mode: 'single',
        enabled: true,
        allowDeselect: true,
        activeCls: 'active',
        selectionEvent: 'singleclick',
      },
      // Seems only column hide/show option is available
      // when you have GRID_Actions enabled as done below
      // Need a way to hide the actions on each row but not the columns
      GRID_ACTIONS: {
        iconCls: 'action-icon',
        menu: [],
      },
      // BULK_ACTIONS: {
      //   iconCls: 'action-icon',
      //   enabled: true,
      //   actions: [
      //     {
      //         text: 'Bulk Action Button',
      //         EVENT_HANDLER: () => {
      //             console.log('Doing a bulk action');
      //         },
      //     },
      //   ],
      // },
      // ROW: {
      //   enabled: true,
      //   renderer: ({ cells, ...rowData }) => (
      //     <ContextMenuRow
      //       {...rowData}
      //       menuItems={menuItems}
      //       stateKey={stateKey}
      //     >
      //       {cells}
      //     </ContextMenuRow>),
      // },
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll({ stateKey });
      },
      HANDLE_ROW_DOUBLE_CLICK: (row) => {
          const Id = row.row.Id;
          this.showProposalDetail(Id);
      },
    };

    const grid = {
      columns,
      plugins,
      events,
      stateKey,
    };
    return (
      <Grid {...grid} data={this.props.planningProposals} store={this.context.store} height={460} />
    );
  }
}

PlanningGrid.defaultProps = {
  planningProposals: [],
};

PlanningGrid.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  // menu: PropTypes.object.isRequired,

  planningProposals: PropTypes.array.isRequired,

  // getPost: PropTypes.func.isRequired,
  // getPostClientScrubbing: PropTypes.func.isRequired,
  // toggleModal: PropTypes.func.isRequired,
  // createAlert: PropTypes.func.isRequired,

  // showMenu: PropTypes.func.isRequired,
  // hideMenu: PropTypes.func.isRequired,
  // selectRow: PropTypes.func.isRequired,
    // history: PropTypes.object,
    deselectAll: PropTypes.func.isRequired,
    doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PlanningGrid);
