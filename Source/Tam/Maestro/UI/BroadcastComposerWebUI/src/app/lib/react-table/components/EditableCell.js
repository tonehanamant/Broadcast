import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { toNumber } from 'lodash';

import { isNumeric } from '../util/util';

class EditableCell extends Component {
  constructor(props) {
    super(props);

    this.handleChange = this.handleChange.bind(this);
  }

  handleChange({ target: { innerText } }) {
    const { onChange, row } = this.props;
    /**  Parse value to possible type string or number.
     *   To return value in correct type to parent onChange function */
    const value = isNumeric(innerText) ? toNumber(innerText) : innerText;
    onChange(row, value);
  }

  render() {
    const { row, style, className } = this.props;

    return (
        <div
          style={style}
          className={className}
          contentEditable
          suppressContentEditableWarning
          onBlur={this.handleChange}
          onChange={this.handleChange}
        >
          {row.value}
        </div>
    );
  }
}

EditableCell.defaultProps = {
  className: '',
  style: {},
  row: {},
};

EditableCell.propTypes = {
  row: PropTypes.objectOf(PropTypes.shape.isRequired),
  onChange: PropTypes.func.isRequired,
  className: PropTypes.string,
  style: PropTypes.objectOf(PropTypes.shape),
};

export default EditableCell;
