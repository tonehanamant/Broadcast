import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, ControlLabel, FormGroup, FormControl, Badge, Button, Glyphicon, Panel, Tooltip, OverlayTrigger } from 'react-bootstrap';
import { Grid } from 'react-redux-grid';
import CSSModules from 'react-css-modules';
import Select from 'react-select';
import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';

import styles from './index.scss';
import { getDateInFormat } from '../../../../utils/dateFormatter';

export class PostScrubbingHeader extends Component {
  componentDidMount() {
    // const { date } = this.props;
    // const dateInProperFormat = getDateForDisplay(date);
    // console.log('dates', dateInProperFormat);
    // this.setState({ dates: dateInProperFormat });
  }
  render() {
    const { advertiser, guaranteedDemo, Id, marketId, name, notes, secondaryDemo } = this.props;
    const isCustomMarket = this.props.marketId === 255;
    const secondaryDemoOptions = [];
    let marketLabel;
    if (isCustomMarket) {
      marketLabel = 'Custom';
    } else {
      marketLabel = marketId === 0 ? 'All' : `Top ${marketId}`;
    }

    secondaryDemo.forEach((item) => {
      const option = {};
      option.Display = item;
      option.Id = item;
      secondaryDemoOptions.push(option);
    });

    const stateKey = 'PostScrubbingDetailsGrid';

    const columns = [
      {
        name: 'ID',
        dataIndex: 'Sequence',
        width: '10%',
      },
      {
        name: 'Flight',
        dataIndex: 'FlightStartDate',
        width: '40%',
        renderer: ({ row }) => {
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
          const tooltip = checkFlightWeeksTip(row.FlightWeeks);
          const start = getDateInFormat(row.FlightStartDate);
          const end = getDateInFormat(row.FlightEndDate);
          const display = `${start} - ${end}`;
          return (
            <div>
              <span>{display}</span>
              { hasTip &&
              <OverlayTrigger placement="top" overlay={tooltip}>
              <Button bsStyle="link"><Glyphicon style={{ color: 'black' }} glyph="info-sign" /></Button>
              </OverlayTrigger>
              }
            </div>
          );
        },
      },
      {
        name: 'Daypart',
        dataIndex: 'DayPart',
        width: '30%',
      },
      {
        name: 'Spot Length',
        dataIndex: 'SpotLength',
        width: '20%',
      },
  ];

  const plugins = {
    COLUMN_MANAGER: {
      resizable: false,
      moveable: false,
      sortable: {
          enabled: false,
          method: 'local',
      },
    },
  };

  const grid = {
    columns,
    plugins,
    stateKey,
  };

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
                        top: '45%',
                        }}
                      >
                      i
                      </Badge>
                    </span>
                  </div>
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={12}>
                <Panel defaultExpanded>
                  <Panel.Heading style={{ padding: '0' }}>
                    <Panel.Title>
                    <Panel.Toggle>
                      <Button bsStyle="link" bsSize="xsmall">
                        <Glyphicon glyph="triangle-bottom" /> Proposal Detail
                      </Button>
                    </Panel.Toggle>
                    </Panel.Title>
                  </Panel.Heading>
                  <Panel.Collapse>
                    <Panel.Body style={{ padding: '10px' }}>
                      <Grid {...grid} data={this.props.details} store={this.context.store} height={false} />
                    </Panel.Body>
                  </Panel.Collapse>
                </Panel>
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
                <FormGroup id="proposal_secondary_demo" controlId="proposalSecondaryDemo">
                  <ControlLabel><strong>Secondary Demo</strong></ControlLabel>
                  <Select
                    placeholder="--"
                    name="proposalSecondaryDemo"
                    multi
                    disabled
                    value={secondaryDemoOptions}
                    labelKey="Display"
                    valueKey="Id"
                  />
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
      </div>
    );
  }
}

PostScrubbingHeader.defaultProps = {
  isReadOnly: true,
  // getProposalDetail: () => { },
};

PostScrubbingHeader.propTypes = {
  advertiser: PropTypes.string.isRequired,
  details: PropTypes.array.isRequired,
  guaranteedDemo: PropTypes.string.isRequired,
  Id: PropTypes.number.isRequired,
  isReadOnly: PropTypes.bool,
  market: PropTypes.array.isRequired,
  marketId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  notes: PropTypes.string.isRequired,
  secondaryDemo: PropTypes.array.isRequired,
  // getProposalDetail: PropTypes.func.isRequired,
};

export default CSSModules(PostScrubbingHeader, styles);
