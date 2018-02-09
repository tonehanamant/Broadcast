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
    this.action = this.action.bind(this);
    this.dismiss = this.dismiss.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: 'confirmModal',
      active: false,
      properties: this.props.modal.properties,
    });
  }

  action() {
    this.props.modal.properties.action();
    this.close();
  }

  dismiss() {
    this.props.modal.properties.dismiss();
    this.close();
  }

  render() {
    return (
      <Modal show={this.props.modal.active} onHide={this.dismiss}>
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>{this.props.modal.properties.titleText}</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.dismiss} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          {this.props.modal.properties.bodyText &&
            <p>{this.props.modal.properties.bodyText}</p>
          }
          {this.props.modal.properties.bodyList &&
            <ul>
              {this.props.modal.properties.bodyList.map(item => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          }
        </Modal.Body>
        <Modal.Footer>
          <Button onClick={this.dismiss} bsStyle={this.props.modal.properties.closeButtonBsStyle || 'default'}>{this.props.modal.properties.closeButtonText}</Button>
          <Button bsStyle={this.props.modal.properties.actionButtonBsStyle} onClick={this.action}>{this.props.modal.properties.actionButtonText}</Button>
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
      bodyText: null, // string
      bodyList: null, // array
      closeButtonText: 'Close',
      closeButtonBsStyle: 'default',
      actionButtonText: 'Action',
      actionButtonBsStyle: 'warning',
      action: () => {},
      dismiss: () => {},
    },
  },
};

ConfirmModal.propTypes = {
  modal: PropTypes.object.isRequired,
	toggleModal: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(ConfirmModal);
