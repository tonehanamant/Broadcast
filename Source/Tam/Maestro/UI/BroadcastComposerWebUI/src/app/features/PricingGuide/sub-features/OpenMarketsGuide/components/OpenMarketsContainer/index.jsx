/* eslint-disable react/no-did-mount-set-state */
import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  Panel,
  Label,
  FormControl,
  Glyphicon,
  Row,
  Col,
  FormGroup,
  ControlLabel,
  ToggleButtonGroup,
  Button,
  ToggleButton
} from "react-bootstrap";
import CSSModules from "react-css-modules";
import { InputNumber } from "antd";
import numeral from "numeral";
import { numberRender, calculateBalanceSum } from "PricingGuide/util/helpers";
import PricingGuideGrid from "../OpenMarketsGrid";
import PricingGuideEditMarkets from "../OpenMarketsEdit";

import styles from "./index.style.scss";

const shouldMarketRender = (isLoaded, data, isEdit) =>
  isLoaded && data && isEdit;

const shouldGridRender = (isLoaded, data, isEdit) =>
  shouldMarketRender(isLoaded, data, !isEdit) &&
  data.Markets &&
  data.Markets.length > 0;

const defaultSort = [{ id: "MarketRank", desc: false }];
const FIRST_ROW_INDEX = 0;

export const getFirstMarket = markets =>
  markets && markets.length > 0
    ? [FIRST_ROW_INDEX, markets[FIRST_ROW_INDEX]]
    : undefined;

class OpenMarketsContainer extends Component {
  constructor(props) {
    super(props);

    this.state = {
      isEditing: false,
      editingOpenCpmMin: null,
      editingOpenCpmMax: null,
      editingOpenUnitCap: null,
      sorted: defaultSort,
      selectedMarket: {},
      editingOpenCpmTarget: 1
    };

    this.toggleEditing = this.toggleEditing.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.onRunDistribution = this.onRunDistribution.bind(this);
    this.onSortedChange = this.onSortedChange.bind(this);
    this.onSelectMarket = this.onSelectMarket.bind(this);
    this.resetTable = this.resetTable.bind(this);
    this.onUpdateEditMarkets = this.onUpdateEditMarkets.bind(this);
  }

  componentDidMount() {
    const { activeOpenMarketData } = this.props;
    if (activeOpenMarketData.OpenMarketPricing) {
      const openData = activeOpenMarketData.OpenMarketPricing;
      const target = openData.OpenMarketCpmTarget || 1;
      this.setState({
        editingOpenCpmMax: openData.CpmMax,
        editingOpenCpmMin: openData.CpmMin,
        editingOpenCpmTarget: target,
        editingOpenUnitCap: openData.UnitCapPerStation
      });
    }
  }

  onSelectMarket(rowIndex, row) {
    this.setState({
      selectedMarket: {
        marketId: row && row.MarketId,
        rowIndex
      }
    });
  }

  onRunDistribution() {
    const { onRunDistribution } = this.props;
    onRunDistribution();
    this.resetTable();
  }

  onUpdateEditMarkets() {
    const { onUpdateEditMarkets } = this.props;
    onUpdateEditMarkets();
  }

  onSortedChange(nextValue) {
    this.setState({ sorted: nextValue });
  }

  onSubmit() {
    const { submit } = this.props;
    const {
      editingOpenCpmMax,
      editingOpenCpmMin,
      editingOpenCpmTarget,
      editingOpenUnitCap
    } = this.state;
    submit({
      openCpmMax: editingOpenCpmMax,
      openCpmMin: editingOpenCpmMin,
      openCpmTarget: editingOpenCpmTarget,
      openUnitCap: editingOpenUnitCap
    });
    this.toggleEditing();
  }

  onCancel() {
    const { openCpmMin, openCpmMax, openCpmTarget, openUnitCap } = this.props;
    this.setState({
      editingOpenCpmMin: openCpmMin,
      editingOpenCpmMax: openCpmMax,
      editingOpenCpmTarget: openCpmTarget,
      editingOpenUnitCap: openUnitCap
    });
    this.toggleEditing();
  }

  handleChange(name, value) {
    this.setState({ [name]: value });
  }

  resetTable() {
    const {
      activeOpenMarketData: { Markets }
    } = this.props;
    this.onSortedChange(defaultSort);
    const firstMarket = getFirstMarket(Markets);
    if (firstMarket) {
      this.onSelectMarket(...firstMarket);
    }
  }

  toggleEditing() {
    const { isEditing } = this.state;
    const { onSetGuideEditing } = this.props;

    this.setState({ isEditing: !isEditing });
    onSetGuideEditing(!isEditing);
  }

  render() {
    const {
      isEditing,
      editingOpenCpmMin,
      editingOpenCpmMax,
      editingOpenUnitCap,
      editingOpenCpmTarget,
      sorted,
      selectedMarket
    } = this.state;
    const {
      isReadOnly,
      hasOpenMarketData,
      isOpenMarketDataSortName,
      onAllocateSpots,
      openMarketLoading,
      openMarketLoaded,
      activeEditMarkets,
      isEditMarketsActive,
      isGuideEditing,
      activeOpenMarketData,
      onCopyToBuy,
      openCpmMin,
      openCpmMax,
      openUnitCap,
      openCpmTarget,
      initialdata,
      proposalEditForm
    } = this.props;

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
    const balanceSum = calculateBalanceSum(
      initialdata.ProprietaryPricingInventorySources,
      this.props
    );

    const isRenderGrid = shouldGridRender(
      openMarketLoaded,
      activeOpenMarketData,
      isEditMarketsActive
    );
    const isRenderMarkets = shouldMarketRender(
      openMarketLoaded,
      activeOpenMarketData,
      isEditMarketsActive
    );

    return (
      <Panel
        id="pricing_openmarket_panel"
        defaultExpanded
        styleName="pricing-guide_open-market"
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
                    )}
                    %
                  </div>
                  <div className="summary-label">MARKET COVERAGE</div>
                </div>
                <div className="summary-item">
                  <div className="summary-display">
                    $
                    {numberRender(
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
                    $
                    {numberRender(
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
              {!isReadOnly && !isEditing && (
                <Button
                  onClick={this.toggleEditing}
                  disabled={isEditMarketsActive || isGuideEditing}
                  bsStyle="link"
                >
                  <Glyphicon glyph="edit" /> Edit
                </Button>
              )}
              {isEditing && (
                <div>
                  <Button onClick={this.onSubmit} bsStyle="link">
                    <Glyphicon glyph="save" /> Ok
                  </Button>
                  <Button
                    className="cancel"
                    onClick={this.onCancel}
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingOpenCpmMin || null}
                            disabled={isReadOnly}
                            min={0}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange("editingOpenCpmMin", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            $
                            {openCpmMin
                              ? numeral(openCpmMin).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={2}>
                      <FormGroup>
                        <ControlLabel>CPM MAX</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingOpenCpmMax || null}
                            disabled={isReadOnly}
                            min={0}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange("editingOpenCpmMax", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            $
                            {openCpmMax
                              ? numeral(openCpmMax).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={2}>
                      <FormGroup>
                        <ControlLabel>STATION UNIT CAP</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingOpenUnitCap || null}
                            disabled={isReadOnly}
                            min={0}
                            max={1000}
                            precision={0}
                            style={{ width: "100%" }}
                            onChange={value => {
                              this.handleChange("editingOpenUnitCap", value);
                            }}
                          />
                        )}
                        {!isEditing && (
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
                        {isEditing && (
                          <div>
                            <ToggleButtonGroup
                              type="radio"
                              value={editingOpenCpmTarget}
                              name="editingOpenCpmTarget"
                              onChange={value =>
                                this.handleChange("editingOpenCpmTarget", value)
                              }
                            >
                              <ToggleButton value={1}>MIN</ToggleButton>
                              <ToggleButton value={2}>AVG</ToggleButton>
                              <ToggleButton value={3}>MAX</ToggleButton>
                            </ToggleButtonGroup>
                          </div>
                        )}
                        {!isEditing && (
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
              <Col sm={4} styleName="open-market_toolbox">
                <Button
                  bsStyle="primary"
                  onClick={onCopyToBuy}
                  disabled={isEditMarketsActive || isGuideEditing}
                >
                  Copy to Buy
                </Button>
                <Button
                  bsStyle="primary"
                  onClick={this.onRunDistribution}
                  disabled={isEditMarketsActive || isGuideEditing}
                >
                  Run Distribution
                </Button>
              </Col>
            </Row>
            {isRenderGrid && (
              <PricingGuideGrid
                activeOpenMarketData={activeOpenMarketData}
                openMarketLoading={openMarketLoading}
                hasOpenMarketData={hasOpenMarketData}
                isOpenMarketDataSortName={isOpenMarketDataSortName}
                onAllocateSpots={onAllocateSpots}
                sorted={sorted}
                selectedMarket={selectedMarket}
                onSortedChange={this.onSortedChange}
                onSelectMarket={this.onSelectMarket}
                resetTable={this.resetTable}
                isGuideEditing={isGuideEditing}
              />
            )}
            {isRenderMarkets && (
              <PricingGuideEditMarkets
                activeEditMarkets={activeEditMarkets}
                marketCoverageGoal={coverage}
                onUpdateEditMarkets={this.onUpdateEditMarkets}
                openCpmTarget={openCpmTarget}
              />
            )}
          </Panel.Body>
        </Panel.Collapse>
      </Panel>
    );
  }
}

OpenMarketsContainer.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  submit: PropTypes.func.isRequired,
  onRunDistribution: PropTypes.func.isRequired,
  onUpdateEditMarkets: PropTypes.func.isRequired,
  onCopyToBuy: PropTypes.func.isRequired,
  onAllocateSpots: PropTypes.func.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  openMarketLoading: PropTypes.bool.isRequired,
  openMarketLoaded: PropTypes.bool.isRequired,
  activeEditMarkets: PropTypes.array.isRequired,
  isEditMarketsActive: PropTypes.bool.isRequired,
  isGuideEditing: PropTypes.bool.isRequired,
  activeOpenMarketData: PropTypes.object,
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  openCpmMin: PropTypes.number,
  openCpmMax: PropTypes.number,
  openUnitCap: PropTypes.number,
  openCpmTarget: PropTypes.number,
  onSetGuideEditing: PropTypes.func.isRequired
};

OpenMarketsContainer.defaultProps = {
  activeOpenMarketData: {},
  openCpmMin: null,
  openCpmMax: null,
  openUnitCap: null,
  openCpmTarget: 1
};

export default CSSModules(OpenMarketsContainer, styles);
