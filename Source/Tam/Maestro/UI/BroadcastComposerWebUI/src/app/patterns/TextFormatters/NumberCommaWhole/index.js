import React from "react";
import PropTypes from "prop-types";
import numeral from "numeral";

function NumberCommaWhole({ number, dash }) {
  if (dash && !number) {
    return "-";
  }

  return <span>{numeral(number).format("0,0")}</span>;
}

NumberCommaWhole.propTypes = {
  number: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  dash: PropTypes.bool
};

NumberCommaWhole.defaultProps = {
  dash: false
};

export default NumberCommaWhole;
