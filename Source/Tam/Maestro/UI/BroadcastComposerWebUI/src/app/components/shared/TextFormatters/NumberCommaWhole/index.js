import React, { Component } from 'react';
import PropTypes from 'prop-types';
import numeral from 'numeral';

/* eslint-disable react/prefer-stateless-function */
export default class NumberCommaWhole extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    const { number, dash } = this.props;

    // 0, null or undefined
    if (dash && !number) {
      return '-';
    }

    return (
			<span>{ numeral(number).format('0,0') }</span>
    );
	}
}

NumberCommaWhole.propTypes = {
  number: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
  dash: PropTypes.bool,
};

NumberCommaWhole.defaultProps = {
  dash: false,
};
