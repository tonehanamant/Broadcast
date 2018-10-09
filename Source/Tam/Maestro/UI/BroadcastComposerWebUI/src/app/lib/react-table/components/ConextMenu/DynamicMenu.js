import React from "react";
import PropTypes from "prop-types";
import { ContextMenu, connectMenu } from "react-contextmenu";

import { contextMenuId, generateMenuItems } from "./util";

import { MenuItemsProps } from "../../propTypes/propTypes";

function DynamicMenu({ menuItems, onShow, onHide, trigger }) {
  return (
    <ContextMenu id={contextMenuId} onShow={onShow} onHide={onHide}>
      {generateMenuItems(trigger)(menuItems)}
    </ContextMenu>
  );
}

DynamicMenu.defaultProps = {
  onShow: undefined,
  onHide: undefined,
  trigger: undefined
};

DynamicMenu.propTypes = {
  menuItems: MenuItemsProps.isRequired,
  onShow: PropTypes.func,
  onHide: PropTypes.func,
  trigger: PropTypes.any
};

const ConnectedMenu = connectMenu(contextMenuId)(DynamicMenu);

export default ConnectedMenu;
