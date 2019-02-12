import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import { Navbar, Nav, Row, Col, MenuItem } from "react-bootstrap";

import UserEnvironment from "../MainHeaderUserEnvironment";

import styles from "./index.style.scss";

const imgSrc = `${__API__}images/logo.png`;

export const MainHeaderNavigation = ({ environment, employee, routing }) => (
  <Navbar default collapseOnSelect fluid fixedTop>
    <Row className="clearfix" styleName="nav-row">
      <Col>
        <Navbar.Header>
          <Navbar.Brand>
            <a href="http://cadentnetwork.com/" styleName="navigation-brand">
              <img
                alt="Cadent Network"
                src={imgSrc}
                className="img-responsive"
              />
            </a>
          </Navbar.Brand>
          <Navbar.Toggle />
        </Navbar.Header>
      </Col>
      <Col styleName="float-right" className="clearfix">
        <Navbar.Header>
          <Navbar.Brand>
            <UserEnvironment environment={environment} employee={employee} />
          </Navbar.Brand>
        </Navbar.Header>
      </Col>
      <Col>
        <Navbar.Collapse>
          <Nav>
            <MenuItem href="/broadcast/rates">Rate Cards</MenuItem>
            <MenuItem
              href="/broadcastreact/planning"
              active={routing.location.pathname === "/broadcastreact/planning"}
            >
              Planning
            </MenuItem>
            <MenuItem href="/broadcast/traffic">Traffic</MenuItem>
            <MenuItem
              href="/broadcastreact/tracker"
              active={routing.location.pathname === "/broadcastreact/tracker"}
            >
              Tracker
            </MenuItem>
            <MenuItem href="/broadcast/Home/TrackerPrePosting">
              Tracker Pre Posting
            </MenuItem>
            <MenuItem
              href="/broadcastreact/post-pre-posting"
              active={
                routing.location.pathname === "/broadcastreact/post-pre-posting"
              }
            >
              Post Pre Posting
            </MenuItem>
            <MenuItem
              href="/broadcastreact/post"
              active={routing.location.pathname === "/broadcastreact/post"}
            >
              Post
            </MenuItem>
          </Nav>
        </Navbar.Collapse>
      </Col>
    </Row>
  </Navbar>
);

MainHeaderNavigation.propTypes = {
  routing: PropTypes.shape({
    location: PropTypes.object.isRequired,
    history: PropTypes.object,
    match: PropTypes.object
  }).isRequired,
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired
};

export default CSSModules(MainHeaderNavigation, styles);
