import PropTypes from "prop-types";
import { SELECTION } from "../util/util";

export const MenuItemProps = PropTypes.shape({
  text: PropTypes.string.isRequired,
  key: PropTypes.string.isRequired,
  EVENT_HANDLER: PropTypes.func.isRequired,
  isShow: PropTypes.func
});
export const MenuItemsProps = PropTypes.arrayOf(MenuItemProps);

export const ContextMenuProps = PropTypes.shape({
  isRender: PropTypes.bool,
  isSelectBeforeOpen: PropTypes.bool,
  menuItems: MenuItemsProps,
  onShow: PropTypes.func,
  onHide: PropTypes.func
});

export const SelectionProps = PropTypes.oneOf([
  SELECTION.MULTI,
  SELECTION.SINGLE,
  SELECTION.NONE
]);

export const HocStateProps = PropTypes.objectOf(PropTypes.shape);

export const RowChildrenProps = PropTypes.oneOfType([
  PropTypes.arrayOf(PropTypes.element),
  PropTypes.element
]);
