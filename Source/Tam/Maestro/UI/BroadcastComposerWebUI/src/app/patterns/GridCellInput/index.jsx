import React, { Component } from "react";
import PropTypes from "prop-types";

import MaskedTextInput from "react-text-mask";
import createNumberMask from "text-mask-addons/dist/createNumberMask";

export default class GridCellInput extends Component {
  constructor(props) {
    super(props);
    this.state = {
      touched: false,
      inputValue: props.value
    };
    this.onInputFocus = this.onInputFocus.bind(this);
    this.onInputChange = this.onInputChange.bind(this);
    this.onInput = this.onInput.bind(this);
  }

  componentWillReceiveProps(nextProps) {
    const { value } = this.props;
    if (nextProps.value !== value) {
      this.setState({
        inputValue: nextProps.value
      });
    }
  }

  onInputFocus() {
    const { inputValue } = this.state;
    if (inputValue === 0 || inputValue === "0") {
      this.setState({
        inputValue: ""
      });
    }
  }

  onInputChange(event) {
    this.setState({
      inputValue: event.target.value
    });
  }

  onInput(event) {
    const { inputValue } = this.state;
    const {
      emptyZeroDefault,
      confirmInput,
      toggleModal,
      confirmModalProperties,
      blurAction,
      value
    } = this.props;

    event.persist();
    if (emptyZeroDefault && (inputValue === "" || inputValue === "0")) {
      this.setState({ inputValue: value });
    } else if (inputValue !== value) {
      if (confirmInput === true) {
        toggleModal({
          modal: "confirmModal",
          active: true,
          properties: {
            ...confirmModalProperties,
            action: () => {
              blurAction(event);
            },
            dismiss: () => {
              this.setState({ inputValue: value });
            }
          }
        });
      } else if (confirmInput === false) {
        event.currentTarget.blur();
        blurAction(event);
      }
    }
  }

  maskType(type) {
    const {
      maskCustom,
      maskPrefix,
      maskSuffix,
      maskIncludeThousandsSeparator,
      maskThousandsSeparatorSymbol,
      maskAllowDecimal,
      maskDecimalSymbol,
      maskDecimalLimit,
      maskRequireDecimal,
      maskAllowNegative,
      maskAllowLeadingZeros,
      maskIntegerLimit
    } = this.props;

    switch (type) {
      case "default":
        return false;
      case "custom":
        return maskCustom;
      case "createNumber":
        return createNumberMask({
          prefix: maskPrefix,
          suffic: maskSuffix,
          includeThousandsSeparator: maskIncludeThousandsSeparator,
          thousandsSeparatorSymbol: maskThousandsSeparatorSymbol,
          allowDecimal: maskAllowDecimal,
          decimalSymbol: maskDecimalSymbol,
          decimalLimit: maskDecimalLimit,
          integerLimit: maskIntegerLimit,
          requireDecimal: maskRequireDecimal,
          allowNegative: maskAllowNegative,
          allowLeadingZeroes: maskAllowLeadingZeros
        });
      default:
        return false;
    }
  }

  render() {
    const { touched, inputValue } = this.state;
    const {
      isEditable,
      isGridCellEdited,
      onSaveShowValidation,
      name,
      placeholder,
      valueKey,
      maxLength,
      maskType,
      onKeyPress
    } = this.props;

    const editableClass = isEditable ? "editable-cell" : "non-editable-cell";
    const touchedClass =
      touched && isEditable && isGridCellEdited ? "editable-cell-changed" : "";
    const hasErrorTouchedClass =
      touched && (inputValue === 0 || inputValue === "0") && isEditable
        ? "editable-cell-has-error"
        : "";
    const hasErrorOnSaveClass =
      onSaveShowValidation &&
      (inputValue === 0 || inputValue === "0") &&
      isEditable
        ? "editable-cell-has-error"
        : "";

    return (
      <MaskedTextInput
        className={`${editableClass} ${touchedClass} ${hasErrorTouchedClass} ${hasErrorOnSaveClass}`}
        name={name}
        placeholder={placeholder}
        value={inputValue}
        valuekey={valueKey}
        disabled={!isEditable}
        onFocus={this.onInputFocus}
        onChange={this.onInputChange}
        maxLength={maxLength}
        onKeyPress={event => {
          if (event.key === "Enter") {
            event.currentTarget.blur();
          }
          onKeyPress(event);
        }}
        onBlur={this.onInput}
        mask={this.maskType(maskType)}
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
  onKeyPress: () => {},

  maskType: "none", // 'custom', 'createNumber'
  maskCustom: null,
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
};
