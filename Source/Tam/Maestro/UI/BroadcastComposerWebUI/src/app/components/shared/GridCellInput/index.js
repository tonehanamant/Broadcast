import React, { Component } from "react";
import PropTypes from "prop-types";

import MaskedTextInput from "react-text-mask";
import createNumberMask from "text-mask-addons/dist/createNumberMask";

/* eslint-disable react/prefer-stateless-function */
export default class GridCellInput extends Component {
  constructor(props) {
    super(props);
    this.state = {
      touched: false,
      inputValue: this.props.value
    };
    this.onInputFocus = this.onInputFocus.bind(this);
    this.onInputChange = this.onInputChange.bind(this);
    this.onInput = this.onInput.bind(this);
  }

  onInputFocus() {
    if (this.state.inputValue === 0 || this.state.inputValue === "0") {
      this.setState({
        // touched: true,
        inputValue: ""
      });
      // this.props.toggleEditGridCellClass(true);
    }
  }

  onInputChange(event) {
    this.setState({
      // touched: true,
      inputValue: event.target.value
    });
    // this.props.toggleEditGridCellClass(true);
  }

  onInput(event) {
    event.persist();
    if (
      this.props.emptyZeroDefault &&
      (this.state.inputValue === "" || this.state.inputValue === "0")
    ) {
      this.setState({ inputValue: this.props.value });
      // } else if (this.state.inputValue.toString() !== this.props.value.toString()) {
    } else if (this.state.inputValue !== this.props.value) {
      if (this.props.confirmInput === true) {
        this.props.toggleModal({
          modal: "confirmModal",
          active: true,
          properties: {
            ...this.props.confirmModalProperties,
            action: () => {
              this.props.blurAction(event);
            },
            dismiss: () => {
              this.setState({ inputValue: this.props.value });
            }
          }
        });
      } else if (this.props.confirmInput === false) {
        event.currentTarget.blur();
        this.props.blurAction(event);
      }
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.value !== this.props.value) {
      this.setState({
        // touched: true,
        inputValue: nextProps.value
      });
      //	this.props.toggleEditGridCellClass(true);
    }
  }

  render() {
    const maskType = type => {
      switch (type) {
        case "default":
          return false;
        case "custom":
          return this.props.maskCustom;
        case "createNumber":
          return createNumberMask({
            prefix: this.props.maskPrefix,
            suffic: this.props.maskSuffix,
            includeThousandsSeparator: this.props.maskIncludeThousandsSeparator,
            thousandsSeparatorSymbol: this.props.maskThousandsSeparatorSymbol,
            allowDecimal: this.props.maskAllowDecimal,
            decimalSymbol: this.props.maskDecimalSymbol,
            decimalLimit: this.props.maskDecimalLimit,
            integerLimit: this.props.maskIntegerLimit,
            requireDecimal: this.props.maskRequireDecimal,
            allowNegative: this.props.maskAllowNegative,
            allowLeadingZeroes: this.props.maskAllowLeadingZeros
          });
        default:
          return false;
      }
    };

    const editableClass = this.props.isEditable
      ? "editable-cell"
      : "non-editable-cell";
    const touchedClass =
      this.state.touched && this.props.isEditable && this.props.isGridCellEdited
        ? "editable-cell-changed"
        : "";
    const hasErrorTouchedClass =
      this.state.touched &&
      (this.state.inputValue === 0 || this.state.inputValue === "0") &&
      this.props.isEditable
        ? "editable-cell-has-error"
        : "";
    const hasErrorOnSaveClass =
      this.props.onSaveShowValidation &&
      (this.state.inputValue === 0 || this.state.inputValue === "0") &&
      this.props.isEditable
        ? "editable-cell-has-error"
        : "";

    return (
      <MaskedTextInput
        className={`${editableClass} ${touchedClass} ${hasErrorTouchedClass} ${hasErrorOnSaveClass}`}
        name={this.props.name}
        placeholder={this.props.placeholder}
        value={this.state.inputValue}
        valuekey={this.props.valueKey}
        disabled={!this.props.isEditable}
        onFocus={this.onInputFocus}
        onChange={this.onInputChange}
        maxLength={this.props.maxLength}
        onKeyPress={event => {
          if (event.key === "Enter") {
            event.currentTarget.blur();
          }
          this.props.onKeyPress(event);
        }}
        onBlur={this.onInput}
        mask={maskType(this.props.maskType)}
      />
    );
  }
}

GridCellInput.defaultProps = {
  isEditable: true,
  emptyZeroDefault: false,
  confirmInput: false,
  confirmModalProperties: {
    titleText: "Confirm Modal",
    bodyText: null, // string
    bodyList: null, // array
    closeButtonText: "Close",
    closeButtonBsStyle: "default",
    actionButtonText: "Action",
    actionButtonBsStyle: "warning"
  },
  toggleModal: () => {},
  onSaveShowValidation: false,

  blurAction: () => {},
  enterKeyPressAction: () => {},
  onKeyPress: () => {},

  maskType: "none", // 'custom', 'createNumber'
  maskCustom: null,
  maskCreateNumber: false,
  maskPrefix: "",
  maskSuffix: "",
  maskIncludeThousandsSeparator: true,
  maskThousandsSeparatorSymbol: ",",
  maskAllowDecimal: false,
  maskDecimalSymbol: ".",
  maskDecimalLimit: 2,
  maskIntegerLimit: null,
  maskRequireDecimal: false,
  maskAllowNegative: false,
  maskAllowLeadingZeros: false,

  maxLength: 500
};

GridCellInput.propTypes = {
  name: PropTypes.string.isRequired,
  placeholder: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  valueKey: PropTypes.string.isRequired,

  isEditable: PropTypes.bool,
  emptyZeroDefault: PropTypes.bool,
  confirmInput: PropTypes.bool,
  confirmModalProperties: PropTypes.object,
  toggleModal: PropTypes.func,
  onSaveShowValidation: PropTypes.bool,

  blurAction: PropTypes.func,
  // enterKeyPressAction: PropTypes.func,
  onKeyPress: PropTypes.func,

  maskType: PropTypes.string,
  maskCustom: PropTypes.array,
  maskPrefix: PropTypes.string,
  maskSuffix: PropTypes.string,
  maskIncludeThousandsSeparator: PropTypes.bool,
  maskThousandsSeparatorSymbol: PropTypes.string,
  maskAllowDecimal: PropTypes.bool,
  maskDecimalSymbol: PropTypes.string,
  maskDecimalLimit: PropTypes.number,
  maskIntegerLimit: PropTypes.number,
  maskRequireDecimal: PropTypes.bool,
  maskAllowNegative: PropTypes.bool,
  maskAllowLeadingZeros: PropTypes.bool,
  maxLength: PropTypes.number,

  isGridCellEdited: PropTypes.bool.isRequired
  // toggleEditGridCellClass: PropTypes.func.isRequired,
};
