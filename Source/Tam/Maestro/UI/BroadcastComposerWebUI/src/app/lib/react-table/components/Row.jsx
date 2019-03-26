import React from "react";
import PropTypes from "prop-types";
import classnames from "classnames";
import { ContextMenuRow } from "./ConextMenu";
import { RowChildrenProps } from "../propTypes/propTypes";

function Row({ children, className, isRender, rowInfo, ...rest }) {
  return (
    <ContextMenuRow isRender={isRender} row={rowInfo}>
      <div
        className={classnames("rt-tr-group", className)}
        role="rowgroup"
        {...rest}
      >
        {children}
      </div>
    </ContextMenuRow>
  );
}

Row.defaultProps = {
  isRender: false,
  rowInfo: null,
  className: ""
};

Row.propTypes = {
  children: RowChildrenProps.isRequired,
  className: PropTypes.string,
  rowInfo: PropTypes.objectOf(PropTypes.shape),
  isRender: PropTypes.bool
};

export default Row;
