import React from "react";
import CSSModules from "react-css-modules";
import AppBody from "Components/body/AppBody";
import PageTitle from "Patterns/PageTitle";
import PageHeaderContainer from "Components/post/PageHeaderContainer";
import DataGridContainer from "Components/post/DataGridContainer";
import PostScrubbingModal from "Components/post/PostScrubbingModal";

import styles from "./index.style.scss";

export const SectionPost = () => (
  <div id="post-section">
    <AppBody>
      <PageTitle title="Post" />
      <PageHeaderContainer />
      <DataGridContainer />
    </AppBody>
    <PostScrubbingModal />
  </div>
);

export default CSSModules(SectionPost, styles);
