import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import CSSModules from "react-css-modules";
import styles from "./index.scss";

const Overlay = ({ type, loading, processing }) => {
  if (
    type === "loading" &&
    Object.keys(loading).some(key => loading[key] === true)
  ) {
    return (
      <div className={styles.overlayLoading}>
        <div className={styles.overlayDialog}>
          <i className="fa fa-refresh fa-spin fa-3x fa-fw" />
          Loading...
        </div>
      </div>
    );
  }

  if (
    type === "processing" &&
    Object.keys(processing).some(key => processing[key] === true)
  ) {
    return (
      <div className={styles.overlayProcessing}>
        <div className={styles.overlayDialog}>
          <i className="fa fa-spinner fa-spin fa-3x fa-fw" />
          Processing...
        </div>
      </div>
    );
  }

  return null;
};

Overlay.defaultProps = {
  loading: {},
  processing: {}
};

Overlay.propTypes = {
  type: PropTypes.string.isRequired,
  loading: PropTypes.object,
  processing: PropTypes.object
};

function mapStateToProps(state) {
  return {
    loading: state.app.loading,
    processing: state.app.processing
  };
}

const styledComponent = CSSModules(Overlay, styles);
export default connect(mapStateToProps)(styledComponent);
