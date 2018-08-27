import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { FormControl } from 'react-bootstrap';

const inputStyle = {
  borderRadius: 'unset',
  fontSize: 11,
  fontFamily: 'Verdana, Arial, sans-serif',
  padding: 0,
};

export default class GridTextInput extends Component {
  constructor(props) {
		super(props);
		this.state = {
			touched: false,
			inputValue: this.props.value,
		};
		this.onInputChange = this.onInputChange.bind(this);
		this.onKeyPress = this.onKeyPress.bind(this);
		this.onBlur = this.onBlur.bind(this);
  }

	componentWillReceiveProps(nextProps) {
		if (nextProps.value !== this.props.value) {
      this.setState({
        inputValue: nextProps.value,
      });
    }
  }

  onKeyPress(event) {
    const { onKeyPress } = this.props;
    if (event.key === 'Enter') {
      event.currentTarget.blur(event);
    } else {
      onKeyPress(event);
    }
  }

  onBlur(event) {
    const { enterKeyPressAction } = this.props;
    enterKeyPressAction(event);
  }

	onInputChange(event) {
    this.setState({
      inputValue: event.target.value,
    });
	}

  render() {
    const { touched, inputValue } = this.state;
    const { isEditable, isGridCellEdited, onSaveShowValidation, maxLength, placeholder } = this.props;
		const editableClass = isEditable ? 'editable-cell' : 'non-editable-cell';
		const touchedClass = (touched && isEditable && isGridCellEdited) ? 'editable-cell-changed' : '';
		const hasErrorTouchedClass = (touched && (inputValue === 0 || inputValue === '0')) && isEditable ? 'editable-cell-has-error' : '';
    const hasErrorOnSaveClass = (onSaveShowValidation && (inputValue === 0 || inputValue === '0')) && isEditable ? 'editable-cell-has-error' : '';
    const className = `${editableClass} ${touchedClass} ${hasErrorTouchedClass} ${hasErrorOnSaveClass}`;

    return (
      <FormControl
				className={className}
        type="text"
        style={inputStyle}
        onChange={this.onInputChange}
        maxLength={maxLength}
				disabled={!isEditable}
				placeholder={placeholder}
        value={inputValue}
        onKeyPress={this.onKeyPress}
        onBlur={this.onBlur}
      />
    );
	}
}

GridTextInput.defaultProps = {
	isEditable: true,
	onSaveShowValidation: false,
	isGridCellEdited: false,
  placeholder: '',
	blurAction: () => {},
  enterKeyPressAction: () => {},
  onKeyPress: () => {},
  maxLength: 500,
};

GridTextInput.propTypes = {
	placeholder: PropTypes.string,
	value: PropTypes.string.isRequired,
	isEditable: PropTypes.bool,
	onSaveShowValidation: PropTypes.bool,
  onKeyPress: PropTypes.func,
  enterKeyPressAction: PropTypes.func,
  maxLength: PropTypes.number,
	isGridCellEdited: PropTypes.bool,
};
