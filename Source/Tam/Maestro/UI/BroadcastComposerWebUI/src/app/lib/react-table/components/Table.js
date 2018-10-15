/* eslint-disable no-underscore-dangle */
import React, { Component, Fragment } from "react";
import PropTypes from "prop-types";
import ReactTable from "react-table";

import "react-table/react-table.css";

import Row from "./Row";
import { rowStyle, selectedRowStyle } from "../style/Selection";
import { rowSelection, SELECTION } from "../util/util";
import { DynamicMenu } from "./ConextMenu/index";
import {
  ContextMenuProps,
  HocStateProps,
  SelectionProps
} from "../propTypes/propTypes";

import "../style/Table.css";

class Table extends Component {
  constructor(props) {
    super(props);

    this.onShowContextMenu = this.onShowContextMenu.bind(this);
    this.onHideContextMenu = this.onHideContextMenu.bind(this);
    this.onRowClick = this.onRowClick.bind(this);
    this._getTrProps = this._getTrProps.bind(this);
    this._getTrGroupProps = this._getTrGroupProps.bind(this);
    this._TrGroupComponent = this._TrGroupComponent.bind(this);
  }

  onShowContextMenu({
    detail: {
      data: { row }
    }
  }) {
    const {
      contextMenu: { onShow, isSelectBeforeOpen }
    } = this.props;
    if (onShow) {
      onShow();
    }
    if (isSelectBeforeOpen) {
      this.onRowClick(row._index, row);
    }
  }

  onHideContextMenu() {
    const {
      contextMenu: { onHide }
    } = this.props;
    if (onHide) {
      onHide();
    }
  }

  onRowClick(index) {
    const {
      dispatch,
      selection,
      hocState: { selected }
    } = this.props;
    if (rowSelection[selection] && !selected.includes(index)) {
      dispatch(rowSelection[selection](index));
    }
  }

  _getTrProps(state, rowInfo) {
    if (!rowInfo) return {};

    const {
      hocState: { selected },
      getTrProps
    } = this.props;
    const rowStyles = selected.includes(rowInfo.index)
      ? selectedRowStyle
      : rowStyle;
    let trProps = {};
    if (getTrProps) {
      trProps = getTrProps(state, rowInfo);
    }
    return {
      ...trProps,
      onClick: () => {
        this.onRowClick(rowInfo.index, rowInfo);
        if (trProps.onClick) {
          trProps.onClick();
        }
      },
      style: Object.assign({}, rowStyles, trProps.style)
    };
  }

  _getTrGroupProps(state, rowInfo) {
    if (!rowInfo) return {};

    const { getTrGroupProps } = this.props;
    let trGroupProps = {};
    if (getTrGroupProps) {
      trGroupProps = getTrGroupProps(state, rowInfo);
    }
    return {
      ...trGroupProps,
      rowInfo
    };
  }

  _TrGroupComponent({ children, className, rowInfo, ...rest }) {
    const {
      contextMenu: { isRender }
    } = this.props;
    return (
      <Row
        isRender={isRender}
        className={className}
        rowInfo={rowInfo}
        {...rest}
      >
        {children}
      </Row>
    );
  }

  render() {
    const {
      contextMenu: { menuItems },
      showPagination,
      showPageSizeOptions,
      defaultPageSize,
      data
    } = this.props;

    return (
      <Fragment>
        <ReactTable
          {...this.props}
          getTrProps={this._getTrProps}
          getTrGroupProps={this._getTrGroupProps}
          TrGroupComponent={this._TrGroupComponent}
          showPagination={showPagination}
          showPageSizeOptions={showPageSizeOptions}
          defaultPageSize={defaultPageSize || data.length}
          pageSize={defaultPageSize || data.length}
        />
        {!!menuItems && (
          <DynamicMenu
            menuItems={menuItems}
            onShow={this.onShowContextMenu}
            onHide={this.onHideContextMenu}
          />
        )}
      </Fragment>
    );
  }
}

Table.defaultProps = {
  data: [],
  selection: SELECTION.NONE,
  contextMenu: {},
  getTrGroupProps: undefined,
  getTrProps: undefined,
  showPageSizeOptions: false,
  showPagination: false,
  defaultPageSize: undefined
};

Table.propTypes = {
  data: PropTypes.any,
  hocState: HocStateProps.isRequired,
  dispatch: PropTypes.func.isRequired,
  getTrGroupProps: PropTypes.func,
  getTrProps: PropTypes.func,
  contextMenu: ContextMenuProps,
  selection: SelectionProps,
  showPagination: PropTypes.bool,
  showPageSizeOptions: PropTypes.bool,
  defaultPageSize: PropTypes.number
};

export default Table;
