import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  Row,
  Col,
  ControlLabel,
  FormGroup,
  FormControl,
  Button,
  Glyphicon,
  Panel,
  Tooltip,
  OverlayTrigger
} from "react-bootstrap";
// import { Grid } from "react-redux-grid";
import Table, { withGrid } from "Lib/react-table";
import CSSModules from "react-css-modules";
import Select from "react-select";
import DateMDYYYY from "Components/shared/TextFormatters/DateMDYYYY";

import styles from "./index.scss";
import { getDateInFormat } from "../../../../utils/dateFormatter";

const generateMarketLabael = (marketGroupId, markets) => {
  if (marketGroupId === 1) {
    return "All";
  }
  return markets.length ? "Custom" : "None";
};

export class TrackerScrubbingHeader extends Component {
  componentDidMount() {
    // const { date } = this.props;
    // const dateInProperFormat = getDateForDisplay(date);
    // console.log('dates', dateInProperFormat);
    // this.setState({ dates: dateInProperFormat });
  }
  render() {
    const {
      advertiser,
      guaranteedDemo,
      Id,
      marketGroupId,
      name,
      notes,
      secondaryDemo,
      market
    } = this.props;
    const secondaryDemoOptions = [];
    const marketLabel = generateMarketLabael(marketGroupId, market);

    secondaryDemo.forEach(item => {
      const option = {};
      option.Display = item;
      option.Id = item;
      secondaryDemoOptions.push(option);
    });

    // const stateKey = "TrackerScrubbingDetailsGrid";

    const columns = [
      {
        Header: "ID",
        accessor: "Sequence",
        maxWidth: 65
      },
      {
        Header: "Flight",
        accessor: "FlightStartDate",
        // maxWidth: 40
        Cell: row => {
          let hasTip = false;
          const checkFlightWeeksTip = flightWeeks => {
            console.log(row);
            if (flightWeeks.length < 1) return "";
            const tip = [<div key="flight">Hiatus Weeks</div>];
            flightWeeks.forEach((flight, idx) => {
              if (flight.IsHiatus) {
                hasTip = true;
                const key = `flight_ + ${idx}`;
                tip.push(
                  <div key={key}>
                    <DateMDYYYY date={flight.StartDate} />
                    <span> - </span>
                    <DateMDYYYY date={flight.EndDate} />
                  </div>
                );
              }
            });
            const display = tip;
            return <Tooltip id="flightstooltip">{display}</Tooltip>;
          };
          const tooltip = checkFlightWeeksTip(row.original.FlightWeeks);
          const start = getDateInFormat(row.original.FlightStartDate);
          const end = getDateInFormat(row.original.FlightEndDate);
          const display = `${start} - ${end}`;
          return (
            <div>
              <span>{display}</span>
              {hasTip && (
                <OverlayTrigger placement="top" overlay={tooltip}>
                  <Button bsStyle="link">
                    <Glyphicon style={{ color: "black" }} glyph="info-sign" />
                  </Button>
                </OverlayTrigger>
              )}
            </div>
          );
        }
      },
      {
        Header: "Daypart",
        accessor: "DayPart"
        // maxWidth: 30
      },
      {
        Header: "Spot Length",
        accessor: "SpotLength"
        // maxWidth: 20
      }
    ];

    return (
      <div>
        <Row>
          <Col md={12}>
            <ControlLabel>
              <strong>Proposal ID : {Id}</strong>
            </ControlLabel>
          </Col>
        </Row>
        <Row>
          <Col md={6}>
            <Row>
              <Col md={4}>
                <FormGroup controlId="proposalName">
                  <ControlLabel>
                    <strong>Proposal Name</strong>
                  </ControlLabel>
                  <FormControl.Static>{name}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup controlId="advertiser">
                  <ControlLabel>
                    <strong>Advertiser</strong>
                  </ControlLabel>
                  <FormControl.Static>{advertiser}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup controlId="proposalMarket">
                  <ControlLabel>
                    <strong>Market</strong>
                  </ControlLabel>
                  <div style={{ overflow: "hidden" }} href="">
                    <span className="pull-left " style={{ width: "100%" }}>
                      <FormControl.Static>{marketLabel}</FormControl.Static>
                    </span>
                  </div>
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={12}>
                <Panel defaultExpanded>
                  <Panel.Heading style={{ padding: "0" }}>
                    <Panel.Title>
                      <Panel.Toggle>
                        <Button bsStyle="link" bsSize="xsmall">
                          <Glyphicon glyph="triangle-bottom" /> Proposal Detail
                        </Button>
                      </Panel.Toggle>
                    </Panel.Title>
                  </Panel.Heading>
                  <Panel.Collapse>
                    <Panel.Body style={{ padding: "10px" }}>
                      <Table
                        data={this.props.details}
                        style={{ fontSize: "12px", margin: 0 }}
                        columns={columns}
                      />
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
                  <ControlLabel>
                    <strong>Guaranteed Demo</strong>
                  </ControlLabel>
                  <FormControl.Static>{guaranteedDemo}</FormControl.Static>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup
                  id="proposal_secondary_demo"
                  controlId="proposalSecondaryDemo"
                >
                  <ControlLabel>
                    <strong>Secondary Demo</strong>
                  </ControlLabel>
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
                  <FormControl.Static>{notes || "--"}</FormControl.Static>
                </FormGroup>
              </Col>
            </Row>
          </Col>
        </Row>
      </div>
    );
  }
}

TrackerScrubbingHeader.defaultProps = {
  isReadOnly: true,
  notes: undefined,
  secondaryDemo: [],
  name: "",
  marketGroupId: null,
  market: null,
  Id: undefined,
  guaranteedDemo: undefined,
  advertiser: undefined,
  details: []
};

TrackerScrubbingHeader.propTypes = {
  advertiser: PropTypes.string,
  details: PropTypes.array,
  guaranteedDemo: PropTypes.string,
  Id: PropTypes.number,
  // isReadOnly: PropTypes.bool,
  market: PropTypes.array,
  marketGroupId: PropTypes.number,
  name: PropTypes.string,
  notes: PropTypes.string,
  secondaryDemo: PropTypes.array
};

export default CSSModules(withGrid(TrackerScrubbingHeader), styles);
