import React, { Component } from 'react';
// import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Button, Collapse, Panel } from 'react-bootstrap';
// import Select from 'react-select';

import ProposalForm from 'Components/planning/ProposalForm';


const mapStateToProps = () => ({
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({}, dispatch)
);

export class ProposalHeader extends Component {
  constructor(props) {
		super(props);
		this.state = {};
		this.state.open = true;
  }

  render() {
    return (
      <div id="proposal-header">
				<h4>Create Planning Proposal</h4>
				<hr />
				<Button bsStyle="primary" onClick={() => this.setState({ open: !this.state.open })}>
					<span className="glyphicon glyphicon-triangle-bottom" aria-hidden="true" />
        </Button>
				<Collapse in={this.state.open}>
          <Panel style={{ marginTop: 10 }}>
						<ProposalForm />
					</Panel>
        </Collapse>
			</div>
    );
  }
}

ProposalHeader.defaultProps = {
};

/* eslint-disable react/no-unused-prop-types */
ProposalHeader.propTypes = {
};

export default connect(mapStateToProps, mapDispatchToProps)(ProposalHeader);
