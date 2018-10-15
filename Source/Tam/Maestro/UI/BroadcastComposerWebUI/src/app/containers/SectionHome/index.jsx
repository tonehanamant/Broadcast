import React from "react";
import CSSModules from "react-css-modules";

// import Logo from '../../../../assets/images/image-one2one-brand.png';
import styles from "./index.style.scss";

export const Home = () => (
  <section id="home-section">
    <div styleName="home-container">
      <h1>
        This screen/Home component is intended to resolve at the route
        /broadcast but is not in use because /broadcast is immediately
        redirected to /broadcast/post (or another section of the application) in
        screens/Main.
      </h1>
    </div>
  </section>
);

export default CSSModules(Home, styles);
