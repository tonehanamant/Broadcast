import React from "react";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
import TrackerHeader from "Tracker/components/TrackerHeader";
import TrackerGrid from "Tracker/components/TrackerGrid";
import TrackerScrubbingModal from "Tracker/sub-features/Scrubbing/components/ScrubbingModal";

import styles from "./index.style.scss";

export const SectionTracker = () => (
  <div id="tracker-section">
    <AppBody>
      <PageTitle title="Tracker" />
      <TrackerHeader />
      <TrackerGrid />
    </AppBody>
    <TrackerScrubbingModal />
  </div>
);

export default CSSModules(SectionTracker, styles);
