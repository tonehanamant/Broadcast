import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";

import Icon from "Patterns/Icon";

import styles from "./index.style.scss";

const CardField = ({ label, value, description, info }) => (
  <article styleName="card-field">
    <section styleName="label">
      <span>{label}</span>
      {description && (
        <Icon
          icon="question-circle"
          iconType="light"
          iconSize="xxs"
          iconColor="gray-2"
        />
      )}
    </section>
    <section styleName="value">
      <span>{value}</span>
      {info && <Icon icon="question-circle" iconType="light" iconSize="xs" />}
    </section>
  </article>
);

CardField.propTypes = {
  label: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  description: PropTypes.string,
  info: PropTypes.string
};

CardField.defaultProps = {
  description: null,
  info: null,
  value: null
};

export default CSSModules(CardField, styles);
