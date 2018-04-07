import React, { Component } from 'react';
import PropTypes from 'prop-types';
// import _ from 'lodash';
import { Button, ButtonToolbar, Popover, Glyphicon, OverlayTrigger, FormGroup, FormControl, ControlLabel } from 'react-bootstrap';

/* eslint-disable react/prefer-stateless-function */
export default class TestFilterGridCell extends Component {
  constructor(props) {
    super(props);
    // console.log(this.props);
    // temporary - can use props
    this.state = {
      active: false,
    };
    this.onSetFilter = this.onSetFilter.bind(this);
    this.onClearFilter = this.onClearFilter.bind(this);
    this.setFilter = this.setFilter.bind(this);
    this.clearFilter = this.clearFilter.bind(this);
    this.popover = null;
    this.closePopover = this.closePopover.bind(this);
    this.showPopover = this.showPopover.bind(this);
  }

  componentWillReceiveProps(nextProps) {
    console.log('NEXT Props', nextProps, this);
  }

  onSetFilter() {
    this.setFilter();
  }

  setFilter(values) {
    console.log('setFilter', this.props.FilterData.name, values);
    this.props.applyFilter(this.props.FilterData.name, true, values);
  // this.props.FilterData.active = true;
  // temp
    this.setState({ active: true });
    this.closePopover();
  }

  onClearFilter() {
    this.clearFilter();
  }

  clearFilter() {
    console.log('clearFilter', this.props.FilterData.name);
    this.props.applyFilter(this.props.FilterData.name, false, null); // or separate mechanism?
    // temp
    this.setState({ active: false });
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
    const title = this.props.FilterData.display;
    // temp - use state - switch to props
    // const isActive = this.props.FilterData.active;
    const isActive = this.state.active;
    const filterValue = JSON.stringify(this.props.FilterData);
    const activeColor = isActive ? 'green' : '#999';
    const popoverFilter = (
      <Popover id="popover-positioned-scrolling-top" title={title}>
        <FormGroup controlId="FilterEditor">
          <ControlLabel>Test</ControlLabel>
          <FormControl componentClass="textarea" placeholder="Enter ISCIs" style={{ height: '100px' }} value={filterValue} />
        </FormGroup>
        <ButtonToolbar style={{ marginBottom: '8px', float: 'right' }}>
          <Button
            bsStyle="default"
            bsSize="small"
            onClick={this.onClearFilter}
          >Clear</Button>
          <Button
            bsStyle="success"
            bsSize="small"
            onClick={this.onSetFilter}
          >Apply</Button>
				</ButtonToolbar>
      </Popover>
    );

    /* const button = isActive ? <Button bsStyle="link" style={{ padding: '2px', fontSize: '11px' }}><div className="truncate-iscis">{this.state.iscisDisplay.join(' | ')}</div></Button> :
      <Button bsStyle="link" style={{ padding: '2px', fontSize: '11px' }}><Glyphicon style={{ marginRight: '6px' }} glyph="plus" />Add Isci</Button>;
      const tooltip = <Tooltip id="Iscistooltip"><span style={{ fontSize: '9px' }}>ISCIs <br />{this.state.iscisValue}</span></Tooltip>;
      const touchedClass = this.state.isChanged ? 'editable-cell-changed' : ''; */
    return (
        <OverlayTrigger trigger="click" placement="bottom" overlay={popoverFilter} rootClose ref={(ref) => { this.popover = ref; }}>
          <div style={{ backgroundColor: 'white' }} className={'editable-cell'}><Glyphicon className="pull-right" style={{ marginTop: '4px', fontSize: '14px', color: activeColor }} glyph="filter" /></div>
        </OverlayTrigger>
    );
	}
}

TestFilterGridCell.defaultProps = {
  FilterData: {
    active: false,
    name: 'DayOfWeek',
    display: 'Day',
    type: 'check',
    exclusions: [],
    options: [{ display: 'Monday', value: 1 }, { display: 'Wednesday', value: 3 }],
  },
  applyFilter: () => {},
};

TestFilterGridCell.propTypes = {
  FilterData: PropTypes.object.isRequired,
  applyFilter: PropTypes.func,
};
