import React, { Component } from "react";
import PropTypes from "prop-types";
import { Row, Col } from "react-bootstrap";

/* eslint-disable react/prefer-stateless-function */
export default class PageTitle extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
      <Row>
        <Col>
          <h4 className="cadent-dk-blue text-center">
            <strong>{this.props.title}</strong>
          </h4>
        </Col>
      </Row>
    );
  }
}

PageTitle.propTypes = {
  title: PropTypes.string.isRequired
};
