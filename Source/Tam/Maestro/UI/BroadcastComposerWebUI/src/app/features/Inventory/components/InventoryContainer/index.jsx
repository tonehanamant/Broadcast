import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
// import InventoryHeader from "Inventory/components/InventoryHeader";
import InventoryCards from "Inventory/components/InventoryCards";

import { getInventoryInitialData } from "Inventory/redux/ducks";

import styles from "./index.style.scss";

export class SectionInventory extends Component {
  componentWillMount() {
    const { getInventoryInitialData } = this.props;
    getInventoryInitialData();
  }

  render() {
    const { cardsInventoryData, initialInventoryData } = this.props;
    return (
      <div id="inventory-section">
        <AppBody>
          <PageTitle title="Inventory Management" />
          {/* <InventoryHeader /> */}
          {cardsInventoryData && (
            <InventoryCards
              data={cardsInventoryData}
              initialData={initialInventoryData}
            />
          )}
        </AppBody>
      </div>
    );
  }
}

SectionInventory.propTypes = {
  getInventoryInitialData: PropTypes.func.isRequired,
  initialInventoryData: PropTypes.object,
  cardsInventoryData: PropTypes.array
};

SectionInventory.defaultProps = {
  initialInventoryData: null,
  cardsInventoryData: null
};

const mapStateToProps = ({
  inventory: { initialInventoryData, cardsInventoryData }
}) => ({
  initialInventoryData,
  cardsInventoryData
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getInventoryInitialData
    },
    dispatch
  );

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(SectionInventory, styles));
