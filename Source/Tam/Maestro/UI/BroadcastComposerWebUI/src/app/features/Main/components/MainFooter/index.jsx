import React from "react";
// import PropTypes from 'prop-types';
import CSSModules from "react-css-modules";

import FooterBar from "./MainFooterBar";

import styles from "./index.style.scss";

export const MainFooter = () => (
  <div id="app-footer">
    <FooterBar />
  </div>
);

export default CSSModules(MainFooter, styles);
