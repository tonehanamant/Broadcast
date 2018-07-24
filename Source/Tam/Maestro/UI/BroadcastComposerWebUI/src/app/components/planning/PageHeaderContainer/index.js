import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { getPlanningFiltered } from 'Ducks/planning';
import { Row, Col, Button } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ getPlanningFiltered }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class PageHeaderContainer extends Component {
  constructor(props) {
    super(props);
    console.log(this.props);
    this.openCreateProposalDetail = this.openCreateProposalDetail.bind(this);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
  }

	SearchInputAction() {
		this.props.getPlanningFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPlanningFiltered(value);
  }

  /* eslint-disable class-methods-use-this */
  openCreateProposalDetail() {
		const url = 'planning/proposal/create';
    window.location.assign(url);
	}

  render() {
    return (
			<Row>
				<Col xs={6}>
          <Button
            bsStyle="success"
            bsSize="small"
            onClick={this.openCreateProposalDetail}
          >Create New Proposal
          </Button>
				</Col>
        <Col xs={6}>
					<SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Filter..."
					/>
				</Col>
			</Row>
    );
	}
}

PageHeaderContainer.propTypes = {
  getPlanningFiltered: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
