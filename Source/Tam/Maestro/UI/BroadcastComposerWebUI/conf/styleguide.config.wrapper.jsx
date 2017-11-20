import React from 'react';
import PropTypes from 'prop-types';
import { Provider } from 'react-redux';
import configureStore from '../src/index.store';

const store = configureStore();

export const Wrapper = props => (
  <Provider store={store}>
    {props.children}
  </Provider>
);

Wrapper.propTypes = {
  children: PropTypes.node.isRequired,
};

export default Wrapper;
