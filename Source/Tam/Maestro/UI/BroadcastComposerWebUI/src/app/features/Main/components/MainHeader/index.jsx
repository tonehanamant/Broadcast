import React, { Component } from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import { withRouter } from "react-router-dom";

import { getEnvironment, getEmployee } from "Main/redux/ducks";

import NavigationBar from "../MainHeaderNavigation";

const mapStateToProps = ({ app: { environment }, app: { employee } }) => ({
  environment,
  employee
});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ getEnvironment, getEmployee }, dispatch);

export class MainHeader extends Component {
  componentWillMount() {
    const { getEnvironment, getEmployee } = this.props;
    getEnvironment();
    getEmployee();
  }

  render() {
    const { environment, employee, location, history, match } = this.props;
    const routing = { location, history, match };
    return (
      <div id="app-header">
        <NavigationBar
          routing={routing}
          environment={environment}
          employee={employee}
        />
      </div>
    );
  }
}

MainHeader.propTypes = {
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired,
  getEnvironment: PropTypes.func.isRequired,
  getEmployee: PropTypes.func.isRequired,
  location: PropTypes.object.isRequired,
  history: PropTypes.object.isRequired,
  match: PropTypes.object.isRequired
};

const AppHeaderRedux = connect(
  mapStateToProps,
  mapDispatchToProps
)(MainHeader);

export default withRouter(AppHeaderRedux);
