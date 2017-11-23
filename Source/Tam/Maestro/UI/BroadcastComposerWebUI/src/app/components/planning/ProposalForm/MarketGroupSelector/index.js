import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Button, Modal } from 'react-bootstrap/lib';

import styles from './index.scss';

class MarketGroupSelector extends Component {
  constructor() {
    super();

    this.closeModal = this.closeModal.bind(this);

    this.state = {
      open: false,
    };
  }

  closeModal() {
    this.setState({ open: false });
  }

  render() {
    return (
      <Modal show={this.props.open}>
        <Modal.Header closeButton>
          <Modal.Title>Learn More</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          TODO
        </Modal.Body>

        <Modal.Footer>
          <Button onClick={this.props.onClose}>Close</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

MarketGroupSelector.propTypes = {
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
};

export default CSSModules(MarketGroupSelector, styles);
