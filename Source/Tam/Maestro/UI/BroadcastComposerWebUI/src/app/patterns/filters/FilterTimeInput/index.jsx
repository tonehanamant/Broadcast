import React, { Component } from "react";
import PropTypes from "prop-types";
import { Col, Form, ButtonToolbar, Button, FormGroup } from "react-bootstrap";
import { TimePicker } from "antd";
import moment from "moment";
import "./index.css";

class FilterTimeInput extends Component {
  constructor(props) {
    super(props);
    this.state = {
      startTime: null,
      endTime: null,
      originalStartTime: this.props.filterOptions.originalTimeAiredStart,
      originalEndTime: this.props.filterOptions.originalTimeAiredEnd,
      filterOptions: {}
    };
    this.handleStartChange = this.handleStartChange.bind(this);
    this.handleEndChange = this.handleEndChange.bind(this);
    this.apply = this.apply.bind(this);
    this.clear = this.clear.bind(this);
  }

  componentWillMount() {
    console.log("Time Input Mount >>>>", this.props);
    // console.log(moment(moment().startOf('day').seconds(this.props.filterOptions.TimeAiredStart).format('H:mm:ss')));
    // startTime: moment(this.props.filterOptions.TimeAiredStart),
    // endTime: moment(this.props.filterOptions.TimeAiredEnd),
    this.setState({
      originalStartTime: moment(
        moment()
          .startOf("day")
          .seconds(this.props.filterOptions.originalTimeAiredStart)
      ),
      originalEndTime: moment(
        moment()
          .startOf("day")
          .seconds(this.props.filterOptions.originalTimeAiredEnd)
      ),
      // startTime: moment(),
      startTime: moment(
        moment()
          .startOf("day")
          .seconds(this.props.filterOptions.TimeAiredStart)
      ),
      endTime: moment(
        moment()
          .startOf("day")
          .seconds(this.props.filterOptions.TimeAiredEnd)
      )
    });
  }

  handleStartChange(time) {
    console.log(time);
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
    const options = {
      TimeAiredStart: this.props.filterOptions.originalTimeAiredStart,
      TimeAiredEnd: this.props.filterOptions.originalTimeAiredEnd
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
    // const startTime = moment(this.state.startTime).format('hh:mm:ss');
    const startTime = moment(this.state.startTime, "HH:mm:ss: A").diff(
      moment().startOf("day"),
      "seconds"
    );
    const endTime = moment(this.state.endTime, "HH:mm:ss: A").diff(
      moment().startOf("day"),
      "seconds"
    );
    let exclusions = true;
    // if startTime and endTime are the same as originalStartTime and originalEndDate
    // then set exclusions to false, otherwise set to true
    if (
      startTime === this.props.filterOptions.originalTimeAiredStart &&
      endTime === this.props.filterOptions.originalTimeAiredEnd
    ) {
      exclusions = false;
    }
    const options = {
      TimeAiredStart: startTime,
      TimeAiredEnd: endTime
    };
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
              Start time
            </Col>
            <Col sm={8}>
              <TimePicker
                use12Hours
                format="h:mm a"
                value={this.state.startTime}
                defaultValue={this.state.startTime}
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
                value={this.state.endTime}
                defaultValue={this.state.endTime}
                onChange={this.handleEndChange}
                // popupClassName="time-picker"
                getPopupContainer={triggerNode => triggerNode.parentNode}
              />
            </Col>
          </FormGroup>
        </Form>
        <ButtonToolbar className="pull-right" style={{ margin: "0 0 8px 0" }}>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            onClick={this.clear}
            // disabled={!canFilter}
          >
            Clear
          </Button>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            // disabled={!canFilter || !this.state.isValidSelection}
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

export default FilterTimeInput;
