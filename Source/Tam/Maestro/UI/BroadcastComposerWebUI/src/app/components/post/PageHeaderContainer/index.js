import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Row, Col, Button } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';

import { createAlert } from 'Ducks/app';
import { getPostFiltered } from 'Ducks/post';

/* const mapStateToProps = ({ routing }) => ({
  routing,
}); */
const mapStateToProps = ({ post: { post } }) => ({
  post,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ createAlert, getPostFiltered }, dispatch)
);

export class PageHeaderContainer extends Component {
  constructor(props) {
		super(props);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
	}

	SearchInputAction() {
		this.props.getPostFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPostFiltered(value);
  }

  render() {
    const unlinkedNumber = this.props.post.UnlinkedIscis;
    const showUnlinked = (unlinkedNumber !== 0);
    const unlinkedText = `Unlinked ISCIs (${unlinkedNumber})`;
    return (
			<Row>
				<Col xs={6}>
        {
          showUnlinked &&
          <Button bsStyle="success" bsSize="small">{unlinkedText}</Button>
        }
				</Col>
        <Col xs={6}>
					<SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Search..."
					/>
				</Col>
			</Row>
    );
	}
}

PageHeaderContainer.propTypes = {
  getPostFiltered: PropTypes.func.isRequired,
  post: PropTypes.object.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
