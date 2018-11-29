import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { getPlanningFiltered } from "Ducks/planning";
import { Glyphicon, Button } from "react-bootstrap";
import { Checkbox, Menu, Dropdown } from "antd";
import { isNil } from "lodash";
import SearchInputButton from "Components/shared/SearchInputButton";

import "./index.scss";

const generateColumnItems = (columns, onVisible) => (
  <Menu>
    {columns.filter(c => isNil(c.show)).map(c => (
      <Menu.Item key={`column-item-${c.id}`}>
        <Checkbox
          defaultChecked
          key={`column-item-${c.id}`}
          onChange={e => onVisible(c.id, e.target.checked)}
        >
          {c.Header}
        </Checkbox>
      </Menu.Item>
    ))}
  </Menu>
);

const openCreateProposalDetail = () => {
  const url = "/broadcastreact/planning/proposal/create";
  window.location.assign(url);
};

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getPlanningFiltered
    },
    dispatch
  );

export class PageHeaderContainer extends Component {
  constructor(props) {
    super(props);
    this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.onVisibleColumn = this.onVisibleColumn.bind(this);
    this.handleMenuClick = this.handleMenuClick.bind(this);
    this.handleVisibleChange = this.handleVisibleChange.bind(this);

    this.state = {
      visible: false
    };
  }

  handleMenuClick(e) {
    if (e.key) {
      this.setState({ visible: false });
    }
  }

  handleVisibleChange(flag) {
    this.setState({ visible: flag });
  }

  SearchInputAction() {
    this.props.getPlanningFiltered();
  }

  SearchSubmitAction(value) {
    this.props.getPlanningFiltered(value);
  }

  onVisibleColumn(columnId, value) {
    const { visibleColumn } = this.props;
    visibleColumn(columnId, value);
  }

  render() {
    const { visible } = this.state;
    const { columns } = this.props;
    const cellItems = generateColumnItems(
      columns,
      this.onVisibleColumn,
      this.handleMenuClick
    );

    return (
      <div className="planning-grid-header">
        <Button
          bsStyle="success"
          bsSize="small"
          onClick={openCreateProposalDetail}
        >
          Create New Proposal
        </Button>
        <div className="planning-grid-header_actions">
          <SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Filter..."
          />
          <Dropdown
            trigger={["click"]}
            overlay={cellItems}
            onVisibleChange={this.handleVisibleChange}
            visible={visible}
          >
            <Glyphicon glyph="menu-hamburger" />
          </Dropdown>
        </div>
      </div>
    );
  }
}

PageHeaderContainer.propTypes = {
  getPlanningFiltered: PropTypes.func.isRequired,
  visibleColumn: PropTypes.func.isRequired,
  columns: PropTypes.array.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(PageHeaderContainer);
