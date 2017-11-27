import React from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Row, Col, Button, Modal } from 'react-bootstrap/lib';
import MarketSelector from './MarketSelector';

import styles from './index.scss';

const MarketGroupSelector = ({ title, open, onClose }) => (
  <Modal show={open}>
    <Modal.Header>
      <Modal.Title>{title}</Modal.Title>
    </Modal.Header>

    <Modal.Body>
      <Row>
        <Col md={6}>
          <MarketSelector />
        </Col>

        <Col md={6}>
          <MarketSelector />
        </Col>
      </Row>
    </Modal.Body>

    <Modal.Footer>
      <Button onClick={onClose}>Close</Button>
    </Modal.Footer>
  </Modal>
);

MarketGroupSelector.propTypes = {
  title: PropTypes.string.isRequired,
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
};

export default CSSModules(MarketGroupSelector, styles);
