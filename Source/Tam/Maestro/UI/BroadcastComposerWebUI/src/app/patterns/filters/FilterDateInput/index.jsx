import React, { Component } from "react";
import CSSModules from "react-css-modules";
import PropTypes from "prop-types";
import { Col, Form, ButtonToolbar, Button, FormGroup } from "react-bootstrap";
import moment from "moment";
import { DatePicker } from "antd";
import styles from "./index.style.scss";

const dateFormat = "MM/DD/YYYY";

class FilterDateInput extends Component {
  constructor(props) {
    super(props);
    const { filterOptions } = this.props;
    this.state = {
      startDate: moment(),
      endDate: moment(),
      originalStartDate: moment(
        filterOptions.originalDateAiredStart,
        "YYYY-MM-DD"
      ).toDate(),
      originalEndDate: moment(
        filterOptions.originalDateAiredEnd,
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
    const { filterOptions } = this.props;
    this.setState({
      startDate: moment(filterOptions.DateAiredStart, "YYYY-MM-DD"),
      endDate: moment(filterOptions.DateAiredEnd, "YYYY-MM-DD")
    });
  }

  setValidSelections() {
    const { startValid, endValid } = this.state;
    if (startValid && endValid) {
      this.setState({
        isValidSelection: true
      });
    } else {
      this.setState({
        isValidSelection: false
      });
    }
  }

  disabledStartDate(current) {
    const { originalStartDate } = this.state;
    return current && current.valueOf() < originalStartDate;
  }

  disabledEndDate(current) {
    const { originalEndDate } = this.state;
    return current && current.valueOf() > originalEndDate;
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

  clear() {
    // update states as needed then apply
    const { filterOptions, filterKey, applySelection } = this.props;
    const options = {
      DateAiredStart: filterOptions.originalDateAiredStart,
      DateAiredEnd: filterOptions.originalDateAiredEnd
    };
    // using exclusions in this context to denote not active;
    applySelection({
      filterKey,
      exclusions: false,
      filterOptions: options
    });
  }

  // apply filters - filterOptions and matchOptions if applicable
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    const { filterOptions, filterKey, applySelection } = this.props;
    const { startDate, endDate } = this.state;
    // get values of both inputs
    const startDateIso = moment(startDate).toISOString();
    const endDateIso = moment(endDate).toISOString();
    let exclusions = true;
    // if startDate and endDate are the same as originalStartDate and originalEndDate
    // then set exclusions to false, otherwise set to true
    if (
      startDateIso ===
        moment(filterOptions.originalDateAiredStart).toISOString() &&
      endDateIso === moment(filterOptions.originalDateAiredEnd).toISOString()
    ) {
      exclusions = false;
    }
    const options = { DateAiredStart: startDateIso, DateAiredEnd: endDateIso };
    applySelection({
      filterKey,
      exclusions,
      filterOptions: options
    });
  }

  render() {
    const { startDate, endDate, isValidSelection } = this.state;
    return (
      <div>
        <Form horizontal>
          <FormGroup>
            <Col style={{ textAlign: "left" }} styleName="control-label" sm={4}>
              Start date
            </Col>
            <Col sm={8}>
              <DatePicker
                disabledDate={this.disabledStartDate}
                format={dateFormat}
                allowClear={false}
                showToday={false}
                value={startDate}
                onChange={this.handleStartChange}
                getCalendarContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
          <FormGroup>
            <Col style={{ textAlign: "left" }} styleName="control-label" sm={4}>
              End date
            </Col>
            <Col sm={8}>
              <DatePicker
                disabledDate={this.disabledEndDate}
                format={dateFormat}
                allowClear={false}
                value={endDate}
                onChange={this.handleEndChange}
                showToday={false}
                getCalendarContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
        </Form>
        <ButtonToolbar styleName="pull-right" style={{ margin: "0 0 8px 0" }}>
          <Button bsStyle="success" bsSize="xsmall" onClick={this.clear}>
            Clear
          </Button>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            disabled={!isValidSelection}
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

export default CSSModules(FilterDateInput, styles);
