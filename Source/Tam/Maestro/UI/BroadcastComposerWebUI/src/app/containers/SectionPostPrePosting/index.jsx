import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import CSSModules from "react-css-modules";
import AppBody from "Components/body/AppBody";
import PageTitle from "Components/shared/PageTitle";
import PageHeaderContainer from "Components/postPrePosting/PageHeaderContainer";
import DataGridContainer from "Components/postPrePosting/DataGridContainer";
import PostPrePostingFileEditModal from "Components/postPrePosting/PostPrePostingFileEditModal";
import PostPrePostingFileUploadModal from "Components/postPrePosting/PostPrePostingFileUploadModal";
import Dropzone from "Components/shared/Dropzone";

import { toggleModal, storeFile } from "Ducks/app";
import {
  getPostPrePostingInitialData,
  getPostPrePosting
} from "Ducks/postPrePosting";

import styles from "./index.style.scss";

export class SectionPost extends Component {
  constructor(props) {
    super(props);

    this.processFiles = this.processFiles.bind(this);
  }

  componentWillMount() {
    this.props.getPostPrePostingInitialData();
    this.props.getPostPrePosting();
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

  render() {
    return (
      <div id="post-section">
        <Dropzone
          fileType="Excel"
          fileTypeExtension=".xlsx"
          processFiles={this.processFiles}
        >
          <AppBody>
            <PageTitle title="Post Pre Posting" />
            <PageHeaderContainer />
            <DataGridContainer />
            <PostPrePostingFileEditModal />
            <PostPrePostingFileUploadModal />
          </AppBody>
        </Dropzone>
      </div>
    );
  }
}

SectionPost.propTypes = {
  getPostPrePosting: PropTypes.func.isRequired,
  getPostPrePostingInitialData: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  storeFile: PropTypes.func.isRequired
};

const mapStateToProps = () => ({});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getPostPrePostingInitialData,
      getPostPrePosting,
      toggleModal,
      storeFile
    },
    dispatch
  );

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(SectionPost, styles));
