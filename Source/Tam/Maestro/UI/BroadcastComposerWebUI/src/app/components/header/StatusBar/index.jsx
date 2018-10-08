import React, { Component } from "react";
// import PropTypes from 'prop-types';
import CSSModules from "react-css-modules";
import { Alert, Row, Col } from "react-bootstrap";

import styles from "./index.style.scss";

export class StatusBar extends Component {
  constructor(props) {
    super(props);
    this.state = {
      status: "Ready"
    };
  }

  render() {
    return (
      <Row className="clearfix" styleName="sb-row">
        <Col
          xs={12}
          sm={6}
          smOffset={6}
          md={4}
          mdOffset={8}
          styleName="sb-col-12"
        >
          <Alert bsStyle="info" styleName="sb-alert">
            <strong>Status: {this.state.status}</strong>
          </Alert>
        </Col>
      </Row>
    );
  }
}

export default CSSModules(StatusBar, styles);
