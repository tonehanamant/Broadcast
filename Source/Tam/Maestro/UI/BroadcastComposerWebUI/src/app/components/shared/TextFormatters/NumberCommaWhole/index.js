import React, { Component } from 'react';
import PropTypes from 'prop-types';
import numeral from 'numeral';

/* eslint-disable react/prefer-stateless-function */
export default class NumberCommaWhole extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
			<span>{ numeral(this.props.number).format('0,0') }</span>
    );
	}
}

NumberCommaWhole.propTypes = {
  number: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
};
