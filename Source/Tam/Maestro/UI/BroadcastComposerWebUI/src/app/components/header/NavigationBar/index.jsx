import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Link } from 'react-router-dom';
import { Navbar, Nav, NavItem, Row, Col } from 'react-bootstrap';

import UserEnvironment from 'Components/header/UserEnvironment';

import Logo from 'Assets/images/cad_logo_sm.png';

import styles from './index.style.scss';

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
                  <img alt="Cadent Network" src={Logo} className="img-responsive" />
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
                <NavItem componentClass={Link} href="/broadcast/rates" to="/broadcast/rates">Rate Cards</NavItem>
                <NavItem componentClass={Link} href="/broadcast/planning" to="/broadcast/planning">Planning</NavItem>
                <NavItem componentClass={Link} href="/broadcast/traffic" to="/broadcast/traffic">Traffic</NavItem>
                <NavItem componentClass={Link} href="/broadcast/Home/TrackerPrePosting" to="/broadcast/Home/TrackerPrePosting">Tracker Pre Posting</NavItem>
                <NavItem componentClass={Link} href="/broadcastreact/post-pre-posting" to="/broadcastreact/post-pre-posting" active={this.props.routing.location.pathname === '/broadcastreact/post-pre-posting'}>Post Pre Posting</NavItem>
                <NavItem componentClass={Link} href="/broadcastreact/post" to="/broadcastreact/post" active={this.props.routing.location.pathname === '/broadcastreact/post'}>Post</NavItem>
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
