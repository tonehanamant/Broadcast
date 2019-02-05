import React from "react";
import PropTypes from "prop-types";
import numeral from "numeral";

function CurrencyDollarWhole({ amount, dash }) {
  if (dash && !amount) {
    return "-";
  }
  return <span>{numeral(amount).format("$0,0")}</span>;
}

CurrencyDollarWhole.propTypes = {
  amount: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  dash: PropTypes.bool
};

CurrencyDollarWhole.defaultProps = {
  dash: false
};

export default CurrencyDollarWhole;
