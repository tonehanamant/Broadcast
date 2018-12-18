/* eslint-disable no-underscore-dangle */
import React, { Component, Fragment } from "react";
import PropTypes from "prop-types";
import ReactTable from "react-table";

import "react-table/react-table.css";

import Row from "./Row";
import { rowStyle, selectedRowStyle } from "../style/Selection";
import { rowSelection, SELECTION, generetaColumns } from "../util/util";
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

    this.state = {
      isSelectedFirstTime: false
    };

    this.onShowContextMenu = this.onShowContextMenu.bind(this);
    this.onHideContextMenu = this.onHideContextMenu.bind(this);
    this.onRowClick = this.onRowClick.bind(this);
    this.onFirstSelect = this.onFirstSelect.bind(this);
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

  onRowClick(index, row) {
    const {
      dispatch,
      selection,
      onSelect,
      hocState: { selected }
    } = this.props;
    if (rowSelection[selection] && !selected.includes(index)) {
      onSelect(index, row);
      dispatch(rowSelection[selection](index));
    }
  }

  onFirstSelect(onClick) {
    const { isSelectedFirstTime } = this.state;
    const { selectOnRender } = this.props;
    if (selectOnRender && !isSelectedFirstTime) {
      onClick();
      this.setState({ isSelectedFirstTime: true });
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
    const onClick = () => {
      this.onRowClick(rowInfo.index, rowInfo);
      if (trProps.onClick) {
        trProps.onClick();
      }
    };
    if (rowInfo.index === 0) {
      this.onFirstSelect(onClick);
    }
    return {
      ...trProps,
      onClick,
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
      columns,
      hocState: { displayColumns },
      data
    } = this.props;

    const tableColumns = generetaColumns(columns, displayColumns);

    return (
      <Fragment>
        <ReactTable
          {...this.props}
          columns={tableColumns}
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
  columns: [],
  selection: SELECTION.NONE,
  contextMenu: {},
  getTrGroupProps: undefined,
  getTrProps: undefined,
  showPageSizeOptions: false,
  showPagination: false,
  selectOnRender: false,
  onSelect: () => {},
  defaultPageSize: undefined
};

Table.propTypes = {
  columns: PropTypes.any,
  data: PropTypes.any,
  hocState: HocStateProps.isRequired,
  dispatch: PropTypes.func.isRequired,
  getTrGroupProps: PropTypes.func,
  onSelect: PropTypes.func,
  getTrProps: PropTypes.func,
  contextMenu: ContextMenuProps,
  selection: SelectionProps,
  showPagination: PropTypes.bool,
  showPageSizeOptions: PropTypes.bool,
  selectOnRender: PropTypes.bool,
  defaultPageSize: PropTypes.number
};

export default Table;
