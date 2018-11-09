import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import { Button, Modal } from "react-bootstrap";

import { toggleModal } from "Ducks/app";

const mapStateToProps = ({
  app: {
    modals: { confirmModal: modal }
  }
}) => ({
  modal
});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ toggleModal }, dispatch);

export class ConfirmModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.action = this.action.bind(this);
    this.dismiss = this.dismiss.bind(this);
  }

  close() {
    const {
      modal: { properties },
      toggleModal
    } = this.props;

    toggleModal({
      modal: "confirmModal",
      active: false,
      properties
    });
  }

  action() {
    const {
      modal: { properties }
    } = this.props;
    properties.action();
    this.close();
  }

  dismiss() {
    const {
      modal: { properties }
    } = this.props;
    properties.dismiss();
    this.close();
  }

  render() {
    const {
      modal: { properties, active }
    } = this.props;
    return (
      <Modal show={active} onHide={this.dismiss}>
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            {properties.titleText}
          </Modal.Title>
          <Button
            className="close"
            bsStyle="link"
            onClick={this.dismiss}
            style={{ display: "inline-block", float: "right" }}
          >
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          {properties.bodyText && <p>{properties.bodyText}</p>}
          {properties.bodyList && (
            <ul>
              {properties.bodyList.map(item => <li key={item}>{item}</li>)}
            </ul>
          )}
        </Modal.Body>
        <Modal.Footer>
          {!properties.closeButtonDisabled && (
            <Button
              onClick={this.dismiss}
              bsStyle={properties.closeButtonBsStyle || "default"}
            >
              {properties.closeButtonText}
            </Button>
          )}
          <Button
            bsStyle={properties.actionButtonBsStyle}
            onClick={this.action}
            href={properties.href}
          >
            {properties.actionButtonText}
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

ConfirmModal.defaultProps = {
  modal: {
    active: false,
    properties: {
      titleText: "Confirm Modal",
      bodyText: null, // string
      bodyList: null, // array
      closeButtonText: "Close",
      closeButtonBsStyle: "default",
      closeButtonDisabled: false,
      actionButtonText: "Action",
      actionButtonBsStyle: "warning",
      href: undefined,
      action: () => {},
      dismiss: () => {}
    }
  }
};

ConfirmModal.propTypes = {
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(ConfirmModal);
