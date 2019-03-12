import React from "react";
import PropTypes from "prop-types";
import { ContextMenuProvider, Item, ContextMenu } from "react-contexify";
import { map } from "lodash/fp";

import "react-contexify/dist/ReactContexify.min.css";
import "./index.scss";

const onClick = ({ data }) => {
  const { EVENT_HANDLER, row } = data;
  EVENT_HANDLER({ metaData: { rowData: row.toJS() } });
};

const generateMenuItems = row =>
  map(({ key, text, EVENT_HANDLER }) => (
    <Item key={key} onClick={onClick} data={{ EVENT_HANDLER, row }}>
      {text}
    </Item>
  ));

const ContextMenuRow = props => {
  const {
    rowProps,
    row,
    menuItems,
    isRender,
    stateKey,
    beforeOpenMenu
  } = props;

  if (!(isRender && menuItems)) {
    return (
      <tr
        {...rowProps}
        onContextMenu={e => {
          e.preventDefault();
        }}
      >
        {props.children}
      </tr>
    );
  }

  const rowId = row.get("_key");
  const menuId = `${stateKey}-${rowId}`;
  const { children } = props;
  return (
    <>
      <ContextMenuProvider
        id={menuId}
        storeRef={false}
        render={({ children, ...rest }) => {
          const onContextMenu = e => {
            rest.onContextMenu(e);
            beforeOpenMenu(rowId);
          };
          return (
            <tr
              {...rowProps}
              {...rest}
              onContextMenu={onContextMenu}
              className={rowProps.className}
            >
              {children}
            </tr>
          );
        }}
      >
        {children}
      </ContextMenuProvider>
      <ContextMenu id={menuId}>{generateMenuItems(row)(menuItems)}</ContextMenu>
    </>
  );
};

ContextMenuRow.defaultProps = {
  isRender: true,
  menuItems: undefined,
  beforeOpenMenu: () => {}
};

ContextMenuRow.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.element),
    PropTypes.element
  ]).isRequired,
  menuItems: PropTypes.arrayOf(PropTypes.object),
  rowProps: PropTypes.any.isRequired,
  row: PropTypes.any.isRequired,
  stateKey: PropTypes.string.isRequired,
  isRender: PropTypes.bool,
  beforeOpenMenu: PropTypes.func
};

export default ContextMenuRow;
