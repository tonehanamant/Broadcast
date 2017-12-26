import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Row, Col } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';
import UploadButton from 'Components/shared/UploadButton';

import { createAlert } from 'Ducks/app';
import { getPostPrePostingFiltered } from 'Ducks/postPrePosting';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ createAlert, getPostPrePostingFiltered }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class PageHeaderContainer extends Component {
  constructor(props) {
		super(props);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
	}

	SearchInputAction() {
		this.props.getPostPrePostingFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPostPrePostingFiltered(value);
  }

  render() {
    return (
			<Row>
				<Col xs={6}>
          <UploadButton
            text="Upload"
            bsStyle="success"
            bsSize="small"
            acceptedMimeTypes="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            fileType="Excel"
            fileTypeExtension=".xlsx"
            postProcessFiles={{
              toggleModal: {
                modal: 'postFileUploadModal',
                active: true,
              },
            }}
          />
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
  getPostPrePostingFiltered: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
