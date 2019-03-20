/* eslint-disable react/no-did-mount-set-state */
import React, { Component } from "react";
import PropTypes from "prop-types";
import {
  Button,
  Panel,
  FormControl,
  Glyphicon,
  Row,
  Col,
  FormGroup,
  ControlLabel
} from "react-bootstrap";
import { InputNumber } from "antd";
import numeral from "numeral";
import { parseFromPercent, parseToPercent } from "PricingGuide/util/parsers";

const initialState = {
  isEditing: false,
  editingImpression: 0,
  editingBudget: 0,
  editingMargin: 0,
  editingImpressionLoss: 0,
  editingInflation: 0
};

class PricingGuideGoal extends Component {
  constructor(props) {
    super(props);

    this.state = initialState;

    this.toggleEditing = this.toggleEditing.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.onChange = this.onChange.bind(this);
  }

  componentDidMount() {
    const { activeOpenMarketData } = this.props;
    this.setState({
      editingImpression: activeOpenMarketData.ImpressionGoal,
      editingBudget: activeOpenMarketData.BudgetGoal,
      editingMargin: activeOpenMarketData.Margin,
      editingImpressionLoss: activeOpenMarketData.ImpressionLoss,
      editingInflation: activeOpenMarketData.Inflation
    });
  }

  onSubmit() {
    const { submit } = this.props;
    const {
      editingImpression,
      editingBudget,
      editingMargin,
      editingImpressionLoss,
      editingInflation
    } = this.state;
    submit({
      impression: editingImpression,
      budget: editingBudget,
      margin: editingMargin,
      impressionLoss: editingImpressionLoss,
      inflation: editingInflation
    });
    this.toggleEditing();
  }

  onCancel() {
    const {
      impression,
      budget,
      margin,
      impressionLoss,
      inflation
    } = this.props;
    this.setState({
      editingImpression: impression,
      editingBudget: budget,
      editingMargin: margin,
      editingImpressionLoss: impressionLoss,
      editingInflation: inflation
    });
    this.toggleEditing();
  }

  onChange(name, value) {
    this.setState({ [name]: value });
  }

  toggleEditing() {
    const { isEditing } = this.state;
    const { onSetGuideEditing } = this.props;

    this.setState({ isEditing: !isEditing });
    onSetGuideEditing(!isEditing);
  }

  render() {
    const {
      isReadOnly,
      impression,
      budget,
      margin,
      impressionLoss,
      inflation,
      isEditMarketsActive,
      isGuideEditing
    } = this.props;
    const {
      isEditing,
      editingImpression,
      editingBudget,
      editingMargin,
      editingImpressionLoss,
      editingInflation
    } = this.state;

    return (
      <Panel id="pricing_inventory_panel" defaultExpanded styleName="panelCard">
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
            <div styleName="formEditToggle">
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
                    styleName="cancel"
                    onClick={this.onCancel}
                    bsStyle="link"
                  >
                    <Glyphicon glyph="remove" /> Cancel
                  </Button>
                </div>
              )}
            </div>
            <Row>
              <Col sm={3}>
                <form styleName="formCard">
                  <p>
                    <strong>GOAL</strong>
                  </p>
                  <Row>
                    <Col sm={6}>
                      <FormGroup>
                        <ControlLabel>IMPRESSIONS (000)</ControlLabel>
                        {isEditing && (
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
                              this.onChange("editingImpression", value * 1000);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {impression
                              ? numeral(impression / 1000).format("0,0.[000]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={6}>
                      <FormGroup>
                        <ControlLabel>BUDGET ($)</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingBudget || null}
                            disabled={isReadOnly}
                            min={0}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange("editingBudget", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            $
                            {budget ? numeral(budget).format("0,0.[00]") : "--"}
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                  </Row>
                </form>
              </Col>
              <Col sm={5}>
                <form styleName="formCard">
                  <p>
                    <strong>ADJUSTMENTS</strong>
                  </p>
                  <Row>
                    <Col sm={4}>
                      <FormGroup>
                        <ControlLabel>MARGIN (%)</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={parseToPercent(editingMargin || null)}
                            disabled={isReadOnly}
                            min={1}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `% ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange(
                                "editingMargin",
                                parseFromPercent(value)
                              );
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {margin
                              ? numeral(parseToPercent(margin)).format(
                                  "0,0.[00]"
                                )
                              : "--"}
                            %
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={4}>
                      <FormGroup>
                        <ControlLabel>RATE INFLATION (%)</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={parseToPercent(
                              editingInflation || null
                            )}
                            disabled={isReadOnly}
                            min={1}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `% ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange(
                                "editingInflation",
                                parseFromPercent(value)
                              );
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {inflation
                              ? numeral(parseToPercent(inflation)).format(
                                  "0,0.[00]"
                                )
                              : "--"}
                            %
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={4}>
                      <FormGroup>
                        <ControlLabel>IMPRESSIONS LOSS (%)</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={parseToPercent(
                              editingImpressionLoss || null
                            )}
                            disabled={isReadOnly}
                            min={1}
                            max={99.99}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `% ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange(
                                "editingImpressionLoss",
                                parseFromPercent(value)
                              );
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {impressionLoss
                              ? numeral(parseToPercent(impressionLoss)).format(
                                  "0,0.[00]"
                                )
                              : "--"}
                            %
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
    );
  }
}

PricingGuideGoal.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  isEditMarketsActive: PropTypes.bool.isRequired,
  isGuideEditing: PropTypes.bool.isRequired,
  impression: PropTypes.number,
  budget: PropTypes.number,
  margin: PropTypes.number,
  impressionLoss: PropTypes.number,
  inflation: PropTypes.number,
  activeOpenMarketData: PropTypes.object,
  submit: PropTypes.func.isRequired,
  onSetGuideEditing: PropTypes.func.isRequired
};
PricingGuideGoal.defaultProps = {
  activeOpenMarketData: {},
  impression: 0,
  budget: 0,
  margin: 0,
  impressionLoss: 0,
  inflation: 0
};

export default PricingGuideGoal;
