import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Button } from "react-bootstrap";
import CSSModules from "react-css-modules";
import { Grid } from "react-redux-grid";
import {
  getScrubbingDataFiltered,
  clearScrubbingFiltersList,
  clearFilteredScrubbingData,
  getClearScrubbingDataFiltered
} from "Post/redux/actions";
import FilterPopoverWrapper from "Patterns/filters/FilterPopoverWrapper";
import styles from "./index.scss";

const mapStateToProps = (grid, dataSource) => ({
  grid,
  dataSource
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getScrubbingDataFiltered,
      clearScrubbingFiltersList,
      clearFilteredScrubbingData,
      getClearScrubbingDataFiltered
    },
    dispatch
  );

export class PostScrubbingFilters extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;

    this.applyFilter = this.applyFilter.bind(this);
    this.onClear = this.onClear.bind(this);
  }

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

  onClear() {
    this.props.getClearScrubbingDataFiltered();
  }

  render() {
    const stateKey = "PostScrubbingFiltersGrid";
    const columns = [
      {
        name: "Status",
        dataIndex: "Status",
        width: 59,
        renderer: () => (
          <Button bsSize="xsmall" onClick={this.onClear}>
            Clear
          </Button>
        )
      },
      {
        name: "Sequence",
        dataIndex: "Sequence",
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
        )
      },
      {
        name: "Market",
        dataIndex: "Market",
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
        )
      },
      {
        name: "Station",
        dataIndex: "Station",
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
        )
      },
      {
        name: "Affiliate",
        dataIndex: "Affiliate",
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
        )
      },
      {
        name: "Week Start",
        dataIndex: "WeekStart",
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
        )
      },
      {
        name: "Day",
        dataIndex: "DayOfWeek",
        width: 80,
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
        )
      },
      {
        name: "Date",
        dataIndex: "DateAired",
        width: 100,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            filterOptions={value.filterOptions}
            filterActive={value.active}
            filterType="dateInput"
            applyFilter={this.applyFilter}
          />
        )
      },
      {
        name: "Time Aired",
        dataIndex: "TimeAired",
        width: 100,
        renderer: ({ value }) => (
          <FilterPopoverWrapper
            filterDisplay={value.filterDisplay}
            filterKey={value.filterKey}
            filterOptions={value.filterOptions}
            filterActive={value.active}
            filterType="timeInput"
            applyFilter={this.applyFilter}
          />
        )
      },
      {
        name: "Program",
        dataIndex: "ProgramName",
        width: 200,
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
        )
      },
      {
        name: "Genre",
        dataIndex: "GenreName",
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
        )
      },
      {
        name: "Show Type",
        dataIndex: "ShowTypeName",
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
        )
      },
      {
        name: "Spot Length",
        dataIndex: "SpotLength",
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
        )
      },
      {
        name: "House ISCI",
        dataIndex: "ISCI",
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
        )
      },
      {
        name: "Client ISCI",
        dataIndex: "ClientISCI",
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
        )
      },
      {
        name: "Comments",
        dataIndex: "Comments",
        width: "100%",
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
        )
      }
    ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: false,
        moveable: false,
        sortable: {
          enabled: false,
          method: "local"
        }
      }
    };

    const grid = {
      columns,
      plugins,
      stateKey
    };

    return (
      <div
        style={{ maxHeight: "28px", overflow: "hidden", marginBottom: "2px" }}
      >
        <Grid
          {...grid}
          classNames={["filter-grid"]}
          data={this.props.activeFilters}
          store={this.context.store}
          height={false}
        />
      </div>
    );
  }
}

PostScrubbingFilters.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeFilters: PropTypes.array.isRequired,
  getScrubbingDataFiltered: PropTypes.func.isRequired,
  clearScrubbingFiltersList: PropTypes.func.isRequired,
  clearFilteredScrubbingData: PropTypes.func.isRequired,
  getClearScrubbingDataFiltered: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(PostScrubbingFilters, styles));
