import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import CSSModules from 'react-css-modules';
import AppBody from 'Components/body/AppBody';
import PageTitle from 'Components/shared/PageTitle';
import PageHeaderContainer from 'Components/postPrePosting/PageHeaderContainer';
import DataGridContainer from 'Components/postPrePosting/DataGridContainer';
import PostPrePostingFileEditModal from 'Components/postPrePosting/PostPrePostingFileEditModal';
import PostPrePostingFileUploadModal from 'Components/postPrePosting/PostPrePostingFileUploadModal';
// import Dropzone from 'Components/shared/Dropzone';

import { getPostPrePostingInitialData, getPostPrePosting } from 'Ducks/postPrePosting';

import styles from './index.style.scss';


export class SectionPost extends Component {
  componentWillMount() {
    this.props.getPostPrePostingInitialData();
    this.props.getPostPrePosting();
  }

  render() {
    return (
      <div id="post-section">
        {/* <Dropzone
          acceptedMimeTypes="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
          fileType="Excel"
          fileTypeExtension=".xlsx"
          postProcessFiles={{
            toggleModal: {
              modal: 'postFileUploadModal',
              active: true,
            },
          }}
        > */}
        <AppBody>
            <PageTitle title="Post Pre Posting" />
            <PageHeaderContainer />
            <DataGridContainer />
            <PostPrePostingFileEditModal />
            <PostPrePostingFileUploadModal />
        </AppBody>
        {/* </Dropzone> */}
      </div>
    );
  }
}


SectionPost.propTypes = {
  getPostPrePosting: PropTypes.func.isRequired,
  getPostPrePostingInitialData: PropTypes.func.isRequired,
};

const mapStateToProps = () => ({});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ getPostPrePostingInitialData, getPostPrePosting }, dispatch)
);

export default connect(mapStateToProps, mapDispatchToProps)(CSSModules(SectionPost, styles));
