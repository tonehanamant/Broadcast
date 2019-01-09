/* eslint-disable react/no-did-mount-set-state */
import React, { Component } from "react";
import PropTypes from "prop-types";
import { Button, Panel, Glyphicon, Row, Col, Table } from "react-bootstrap";
import numeral from "numeral";
import { numberRender, calculateBalanceSum } from "../util";
import { transformInvetorySrc, transformInvetoryLabel } from "./util";

class PricingProprietary extends Component {
  constructor(props) {
    super(props);

    this.state = {
      isEditing: false,
      values: {}
    };

    this.toggleEditing = this.toggleEditing.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
  }

  componentDidMount() {
    const {
      activeOpenMarketData: { ProprietaryPricing },
      initialdata
    } = this.props;
    const { ProprietaryPricingInventorySources: invSrcEnum } = initialdata;
    const toUpdate = {};
    invSrcEnum.forEach(({ Id }) => {
      const { ImpressionsBalance: bal = 0, Cpm: cpm = 0 } =
        (ProprietaryPricing || []).find(it => Id === it.InventorySource) || {};
      toUpdate[`editingPropImpressions${Id}`] = bal;
      toUpdate[`editingPropCpm${Id}`] = cpm;
    });
    this.setState({ values: toUpdate });
  }

  toggleEditing() {
    const { isEditing } = this.state;

    this.setState({ isEditing: !isEditing });
    this.props.onSetGuideEditing(!isEditing);
  }

  handleChange(name, value) {
    const { values } = this.state;
    this.setState({ values: { ...values, [name]: value } });
  }

  onSave() {
    const {
      onUpdateProprietaryCpms,
      submit,
      initialdata: { ProprietaryPricingInventorySources: invSrcEnum }
    } = this.props;
    const { values } = this.state;
    const newValues = {};
    invSrcEnum.forEach(({ Display, Id }) => {
      newValues[`propImpressions${Display}`] =
        values[`editingPropImpressions${Id}`];
      newValues[`propCpm${Display}`] = values[`editingPropCpm${Id}`];
    });

    submit(newValues, onUpdateProprietaryCpms);
    this.toggleEditing();
  }

  onCancel() {
    const {
      initialdata: { ProprietaryPricingInventorySources: invSrcEnum }
    } = this.props;
    const oldValues = {};
    invSrcEnum.forEach(({ Display, Id }) => {
      oldValues[`editingPropImpressions${Id}`] = this.props[
        `propImpressions${Display}`
      ];
      oldValues[`editingPropCpm${Id}`] = this.props[`propCpm${Display}`];
    });
    this.setState({ values: oldValues });
    this.toggleEditing();
  }

  render() {
    const {
      isReadOnly,
      activeOpenMarketData,
      initialdata,
      isEditMarketsActive,
      isGuideEditing
    } = this.props;
    const { isEditing, values } = this.state;

    const balanceSum = calculateBalanceSum(
      initialdata.ProprietaryPricingInventorySources,
      this.props
    );

    const isBalanceWarning = balanceSum > 1;

    return (
      <Panel
        id="pricing_proprietary_panel"
        defaultExpanded
        className="panelCard"
      >
        <Panel.Heading>
          <Panel.Title toggle>
            <Glyphicon glyph="chevron-up" /> PRE OWNED INVENTORY
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
                {transformInvetoryLabel(
                  initialdata.ProprietaryPricingInventorySources,
                  this.props
                )}
              </div>
            </Col>
            <Col sm={6}>
              <div className="summary-bar">
                <div className="summary-item">
                  <div className="summary-display">
                    ${numberRender(
                      activeOpenMarketData,
                      "ProprietaryTotals.Cpm",
                      "0,0.00"
                    )}
                  </div>
                  <div className="summary-label">CPM</div>
                </div>
                <div className="summary-item">
                  <div className="summary-display">
                    {numberRender(
                      activeOpenMarketData,
                      "ProprietaryTotals.Impressions",
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
                      "ProprietaryTotals.Cost",
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
                !isEditing && (
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
                  <Button
                    onClick={this.onSave}
                    bsStyle="link"
                    disabled={isBalanceWarning}
                  >
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
              <Col sm={4}>
                <Table condensed>
                  <thead>
                    <tr>
                      <th className="cardLabel">SOURCE</th>
                      <th className="cardLabel">BALANCE (%)</th>
                      <th className="cardLabel">CPM ($)</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transformInvetorySrc(
                      initialdata.ProprietaryPricingInventorySources,
                      this.props,
                      values,
                      this.handleChange,
                      isEditing
                    )}
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
    );
  }
}

PricingProprietary.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  isEditMarketsActive: PropTypes.bool.isRequired,
  isGuideEditing: PropTypes.bool.isRequired,
  submit: PropTypes.func.isRequired,
  onUpdateProprietaryCpms: PropTypes.func.isRequired,
  activeOpenMarketData: PropTypes.object,
  initialdata: PropTypes.object.isRequired,
  onSetGuideEditing: PropTypes.func.isRequired
};

PricingProprietary.defaultProps = {
  activeOpenMarketData: {}
};

export default PricingProprietary;
