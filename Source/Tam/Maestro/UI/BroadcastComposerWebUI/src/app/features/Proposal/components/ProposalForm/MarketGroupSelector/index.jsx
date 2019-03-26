import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import CSSModules from "react-css-modules";
import { Row, Col, Button, Modal } from "react-bootstrap";
import MarketSelector from "./MarketSelector";
import styles from "./index.style.scss";

const isOpenning = (modal, nextModal) =>
  !(modal && modal.active) && (nextModal && nextModal.active);

const mapStateToProps = ({
  app: {
    modals: { marketSelectorModal: modal }
  }
}) => ({
  modal
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
      currentBlackoutMarkets: []
    };
  }

  componentWillReceiveProps(nextProps) {
    const { proposalEditForm, modal } = this.props;
    const { MarketGroup, Markets, BlackoutMarketGroup } = proposalEditForm;
    const isCustom =
      (Markets && Markets.length > 0) || (MarketGroup && BlackoutMarketGroup);
    if (isCustom && isOpenning(modal, nextProps.modal)) {
      const blackoutMarkets = Markets.filter(market => market.IsBlackout);
      const selectedMarkets = Markets.filter(market => !market.IsBlackout);
      this.setState({
        blackoutMarkets: [].concat(blackoutMarkets || [], MarketGroup),
        currentBlackoutMarkets: blackoutMarkets,
        selectedMarkets: [].concat(selectedMarkets || [], MarketGroup),
        currentSelectedMarkets: selectedMarkets
      });
    }
  }

  onMarketsSelectionChange(markets, selectorName) {
    if (selectorName === "Markets") {
      this.setState({ currentSelectedMarkets: markets });
    } else {
      this.setState({ currentBlackoutMarkets: markets });
    }
  }

  cancel() {
    const { selectedMarkets, blackoutMarkets } = this.state;
    const { toggleModal } = this.props;
    this.setState({
      currentSelectedMarkets: selectedMarkets,
      currentBlackoutMarkets: blackoutMarkets
    });

    toggleModal({
      modal: "marketSelectorModal",
      active: false
    });
  }

  save() {
    const { updateProposalEditForm, toggleModal } = this.props;
    let { currentSelectedMarkets, currentBlackoutMarkets } = this.state;

    let marketGroup = null;
    const simpleMarkets = [];
    currentSelectedMarkets = currentSelectedMarkets.filter(m => m !== null);
    currentSelectedMarkets.map(market => {
      if (market.Display === "All") {
        marketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: false
        });
      }

      return market;
    });

    let blackoutMarketGroup = null;
    currentBlackoutMarkets = currentBlackoutMarkets.filter(m => m !== null);
    currentBlackoutMarkets.map(market => {
      // if (market.Count) {
      if (market.Display === "All") {
        blackoutMarketGroup = market.Id;
      } else {
        simpleMarkets.push({
          ...market,
          IsBlackout: true
        });
      }

      return market;
    });

    updateProposalEditForm({ key: "Markets", value: simpleMarkets });
    updateProposalEditForm({ key: "MarketGroup", value: null });
    updateProposalEditForm({ key: "MarketGroupId", value: marketGroup });
    updateProposalEditForm({ key: "BlackoutMarketGroup", value: null });
    updateProposalEditForm({
      key: "BlackoutMarketGroupId",
      value: blackoutMarketGroup
    });

    this.setState({
      selectedMarkets: currentSelectedMarkets,
      blackoutMarkets: currentBlackoutMarkets
    });

    toggleModal({
      modal: "marketSelectorModal",
      active: false
    });
  }

  render() {
    const { initialdata, modal, isReadOnly } = this.props;
    const { currentSelectedMarkets, currentBlackoutMarkets } = this.state;

    const marketGroup = initialdata.MarketGroups.find(
      market => market.Id === 1
    );

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
                markets={[marketGroup, ...initialdata.Markets]}
                selectedMarkets={currentSelectedMarkets}
                onMarketsSelectionChange={this.onMarketsSelectionChange}
                isReadOnly={isReadOnly}
              />
            </Col>

            <Col md={6}>
              <MarketSelector
                name="Blackout Markets"
                markets={[marketGroup, ...initialdata.Markets]}
                selectedMarkets={currentBlackoutMarkets}
                onMarketsSelectionChange={this.onMarketsSelectionChange}
                isReadOnly={isReadOnly}
              />
            </Col>
          </Row>
        </Modal.Body>

        <Modal.Footer>
          <Button onClick={this.cancel} bsStyle="danger">
            Cancel
          </Button>
          <Button onClick={this.save} disabled={isReadOnly} bsStyle="success">
            Save
          </Button>
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
  isReadOnly: PropTypes.bool
};

MarketGroupSelector.defaultProps = {
  modal: null,
  toggleModal: () => {},
  initialdata: null,
  proposalEditForm: null,
  updateProposalEditForm: null,
  isReadOnly: false
};

const styledComponent = CSSModules(MarketGroupSelector, styles);
export default connect(mapStateToProps)(styledComponent);
