import React from "react";
import { map, flow, filter } from "lodash/fp";
import { MenuItem } from "react-contextmenu";

const contextMenuId = "react-table-context-menu";

/**
 * Check if possible to render menu item. Cases:
 *
 * 1. if exist function [isShow] and [rowDetail],
 * then execute isShow,
 * this function returns value from menu items configuration array.
 *
 * 2. if function [isShow] is not exist,
 * then check if exist rowDetail.
 * !!! this case related to react-contextmenu.
 * when user reopen menu on the same row,
 * they don't pass detail properties
 * */
const isRenderMenuItem = (isShow, rowDetail) =>
  isShow && rowDetail ? isShow(rowDetail) : !!rowDetail;

const filterMenuItems = rowDetail => ({ isShow }) =>
  isRenderMenuItem(isShow, rowDetail);

// eslint-disable-next-line react/prop-types
const mapMenuItems = ({ key, text, EVENT_HANDLER }) => (
  <MenuItem
    key={key}
    onClick={(event, { original }) => {
      EVENT_HANDLER({ metaData: { rowData: original } });
    }}
  >
    {text}
  </MenuItem>
);

const generateMenuItems = rowDetail =>
  flow([filter(filterMenuItems(rowDetail)), map(mapMenuItems)]);

export { generateMenuItems, contextMenuId };
