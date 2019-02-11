import React, { Component } from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import { withRouter } from "react-router-dom";

// import { getEnvironment, getEmployee } from "Ducks/app";
import { getEnvironment, getEmployee } from "Main/redux/actions";

import NavigationBar from "./MainHeaderNavigation";

const mapStateToProps = ({ app: { environment }, app: { employee } }) => ({
  environment,
  employee
});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ getEnvironment, getEmployee }, dispatch);

export class MainHeader extends Component {
  // constructor(props) {
  //   super(props);
  // }

  componentWillMount() {
    this.props.getEnvironment();
    this.props.getEmployee();
  }

  render() {
    const routing = {
      location: this.props.location,
      history: this.props.history,
      match: this.props.match
    };
    const { environment, employee } = this.props;
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
  // redux props:
  environment: PropTypes.string.isRequired,
  employee: PropTypes.object.isRequired,
  getEnvironment: PropTypes.func.isRequired,
  getEmployee: PropTypes.func.isRequired,
  // withRouter props:
  location: PropTypes.object.isRequired,
  history: PropTypes.object.isRequired,
  match: PropTypes.object.isRequired
};

const AppHeaderRedux = connect(
  mapStateToProps,
  mapDispatchToProps
)(MainHeader);

export default withRouter(AppHeaderRedux);
