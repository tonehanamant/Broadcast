import React, { Component } from "react";
import PropTypes from "prop-types";
import numeral from "numeral";

/* eslint-disable react/prefer-stateless-function */
export default class CurrencyDollarWhole extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    const { amount, dash } = this.props;

    // 0, null or undefined
    if (dash && !amount) {
      return "-";
    }

    return <span>{numeral(amount).format("$0,0")}</span>;
  }
}

CurrencyDollarWhole.propTypes = {
  amount: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  dash: PropTypes.bool
};

CurrencyDollarWhole.defaultProps = {
  dash: false
};
