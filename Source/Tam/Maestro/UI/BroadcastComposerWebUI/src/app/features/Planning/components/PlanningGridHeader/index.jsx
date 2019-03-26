import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { getPlanningFiltered } from "Ducks/planning";
import { Glyphicon, Button } from "react-bootstrap";
import { Checkbox, Menu, Dropdown } from "antd";
import { isNil } from "lodash";
import SearchInputButton from "Patterns/SearchInputButton";
import { columns } from "Planning/util/grid";
import CSSModules from "react-css-modules";

import styles from "./index.style.scss";

const generateColumnItems = onVisible => (
  <Menu>
    {columns
      .filter(c => isNil(c.show))
      .map(c => (
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

  onVisibleColumn(columnId, value) {
    const { visibleColumn } = this.props;
    visibleColumn(columnId, value);
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
    const { getPlanningFiltered } = this.props;
    getPlanningFiltered();
  }

  SearchSubmitAction(value) {
    const { getPlanningFiltered } = this.props;
    getPlanningFiltered(value);
  }

  render() {
    const { visible } = this.state;
    const cellItems = generateColumnItems(
      this.onVisibleColumn,
      this.handleMenuClick
    );

    return (
      <div styleName="planning-grid-header">
        <Button
          bsStyle="success"
          bsSize="small"
          onClick={openCreateProposalDetail}
        >
          Create New Proposal
        </Button>
        <div styleName="planning-grid-header_actions">
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
  visibleColumn: PropTypes.func.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(CSSModules(PageHeaderContainer, styles));
