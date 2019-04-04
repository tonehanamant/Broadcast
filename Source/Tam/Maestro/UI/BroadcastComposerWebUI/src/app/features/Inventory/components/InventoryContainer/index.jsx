import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
// import InventoryHeader from "Inventory/components/InventoryHeader";
// import InventoryCards from "Inventory/components/InventoryCards";

import { getInventoryInitialData } from "Inventory/redux/ducks";

import styles from "./index.style.scss";

export class SectionInventory extends Component {
  /* constructor(props) {
    super(props);
  } */

  componentWillMount() {
    const { getInventoryInitialData } = this.props;
    getInventoryInitialData();
  }

  render() {
    return (
      <div id="inventory-section">
        <AppBody>
          <PageTitle title="Inventory Management" />
          {/*  <InventoryHeader />
      <InventoryCards /> */}
        </AppBody>
      </div>
    );
  }
}

SectionInventory.propTypes = {
  getInventoryInitialData: PropTypes.func.isRequired
};

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getInventoryInitialData
    },
    dispatch
  );

export default connect(
  null,
  mapDispatchToProps
)(CSSModules(SectionInventory, styles));
