import React from "react";
import PropTypes from "prop-types";
import moment from "moment";

function DateMDYYYY({ date }) {
  return <span>{moment(date).format("M/D/YYYY")}</span>;
}

DateMDYYYY.propTypes = {
  date: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired
};

export default DateMDYYYY;
