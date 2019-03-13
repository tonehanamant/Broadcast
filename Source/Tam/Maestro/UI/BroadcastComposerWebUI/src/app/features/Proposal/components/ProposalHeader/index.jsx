import React, { Component } from "react";
import PropTypes from "prop-types";

import { Button, Panel, Row, Col } from "react-bootstrap";

import ProposalForm from "Proposal/components/ProposalForm";
import ProposalHeaderActions from "Proposal/components/ProposalHeaderActions";

export default class ProposalHeader extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.state.open = true;
  }

  render() {
    const {
      toggleModal,
      isEdit,
      initialdata,
      proposal,
      proposalEditForm,
      updateProposalEditForm,
      getProposalVersions,
      deleteProposal,
      saveProposalAsVersion,
      unorderProposal,
      proposalValidationStates,
      isReadOnly,
      generateScx
    } = this.props;
    const { open } = this.state;
    return (
      <div id="proposal-header">
        {isEdit && (
          <h4>
            {proposal.ProposalName} - Version: {proposal.Version}
          </h4>
        )}
        {!isEdit && <h4>Create Planning Proposal</h4>}
        <hr />
        {isEdit && (
          <Row>
            <Col md={8}>
              <Button
                bsStyle="primary"
                onClick={() => this.setState({ open: !open })}
              >
                <span
                  className="glyphicon glyphicon-triangle-bottom"
                  aria-hidden="true"
                />
              </Button>
            </Col>
            <Col md={4} style={{ float: "right" }}>
              <ProposalHeaderActions
                initialdata={initialdata}
                proposalEditForm={proposalEditForm}
                getProposalVersions={getProposalVersions}
                updateProposalEditForm={updateProposalEditForm}
                deleteProposal={deleteProposal}
                saveProposalAsVersion={saveProposalAsVersion}
                unorderProposal={unorderProposal}
                generateScx={generateScx}
                toggleModal={toggleModal}
                isReadOnly={isReadOnly}
                proposal={proposal}
              />
            </Col>
          </Row>
        )}
        {!isEdit && (
          <Button
            bsStyle="primary"
            onClick={() => this.setState({ open: !open })}
          >
            <span
              className="glyphicon glyphicon-triangle-bottom"
              aria-hidden="true"
            />
          </Button>
        )}
        <Panel style={{ marginTop: 10 }} expanded={open}>
          <Panel.Collapse>
            <Panel.Body>
              <ProposalForm
                initialdata={initialdata}
                proposal={proposal}
                proposalEditForm={proposalEditForm}
                updateProposalEditForm={updateProposalEditForm}
                toggleModal={toggleModal}
                isReadOnly={isReadOnly}
                proposalValidationStates={proposalValidationStates}
                isEdit={isEdit}
              />
            </Panel.Body>
          </Panel.Collapse>
        </Panel>
      </div>
    );
  }
}

ProposalHeader.defaultProps = {
  isEdit: false,
  getProposalVersions: () => {},
  deleteProposal: () => {},
  saveProposalAsVersion: () => {},
  unorderProposal: () => {}
};

/* eslint-disable react/no-unused-prop-types */
ProposalHeader.propTypes = {
  isEdit: PropTypes.bool,
  proposal: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,

  proposalValidationStates: PropTypes.object.isRequired,

  getProposalVersions: PropTypes.func,
  deleteProposal: PropTypes.func,
  saveProposalAsVersion: PropTypes.func,
  unorderProposal: PropTypes.func,
  generateScx: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool.isRequired
};
