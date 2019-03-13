import React from "react";
import PropTypes from "prop-types";
import { Row, Col } from "react-bootstrap";

function PageTitle({ title }) {
  return (
    <Row>
      <Col>
        <h4 className="cadent-dk-blue text-center">
          <strong>{title}</strong>
        </h4>
      </Col>
    </Row>
  );
}

PageTitle.propTypes = {
  title: PropTypes.string.isRequired
};

export default PageTitle;
