import React from "react";
import CSSModules from "react-css-modules";
import AppBody from "Patterns/layout/Body";
import PageTitle from "Patterns/PageTitle";
// import InventoryHeader from "Inventory/components/InventoryHeader";
// import InventoryCards from "Inventory/components/InventoryCards";

import styles from "./index.style.scss";

export const SectionInventory = () => (
  <div id="inventory-section">
    <AppBody>
      <PageTitle title="Inventory Management" />
      {/*  <InventoryHeader />
      <InventoryCards /> */}
    </AppBody>
  </div>
);

export default CSSModules(SectionInventory, styles);
