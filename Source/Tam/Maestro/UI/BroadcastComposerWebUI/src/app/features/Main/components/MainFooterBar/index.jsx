import React from "react";
import { Row, Col } from "react-bootstrap";

export default function MainFooterBar() {
  return (
    <div className="footer">
      <Row>
        <Col>
          <p
            className="text-muted"
            style={{ textAlign: "center", padding: 10 }}
          >
            Cadent Broadcast © {new Date().getFullYear()} Cadent Network
          </p>
        </Col>
      </Row>
    </div>
  );
}
