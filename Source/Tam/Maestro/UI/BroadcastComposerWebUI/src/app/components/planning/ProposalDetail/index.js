import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Well, Form, FormGroup, ControlLabel } from 'react-bootstrap';

import FlightPicker from 'Components/shared/FlightPicker';


export default class ProposalDetail extends Component {
  constructor(props) {
    super(props);
		this.state = {};
	}

  render() {
		/* eslint-disable no-unused-vars */
		const { detail, proposalEditForm } = this.props;
    return (
			<Well>
				<Form inline>
					<FormGroup controlId="detailFlight">
						<ControlLabel>Flight</ControlLabel>
						<FlightPicker
							startDate={detail.FlightStartDate}
							endDate={detail.FlightEndDate}
						/>
					</FormGroup>
				</Form>
			</Well>
    );
  }
}

ProposalDetail.defaultProps = {
};

ProposalDetail.propTypes = {
	detail: PropTypes.object.isRequired,
	proposalEditForm: PropTypes.object.isRequired,
};
