import React, { Component } from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';

/* eslint-disable react/prefer-stateless-function */
export default class DateMDYYYY extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
			<span>{ moment(this.props.date).format('M/D/YYYY') }</span>
    );
	}
}

DateMDYYYY.propTypes = {
  date: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
};
