import React, { Component } from "react";
import PropTypes from "prop-types";
import numeral from "numeral";

/* eslint-disable react/prefer-stateless-function */
export default class PercentWhole extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return <span>{numeral(this.props.percent / 100).format("0%")}</span>;
  }
}

PercentWhole.propTypes = {
  percent: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired
};
