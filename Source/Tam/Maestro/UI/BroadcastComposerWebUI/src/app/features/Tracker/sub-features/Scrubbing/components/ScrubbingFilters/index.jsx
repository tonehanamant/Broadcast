import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Button } from "react-bootstrap";
import CSSModules from "react-css-modules";
import { Grid } from "Lib/react-redux-grid";
import { scrubbingActions } from "Tracker";
import FilterPopoverWrapper from "Patterns/filters/FilterPopoverWrapper";
import styles from "./index.style.scss";

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getScrubbingDataFiltered: scrubbingActions.getScrubbingDataFiltered,
      clearFilteredScrubbingData: scrubbingActions.clearFilteredScrubbingData
    },
    dispatch
  );

export class TrackerScrubbingFilters extends Component {
  constructor(props) {
    super(props);

    this.applyFilter = this.applyFilter.bind(this);
    this.onClear = this.onClear.bind(this);
  }

  onClear() {
    const { clearFilteredScrubbingData } = this.props;
    clearFilteredScrubbingData();
  }

  applyFilter(filter) {
    const { getScrubbingDataFiltered } = this.props;
    getScrubbingDataFiltered(filter);
  }

  render() {
    const stateKey = "TrackerScrubbingFiltersGrid";
    const inactiveFilterStyle = {
      backgroundColor: "#bfbfbf",
      minHeight: "20px",
      maxHeight: "20px",
      width: "100%",
      borderRadius: "2px"
    };
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
        renderer: () => <div style={inactiveFilterStyle} />
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

    const { activeFilters } = this.props;

    const grid = {
      data: activeFilters,
      classNames: ["filter-grid"],
      height: false,
      columns,
      plugins,
      stateKey
    };
    return (
      <div
        style={{ maxHeight: "28px", overflow: "hidden", marginBottom: "2px" }}
      >
        <Grid {...grid} />
      </div>
    );
  }
}

TrackerScrubbingFilters.propTypes = {
  activeFilters: PropTypes.array.isRequired,
  getScrubbingDataFiltered: PropTypes.func.isRequired,
  clearFilteredScrubbingData: PropTypes.func.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(CSSModules(TrackerScrubbingFilters, styles));
