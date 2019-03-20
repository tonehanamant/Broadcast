import React, { Component } from "react";
import PropTypes from "prop-types";
import { Popover, Glyphicon, OverlayTrigger } from "react-bootstrap";
import FilterListInput from "../FilterListInput";
import FilterDateInput from "../FilterDateInput";
import FilterTimeInput from "../FilterTimeInput";

export default class FilterPopoverWrapper extends Component {
  constructor(props) {
    super(props);
    this.popover = null;
    this.closePopover = this.closePopover.bind(this);
    this.showPopover = this.showPopover.bind(this);
    this.setFilter = this.setFilter.bind(this);
    // this.clearFilter = this.clearFilter.bind(this);
  }

  setFilter(filter) {
    const { applyFilter } = this.props;
    applyFilter(filter);
    this.closePopover();
  }

  closePopover() {
    // console.log('closePopover', this, this.popover);
    this.popover.hide();
  }

  showPopover() {
    // console.log('showPopover', this, this.popover);
    this.popover.show();
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
    const isActive = filterActive;
    const activeColor = isActive ? "green" : "#999";
    // console.log('render filter wrapper', filterOptions);
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
        ref={ref => {
          this.popover = ref;
        }}
      >
        <div
          style={{ backgroundColor: "white", cursor: "pointer" }}
          styleName="editable-cell"
        >
          <Glyphicon
            styleName="pull-right"
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
