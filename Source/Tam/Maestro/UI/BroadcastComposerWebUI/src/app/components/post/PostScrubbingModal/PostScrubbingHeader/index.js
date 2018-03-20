/* eslint-disable */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, ControlLabel, FormGroup, FormControl, Badge } from 'react-bootstrap';
import Select from 'react-select';

import { getDateForDisplay } from '../../../../utils/dateFormatter';

export class PostScrubbingHeader extends Component {
  constructor(props) {
    super(props);
    this.state = {
      dates: [],
    };

    this.datesSelectorOptionRenderer = this.datesSelectorOptionRenderer.bind(this);
    this.datesSelectorValueRenderer = this.datesSelectorValueRenderer.bind(this);
  }

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
    const isCustom = this.props.marketId === 255;

    return (
      <div style={{ overflow: 'hidden' }} href="">
        <span className="pull-left " style={{ width: '100%' }} >
          {isCustom ? 'Custom' : `Top ${this.props.marketId}`}
          <Badge style={{
            display: isCustom ? 'block' : 'none',
            position: 'absolute',
            left: '70%',
            top: '20%'
          }}>i</Badge>
        </span>
      </div>
    );
  }

  datesSelectorOptionRenderer(option) {
    const divStyle = { overflow: 'hidden' };

    return (
      <div style={divStyle} href="">
        <span className="pull-left">{option.Display}</span>
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

  render() {
    const { advertiser, guaranteedDemo, Id, isReadOnly, marketId, name, notes, secondaryDemo } = this.props;
    const isCustomMarket = this.props.marketId === 255;
    let marketLabel;
    if (isCustomMarket) {
      marketLabel = 'Custom';
    } else {
      marketLabel = marketId === 0 ? 'All' : `Top ${marketId}`
    }

    return (
      <div>
        <Row>
          <Col md={12}><ControlLabel><strong>Proposal ID : {Id}</strong></ControlLabel></Col>
        </Row>
        <Row>
          <Col md={6}>
            <Row>
              <Col md={4}>
                <FormGroup controlId="proposalName">
                  <ControlLabel><strong>Proposal Name</strong></ControlLabel>
                  <FormControl.Static>{name}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup controlId="advertiser">
                  <ControlLabel><strong>Advertiser</strong></ControlLabel>
                  <FormControl.Static>{advertiser}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup controlId="proposalMarket">
                  <ControlLabel><strong>Market</strong></ControlLabel>
                  <div style={{ overflow: 'hidden' }} href="">
                    <span className="pull-left "style={{ width: '100%' }} >
                      <FormControl.Static>{marketLabel}</FormControl.Static>
                      <Badge style={{
                        display: isCustomMarket ? 'block' : 'none',
                        position: 'absolute',
                        left: '50%',
                        top: '45%'
                      }}>i</Badge>
                    </span>
                  </div>
                </FormGroup>
              </Col>
            </Row>
          </Col>
          <Col md={6}>
            <Row>
              <Col md={4}>
                <FormGroup controlId="guaranteedDemo">
                  <ControlLabel><strong>Guaranteed Demo</strong></ControlLabel>
                  <FormControl.Static>{guaranteedDemo}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup controlId="proposalSecondaryDemo">
                  <ControlLabel><strong>Secondary Demo</strong></ControlLabel>
                  <FormControl.Static>{secondaryDemo.join(', ') || '--'}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup controlId="proposalNotes">
                  <ControlLabel>Notes</ControlLabel>
                  <FormControl.Static>{notes || '--'}</FormControl.Static>
                </FormGroup>
              </Col>
            </Row>
          </Col>
        </Row>
        {/* <Row>
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
        </Row> */}
      </div>
    );
  }
};

PostScrubbingHeader.defaultProps = {
  isReadOnly: true,
  // getProposalDetail: () => { },
};

PostScrubbingHeader.propTypes = {
  advertiser: PropTypes.string,
  date: PropTypes.object,
  guaranteedDemo: PropTypes.string,
  Id: PropTypes.string,
  isReadOnly: PropTypes.bool,
  market: PropTypes.object,
  marketId: PropTypes.number,
  name: PropTypes.string,
  notes: PropTypes.string,
  secondaryDemo: PropTypes.object,
  // getProposalDetail: PropTypes.func.isRequired,
};

export default PostScrubbingHeader;
