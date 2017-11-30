import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { DayPickerRangeController } from 'react-dates';
import moment from 'moment';
/* eslint-disable no-unused-vars */
import { InputGroup, FormControl, Overlay } from 'react-bootstrap';

// import DropdownToggle from './DropdownToggle';
// import DropdownMenu from './DropdownMenu';

export default class FlightPicker extends Component {
  constructor(props) {
		super(props);
		this.state = {
			show: false,
		};
		this.toggle = this.toggle.bind(this);
	}

	toggle() {
    this.setState({ show: !this.state.show });
  }

  render() {
		const { startDate, endDate } = this.props;
    return (
			<div id="flight-picker">
				<div style={{ position: 'relative' }}>
					<InputGroup onClick={this.toggle}>
							<FormControl
								type="text"
								value={`${moment(startDate).format('M/D/YYYY')} - ${moment(endDate).format('M/D/YYYY')}`}
								readOnly
								inputRef={(ref) => { this.input = ref; }}
							/>
						<InputGroup.Addon><span className="glyphicon glyphicon-calendar" aria-hidden="true" /></InputGroup.Addon>
					</InputGroup>
					<Overlay
						show={this.state.show}
						onHide={() => this.setState({ show: false })}
						placement="bottom"
						container={this}
						target={this.input}

					>
						<div
							style={{
								position: 'absolute',
								backgroundColor: '#FFF',
								boxShadow: '0 5px 10px rgba(0, 0, 0, 0.2)',
								border: '1px solid #CCC',
								borderRadius: 3,
								marginTop: 5,
								padding: 10,
								top: 'auto',
								left: 'auto',
							}}
						>
							<DayPickerRangeController
								numberOfMonths={2}
								keepOpenOnDateSelect
								hideKeyboardShortcutsPanel

								startDate={moment(startDate)} // momentPropTypes.momentObj or null,
								endDate={moment(endDate)} // momentPropTypes.momentObj or null,
								onDatesChange={({ startDate, endDate }) => this.setState({ startDate, endDate })} // PropTypes.func.isRequired,
								focusedInput={this.state.focusedInput} // PropTypes.oneOf([START_DATE, END_DATE]) or null,
								onFocusChange={focusedInput => this.setState({ focusedInput })} // PropTypes.func.isRequired,
							/>
						</div>
					</Overlay>
				</div>
			</div>
    );
  }
}

// ref={(ref) => { this.input = ref; }}

FlightPicker.defaultProps = {
	startDate: moment(),
	endDate: moment(),
};

FlightPicker.propTypes = {
	startDate: PropTypes.string,
	endDate: PropTypes.string,
};
