import React, { Component } from 'react';
import PropTypes from 'prop-types';

import ProposalDetail from 'Components/planning/ProposalDetail';

export default class ProposalDetails extends Component {
  constructor(props) {
    super(props);
		this.state = {};
	}

  render() {
		const { proposalEditForm, initialdata, updateProposalEditFormDetail, deleteProposalDetail, toggleModal } = this.props;
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
            deleteProposalDetail={deleteProposalDetail}
            toggleModal={toggleModal}
					/>
				))}
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
  deleteProposalDetail: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
