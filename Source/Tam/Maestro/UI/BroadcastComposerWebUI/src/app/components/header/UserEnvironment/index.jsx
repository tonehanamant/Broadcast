import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';

import styles from './index.style.scss';

/* eslint-disable react/prefer-stateless-function */
export class UserEnvironment extends Component {
  // constructor(props) {
  //   super(props);
  // }

  render() {
    return (
      <div styleName="status-brand">
        <div className="cadent-blue-emphasis">
                cadent broadcast
          <span id="app_environment_name" className="label label-primary">{this.props.environment}</span>
        </div>
        <div id="user_info" styleName="username" className="user-green">{this.props.employee.Username}</div>
      </div>
    );
  }
}

UserEnvironment.propTypes = {
  employee: PropTypes.object.isRequired,
  environment: PropTypes.string.isRequired,
};

export default CSSModules(UserEnvironment, styles);

