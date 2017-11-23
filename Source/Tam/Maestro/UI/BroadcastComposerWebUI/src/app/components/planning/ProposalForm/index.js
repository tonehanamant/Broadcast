import React, { Component } from 'react';
// import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Row, Col, Label, FormGroup, ControlLabel, FormControl, HelpBlock } from 'react-bootstrap';
import Select from 'react-select';
import MarketGroupSelector from './MarketGroupSelector';

const mapStateToProps = () => ({
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({}, dispatch)
);

export class ProposalForm extends Component {
  constructor(props) {
    super(props);

    this.toggleMarketSelector = this.toggleMarketSelector.bind(this);
    this.onMarketChange = this.onMarketChange.bind(this);

		this.state = {
      isMarketSelectorOpen: false,
    };

		this.state.Invalid = null;
  }

  toggleMarketSelector() {
    this.setState({ isMarketSelectorOpen: !this.state.isMarketSelectorOpen });
  }

  onMarketChange(marketGroup) {
    if (marketGroup.Id === -1) {
      this.toggleMarketSelector();
    }
  }

  render() {
    return (
      <div id="proposal-form">
					<form>
						<Row className="clearfix">
							<Col md={6}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="proposalName" validationState={this.state.Invalid} >
											<ControlLabel><strong>Proposal Name</strong></ControlLabel>
											<FormControl type="text" />
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalMarket" validationState={this.state.Invalid} >
											<ControlLabel><strong>Market</strong></ControlLabel>
											<Select
                        name="marketGroup"
                        value={null}
                        placeholder="Choose a market..."
                        options={[
                          { Display: 'Market1', Id: 'market1' },
                          { Display: 'Market2', Id: 'market2' },
                          { Display: 'Custom', Id: -1 },
                        ]}
                        labelKey="Display"
                        valueKey="Id"
                        onChange={this.onMarketChange}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={2}>
										<FormGroup controlId="proposalPostType" validationState={this.state.Invalid} >
											<ControlLabel><strong>Post Type</strong></ControlLabel>
											<Select
												name="proposalPostType"
												value={null}
												// placeholder="Choose Posting..."
												// className="form-control"
												options={null}
												labelKey=""
												valueKey=""
												onChange={null}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={2}>
										<FormGroup controlId="proposalEquivalized" validationState={this.state.Invalid} >
											<ControlLabel><strong>Equivalized</strong></ControlLabel>
											<Select
												name="proposalEquivalized"
												value={null}
												// placeholder="Choose Posting..."
												// className="form-control"
												options={null}
												labelKey=""
												valueKey=""
												onChange={null}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
								</Row>
							</Col>
							<Col md={6}>
								<Row>
									<Col md={3}>
										<strong>Target CPM</strong><br />
										<span>-</span> / <span>-</span>
										<Label bsStyle="success">0%</Label>
									</Col>
									<Col md={3}>
										<strong>Target Budget</strong><br />
										<span>-</span> / <span>-</span>
										<Label bsStyle="success">0%</Label>
									</Col>
									<Col md={3}>
										<strong>Target Impressions</strong><br />
										<span>0</span> / <span>-</span>
										<Label bsStyle="danger">0%</Label>
									</Col>
									<Col md={3}>
										<strong>Target Units</strong><br />
										<span>-</span>
									</Col>
								</Row>
							</Col>
						</Row>
						<Row className="clearfix">
							<Col md={7}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="proposalAdvertiser" validationState={this.state.Invalid} >
											<ControlLabel><strong>Advertiser</strong></ControlLabel>
											<Select
												name="proposalAdvertiser"
												value={null}
												// placeholder="Choose Posting..."
												// className="form-control"
												options={null}
												labelKey=""
												valueKey=""
												onChange={null}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalGuaranteedDemo" validationState={this.state.Invalid} >
											<ControlLabel><strong>Guaranteed Demo</strong></ControlLabel>
											<Select
												name="proposalGuaranteedDemo"
												value={null}
												// placeholder="Choose Posting..."
												// className="form-control"
												options={null}
												labelKey=""
												valueKey=""
												onChange={null}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalSecondaryDemo" validationState={this.state.Invalid} >
											<ControlLabel><strong>Secondary Demo</strong></ControlLabel>
											<Select
												name="proposalSecondaryDemo"
												value={null}
												// placeholder="Choose Posting..."
												// className="form-control"
												options={null}
												labelKey=""
												valueKey=""
												onChange={null}
											/>
											{this.state.Invalid != null &&
											<HelpBlock>
												<p className="text-danger">Required</p>
											</HelpBlock>
											}
										</FormGroup>
									</Col>
								</Row>
							</Col>
							<Col md={5}>
								<Row>
									<Col md={4}>
										<strong>Spot Length</strong><br />
										<span>-</span>
									</Col>
									<Col md={4}>
										<strong>Flight</strong><br />
										<span>-</span>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalNotes">
											<ControlLabel>Notes</ControlLabel>
											<FormControl componentClass="textarea" />
										</FormGroup>
									</Col>
								</Row>
							</Col>
						</Row>
					</form>

          <MarketGroupSelector
            open={this.state.isMarketSelectorOpen}
            onClose={this.toggleMarketSelector}
          />
			</div>
    );
  }
}

ProposalForm.defaultProps = {
};

/* eslint-disable react/no-unused-prop-types */
ProposalForm.propTypes = {
};

export default connect(mapStateToProps, mapDispatchToProps)(ProposalForm);
