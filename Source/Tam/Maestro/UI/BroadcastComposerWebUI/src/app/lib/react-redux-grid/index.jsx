import React from "react";
import { Grid as GridComponent, Actions } from "react-redux-grid";
import { StoreContext } from "AppConfigs";

function Grid(props) {
  return (
    <StoreContext.Consumer>
      {context => <GridComponent {...props} store={context} />}
    </StoreContext.Consumer>
  );
}

export { Grid, Actions };
