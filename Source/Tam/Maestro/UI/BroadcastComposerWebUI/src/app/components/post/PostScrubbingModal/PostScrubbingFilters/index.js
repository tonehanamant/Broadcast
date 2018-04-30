
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import CSSModules from 'react-css-modules';
import { Grid } from 'react-redux-grid';
import { getScrubbingDataFiltered, clearScrubbingFiltersList } from 'Ducks/post';
import styles from './index.scss';
import FilterPopoverWrapper from './Filters/FilterPopoverWrapper';

const mapStateToProps = (grid, dataSource) => ({
  grid,
  dataSource,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
    {
      getScrubbingDataFiltered,
      clearScrubbingFiltersList,
    }, dispatch)
);

export class PostScrubbingFilters extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;

    this.applyFilter = this.applyFilter.bind(this);
    // remove as should not be needed - use saga/props
    /* this.state = {
      filterOptions: {},
    }; */
  }

  /* componentWillReceiveProps(nextProps) {
    console.log('filtergrid receive props', nextProps, this);
  } */

  /* shouldComponentUpdate(nextProps, nextState) {
    console.log('filtergrid should component update', nextProps, nextState);
    return true;
  } */

  applyFilter(filter) {
    // ISSUE: Data changes but Object so does not update
    // clear the grid data then reset (combining in saga/reducer does not work)
    this.props.clearScrubbingFiltersList();
    // no longer maintaining here
    // const filterOptions = { ...this.state.filterOptions };
    // filterOptions[filter.filterKey] = filter;
    // this.setState({ filterOptions });
    // console.log('apply filter', filter);
    // wait so the store will update/clear first
    setTimeout(() => {
      this.props.getScrubbingDataFiltered(filter);
    }, 50);
  }

  render() {
    const stateKey = 'PostScrubbingFiltersGrid';
    const inactiveFilterStyle = { backgroundColor: '#bfbfbf', minHeight: '20px', maxHeight: '20px', width: '100%', borderRadius: '2px' };
    const columns = [
      {
        name: 'Status',
        dataIndex: 'Status',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Week Start',
        dataIndex: 'WeekStart',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Date',
        dataIndex: 'TimeAired',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Time Aired',
        dataIndex: 'MatchTime',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Day',
        dataIndex: 'DayOfWeek',
        width: '6%',
        // renderer: ({ value, row }) => {
        renderer: ({ value }) => (
            <FilterPopoverWrapper
              filterDisplay={value.filterDisplay}
              filterKey={value.filterKey}
              textSearch={false}
              filterOptions={value.filterOptions}
              filterActive={value.active}
              applyFilter={this.applyFilter}
            />
          ),
      },
      {
        name: 'Ad Length',
        dataIndex: 'SpotLength',
        width: '4%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'House ISCI',
        dataIndex: 'ISCI',
        width: '7%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Client ISCI',
        dataIndex: 'ClientISCI',
        width: '7%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Program',
        dataIndex: 'ProgramName',
        width: '12%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
        // testing
       /*  renderer: ({ value }) => (
            <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            textSearch
            filterOptions={value.filterOptions}
            filterActive={value.active}
            applyFilter={this.applyFilter}
            />
          ), */
      },
      {
        name: 'Genre',
        dataIndex: 'GenreName',
        width: '6%',
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            textSearch
            filterOptions={value.filterOptions}
            filterActive={value.active}
            applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Affiliate',
        dataIndex: 'Affiliate',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Market',
        dataIndex: 'Market',
        width: '12%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Station',
        dataIndex: 'Station',
        width: '6%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Comments',
        dataIndex: 'Comments',
        width: '10%',
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
  ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: false,
        moveable: false,
        sortable: {
            enabled: false,
            method: 'local',
        },
      },
    };

    const grid = {
      columns,
      plugins,
      stateKey,
    };

    return (
      <div style={{ maxHeight: '28px', overflow: 'hidden', marginBottom: '2px' }}>
        <Grid {...grid} classNames={['filter-grid']} data={this.props.activeFilters} store={this.context.store} height={false} />
      </div>
    );
  }
}

PostScrubbingFilters.defaultProps = {
  // getScrubbingDataFiltered: () => { },
  /* activeFilters: [
    {
      DayOfWeek: {
        filterDisplay: 'Days',
        filterKey: 'DayOfWeek',
        type: 'filterList',
        exclusions: [],
        filterOptions: [
          { Display: 'Monday', Value: 0, Selected: true },
          { Display: 'Tuesday', Value: 1, Selected: true },
          { Display: 'Wednesday', Value: 2, Selected: false },
          { Display: 'Thursday', Value: 3, Selected: true },
          { Display: 'Friday', Value: 4, Selected: true },
          { Display: 'Saturday', Value: 5, Selected: true },
          { Display: 'Sunday', Value: 6, Selected: false },
        ],
      },
      ProgramName: {
        filterDisplay: 'Programs',
        filterKey: 'ProgramName',
        type: 'filterList',
        exclusions: [],
        filterOptions: [
          { Display: 'Hot Bench', Value: 'Hot Bench', Selected: true },
          { Display: 'Inside Edition', Value: 'Inside Edition', Selected: true },
          { Display: 'Jeopardy', Value: 'Jeopardy', Selected: true },
          { Display: 'Jimmy Fallon', Value: 'Jimmy Fallon', Selected: true },
          { Display: 'Judge Judy', Value: 'Judge Judy', Selected: true },
          { Display: 'TMZ Live', Value: 'TMZ Live', Selected: true },
          { Display: 'Regis & Kelly', Value: 'Regis & Kelly', Selected: true },
          { Display: 'Stephen Colbert', Value: 'Stephen Colbert', Selected: true },
        ],
      },
    },
  ], */
};
PostScrubbingFilters.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeFilters: PropTypes.array.isRequired,
  // doLocalSort: PropTypes.func.isRequired,
  getScrubbingDataFiltered: PropTypes.func.isRequired,
  clearScrubbingFiltersList: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(CSSModules(PostScrubbingFilters, styles));
