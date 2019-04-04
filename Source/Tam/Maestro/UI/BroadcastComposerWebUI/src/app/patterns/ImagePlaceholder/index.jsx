import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";

import styles from "./index.style.scss";

export const ImagePlaceholder = ({ size }) => (
  <div styleName={`image-placeholder ${size}`} />
);

ImagePlaceholder.propTypes = {
  size: PropTypes.oneOf(["card"])
};

ImagePlaceholder.defaultProps = {
  size: "card"
};

export default CSSModules(ImagePlaceholder, styles, { allowMultiple: true });
