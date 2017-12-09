import React from 'react';
import CSSModules from 'react-css-modules';
import AppBody from 'Components/body/AppBody';
import PageTitle from 'Components/shared/PageTitle';
import PageHeaderContainer from 'Components/post/PageHeaderContainer';
import DataGridContainer from 'Components/post/DataGridContainer';
import PostFileEditModal from 'Components/post/PostFileEditModal';
import PostFileUploadModal from 'Components/post/PostFileUploadModal';
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
          <PageTitle title="Posting" />
          <PageHeaderContainer />
          <DataGridContainer />
          <PostFileEditModal />
          <PostFileUploadModal />
      </AppBody>
    </Dropzone>
  </div>
);

export default CSSModules(SectionPost, styles);
