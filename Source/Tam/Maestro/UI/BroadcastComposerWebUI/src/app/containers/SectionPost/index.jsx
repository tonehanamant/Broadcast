import React from 'react';
import CSSModules from 'react-css-modules';
import AppBody from 'Components/body/AppBody';
import PageTitle from 'Components/shared/PageTitle';
// import PageHeaderContainer from 'Components/post/PageHeaderContainer';
import DataGridContainer from 'Components/post/DataGridContainer';

import styles from './index.style.scss';

export const SectionPost = () => (
  <div id="post-section">
    <AppBody>
        <PageTitle title="Post" />
        {/* <PageHeaderContainer /> */}
        <DataGridContainer />
    </AppBody>
  </div>
);

export default CSSModules(SectionPost, styles);
