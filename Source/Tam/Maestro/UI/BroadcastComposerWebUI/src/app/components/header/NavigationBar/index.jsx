import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
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
                <NavItem href="/broadcast/rates" target="_self">Rate Cards</NavItem>
                <NavItem href="/broadcast/rates" target="_self">Planning</NavItem>
                <NavItem href="/broadcast/traffic" target="_self">Traffic</NavItem>
                <NavItem href="/broadcast/Home/TrackerPrePosting" target="_self">Tracker Pre Posting</NavItem>
                <NavItem active href="/post">Post</NavItem>
              </Nav>
            </Navbar.Collapse>
          </Col>
        </Row>
      </Navbar>
    );
  }
}

NavigationBar.propTypes = {
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired,
};

export default CSSModules(NavigationBar, styles);
