import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Row, Col, Label, FormGroup, InputGroup, ControlLabel, FormControl, HelpBlock } from 'react-bootstrap';
import Select from 'react-select';

import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';
import CurrencyDollarWhole from 'Components/shared/TextFormatters/CurrencyDollarWhole';
import PercentWhole from 'Components/shared/TextFormatters/PercentWhole';
import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';


export default class ProposalForm extends Component {
  constructor(props) {
		super(props);

		this.onChangeProposalName = this.onChangeProposalName.bind(this);
		this.onChangePostType = this.onChangePostType.bind(this);
		this.onChangeEquivalized = this.onChangeEquivalized.bind(this);
		this.onChangeAdvertiserId = this.onChangeAdvertiserId.bind(this);
		this.onChangeGuaranteedDemoId = this.onChangeGuaranteedDemoId.bind(this);
		this.onChangeSecondaryDemos = this.onChangeSecondaryDemos.bind(this);
    this.onChangeNotes = this.onChangeNotes.bind(this);

    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);

		this.state = {
      nameInvalid: null,
      advertiserInvalid: null,
    };
	}

	onChangeProposalName(event) {
    const val = event.target.value ? event.target.value : '';
    this.props.updateProposalEditForm({ key: 'ProposalName', value: val });
    this.setValidationState('nameInvalid', val ? null : 'error');
	}

	onChangePostType(value) {
		this.props.updateProposalEditForm({ key: 'PostType', value: value ? value.Id : null });
	}

	onChangeEquivalized(value) {
		this.props.updateProposalEditForm({ key: 'Equivalized', value: value ? value.Bool : null });
	}

	onChangeAdvertiserId(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditForm({ key: 'AdvertiserId', value: val });
    this.setValidationState('advertiserInvalid', val ? null : 'error');
	}

	onChangeGuaranteedDemoId(value) {
		this.props.updateProposalEditForm({ key: 'GuaranteedDemoId', value: value ? value.Id : null });
	}

	onChangeSecondaryDemos(value) {
		this.props.updateProposalEditForm({ key: 'SecondaryDemos', value: value.map(item => item.Id) });
	}

	onChangeNotes(event) {
		this.props.updateProposalEditForm({ key: 'Notes', value: event.target.value });
  }

  checkValid() {
    const nameValid = (this.props.proposalEditForm.ProposalName != null) && (this.props.proposalEditForm.ProposalName !== '');
    const advertiserValid = this.props.proposalEditForm.AdvertiserId != null;
    if (nameValid && advertiserValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState('nameInvalid', nameValid ? null : 'error');
    this.setValidationState('advertiserInvalid', advertiserValid ? null : 'error');
    return false;
  }

  setValidationState(type, state) {
    this.state[type] = state;
  }

  clearValidationStates() {
    this.setState({
      nameInvalid: null,
      advertiserInvalid: null,
    });
  }

  render() {
		const { initialdata, proposalEditForm } = this.props;
    return (
      <div id="proposal-form">
					<form>
						<Row className="clearfix">
							<Col md={6}>
								<Row>
									<Col md={6}>
										<FormGroup controlId="proposalName" validationState={this.state.nameInvalid} >
											<ControlLabel><strong>Proposal Name</strong></ControlLabel>
											<InputGroup>
												<FormControl
													type="text"
													defaultValue={proposalEditForm.ProposalName || ''}
													onChange={this.onChangeProposalName}
												/>
												{ proposalEditForm.Id &&
														<InputGroup.Addon>
															Id: {proposalEditForm.Id}
														</InputGroup.Addon>
												}
											</InputGroup>
											{this.state.nameInvalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={2}>
									...
									</Col>
									<Col md={2}>
										<FormGroup controlId="proposalPostType" >
											<ControlLabel><strong>Post Type</strong></ControlLabel>
											<Select
												name="proposalPostType"
												value={proposalEditForm.PostType}
												// placeholder=""
												options={initialdata.SchedulePostTypes}
												labelKey="Display"
												valueKey="Id"
												onChange={this.onChangePostType}
												clearable={false}
											/>
										</FormGroup>
									</Col>
									<Col md={2}>
										<FormGroup controlId="proposalEquivalized" >
											<ControlLabel><strong>Equivalized</strong></ControlLabel>
											<Select
												name="proposalEquivalized"
												value={proposalEditForm.Equivalized}
												// placeholder=""
												options={[{ Display: 'Yes', Bool: true }, { Display: 'No', Bool: false }]}
												labelKey="Display"
												valueKey="Bool"
												onChange={this.onChangeEquivalized}
												clearable={false}
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
							<Col md={6}>
								<Row>
									<Col md={3}>
										<FormGroup controlId="proposalTargetCPM">
											<ControlLabel><strong>Target CPM</strong></ControlLabel>
											<FormControl.Static>
												<CurrencyDollarWhole amount={proposalEditForm.TotalCPM} /> / <CurrencyDollarWhole amount={proposalEditForm.TargetCPM} /> <Label bsStyle="success"><PercentWhole percent={proposalEditForm.TotalCPMPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetBudget">
											<ControlLabel><strong>Target Budget</strong></ControlLabel>
											<FormControl.Static>
												<CurrencyDollarWhole amount={proposalEditForm.TotalCost} /> / <CurrencyDollarWhole amount={proposalEditForm.TargetBudget} /> <Label bsStyle="success"><PercentWhole percent={proposalEditForm.TotalCostPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetImpressions">
											<ControlLabel><strong>Target Impressions</strong></ControlLabel>
											<FormControl.Static>
												<NumberCommaWhole number={proposalEditForm.TotalImpressions / 1000} /> / <NumberCommaWhole number={proposalEditForm.TargetImpressions / 1000} /> <Label bsStyle="danger"><PercentWhole percent={proposalEditForm.TotalImpressionsPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetUnits">
											<ControlLabel><strong>Target Units</strong></ControlLabel>
											<FormControl.Static>
												{proposalEditForm.TargetUnits}
											</FormControl.Static>
										</FormGroup>
									</Col>
								</Row>
							</Col>
						</Row>
						<Row className="clearfix">
							<Col md={7}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="proposalAdvertiser" validationState={this.state.advertiserInvalid} >
											<ControlLabel><strong>Advertiser</strong></ControlLabel>
											<Select
												name="proposalAdvertiser"
												value={proposalEditForm.AdvertiserId}
												// placeholder=""
												options={initialdata.Advertisers}
												labelKey="Display"
												valueKey="Id"
												onChange={this.onChangeAdvertiserId}
												clearable={false}
											/>
											{this.state.advertiserInvalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalGuaranteedDemo" >
											<ControlLabel><strong>Guaranteed Demo</strong></ControlLabel>
											<Select
												name="proposalGuaranteedDemo"
												value={proposalEditForm.GuaranteedDemoId}
												// placeholder=""
												options={this.props.initialdata.Audiences}
												labelKey="Display"
												valueKey="Id"
												onChange={this.onChangeGuaranteedDemoId}
												clearable={false}
											/>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalSecondaryDemo" >
											<ControlLabel><strong>Secondary Demo</strong></ControlLabel>
											<Select
												name="proposalSecondaryDemo"
												value={proposalEditForm.SecondaryDemos}
												// placeholder=""
												multi
												options={initialdata.Audiences}
												labelKey="Display"
												valueKey="Id"
												closeOnSelect
												onChange={this.onChangeSecondaryDemos}
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
							<Col md={5}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="proposalSpotLength">
											<ControlLabel><strong>Spot Length</strong></ControlLabel>
											<FormControl.Static>
												{ proposalEditForm.SpotLengths.map((spot, index, arr) => (arr.length !== index ? `${spot.Display}` : `${spot.Display}, `)) }
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalFlight">
											<ControlLabel><strong>Flight</strong></ControlLabel>
											<FormControl.Static>
												<DateMDYYYY date={proposalEditForm.FlightStartDate} /><span> - </span><DateMDYYYY date={proposalEditForm.FlightEndDate} />
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalNotes">
											<ControlLabel>Notes</ControlLabel>
											<FormControl
												componentClass="textarea"
												defaultValue={proposalEditForm.Notes || ''}
												onChange={this.onChangeNotes}
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
						</Row>
					</form>
			</div>
    );
  }
}

ProposalForm.defaultProps = {
};

/* eslint-disable react/no-unused-prop-types */
ProposalForm.propTypes = {
	initialdata: PropTypes.object.isRequired,
	proposalEditForm: PropTypes.object.isRequired,
	updateProposalEditForm: PropTypes.func.isRequired,
};
