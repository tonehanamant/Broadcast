import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, Label, FormGroup, InputGroup, ControlLabel, FormControl, HelpBlock } from 'react-bootstrap';
import Select from 'react-select';
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

    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);

    this.toggleMarketSelector = this.toggleMarketSelector.bind(this);
    this.onMarketGroupChange = this.onMarketGroupChange.bind(this);
    this.onChangeCustomMarketSelection = this.onChangeCustomMarketSelection.bind(this);
    this.marketSelectorOptionRenderer = this.marketSelectorOptionRenderer.bind(this);
    this.marketSelectorValueRenderer = this.marketSelectorValueRenderer.bind(this);
    this.onMarketsSelectionChange = this.onMarketsSelectionChange.bind(this);
    this.initializeMarkets = this.initializeMarkets.bind(this);

    this.state = {
      nameInvalid: null,
      advertiserInvalid: null,
      selectedMarketGroup: {},
      isMarketSelectorOpen: false,
      customMarketCount: 0,
      selectedMarkets: [],
      blackoutMarkets: [],
    };

    this.state.Invalid = null;
  }

  onChangeProposalName(event) {
    const val = event.target.value ? event.target.value : '';
    this.props.updateProposalEditForm({ key: 'ProposalName', value: val });
    this.setValidationState('nameInvalid', val ? null : 'error');
  }

  onChangeAdvertiserId(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditForm({ key: 'AdvertiserId', value: val });
    this.setValidationState('advertiserInvalid', val ? null : 'error');
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

  clearValidationStates() {
    this.setState({
      nameInvalid: null,
      advertiserInvalid: null,
    });
  }

  setValidationState(type, state) {
    this.state[type] = state;
  }

  toggleMarketSelector() {
    this.setState({ isMarketSelectorOpen: !this.state.isMarketSelectorOpen });
  }

  onMarketGroupChange(selectedMarketGroup) {
    const { initialdata } = this.props;

    let option = selectedMarketGroup;
    if (selectedMarketGroup.Count) {
      option = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === selectedMarketGroup.Id);
    }

    if (option.Id === -1) {
      option = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === 255);
      this.toggleMarketSelector();
    } else {
      this.props.updateProposalEditForm({ key: 'Markets', value: null });
      this.props.updateProposalEditForm({ key: 'MarketGroupId', value: option.Id });
      this.props.updateProposalEditForm({ key: 'BlackoutMarketGroupId', value: null });
    }

    this.setState({
      selectedMarketGroup: option,
    });
  }

  onChangeCustomMarketSelection() {
    const { initialdata } = this.props;
    const { selectedMarkets, blackoutMarkets } = this.state;

    let marketGroup;
    const simpleMarkets = [];
    selectedMarkets.map((market) => {
      if (market.Count) {
        marketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: false,
        });
      }

      return market;
    });

    let blackoutMarketGroup;
    blackoutMarkets.map((market) => {
      if (market.Count) {
        blackoutMarketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: true,
        });
      }

      return market;
    });

    // updates values for BE
    this.props.updateProposalEditForm({ key: 'Markets', value: simpleMarkets });
    this.props.updateProposalEditForm({ key: 'MarketGroupId', value: marketGroup });
    this.props.updateProposalEditForm({ key: 'BlackoutMarketGroupId', value: blackoutMarketGroup });

    // total markets selected for custom option -- if selector was cleared, assign the first option from initialData (i.e. 'All')
    const customMarketCount = selectedMarkets.concat(blackoutMarkets).reduce((sum, market) => sum + (market.Count || 1), 0);
    this.setState({ customMarketCount });

    if (customMarketCount === 0) {
      this.setState({ selectedMarketGroup: initialdata.MarketGroups[0] });
    } else {
      const customOption = initialdata.MarketGroups.find(marketgGroup => marketgGroup.Id === 255);
      this.setState({ selectedMarketGroup: customOption });
    }

    this.toggleMarketSelector();
  }

  onMarketsSelectionChange(markets, selectorName) {
    if (selectorName === 'Markets') {
      this.setState({ selectedMarkets: markets });
    } else {
      this.setState({ blackoutMarkets: markets });
    }
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

  initializeMarkets() {
    const { initialdata, proposalEditForm } = this.props;
    const { MarketGroup, Markets, BlackoutMarketGroup } = proposalEditForm;
    let selectedMarketGroup = MarketGroup;
    let customMarketCount = 0;

    // conditions to be custom: has any amount of 'single' markets OR has more than one marketGroup
    const isCustom = ((Markets && Markets.length > 0) || (MarketGroup && BlackoutMarketGroup));
    if (isCustom) {
      customMarketCount = Markets.length;
      customMarketCount += MarketGroup ? MarketGroup.Count : 0;
      customMarketCount += BlackoutMarketGroup ? BlackoutMarketGroup.Count : 0;

      selectedMarketGroup = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === 255);
      selectedMarketGroup.Count = customMarketCount;

      // update selected lists (simple and blackout)
      const selectedMarkets = Markets.filter(market => !market.IsBlackout);
      selectedMarkets.unshift(MarketGroup);
      this.setState({ selectedMarkets });

      const blackoutMarkets = Markets.filter(market => market.IsBlackout);
      blackoutMarkets.unshift(BlackoutMarketGroup);
      this.setState({ blackoutMarkets });
    }

    this.setState({ customMarketCount });
    this.setState({ selectedMarketGroup });
  }

  onChangePostType(value) {
    this.props.updateProposalEditForm({ key: 'PostType', value: value ? value.Id : null });
  }

  onChangeEquivalized(value) {
    this.props.updateProposalEditForm({ key: 'Equivalized', value: value ? value.Bool : null });
  }

  onChangeGuaranteedDemoId(value) {
    this.props.updateProposalEditForm({ key: 'GuaranteedDemoId', value: value ? value.Id : null });
  }

  onChangeSecondaryDemos(value) {
    this.props.updateProposalEditForm({ key: 'SecondaryDemos', value: value.map(item => item.Id) });
  }

  componentWillMount() {
    this.initializeMarkets();
  }

  render() {
    const { initialdata, proposalEditForm } = this.props;

    // update custom count
    const customIndex = initialdata.MarketGroups.findIndex(marketGroup => marketGroup.Id === 255);
    initialdata.MarketGroups[customIndex].Count = this.state.customMarketCount;

    // add 'Edit Custom Market List' option
    const marketOptions = initialdata.MarketGroups.filter(marketGroup => marketGroup.Count > 0);
    marketOptions.push({ Id: -1, Display: 'Edit Custom Market List' });

    return (
      <div id="proposal-form">
					<form>
						<Row className="clearfix">
							<Col md={6}>
								<Row>
									<Col md={5}>
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
									<Col md={3}>
                    <FormGroup controlId="proposalMarket" validationState={this.state.Invalid} >
                      <ControlLabel><strong>Market</strong></ControlLabel>
                      <Select
                        name="marketGroup"
                        value={this.state.selectedMarketGroup}
                        placeholder="Choose a market..."
                        options={marketOptions}
                        optionRenderer={this.marketSelectorOptionRenderer}
                        valueRenderer={this.marketSelectorValueRenderer}
                        onChange={this.onMarketGroupChange}
                        clearable={false}
                      />
                      {this.state.Invalid != null &&
                        <HelpBlock>
                          <p className="text-danger">Required</p>
                        </HelpBlock>
                      }
                    </FormGroup>
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

          <MarketGroupSelector
            title={'Custom Market'}
            open={this.state.isMarketSelectorOpen}
            onClose={this.toggleMarketSelector}
            marketGroups={initialdata.MarketGroups.filter(market => (market.Id !== -1) && (market.Id !== 255))}
            markets={initialdata.Markets}
            onApplyChange={this.onChangeCustomMarketSelection}
            onMarketsSelectionChange={this.onMarketsSelectionChange}
            selectedMarkets={this.state.selectedMarkets}
            blackoutMarkets={this.state.blackoutMarkets}
          />
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
