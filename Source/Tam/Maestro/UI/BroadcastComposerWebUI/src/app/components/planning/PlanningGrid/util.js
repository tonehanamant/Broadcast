import React from "react";
import { Button, Glyphicon, Tooltip, OverlayTrigger } from "react-bootstrap";
import DateMDYYYY from "Components/shared/TextFormatters/DateMDYYYY";

const FlightCellRender = row => {
  let hasTip = false;
  const checkFlightWeeksTip = Flights => {
    if (Flights.length < 1) return "";
    const tip = [<div key="flight">Hiatus Weeks</div>];
    Flights.forEach((flight, idx) => {
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
  const tooltip = checkFlightWeeksTip(row.original.Flights);
  return (
    <div>
      <span>{row.original.displayFlights}</span>
      {hasTip && (
        <OverlayTrigger placement="top" overlay={tooltip}>
          <Button bsStyle="link">
            <Glyphicon style={{ color: "black" }} glyph="info-sign" />
          </Button>
        </OverlayTrigger>
      )}
    </div>
  );
};

export const columns = [
  // use displayId string for search or breaks Fuzzy; use Id displayed/sort (int)
  {
    Header: "Search Id",
    id: "displayId",
    accessor: "displayId",
    show: false,
    width: "5%"
  },
  {
    Header: "Id",
    id: "Id",
    accessor: "Id",
    hideable: false,
    // defaultSortDirection: "ASC",
    width: "5%"
  },
  {
    Header: "Name",
    id: "ProposalName",
    accessor: "ProposalName",
    width: "20%"
  },
  {
    Header: "Advertiser",
    id: "displayAdvertiser",
    accessor: "displayAdvertiser",
    width: "20%"
  },
  {
    Header: "Status",
    id: "displayStatus",
    accessor: "displayStatus",
    width: "10%"
  },
  {
    Header: "Flight",
    id: "displayFlights",
    accessor: "displayFlights",
    width: "20%",
    Cell: FlightCellRender
  },
  {
    Header: "Owner",
    id: "Owner",
    accessor: "Owner",
    width: "15%"
  },
  {
    Header: "Last Modified",
    id: "displayLastModified",
    // accessor: "displayLastModified",
    accessor: "LastModified",
    width: "10%",
    Cell: row => <span>{row.original.displayLastModified}</span>
  }
];
