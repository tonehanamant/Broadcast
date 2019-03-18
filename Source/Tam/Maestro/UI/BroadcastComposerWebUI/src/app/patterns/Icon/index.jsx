import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import styles from "./index.style.scss";

export const Icon = ({ icon, iconType, iconSize, iconColor, spin, pulse }) => {
  let prefix = "fa";

  switch (iconType) {
    case "solid":
      prefix = "fas";
      break;
    case "regular":
      prefix = "far";
      break;
    case "light":
    default:
      prefix = "fal";
      break;
  }

  return (
    <div styleName={`${iconSize} ${iconColor}`} className="icon">
      <FontAwesomeIcon
        icon={[prefix, icon]}
        spin={spin}
        pulse={pulse}
        fixedWidth
      />
    </div>
  );
};

Icon.propTypes = {
  /**
   * icon string name. example, "file-export".
   * it should match an icon from: https://fontawesome.com/icons?d=gallery
   * @type {string}
   */
  icon: PropTypes.string.isRequired,
  /**
   * icon type.
   * it should match a prefix from the index.icons.js file.
   * @type {string}
   */
  iconType: PropTypes.oneOf(["solid", "regular", "light"]),
  /**
   * size of icon.
   * @type {string}
   */
  iconSize: PropTypes.oneOf([
    "tiny",
    "xxs",
    "xs",
    "sm",
    "md",
    "lg",
    "xl",
    "xxl",
    "major",
    "minor"
  ]),
  /**
   * color of icon.
   * @type {string}
   */
  iconColor: PropTypes.oneOf([
    "default",
    "primary",
    "primary-0",
    "primary-2",
    "primary-3",
    "primary-4",
    "secondary",
    "secondary-2",
    "secondary-3",
    "secondary-4",
    "tertiary",
    "tertiary-2",
    "tertiary-3",
    "tertiary-4",
    "gray-0",
    "gray-1",
    "gray-2",
    "gray-3",
    "gray-4",
    "invalid",
    "success",
    "warning",
    "transparent50"
  ]),
  /**
   * spinning animation of icon.
   * @type {bool}
   */
  spin: PropTypes.bool,
  /**
   * pulsing animation of icon.
   * @type {bool}
   */
  pulse: PropTypes.bool
};

Icon.defaultProps = {
  iconType: "light",
  iconColor: "default",
  iconSize: "xxs",
  spin: false,
  pulse: false
};

const IconStyled = CSSModules(Icon, styles, { allowMultiple: true });
export default IconStyled;
