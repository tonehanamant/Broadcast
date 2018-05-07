import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Popover, Glyphicon, OverlayTrigger } from 'react-bootstrap';
import FilterListInput from '../FilterListInput';

export default class FilterPopoverWrapper extends Component {
  constructor(props) {
    super(props);

    this.popover = null;
    this.closePopover = this.closePopover.bind(this);
    this.showPopover = this.showPopover.bind(this);
    this.setFilter = this.setFilter.bind(this);
    // this.clearFilter = this.clearFilter.bind(this);
  }

  /* componentWillReceiveProps(nextProps) {
    console.log('filter popover  receive props', nextProps, this);
  } */

  // REMOVE clear just pass through - unless need a check, etc?
  // intercept to close popover; todo key from here, etc?
  setFilter(filter) {
    // console.log('setFilter', filter.filterKey, filter.exclusions);
    /* if (filter.exclusions.length > 0) {
      this.props.applyFilter(filter);
      this.closePopover();
    } else {
      this.clearFilter(filter);
    } */
    this.props.applyFilter(filter);
    this.closePopover();
  }
 // may not need
 /*  clearFilter(filter) {
    // console.log('clearFilter', this);
    this.props.applyFilter(filter);
    this.closePopover();
  } */

  closePopover() {
    // console.log('closePopover', this, this.popover);
    this.popover.hide();
  }

  showPopover() {
    // console.log('showPopover', this, this.popover);
    this.popover.show();
  }

  render() {
    const { filterKey, filterDisplay, filterOptions, matchOptions, hasTextSearch, hasMatchSpec } = this.props;
    const isActive = this.props.filterActive;
    const activeColor = isActive ? 'green' : '#999';
    // console.log('render filter wrapper', filterOptions);
    const popoverFilter = (
      <Popover
        id="popover-positioned-scrolling-top"
        title={filterDisplay}
      >
        <FilterListInput
          filterKey={filterKey}
          filterOptions={filterOptions}
          matchOptions={matchOptions}
          applySelection={this.setFilter}
          hasTextSearch={hasTextSearch}
          hasMatchSpec={hasMatchSpec}
        />
      </Popover>
    );

    return (
        <OverlayTrigger
          trigger="click"
          placement="bottom"
          overlay={popoverFilter}
          rootClose
          ref={(ref) => { this.popover = ref; }}
        >
          <div
            style={{ backgroundColor: 'white' }}
            className={'editable-cell'}
          >
            <Glyphicon
              className="pull-right"
              style={{ marginTop: '4px', fontSize: '14px', color: activeColor }}
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
  filterActive: false,
};

FilterPopoverWrapper.propTypes = {
  applyFilter: PropTypes.func,
  hasTextSearch: PropTypes.bool,
  hasMatchSpec: PropTypes.bool,
  matchOptions: PropTypes.object.isRequired,
  filterKey: PropTypes.string.isRequired,
  filterDisplay: PropTypes.string.isRequired,
  filterOptions: PropTypes.array.isRequired,
  filterActive: PropTypes.bool.isRequired,
};
