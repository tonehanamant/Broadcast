import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Button, Collapse, Panel, Row, Col } from 'react-bootstrap';

import ProposalForm from 'Components/planning/ProposalForm';
import ProposalHeaderActions from 'Components/planning/ProposalHeaderActions';

export default class ProposalHeader extends Component {
  constructor(props) {
    super(props);
		this.state = {};
		this.state.open = true;
  }

  render() {
    console.log('HEADER PROPS>>>>>>>>>>>>>>>>>', this.props);
    const { toggleModal, isEdit, initialdata, proposal, proposalEditForm, updateProposalEditForm, deleteProposal, saveProposalAsVersion } = this.props;
    return (
      <div id="proposal-header">
        {isEdit &&
				<h4>{proposal.ProposalName} - Version: {proposal.Version}</h4>
        }
        {!isEdit &&
				<h4>Create Planning Proposal</h4>
        }
				<hr />
        {isEdit &&
          <Row>
            <Col md={8}>
              <Button bsStyle="primary" onClick={() => this.setState({ open: !this.state.open })}>
                <span className="glyphicon glyphicon-triangle-bottom" aria-hidden="true" />
              </Button>
            </Col>
            <Col md={4} style={{ float: 'right' }}>
              <ProposalHeaderActions
                initialdata={initialdata}
                proposalEditForm={proposalEditForm}
                updateProposalEditForm={updateProposalEditForm}
                deleteProposal={deleteProposal}
                saveProposalAsVersion={saveProposalAsVersion}
                toggleModal={toggleModal}
              />
            </Col>
          </Row>
        }
        {!isEdit &&
          <Button bsStyle="primary" onClick={() => this.setState({ open: !this.state.open })}>
            <span className="glyphicon glyphicon-triangle-bottom" aria-hidden="true" />
          </Button>
        }
				<Collapse in={this.state.open}>
          <Panel style={{ marginTop: 10 }}>
						<ProposalForm
              initialdata={initialdata}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
              toggleModal={toggleModal}
						/>
					</Panel>
        </Collapse>
			</div>
    );
  }
}

ProposalHeader.defaultProps = {
  isEdit: false,
};

/* eslint-disable react/no-unused-prop-types */
ProposalHeader.propTypes = {
  isEdit: PropTypes.bool,
  proposal: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,
  deleteProposal: PropTypes.func.isRequired,
  saveProposalAsVersion: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
