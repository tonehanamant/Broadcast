import React, { Component } from "react";
import { Row, Col } from "react-bootstrap";

export default class MainFooterBar extends Component {
  constructor(props) {
    super(props);
    this.state = {
      year: new Date().getFullYear()
    };
  }

  render() {
    return (
      <div className="footer">
        <Row>
          <Col>
            <p
              className="text-muted"
              style={{ textAlign: "center", padding: 10 }}
            >
              Cadent Broadcast © {this.state.year} Cadent Network
            </p>
          </Col>
        </Row>
      </div>
    );
  }
}
