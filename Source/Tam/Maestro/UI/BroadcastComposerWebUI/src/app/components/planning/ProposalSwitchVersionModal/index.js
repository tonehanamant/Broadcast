import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";

import { Button, Modal } from "react-bootstrap";
// import { Grid } from "react-redux-grid";
import Table, { withGrid } from "Lib/react-table";

import DateMDYYYY from "Components/shared/TextFormatters/DateMDYYYY";

const mapStateToProps = ({
  app: {
    modals: { planningSwitchVersionsModal: modal }
  }
}) => ({
  modal
});

export class ProposalSwitchVersionModal extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.close = this.close.bind(this);
    this.openVersion = this.openVersion.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: "planningSwitchVersionsModal",
      active: false,
      properties: this.props.modal.properties
    });
  }

  openVersion() {
    const { selected, versions, proposal } = this.props;
    const row = versions[selected[0]];
    window.location.assign(
      `/broadcastreact/planning/proposal/${proposal.Id}/version/${row.Version}`
    );
    this.close();
  }

  render() {
    const { Statuses } = this.props.initialdata;
    /* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */

    /* GRID COLUMNS */
    const columns = [
      {
        Header: "Version",
        accessor: "Version",
        minWidth: 5
      },
      {
        Header: "Status",
        accessor: "Status",
        minWidth: 15,
        Cell: ({ value }) =>
          Statuses.map(status => {
            if (status.Id === value) {
              return <span key={status.Id}>{status.Display}</span>;
            }
            return <span key={status.Id} />;
          })
      },
      {
        Header: "Advertiser",
        accessor: "Advertiser",
        minWidth: 10
      },
      {
        Header: "Flight",
        accessor: "StartDate",
        minWidth: 20,
        Cell: ({ row }) => (
          <span>
            <DateMDYYYY date={row.StartDate} /> -{" "}
            <DateMDYYYY date={row.EndDate} />
          </span>
        )
      },
      {
        Header: "Guaranteed Demos",
        accessor: "GuaranteedAudience",
        minWidth: 15
      },
      {
        Header: "Owner",
        accessor: "Owner",
        minWidth: 15
      },
      {
        Header: "Date Modified",
        accessor: "DateModified",
        minWidth: 10,
        Cell: ({ value }) => (
          <span>
            <DateMDYYYY date={value} />
          </span>
        )
      },
      {
        Header: "Notes",
        accessor: "Notes",
        minWidth: 10
      }
    ];

    return (
      <Modal
        show={this.props.modal.active}
        onHide={this.close}
        dialogClassName="large-80-modal"
      >
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Switch Proposal Version
          </Modal.Title>
          <Button
            className="close"
            bsStyle="link"
            onClick={this.close}
            style={{ display: "inline-block", float: "right" }}
          >
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <Table
            data={this.props.versions}
            style={{ marginBottom: "100px" }}
            columns={columns}
            getTrGroupProps={() => ({
              onDoubleClick: () => {
                this.openVersion();
              }
            })}
            selection="single"
          />
        </Modal.Body>
        <Modal.Footer>
          <Button onClick={this.close}>Cancel</Button>
          <Button onClick={this.openVersion} bsStyle="success">
            Open
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

ProposalSwitchVersionModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {}
  }
};

ProposalSwitchVersionModal.propTypes = {
  modal: PropTypes.object,
  selected: PropTypes.object.isRequired,

  toggleModal: PropTypes.func.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposal: PropTypes.object.isRequired,
  versions: PropTypes.array.isRequired
};

export default connect(mapStateToProps)(withGrid(ProposalSwitchVersionModal));
