import React from "react";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
import PostHeader from "Post/components/PostHeader";
import PostGrid from "Post/components/PostGrid";
import PostScrubbingModal from "Post/sub-features/Scrubbing/components/ScrubbingModal";

import styles from "./index.style.scss";

export const SectionPost = () => (
  <div id="post-section">
    <AppBody>
      <PageTitle title="Post" />
      <PostHeader />
      <PostGrid />
    </AppBody>
    <PostScrubbingModal />
  </div>
);

export default CSSModules(SectionPost, styles);
