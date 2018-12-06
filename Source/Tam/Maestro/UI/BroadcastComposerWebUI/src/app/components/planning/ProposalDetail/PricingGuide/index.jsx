import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Modal, Button, Row, Col } from "react-bootstrap";
import { bindActionCreators } from "redux";

import { toggleModal, createAlert } from "Ducks/app";
import {
  updateProposalEditFormDetail,
  loadOpenMarketData,
  allocateSpots,
  clearOpenMarketData,
  savePricingData,
  showEditMarkets,
  updateEditMarkets,
  updateProprietaryCpms
} from "Ducks/planning";
import {
  panelsList,
  numberRender,
  initialState,
  calculateBalanceSum,
  parsePrograms
} from "./util";
import "./index.scss";

const isActiveDialog = (detail, modal) =>
  modal && detail && modal.properties.detailId === detail.Id && modal.active;

const mapStateToProps = ({
  app: {
    modals: { pricingGuide: modal }
  },
  planning: {
    proposalEditForm,
    activeOpenMarketData,
    hasOpenMarketData,
    isOpenMarketDataSortName,
    openMarketLoading,
    openMarketLoaded,
    activeEditMarkets,
    isEditMarketsActive,
    hasActiveDistribution
  }
}) => ({
  modal,
  proposalEditForm,
  activeOpenMarketData,
  hasOpenMarketData,
  isOpenMarketDataSortName,
  openMarketLoading,
  openMarketLoaded,
  activeEditMarkets,
  isEditMarketsActive,
  hasActiveDistribution
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      createAlert,
      loadOpenMarketData,
      clearOpenMarketData,
      showEditMarkets,
      allocateSpots,
      savePricingData,
      updateEditMarkets,
      updateProprietaryCpms,
      updateDetail: updateProposalEditFormDetail
    },
    dispatch
  );

class PricingGuide extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onApply = this.onApply.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.onError = this.onError.bind(this);
    this.clearState = this.clearState.bind(this);
    this.submitChanges = this.submitChanges.bind(this);
    this.onClickToSave = this.onClickToSave.bind(this);
    this.onClickToDiscard = this.onClickToDiscard.bind(this);

    this.setInventory = this.setInventory.bind(this);
    this.onRunDistribution = this.onRunDistribution.bind(this);
    this.getDistributionRequest = this.getDistributionRequest.bind(this);
    this.onUpdateEditMarkets = this.onUpdateEditMarkets.bind(this);
    this.setProprietaryPricing = this.setProprietaryPricing.bind(this);
    this.setOpenMarketPricing = this.setOpenMarketPricing.bind(this);
    this.getOpenMarketShare = this.getOpenMarketShare.bind(this);
    this.onUpdateProprietaryCpms = this.onUpdateProprietaryCpms.bind(this);
    this.onAllocateSpots = this.onAllocateSpots.bind(this);

    this.onModalShow = this.onModalShow.bind(this);

    this.state = initialState;
  }

  componentDidUpdate(prevProps) {
    const { modal, detail } = this.props;
    // clear local state if modal window are closing
    if (
      isActiveDialog(prevProps.detail, prevProps.modal) &&
      !isActiveDialog(detail, modal)
    ) {
      this.clearState();
    }
  }

  submitChanges(nextValues, cb) {
    this.setState(
      {
        ...nextValues,
        isDistributionRunned: false,
        isGuideChanged: true
      },
      cb
    );
  }

  onModalShow() {
    // process inventory/proprietary pricing/ open market just once on open
    const { activeOpenMarketData, showEditMarkets } = this.props;
    if (activeOpenMarketData) {
      showEditMarkets(false);
      this.setInventory(activeOpenMarketData);
      this.setProprietaryPricing(activeOpenMarketData);
      this.setOpenMarketPricing(activeOpenMarketData);
    }
  }

  // INVENTORY Goals and Adjustments

  setInventory(guide) {
    this.setState({
      impression: guide.ImpressionGoal,
      budget: guide.BudgetGoal,
      margin: guide.Margin,
      impressionLoss: guide.ImpressionLoss,
      inflation: guide.Inflation
    });
  }

  // PROPRIETARY

  // set states from detail ProprietaryPricing array
  // InventorySource 3 (TVB), 4 (TTNW), 5 (CNN), 6 (Sinclair)
  setProprietaryPricing({ ProprietaryPricing }) {
    const {
      initialdata: { ProprietaryPricingInventorySources }
    } = this.props;
    const toUpdate = {};
    ProprietaryPricingInventorySources.forEach(({ Id, Display }) => {
      const { ImpressionsBalance: bal = 0, Cpm: cpm = 0 } =
        (ProprietaryPricing || []).find(it => Id === it.InventorySource) || {};
      toUpdate[`propImpressions${Display}`] = bal;
      toUpdate[`propCpm${Display}`] = cpm;
    });
    this.setState(toUpdate);
  }

  setOpenMarketPricing(guide) {
    if (guide.OpenMarketPricing) {
      const openData = guide.OpenMarketPricing;
      const target = openData.OpenMarketCpmTarget || 1;
      this.setState({
        openCpmMax: openData.CpmMax,
        openCpmMin: openData.CpmMin,
        openCpmTarget: target,
        openUnitCap: openData.UnitCapPerStation
      });
    }
  }

  getOpenMarketShare() {
    const { initialdata } = this.props;

    const balanceSum = calculateBalanceSum(
      initialdata.ProprietaryPricingInventorySources,
      this.state
    );
    const share = 1 - balanceSum;
    return Number(share.toFixed(2));
  }

  clearState() {
    this.setState(initialState);
  }

  getDistributionRequest() {
    const { detail, proposalEditForm } = this.props;
    const proprietaryData = this.saveProprietaryPricingDetail();
    const {
      openCpmMax,
      openCpmMin,
      openCpmTarget,
      openUnitCap,
      budget,
      margin,
      impressionLoss,
      inflation,
      impression
    } = this.state;
    const request = {
      Margin: margin || null,
      ImpressionLoss: impressionLoss || null,
      Inflation: inflation || null,
      ProposalId: proposalEditForm.Id,
      ProposalDetailId: detail.Id,
      BudgetGoal: budget || null,
      ImpressionGoal: impression || null,
      ProprietaryPricing: proprietaryData,
      OpenMarketPricing: {
        CpmMax: openCpmMax || null,
        CpmMin: openCpmMin || null,
        OpenMarketCpmTarget: openCpmTarget,
        UnitCapPerStation: openUnitCap || null
      },
      OpenMarketShare: this.getOpenMarketShare()
    };
    return request;
  }

  // run with params - temporary until get new open market BE object
  onRunDistribution() {
    const request = this.getDistributionRequest();
    this.props.loadOpenMarketData(request);
    this.setState({ isDistributionRunned: true });
  }
  // call from edit markets to get params needed here
  onUpdateEditMarkets() {
    const request = this.getDistributionRequest();
    this.props.updateEditMarkets(request);
  }

  onUpdateProprietaryCpms() {
    const { hasActiveDistribution, updateProprietaryCpms } = this.props;
    if (hasActiveDistribution) {
      const request = this.getDistributionRequest();
      updateProprietaryCpms(request);
    }
  }

  // change to inner object PricingGuide - need to combine call to updateDetail else each overrides other
  onApply() {
    const {
      impression,
      budget,
      margin,
      impressionLoss,
      inflation
    } = this.state;
    const { savePricingData, detail, activeOpenMarketData = {} } = this.props;
    const proprietaryData = this.saveProprietaryPricingDetail();
    const openData = this.saveOpenMarketPricingDetail();
    // change to update inner object
    const guideUpdates = {
      ...openData,
      ProposalDetailId: detail.Id,
      GoalImpression: impression || null,
      GoalBudget: budget || null,
      Margin: margin || null,
      ImpressionLoss: impressionLoss || null,
      Inflation: inflation || null,
      ProprietaryPricing: proprietaryData,
      ProprietaryTotals: activeOpenMarketData.ProprietaryTotals,
      OpenMarketTotals: activeOpenMarketData.OpenMarketTotals,
      Markets: parsePrograms(activeOpenMarketData.Markets)
    };
    savePricingData(guideUpdates);
    this.setState({ isGuideChanged: false });
  }

  onSave() {
    const { toggleModal } = this.props;
    this.onApply();
    toggleModal({
      modal: "pricingGuide",
      active: false,
      properties: { detailId: this.props.detail.Id }
    });
  }

  // update detail - with proprietary pricing states
  saveProprietaryPricingDetail() {
    const {
      initialdata: { ProprietaryPricingInventorySources: invSrcEnum }
    } = this.props;
    const proprietaryPricing = invSrcEnum.map(i => ({
      InventorySource: i.Id,
      ImpressionsBalance: this.state[`propImpressions${i.Display}`],
      Cpm: this.state[`propCpm${i.Display}`]
    }));
    return proprietaryPricing;
  }

  saveOpenMarketPricingDetail() {
    const { openCpmMax, openCpmMin, openCpmTarget, openUnitCap } = this.state;
    const openData = {
      CpmMax: openCpmMax || null,
      CpmMin: openCpmMin || null,
      OpenMarketCpmTarget: openCpmTarget,
      UnitCapPerStation: openUnitCap || null
    };
    return openData;
  }

  // intercept from grid to update  distribution request
  onAllocateSpots(openMarketData) {
    const distribution = this.getDistributionRequest();
    this.props.allocateSpots({
      ...openMarketData,
      ...distribution
    });
    this.setState({ isGuideChanged: true });
  }

  handleChange(fieldName, value) {
    const newVal = !isNaN(value) ? value : 0;
    this.setState({ [fieldName]: newVal });
  }

  onCancel() {
    this.props.toggleModal({
      modal: "pricingGuide",
      active: false,
      properties: { detailId: this.props.detail.Id }
    });
    this.props.clearOpenMarketData();
  }

  onClickToSave(cb) {
    const { isDistributionRunned } = this.state;
    if (isDistributionRunned) {
      return cb;
    }
    return () => {
      this.onError("distribution");
    };
  }

  onClickToDiscard() {
    const { isGuideChanged } = this.state;
    if (isGuideChanged) {
      return this.onError("discard");
    }
    return this.onCancel();
  }

  onError(errorName) {
    const { [errorName]: error } = this.state;
    this.setState({ [errorName]: !error });
  }

  render() {
    const { modal, detail, isReadOnly, activeOpenMarketData } = this.props;
    const { distribution, discard } = this.state;
    const show = isActiveDialog(detail, modal);
    return (
      <div>
        <Modal
          show={show}
          onEntered={this.onModalShow}
          dialogClassName="large-wide-modal"
        >
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.onCancel}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Row>
              <Col sm={6}>
                <Modal.Title>Pricing Guide</Modal.Title>
              </Col>
              <Col sm={6}>
                <div className="summary-bar" style={{ marginRight: "32px" }}>
                  <div className="summary-item">
                    {/* <div className="summary-tag">--%</div> */}
                    <div className="summary-display">
                      {numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Coverage",
                        "0.000"
                      )}%
                    </div>
                    <div className="summary-label">MARKET COVERAGE</div>
                  </div>
                  <div className="summary-item">
                    {/* <div className="summary-tag">--%</div> */}
                    <div className="summary-display">
                      ${numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Cpm",
                        "0,0.00"
                      )}
                    </div>
                    <div className="summary-label">CPM</div>
                  </div>
                  <div className="summary-item">
                    {/* <div className="summary-tag">--%</div> */}
                    <div className="summary-display">
                      {numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Impressions",
                        "0,0",
                        1000
                      )}
                    </div>
                    <div className="summary-label">IMPRESSIONS (000)</div>
                  </div>
                  <div className="summary-item">
                    {/* <div className="summary-tag">--%</div> */}
                    <div className="summary-display">
                      ${numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Cost",
                        "0,0"
                      )}
                    </div>
                    <div className="summary-label">TOTAL COST</div>
                  </div>
                </div>
              </Col>
            </Row>
          </Modal.Header>

          <Modal.Body className="modalBodyScroll">
            {panelsList.map(panel =>
              panel.render({
                key: `prcing-guide-panel#${panel.id}`,
                ...this.props,
                ...this.state,
                modal: modal && modal.active,
                submit: this.submitChanges,
                onUpdateProprietaryCpms: this.onUpdateProprietaryCpms,
                onUpdateEditMarkets: this.onUpdateEditMarkets,
                onAllocateSpots: this.onAllocateSpots,
                onRunDistribution: this.onRunDistribution
              })
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onClickToDiscard} bsStyle="default">
              Cancel
            </Button>
            <Button
              disabled={isReadOnly}
              onClick={this.onClickToSave(this.onApply)}
              bsStyle="success"
            >
              Apply
            </Button>
            <Button
              disabled={isReadOnly}
              onClick={this.onClickToSave(this.onSave)}
              bsStyle="success"
            >
              Save
            </Button>
          </Modal.Footer>
        </Modal>
        <Modal show={distribution}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={() => this.onError("distribution")}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            You have changed goals, but did not run Distribution Algorithm.
            Either run Distribution Algorithm or discard changes
          </Modal.Body>

          <Modal.Footer>
            <Button
              onClick={() => this.onError("distribution")}
              bsStyle="warning"
            >
              Dismiss
            </Button>
          </Modal.Footer>
        </Modal>
        <Modal show={discard}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={() => this.onError("discard")}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>Are you sure you want to discard changes?</Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="warning">
              Discard
            </Button>
            <Button onClick={() => this.onError("discard")}>Cancel</Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

PricingGuide.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool,
  clearOpenMarketData: PropTypes.func.isRequired,
  loadOpenMarketData: PropTypes.func.isRequired,
  updateProprietaryCpms: PropTypes.func.isRequired,
  detail: PropTypes.object,
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  activeOpenMarketData: PropTypes.object,
  allocateSpots: PropTypes.func.isRequired,
  hasActiveDistribution: PropTypes.bool.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
  savePricingData: PropTypes.func.isRequired,
  updateEditMarkets: PropTypes.func.isRequired
};

PricingGuide.defaultProps = {
  modal: null,
  isReadOnly: false,
  activeOpenMarketData: undefined,
  detail: undefined
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PricingGuide);
