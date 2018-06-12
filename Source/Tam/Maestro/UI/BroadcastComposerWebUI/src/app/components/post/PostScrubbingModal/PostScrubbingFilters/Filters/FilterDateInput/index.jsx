import React, { Component } from 'react';
// import CSSModules from 'react-css-modules';
import PropTypes from 'prop-types';
// import { v4 } from 'uuid';
import { Col, Form, ButtonToolbar, Button, FormGroup } from 'react-bootstrap/lib/';
import DatePicker from 'react-datepicker';
import moment from 'moment';
import 'react-datepicker/dist/react-datepicker.css';
import './index.css';

class FilterDateInput extends Component {
  constructor(props) {
    super(props);
    this.state = {
      startDate: moment(),
      endDate: moment(),
      originalStartDate: moment(this.props.filterOptions.originalDateAiredStart, 'YYYY-MM-DD').toDate(),
      originalEndDate: moment(this.props.filterOptions.originalDateAiredEnd, 'YYYY-MM-DD').toDate(),
      filterOptions: {},
    };
    this.startInput = null;
    this.endInput = null;
    this.handleStartChange = this.handleStartChange.bind(this);
    this.handleEndChange = this.handleEndChange.bind(this);
    this.apply = this.apply.bind(this);
    this.clear = this.clear.bind(this);
  }

  componentWillMount() {
    // console.log('Date Input Mount >>>>', this.props);
    this.setState({
      startDate: moment(this.props.filterOptions.DateAiredStart, 'YYYY-MM-DD'),
      endDate: moment(this.props.filterOptions.DateAiredEnd, 'YYYY-MM-DD'),
    });
  }

  handleStartChange(date) {
    this.setState({
      startDate: date,
    });
  }

  handleEndChange(date) {
    this.setState({
      endDate: date,
    });
  }

  clear() {
    // update states as needed then apply
    // REVIEW may be problematic as filterOptions may be changed - need originals? yes
    const options = { DateAiredStart: this.props.filterOptions.originalDateAiredStart, DateAiredEnd: this.props.filterOptions.originalDateAiredEnd };
    // using exclusions in this context to denote not active;
    this.props.applySelection({ filterKey: this.props.filterKey, exclusions: false, filterOptions: options });
  }
  // apply filters - filterOptions and matchOptions if applicable
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    // get values of both inputs
    const startDate = moment(this.state.startDate).toISOString();
    const endDate = moment(this.state.endDate).toISOString();

    const options = { DateAiredStart: startDate, DateAiredEnd: endDate };
    this.props.applySelection({
      filterKey: this.props.filterKey,
      exclusions: true,
      filterOptions: options,
    });
  }

  render() {
    return (
      <div>
        <Form horizontal>
          <FormGroup>
            <Col style={{ textAlign: 'left' }} className="control-label" sm={4}>Start date</Col>
            <Col sm={8}>
              {/* <label style={{ display: 'inline' }} className="control-label" htmlFor="date1">Start date</label> */}
              <DatePicker
                // className="form-control"
                selected={this.state.startDate}
                onChange={this.handleStartChange}
                minDate={this.state.originalStartDate}
                maxDate={this.state.originalEndDate}
              />
            </Col>
          </FormGroup>
          <FormGroup>
            {/* <label style={{ display: 'inline' }} className="control-label" htmlFor="date2">End date</label> */}
            <Col style={{ textAlign: 'left' }} className="control-label" sm={4}>End date</Col>
            <Col sm={8}>
              <DatePicker
                // className="form-control"
                selected={this.state.endDate}
                onChange={this.handleEndChange}
                minDate={this.state.originalStartDate}
                maxDate={this.state.originalEndDate}
              />
            </Col>
          </FormGroup>
        </Form>
        <ButtonToolbar style={{ minWidth: '90%' }}>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            onClick={this.clear}
            // disabled={!canFilter}
          >Clear
          </Button>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            // disabled={!canFilter || !this.state.isValidSelection}
            style={{ marginLeft: '10px' }}
            onClick={this.apply}
          > Apply
          </Button>
        </ButtonToolbar>
      </div>
    );
  }
}

FilterDateInput.defaultProps = {
  applySelection: () => {},
};

FilterDateInput.propTypes = {
  applySelection: PropTypes.func,
  filterOptions: PropTypes.object.isRequired,
  filterKey: PropTypes.string.isRequired,
};

export default FilterDateInput;
