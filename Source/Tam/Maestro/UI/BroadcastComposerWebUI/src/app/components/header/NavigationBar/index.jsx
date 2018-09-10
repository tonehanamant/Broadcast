import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
// import { Link } from 'react-router-dom';
import { Navbar, Nav, Row, Col, MenuItem } from 'react-bootstrap';

import UserEnvironment from 'Components/header/UserEnvironment';

// import Logo from 'Assets/images/cad_logo_sm.png';

import styles from './index.style.scss';

/* let hostname = window.location.hostname;
const ports = window.location.port;
const url = ports !== '' ? hostname += `:${ports}` : '';

const host = `http://${url}/api/images/logo.png`; */

/* eslint-disable no-undef */
const apiBase = __API__;
const imgSrc = `${apiBase}images/logo.png`;

/* eslint-disable react/prefer-stateless-function */
export class NavigationBar extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
      <Navbar default collapseOnSelect fluid fixedTop>
        <Row className="clearfix" styleName="nav-row">
          <Col>
            <Navbar.Header>
              <Navbar.Brand>
                <a href="http://cadentnetwork.com/" styleName="navigation-brand">
                  <img alt="Cadent Network" src={imgSrc} className="img-responsive" />
                </a>
              </Navbar.Brand>
              <Navbar.Toggle />
            </Navbar.Header>
          </Col>
          <Col styleName="float-right" className="clearfix">
            <Navbar.Header>
              <Navbar.Brand>
                <UserEnvironment environment={this.props.environment} employee={this.props.employee} />
              </Navbar.Brand>
            </Navbar.Header>
          </Col>
          <Col>
            <Navbar.Collapse>
              <Nav>
                <MenuItem href="/broadcast/rates">Rate Cards</MenuItem>
                <MenuItem href="/broadcastreact/planning" active={this.props.routing.location.pathname === '/broadcastreact/planning'}>Planning</MenuItem>
                <MenuItem href="/broadcast/traffic">Traffic</MenuItem>
                <MenuItem href="/broadcastreact/tracker" active={this.props.routing.location.pathname === '/broadcastreact/tracker'}>Tracker</MenuItem>
                <MenuItem href="/broadcast/Home/TrackerPrePosting">Tracker Pre Posting</MenuItem>
                <MenuItem href="/broadcastreact/post-pre-posting" active={this.props.routing.location.pathname === '/broadcastreact/post-pre-posting'}>Post Pre Posting</MenuItem>
                <MenuItem href="/broadcastreact/post" active={this.props.routing.location.pathname === '/broadcastreact/post'}>Post</MenuItem>
              </Nav>
            </Navbar.Collapse>
          </Col>
        </Row>
      </Navbar>
    );
  }
}

NavigationBar.propTypes = {
  routing: PropTypes.object.isRequired,
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired,
};

export default CSSModules(NavigationBar, styles);
