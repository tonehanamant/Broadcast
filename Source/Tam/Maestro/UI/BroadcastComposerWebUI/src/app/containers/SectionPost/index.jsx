import React from 'react';
import CSSModules from 'react-css-modules';
import AppBody from 'Components/body/AppBody';
import PageTitle from 'Components/shared/PageTitle';
// import PageHeaderContainer from 'Components/postPrePosting/PageHeaderContainer';
import DataGridContainer from 'Components/postPrePosting/DataGridContainer';
// import PostPrePostingFileEditModal from 'Components/postPrePosting/PostPrePostingFileEditModal';
// import PostPrePostingFileUploadModal from 'Components/postPrePosting/PostPrePostingFileUploadModal';

import styles from './index.style.scss';

export const SectionPost = () => (
  <div id="post-section">
    <AppBody>
        <PageTitle title="Post" />
        {/* <PageHeaderContainer /> */}
        <DataGridContainer />
        {/*
        <PostPrePostingFileEditModal />
        <PostPrePostingFileUploadModal /> */}
    </AppBody>
  </div>
);

export default CSSModules(SectionPost, styles);
