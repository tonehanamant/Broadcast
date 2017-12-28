import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, Label, FormGroup, InputGroup, ControlLabel, FormControl, HelpBlock, Tooltip, Glyphicon, Button, OverlayTrigger } from 'react-bootstrap';
import Select from 'react-select';

import moment from 'moment';

import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';
import CurrencyDollarWhole from 'Components/shared/TextFormatters/CurrencyDollarWhole';
import PercentWhole from 'Components/shared/TextFormatters/PercentWhole';
import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';
import MarketGroupSelector from './MarketGroupSelector';

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

    this.setValidationState = this.setValidationState.bind(this);

    this.onMarketGroupChange = this.onMarketGroupChange.bind(this);
    this.marketSelectorOptionRenderer = this.marketSelectorOptionRenderer.bind(this);
    this.marketSelectorValueRenderer = this.marketSelectorValueRenderer.bind(this);
    this.updateMarketCount = this.updateMarketCount.bind(this);

    this.setValidationState = this.setValidationState.bind(this);
    this.onSaveShowValidation = this.onSaveShowValidation.bind(this);
    this.checkValidProposalName = this.checkValidProposalName.bind(this);
    this.checkValidAdvertiserId = this.checkValidAdvertiserId.bind(this);

    this.state = {
      validationStates: {
        Name: null,
        Name_Alphanumeric: null,
        Name_MaxChar: null,
        AdvertiserId: null,
      },
    };
  }

  onChangeProposalName(event) {
    const val = event.target.value;
    this.props.updateProposalEditForm({ key: 'ProposalName', value: val });
    this.checkValidProposalName(val);
  }

  onMarketGroupChange(selectedMarketGroup) {
    const { initialdata, updateProposalEditForm } = this.props;

    let option = selectedMarketGroup;
    if (selectedMarketGroup.Count) {
      option = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === selectedMarketGroup.Id);
    }

    if (option.Id === -1) {
      option = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === 255);
      this.props.toggleModal({
        modal: 'marketSelectorModal',
        active: true,
      });
    } else {
      updateProposalEditForm({ key: 'Markets', value: null });
      updateProposalEditForm({ key: 'MarketGroupId', value: option.Id });
      updateProposalEditForm({ key: 'BlackoutMarketGroupId', value: null });
    }

    this.setState({ selectedMarketGroup: option });
  }

  onChangePostType(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditForm({ key: 'PostType', value: val });
  }

  onChangeEquivalized(value) {
    const val = value ? value.Bool : null;
    this.props.updateProposalEditForm({ key: 'Equivalized', value: val });
  }

  onChangeAdvertiserId(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditForm({ key: 'AdvertiserId', value: val });
    this.checkValidAdvertiserId(val);
  }

  onChangeGuaranteedDemoId(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditForm({ key: 'GuaranteedDemoId', value: val });
  }

  onChangeSecondaryDemos(value) {
    const val = value.map(item => item.Id);
    this.props.updateProposalEditForm({ key: 'SecondaryDemos', value: val });
  }

  onChangeNotes(event) {
    const val = event.target.value;
    this.props.updateProposalEditForm({ key: 'Notes', value: val });
  }

  marketSelectorOptionRenderer(option) {
    let count = option.Count;
    const divStyle = { overflow: 'hidden' };
    const countStyle = { color: '#c0c0c0' };

    // custom
    if (option.Id === 255) {
      count = this.state.customMarketCount;
    }

    // select custom
    const isOpenCustomOption = option.Id === -1;
    if (isOpenCustomOption) {
      countStyle.Display = 'none';
    }

    return (
      <div style={divStyle} href="">
        {isOpenCustomOption ? <hr style={{ margin: '8px' }} /> : null}
        <span className="pull-left">{option.Display}</span>
        <span className="pull-right" style={countStyle}>{count}</span>
      </div>
    );
  }

  marketSelectorValueRenderer() {
    return (
      <div style={{ overflow: 'hidden' }} href="">
        <span className="pull-left ">{this.state.selectedMarketGroup.Display}</span>
      </div>
    );
  }

  updateMarketCount(customMarketCount) {
    // if selector was cleared, assign the first option from initialData (i.e. 'All')
    let selectedMarketGroup = this.props.initialdata.MarketGroups.find(marketgGroup => marketgGroup.Id === 255);
    if (customMarketCount === 0) {
      selectedMarketGroup = this.props.initialdata.MarketGroups[0];
    }

    this.setState({
      selectedMarketGroup,
      customMarketCount,
    });
  }

  setValidationState(type, state) {
    this.setState(prevState => ({
      ...prevState,
      validationStates: {
        ...prevState.validationStates,
        [type]: state,
      },
    }));
  }

  onSaveShowValidation(nextProps) {
    const { ProposalName, AdvertiserId } = nextProps.proposalEditForm;
    this.checkValidProposalName(ProposalName);
    this.checkValidAdvertiserId(AdvertiserId);
  }

  checkValidProposalName(value) {
    const val = value || '';
    this.setValidationState('Name', val !== '' ? null : 'error');
    this.setValidationState('Name', val === '' ? 'error' : null);
    const re = /^[A-Za-z0-9- ]+$/i; // check alphanumeric
    this.setValidationState('Name_Alphanumeric', (re.test(val) || val === '') ? null : 'error');
    this.setValidationState('Name_MaxChar', val.length <= 100 ? null : 'error');
  }

  checkValidAdvertiserId(value) {
    const val = value;
    this.setValidationState('AdvertiserId', val ? null : 'error');
  }

  componentWillMount() {
    const { initialdata, proposalEditForm } = this.props;
    const { MarketGroup, Markets, BlackoutMarketGroup } = proposalEditForm;

    let selectedMarketGroup = MarketGroup;
    let customMarketCount = 0;

    const isCustom = ((Markets && Markets.length > 0) || (MarketGroup && BlackoutMarketGroup));
    if (isCustom) {
      customMarketCount = Markets.length;
      customMarketCount += MarketGroup ? MarketGroup.Count : 0;
      customMarketCount += BlackoutMarketGroup ? BlackoutMarketGroup.Count : 0;

      selectedMarketGroup = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === 255);
      selectedMarketGroup.Count = customMarketCount;
    }

    this.setState({
      customMarketCount,
      selectedMarketGroup,
    });
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.proposalValidationStates.FormInvalid === true) {
      this.onSaveShowValidation(nextProps);
    }
  }

  render() {
    const { initialdata, proposalEditForm, isReadOnly } = this.props;

    // update custom count
    const customIndex = initialdata.MarketGroups.findIndex(marketGroup => marketGroup.Id === 255);
    initialdata.MarketGroups[customIndex].Count = this.state.customMarketCount;

    // add 'Edit Custom Market List' option
    const marketOptions = initialdata.MarketGroups.filter(marketGroup => marketGroup.Count > 0);
    marketOptions.push({ Id: -1, Display: 'Edit Custom Market List' });

    // selected market group
    const selectedMarketGroup = this.state.selectedMarketGroup || marketOptions[0];

    //  handle hiatus flights tips display
    let hasTip = false;
    const checkFlightWeeksTip = (flightWeeks) => {
      if (flightWeeks.length < 1) return '';
      const tip = [<div key="flight">Hiatus Weeks</div>];
      flightWeeks.forEach((flight, idx) => {
        if (flight.IsHiatus) {
          hasTip = true;
          const key = `flight_ + ${idx}`;
          tip.push(<div key={key}><DateMDYYYY date={flight.StartDate} /><span> - </span><DateMDYYYY date={flight.EndDate} /></div>);
        }
      });
      const display = tip;
      return (
        <Tooltip id="flightstooltip">{display}</Tooltip>
      );
    };
    const tooltip = checkFlightWeeksTip(this.props.proposalEditForm.FlightWeeks);

    return (
      <div id="proposal-form">
					<form>
						<Row className="clearfix">
							<Col md={6}>
								<Row>
									<Col md={5}>
										<FormGroup controlId="proposalName" validationState={this.state.validationStates.Name || this.state.validationStates.Name_Alphanumeric || this.state.validationStates.Name_MaxChar} >
											<ControlLabel><strong>Proposal Name</strong></ControlLabel>
                      { !proposalEditForm.Id &&
                      <FormControl
                        type="text"
                        defaultValue={proposalEditForm.ProposalName || ''}
                        onChange={this.onChangeProposalName}
                        disabled={isReadOnly}
                      />
                      }
                      { proposalEditForm.Id &&
                      <InputGroup>
												<FormControl
													type="text"
													defaultValue={proposalEditForm.ProposalName || ''}
                          onChange={this.onChangeProposalName}
                          disabled={isReadOnly}
												/>
                        <InputGroup.Addon>
                          Id: {proposalEditForm.Id}
                        </InputGroup.Addon>
											</InputGroup>
                      }
											{this.state.validationStates.Name != null &&
											<HelpBlock>
												<span className="text-danger" style={{ fontSize: 11 }}>Required.</span>
											</HelpBlock>
                      }
                      {this.state.validationStates.Name_Alphanumeric != null &&
											<HelpBlock>
												<span className="text-danger" style={{ fontSize: 11 }}>Please enter only alphanumeric characters.</span>
											</HelpBlock>
                      }
                      {this.state.validationStates.Name_MaxChar != null &&
											<HelpBlock>
												<span className="text-danger" style={{ fontSize: 11 }}>Please enter no more than 100 characters.</span>
											</HelpBlock>
                      }
										</FormGroup>
									</Col>
									<Col md={3}>
                    <FormGroup controlId="proposalMarket">
                      <ControlLabel><strong>Market</strong></ControlLabel>
                      <Select
                        name="marketGroup"
                        placeholder="Choose a market..."
                        value={selectedMarketGroup}
                        onChange={this.onMarketGroupChange}
                        options={marketOptions}
                        optionRenderer={this.marketSelectorOptionRenderer}
                        valueRenderer={this.marketSelectorValueRenderer}
                        clearable={false}
                        disabled={isReadOnly}
                        valueKey="Id"
                      />
                    </FormGroup>
                  </Col>
									<Col md={2}>
										<FormGroup controlId="proposalPostType">
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
                        disabled={isReadOnly}
											/>
										</FormGroup>
									</Col>
									<Col md={2}>
										<FormGroup controlId="proposalEquivalized">
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
                        disabled={isReadOnly}
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
												<CurrencyDollarWhole dash amount={proposalEditForm.TotalCPM} /> / <CurrencyDollarWhole dash amount={proposalEditForm.TargetCPM} /> <Label bsStyle="success"><PercentWhole dash percent={proposalEditForm.TotalCPMPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetBudget">
											<ControlLabel><strong>Target Budget</strong></ControlLabel>
											<FormControl.Static>
												<CurrencyDollarWhole dash amount={proposalEditForm.TotalCost} /> / <CurrencyDollarWhole dash amount={proposalEditForm.TargetBudget} /> <Label bsStyle="success"><PercentWhole dash percent={proposalEditForm.TotalCostPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetImpressions">
											<ControlLabel><strong>Target Impressions</strong></ControlLabel>
											<FormControl.Static>
												<NumberCommaWhole dash number={proposalEditForm.TotalImpressions / 1000} /> / <NumberCommaWhole dash number={proposalEditForm.TargetImpressions / 1000} /> <Label bsStyle="danger"><PercentWhole dash percent={proposalEditForm.TotalImpressionsPercent} /></Label>
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalTargetUnits">
											<ControlLabel><strong>Target Units</strong></ControlLabel>
											<FormControl.Static>
												{proposalEditForm.TargetUnits || '-'}
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
										<FormGroup controlId="proposalAdvertiser" validationState={this.state.validationStates.AdvertiserId} >
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
                        disabled={isReadOnly}
											/>
                      {this.state.validationStates.AdvertiserId != null &&
											<HelpBlock>
												<span className="text-danger" style={{ fontSize: 11 }}>Required</span>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalGuaranteedDemo">
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
                        disabled={isReadOnly}
											/>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalSecondaryDemo">
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
                        disabled={isReadOnly}
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
                        { proposalEditForm.SpotLengths.length <= 0 && <span> - </span> }
												{ proposalEditForm.SpotLengths.map((spot, index, arr) => (arr.length !== (index + 1) ? `${spot.Display}, ` : `${spot.Display}`)) }
											</FormControl.Static>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalFlight">
											<ControlLabel><strong>Flight</strong></ControlLabel>
											<FormControl.Static>
												{ moment(proposalEditForm.FlightStartDate).isValid() && moment(proposalEditForm.FlightEndDate).isValid() ?
                          <span><DateMDYYYY date={proposalEditForm.FlightStartDate} /><span> - </span><DateMDYYYY date={proposalEditForm.FlightEndDate} /></span>
                        :
                          <span>-</span>
                        }
                        {hasTip &&
                          <OverlayTrigger placement="top" overlay={tooltip}>
                          <Button bsStyle="link"><Glyphicon style={{ color: 'black' }} glyph="info-sign" /></Button>
                          </OverlayTrigger>
                        }
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
                        disabled={isReadOnly}
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
						</Row>
					</form>

          <MarketGroupSelector
            toggleModal={this.props.toggleModal}
            initialdata={initialdata}
            proposalEditForm={proposalEditForm}
            updateProposalEditForm={this.props.updateProposalEditForm}
            updateMarketCount={this.updateMarketCount}
          />
			</div>
    );
  }
}

ProposalForm.defaultProps = {
  toggleModal: () => {},
};

/* eslint-disable react/no-unused-prop-types */
ProposalForm.propTypes = {
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool.isRequired,
  toggleModal: PropTypes.func,

  proposalValidationStates: PropTypes.object.isRequired,
};

