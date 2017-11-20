import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { getEnvironment, getEmployee } from 'Ducks/app';

import NavigationBar from 'Components/header/NavigationBar';

const mapStateToProps = ({ routing, app: { environment }, app: { employee } }) => ({
  routing,
  environment,
  employee,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ getEnvironment, getEmployee }, dispatch)
);

export class AppHeader extends Component {
  // constructor(props) {
  //   super(props);
  // }

  componentWillMount() {
    this.props.getEnvironment();
    this.props.getEmployee();
  }

  render() {
    return (
      <div id="app-header">
        <NavigationBar environment={this.props.environment} employee={this.props.employee} />
      </div>
    );
  }
}

AppHeader.propTypes = {
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired,
  getEnvironment: PropTypes.func.isRequired,
  getEmployee: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(AppHeader);
