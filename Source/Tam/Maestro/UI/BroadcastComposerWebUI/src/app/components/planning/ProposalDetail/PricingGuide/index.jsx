import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
  Modal,
  Button,
  Panel,
  Table,
  Label,
  FormControl,
  Glyphicon,
  Row,
  Col,
  FormGroup,
  ControlLabel,
  ToggleButtonGroup,
  ToggleButton
} from "react-bootstrap";
import { bindActionCreators } from "redux";
import { InputNumber } from "antd";
import numeral from "numeral";
import { get } from "lodash";

import { toggleModal } from "Ducks/app";
import {
  updateProposalEditFormDetail,
  loadOpenMarketData,
  allocateSpots,
  clearOpenMarketData,
  showEditMarkets,
  updateEditMarkets
} from "Ducks/planning";
import PricingGuideGrid from "./PricingGuideGrid";
import PricingGuideEditMarkets from "./PricingGuideEditMarkets";
import "./index.scss";

const isActiveDialog = (detail, modal) =>
  modal && detail && modal.properties.detailId === detail.Id && modal.active;

const numberRender = (data, path, format, divideBy) => {
  let number = get(data, path);
  if (number && divideBy) number /= divideBy;
  return number ? numeral(number).format(format) : "--";
};

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
    isEditMarketsActive
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
  isEditMarketsActive
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      loadOpenMarketData,
      clearOpenMarketData,
      showEditMarkets,
      allocateSpots,
      updateEditMarkets,
      updateDetail: updateProposalEditFormDetail
    },
    dispatch
  );

class PricingGuide extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.clearState = this.clearState.bind(this);

    this.setInventory = this.setInventory.bind(this);
    this.toggleInventoryEditing = this.toggleInventoryEditing.bind(this);
    this.saveInventory = this.saveInventory.bind(this);
    this.cancelInventory = this.cancelInventory.bind(this);
    this.onRunDistribution = this.onRunDistribution.bind(this);
    this.getDistributionRequest = this.getDistributionRequest.bind(this);
    this.onUpdateEditMarkets = this.onUpdateEditMarkets.bind(this);

    this.saveProprietaryPricingDetail = this.saveProprietaryPricingDetail.bind(
      this
    );
    this.setProprietaryPricing = this.setProprietaryPricing.bind(this);
    this.toggleProprietaryEditing = this.toggleProprietaryEditing.bind(this);
    this.saveProprietary = this.saveProprietary.bind(this);
    this.cancelProprietary = this.cancelProprietary.bind(this);

    this.saveOpenMarketPricingDetail = this.saveOpenMarketPricingDetail.bind(
      this
    );
    this.setOpenMarketPricing = this.setOpenMarketPricing.bind(this);
    this.toggleOpenMarketEditing = this.toggleOpenMarketEditing.bind(this);
    this.saveOpenMarket = this.saveOpenMarket.bind(this);
    this.cancelOpenMarket = this.cancelOpenMarket.bind(this);
    this.handleCpmTargetChange = this.handleCpmTargetChange.bind(this);

    this.onModalShow = this.onModalShow.bind(this);

    this.state = {
      // goals/adjustments - editing version separate state to cancel/save individually
      impression: "",
      budget: "",
      margin: "",
      rateInflation: "",
      impressionInflation: "",
      isInventoryEditing: false,
      editingImpression: "",
      editingBudget: "",
      editingMargin: "",
      editingRateInflation: "",
      editingImpressionInflation: "",
      // proprietary based on array - break down here (uses hard coded values for CPM for now)
      isProprietaryEditing: false,
      propCpmCNN: 8.0,
      propCpmSinclair: 10.0,
      propCpmTTNW: 12.0,
      propCpmTVB: 14.0,
      propImpressionsCNN: 0,
      propImpressionsSinclair: 0,
      propImpressionsTTNW: 0,
      propImpressionsTVB: 0,
      editingPropImpressionsCNN: 0,
      editingPropImpressionsSinclair: 0,
      editingPropImpressionsTTNW: 0,
      editingPropImpressionsTVB: 0,
      // open market
      isOpenMarketEditing: false,
      openCpmMin: null,
      openCpmMax: null,
      openUnitCap: null,
      openCpmTarget: 1,
      editingOpenCpmMin: null,
      editingOpenCpmMax: null,
      editingOpenUnitCap: null,
      editingOpenCpmTarget: 1
    };
  }

  componentDidUpdate(prevProps) {
    const { modal, detail } = this.props;
    const prevActiveDialog = isActiveDialog(prevProps.detail, prevProps.modal);
    const activeDialog = isActiveDialog(detail, modal);
    // clear local state if modal window are closing
    if (prevActiveDialog && !activeDialog) {
      this.clearState();
    }
  }

  // this is unreliable - updates constantly and clears set states
  /* componentWillReceiveProps(nextProps) {
    if (nextProps.detail) {
      console.log("component receive props detail", nextProps.detail);
      this.setState({
        impression: nextProps.detail.GoalImpression,
        budget: nextProps.detail.GoalBudget,
        margin: nextProps.detail.AdjustmentMargin,
        rateInflation: nextProps.detail.AdjustmentRate,
        impressionInflation: nextProps.detail.AdjustmentInflation,
        editingImpression: nextProps.detail.GoalImpression,
        editingBudget: nextProps.detail.GoalBudget,
        editingMargin: nextProps.detail.AdjustmentMargin,
        editingRateInflation: nextProps.detail.AdjustmentRate,
        editingImpressionInflation: nextProps.detail.AdjustmentInflation
      });
    }
  } */

  onModalShow() {
    // console.log('MODAL SHOW>>>>>>>>>', this.props.detail);
    // process inventory/proprietary pricing/ open market just once on open
    this.props.showEditMarkets(false);
    this.setInventory(this.props.detail.PricingGuide);
    this.setProprietaryPricing(this.props.detail.PricingGuide);
    this.setOpenMarketPricing(this.props.detail.PricingGuide);
  }

  // INVENTORY Goals and Adjustments

  setInventory(guide) {
    if (guide) {
      this.setState({
        impression: guide.GoalImpression,
        budget: guide.GoalBudget,
        margin: guide.AdjustmentMargin,
        rateInflation: guide.AdjustmentRate,
        impressionInflation: guide.AdjustmentInflation
      });
      this.setState({
        editingImpression: guide.GoalImpression,
        editingBudget: guide.GoalBudget,
        editingMargin: guide.AdjustmentMargin,
        editingRateInflation: guide.AdjustmentRate,
        editingImpressionInflation: guide.AdjustmentInflation
      });
    }
  }

  toggleInventoryEditing() {
    this.setState({ isInventoryEditing: !this.state.isInventoryEditing });
  }

  saveInventory() {
    this.setState({
      impression: this.state.editingImpression,
      budget: this.state.editingBudget,
      margin: this.state.editingMargin,
      rateInflation: this.state.editingRateInflation,
      impressionInflation: this.state.editingImpressionInflation
    });
    this.toggleInventoryEditing();
  }

  cancelInventory() {
    this.setState({
      editingImpression: this.state.impression,
      editingBudget: this.state.budget,
      editingMargin: this.state.margin,
      editingRateInflation: this.state.rateInflation,
      editingImpressionInflation: this.state.impressionInflation
    });
    this.toggleInventoryEditing();
  }

  // PROPRIETARY

  // set states from detail ProprietaryPricing array - NOT using CPM from BE as hard coded
  // todo - possible compare with initial data id/objects?
  // InventorySource 3 (TVB), 4 (TTNW), 5 (CNN), 6 (Sinclair)
  setProprietaryPricing(guide) {
    // console.log('set pricing', detail.ProprietaryPricing, this);
    if (guide.ProprietaryPricing && guide.ProprietaryPricing.length) {
      const toUpdate = {};
      guide.ProprietaryPricing.forEach(item => {
        const bal = item.ImpressionsBalance;
        if (bal) {
          const src = item.InventorySource;
          if (src === 3) {
            toUpdate.propImpressionsTVB = bal;
            toUpdate.editingPropImpressionsTVB = bal;
          }
          if (src === 4) {
            toUpdate.propImpressionsTTNW = bal;
            toUpdate.editingPropImpressionsTTNW = bal;
          }
          if (src === 5) {
            toUpdate.propImpressionsCNN = bal;
            toUpdate.editingPropImpressionsCNN = bal;
          }
          if (src === 6) {
            toUpdate.propImpressionsSinclair = bal;
            toUpdate.editingPropImpressionsSinclair = bal;
          }
        }
      });
      this.setState(toUpdate);
      // console.log('set proprietary', toUpdate);
    }
  }

  toggleProprietaryEditing() {
    this.setState({ isProprietaryEditing: !this.state.isProprietaryEditing });
  }

  saveProprietary() {
    // only dealing with Impresions Balances for now - not CPM
    this.setState({
      propImpressionsCNN: this.state.editingPropImpressionsCNN,
      propImpressionsSinclair: this.state.editingPropImpressionsSinclair,
      propImpressionsTTNW: this.state.editingPropImpressionsTTNW,
      propImpressionsTVB: this.state.editingPropImpressionsTVB
    });
    this.toggleProprietaryEditing();
  }

  cancelProprietary() {
    // only dealing with Impresions Balances for now - not CPM
    this.setState({
      editingPropImpressionsCNN: this.state.propImpressionsCNN,
      editingPropImpressionsSinclair: this.state.propImpressionsSinclair,
      editingPropImpressionsTTNW: this.state.propImpressionsTTNW,
      editingPropImpressionsTVB: this.state.propImpressionsTVB
    });
    this.toggleProprietaryEditing();
  }

  toggleOpenMarketEditing() {
    this.setState({ isOpenMarketEditing: !this.state.isOpenMarketEditing });
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
      this.setState({
        editingOpenCpmMax: openData.CpmMax,
        editingOpenCpmMin: openData.CpmMin,
        editingOpenCpmTarget: target,
        editingOpenUnitCap: openData.UnitCapPerStation
      });
    }
  }

  saveOpenMarket() {
    this.setState({
      openCpmMax: this.state.editingOpenCpmMax,
      openCpmMin: this.state.editingOpenCpmMin,
      openCpmTarget: this.state.editingOpenCpmTarget,
      openUnitCap: this.state.editingOpenUnitCap
    });
    this.toggleOpenMarketEditing();
  }

  cancelOpenMarket() {
    this.setState({
      editingOpenCpmMin: this.state.openCpmMin,
      editingOpenCpmMax: this.state.openCpmMax,
      editingOpenCpmTarget: this.state.openCpmTarget,
      editingOpenUnitCap: this.state.openUnitCap
    });
    this.toggleOpenMarketEditing();
  }

  handleCpmTargetChange(val) {
    this.setState({ editingOpenCpmTarget: val });
  }

  clearState() {
    this.setState({
      editingImpression: "",
      impression: "",
      editingBudget: "",
      budget: "",
      editingMargin: "",
      margin: "",
      editingRateInflation: "",
      rateInflation: "",
      editingImpressionInflation: "",
      impressionInflation: "",
      isInventoryEditing: false,
      isProprietaryEditing: false,
      // propCpmCNN: 8.00,
      // propCpmSinclair: 10.00,
      // propCpmTTNW: 12.00,
      // propCpmTVB: 14.00,
      propImpressionsCNN: 0,
      propImpressionsSinclair: 0,
      propImpressionsTTNW: 0,
      propImpressionsTVB: 0,
      editingPropImpressionsCNN: 0,
      editingPropImpressionsSinclair: 0,
      editingPropImpressionsTTNW: 0,
      editingPropImpressionsTVB: 0,

      isOpenMarketEditing: false,
      openCpmMin: null,
      openCpmMax: null,
      openUnitCap: null,
      openCpmTarget: 1,
      editingOpenCpmMin: null,
      editingOpenCpmMax: null,
      editingOpenUnitCap: null,
      editingOpenCpmTarget: 1
    });
  }

  getDistributionRequest() {
    const { detail, proposalEditForm } = this.props;
    const {
      openCpmMax,
      openCpmMin,
      openCpmTarget,
      openUnitCap,
      budget,
      impression
    } = this.state;
    const openData = {
      CpmMax: openCpmMax,
      CpmMin: openCpmMin,
      OpenMarketCpmTarget: openCpmTarget,
      UnitCapPerStation: openUnitCap
    };
    const request = {
      ProposalId: proposalEditForm.Id,
      ProposalDetailId: detail.Id,
      BudgetGoal: budget,
      ImpressionGoal: impression,
      OpenMarketPricing: openData
    };
    return request;
  }

  // run with params - temporary until get new open market BE object
  onRunDistribution() {
    const request = this.getDistributionRequest();
    this.props.loadOpenMarketData(request);
  }
  // call from edit markets to get params needed here
  onUpdateEditMarkets() {
    const request = this.getDistributionRequest();
    this.props.updateEditMarkets(request);
  }

  // change to inner object PricingGuide - need to combine call to updateDetail else each overrides other
  onSave() {
    const {
      impression,
      budget,
      margin,
      rateInflation,
      impressionInflation
    } = this.state;
    const { updateDetail, detail } = this.props;
    const proprietaryData = this.saveProprietaryPricingDetail();
    const openData = this.saveOpenMarketPricingDetail();
    // change to update inner object
    const guideUpdates = {
      GoalImpression: impression,
      GoalBudget: budget,
      AdjustmentMargin: margin,
      AdjustmentRate: rateInflation,
      AdjustmentInflation: impressionInflation,
      OpenMarketPricing: openData,
      ProprietaryPricing: proprietaryData
    };
    updateDetail({ id: detail.Id, key: "PricingGuide", value: guideUpdates });
    /* updateDetail({ id: detail.Id, key: "GoalImpression", value: impression });
    updateDetail({ id: detail.Id, key: "GoalBudget", value: budget });
    updateDetail({ id: detail.Id, key: "AdjustmentMargin", value: margin });
    updateDetail({
      id: detail.Id,
      key: "AdjustmentRate",
      value: rateInflation
    });
    updateDetail({
      id: detail.Id,
      key: "AdjustmentInflation",
      value: impressionInflation
    }); */
    this.onCancel();
  }

  // update detail - with proprietary pricing states (CPM set for future but is harcoded)
  // // InventorySource 3 (TVB), 4 (TTNW), 5 (CNN), 6 (Sinclair)
  saveProprietaryPricingDetail() {
    // change inner object PricingGuide
    // const { updateDetail, detail } = this.props;
    const { propCpmCNN, propCpmSinclair, propCpmTTNW, propCpmTVB } = this.state;
    const {
      propImpressionsCNN,
      propImpressionsSinclair,
      propImpressionsTTNW,
      propImpressionsTVB
    } = this.state;
    const proprietaryPricing = [
      {
        InventorySource: 3,
        ImpressionsBalance: propImpressionsTVB,
        Cpm: propCpmTVB
      },
      {
        InventorySource: 4,
        ImpressionsBalance: propImpressionsTTNW,
        Cpm: propCpmTTNW
      },
      {
        InventorySource: 5,
        ImpressionsBalance: propImpressionsCNN,
        Cpm: propCpmCNN
      },
      {
        InventorySource: 6,
        ImpressionsBalance: propImpressionsSinclair,
        Cpm: propCpmSinclair
      }
    ];
    /*  updateDetail({
      id: detail.Id,
      key: "ProprietaryPricing",
      value: proprietaryPricing
    }); */
    return proprietaryPricing;
  }

  saveOpenMarketPricingDetail() {
    const { openCpmMax, openCpmMin, openCpmTarget, openUnitCap } = this.state;
    // const { updateDetail, detail } = this.props;
    // change inner object PricingGuide
    const openData = {
      CpmMax: openCpmMax,
      CpmMin: openCpmMin,
      OpenMarketCpmTarget: openCpmTarget,
      UnitCapPerStation: openUnitCap
    };
    // const guideData = { OpenMarketPricing: openData };
    // updateDetail({ id: detail.Id, key: "OpenMarketPricing", value: openData });
    // updateDetail({ id: detail.Id, key: "PricingGuide", value: guideData });
    return openData;
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

  render() {
    const {
      modal,
      detail,
      isReadOnly,
      activeOpenMarketData,
      hasOpenMarketData,
      isOpenMarketDataSortName,
      openMarketLoading,
      allocateSpots,
      openMarketLoaded,
      activeEditMarkets,
      isEditMarketsActive,
      proposalEditForm
    } = this.props;
    const show = isActiveDialog(detail, modal);
    // const labelStyle = { fontSize: '11px', fontWeight: 'normal', color: '#333' };
    const {
      isInventoryEditing,
      isProprietaryEditing,
      isOpenMarketEditing,
      impression,
      budget,
      margin,
      rateInflation,
      impressionInflation,
      editingImpression,
      editingBudget,
      editingMargin,
      editingRateInflation,
      editingImpressionInflation
    } = this.state;
    const { propCpmCNN, propCpmSinclair, propCpmTTNW, propCpmTVB } = this.state;
    const {
      propImpressionsCNN,
      propImpressionsSinclair,
      propImpressionsTTNW,
      propImpressionsTVB,
      editingPropImpressionsCNN,
      editingPropImpressionsSinclair,
      editingPropImpressionsTTNW,
      editingPropImpressionsTVB
    } = this.state;

    const balanceSum =
      editingPropImpressionsCNN +
      editingPropImpressionsSinclair +
      editingPropImpressionsTTNW +
      editingPropImpressionsTVB;
    const isBalanceWarning = balanceSum > 1;
    const CNNActive =
      propImpressionsCNN > 0 ? "tag-label active" : "tag-label inactive";
    const sinclairActive =
      propImpressionsSinclair > 0 ? "tag-label active" : "tag-label inactive";
    const TTNWActive =
      propImpressionsTTNW > 0 ? "tag-label active" : "tag-label inactive";
    const TVBActive =
      propImpressionsTVB > 0 ? "tag-label active" : "tag-label inactive";

    const { openCpmMin, openCpmMax, openUnitCap, openCpmTarget } = this.state;
    const {
      editingOpenCpmMin,
      editingOpenCpmMax,
      editingOpenUnitCap,
      editingOpenCpmTarget
    } = this.state;
    const TargetMinActive =
      openCpmTarget === 1 ? "tag-label active" : "tag-label inactive";
    const TargetAvgActive =
      openCpmTarget === 2 ? "tag-label active" : "tag-label inactive";
    const TargetMaxActive =
      openCpmTarget === 3 ? "tag-label active" : "tag-label inactive";
    const coverage =
      proposalEditForm && proposalEditForm.MarketCoverage
        ? proposalEditForm.MarketCoverage
        : 0;
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
                    <div className="summary-tag">--%</div>
                    <div className="summary-display">--%</div>
                    <div className="summary-label">MARKET COVERAGE</div>
                  </div>
                  <div className="summary-item">
                    <div className="summary-tag">--%</div>
                    <div className="summary-display">$--</div>
                    <div className="summary-label">CPM</div>
                  </div>
                  <div className="summary-item">
                    <div className="summary-tag">--%</div>
                    <div className="summary-display">--</div>
                    <div className="summary-label">IMPRESSIONS (000)</div>
                  </div>
                  <div className="summary-item">
                    <div className="summary-tag">--%</div>
                    <div className="summary-display">$--</div>
                    <div className="summary-label">TOTAL COST</div>
                  </div>
                </div>
              </Col>
            </Row>
          </Modal.Header>

          <Modal.Body className="modalBodyScroll">
            <Panel
              id="pricing_inventory_panel"
              defaultExpanded
              className="panelCard"
            >
              <Panel.Heading>
                <Row>
                  <Col sm={6}>
                    <Panel.Title toggle>
                      <Glyphicon glyph="chevron-up" /> GOAL & ADJUSTMENTS
                    </Panel.Title>
                  </Col>
                </Row>
              </Panel.Heading>
              <Panel.Collapse>
                <Panel.Body>
                  <div className="formEditToggle">
                    {!isReadOnly &&
                      !isInventoryEditing && (
                        <Button
                          onClick={this.toggleInventoryEditing}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="edit" /> Edit
                        </Button>
                      )}
                    {isInventoryEditing && (
                      <div>
                        <Button onClick={this.saveInventory} bsStyle="link">
                          <Glyphicon glyph="save" /> Save
                        </Button>
                        <Button
                          className="cancel"
                          onClick={this.cancelInventory}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="remove" /> Cancel
                        </Button>
                      </div>
                    )}
                  </div>
                  <Row>
                    <Col sm={3}>
                      <form className="formCard">
                        <p>
                          <strong>GOAL</strong>
                        </p>
                        <Row>
                          <Col sm={6}>
                            <FormGroup>
                              <ControlLabel>IMPRESSIONS (000)</ControlLabel>
                              {isInventoryEditing && (
                                <InputNumber
                                  defaultValue={
                                    editingImpression
                                      ? editingImpression / 1000
                                      : null
                                  }
                                  disabled={isReadOnly}
                                  min={0}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  onChange={value => {
                                    this.handleChange(
                                      "editingImpression",
                                      value * 1000
                                    );
                                  }}
                                />
                              )}
                              {!isInventoryEditing && (
                                <FormControl.Static>
                                  {impression
                                    ? numeral(impression / 1000).format(
                                        "0,0.[000]"
                                      )
                                    : "--"}
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={6}>
                            <FormGroup>
                              <ControlLabel>BUDGET</ControlLabel>
                              {isInventoryEditing && (
                                <InputNumber
                                  defaultValue={editingBudget || null}
                                  disabled={isReadOnly}
                                  min={0}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `$ ${value}`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/\$\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange("editingBudget", value);
                                  }}
                                />
                              )}
                              {!isInventoryEditing && (
                                <FormControl.Static>
                                  ${budget
                                    ? numeral(budget).format("0,0.[00]")
                                    : "--"}
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                        </Row>
                      </form>
                    </Col>
                    <Col sm={5}>
                      <form className="formCard">
                        <p>
                          <strong>ADJUSTMENTS</strong>
                        </p>
                        <Row>
                          <Col sm={4}>
                            <FormGroup>
                              <ControlLabel>MARGIN</ControlLabel>
                              {isInventoryEditing && (
                                <InputNumber
                                  defaultValue={editingMargin || null}
                                  disabled={isReadOnly}
                                  min={1}
                                  max={1000}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange("editingMargin", value);
                                  }}
                                />
                              )}
                              {!isInventoryEditing && (
                                <FormControl.Static>
                                  {margin
                                    ? numeral(margin).format("0,0.[00]")
                                    : "--"}%
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={4}>
                            <FormGroup>
                              <ControlLabel>RATE INFLATION</ControlLabel>
                              {isInventoryEditing && (
                                <InputNumber
                                  defaultValue={editingRateInflation || null}
                                  disabled={isReadOnly}
                                  min={1}
                                  max={1000}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingRateInflation",
                                      value
                                    );
                                  }}
                                />
                              )}
                              {!isInventoryEditing && (
                                <FormControl.Static>
                                  {rateInflation
                                    ? numeral(rateInflation).format("0,0.[00]")
                                    : "--"}%
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={4}>
                            <FormGroup>
                              <ControlLabel>IMPRESSIONS LOSS</ControlLabel>
                              {isInventoryEditing && (
                                <InputNumber
                                  defaultValue={
                                    editingImpressionInflation || null
                                  }
                                  disabled={isReadOnly}
                                  min={1}
                                  max={1000}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingImpressionInflation",
                                      value
                                    );
                                  }}
                                />
                              )}
                              {!isInventoryEditing && (
                                <FormControl.Static>
                                  {impressionInflation
                                    ? numeral(impressionInflation).format(
                                        "0,0.[00]"
                                      )
                                    : "--"}%
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                        </Row>
                      </form>
                    </Col>
                  </Row>
                </Panel.Body>
              </Panel.Collapse>
            </Panel>
            <Panel
              id="pricing_proprietary_panel"
              defaultExpanded
              className="panelCard"
            >
              <Panel.Heading>
                <Panel.Title toggle>
                  <Glyphicon glyph="chevron-up" /> PROPRIETARY
                </Panel.Title>
                <Row>
                  <Col sm={1}>
                    <div className="summary-item single">
                      <div className="summary-display">
                        {numeral(balanceSum * 100).format("0,0.[00]")}%
                      </div>
                    </div>
                  </Col>
                  <Col sm={5}>
                    <div style={{ marginTop: "12px" }}>
                      <Label className={CNNActive}>CNN</Label>
                      <Label className={sinclairActive}>SINCLAIR</Label>
                      <Label className={TTNWActive}>TTWN</Label>
                      <Label className={TVBActive}>TVB</Label>
                    </div>
                  </Col>
                  <Col sm={6}>
                    <div className="summary-bar">
                      <div className="summary-item">
                        <div className="summary-display">$--</div>
                        <div className="summary-label">CPM</div>
                      </div>
                      <div className="summary-item">
                        <div className="summary-display">--</div>
                        <div className="summary-label">IMPRESSIONS (000)</div>
                      </div>
                      <div className="summary-item">
                        <div className="summary-display">$--</div>
                        <div className="summary-label">TOTAL COST</div>
                      </div>
                    </div>
                  </Col>
                </Row>
              </Panel.Heading>
              <Panel.Collapse>
                <Panel.Body>
                  <div className="formEditToggle">
                    {!isReadOnly &&
                      !isProprietaryEditing && (
                        <Button
                          onClick={this.toggleProprietaryEditing}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="edit" /> Edit
                        </Button>
                      )}
                    {isProprietaryEditing && (
                      <div>
                        <Button
                          onClick={this.saveProprietary}
                          bsStyle="link"
                          disabled={isBalanceWarning}
                        >
                          <Glyphicon glyph="save" /> Save
                        </Button>
                        <Button
                          className="cancel"
                          onClick={this.cancelProprietary}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="remove" /> Cancel
                        </Button>
                      </div>
                    )}
                  </div>
                  <Row>
                    <Col sm={4}>
                      <Table condensed>
                        <thead>
                          <tr>
                            <th className="cardLabel">SOURCE</th>
                            <th className="cardLabel">BALANCE</th>
                            <th className="cardLabel">CPM</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>CNN</td>
                            <td>
                              {!isProprietaryEditing && (
                                <FormControl.Static>
                                  {propImpressionsCNN
                                    ? numeral(propImpressionsCNN * 100).format(
                                        "0,0.[00]"
                                      )
                                    : "--"}%
                                </FormControl.Static>
                              )}
                              {isProprietaryEditing && (
                                <InputNumber
                                  defaultValue={editingPropImpressionsCNN * 100}
                                  disabled={isReadOnly}
                                  min={0}
                                  max={100}
                                  precision={2}
                                  // style={{ width: '100px' }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingPropImpressionsCNN",
                                      value / 100
                                    );
                                  }}
                                />
                              )}
                            </td>
                            <td>
                              {/*  {!isProprietaryEditing &&
                    <FormControl.Static>${propCpmCNN ? numeral(propCpmCNN).format('0,0.[00]') : '--'}</FormControl.Static>
                    } */}
                              <FormControl.Static>
                                ${propCpmCNN
                                  ? numeral(propCpmCNN).format("0,0.[00]")
                                  : "--"}
                              </FormControl.Static>
                            </td>
                          </tr>
                          <tr>
                            <td>SINCLAIR</td>
                            <td>
                              {!isProprietaryEditing && (
                                <FormControl.Static>
                                  {propImpressionsSinclair
                                    ? numeral(
                                        propImpressionsSinclair * 100
                                      ).format("0,0.[00]")
                                    : "--"}%
                                </FormControl.Static>
                              )}
                              {isProprietaryEditing && (
                                <InputNumber
                                  defaultValue={
                                    editingPropImpressionsSinclair * 100
                                  }
                                  disabled={isReadOnly}
                                  min={0}
                                  max={100}
                                  precision={2}
                                  // style={{ width: '100px' }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingPropImpressionsSinclair",
                                      value / 100
                                    );
                                  }}
                                />
                              )}
                            </td>
                            <td>
                              <FormControl.Static>
                                ${propCpmSinclair
                                  ? numeral(propCpmSinclair).format("0,0.[00]")
                                  : "--"}
                              </FormControl.Static>
                            </td>
                          </tr>
                          <tr>
                            <td>TTNW</td>
                            <td>
                              {!isProprietaryEditing && (
                                <FormControl.Static>
                                  {propImpressionsTTNW
                                    ? numeral(propImpressionsTTNW * 100).format(
                                        "0,0.[00]"
                                      )
                                    : "--"}%
                                </FormControl.Static>
                              )}
                              {isProprietaryEditing && (
                                <InputNumber
                                  defaultValue={
                                    editingPropImpressionsTTNW * 100
                                  }
                                  disabled={isReadOnly}
                                  min={0}
                                  max={100}
                                  precision={2}
                                  // style={{ width: '100px' }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingPropImpressionsTTNW",
                                      value / 100
                                    );
                                  }}
                                />
                              )}
                            </td>
                            <td>
                              <FormControl.Static>
                                ${propCpmTTNW
                                  ? numeral(propCpmTTNW).format("0,0.[00]")
                                  : "--"}
                              </FormControl.Static>
                            </td>
                          </tr>
                          <tr>
                            <td>TVB</td>
                            <td>
                              {!isProprietaryEditing && (
                                <FormControl.Static>
                                  {propImpressionsTVB
                                    ? numeral(propImpressionsTVB * 100).format(
                                        "0,0.[00]"
                                      )
                                    : "--"}%
                                </FormControl.Static>
                              )}
                              {isProprietaryEditing && (
                                <InputNumber
                                  defaultValue={editingPropImpressionsTVB * 100}
                                  disabled={isReadOnly}
                                  min={0}
                                  max={100}
                                  precision={2}
                                  // style={{ width: '100px' }}
                                  formatter={value =>
                                    `${value}%`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/%\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingPropImpressionsTVB",
                                      value / 100
                                    );
                                  }}
                                />
                              )}
                            </td>
                            <td>
                              <FormControl.Static>
                                ${propCpmTVB
                                  ? numeral(propCpmTVB).format("0,0.[00]")
                                  : "--"}
                              </FormControl.Static>
                            </td>
                          </tr>
                          <tr>
                            <td>
                              <strong>TOTALS</strong>
                            </td>
                            <td>
                              <strong>
                                {numeral(balanceSum * 100).format("0,0.[00]")}%
                              </strong>
                            </td>
                            <td>
                              <strong>&nbsp;</strong>
                            </td>
                          </tr>
                        </tbody>
                      </Table>
                      {isBalanceWarning && (
                        <div style={{ color: "red", textAlign: "center" }}>
                          <Glyphicon glyph="alert" /> Balance Entries Over 100%
                        </div>
                      )}
                    </Col>
                  </Row>
                </Panel.Body>
              </Panel.Collapse>
            </Panel>

            <Panel
              id="pricing_openmarket_panel"
              defaultExpanded
              className="panelCard"
            >
              <Panel.Heading>
                <Panel.Title toggle>
                  <Glyphicon glyph="chevron-up" /> OPEN MARKETS
                </Panel.Title>
                <Row>
                  <Col sm={6}>
                    <div className="summary-item single">
                      <div className="summary-display">
                        {numeral((1 - balanceSum) * 100).format("0,0.[00]")}%
                      </div>
                    </div>
                  </Col>
                  <Col sm={6}>
                    <div className="summary-bar">
                      <div className="summary-item">
                        <div className="summary-display">
                          {numberRender(
                            activeOpenMarketData,
                            "OpenMarketTotals.Coverage",
                            "0.000"
                          )}%
                        </div>
                        <div className="summary-label">MARKET COVERAGE</div>
                      </div>
                      <div className="summary-item">
                        <div className="summary-display">
                          ${numberRender(
                            activeOpenMarketData,
                            "OpenMarketTotals.Cpm",
                            "0,0.00"
                          )}
                        </div>
                        <div className="summary-label">CPM</div>
                      </div>
                      <div className="summary-item">
                        <div className="summary-display">
                          {numberRender(
                            activeOpenMarketData,
                            "OpenMarketTotals.Impressions",
                            "0,0",
                            1000
                          )}
                        </div>
                        <div className="summary-label">IMPRESSIONS (000)</div>
                      </div>
                      <div className="summary-item">
                        <div className="summary-display">
                          ${numberRender(
                            activeOpenMarketData,
                            "OpenMarketTotals.Cost",
                            "0,0"
                          )}
                        </div>
                        <div className="summary-label">TOTAL COST</div>
                      </div>
                    </div>
                  </Col>
                </Row>
              </Panel.Heading>
              <Panel.Collapse>
                <Panel.Body>
                  <div className="formEditToggle">
                    {!isReadOnly &&
                      !isOpenMarketEditing && (
                        <Button
                          onClick={this.toggleOpenMarketEditing}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="edit" /> Edit
                        </Button>
                      )}
                    {isOpenMarketEditing && (
                      <div>
                        <Button onClick={this.saveOpenMarket} bsStyle="link">
                          <Glyphicon glyph="save" /> Save
                        </Button>
                        <Button
                          className="cancel"
                          onClick={this.cancelOpenMarket}
                          bsStyle="link"
                        >
                          <Glyphicon glyph="remove" /> Cancel
                        </Button>
                      </div>
                    )}
                  </div>
                  <Row>
                    <Col sm={8}>
                      <form className="formCard">
                        <Row>
                          <Col sm={2}>
                            <FormGroup>
                              <ControlLabel>CPM MIN</ControlLabel>
                              {isOpenMarketEditing && (
                                <InputNumber
                                  defaultValue={editingOpenCpmMin || null}
                                  disabled={isReadOnly}
                                  min={0}
                                  max={1000}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `$ ${value}`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/\$\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingOpenCpmMin",
                                      value
                                    );
                                  }}
                                />
                              )}
                              {!isOpenMarketEditing && (
                                <FormControl.Static>
                                  ${openCpmMin
                                    ? numeral(openCpmMin).format("0,0.[00]")
                                    : "--"}
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={2}>
                            <FormGroup>
                              <ControlLabel>CPM MAX</ControlLabel>
                              {isOpenMarketEditing && (
                                <InputNumber
                                  defaultValue={editingOpenCpmMax || null}
                                  disabled={isReadOnly}
                                  min={0}
                                  max={1000}
                                  precision={2}
                                  style={{ width: "100%" }}
                                  formatter={value =>
                                    `$ ${value}`.replace(
                                      /\B(?=(\d{3})+(?!\d))/g,
                                      ","
                                    )
                                  }
                                  parser={value =>
                                    value.replace(/\$\s?|(,*)/g, "")
                                  }
                                  onChange={value => {
                                    this.handleChange(
                                      "editingOpenCpmMax",
                                      value
                                    );
                                  }}
                                />
                              )}
                              {!isOpenMarketEditing && (
                                <FormControl.Static>
                                  ${openCpmMax
                                    ? numeral(openCpmMax).format("0,0.[00]")
                                    : "--"}
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={2}>
                            <FormGroup>
                              <ControlLabel>STATION UNIT CAP</ControlLabel>
                              {isOpenMarketEditing && (
                                <InputNumber
                                  defaultValue={editingOpenUnitCap || null}
                                  disabled={isReadOnly}
                                  min={0}
                                  max={1000}
                                  precision={0}
                                  style={{ width: "100%" }}
                                  onChange={value => {
                                    this.handleChange(
                                      "editingOpenUnitCap",
                                      value
                                    );
                                  }}
                                />
                              )}
                              {!isOpenMarketEditing && (
                                <FormControl.Static>
                                  {openUnitCap
                                    ? numeral(openUnitCap).format("0,0.[000]")
                                    : "--"}
                                </FormControl.Static>
                              )}
                            </FormGroup>
                          </Col>
                          <Col sm={4}>
                            <FormGroup>
                              <ControlLabel>CPM TARGET</ControlLabel>
                              {isOpenMarketEditing && (
                                <div>
                                  <ToggleButtonGroup
                                    type="radio"
                                    value={editingOpenCpmTarget}
                                    name="editingOpenCpmTarget"
                                    onChange={this.handleCpmTargetChange}
                                  >
                                    <ToggleButton value={1}>MIN</ToggleButton>
                                    <ToggleButton value={2}>AVG</ToggleButton>
                                    <ToggleButton value={3}>MAX</ToggleButton>
                                  </ToggleButtonGroup>
                                </div>
                              )}
                              {!isOpenMarketEditing && (
                                <div style={{ marginTop: "6px" }}>
                                  <Label className={TargetMinActive}>MIN</Label>
                                  <Label className={TargetAvgActive}>AVG</Label>
                                  <Label className={TargetMaxActive}>MAX</Label>
                                </div>
                              )}
                            </FormGroup>
                          </Col>
                        </Row>
                      </form>
                    </Col>
                    <Col sm={4}>
                      <div style={{ textAlign: "right", marginTop: "20px" }}>
                        <Button
                          bsStyle="primary"
                          onClick={this.onRunDistribution}
                          disabled={isEditMarketsActive}
                        >
                          Run Distribution
                        </Button>
                      </div>
                    </Col>
                  </Row>
                  {openMarketLoaded &&
                    activeOpenMarketData &&
                    !isEditMarketsActive && (
                      <PricingGuideGrid
                        activeOpenMarketData={activeOpenMarketData}
                        openMarketLoading={openMarketLoading}
                        hasOpenMarketData={hasOpenMarketData}
                        isOpenMarketDataSortName={isOpenMarketDataSortName}
                        allocateSpots={allocateSpots}
                      />
                    )}
                  {openMarketLoaded &&
                    activeOpenMarketData &&
                    isEditMarketsActive && (
                      <PricingGuideEditMarkets
                        activeEditMarkets={activeEditMarkets}
                        marketCoverageGoal={coverage}
                        onUpdateEditMarkets={this.onUpdateEditMarkets}
                      />
                    )}
                </Panel.Body>
              </Panel.Collapse>
            </Panel>
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">
              Cancel
            </Button>
            <Button
              disabled={isReadOnly}
              onClick={this.onSave}
              bsStyle="success"
            >
              OK
            </Button>
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
  updateDetail: PropTypes.func.isRequired,
  clearOpenMarketData: PropTypes.func.isRequired,
  loadOpenMarketData: PropTypes.func.isRequired,
  detail: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  activeOpenMarketData: PropTypes.object,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  openMarketLoading: PropTypes.bool.isRequired,
  allocateSpots: PropTypes.func.isRequired,
  openMarketLoaded: PropTypes.bool.isRequired,
  activeEditMarkets: PropTypes.array.isRequired,
  isEditMarketsActive: PropTypes.bool.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
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
