import React, { Component } from 'react';
import PropTypes from 'prop-types';
import numeral from 'numeral';

/* eslint-disable react/prefer-stateless-function */
export default class CurrencyDollarWhole extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
			<span>{ numeral(this.props.amount).format('$0,0') }</span>
    );
	}
}

CurrencyDollarWhole.propTypes = {
  amount: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
};
