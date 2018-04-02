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

    this.cancel = this.cancel.bind(this);
    this.onMarketsSelectionChange = this.onMarketsSelectionChange.bind(this);
    this.save = this.save.bind(this);

    this.state = {
      selectedMarkets: [],
      currentSelectedMarkets: [],

      blackoutMarkets: [],
      currentBlackoutMarkets: [],
    };
  }

  cancel() {
    this.setState({
      currentSelectedMarkets: this.state.selectedMarkets,
      currentBlackoutMarkets: this.state.blackoutMarkets,
    });

    this.props.toggleModal({
      modal: 'marketSelectorModal',
      active: false,
    });
  }

  onMarketsSelectionChange(markets, selectorName) {
    if (selectorName === 'Markets') {
      this.setState({ currentSelectedMarkets: markets });
    } else {
      this.setState({ currentBlackoutMarkets: markets });
    }
  }

  save() {
    const { updateProposalEditForm, updateMarketCount, initialdata } = this.props;
    let { currentSelectedMarkets, currentBlackoutMarkets } = this.state;

    let marketGroup;
    const simpleMarkets = [];
    currentSelectedMarkets = currentSelectedMarkets.filter(m => m !== null);
    currentSelectedMarkets.map((market) => {
      if (market.Count) {
        marketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: false,
        });
      }

      return market;
    });

    let blackoutMarketGroup;
    currentBlackoutMarkets = currentBlackoutMarkets.filter(m => m !== null);
    currentBlackoutMarkets.map((market) => {
      if (market.Count) {
        blackoutMarketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: true,
        });
      }

      return market;
    });

    // total markets selected for custom option
    const customMarketCount = currentSelectedMarkets.concat(currentBlackoutMarkets).reduce((sum, market) => sum + (market.Count || 1), 0);

    // updates values for BE
    const customGroup = initialdata.MarketGroups.find(m => m.Display === 'Custom');
    updateProposalEditForm({ key: 'Markets', value: simpleMarkets });
    updateProposalEditForm({ key: 'MarketGroup', value: null });
    updateProposalEditForm({ key: 'MarketGroupId', value: (marketGroup === undefined) || (marketGroup === null) ? customGroup.Id : marketGroup });
    updateProposalEditForm({ key: 'BlackoutMarketGroup', value: null });
    updateProposalEditForm({ key: 'BlackoutMarketGroupId', value: (blackoutMarketGroup === undefined) || (blackoutMarketGroup === null) ? customGroup.Id : blackoutMarketGroup });

    updateMarketCount(customMarketCount);

    this.setState({
      selectedMarkets: currentSelectedMarkets,
      blackoutMarkets: currentBlackoutMarkets,
    });

    // close modal
    this.props.toggleModal({
      modal: 'marketSelectorModal',
      active: false,
    });
  }

  componentWillMount() {
    const { proposalEditForm } = this.props;
    const { MarketGroup, Markets, BlackoutMarketGroup } = proposalEditForm;


    const isCustom = ((Markets && Markets.length > 0) || (MarketGroup && BlackoutMarketGroup));
    if (isCustom) {
      const selectedMarkets = Markets.filter(market => !market.IsBlackout);

      if (MarketGroup) {
        selectedMarkets.unshift(MarketGroup);
      }

      this.setState({
        selectedMarkets,
        currentSelectedMarkets: selectedMarkets,
      });

      const blackoutMarkets = Markets.filter(market => market.IsBlackout);

      if (BlackoutMarketGroup) {
        blackoutMarkets.unshift(BlackoutMarketGroup);
      }

      this.setState({
        blackoutMarkets,
        currentBlackoutMarkets: blackoutMarkets,
      });
    }
  }

  render() {
    const { initialdata, modal } = this.props;
    const { currentSelectedMarkets, currentBlackoutMarkets } = this.state;

    const marketGroups = initialdata.MarketGroups.filter(market => (market.Id !== -1) && (market.Id !== 255));

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
                markets={initialdata.Markets}
                selectedMarkets={currentSelectedMarkets}
                onMarketsSelectionChange={this.onMarketsSelectionChange}
              />
            </Col>

            <Col md={6}>
              <MarketSelector
                name="Blackout Markets"
                marketGroups={marketGroups}
                markets={initialdata.Markets}
                selectedMarkets={currentBlackoutMarkets}
                onMarketsSelectionChange={this.onMarketsSelectionChange}
              />
            </Col>
          </Row>
        </Modal.Body>

        <Modal.Footer>
          <Button onClick={this.cancel} bsStyle="danger">Cancel</Button>
          <Button onClick={this.save} bsStyle="success">Save</Button>
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
  updateProposalEditForm: PropTypes.func,
  updateMarketCount: PropTypes.func,
};

MarketGroupSelector.defaultProps = {
  modal: null,
  toggleModal: () => {},
  initialdata: null,
  proposalEditForm: null,
  updateProposalEditForm: null,
  updateMarketCount: () => {},
};

const styledComponent = CSSModules(MarketGroupSelector, styles);
export default connect(mapStateToProps)(styledComponent);
