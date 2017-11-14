import React from 'react';
// import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';

import FooterBar from 'Components/footer/FooterBar';

import styles from './index.style.scss';

export const Footer = () => (
  <div id="app-footer">
    <FooterBar />
  </div>
);

export default CSSModules(Footer, styles);
