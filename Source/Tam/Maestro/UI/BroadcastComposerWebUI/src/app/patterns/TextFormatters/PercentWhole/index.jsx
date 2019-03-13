import React from "react";
import PropTypes from "prop-types";
import numeral from "numeral";

function PercentWhole({ percent }) {
  return <span>{numeral(percent / 100).format("0%")}</span>;
}

PercentWhole.propTypes = {
  percent: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired
};

export default PercentWhole;
