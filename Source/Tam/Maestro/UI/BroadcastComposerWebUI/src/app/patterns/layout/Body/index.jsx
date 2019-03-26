import React from "react";
import PropTypes from "prop-types";
import { Row, Col } from "react-bootstrap";
import CSSModules from "react-css-modules";
import styles from "./index.style.scss";

function Body({ children }) {
  return (
    <Row styleName="body-row">
      <Col xs={12} sm={12} md={12} styleName="body-row-col-12">
        {children}
      </Col>
    </Row>
  );
}
Body.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.element,
    PropTypes.node,
    PropTypes.array,
    PropTypes.object
  ]).isRequired
};
export default CSSModules(Body, styles);
