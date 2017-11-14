import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { v4 } from 'uuid';
import { connect } from 'react-redux';
import { AlertList } from 'react-bs-notifier';

class Toast extends Component {
  constructor(props) {
    super(props);

    this.closeAlert = this.closeAlert.bind(this);

    this.state = {
      alerts: [],
      timeout: 3000,
    };
  }

  componentWillReceiveProps(nextProps) {
    const { alert } = nextProps;

    if (alert) {
      alert.id = v4();
      this.setState({
        alerts: [...this.state.alerts, alert],
      });
    }
  }

  closeAlert(alert) {
    const { alerts } = this.state;
    const idx = alerts.indexOf(alert);

    if (idx >= 0) {
      this.setState({
        alerts: [...alerts.slice(0, idx), ...alerts.slice(idx + 1)],
      });
    }
  }

  render() {
    return (
      <AlertList
        position="top-right"
        alerts={this.state.alerts}
        timeout={this.state.timeout}
        dismissTitle="Dismiss"
        onDismiss={(alert) => { this.closeAlert(alert); }}
      />
    );
  }
}

Toast.defaultProps = {
  alert: null,
};

Toast.propTypes = {
  alert: PropTypes.shape({
    display: PropTypes.bool,
    type: PropTypes.string,
    headline: PropTypes.string,
    message: PropTypes.string,
  }),
};

function mapStateToProps(state) {
  return {
    alert: state.app.alert,
  };
}

export default connect(mapStateToProps)(Toast);

