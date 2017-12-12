import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { DayPickerRangeController } from 'react-dates';
import moment from 'moment';
/* eslint-disable no-unused-vars */
import { Row, Col, Well, Form, FormGroup, Panel, ListGroup, ListGroupItem, Checkbox, ButtonToolbar, Button, InputGroup, FormControl, Overlay } from 'react-bootstrap';

// import 'react-dates/initialize'; // @root index.jsx
// import 'react-dates/lib/css/_datepicker.css'; // @root index.jsx
import './style.css';

const isValidDate = date => !!((date && moment.isMoment(date) && moment(date).isValid()));

const isoWeekStart = moment().endOf('isoweek').add(1, 'days').startOf('isoweek');
const isoWeekEndFuture = moment().add(4, 'weeks').endOf('isoweek');


export default class FlightPicker extends Component {
  constructor(props) {
		super(props);
		this.state = {
			show: false,
			startDate: this.props.startDate ? moment(this.props.startDate) : isoWeekStart,
			endDate: this.props.endDate ? moment(this.props.endDate) : isoWeekEndFuture,
			focusedInput: 'startDate',
			inputStartDate: moment(this.props.startDate || isoWeekStart).format('M/D/YYYY'),
			inputEndDate: moment(this.props.endDate || isoWeekEndFuture).format('M/D/YYYY'),
			FlightWeeks: this.props.flightWeeks || [],
			validationStates: {
				startDate: null,
				endDate: null,
			},
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

	componentDidUpdate() {
		// Set flight weeks if not default property = []
		if (this.state.FlightWeeks.length === 0) {
			this.setFlightWeeks(isoWeekStart, isoWeekEndFuture);
		}
	}

	toggle() {
		this.setState({ show: !this.state.show });
	}

	setStartDate(value) {
		const date = moment(value, 'M/D/YYYY', true).isValid() ? value : null;
		let startDate = moment(date).startOf('isoweek');
		const inputStartDate = moment(date).startOf('isoweek').format('M/D/YYYY');

		if (moment.isMoment(startDate) && !startDate.isValid()) { startDate = null; }

		this.setState({ startDate, inputStartDate }, this.setFlightWeeks(startDate, this.state.endDate));

		if (date && moment(date).isAfter(this.state.endDate)) {
			const endDate = null; // moment(date).endOf('isoweek');
			const inputEndDate = moment(null).format('M/D/YYYY'); // moment(date).endOf('isoweek').format('M/D/YYYY');
			this.setState({ endDate, inputEndDate }, this.setFlightWeeks(this.state.startDate, endDate));
			this.setValidationState('endDate', 'error');
		}

		if (isValidDate(date) && moment.isMoment(startDate)) {
			this.inputEndDate.focus();
			this.setState({ focusedInput: 'endDate' });
		}

		this.setValidationState('startDate', isValidDate(date) ? null : 'error');
	}

	setEndDate(value) {
		const date = moment(value, 'M/D/YYYY', true).isValid() ? value : null;
		let endDate = moment(date).endOf('isoweek');
		const inputEndDate = moment(date).endOf('isoweek').format('M/D/YYYY');

		if (moment.isMoment(endDate) && !endDate.isValid()) { endDate = null; }

		this.setState({ endDate, inputEndDate }, this.setFlightWeeks(this.state.startDate, endDate));

		if (date && moment(date).isBefore(this.state.startDate)) {
			const startDate = null; // moment(date).startOf('isoweek');
			const inputStartDate = moment(null).format('M/D/YYYY'); // moment(date).startOf('isoweek').format('M/D/YYYY');
			this.setState({ startDate, inputStartDate }, this.setFlightWeeks(startDate, this.state.endDate));
			this.setValidationState('startDate', 'error');
		}

		if (isValidDate(date) && moment.isMoment(endDate)) {
			this.inputStartDate.focus();
			this.setState({ focusedInput: 'startDate' });
		}

		this.setValidationState('endDate', isValidDate(date) ? null : 'error');
	}

	setFlightWeeks(start, end) {
		if (moment.isMoment(start) && moment.isMoment(end)) {
			const weeks = end.diff(start, 'weeks');
			const FlightWeeks = [];
			/* eslint-disable no-plusplus */
			for (let i = 0; i <= weeks; i++) {
				FlightWeeks.push({
					// Id: i,
					StartDate: moment(start).add(i, 'weeks'),
					EndDate: moment(start).add(i, 'weeks').add(6, 'days'),
					IsHiatus: false,
					MediaWeekId: i,
				});
			}
			this.setState({ FlightWeeks });
		}
	}

	updateFlightWeekHiatus(weekId) {
		const FlightWeeks = [...this.state.FlightWeeks];
		const adjustedFlightWeeks = [];

		FlightWeeks.map((week) => {
			const adjustedWeek = { ...week };
			if (week.MediaWeekId === weekId) { adjustedWeek.IsHiatus = !week.IsHiatus; }
			adjustedFlightWeeks.push(adjustedWeek);
			return adjustedWeek;
		});

		this.setState({ FlightWeeks: adjustedFlightWeeks });
	}

	checkValid() {
		return moment.isMoment(this.state.startDate).isValid() && moment.isMoment(this.state.endDate).isValid();
	}

	clearValidationStates() {
    this.setState({
      validationStates: {
        startDate: null,
        endDate: null,
      },
    });
  }

  setValidationState(type, state) {
    this.state.validationStates[type] = state;
  }

	onApply(event) {
		const parsedStartDate = moment.utc(this.state.startDate).format();
		const parsedEndDate = moment.utc(this.state.endDate).format();
		const parsedFlightWeeks = this.state.FlightWeeks.map((flightWeek) => {
			const parsedFlightWeek = {
				...flightWeek,
				StartDate: moment.utc(flightWeek.StartDate).format(),
				EndDate: moment.utc(flightWeek.EndDate).format(),
			};
			return parsedFlightWeek;
		});

		this.props.onApply({
			StartDate: parsedStartDate,
			EndDate: parsedEndDate,
			FlightWeeks: parsedFlightWeeks,
		});

		this.toggle();
	}

	resetOrRestore() {
		if (this.props.flightWeeks) {
			this.restoreDatesProps();
		} else {
			this.resetDatesDefault();
		}
	}

	restoreDatesProps() {
		this.setState({
			startDate: moment(this.props.startDate) || isoWeekStart,
			endDate: moment(this.props.endDate) || isoWeekEndFuture,
			inputStartDate: moment(this.props.startDate || isoWeekStart).format('M/D/YYYY'),
			inputEndDate: moment(this.props.endDate || isoWeekEndFuture).format('M/D/YYYY'),
			FlightWeeks: this.props.flightWeeks,
		});
	}

	resetDatesDefault() {
		this.setState({
			startDate: isoWeekStart,
			endDate: isoWeekEndFuture,
			inputStartDate: moment(isoWeekStart).format('M/D/YYYY'),
			inputEndDate: moment(isoWeekEndFuture).format('M/D/YYYY'),
		});
		this.setFlightWeeks(isoWeekStart, isoWeekEndFuture);
	}

  render() {
    return (
			<div
				id="flight-picker"
				style={{ position: 'relative', display: 'inline' }}
			>
				<FormGroup validationState={(this.state.validationStates.startDate || this.state.validationStates.endDate === 'error' ? 'error' : null)}>
				<InputGroup onClick={this.toggle}>
						<FormControl
							type="text"
							value={`${moment(this.state.startDate).format('M/D/YYYY')} - ${moment(this.state.endDate).format('M/D/YYYY')}`}
							onChange={() => null}
							inputRef={(ref) => { this.input = ref; }}
						/>
						<InputGroup.Addon><span className="glyphicon glyphicon-calendar" aria-hidden="true" /></InputGroup.Addon>
				</InputGroup>
				</FormGroup>
				<Overlay
					show={this.state.show}
					onHide={() => this.setState({ show: false })}
					placement="bottom"
					container={this}
					target={this.input}
					shouldUpdatePosition={false}
					rootClose
				>
					<div
						style={{
							position: 'absolute',
							backgroundColor: '#FFF',
							boxShadow: '0 5px 10px rgba(0, 0, 0, 0.2)',
							border: '1px solid #CCC',
							borderRadius: 3,
							marginTop: 5,
							marginBottom: 60,
							padding: 10,
							zIndex: 99,
							width: 938,
						}}
						// arrowOffsetLeft={null}
						// arrowOffsetTop={null}
						// positionLeft={null}
						// positionTop={null}
					>
						<Row>
							<Col md={8}>
								<Row style={{ marginBottom: 10 }}>
									<Col md={12} style={{ paddingRight: 0 }}>
										<FormGroup style={{ width: '49%' }} validationState={this.state.validationStates.startDate}>
											<InputGroup style={{ width: '100%' }}>
												<InputGroup.Addon><span className="glyphicon glyphicon-calendar" aria-hidden="true" /></InputGroup.Addon>
												<FormControl
													type="text"
													value={this.state.inputStartDate}
													autoFocus
													onFocus={() => this.setState({ focusedInput: 'startDate' })}
													onChange={(event) => {
														this.setState({ inputStartDate: event.target.value });
														this.setValidationState('startDate', isValidDate(event.target.value) ? null : 'error');
													}}
													onKeyPress={(event) => {
														if (event.key === 'Enter') { this.setStartDate(event.target.value); }
													}}
													onBlur={(event) => {
														// this.setStartDate(event.target.value);
													}}
													inputRef={(ref) => { this.inputStartDate = ref; }}
													style={{ border: this.state.focusedInput === 'startDate' ? '1px solid #66afe9' : null }}
												/>
											</InputGroup>
										</FormGroup>
										<FormGroup style={{ width: '49%', float: 'right' }} validationState={this.state.validationStates.endDate}>
											<InputGroup style={{ width: '100%' }}>
												<InputGroup.Addon><span className="glyphicon glyphicon-calendar" aria-hidden="true" /></InputGroup.Addon>
												<FormControl
													type="text"
													value={this.state.inputEndDate}
													onFocus={() => this.setState({ focusedInput: 'endDate' })}
													onChange={(event) => {
														this.setState({ inputEndDate: event.target.value });
														this.setValidationState('endDate', isValidDate(event.target.value) ? null : 'error');
													}}
													onKeyPress={(event) => {
														if (event.key === 'Enter') { this.setEndDate(event.target.value); }
													}}
													onBlur={(event) => {
														// this.setEndDate(event.target.value);
													}}
													inputRef={(ref) => { this.inputEndDate = ref; }}
													style={{ border: this.state.focusedInput === 'endDate' ? '1px solid #66afe9' : null }}
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

									startDate={this.state.startDate} // momentPropTypes.momentObj or null,
									endDate={this.state.endDate} // momentPropTypes.momentObj or null,

									onDatesChange={({ startDate, endDate }) => {
										if (this.state.focusedInput === 'startDate') { this.setStartDate(startDate); }
										if (this.state.focusedInput === 'endDate') { this.setEndDate(endDate || startDate); }
									}}

									focusedInput={this.state.focusedInput} // PropTypes.oneOf([START_DATE, END_DATE]) or null,
									onFocusChange={focusedInput => this.setState({ focusedInput })}
								/>
							</Col>
							<Col md={4}>
								<Panel header="Flight Weeks" style={{ marginBotton: 10 }}>
									<ListGroup fill style={{ minHeight: 250, maxHeight: 250, overflow: 'auto' }}>
									{this.state.FlightWeeks.map(week => (
										<ListGroupItem key={week.MediaWeekId} style={{ padding: 10 }}>
											<Button bsSize="xsmall" className={'flight-week-btn'} style={{ width: '100%' }}>
												<Checkbox
													checked={!week.IsHiatus}
													style={{ width: '100%' }}
													onClick={() => { this.updateFlightWeekHiatus(week.MediaWeekId); }}
												>
													{moment(week.StartDate).format('M/D/YYYY')} - {moment(week.EndDate).format('M/D/YYYY')}
												</Checkbox>
											</Button>
										</ListGroupItem>
									))}
									</ListGroup>
								</Panel>
								<ButtonToolbar style={{ float: 'right' }}>
									<Button
										bsStyle="default"
										bsSize="small"
										onClick={() => {
											this.toggle();
											this.clearValidationStates();
                      this.resetOrRestore();
										}}
									>Cancel</Button>
									<Button
										bsStyle="success"
										bsSize="small"
										onClick={this.onApply}
										disabled={this.state.validationStates.startDate || this.state.validationStates.endDate === 'error'}
									>Apply</Button>
								</ButtonToolbar>
							</Col>
						</Row>
					</div>
				</Overlay>
			</div>
    );
  }
}

// ref={(ref) => { this.input = ref; }}

FlightPicker.defaultProps = {
	startDate: moment(),
	endDate: moment(),
	flightWeeks: [],
	onApply: () => {},
};

FlightPicker.propTypes = {
	startDate: PropTypes.string,
	endDate: PropTypes.string,
	flightWeeks: PropTypes.array,
	onApply: PropTypes.func,
};
