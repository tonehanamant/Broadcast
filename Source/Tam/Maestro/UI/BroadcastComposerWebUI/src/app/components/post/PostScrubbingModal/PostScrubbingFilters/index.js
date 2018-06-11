
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
    // this.props.clearScrubbingFiltersList();
    // wait so the store will update/clear first
    /* setTimeout(() => {
      this.props.getScrubbingDataFiltered(filter);
    }, 50); */
    // Change: use call in saga to block
    this.props.getScrubbingDataFiltered(filter);
  }

  render() {
    const stateKey = 'PostScrubbingFiltersGrid';
    const inactiveFilterStyle = { backgroundColor: '#bfbfbf', minHeight: '20px', maxHeight: '20px', width: '100%', borderRadius: '2px' };
    const columns = [
      {
        name: 'Status',
        dataIndex: 'Status',
        // width: '3%',
        // test specific width
        width: 59,
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Sequence',
        dataIndex: 'Sequence',
        width: 75,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch={false}
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Week Start',
        dataIndex: 'WeekStart',
        width: 100,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Date',
        // we don't want to mix TimeAired and MatchTime
        // dataIndex: 'TimeAired',
        dataIndex: 'DateAired',
        width: 100,
        renderer: ({ value }) => (
          // <div style={inactiveFilterStyle} />
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          filterType="dateInput"
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Time Aired',
        // we need a separate BE dataset
        dataIndex: 'MatchTime',
        width: 100,
        renderer: () => (
          <div style={inactiveFilterStyle} />
        ),
      },
      {
        name: 'Day',
        dataIndex: 'DayOfWeek',
        width: 80,
        // renderer: ({ value, row }) => {
        renderer: ({ value }) => (
            <FilterPopoverWrapper
              filterDisplay={value.filterDisplay}
              filterKey={value.filterKey}
              hasTextSearch={false}
              hasMatchSpec={value.hasMatchSpec}
              matchOptions={value.matchOptions}
              filterOptions={value.filterOptions}
              filterActive={value.active}
              applyFilter={this.applyFilter}
            />
          ),
      },
      {
        name: 'Ad Length',
        dataIndex: 'SpotLength',
        width: 95,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch={false}
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'House ISCI',
        dataIndex: 'ISCI',
        width: 150,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Client ISCI',
        dataIndex: 'ClientISCI',
        width: 150,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Program',
        dataIndex: 'ProgramName',
        width: 150,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Genre',
        dataIndex: 'GenreName',
        width: 100,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            hasTextSearch
            hasMatchSpec={value.hasMatchSpec}
            matchOptions={value.matchOptions}
            filterOptions={value.filterOptions}
            filterActive={value.active}
            applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Show Type',
        dataIndex: 'ShowTypeName',
        width: 100,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            hasTextSearch
            hasMatchSpec={value.hasMatchSpec}
            matchOptions={value.matchOptions}
            filterOptions={value.filterOptions}
            filterActive={value.active}
            applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Affiliate',
        dataIndex: 'Affiliate',
        width: 75,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Market',
        dataIndex: 'Market',
        width: 150,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            hasTextSearch
            hasMatchSpec={value.hasMatchSpec}
            matchOptions={value.matchOptions}
            filterOptions={value.filterOptions}
            filterActive={value.active}
            applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Station',
        dataIndex: 'Station',
        width: 75,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
          filterDisplay={value.filterDisplay}
          filterKey={value.filterKey}
          hasTextSearch
          hasMatchSpec={value.hasMatchSpec}
          matchOptions={value.matchOptions}
          filterOptions={value.filterOptions}
          filterActive={value.active}
          applyFilter={this.applyFilter}
          />
        ),
      },
      {
        name: 'Comments',
        dataIndex: 'Comments',
        // width: 150,
        width: '100%',
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

PostScrubbingFilters.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeFilters: PropTypes.array.isRequired,
  // doLocalSort: PropTypes.func.isRequired,
  getScrubbingDataFiltered: PropTypes.func.isRequired,
  clearScrubbingFiltersList: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(CSSModules(PostScrubbingFilters, styles));
