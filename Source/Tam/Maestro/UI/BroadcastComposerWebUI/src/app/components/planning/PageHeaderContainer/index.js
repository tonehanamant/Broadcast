import React, { Component } from 'react';
// import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Row, Col, Button } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({}, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class PageHeaderContainer extends Component {
  // constructor(props) {
	// 	super(props);
	// 	this.SearchInputAction = this.SearchInputAction.bind(this);
  //   this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
	// }

	// SearchInputAction() {
	// 	this.props.getPostFiltered();
	// }

	// SearchSubmitAction(value) {
	// 	this.props.getPostFiltered(value);
  // }

  render() {
    return (
			<Row>
				<Col xs={6}>
          <Button bsStyle="success" bsSize="small">Create New Proposal</Button>
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
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
