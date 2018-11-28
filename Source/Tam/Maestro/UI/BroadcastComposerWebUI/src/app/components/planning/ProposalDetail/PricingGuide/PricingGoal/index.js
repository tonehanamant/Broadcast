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

class PricingGoal extends Component {
  constructor(props) {
    super(props);

    this.state = {
      isEditing: false,
      editingImpression: "",
      editingBudget: "",
      editingMargin: "",
      editingRateInflation: "",
      editingImpressionInflation: ""
    };

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
      editingMargin: activeOpenMarketData.AdjustmentMargin,
      editingRateInflation: activeOpenMarketData.AdjustmentRate,
      editingImpressionInflation: activeOpenMarketData.AdjustmentInflation
    });
  }

  toggleEditing() {
    const { isEditing } = this.state;

    this.setState({ isEditing: !isEditing });
  }

  onSubmit() {
    const { submit } = this.props;
    const {
      editingImpression,
      editingBudget,
      editingMargin,
      editingRateInflation,
      editingImpressionInflation
    } = this.state;
    submit({
      impression: editingImpression,
      budget: editingBudget,
      margin: editingMargin,
      rateInflation: editingRateInflation,
      impressionInflation: editingImpressionInflation
    });
    this.toggleEditing();
  }

  onCancel() {
    const {
      impression,
      budget,
      margin,
      rateInflation,
      impressionInflation
    } = this.state;
    this.setState({
      editingImpression: impression,
      editingBudget: budget,
      editingMargin: margin,
      editingRateInflation: rateInflation,
      editingImpressionInflation: impressionInflation
    });
    this.toggleEditing();
  }

  onChange(name, value) {
    this.setState({ [name]: value });
  }

  render() {
    const {
      isReadOnly,
      impression,
      budget,
      margin,
      rateInflation,
      impressionInflation
    } = this.props;
    const {
      isEditing,
      editingImpression,
      editingBudget,
      editingMargin,
      editingRateInflation,
      editingImpressionInflation
    } = this.state;

    return (
      <Panel id="pricing_inventory_panel" defaultExpanded className="panelCard">
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
                !isEditing && (
                  <Button onClick={this.toggleEditing} bsStyle="link">
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
              <Col sm={3}>
                <form className="formCard">
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
                        <ControlLabel>BUDGET</ControlLabel>
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingMargin || null}
                            disabled={isReadOnly}
                            min={1}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange("editingMargin", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {margin ? numeral(margin).format("0,0.[00]") : "--"}%
                          </FormControl.Static>
                        )}
                      </FormGroup>
                    </Col>
                    <Col sm={4}>
                      <FormGroup>
                        <ControlLabel>RATE INFLATION</ControlLabel>
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingRateInflation || null}
                            disabled={isReadOnly}
                            min={1}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange("editingRateInflation", value);
                            }}
                          />
                        )}
                        {!isEditing && (
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingImpressionInflation || null}
                            disabled={isReadOnly}
                            min={1}
                            max={1000}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
                            onChange={value => {
                              this.onChange(
                                "editingImpressionInflation",
                                value
                              );
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            {impressionInflation
                              ? numeral(impressionInflation).format("0,0.[00]")
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
    );
  }
}

PricingGoal.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  impression: PropTypes.string,
  budget: PropTypes.string,
  margin: PropTypes.string,
  rateInflation: PropTypes.string,
  impressionInflation: PropTypes.string,
  activeOpenMarketData: PropTypes.object.isRequired,
  submit: PropTypes.func.isRequired
};
PricingGoal.defaultProps = {
  impression: "",
  budget: "",
  margin: "",
  rateInflation: "",
  impressionInflation: ""
};

export default PricingGoal;
