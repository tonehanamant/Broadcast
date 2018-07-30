import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { TimePicker } from 'antd';
import moment from 'moment';

/* eslint-disable no-unused-vars */
/* eslint-disable no-plusplus */

import { Row, Col, Well, Form, FormGroup, Panel, ListGroup, ListGroupItem, Checkbox, Radio, ButtonToolbar, Button, InputGroup, ControlLabel, FormControl, HelpBlock, Overlay } from 'react-bootstrap';

// required field
// standard daypart picker allowing user to select days of week and time of day.
// User has option to select all days, weekdays, weekends.
// Time shows each hour denoted with am/pm.
// Minutes are in 30 min increments.
// User can type in minutes to 1 min granularity.

const timeToString = (time) => {
	let seconds = time % 60;
			seconds = parseInt(seconds, 10); // radix 10
	let minutes = (time / 60) % 60;
			minutes = parseInt(minutes, 10);
	let hours = (time / (60 * 60)) % 24;
			hours = parseInt(hours, 10);

	const t = new Date(1970, 0, 2, hours, minutes, seconds, 0);

	return moment(t).format('h:mmA').replace(':00', '');
};

const dateToString = (dateObj) => {
	const builder = [];
	const daysBuilder = [];
	const dayAbbreviationStrings = ['M', 'TU', 'W', 'TH', 'F', 'SA', 'SU'];
	const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
	let startDay = 0;
	let endDay = 0;
	let currentDay = 0;

	while (currentDay <= 6) {
			if (dateObj[days[currentDay]]) {
					startDay = currentDay;
					while (++currentDay <= 6 && dateObj[days[currentDay]]) { endDay = currentDay; }

					const aux = daysBuilder.length > 0 ? ',' : '';
					if (endDay > startDay) {
							daysBuilder.push(`${aux + dayAbbreviationStrings[startDay]}-${dayAbbreviationStrings[endDay]}`);
					} else {
							daysBuilder.push(aux + dayAbbreviationStrings[startDay]);
					}
			} else { currentDay++; }
	}

	builder.push(daysBuilder.join(''));
	builder.push(`${timeToString(dateObj.StartTime)}-${timeToString(dateObj.EndTime)}`);
	return builder.join(' ');
};


export default class DayPartPicker extends Component {
  constructor(props) {
		super(props);
		this.state = {
			show: false,
			startTime: moment().startOf('day').add(this.props.dayPart.startTime, 'seconds'),
			startTimeSeconds: this.props.dayPart.startTime,
			endTime: moment().startOf('day').add(this.props.dayPart.endTime, 'seconds'),
			endTimeSeconds: this.props.dayPart.endTime,
			text: this.props.dayPart.Text,
			mon: this.props.dayPart.mon,
			tue: this.props.dayPart.tue,
			wed: this.props.dayPart.wed,
			thu: this.props.dayPart.thu,
			fri: this.props.dayPart.fri,
			sat: this.props.dayPart.sat,
			sun: this.props.dayPart.sun,
			everyday: false,
			weekdays: false,
			weekends: false,
		};
		this.toggle = this.toggle.bind(this);
		this.onStartTimeChange = this.onStartTimeChange.bind(this);
		this.onEndTimeChange = this.onEndTimeChange.bind(this);
		this.onDaySelect = this.onDaySelect.bind(this);
		this.onQuickOptionSelect = this.onQuickOptionSelect.bind(this);
		this.setQuickOptions = this.setQuickOptions.bind(this);
		this.updateInput = this.updateInput.bind(this);
		this.onApply = this.onApply.bind(this);
	}

	toggle() {
		if (this.props.isReadOnly) return;
		if (this.state.show === false) {
			this.updateInput();
		}
		this.setState({ show: !this.state.show });
	}

	resetDaypartDefault() {
		this.setState(
			{
				startTime: moment().startOf('day').add(this.props.dayPart.startTime, 'seconds'),
				startTimeSeconds: this.props.dayPart.startTime,
				endTime: moment().startOf('day').add(this.props.dayPart.endTime, 'seconds'),
				endTimeSeconds: this.props.dayPart.endTime,
				text: this.props.dayPart.Text,
				mon: this.props.dayPart.mon,
				tue: this.props.dayPart.tue,
				wed: this.props.dayPart.wed,
				thu: this.props.dayPart.thu,
				fri: this.props.dayPart.fri,
				sat: this.props.dayPart.sat,
				sun: this.props.dayPart.sun,
				everyday: false,
				weekdays: false,
				weekends: false,
			},
			() => this.setQuickOptions(),
		);
	}

	onStartTimeChange(time, timeString) {
		const timeSeconds = moment(time).diff(moment().startOf('day'), 'seconds');
		this.setState({
			startTime: time,
			startTimeSeconds: timeSeconds,
		}, () => {
			this.updateInput();
		});
	}

	onEndTimeChange(time, timeString) {
		const timeSeconds = moment(time).diff(moment(time).startOf('day'), 'seconds');
		this.setState({
			endTime: time,
			endTimeSeconds: timeSeconds,
		}, () => {
			this.updateInput();
		});
	}

	onDaySelect(day) {
		this.setState(
			{ [day]: !this.state[day] },
			() => {
				this.setQuickOptions();
				this.updateInput();
			},
		);
	}

	onQuickOptionSelect(selection) {
		switch (selection) {
			case 'everyday':
				this.setState({
					mon: true,
					tue: true,
					wed: true,
					thu: true,
					fri: true,
					sat: true,
					sun: true,
					everyday: true,
					weekdays: false,
					weekends: false,
				}, () => {
					this.updateInput();
				});
				break;
			case 'weekdays':
				this.setState({
					mon: true,
					tue: true,
					wed: true,
					thu: true,
					fri: true,
					sat: false,
					sun: false,
					everyday: false,
					weekdays: true,
					weekends: false,
				}, () => {
					this.updateInput();
				});
				break;
			case 'weekends':
				this.setState({
					mon: false,
					tue: false,
					wed: false,
					thu: false,
					fri: false,
					sat: true,
					sun: true,
					everyday: false,
					weekdays: false,
					weekends: true,
				}, () => {
					this.updateInput();
				});
				break;
			default: break;
		}
	}

	setQuickOptions() {
		if (this.state.mon === true &&
					this.state.tue === true &&
					this.state.wed === true &&
					this.state.thu === true &&
					this.state.fri === true &&
					this.state.sat === true &&
					this.state.sun === true) {
			this.setState({
				everyday: true,
				weekdays: false,
				weekends: false,
			});
		} else if (this.state.mon &&
					this.state.tue === true &&
					this.state.wed === true &&
					this.state.thu === true &&
					this.state.fri === true &&
					this.state.sat === false &&
					this.state.sun === false) {
			this.setState({
				everyday: false,
				weekdays: true,
				weekends: false,
			});
		} else if (this.state.mon === false &&
					this.state.tue === false &&
					this.state.wed === false &&
					this.state.thu === false &&
					this.state.fri === false &&
					this.state.sat === true &&
					this.state.sun === true) {
			this.setState({
				everyday: false,
				weekdays: false,
				weekends: true,
			});
		} else {
			this.setState({
				everyday: false,
				weekdays: false,
				weekends: false,
			});
		}
	}

	updateInput() {
		const dateObj = {
			StartTime: this.state.startTimeSeconds,
			EndTime: this.state.endTimeSeconds,
			Mon: this.state.mon,
			Tue: this.state.tue,
			Wed: this.state.wed,
			Thu: this.state.thu,
			Fri: this.state.fri,
			Sat: this.state.sat,
			Sun: this.state.sun,
		};
		this.setState({
			text: dateToString(dateObj),
		});
	}

	onApply() {
		const daypart = {
			Text: this.state.text,
			startTime: this.state.startTimeSeconds,
			endTime: this.state.endTimeSeconds,
			mon: this.state.mon,
			tue: this.state.tue,
			wed: this.state.wed,
			thu: this.state.thu,
			fri: this.state.fri,
			sat: this.state.sat,
			sun: this.state.sun,
		};
		this.props.onApply(daypart);
		this.toggle();
	}

	componentDidMount() {
		this.setQuickOptions();
	}

  render() {
    return (
			<div
				id="daypart-picker"
				style={{ position: 'relative', display: 'inline' }}
			>
				<FormGroup validationState={this.props.validationState}>
					<ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart</ControlLabel>
					<InputGroup onClick={this.toggle}>
						<FormControl
              style={{ width: '220px' }}
							type="text"
							value={this.state.text}
							onChange={this.props.onApply}
							inputRef={(ref) => { this.input = ref; }}
							disabled={this.props.isReadOnly}
						/>
						<InputGroup.Addon><span className="glyphicon glyphicon-time" aria-hidden="true" /></InputGroup.Addon>
					</InputGroup>
					{this.props.validationState != null &&
						<HelpBlock style={{ margin: '0 0 0 16px' }}>
							<span className="text-danger" style={{ fontSize: 11 }}>Required.</span>
						</HelpBlock>
					}
				</FormGroup>
				<Overlay
					show={this.state.show}
					onHide={() => {
						this.setState({ show: false });
						this.resetDaypartDefault();
					}}
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
							width: 510,
						}}
						// arrowOffsetLeft={null}
						// arrowOffsetTop={null}
						// positionLeft={null}
						// positionTop={null}
					>
						<Row>
							<Col md={12}>
								<Panel header="Select Weekdays" style={{ marginBottom: 10 }}>
									<ListGroup fill>
										<ListGroupItem>
											<Row>
												<Col md={3}>
													<Checkbox checked={this.state.mon} onChange={() => { this.onDaySelect('mon'); }}>
														Monday
													</Checkbox>
													<Checkbox checked={this.state.tue} onChange={() => { this.onDaySelect('tue'); }}>
														Tuesday
													</Checkbox>
												</Col>
												<Col md={3}>
													<Checkbox checked={this.state.wed} onChange={() => { this.onDaySelect('wed'); }}>
														Wednesday
													</Checkbox>
													<Checkbox checked={this.state.thu} onChange={() => { this.onDaySelect('thu'); }}>
														Thursday
													</Checkbox>
												</Col>
												<Col md={3}>
													<Checkbox checked={this.state.fri} onChange={() => { this.onDaySelect('fri'); }}>
														Friday
													</Checkbox>
												</Col>
												<Col md={3}>
													<Checkbox checked={this.state.sat} onChange={() => { this.onDaySelect('sat'); }}>
														Saturday
													</Checkbox>
													<Checkbox checked={this.state.sun} onChange={() => { this.onDaySelect('sun'); }}>
														Sunday
													</Checkbox>
												</Col>
											</Row>
										</ListGroupItem>
										<ListGroupItem>
											<Row>
												<Col md={12}>
													<FormGroup controlId="quickOptions" validationState={null}>
														<ControlLabel style={{ display: 'block', paddingBottom: 5 }}>Quick Options</ControlLabel>
														<Row>
															<Col md={4}>
															<Radio inline checked={this.state.everyday} onChange={() => { this.onQuickOptionSelect('everyday'); }}>
																Everyday
															</Radio>
															</Col>
															<Col md={4}>
															<Radio inline checked={this.state.weekdays} onChange={() => { this.onQuickOptionSelect('weekdays'); }}>
																Weekdays
															</Radio>
															</Col>
															<Col md={4}>
															<Radio inline checked={this.state.weekends} onChange={() => { this.onQuickOptionSelect('weekends'); }}>
																Weekends
															</Radio>
															</Col>
														</Row>
													</FormGroup>
												</Col>
											</Row>
										</ListGroupItem>
									</ListGroup>
								</Panel>
							</Col>
						</Row>
						<Row>
							<Col md={12}>
								<Panel header="Select Time" style={{ marginBottom: 10 }}>
									<Row>
										<Col md={6}>
											<FormGroup controlId="startTime" validationState={null}>
												<ControlLabel style={{ display: 'block', paddingBottom: 5 }}>Start Time</ControlLabel>
												<TimePicker
													value={this.state.startTime}
													onChange={this.onStartTimeChange}
													use12Hours
													format={'h:mm a'}
													allowEmpty={false}
													getPopupContainer={() => document.getElementById('startTimePicker')}
												/>
											</FormGroup>
											<div id="startTimePicker" />
										</Col>
										<Col md={6}>
											<FormGroup controlId="endTime" validationState={null}>
												<ControlLabel style={{ display: 'block', paddingBottom: 5 }}>End Time</ControlLabel>
												<TimePicker
													value={this.state.endTime}
													onChange={this.onEndTimeChange}
													use12Hours
													format={'h:mm a'}
													allowEmpty={false}
													getPopupContainer={() => document.getElementById('endTimePicker')}
												/>
											</FormGroup>
											<div id="endTimePicker" />
										</Col>
									</Row>
								</Panel>
							</Col>
						</Row>
						<Row>
							<Col md={12}>
								<hr style={{ marginTop: 0 }} />
								<ButtonToolbar style={{ float: 'right' }}>
									<Button
										bsStyle="default"
										bsSize="small"
										onClick={() => {
											this.toggle();
											this.resetDaypartDefault();
										}}
									>Cancel</Button>
									<Button
										bsStyle="success"
										bsSize="small"
										onClick={this.onApply}
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

DayPartPicker.defaultProps = {
	dayPart: {
		Text: '',
		endTime: 0,
		startTime: 0,
		mon: true,
		tue: true,
		wed: true,
		thu: true,
		fri: true,
		sat: true,
		sun: true,
	},
	isReadOnly: false,
	validationState: null,
};

DayPartPicker.propTypes = {
	dayPart: PropTypes.object,
	onApply: PropTypes.func.isRequired,
	isReadOnly: PropTypes.bool,
	validationState: PropTypes.oneOfType([PropTypes.string]),
};
