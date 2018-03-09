import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, ControlLabel, FormGroup, FormControl } from 'react-bootstrap';
import Select from 'react-select';

import { getDateForDisplay } from '../../../../utils/dateFormatter';

export class PostScrubbingHeader extends Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedMarketGroup: '',
            dates: [],
        };

        this.marketSelectorOptionRenderer = this.marketSelectorOptionRenderer.bind(this);
        this.marketSelectorValueRenderer = this.marketSelectorValueRenderer.bind(this);
        this.datesSelectorOptionRenderer = this.datesSelectorOptionRenderer.bind(this);
        this.datesSelectorValueRenderer = this.datesSelectorValueRenderer.bind(this);
        this.handleDatesOnChange = this.handleDatesOnChange.bind(this);
    }

    /* eslint-disable */
    componentDidMount() {
        const { date } = this.props;
        const dateInProperFormat = getDateForDisplay(date);

        this.setState({ dates: dateInProperFormat });
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
            <span className="pull-left ">{`Top ${this.props.marketId}`}</span>
          </div>
        );
    }


    datesSelectorOptionRenderer(option) {
		const divStyle = { overflow: 'hidden' };

		return (
		<div style={divStyle} href="">
			<span className="pull-left">{ option.Display }</span>
		</div>
		);
    }

    datesSelectorValueRenderer() {
        return (
            <div style={{ overflow: 'hidden' }} href="">
              <span className="pull-left ">{this.state.activeDate}</span>
            </div>
        );
    }

    handleDatesOnChange(option) {
        const selectedDate = this.state.dates.filter(item => option.Id === item.Id);

        this.setState({ activeDate: selectedDate[0].Display});
    }

    render() {
        const {
            advertiser,
            gaurantedDemo,
            Id,
            isReadOnly,
            market,
            name,
            notes,
            secondaryDemo,
        } = this.props;

        return (
            <div>
                <Row>
                    <Col md={12}><ControlLabel><strong>Proposal ID : { Id }</strong></ControlLabel></Col>
                </Row>
                <Row>
                    <Col md={6}>
                        <Row>
                            <Col md={4}>
                                <FormGroup controlId="proposalName">
                                    <ControlLabel><strong>ProposalName</strong></ControlLabel>
                                    <FormControl
                                        type="text"
                                        defaultValue={name}
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                            <Col md={4}>
                                <FormGroup controlId="advertiser">
                                    <ControlLabel><strong>Advertiser</strong></ControlLabel>
                                    <FormControl
                                        type="text"
                                        defaultValue={advertiser}
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                            <Col md={3}>
                                <FormGroup controlId="proposalMarket">
                                    <ControlLabel><strong>Market</strong></ControlLabel>
                                    <Select
                                        name="marketGroup"
                                        placeholder="Choose a market..."
                                        onChange={() => {}}
                                        value={market}
                                        optionRenderer={this.marketSelectorOptionRenderer}
                                        valueRenderer={this.marketSelectorValueRenderer}
                                        options={market}
                                        clearable={false}
                                        valueKey="Id"
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                        </Row>
                    </Col>
                    <Col md={6}>
                        <Row>
                            <Col md={4}>
                                <FormGroup controlId="gaurantedDemo">
                                    <ControlLabel><strong>Gauranted Demo</strong></ControlLabel>
                                    <FormControl
                                        type="text"
                                        defaultValue={gaurantedDemo}
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                            <Col md={4}>
                                <FormGroup controlId="proposalSecondaryDemo">
                                    <ControlLabel><strong>Secondary Demo</strong></ControlLabel>
                                    <Select
                                        name="proposalSecondaryDemo"
                                        value={secondaryDemo}
                                        labelKey="Display"
                                        valueKey="Id"
                                        multi
                                        options={secondaryDemo}
                                        closeOnSelect
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                            <Col md={4}>
                                <FormGroup controlId="proposalNotes">
                                    <ControlLabel>Notes</ControlLabel>
                                    <FormControl
                                        componentClass="textarea"
                                        defaultValue={notes}
                                        disabled={isReadOnly}
                                    />
                                </FormGroup>
                            </Col>
                        </Row>
                    </Col>
                </Row>
                <Row>
                    <Col md={5}>
                        <FormGroup controlId="proposalDates">
                            <Select
                                value={this.state.dates}
                                name="proposalDates"
                                onChange={option => this.handleDatesOnChange(option)}
                                optionRenderer={this.datesSelectorOptionRenderer}
                                valueRenderer={this.datesSelectorValueRenderer}
                                options={this.state.dates}
                                clearable={false}
                                valueKey="Id"
                            />
                        </FormGroup>
                    </Col>
                </Row>
                <Row>
                    <Col md={2}>
                        <FormGroup controlId="Flight">
                            <ControlLabel><strong>Flight</strong></ControlLabel>
                            <FormControl
                                type="text"
                                defaultValue={'test'}
                                disabled={isReadOnly}
                            />
                        </FormGroup>
                    </Col>
                    <Col md={2}>
                        <FormGroup controlId="DayPart">
                            <ControlLabel><strong>Day Part</strong></ControlLabel>
                            <FormControl
                                type="text"
                                defaultValue={'Some Day Aprt'}
                                disabled={isReadOnly}
                            />
                        </FormGroup>
                    </Col>
                    <Col md={2}>
                        <FormGroup controlId="Spot Length">
                            <ControlLabel><strong>Spot Length</strong></ControlLabel>
                            <FormControl
                                type="text"
                                defaultValue={'15'}
                                disabled={isReadOnly}
                            />
                        </FormGroup>
                    </Col>
                    <Col md={2}>
                        <FormGroup controlId="Program/Genre">
                            <ControlLabel><strong>Program/Genre</strong></ControlLabel>
                            <FormControl
                                type="text"
                                defaultValue={'Test'}
                                disabled={isReadOnly}
                            />
                        </FormGroup>
                    </Col>
                </Row>
            </div>
        );
    }
}

PostScrubbingHeader.defaultProps = {
    isReadOnly: true,
};

PostScrubbingHeader.PropTypes = {
    advertiser: PropTypes.string.isRequired,
    date: PropTypes.object.isRequired,
    gaurantedDemo: PropTypes.string.isRequired,
    Id: PropTypes.string.isRequired,
    isReadOnly: PropTypes.bool,
    market: PropTypes.object.isRequired,
    marketId: PropTypes.number.isRequired,
    name: PropTypes.string.isRequired,
    notes: PropTypes.string.isRequired,
    secondaryDemo: PropTypes.object.isRequired,
};

export default PostScrubbingHeader;
