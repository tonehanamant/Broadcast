import React from 'react';
import CSSModules from 'react-css-modules';

import styles from './index.style.scss';

export const SectionPlanning = () => (
  <section id="planning-section">
    <div styleName="planning-container">
			<p>Planning Placeholder</p>
    </div>
  </section>
);

export default CSSModules(SectionPlanning, styles);
