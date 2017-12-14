import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import CSSModules from 'react-css-modules';
import { Row, Col, Button, Modal } from 'react-bootstrap/lib';
import MarketSelector from './MarketSelector';
import styles from './index.scss';

const mapStateToProps = ({ app: { modals: { marketSelectorModal: modal } } }) => ({
  modal,
});

class MarketGroupSelector extends Component {
  constructor(props) {
    super(props);

    this.onCancel = this.onCancel.bind(this);

    this.state = {
      selectedMarkets: [],
      blackoutMarkets: [],
    };
  }

  onCancel() {
    this.props.toggleModal({
      modal: 'marketSelectorModal',
      active: false,
    });
  }

  componentWillMount() {
    const { initialdata, proposalEditForm } = this.props;
    const { MarketGroup, Markets, BlackoutMarketGroup } = proposalEditForm;
    let selectedMarketGroup = MarketGroup;
    let customMarketCount = 0;

    // conditions to be custom: has any amount of 'single' markets OR has more than one marketGroup
    const isCustom = ((Markets && Markets.length > 0) || (MarketGroup && BlackoutMarketGroup));
    if (isCustom) {
      customMarketCount = Markets.length;
      customMarketCount += MarketGroup ? MarketGroup.Count : 0;
      customMarketCount += BlackoutMarketGroup ? BlackoutMarketGroup.Count : 0;

      selectedMarketGroup = initialdata.MarketGroups.find(marketGroup => marketGroup.Id === 255);
      selectedMarketGroup.Count = customMarketCount;

      // update selected lists (simple and blackout)
      const selectedMarkets = Markets.filter(market => !market.IsBlackout);
      selectedMarkets.unshift(MarketGroup);
      this.setState({ selectedMarkets });

      const blackoutMarkets = Markets.filter(market => market.IsBlackout);
      blackoutMarkets.unshift(BlackoutMarketGroup);
      this.setState({ blackoutMarkets });
    }

    this.setState({ customMarketCount });
    this.setState({ selectedMarketGroup });
  }

  render() {
    const { modal, marketGroups, markets, onApplyChange, onMarketsSelectionChange } = this.props;
    const { selectedMarkets, blackoutMarkets } = this.state;

    return (
      <Modal show={modal && modal.active} bsSize="large">
        <Modal.Header>
          <Modal.Title>Custom Market</Modal.Title>
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
          <Button onClick={this.onCancel} bsStyle="danger">Cancel</Button>
          <Button onClick={() => onApplyChange()} bsStyle="success">Save</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

MarketGroupSelector.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func,
  initialdata: PropTypes.object,
  proposalEditForm: PropTypes.object,

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
};

MarketGroupSelector.defaultProps = {
  modal: null,
  toggleModal: () => {},
  initialdata: null,
  proposalEditForm: null,

  marketGroups: [],
  markets: [],
};

const styledComponent = CSSModules(MarketGroupSelector, styles);
export default connect(mapStateToProps)(styledComponent);
