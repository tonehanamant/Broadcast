import React from "react";
import { ContextMenuTrigger } from "react-contextmenu";
import PropTypes from "prop-types";
import { RowChildrenProps } from "../../propTypes/propTypes";
import { contextMenuId } from "./util";

import "../../style/ContextMenuRow.css";

function ContextMenuRow({ isRender, row, children }) {
  if (!(isRender && row)) {
    return children;
  }

  return (
    <ContextMenuTrigger id={contextMenuId} collect={() => row}>
      {children}
    </ContextMenuTrigger>
  );
}

ContextMenuRow.defaultProps = {
  isRender: true,
  row: null
};

ContextMenuRow.propTypes = {
  children: RowChildrenProps.isRequired,
  row: PropTypes.any,
  isRender: PropTypes.bool
};

export default ContextMenuRow;
