import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import CSSModules from "react-css-modules";
import Icon from "Patterns/Icon";
import styles from "./index.style.scss";

const Overlay = ({ type, loading, processing }) => {
  if (
    type === "loading" &&
    Object.keys(loading).some(key => loading[key] === true)
  ) {
    return (
      <div styleName="overlayLoading">
        <div styleName="overlayDialog">
          <Icon
            iconType="light"
            icon="spinner-third"
            iconSize="md"
            spin
            iconColor="tertiary"
          />
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
      <div styleName="overlayProcessing">
        <div styleName="overlayDialog">
          <Icon
            iconType="light"
            icon="spinner-third"
            iconSize="md"
            spin
            iconColor="tertiary"
          />
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
