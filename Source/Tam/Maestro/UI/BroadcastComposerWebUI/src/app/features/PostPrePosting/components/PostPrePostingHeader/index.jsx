import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import { Row, Col } from "react-bootstrap";
import SearchInputButton from "Patterns/SearchInputButton";
import UploadButton from "Patterns/UploadButton";

import { toggleModal, storeFile } from "Main/redux/ducks";

import { getPostPrePostingFiltered } from "PostPrePosting/redux/ducks";

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getPostPrePostingFiltered,
      toggleModal,
      storeFile
    },
    dispatch
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
      modal: "postFileUploadModal",
      active: true,
      properties: {}
    });
  }

  SearchInputAction() {
    const { getPostPrePostingFiltered } = this.props;
    getPostPrePostingFiltered();
  }

  SearchSubmitAction(value) {
    const { getPostPrePostingFiltered } = this.props;
    getPostPrePostingFiltered(value);
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
  storeFile: PropTypes.func.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(PageHeaderContainer);
