import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
import PostPrePostingHeader from "PostPrePosting/components/PostPrePostingHeader";
import PostPrePostingGrid from "PostPrePosting/components/PostPrePostingGrid";
import PostPrePostingFileEditModal from "PostPrePosting/components/PostPrePostingFileEditModal";
import PostPrePostingFileUploadModal from "PostPrePosting/components/PostPrePostingFileUploadModal";
import Dropzone from "Patterns/Dropzone";

import { toggleModal, storeFile } from "Main/redux/ducks";
import {
  getPostPrePostingInitialData,
  getPostPrePosting
} from "PostPrePosting/redux/ducks";

import styles from "./index.style.scss";

export class SectionPost extends Component {
  constructor(props) {
    super(props);

    this.processFiles = this.processFiles.bind(this);
  }

  componentWillMount() {
    const { getPostPrePostingInitialData, getPostPrePosting } = this.props;
    getPostPrePostingInitialData();
    getPostPrePosting();
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
            <PostPrePostingHeader />
            <PostPrePostingGrid />
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
  null,
  mapDispatchToProps
)(CSSModules(SectionPost, styles));
