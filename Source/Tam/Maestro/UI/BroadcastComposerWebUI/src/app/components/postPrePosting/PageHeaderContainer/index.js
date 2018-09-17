import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Row, Col } from 'react-bootstrap';
import SearchInputButton from 'Components/shared/SearchInputButton';
import UploadButton from 'Components/shared/UploadButton';

import { toggleModal, storeFile } from 'Ducks/app';

import { getPostPrePostingFiltered } from 'Ducks/postPrePosting';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
     getPostPrePostingFiltered,
     toggleModal,
     storeFile,
    }, dispatch)
);

export class PageHeaderContainer extends Component {
  constructor(props) {
		super(props);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.processFiles = this.processFiles.bind(this);
  }

  processFiles(file) {
    const { storeFile, toggleModal } = this.props;
    storeFile(file);
    toggleModal({
      modal: 'postFileUploadModal',
      active: true,
      properties: {},
    });
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
            fileTypeExtension=".xlsx"
            processFiles={this.processFiles}
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
  toggleModal: PropTypes.func.isRequired,
  storeFile: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
