import React, { Component } from 'react';
import PropTypes from 'prop-types';

import ProposalDetail from 'Components/planning/ProposalDetail';

export default class ProposalDetails extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.onUpdateProposal = this.onUpdateProposal.bind(this);
	}

  onUpdateProposal() {
    const { updateProposal } = this.props;
    updateProposal();
  }

  render() {
    const { proposalEditForm, initialdata, updateProposalEditFormDetail, deleteProposalDetail, toggleModal, modelNewProposalDetail } = this.props;
    return (
      <div id="proposal-details">
				<h5 style={{ textAlign: 'center', color: '#1e5fa8' }}><strong>Proposal Details</strong></h5>
        {proposalEditForm.Details.map(detail => (
					<ProposalDetail
            key={detail.Id}
						proposalEditForm={proposalEditForm}
            detail={detail}
            initialdata={initialdata}
            updateProposalEditFormDetail={updateProposalEditFormDetail}
            onUpdateProposal={() => this.onUpdateProposal()}
            deleteProposalDetail={deleteProposalDetail}
            toggleModal={toggleModal}
					/>
        ))}
        <ProposalDetail
          initialdata={initialdata}
          modelNewProposalDetail={modelNewProposalDetail}
        />
			</div>
    );
  }
}

ProposalDetails.defaultProps = {
};

ProposalDetails.propTypes = {
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
  updateProposal: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
