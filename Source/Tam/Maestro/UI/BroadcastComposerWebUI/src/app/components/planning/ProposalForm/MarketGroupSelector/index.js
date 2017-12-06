import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Row, Col, Button, Modal } from 'react-bootstrap/lib';
import MarketSelector from './MarketSelector';

import styles from './index.scss';

/*eslint-disable*/
class MarketGroupSelector extends Component {
  render() {
    const { title, open, onClose, marketGroups, markets, onApplyChange, onMarketsSelectionChange, selectedMarkets, blackoutMarkets } = this.props;

    return (
      <Modal show={open} bsSize="large">
        <Modal.Header>
          <Modal.Title>{title}</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <Row>
            <Col md={6}>
              <MarketSelector
                name="Markets"
                marketGroups={marketGroups}
                markets={markets}
                selectedMarkets={selectedMarkets}
                onMarketsSelectionChange={onMarketsSelectionChange}
              />
            </Col>

            <Col md={6}>
              <MarketSelector
                name="Blackout Markets"
                marketGroups={marketGroups}
                markets={markets}
                selectedMarkets={blackoutMarkets}
                onMarketsSelectionChange={onMarketsSelectionChange}
              />
            </Col>
          </Row>
        </Modal.Body>

        <Modal.Footer>
          <Button onClick={onClose} bsStyle="danger">Cancel</Button>
          <Button onClick={() => onApplyChange()} bsStyle="success">Save</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

MarketGroupSelector.propTypes = {
  title: PropTypes.string.isRequired,
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  onApplyChange: PropTypes.func.isRequired,
  onMarketsSelectionChange: PropTypes.func.isRequired,

  marketGroups: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
    Count: PropTypes.number,
  })),

  markets: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
  })),

  selectedMarkets: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
    Count: PropTypes.number,
  })),

  blackoutMarkets: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
    Count: PropTypes.number,
  })),
};

MarketGroupSelector.defaultProps = {
  marketGroups: [],
  markets: [],
  selectedMarkets: [],
  blackoutMarkets: [],
};

export default CSSModules(MarketGroupSelector, styles);
