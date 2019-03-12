import React, { Component } from "react";
// import CSSModules from 'react-css-modules';
import PropTypes from "prop-types";
// import { v4 } from 'uuid';
import {
  Col,
  Form,
  ButtonToolbar,
  Button,
  FormGroup
} from "react-bootstrap";
import moment from "moment";
// import DatePicker from 'react-datepicker';
// import 'react-datepicker/dist/react-datepicker.css';
import { DatePicker } from "antd";
import "./index.css";

const dateFormat = "MM/DD/YYYY";

class FilterDateInput extends Component {
  constructor(props) {
    super(props);
    this.state = {
      startDate: moment(),
      endDate: moment(),
      originalStartDate: moment(
        this.props.filterOptions.originalDateAiredStart,
        "YYYY-MM-DD"
      ).toDate(),
      originalEndDate: moment(
        this.props.filterOptions.originalDateAiredEnd,
        "YYYY-MM-DD"
      ).toDate(),
      filterOptions: {},
      startValid: true,
      endValid: true,
      isValidSelection: true
    };
    this.disabledStartDate = this.disabledStartDate.bind(this);
    this.disabledEndDate = this.disabledEndDate.bind(this);
    this.handleStartChange = this.handleStartChange.bind(this);
    this.handleEndChange = this.handleEndChange.bind(this);
    this.apply = this.apply.bind(this);
    this.clear = this.clear.bind(this);
    this.setValidSelections = this.setValidSelections.bind(this);
  }

  componentWillMount() {
    this.setState({
      startDate: moment(this.props.filterOptions.DateAiredStart, "YYYY-MM-DD"),
      endDate: moment(this.props.filterOptions.DateAiredEnd, "YYYY-MM-DD")
    });
  }

  disabledStartDate(current) {
    console.log(this);
    return current && current.valueOf() < this.state.originalStartDate;
  }

  disabledEndDate(current) {
    console.log(this);
    return current && current.valueOf() > this.state.originalEndDate;
  }

  handleStartChange(date) {
    if (date && moment(date).isValid()) {
      this.setState(
        {
          startDate: date,
          startValid: true
        },
        () => {
          this.setValidSelections();
        }
      );
    }
  }

  handleEndChange(date) {
    if (date && moment(date).isValid()) {
      this.setState(
        {
          endDate: date,
          endValid: true
        },
        () => {
          this.setValidSelections();
        }
      );
    }
  }

  setValidSelections() {
    if (this.state.startValid && this.state.endValid) {
      this.setState({
        isValidSelection: true
      });
    } else {
      this.setState({
        isValidSelection: false
      });
    }
  }

  clear() {
    // update states as needed then apply
    // REVIEW may be problematic as filterOptions may be changed - need originals? yes
    const options = {
      DateAiredStart: this.props.filterOptions.originalDateAiredStart,
      DateAiredEnd: this.props.filterOptions.originalDateAiredEnd
    };
    // using exclusions in this context to denote not active;
    this.props.applySelection({
      filterKey: this.props.filterKey,
      exclusions: false,
      filterOptions: options
    });
  }
  // apply filters - filterOptions and matchOptions if applicable
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    // get values of both inputs
    const startDate = moment(this.state.startDate).toISOString();
    const endDate = moment(this.state.endDate).toISOString();
    let exclusions = true;
    // if startDate and endDate are the same as originalStartDate and originalEndDate
    // then set exclusions to false, otherwise set to true
    if (
      startDate ===
        moment(this.props.filterOptions.originalDateAiredStart).toISOString() &&
      endDate ===
        moment(this.props.filterOptions.originalDateAiredEnd).toISOString()
    ) {
      exclusions = false;
    }
    const options = { DateAiredStart: startDate, DateAiredEnd: endDate };
    this.props.applySelection({
      filterKey: this.props.filterKey,
      exclusions,
      filterOptions: options
    });
  }

  render() {
    return (
      <div>
        <Form horizontal>
          <FormGroup>
            <Col style={{ textAlign: "left" }} className="control-label" sm={4}>
              Start date
            </Col>
            <Col sm={8}>
              <DatePicker
                disabledDate={this.disabledStartDate}
                format={dateFormat}
                allowClear={false}
                showToday={false}
                value={this.state.startDate}
                onChange={this.handleStartChange}
                getCalendarContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
          <FormGroup>
            <Col style={{ textAlign: "left" }} className="control-label" sm={4}>
              End date
            </Col>
            <Col sm={8}>
              <DatePicker
                disabledDate={this.disabledEndDate}
                format={dateFormat}
                allowClear={false}
                value={this.state.endDate}
                onChange={this.handleEndChange}
                showToday={false}
                getCalendarContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
        </Form>
        <ButtonToolbar className="pull-right" style={{ margin: "0 0 8px 0" }}>
          <Button bsStyle="success" bsSize="xsmall" onClick={this.clear}>
            Clear
          </Button>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            disabled={!this.state.isValidSelection}
            style={{ marginLeft: "10px" }}
            onClick={this.apply}
          >
            {" "}
            Apply
          </Button>
        </ButtonToolbar>
      </div>
    );
  }
}

FilterDateInput.defaultProps = {
  applySelection: () => {}
};

FilterDateInput.propTypes = {
  applySelection: PropTypes.func,
  filterOptions: PropTypes.object.isRequired,
  filterKey: PropTypes.string.isRequired
};

export default FilterDateInput;
