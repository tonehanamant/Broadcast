import React from "react";
import CSSModules from "react-css-modules";
import AppBody from "Components/body/AppBody";
import PageTitle from "Components/shared/PageTitle";
import PageHeaderContainer from "Components/tracker/PageHeaderContainer";
import DataGridContainer from "Components/tracker/DataGridContainer";
import TrackerScrubbingModal from "Components/tracker/TrackerScrubbingModal";

import styles from "./index.style.scss";

export const SectionTracker = () => (
  <div id="tracker-section">
    <AppBody>
      <PageTitle title="Tracker" />
      <PageHeaderContainer />
      <DataGridContainer />
    </AppBody>
    <TrackerScrubbingModal />
  </div>
);

export default CSSModules(SectionTracker, styles);
