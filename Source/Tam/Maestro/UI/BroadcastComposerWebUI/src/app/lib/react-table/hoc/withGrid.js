import React, { Component } from 'react';
import { isEqual } from 'lodash';
import reducer, { initialState } from '../actions/reducer';

export const GridContext = React.createContext();

function withGrid(WrappedComponent) {
  return class Wrapper extends Component {
    constructor(props) {
      super(props);
      this.dispatch = this.dispatch.bind(this);
      this.handleAction = this.handleAction.bind(this);

      this.state = initialState;
    }

    dispatch({ type, ...payload }) {
      const action = reducer[type];
      if (action) {
        this.handleAction(action, type, payload);
      } else {
        console.exception(`Action is not found. Type - ${type}.`);
      }
    }

    handleAction(action, type, payload) {
      const nextState = action(this.state, payload);
      if (!nextState) {
        console.exception(`Action is not defined. Type - ${type}.`);
      }
      if (!isEqual(this.state, nextState)) {
        this.setState(nextState);
      }
    }


    render() {
      return (
        <GridContext.Provider value={{ dispatch: this.dispatch, hocState: this.state }}>
            <WrappedComponent {...this.props} {...this.state} dispatch={this.dispatch} />
        </GridContext.Provider>
      );
    }
  };
}

export default withGrid;
