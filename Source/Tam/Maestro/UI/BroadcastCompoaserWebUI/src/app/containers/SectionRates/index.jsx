import React from 'react';
import CSSModules from 'react-css-modules';

// import Logo from '../../../../assets/images/image-one2one-brand.png';
import styles from './index.style.scss';

export const SectionRates = () => (
  <section id="rates-section">
    <div styleName="rates-container">
			<p>Rates Placeholder</p>
    </div>
  </section>
);

export default CSSModules(SectionRates, styles);
