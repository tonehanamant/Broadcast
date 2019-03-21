import React, { Component } from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import { Col, Form, ButtonToolbar, Button, FormGroup } from "react-bootstrap";
import { TimePicker } from "antd";
import moment from "moment";
import styles from "./index.style.scss";

class FilterTimeInput extends Component {
  constructor(props) {
    super(props);
    const { filterOptions } = this.props;
    this.state = {
      startTime: null,
      endTime: null,
      originalStartTime: filterOptions.originalTimeAiredStart,
      originalEndTime: filterOptions.originalTimeAiredEnd,
      filterOptions: {}
    };
    this.handleStartChange = this.handleStartChange.bind(this);
    this.handleEndChange = this.handleEndChange.bind(this);
    this.apply = this.apply.bind(this);
    this.clear = this.clear.bind(this);
  }

  componentWillMount() {
    const { filterOptions } = this.props;
    this.setState({
      originalStartTime: moment(
        moment()
          .startOf("day")
          .seconds(filterOptions.originalTimeAiredStart)
      ),
      originalEndTime: moment(
        moment()
          .startOf("day")
          .seconds(filterOptions.originalTimeAiredEnd)
      ),
      // startTime: moment(),
      startTime: moment(
        moment()
          .startOf("day")
          .seconds(filterOptions.TimeAiredStart)
      ),
      endTime: moment(
        moment()
          .startOf("day")
          .seconds(filterOptions.TimeAiredEnd)
      )
    });
  }

  handleStartChange(time) {
    this.setState({
      startTime: time
    });
  }

  handleEndChange(time) {
    this.setState({
      endTime: time
    });
  }

  clear() {
    // update states as needed then apply
    // REVIEW may be problematic as filterOptions may be changed - need originals? yes
    const { filterOptions, filterKey, applySelection } = this.props;
    const options = {
      TimeAiredStart: filterOptions.originalTimeAiredStart,
      TimeAiredEnd: filterOptions.originalTimeAiredEnd
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
    const { startTime, endTime } = this.state;
    const { filterOptions, filterKey, applySelection } = this.props;
    const startTimeConvert = moment(startTime, "HH:mm:ss: A").diff(
      moment().startOf("day"),
      "seconds"
    );
    const endTimeConvert = moment(endTime, "HH:mm:ss: A").diff(
      moment().startOf("day"),
      "seconds"
    );
    let exclusions = true;
    // if startTime and endTime are the same as originalStartTime and originalEndDate
    // then set exclusions to false, otherwise set to true
    if (
      startTimeConvert === filterOptions.originalTimeAiredStart &&
      endTimeConvert === filterOptions.originalTimeAiredEnd
    ) {
      exclusions = false;
    }
    const options = {
      TimeAiredStart: startTimeConvert,
      TimeAiredEnd: endTimeConvert
    };
    applySelection({
      filterKey,
      exclusions,
      filterOptions: options
    });
  }

  render() {
    const { startTime, endTime } = this.state;
    return (
      <div>
        <Form horizontal>
          <FormGroup>
            <Col style={{ textAlign: "left" }} className="control-label" sm={4}>
              Start time
            </Col>
            <Col sm={8}>
              <TimePicker
                use12Hours
                format="h:mm a"
                value={startTime}
                defaultValue={startTime}
                onChange={this.handleStartChange}
                // popupClassName="time-picker"
                getPopupContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
          <FormGroup>
            <Col style={{ textAlign: "left" }} className="control-label" sm={4}>
              End time
            </Col>
            <Col sm={8}>
              <TimePicker
                use12Hours
                format="h:mm a"
                value={endTime}
                defaultValue={endTime}
                onChange={this.handleEndChange}
                getPopupContainer={triggerNode => triggerNode.parentNode}
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

FilterTimeInput.defaultProps = {
  applySelection: () => {}
};

FilterTimeInput.propTypes = {
  applySelection: PropTypes.func,
  filterOptions: PropTypes.object.isRequired,
  filterKey: PropTypes.string.isRequired
};

export default CSSModules(FilterTimeInput, styles);
