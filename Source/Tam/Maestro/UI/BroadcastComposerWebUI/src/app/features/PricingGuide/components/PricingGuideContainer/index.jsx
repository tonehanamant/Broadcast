import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
  Modal,
  Button,
  Row,
  Col,
  Radio,
  ToggleButtonGroup
} from "react-bootstrap";
import { bindActionCreators } from "redux";
import CSSModules from "react-css-modules";
import { toggleModal, createAlert } from "Main/redux/ducks";
import {
  updateProposalEditFormDetail,
  loadOpenMarketData,
  allocateSpots,
  clearOpenMarketData,
  savePricingData,
  showEditMarkets,
  updateEditMarkets,
  copyToBuy,
  copyToBuyFlow,
  hasSpotsAllocate,
  onCopyConfirmMsg,
  updateProprietaryCpms
} from "Ducks/planning";
import { numberRender, calculateBalanceSum } from "PricingGuide/util/helpers";
import {
  panelsList,
  initialState,
  getDistributionPrograms,
  parseProgramsToList
} from "./util";
import PricingProposalSummary from "../PricingGuideSummary";
import styles from "./index.style.scss";

const isActiveDialog = (detail, modal) =>
  modal && detail && modal.properties.detailId === detail.Id && modal.active;

const mapStateToProps = ({
  app: {
    modals: { pricingGuide: modal }
  },
  planning: {
    proposalEditForm,
    activeOpenMarketData,
    hasSpotsAllocated,
    isSpotsCopied,
    hasOpenMarketData,
    isOpenMarketDataSortName,
    openMarketLoading,
    openMarketLoaded,
    openMarketData,
    activeEditMarkets,
    isEditMarketsActive,
    hasActiveDistribution
  }
}) => ({
  modal,
  proposalEditForm,
  activeOpenMarketData,
  hasOpenMarketData,
  hasSpotsAllocated,
  isSpotsCopied,
  isOpenMarketDataSortName,
  openMarketLoading,
  openMarketLoaded,
  openMarketData,
  activeEditMarkets,
  isEditMarketsActive,
  hasActiveDistribution
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      createAlert,
      onCopyConfirmMsg,
      loadOpenMarketData,
      clearOpenMarketData,
      showEditMarkets,
      allocateSpots,
      savePricingData,
      updateEditMarkets,
      updateProprietaryCpms,
      copyToBuy,
      copyToBuyFlow,
      hasSpotsAllocate,
      updateDetail: updateProposalEditFormDetail
    },
    dispatch
  );

class PricingGuideContainer extends Component {
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
    this.setGuideEditing = this.setGuideEditing.bind(this);

    this.onCopyToBuy = this.onCopyToBuy.bind(this);
    this.clearSpots = this.clearSpots.bind(this);
    this.onClickToRunDistribution = this.onClickToRunDistribution.bind(this);
    this.onChangeDistributionOption = this.onChangeDistributionOption.bind(
      this
    );
    this.closeDistributionDialog = this.closeDistributionDialog.bind(this);
    this.hasSpotsAllocate = this.hasSpotsAllocate.bind(this);
    this.copyToBuyFlow = this.copyToBuyFlow.bind(this);
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

  onModalShow() {
    const { activeOpenMarketData, showEditMarkets } = this.props;
    if (activeOpenMarketData) {
      showEditMarkets(false);
      this.setInventory(activeOpenMarketData);
      this.setProprietaryPricing(activeOpenMarketData);
      this.setOpenMarketPricing(activeOpenMarketData);
    }
  }

  onRunDistribution() {
    const {
      activeOpenMarketData: { Markets },
      loadOpenMarketData
    } = this.props;
    const { isAutoDistribution, changedPrograms } = this.state;
    const request = this.getDistributionRequest();
    loadOpenMarketData({
      KeepManuallyEditedSpots: !isAutoDistribution,
      ProgramsWithManuallyEditedSpots: isAutoDistribution
        ? []
        : getDistributionPrograms(Markets, changedPrograms),
      ...request
    });
    this.setState({
      isDistributionRunned: true,
      isSpotsChanged: false,
      discardSpots: false,
      isAutoDistribution: true,
      isGuideApplied: false,
      confirmationDistribution: false
    });
  }

  onChangeDistributionOption(value) {
    this.setState({ isAutoDistribution: value });
  }

  onUpdateEditMarkets() {
    const { updateEditMarkets } = this.props;
    const request = this.getDistributionRequest();
    updateEditMarkets(request);
  }

  onUpdateProprietaryCpms() {
    const { hasActiveDistribution, updateProprietaryCpms } = this.props;
    if (hasActiveDistribution) {
      const request = this.getDistributionRequest();
      updateProprietaryCpms(request);
    }
  }

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
      Markets: parseProgramsToList(activeOpenMarketData.Markets),
      MarketCoverageFileId: activeOpenMarketData.MarketCoverageFileId
    };
    savePricingData(guideUpdates);
    this.setState({
      isGuideChanged: false,
      isSpotsChanged: false,
      isGuideApplied: true
    });
  }

  onSave() {
    const { toggleModal, detail } = this.props;
    this.onApply();
    toggleModal({
      modal: "pricingGuide",
      active: false,
      properties: { detailId: detail.Id }
    });
  }

  onCopyToBuy() {
    const { detail, copyToBuy } = this.props;
    copyToBuy(detail.Id);
  }

  onAllocateSpots(openMarketData, row) {
    const { changedPrograms } = this.state;
    const { allocateSpots } = this.props;
    const distribution = this.getDistributionRequest();
    allocateSpots(
      {
        ...openMarketData,
        ...distribution
      },
      row
    );
    this.setState({
      isGuideChanged: true,
      isSpotsChanged: true,
      changedPrograms: changedPrograms.concat(row.ProgramId)
    });
  }

  onCancel() {
    const { toggleModal, detail, clearOpenMarketData } = this.props;
    toggleModal({
      modal: "pricingGuide",
      active: false,
      properties: { detailId: detail.Id }
    });
    clearOpenMarketData();
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

  onClickToRunDistribution() {
    const { isSpotsChanged, isGuideApplied } = this.state;
    if (!isSpotsChanged && isGuideApplied) {
      this.onError("confirmationDistribution");
    } else if (isSpotsChanged && !isGuideApplied) {
      this.onError("discardSpots");
    } else {
      this.onRunDistribution();
    }
  }

  onError(errorName) {
    const { [errorName]: error } = this.state;
    this.setState({ [errorName]: !error });
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

  getOpenMarketShare() {
    const { initialdata } = this.props;

    const balanceSum = calculateBalanceSum(
      initialdata.ProprietaryPricingInventorySources,
      this.state
    );
    const share = 1 - balanceSum;
    return Number(share.toFixed(2));
  }

  setInventory(guide) {
    this.setState({
      impression: guide.ImpressionGoal,
      budget: guide.BudgetGoal,
      margin: guide.Margin,
      impressionLoss: guide.ImpressionLoss,
      inflation: guide.Inflation
    });
  }

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

  setGuideEditing(edit) {
    this.setState({ isGuideEditing: edit });
  }

  handleChange(fieldName, value) {
    const newVal = !Number.isNaN(value) ? value : 0;
    this.setState({ [fieldName]: newVal });
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

  saveProprietaryPricingDetail() {
    const {
      initialdata: { ProprietaryPricingInventorySources: invSrcEnum }
    } = this.props;
    const proprietaryPricing = invSrcEnum.map(({ Id, Display }) => {
      const {
        [`propImpressions${Display}`]: impression,
        [`propCpm${Display}`]: cpm
      } = this.state;
      return {
        InventorySource: Id,
        ImpressionsBalance: impression,
        Cpm: cpm
      };
    });
    return proprietaryPricing;
  }

  copyToBuyFlow() {
    const { detail, copyToBuyFlow } = this.props;
    copyToBuyFlow(detail.Id);
  }

  hasSpotsAllocate() {
    const { detail, hasSpotsAllocate, activeOpenMarketData } = this.props;
    hasSpotsAllocate(detail.Id, activeOpenMarketData.hasSpotsAllocated);
  }

  clearState() {
    this.setState(initialState);
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

  clearSpots() {
    const { openMarketData, activeOpenMarketData } = this.props;
    this.onAllocateSpots({
      ...activeOpenMarketData,
      Markets: openMarketData.Markets
    });
  }

  closeDistributionDialog() {
    this.setState({ isAutoDistribution: true });
    this.onError("confirmationDistribution");
  }

  render() {
    const {
      modal,
      detail,
      isReadOnly,
      activeOpenMarketData,
      hasSpotsAllocated,
      onCopyConfirmMsg,
      isSpotsCopied,
      isEditMarketsActive,
      initialdata,
      proposalEditForm
    } = this.props;
    const {
      distribution,
      discard,
      isGuideEditing,
      discardSpots,
      isAutoDistribution,
      confirmationDistribution
    } = this.state;
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
                    {/* <div styleName="summary-tag">--%</div> */}
                    <div className="summary-display">
                      {numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Coverage",
                        "0.000"
                      )}
                      %
                    </div>
                    <div className="summary-label">MARKET COVERAGE</div>
                  </div>
                  <div className="summary-item">
                    {/* <div styleName="summary-tag">--%</div> */}
                    <div className="summary-display">
                      $
                      {numberRender(
                        activeOpenMarketData,
                        "PricingTotals.Cpm",
                        "0,0.00"
                      )}
                    </div>
                    <div className="summary-label">CPM</div>
                  </div>
                  <div className="summary-item">
                    {/* <div styleName="summary-tag">--%</div> */}
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
                    {/* <div styleName="summary-tag">--%</div> */}
                    <div className="summary-display">
                      $
                      {numberRender(
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
            <Row>
              <Col sm={12}>
                <PricingProposalSummary
                  initialdata={initialdata}
                  proposalEditForm={proposalEditForm}
                  detail={detail}
                />
              </Col>
            </Row>
          </Modal.Header>

          <Modal.Body styleName="modalBodyScroll">
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
                onRunDistribution: this.onClickToRunDistribution,
                onCopyToBuy: this.copyToBuyFlow,
                onSetGuideEditing: this.setGuideEditing
              })
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button
              onClick={this.onClickToDiscard}
              disabled={isGuideEditing || isEditMarketsActive}
              bsStyle="default"
            >
              Cancel
            </Button>
            <Button
              disabled={isReadOnly || isGuideEditing || isEditMarketsActive}
              onClick={this.onClickToSave(this.onApply)}
              bsStyle="success"
            >
              Apply
            </Button>
            <Button
              disabled={isReadOnly || isGuideEditing || isEditMarketsActive}
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

        <Modal show={confirmationDistribution}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.closeDistributionDialog}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <div styleName="pricing-guide_confirmation-message">
              <span styleName="confirmation-text">
                You have changed spots, how do you want to run distribution?
              </span>
              <ToggleButtonGroup
                type="radio"
                name="options"
                value={isAutoDistribution}
                onChange={this.onChangeDistributionOption}
              >
                <Radio styleName="confirmation-checkbox" value>
                  Discard spots you have edited or modified
                </Radio>
                <Radio styleName="confirmation-checkbox" value={false}>
                  Maintain edited or modified spots
                </Radio>
              </ToggleButtonGroup>
            </div>
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.closeDistributionDialog}>Cancel</Button>
            <Button onClick={this.onRunDistribution} bsStyle="primary">
              Run Distribution
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
            <Button onClick={this.onCancel} bsStyle="primary">
              Discard
            </Button>
            <Button onClick={() => this.onError("discard")}>Cancel</Button>
          </Modal.Footer>
        </Modal>
        <Modal show={discardSpots}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={() => this.onError("discardSpots")}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>Are you sure you want to discard spots?</Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onRunDistribution} bsStyle="primary">
              Run Distribution
            </Button>
            <Button onClick={() => this.onError("discardSpots")}>Cancel</Button>
          </Modal.Footer>
        </Modal>
        <Modal show={hasSpotsAllocated}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.hasSpotsAllocate}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            Proposal has spots allocated in Open Market buy already Continuing
            will overwrite any existing spot allocated. Cancel will stop copy.
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onCopyToBuy} bsStyle="primary">
              Continue
            </Button>
            <Button onClick={this.hasSpotsAllocate}>Cancel</Button>
          </Modal.Footer>
        </Modal>
        <Modal show={isSpotsCopied}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={onCopyConfirmMsg}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Warning</Modal.Title>
          </Modal.Header>
          <Modal.Body>Spots copied succesfully to Open Market Buy.</Modal.Body>
          <Modal.Footer>
            <Button onClick={onCopyConfirmMsg} bsStyle="success">
              Ok
            </Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

PricingGuideContainer.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool,
  hasSpotsAllocated: PropTypes.bool.isRequired,
  isEditMarketsActive: PropTypes.bool.isRequired,
  isSpotsCopied: PropTypes.bool.isRequired,
  clearOpenMarketData: PropTypes.func.isRequired,
  loadOpenMarketData: PropTypes.func.isRequired,
  onCopyConfirmMsg: PropTypes.func.isRequired,
  updateProprietaryCpms: PropTypes.func.isRequired,
  detail: PropTypes.object,
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  activeOpenMarketData: PropTypes.object,
  openMarketData: PropTypes.object,
  allocateSpots: PropTypes.func.isRequired,
  hasActiveDistribution: PropTypes.bool.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
  savePricingData: PropTypes.func.isRequired,
  copyToBuy: PropTypes.func.isRequired,
  copyToBuyFlow: PropTypes.func.isRequired,
  hasSpotsAllocate: PropTypes.func.isRequired,
  updateEditMarkets: PropTypes.func.isRequired
};

PricingGuideContainer.defaultProps = {
  modal: null,
  isReadOnly: false,
  activeOpenMarketData: undefined,
  openMarketData: undefined,
  detail: undefined
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(PricingGuideContainer, styles));
