import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import { withRouter } from "react-router";
import { Icon } from "Patterns/Icon";

import styles from "./index.style.scss";

export const CompanyCardSubMenu = ({ toggleMenu, menuOptions }) => (
  <div styleName="sub-menu">
    <div styleName="sub-menu-header">
      <button type="button" onClick={toggleMenu}>
        <Icon
          icon="times"
          iconType="light"
          iconSize="sm"
          iconColor="tertiary"
        />
      </button>
    </div>
    <div styleName="sub-menu-options">
      {menuOptions.map(({ label, action }) => (
        <div key={label} styleName="sub-menu-options-link">
          <button type="button" onClick={action}>
            {label}
          </button>
        </div>
      ))}
    </div>
  </div>
);

CompanyCardSubMenu.propTypes = {
  toggleMenu: PropTypes.func.isRequired,
  menuOptions: PropTypes.array.isRequired
};

const CompanyCardSubMenuStyled = CSSModules(CompanyCardSubMenu, styles);

export default withRouter(CompanyCardSubMenuStyled);
