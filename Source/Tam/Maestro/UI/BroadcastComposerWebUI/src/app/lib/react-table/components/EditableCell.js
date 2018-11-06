import React, { Component } from "react";
import PropTypes from "prop-types";
import MaskedInput from "react-text-mask";
import createNumberMask from "text-mask-addons/dist/createNumberMask";
import { pick, isPlainObject } from "lodash";
import { pipe, replace, toNumber } from "lodash/fp";
import { Glyphicon } from "react-bootstrap";

import "../style/EditableCell.scss";

const INTEGER = "integer";
const DECIMAL = "decimal";

const integer = {
  prefix: "",
  includeThousandsSeparator: true,
  thousandsSeparatorSymbol: ",",
  allowDecimal: false
};

const decimal = {
  prefix: "",
  includeThousandsSeparator: true,
  thousandsSeparatorSymbol: ",",
  allowDecimal: true,
  decimalSymbol: ".",
  decimalLimit: 2
};

const numberMasks = {
  decimal,
  integer
};

const createRegExp = symbol => {
  if (symbol) {
    return new RegExp(symbol, "g");
  }
  return undefined;
};

const parseValue = (value, mask) => {
  if (mask === INTEGER || mask === DECIMAL) {
    return pipe(
      replace(createRegExp(numberMasks[mask].thousandsSeparatorSymbol), ""),
      replace(createRegExp(numberMasks[mask].decimalSymbol), "."),
      toNumber
    )(value);
  }
  return value;
};

const getMask = mask => {
  if (isPlainObject(mask)) return mask;

  switch (mask) {
    case INTEGER:
      return createNumberMask(integer);
    case DECIMAL:
      return createNumberMask(decimal);
    default:
      return false;
  }
};

class EditableCell extends Component {
  constructor(props) {
    super(props);

    this.state = {
      value: props.clearEmptyValue ? props.value || null : props.value,
      mask: getMask(props.mask),
      isFocused: false
    };

    this.onFocus = this.onFocus.bind(this);
    this.onBlur = this.onBlur.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.onChange = this.onChange.bind(this);
    this.onKeyPress = this.onKeyPress.bind(this);
    this.updateValue = this.updateValue.bind(this);
  }

  componentDidUpdate({ value: prevValue }) {
    const { value } = this.props;
    if (prevValue !== value) {
      this.updateValue();
    }
  }

  updateValue() {
    const { value } = this.props;
    this.setState({ value });
  }

  onFocus() {
    const { mask, value: propsValue, clearEmptyValue } = this.props;
    const { value } = this.state;
    this.setState({
      value: clearEmptyValue ? value || propsValue : propsValue,
      mask: getMask(mask),
      isFocused: true
    });
  }

  onBlur() {
    const { value: newValue } = this.state;
    const { value: pVal, allowSubmitEmpty, clearEmptyValue, mask } = this.props;
    const parsedValue = parseValue(newValue, mask);
    this.setState({
      value: clearEmptyValue ? parsedValue || pVal || null : newValue,
      isFocused: false
    });
    if ((allowSubmitEmpty || parsedValue) && parsedValue !== pVal) {
      this.onSubmit(parsedValue);
    }
  }

  onSubmit(value) {
    const { onChange, id } = this.props;
    onChange(id, value);
  }

  onChange({ target: { value } }) {
    this.setState({ value });
  }

  onKeyPress(event) {
    const { onKeyPress } = this.props;
    const { key, target } = event;

    if (key === "Enter") target.blur();

    onKeyPress(event);
  }

  render() {
    const { value, mask, isFocused } = this.state;
    const { placeholder } = this.props;
    const MaskProps = pick(this.props, Object.keys(MaskedInput.propTypes));

    return (
      <div className="editable-table-cell-wrap">
        <MaskedInput
          {...MaskProps}
          className="editable-table-cell"
          onFocus={this.onFocus}
          onBlur={this.onBlur}
          onChange={this.onChange}
          onKeyPress={this.onKeyPress}
          value={value}
          mask={mask}
          placeholder={placeholder}
          data-value={!!value}
        />
        {!isFocused && <Glyphicon glyph="edit" data-value={!!value} />}
      </div>
    );
  }
}

EditableCell.defaultProps = {
  value: null,
  id: "",
  onKeyPress: () => {},
  mask: undefined,
  placeholder: "-",
  allowSubmitEmpty: false,
  clearEmptyValue: true
};

EditableCell.propTypes = {
  mask: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.objectOf(PropTypes.shape)
  ]),
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  onChange: PropTypes.func.isRequired,
  onKeyPress: PropTypes.func,
  placeholder: PropTypes.string,
  allowSubmitEmpty: PropTypes.bool,
  clearEmptyValue: PropTypes.bool,
  id: PropTypes.string
};

export default EditableCell;
