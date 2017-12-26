import React from 'react';
import CSSModules from 'react-css-modules';
import AppBody from 'Components/body/AppBody';
import PageTitle from 'Components/shared/PageTitle';
import PageHeaderContainer from 'Components/postPrePosting/PageHeaderContainer';
import DataGridContainer from 'Components/postPrePosting/DataGridContainer';
import PostPrePostingFileEditModal from 'Components/postPrePosting/PostPrePostingFileEditModal';
import PostPrePostingFileUploadModal from 'Components/postPrePosting/PostPrePostingFileUploadModal';
import Dropzone from 'Components/shared/Dropzone';

import styles from './index.style.scss';

export const SectionPost = () => (
  <div id="post-section">
    <Dropzone
      acceptedMimeTypes="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
      fileType="Excel"
      fileTypeExtension=".xlsx"
      postProcessFiles={{
        toggleModal: {
          modal: 'postFileUploadModal',
          active: true,
        },
      }}
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

export default CSSModules(SectionPost, styles);
