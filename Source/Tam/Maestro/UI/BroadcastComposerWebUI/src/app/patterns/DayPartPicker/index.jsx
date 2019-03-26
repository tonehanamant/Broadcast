import React, { Component } from "react";
import PropTypes from "prop-types";
import { TimePicker } from "antd";
import moment from "moment";
import CSSModules from "react-css-modules";
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
  dateToString,
  nullDayPart,
  initialDayPart,
  validator
} from "./util";
import styles from "./index.style.scss";

const transformTimeFromSeconds = time =>
  moment()
    .startOf("day")
    .add(time % 86400, "seconds");

const transformDayPart = ({
  Text: text,
  startTime,
  endTime,
  ...days
} = initialDayPart) => {
  const dayPartValue = {
    startTime: transformTimeFromSeconds(startTime),
    endTime: transformTimeFromSeconds(endTime),
    days
  };
  return {
    ...dayPartValue,
    text: text || dateToString(dayPartValue)
  };
};

const generateDayPart = (dayPart, allowEmpty) => {
  if (allowEmpty) {
    return dayPart ? transformDayPart(dayPart) : nullDayPart;
  }
  return transformDayPart(dayPart);
};

const generateInitialState = (dayPart, allowEmpty) => {
  const newDayPart = generateDayPart(dayPart, allowEmpty);
  return {
    ...newDayPart,
    show: false
  };
};

const tranformTime = time =>
  time.seconds(0).diff(moment().startOf("day"), "seconds");

class DayPartPicker extends Component {
  constructor(props) {
    super(props);

    this.state = generateInitialState(props.dayPart, props.allowEmpty);

    this.onDayChange = this.onDayChange.bind(this);
    this.onRangeChange = this.onRangeChange.bind(this);
    this.onTimeChange = this.onTimeChange.bind(this);
    this.onShow = this.onShow.bind(this);
    this.onHide = this.onHide.bind(this);
    this.onApply = this.onApply.bind(this);
    this.editDayPart = this.editDayPart.bind(this);
  }

  componentDidMount() {
    const { applyOnMount, dayPart } = this.props;
    if (applyOnMount) {
      this.onApply(dayPart || transformDayPart(initialDayPart));
    }
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
    const { disabled } = this.props;
    this.setState({ show: !disabled });
  }

  onHide() {
    const { dayPart, allowEmpty } = this.props;
    this.setState(generateInitialState(dayPart, allowEmpty));
  }

  onApply() {
    const { text, days, startTime, endTime } = this.state;
    const { onApply } = this.props;
    onApply({
      Text: text,
      startTime: tranformTime(startTime),
      endTime: tranformTime(endTime),
      ...days
    });
    this.setState({ show: false });
  }

  editDayPart(nextDayPart) {
    this.setState({
      ...nextDayPart,
      text: dateToString(nextDayPart)
    });
  }

  render() {
    const { days, show, startTime, endTime, text } = this.state;
    const { disabled, allowEmpty } = this.props;

    const weekends = transformDaysByWeekends(days, true);
    const weekdays = transformDaysByWeekends(days, false);
    const isValid = validator(days, startTime, endTime);

    return (
      <div styleName="daypart-picker">
        <InputGroup
          onClick={this.onShow}
          styleName="daypicker-input"
          className="read-only-picker"
        >
          <FormControl
            type="text"
            value={text}
            readOnly
            spellCheck="false"
            autoCorrect="off"
            autoComplete="off" // attempt overide chrome
            disabled={disabled}
          />
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
          <div styleName="daypart-picker-overlay">
            <Panel header="Select Weekdays" styleName="days-selectors">
              <ListGroup>
                <ListGroupItem>
                  <div styleName="days-selections-wrap">
                    <div>
                      {daysSelectionsRender(weekdays, this.onDayChange)}
                    </div>
                    <div>
                      {daysSelectionsRender(weekends, this.onDayChange)}
                    </div>
                  </div>
                </ListGroupItem>
                <ListGroupItem styleName="quick-options">
                  <ControlLabel>Quick Options</ControlLabel>
                  {quickOptionsRender(days, this.onRangeChange)}
                </ListGroupItem>
              </ListGroup>
            </Panel>
            <Panel header="Select Time" styleName="time-pickers">
              <Col md={6}>
                <ControlLabel>Start Time</ControlLabel>
                <TimePicker
                  value={startTime}
                  onChange={time => {
                    this.onTimeChange("startTime", time);
                  }}
                  use12Hours
                  format="h:mm a"
                  allowEmpty={allowEmpty}
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
                  format="h:mm a"
                  allowEmpty={allowEmpty}
                  getPopupContainer={() =>
                    document.getElementById("endTimePicker")
                  }
                />
                <div id="endTimePicker" />
              </Col>
            </Panel>
            <Col md={12} styleName="actions-toolbar">
              <hr />
              <ButtonToolbar>
                <Button bsStyle="default" bsSize="small" onClick={this.onHide}>
                  Cancel
                </Button>
                <Button
                  bsStyle="success"
                  bsSize="small"
                  onClick={this.onApply}
                  disabled={!isValid}
                >
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
  dayPart: undefined,
  disabled: false,
  applyOnMount: false,
  allowEmpty: false
};

DayPartPicker.propTypes = {
  dayPart: PropTypes.object,
  onApply: PropTypes.func.isRequired,
  disabled: PropTypes.bool,
  applyOnMount: PropTypes.bool,
  allowEmpty: PropTypes.bool
};

export default CSSModules(DayPartPicker, styles);