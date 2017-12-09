import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Button, Modal } from 'react-bootstrap';

import { toggleModal } from 'Ducks/app';

const mapStateToProps = ({ app: { modals: { confirmModal: modal } } }) => ({
  modal,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal }, dispatch)
);

export class ConfirmModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.actionButtonClick = this.actionButtonClick.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: 'confirmModal',
      active: false,
      properties: this.props.modal.properties,
    });
  }

  actionButtonClick() {
    this.close();
    this.props.modal.properties.action();
  }

  render() {
    return (
      <Modal show={this.props.modal.active} onHide={this.close}>
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>{this.props.modal.properties.titleText}</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <p>{this.props.modal.properties.bodyText}</p>
        </Modal.Body>
        <Modal.Footer>
          <Button bsStyle={this.props.modal.properties.actionButtonBsStyle} onClick={this.actionButtonClick}>{this.props.modal.properties.actionButtonText}</Button>
          <Button onClick={this.close} bsStyle={this.props.modal.properties.closeButtonBsStyle || 'default'}>{this.props.modal.properties.closeButtonText}</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

ConfirmModal.defaultProps = {
  modal: {
    active: false,
    properties: {
      titleText: 'Confirm Modal',
      bodyText: 'Body Text',
      closeButtonText: 'Close',
      closeButtonBsStyle: 'default',
      actionButtonText: 'Action',
      actionButtonBsStyle: 'warning',
      action: () => {},
    },
  },
};

ConfirmModal.propTypes = {
  modal: PropTypes.object.isRequired,
	toggleModal: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(ConfirmModal);
