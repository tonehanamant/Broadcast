import React, { Component } from "react";
import PropTypes from "prop-types";
import { Popover, Glyphicon, OverlayTrigger } from "react-bootstrap";
import FilterListInput from "../FilterListInput";
import FilterDateInput from "../FilterDateInput";
import FilterTimeInput from "../FilterTimeInput";

export default class FilterPopoverWrapper extends Component {
  constructor(props) {
    super(props);
    this.popover = React.createRef();
    this.closePopover = this.closePopover.bind(this);
    this.showPopover = this.showPopover.bind(this);
    this.setFilter = this.setFilter.bind(this);
  }

  setFilter(filter) {
    const { applyFilter } = this.props;
    applyFilter(filter);
    this.closePopover();
  }

  closePopover() {
    this.popover.current.hide();
  }

  showPopover() {
    this.popover.current.show();
  }

  render() {
    const {
      filterKey,
      filterDisplay,
      filterOptions,
      filterType,
      matchOptions,
      hasTextSearch,
      hasMatchSpec,
      filterActive
    } = this.props;
    const activeColor = filterActive ? "green" : "#999";
    const popoverFilter = (
      <Popover id="popover-positioned-scrolling-top" title={filterDisplay}>
        {filterType === "dateInput" && (
          <FilterDateInput
            filterKey={filterKey}
            filterOptions={filterOptions}
            applySelection={this.setFilter}
          />
        )}

        {filterType === "timeInput" && (
          <FilterTimeInput
            filterKey={filterKey}
            filterOptions={filterOptions}
            applySelection={this.setFilter}
          />
        )}

        {filterType === "filterList" && (
          <FilterListInput
            filterKey={filterKey}
            filterOptions={filterOptions}
            matchOptions={matchOptions}
            applySelection={this.setFilter}
            hasTextSearch={hasTextSearch}
            hasMatchSpec={hasMatchSpec}
          />
        )}
      </Popover>
    );

    return (
      <OverlayTrigger
        trigger="click"
        placement="bottom"
        overlay={popoverFilter}
        rootClose
        ref={this.popover}
      >
        <div
          style={{ backgroundColor: "white", cursor: "pointer" }}
          className="editable-cell"
        >
          <Glyphicon
            className="pull-right"
            style={{ marginTop: "4px", fontSize: "14px", color: activeColor }}
            glyph="filter"
          />
        </div>
      </OverlayTrigger>
    );
  }
}

FilterPopoverWrapper.defaultProps = {
  applyFilter: () => {},
  hasTextSearch: true,
  hasMatchSpec: false,
  matchOptions: {},
  filterActive: false,
  filterType: "filterList",
  filterDisplay: "Filter"
};

FilterPopoverWrapper.propTypes = {
  applyFilter: PropTypes.func,
  hasTextSearch: PropTypes.bool,
  hasMatchSpec: PropTypes.bool,
  matchOptions: PropTypes.object,
  filterType: PropTypes.string,
  filterKey: PropTypes.string.isRequired,
  filterDisplay: PropTypes.string,
  // filterOptions: PropTypes.array.isRequired,
  filterOptions: PropTypes.oneOfType([PropTypes.array, PropTypes.object])
    .isRequired,
  filterActive: PropTypes.bool
};
