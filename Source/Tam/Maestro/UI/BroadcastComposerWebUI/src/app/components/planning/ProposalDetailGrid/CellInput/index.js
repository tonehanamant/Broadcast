import React, { Component } from 'react';
import PropTypes from 'prop-types';

import MaskedTextInput from 'react-text-mask';
import createNumberMask from 'text-mask-addons/dist/createNumberMask';

/* eslint-disable react/prefer-stateless-function */
export default class CellInput extends Component {
  constructor(props) {
		super(props);
		this.state = {
			inputValue: this.props.value,
		};
		this.onInputChange = this.onInputChange.bind(this);
	}

	onInputChange(event) {
		this.setState({
			inputValue: event.target.value,
		});
	}

  render() {
		const maskType = (type) => {
			switch (type) {
				case 'DollarThousandsDecimalsLimit3':
					return createNumberMask({
						prefix: '$',
						includeThousandsSeparator: true,
						allowDecimal: true,
						decimalLimit: 3,
					});
				case 'ImpressionsGoalDecimalsLimit3':
					return createNumberMask({
						prefix: 'Imp Goal (000) ',
						includeThousandsSeparator: false,
						allowDecimal: true,
						decimalLimit: 3,
					});
				case 'NumberDecimalsLimit3':
					return createNumberMask({
						prefix: '',
						includeThousandsSeparator: false,
						allowDecimal: true,
						decimalLimit: 3,
					});
				default:
					return false;
			}
		};


    return (
			<MaskedTextInput
				className={'editable-cell'}
				name={this.props.name}
				placeholder={this.props.placeholder}
				value={this.state.inputValue}
				onChange={this.onInputChange}
				mask={maskType(this.props.mask)}
			/>
    );
	}
}

CellInput.propTypes = {
	name: PropTypes.string.isRequired,
	placeholder: PropTypes.string.isRequired,
	value: PropTypes.string.isRequired,
	mask: PropTypes.string.isRequired,
};
