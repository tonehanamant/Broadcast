import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Badge } from "react-bootstrap";
import { toggleModal, createAlert } from "Ducks/app";
import { getTracker, getTrackerClientScrubbing } from "Ducks/tracker";
import Table, { withGrid } from "Lib/react-table";
import numeral from "numeral";

const mapStateToProps = ({ tracker: { trackerGridData }, menu }) => ({
  trackerGridData,
  menu
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getTracker,
      createAlert,
      toggleModal,
      getTrackerClientScrubbing
    },
    dispatch
  );

export class DataGridContainer extends Component {
  constructor(props, context) {
    super(props, context);
    console.log(this.context);
  }

  componentWillMount() {
    this.props.getTracker();
  }

  showscrubbingModal(Id) {
    // change to params
    this.props.getTrackerClientScrubbing({
      proposalId: Id,
      showModal: true,
      filterKey: "All"
    });
  }
  render() {
    const menuItems = [
      {
        text: "Spot Tracker Report",
        key: "menu-tracker-spot-tracker-report",
        EVENT_HANDLER: ({ metaData }) => {
          window.open(
            `${__API__}SpotTracker/SpotTrackerReport/${
              metaData.rowData.ContractId
            }`,
            "_blank"
          );
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
        minWidth: 15
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
        Header: "Post Log Upload Date",
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
          console.log(row);
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
        data={this.props.trackerGridData}
        style={{ fontSize: "12px", marginBottom: "100px" }}
        columns={columns}
        getTrGroupProps={(state, rowInfo) => ({
          onDoubleClick: () => {
            console.log(rowInfo);
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
  trackerGridData: PropTypes.array.isRequired,

  getTracker: PropTypes.func.isRequired,
  getTrackerClientScrubbing: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(withGrid(DataGridContainer));
