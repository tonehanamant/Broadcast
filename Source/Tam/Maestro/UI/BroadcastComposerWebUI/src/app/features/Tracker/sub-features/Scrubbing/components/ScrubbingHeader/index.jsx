import React from "react";
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
import Table, { withGrid } from "Lib/react-table";
import CSSModules from "react-css-modules";
import Select from "react-select";
import DateMDYYYY from "Patterns/TextFormatters/DateMDYYYY";
import { getDateInFormat } from "Utils/dateFormatter";

import styles from "./index.style.scss";

const generateMarketLabael = (marketGroupId, markets) => {
  if (marketGroupId === 1) {
    return "All";
  }
  return markets.length ? "Custom" : "None";
};

const renderCoverageGoal = coverage => (coverage ? `${coverage * 100}%` : "--");

function TrackerScrubbingHeader({
  advertiser,
  guaranteedDemo,
  Id,
  marketGroupId,
  details,
  name,
  notes,
  coverageGoal,
  equivalized,
  postingType,
  secondaryDemo,
  market
}) {
  const marketLabel = generateMarketLabael(marketGroupId, market);
  const secondaryDemoOptions = secondaryDemo.map(item => ({
    Display: item,
    Id: item
  }));

  const columns = [
    {
      Header: "ID",
      accessor: "Sequence",
      minWidth: 20
    },
    {
      Header: "Flight",
      accessor: "FlightStartDate",
      Cell: row => {
        let hasTip = false;
        const checkFlightWeeksTip = flightWeeks => {
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
      },
      minWidth: 70
    },
    {
      Header: "Daypart",
      accessor: "DayPart",
      minWidth: 60
    },
    {
      Header: "Spot Length",
      accessor: "SpotLength",
      minWidth: 40
    },
    {
      Header: "Daypart Code",
      accessor: "DaypartCodeDisplay",
      minWidth: 40
    },
    {
      Header: "Estimate Id",
      accessor: "EstimateId",
      Cell: row => (row.value ? row.value : "-"),
      minWidth: 40
    },
    {
      Header: "Inventory Source",
      accessor: "InventorySourceDisplay"
    },
    {
      Header: "Posting Book",
      accessor: "PostingBook",
      Cell: row => (row.value ? row.value : "-"),
      minWidth: 60
    },
    {
      Header: "Playback Type",
      accessor: "PlaybackTypeDisplay",
      Cell: row => (row.value ? row.value : "-"),
      minWidth: 60
    }
  ];

  return (
    <div styleName="tracker-scrubbing-header">
      <Row>
        <Col md={12}>
          <ControlLabel>
            <strong>Proposal ID : {Id}</strong>
          </ControlLabel>
        </Col>
      </Row>
      <div styleName="header-items">
        <FormGroup controlId="proposalName">
          <ControlLabel>Proposal Name</ControlLabel>
          <FormControl.Static>{name}</FormControl.Static>
        </FormGroup>
        <FormGroup controlId="advertiser">
          <ControlLabel>Advertiser</ControlLabel>
          <FormControl.Static>{advertiser}</FormControl.Static>
        </FormGroup>
        <FormGroup controlId="proposalMarket">
          <ControlLabel>Market</ControlLabel>
          <FormControl.Static>{marketLabel}</FormControl.Static>
        </FormGroup>
        <FormGroup controlId="proposalMarket">
          <ControlLabel>Coverage Goal</ControlLabel>
          <FormControl.Static>
            {renderCoverageGoal(coverageGoal)}
          </FormControl.Static>
        </FormGroup>
        <FormGroup controlId="proposalMarket">
          <ControlLabel>Posting Type</ControlLabel>
          <FormControl.Static>{postingType}</FormControl.Static>
        </FormGroup>
        <FormGroup controlId="proposalMarket">
          <ControlLabel>Equivalized</ControlLabel>
          <FormControl.Static>{equivalized ? "Yes" : "No"}</FormControl.Static>
        </FormGroup>
        <FormGroup controlId="guaranteedDemo">
          <ControlLabel>Guaranteed Demo</ControlLabel>
          <FormControl.Static>{guaranteedDemo}</FormControl.Static>
        </FormGroup>
        <FormGroup
          id="proposal_secondary_demo"
          controlId="proposalSecondaryDemo"
        >
          <ControlLabel>Secondary Demo</ControlLabel>
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
        <FormGroup controlId="proposalNotes">
          <ControlLabel>Notes</ControlLabel>
          <FormControl.Static>{notes || "--"}</FormControl.Static>
        </FormGroup>
      </div>
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
            <Table data={details} style={{ margin: 0 }} columns={columns} />
          </Panel.Body>
        </Panel.Collapse>
      </Panel>
    </div>
  );
}

TrackerScrubbingHeader.defaultProps = {
  notes: undefined,
  secondaryDemo: [],
  name: "",
  marketGroupId: null,
  market: null,
  Id: undefined,
  guaranteedDemo: undefined,
  advertiser: undefined,
  coverageGoal: undefined,
  equivalized: undefined,
  postingType: undefined,
  details: []
};

TrackerScrubbingHeader.propTypes = {
  advertiser: PropTypes.string,
  details: PropTypes.array,
  guaranteedDemo: PropTypes.string,
  Id: PropTypes.number,
  coverageGoal: PropTypes.number,
  equivalized: PropTypes.bool,
  postingType: PropTypes.string,
  market: PropTypes.array,
  marketGroupId: PropTypes.number,
  name: PropTypes.string,
  notes: PropTypes.string,
  secondaryDemo: PropTypes.array
};

export default withGrid(CSSModules(TrackerScrubbingHeader, styles));
