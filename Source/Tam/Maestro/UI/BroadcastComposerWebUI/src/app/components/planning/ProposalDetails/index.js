import React, { Component } from 'react';
import PropTypes from 'prop-types';

import ProposalDetail from 'Components/planning/ProposalDetail';

export default class ProposalDetails extends Component {
  constructor(props) {
    super(props);
		this.state = {};
	}

  render() {
		const { proposalEditForm } = this.props;
    return (
      <div id="proposal-details">
				<h4 style={{ textAlign: 'center' }}>Proposal Details</h4>
        {proposalEditForm.Details.map(detail => (
					<ProposalDetail
						key={detail.Id}
						proposalEditForm={proposalEditForm}
						detail={detail}
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
};
