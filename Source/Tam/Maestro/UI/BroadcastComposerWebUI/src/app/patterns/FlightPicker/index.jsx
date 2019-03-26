import React, { Component } from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import { DayPickerRangeController } from "react-dates";
import moment from "moment";
import {
  Row,
  Col,
  FormGroup,
  Panel,
  ListGroup,
  ListGroupItem,
  Checkbox,
  ButtonToolbar,
  Button,
  InputGroup,
  FormControl,
  Overlay
} from "react-bootstrap";

import styles from "./index.style.scss";

const isValidDate = date =>
  !!(date && moment.isMoment(date) && moment(date).isValid());

const isoWeekStart = moment()
  .endOf("isoweek")
  .add(1, "days")
  .startOf("isoweek");
const isoWeekEndFuture = moment()
  .add(4, "weeks")
  .endOf("isoweek");

class FlightPicker extends Component {
  constructor(props) {
    super(props);
    const { startDate, endDate, flightWeeks } = this.props;
    this.state = {
      show: false,
      startDate: startDate ? moment(startDate) : isoWeekStart,
      endDate: endDate ? moment(endDate) : isoWeekEndFuture,
      focusedInput: "startDate",
      inputStartDate: moment(startDate || isoWeekStart).format("M/D/YYYY"),
      inputEndDate: moment(endDate || isoWeekEndFuture).format("M/D/YYYY"),
      FlightWeeks: flightWeeks || [],
      validationStates: {
        startDate: null,
        endDate: null
      }
    };
    this.toggle = this.toggle.bind(this);
    this.setStartDate = this.setStartDate.bind(this);
    this.setEndDate = this.setEndDate.bind(this);
    this.setFlightWeeks = this.setFlightWeeks.bind(this);
    this.updateFlightWeekHiatus = this.updateFlightWeekHiatus.bind(this);
    this.onApply = this.onApply.bind(this);
    this.resetOrRestore = this.resetOrRestore.bind(this);
    this.restoreDatesProps = this.restoreDatesProps.bind(this);
    this.resetDatesDefault = this.resetDatesDefault.bind(this);
  }

  componentWillReceiveProps() {
    this.resetOrRestore();
  }

  componentDidUpdate() {
    const { FlightWeeks } = this.state;
    if (FlightWeeks.length === 0) {
      this.setFlightWeeks(isoWeekStart, isoWeekEndFuture);
    }
  }

  onApply() {
    const { startDate, endDate, FlightWeeks } = this.state;
    const { onApply } = this.props;
    const parsedStartDate = moment(startDate).format();
    const parsedEndDate = moment(endDate).format();
    const parsedFlightWeeks = FlightWeeks.map(flightWeek => {
      const parsedFlightWeek = {
        ...flightWeek,
        StartDate: moment(flightWeek.StartDate).format(),
        EndDate: moment(flightWeek.EndDate).format()
      };
      return parsedFlightWeek;
    });

    onApply({
      StartDate: parsedStartDate,
      EndDate: parsedEndDate,
      FlightWeeks: parsedFlightWeeks
    });

    this.toggle();
    this.resetOrRestore();
  }

  setStartDate(value) {
    const { endDate: eDate, startDate: sDate } = this.state;
    const date = moment(value, "M/D/YYYY", true).isValid() ? value : null;
    let startDate = moment(date).startOf("isoweek");
    const inputStartDate = moment(date)
      .startOf("isoweek")
      .format("M/D/YYYY");

    if (moment.isMoment(startDate) && !startDate.isValid()) {
      startDate = null;
    }

    this.setState(
      { startDate, inputStartDate },
      this.setFlightWeeks(startDate, eDate)
    );

    if (date && moment(date).isAfter(eDate)) {
      const endDate = null;
      const inputEndDate = moment(null).format("M/D/YYYY");
      this.setState(
        { endDate, inputEndDate },
        this.setFlightWeeks(sDate, endDate)
      );
      this.setValidationState("endDate", "warning");
    }

    if (isValidDate(date) && moment.isMoment(startDate)) {
      this.inputEndDate.focus();
      this.setState({ focusedInput: "endDate" });
    }

    this.setValidationState("startDate", isValidDate(date) ? null : "warning");
  }

  setEndDate(value) {
    const { endDate: eDate, startDate: sDate } = this.state;
    const date = moment(value, "M/D/YYYY", true).isValid() ? value : null;
    let endDate = moment(date).endOf("isoweek");
    const inputEndDate = moment(date)
      .endOf("isoweek")
      .format("M/D/YYYY");

    if (moment.isMoment(endDate) && !endDate.isValid()) {
      endDate = null;
    }

    this.setState(
      { endDate, inputEndDate },
      this.setFlightWeeks(sDate, endDate)
    );

    if (date && moment(date).isBefore(sDate)) {
      const startDate = null;
      const inputStartDate = moment(null).format("M/D/YYYY");
      this.setState(
        { startDate, inputStartDate },
        this.setFlightWeeks(startDate, eDate)
      );
      this.setValidationState("startDate", "warning");
    }

    if (isValidDate(date) && moment.isMoment(endDate)) {
      this.inputStartDate.focus();
      this.setState({ focusedInput: "startDate" });
    }

    this.setValidationState("endDate", isValidDate(date) ? null : "warning");
  }

  setFlightWeeks(start, end) {
    if (moment.isMoment(start) && moment.isMoment(end)) {
      const weeks = end.diff(start, "weeks");
      const FlightWeeks = [];
      for (let i = 0; i <= weeks; i += 1) {
        FlightWeeks.push({
          StartDate: moment(start).add(i, "weeks"),
          EndDate: moment(start)
            .add(i, "weeks")
            .add(6, "days"),
          IsHiatus: false,
          MediaWeekId: i
        });
      }
      this.setState({ FlightWeeks });
    }
  }

  setValidationState(type, state) {
    const { validationStates } = this.state;
    validationStates[type] = state;
  }

  clearValidationStates() {
    this.setState({
      validationStates: {
        startDate: null,
        endDate: null
      }
    });
  }

  checkValid() {
    const { startDate, endDate } = this.state;
    return (
      moment.isMoment(startDate).isValid() && moment.isMoment(endDate).isValid()
    );
  }

  updateFlightWeekHiatus(weekId) {
    const { FlightWeeks = [] } = this.state;
    const adjustedFlightWeeks = FlightWeeks.map(week => ({
      ...week,
      IsHiatus: week.MediaWeekId === weekId ? !week.IsHiatus : week.IsHiatus
    }));
    this.setState({ FlightWeeks: adjustedFlightWeeks });
  }

  toggle() {
    const { isReadOnly } = this.props;
    const { show } = this.state;
    if (isReadOnly) return;
    this.setState({ show: !show });
  }

  resetOrRestore() {
    const { flightWeeks } = this.props;
    if (flightWeeks) {
      this.restoreDatesProps();
    } else {
      this.resetDatesDefault();
    }
  }

  restoreDatesProps() {
    const { startDate, endDate, flightWeeks } = this.props;
    this.setState({
      startDate: moment(startDate) || isoWeekStart,
      endDate: moment(endDate) || isoWeekEndFuture,
      inputStartDate: moment(startDate || isoWeekStart).format("M/D/YYYY"),
      inputEndDate: moment(endDate || isoWeekEndFuture).format("M/D/YYYY"),
      FlightWeeks: flightWeeks
    });
  }

  resetDatesDefault() {
    this.setState({
      startDate: isoWeekStart,
      endDate: isoWeekEndFuture,
      inputStartDate: moment(isoWeekStart).format("M/D/YYYY"),
      inputEndDate: moment(isoWeekEndFuture).format("M/D/YYYY")
    });
    this.setFlightWeeks(isoWeekStart, isoWeekEndFuture);
  }

  render() {
    const {
      FlightWeeks,
      validationStates,
      startDate,
      endDate,
      show,
      inputStartDate,
      focusedInput,
      inputEndDate
    } = this.state;
    const { isReadOnly } = this.props;
    return (
      <div
        id="flight-picker"
        style={{ position: "relative", display: "inline" }}
      >
        <FormGroup
          validationState={
            validationStates.startDate || validationStates.endDate === "warning"
              ? "warning"
              : null
          }
        >
          <InputGroup onClick={this.toggle}>
            <FormControl
              bsClass="flight-range-input form-control"
              type="text"
              value={`${moment(startDate).format("M/D/YYYY")} - ${moment(
                endDate
              ).format("M/D/YYYY")}`}
              onChange={() => null}
              inputRef={ref => {
                this.input = ref;
              }}
              disabled={isReadOnly}
            />
            <InputGroup.Addon>
              <span
                className="glyphicon glyphicon-calendar"
                aria-hidden="true"
              />
            </InputGroup.Addon>
          </InputGroup>
        </FormGroup>
        <Overlay
          show={show}
          onHide={() => {
            this.setState({ show: false });
            this.resetOrRestore();
          }}
          placement="bottom"
          container={this}
          target={this.input}
          shouldUpdatePosition={false}
          rootClose
        >
          <div
            style={{
              position: "absolute",
              backgroundColor: "#FFF",
              boxShadow: "0 5px 10px rgba(0, 0, 0, 0.2)",
              border: "1px solid #CCC",
              borderRadius: 3,
              marginTop: 5,
              marginBottom: 60,
              padding: 10,
              zIndex: 99,
              width: 938
            }}
          >
            <Row>
              <Col md={8}>
                <Row style={{ marginBottom: 10 }}>
                  <Col md={12} style={{ paddingRight: 0 }}>
                    <FormGroup
                      style={{ width: "49%" }}
                      validationState={validationStates.startDate}
                    >
                      <InputGroup style={{ width: "100%" }}>
                        <InputGroup.Addon>
                          <span
                            className="glyphicon glyphicon-calendar"
                            aria-hidden="true"
                          />
                        </InputGroup.Addon>
                        <FormControl
                          type="text"
                          value={inputStartDate}
                          autoFocus
                          onFocus={() =>
                            this.setState({ focusedInput: "startDate" })
                          }
                          onChange={event => {
                            this.setState({
                              inputStartDate: event.target.value
                            });
                            this.setValidationState(
                              "startDate",
                              isValidDate(event.target.value) ? null : "warning"
                            );
                          }}
                          onKeyPress={event => {
                            if (event.key === "Enter") {
                              this.setStartDate(event.target.value);
                            }
                          }}
                          inputRef={ref => {
                            this.inputStartDate = ref;
                          }}
                          style={{
                            border:
                              focusedInput === "startDate"
                                ? "1px solid #66afe9"
                                : null
                          }}
                        />
                      </InputGroup>
                    </FormGroup>
                    <FormGroup
                      style={{ width: "49%", float: "right" }}
                      validationState={validationStates.endDate}
                    >
                      <InputGroup style={{ width: "100%" }}>
                        <InputGroup.Addon>
                          <span
                            className="glyphicon glyphicon-calendar"
                            aria-hidden="true"
                          />
                        </InputGroup.Addon>
                        <FormControl
                          type="text"
                          value={inputEndDate}
                          onFocus={() =>
                            this.setState({ focusedInput: "endDate" })
                          }
                          onChange={event => {
                            this.setState({ inputEndDate: event.target.value });
                            this.setValidationState(
                              "endDate",
                              isValidDate(event.target.value) ? null : "warning"
                            );
                          }}
                          onKeyPress={event => {
                            if (event.key === "Enter") {
                              this.setEndDate(event.target.value);
                            }
                          }}
                          inputRef={ref => {
                            this.inputEndDate = ref;
                          }}
                          style={{
                            border:
                              focusedInput === "endDate"
                                ? "1px solid #66afe9"
                                : null
                          }}
                        />
                      </InputGroup>
                    </FormGroup>
                  </Col>
                </Row>
                <DayPickerRangeController
                  numberOfMonths={2}
                  keepOpenOnDateSelect
                  hideKeyboardShortcutsPanel
                  firstDayOfWeek={1}
                  enableOutsideDays
                  startDate={startDate} // momentPropTypes.momentObj or null,
                  endDate={endDate} // momentPropTypes.momentObj or null,
                  onDatesChange={({ startDate, endDate }) => {
                    if (focusedInput === "startDate") {
                      this.setStartDate(startDate);
                    }
                    if (focusedInput === "endDate") {
                      this.setEndDate(endDate || startDate);
                    }
                  }}
                  focusedInput={focusedInput} // PropTypes.oneOf([START_DATE, END_DATE]) or null,
                  onFocusChange={focusedInput =>
                    this.setState({ focusedInput })
                  }
                />
              </Col>
              <Col md={4}>
                <Panel header="Flight Weeks" style={{ marginBotton: 10 }}>
                  <ListGroup
                    fill
                    style={{ minHeight: 250, maxHeight: 250, overflow: "auto" }}
                  >
                    {FlightWeeks.map(week => (
                      <ListGroupItem
                        key={week.MediaWeekId}
                        style={{ padding: 10 }}
                      >
                        <Button
                          bsSize="xsmall"
                          styleName="flight-week-btn"
                          style={{ width: "100%" }}
                        >
                          <Checkbox
                            checked={!week.IsHiatus}
                            style={{ width: "100%" }}
                            onClick={() => {
                              this.updateFlightWeekHiatus(week.MediaWeekId);
                            }}
                          >
                            {moment(week.StartDate).format("M/D/YYYY")} -{" "}
                            {moment(week.EndDate).format("M/D/YYYY")}
                          </Checkbox>
                        </Button>
                      </ListGroupItem>
                    ))}
                  </ListGroup>
                </Panel>
                <ButtonToolbar style={{ float: "right" }}>
                  <Button
                    bsStyle="default"
                    bsSize="small"
                    onClick={() => {
                      this.toggle();
                      this.clearValidationStates();
                      this.resetOrRestore();
                    }}
                  >
                    Cancel
                  </Button>
                  <Button
                    bsStyle="success"
                    bsSize="small"
                    onClick={this.onApply}
                    disabled={
                      validationStates.startDate ||
                      validationStates.endDate === "warning"
                    }
                  >
                    Apply
                  </Button>
                </ButtonToolbar>
              </Col>
            </Row>
          </div>
        </Overlay>
      </div>
    );
  }
}

FlightPicker.defaultProps = {
  startDate: moment(),
  endDate: moment(),
  flightWeeks: [],
  onApply: () => {}
};

FlightPicker.propTypes = {
  startDate: PropTypes.string,
  endDate: PropTypes.string,
  flightWeeks: PropTypes.array,
  onApply: PropTypes.func,
  isReadOnly: PropTypes.bool.isRequired
};

export default CSSModules(FlightPicker, styles);
