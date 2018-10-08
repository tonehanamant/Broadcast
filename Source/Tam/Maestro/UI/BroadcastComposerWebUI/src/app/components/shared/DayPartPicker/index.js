import React, { Component } from "react";
import PropTypes from "prop-types";

import { TimePicker } from "antd";
import moment from "moment";

import {
  Col,
  Panel,
  ListGroup,
  ListGroupItem,
  ButtonToolbar,
  Button,
  InputGroup,
  ControlLabel,
  FormControl,
  Overlay
} from "react-bootstrap";

import {
  daysSelectionsRender,
  transformDaysByWeekends,
  quickOptionsRender,
  dateToString
} from "./util";
import "./index.scss";

const transformDayPart = ({ Text: text, startTime, endTime, ...days }) => {
  const dayPartValue = {
    startTime: moment()
      .startOf("day")
      .add(startTime, "seconds"),
    endTime: moment()
      .startOf("day")
      .add(endTime, "seconds"),
    days
  };
  return {
    ...dayPartValue,
    text: text || dateToString(dayPartValue)
  };
};

const generateInitialState = dayPart => {
  const newDayPart = transformDayPart(dayPart);
  return {
    ...newDayPart,
    show: false
  };
};

export default class DayPartPicker extends Component {
  constructor(props) {
    super(props);

    this.state = generateInitialState(props.dayPart);

    this.onDayChange = this.onDayChange.bind(this);
    this.onRangeChange = this.onRangeChange.bind(this);
    this.onTimeChange = this.onTimeChange.bind(this);
    this.onShow = this.onShow.bind(this);
    this.onHide = this.onHide.bind(this);
    this.onApply = this.onApply.bind(this);
    this.editDayPart = this.editDayPart.bind(this);
  }

  editDayPart(nextDayPart) {
    this.setState({
      ...nextDayPart,
      text: dateToString(nextDayPart)
    });
  }

  onDayChange(name, value) {
    const { days, startTime, endTime } = this.state;
    const nextDayPart = {
      days: { ...days, [name]: value },
      startTime,
      endTime
    };
    this.editDayPart(nextDayPart);
  }

  onRangeChange(values) {
    const { startTime, endTime } = this.state;
    const nextDayPart = { days: values, startTime, endTime };
    this.editDayPart(nextDayPart);
  }

  onTimeChange(name, time) {
    const { days, startTime, endTime } = this.state;
    const nextDayPart = { days, startTime, endTime, [name]: time };
    this.editDayPart(nextDayPart);
  }

  onShow() {
    this.setState({ show: true });
  }

  onHide() {
    const { dayPart } = this.props;
    this.setState(generateInitialState(dayPart));
  }

  onApply() {
    const { text, days, startTime, endTime } = this.state;
    this.props.onApply({
      Text: text,
      startTime: moment(startTime).diff(moment().startOf("day"), "seconds"),
      endTime: moment(endTime).diff(moment().startOf("day"), "seconds"),
      ...days
    });
    this.setState({ show: false });
  }

  render() {
    const { days, show, startTime, endTime, text } = this.state;
    const { disabled } = this.props;

    const weekends = transformDaysByWeekends(days, true);
    const weekdays = transformDaysByWeekends(days, false);

    return (
      <div className="daypart-picker">
        <InputGroup onClick={this.onShow} className="daypicker-input">
          <FormControl type="text" value={text} disabled={disabled} />
          <InputGroup.Addon>
            <span className="glyphicon glyphicon-time" aria-hidden="true" />
          </InputGroup.Addon>
        </InputGroup>
        <Overlay
          show={show}
          onHide={this.onHide}
          placement="bottom"
          container={this}
          target={this.input}
          shouldUpdatePosition={false}
          rootClose
        >
          <div className="daypart-picker-overlay">
            <Panel header="Select Weekdays" className="days-selectors">
              <ListGroup>
                <ListGroupItem>
                  <div className="days-selections-wrap">
                    <div>
                      {daysSelectionsRender(weekdays, this.onDayChange)}
                    </div>
                    <div>
                      {daysSelectionsRender(weekends, this.onDayChange)}
                    </div>
                  </div>
                </ListGroupItem>
                <ListGroupItem className="quick-options">
                  <ControlLabel>Quick Options</ControlLabel>
                  {quickOptionsRender(days, this.onRangeChange)}
                </ListGroupItem>
              </ListGroup>
            </Panel>
            <Panel header="Select Time" className="time-pickers">
              <Col md={6}>
                <ControlLabel>Start Time</ControlLabel>
                <TimePicker
                  value={startTime}
                  onChange={time => {
                    this.onTimeChange("startTime", time);
                  }}
                  use12Hours
                  format={"h:mm a"}
                  allowEmpty={false}
                  getPopupContainer={() =>
                    document.getElementById("startTimePicker")
                  }
                />
                <div id="startTimePicker" />
              </Col>
              <Col md={6}>
                <ControlLabel>End Time</ControlLabel>
                <TimePicker
                  value={endTime}
                  onChange={time => {
                    this.onTimeChange("endTime", time);
                  }}
                  use12Hours
                  format={"h:mm a"}
                  allowEmpty={false}
                  getPopupContainer={() =>
                    document.getElementById("endTimePicker")
                  }
                />
                <div id="endTimePicker" />
              </Col>
            </Panel>
            <Col md={12} className="actions-toolbar">
              <hr />
              <ButtonToolbar>
                <Button bsStyle="default" bsSize="small" onClick={this.onHide}>
                  Cancel
                </Button>
                <Button bsStyle="success" bsSize="small" onClick={this.onApply}>
                  Apply
                </Button>
              </ButtonToolbar>
            </Col>
          </div>
        </Overlay>
      </div>
    );
  }
}

DayPartPicker.defaultProps = {
  dayPart: {
    Text: "",
    endTime: 0,
    startTime: 0,
    mon: true,
    tue: true,
    wed: true,
    thu: true,
    fri: true,
    sat: true,
    sun: true
  },
  disabled: false
};

DayPartPicker.propTypes = {
  dayPart: PropTypes.object,
  onApply: PropTypes.func.isRequired,
  disabled: PropTypes.bool
};
