import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import {
  Badge,
  Button,
  Glyphicon,
  Tooltip,
  OverlayTrigger
} from "react-bootstrap";
import { toggleModal, createAlert } from "Ducks/app";
import { getPost, getPostClientScrubbing } from "Ducks/post";
import Table, { withGrid } from "Lib/react-table";
import numeral from "numeral";
import DateMDYYYY from "Patterns/TextFormatters/DateMDYYYY";
import { getDateInFormat } from "Utils/dateFormatter";

const mapStateToProps = ({ post: { postGridData }, menu }) => ({
  postGridData,
  menu
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getPost,
      createAlert,
      toggleModal,
      getPostClientScrubbing
    },
    dispatch
  );

export class DataGridContainer extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.showscrubbingModal = this.showscrubbingModal.bind(this);
  }

  componentWillMount() {
    this.props.getPost();
  }

  /* ////////////////////////////////// */
  /* // GRID ACTION METHOD BINDINGS
  /* ////////////////////////////////// */
  showscrubbingModal(Id) {
    // change to params
    this.props.getPostClientScrubbing({
      proposalId: Id,
      showModal: true,
      filterKey: "All"
    });
  }

  render() {
    const menuItems = [
      {
        text: "NSI Post Report",
        key: "menu-post-nsi-report",
        EVENT_HANDLER: ({
          metaData: {
            rowData: { SpotsInSpec, ContractId }
          }
        }) => {
          if (SpotsInSpec !== 0) {
            window.open(
              `${__API__}Post/DownloadNSIPostReport/${ContractId}`,
              "_blank"
            );
          } else {
            this.props.createAlert({
              type: "warning",
              headline: "NSI Report Unavailable",
              message: "There are no in-spec spots for this proposal."
            });
          }
        }
      },
      {
        text: "NSI Post Report with Overnights",
        key: "menu-post-nsi-report-overnight",
        EVENT_HANDLER: ({ metaData }) => {
          if (metaData.rowData.SpotsInSpec !== 0) {
            window.open(
              `${__API__}Post/DownloadNSIPostReportWithOvernight/${
                metaData.rowData.ContractId
              }`,
              "_blank"
            );
          } else {
            this.props.createAlert({
              type: "warning",
              headline: "NSI Report with Overnights Unavailable",
              message: "There are no in-spec spots for this proposal."
            });
          }
        }
      },
      {
        text: "MYEvents Report",
        key: "menu-post-myevents-report",
        EVENT_HANDLER: ({ metaData }) => {
          if (metaData.rowData.SpotsInSpec !== 0) {
            window.open(
              `${__API__}Post/DownloadMyEventsReport/${
                metaData.rowData.ContractId
              }`,
              "_blank"
            );
          } else {
            this.props.createAlert({
              type: "warning",
              headline: "MYEvents Report Unavailable",
              message: "There are no in-spec spots for this proposal."
            });
          }
        }
      },
      {
        text: "NTI Post Report",
        key: "menu-post-download-nti-report",
        isShow: rowDetail => rowDetail.original.PostType === 2,
        EVENT_HANDLER: ({ metaData }) => {
          if (metaData.rowData.PostType === 2) {
            window.open(
              `${__API__}Post/DownloadNTIReport/${metaData.rowData.ContractId}`,
              "_blank"
            );
          } else {
            this.props.createAlert({
              type: "warning",
              headline: "NTI Post Report Unavailable",
              message: "Please select a different proposal"
            });
          }
        }
      }
    ];

    const columns = [
      {
        Header: "Contract",
        accessor: "ContractName",
        minWidth: 15
      },
      {
        Header: "Contract Id",
        accessor: "ContractId",
        minWidth: 10
      },
      {
        Header: "Contract Id",
        accessor: "searchContractId",
        show: false,
        minWidth: 5
      },
      {
        Header: "Advertiser",
        accessor: "Advertiser",
        minWidth: 15
      },
      {
        Header: "Flight",
        accessor: "FlightStartDate",
        minWidth: 20,
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
        }
      },
      {
        Header: "Affidavit Upload Date",
        accessor: "searchUploadDate",
        minWidth: 15
      },
      {
        Header: "Spots in Spec",
        accessor: "SpotsInSpec",
        minWidth: 15
      },
      {
        Header: "Spots in Spec",
        accessor: "searchSpotsInSpec",
        show: false,
        minWidth: 5
      },
      {
        Header: "Spots Out of Spec",
        accessor: "SpotsOutOfSpec",
        minWidth: 15
      },
      {
        Header: "Spots Out of Spec",
        accessor: "searchSpotsOutOfSpec",
        show: false,
        minWidth: 5
      },
      {
        Header: "Primary Demo Booked",
        accessor: "PrimaryAudienceBookedImpressions",
        minWidth: 15,
        Cell: row =>
          row.value ? numeral(row.value / 1000).format("0,0.[000]") : "-"
      },
      {
        Header: "Primary Demo Delivered",
        accessor: "PrimaryAudienceDeliveredImpressions",
        minWidth: 15,
        Cell: row => {
          // handle equivalized indicator as badge if true
          const val = row.value
            ? numeral(row.value / 1000).format("0,0.[000]")
            : "-";
          return row.original.Equivalized ? (
            <div>
              {val}
              <Badge style={{ fontSize: "9px", marginTop: "4px" }} pullRight>
                EQ
              </Badge>
            </div>
          ) : (
            val
          );
        }
      },
      {
        Header: "Primary Demo % Delivery",
        accessor: "PrimaryAudienceDelivery",
        minWidth: 15,
        Cell: row => {
          const val = row.value ? numeral(row.value).format("0,0.[00]") : false;
          return val ? `${val}%` : "-";
        }
      },
      {
        Header: "Household Delivered",
        accessor: "HouseholdDeliveredImpressions",
        minWidth: 15,
        Cell: row => {
          // handle equivalized indicator as badge if true
          const val = row.value
            ? numeral(row.value / 1000).format("0,0.[000]")
            : "-";
          return row.original.Equivalized ? (
            <div>
              {val}
              <Badge style={{ fontSize: "9px", marginTop: "4px" }} pullRight>
                EQ
              </Badge>
            </div>
          ) : (
            val
          );
        }
      }
    ];

    return (
      <Table
        data={this.props.postGridData}
        style={{ marginBottom: "100px" }}
        columns={columns}
        getTrGroupProps={(state, rowInfo) => ({
          onDoubleClick: () => {
            const Id = rowInfo.original.ContractId;
            this.showscrubbingModal(Id);
          }
        })}
        contextMenu={{
          isRender: true,
          menuItems
        }}
        selection="single"
      />
    );
  }
}

DataGridContainer.propTypes = {
  postGridData: PropTypes.array.isRequired,

  getPost: PropTypes.func.isRequired,
  getPostClientScrubbing: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(withGrid(DataGridContainer));
