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
  Table,
  Label
} from "react-bootstrap";
import { InputNumber } from "antd";
import numeral from "numeral";
import { numberRender, invSrcEnum } from "../util";

class PricingProprietary extends Component {
  constructor(props) {
    super(props);

    this.state = {
      isEditing: false,
      editingPropCpmCNN: 0,
      editingPropCpmSinclair: 0,
      editingPropCpmTTNW: 0,
      editingPropCpmTVB: 0,
      editingPropImpressionsCNN: 0,
      editingPropImpressionsSinclair: 0,
      editingPropImpressionsTTNW: 0,
      editingPropImpressionsTVB: 0
    };

    this.toggleEditing = this.toggleEditing.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
  }

  componentDidMount() {
    const { activeOpenMarketData } = this.props;
    if (activeOpenMarketData.ProprietaryPricing) {
      const toUpdate = {};
      activeOpenMarketData.ProprietaryPricing.forEach(item => {
        const bal = item.ImpressionsBalance || 0;
        const cpm = item.Cpm || 0;
        if (bal || cpm) {
          const src = item.InventorySource;
          toUpdate[`editingPropImpressions${invSrcEnum[src]}`] = bal;
          toUpdate[`editingPropCpm${invSrcEnum[src]}`] = cpm;
        }
      });
      this.setState(toUpdate);
    }
  }

  toggleEditing() {
    const { isEditing } = this.state;

    this.setState({ isEditing: !isEditing });
  }

  handleChange(name, value) {
    this.setState({ [name]: value });
  }

  onSave() {
    const { onUpdateProprietaryCpms, submit } = this.props;
    const {
      editingPropImpressionsCNN,
      editingPropImpressionsSinclair,
      editingPropImpressionsTTNW,
      editingPropImpressionsTVB,
      editingPropCpmCNN,
      editingPropCpmTTNW,
      editingPropCpmTVB,
      editingPropCpmSinclair
    } = this.state;
    submit({
      propImpressionsCNN: editingPropImpressionsCNN,
      propImpressionsSinclair: editingPropImpressionsSinclair,
      propImpressionsTTNW: editingPropImpressionsTTNW,
      propImpressionsTVB: editingPropImpressionsTVB,
      propCpmCNN: editingPropCpmCNN,
      propCpmSinclair: editingPropCpmSinclair,
      propCpmTTNW: editingPropCpmTTNW,
      propCpmTVB: editingPropCpmTVB
    });
    onUpdateProprietaryCpms();
    this.toggleEditing();
  }

  onCancel() {
    const {
      propImpressionsCNN,
      propImpressionsSinclair,
      propImpressionsTTNW,
      propImpressionsTVB,
      propCpmCNN,
      propCpmSinclair,
      propCpmTTNW,
      propCpmTVB
    } = this.props;
    this.setState({
      editingPropImpressionsCNN: propImpressionsCNN,
      editingPropImpressionsSinclair: propImpressionsSinclair,
      editingPropImpressionsTTNW: propImpressionsTTNW,
      editingPropImpressionsTVB: propImpressionsTVB,
      editingPropCpmCNN: propCpmCNN,
      editingPropCpmSinclair: propCpmSinclair,
      editingPropCpmTTNW: propCpmTTNW,
      editingPropCpmTVB: propCpmTVB
    });
    this.toggleEditing();
  }

  render() {
    const {
      isReadOnly,
      propImpressionsCNN,
      propImpressionsSinclair,
      propImpressionsTTNW,
      activeOpenMarketData,
      propImpressionsTVB,
      propCpmCNN,
      propCpmSinclair,
      propCpmTTNW,
      propCpmTVB
    } = this.props;
    const {
      isEditing,
      editingPropCpmCNN,
      editingPropCpmSinclair,
      editingPropCpmTTNW,
      editingPropCpmTVB,
      editingPropImpressionsCNN,
      editingPropImpressionsSinclair,
      editingPropImpressionsTTNW,
      editingPropImpressionsTVB
    } = this.state;

    const balanceSum =
      propImpressionsCNN +
      propImpressionsSinclair +
      propImpressionsTTNW +
      propImpressionsTVB;

    const isBalanceWarning = balanceSum > 1;
    const CNNActive =
      propImpressionsCNN > 0 ? "tag-label active" : "tag-label inactive";
    const sinclairActive =
      propImpressionsSinclair > 0 ? "tag-label active" : "tag-label inactive";
    const TTNWActive =
      propImpressionsTTNW > 0 ? "tag-label active" : "tag-label inactive";
    const TVBActive =
      propImpressionsTVB > 0 ? "tag-label active" : "tag-label inactive";

    return (
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
                  <Button onClick={this.toggleEditing} bsStyle="link">
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
                      <th className="cardLabel">BALANCE</th>
                      <th className="cardLabel">CPM</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>CNN</td>
                      <td>
                        {!isEditing && (
                          <FormControl.Static>
                            {propImpressionsCNN
                              ? numeral(propImpressionsCNN * 100).format(
                                  "0,0.[00]"
                                )
                              : "--"}%
                          </FormControl.Static>
                        )}
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropImpressionsCNN * 100}
                            disabled={isReadOnly}
                            min={0}
                            max={100}
                            precision={2}
                            // style={{ width: '100px' }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropCpmCNN || null}
                            disabled={isReadOnly}
                            min={0}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange("editingPropCpmCNN", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            ${propCpmCNN
                              ? numeral(propCpmCNN).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </td>
                    </tr>
                    <tr>
                      <td>SINCLAIR</td>
                      <td>
                        {!isEditing && (
                          <FormControl.Static>
                            {propImpressionsSinclair
                              ? numeral(propImpressionsSinclair * 100).format(
                                  "0,0.[00]"
                                )
                              : "--"}%
                          </FormControl.Static>
                        )}
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropImpressionsSinclair * 100}
                            disabled={isReadOnly}
                            min={0}
                            max={100}
                            precision={2}
                            // style={{ width: '100px' }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropCpmSinclair || null}
                            disabled={isReadOnly}
                            min={0}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange(
                                "editingPropCpmSinclair",
                                value
                              );
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            ${propCpmSinclair
                              ? numeral(propCpmSinclair).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </td>
                    </tr>
                    <tr>
                      <td>TTNW</td>
                      <td>
                        {!isEditing && (
                          <FormControl.Static>
                            {propImpressionsTTNW
                              ? numeral(propImpressionsTTNW * 100).format(
                                  "0,0.[00]"
                                )
                              : "--"}%
                          </FormControl.Static>
                        )}
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropImpressionsTTNW * 100}
                            disabled={isReadOnly}
                            min={0}
                            max={100}
                            precision={2}
                            // style={{ width: '100px' }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropCpmTTNW || null}
                            disabled={isReadOnly}
                            min={0}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange("editingPropCpmTTNW", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            ${propCpmTTNW
                              ? numeral(propCpmTTNW).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
                      </td>
                    </tr>
                    <tr>
                      <td>TVB</td>
                      <td>
                        {!isEditing && (
                          <FormControl.Static>
                            {propImpressionsTVB
                              ? numeral(propImpressionsTVB * 100).format(
                                  "0,0.[00]"
                                )
                              : "--"}%
                          </FormControl.Static>
                        )}
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropImpressionsTVB * 100}
                            disabled={isReadOnly}
                            min={0}
                            max={100}
                            precision={2}
                            // style={{ width: '100px' }}
                            formatter={value =>
                              `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/%\s?|(,*)/g, "")}
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
                        {isEditing && (
                          <InputNumber
                            defaultValue={editingPropCpmTVB || null}
                            disabled={isReadOnly}
                            min={0}
                            precision={2}
                            style={{ width: "100%" }}
                            formatter={value =>
                              `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={value => value.replace(/\$\s?|(,*)/g, "")}
                            onChange={value => {
                              this.handleChange("editingPropCpmTVB", value);
                            }}
                          />
                        )}
                        {!isEditing && (
                          <FormControl.Static>
                            ${propCpmTVB
                              ? numeral(propCpmTVB).format("0,0.[00]")
                              : "--"}
                          </FormControl.Static>
                        )}
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
    );
  }
}

PricingProprietary.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  submit: PropTypes.func.isRequired,
  onUpdateProprietaryCpms: PropTypes.func.isRequired,
  activeOpenMarketData: PropTypes.object.isRequired,
  propCpmCNN: PropTypes.number,
  propCpmSinclair: PropTypes.number,
  propCpmTTNW: PropTypes.number,
  propCpmTVB: PropTypes.number,
  propImpressionsCNN: PropTypes.number,
  propImpressionsSinclair: PropTypes.number,
  propImpressionsTTNW: PropTypes.number,
  propImpressionsTVB: PropTypes.number
};

PricingProprietary.defaultProps = {
  propCpmCNN: 0,
  propCpmSinclair: 0,
  propCpmTTNW: 0,
  propCpmTVB: 0,
  propImpressionsCNN: 0,
  propImpressionsSinclair: 0,
  propImpressionsTTNW: 0,
  propImpressionsTVB: 0,
  // open market
  openCpmMin: null,
  openCpmMax: null,
  openUnitCap: null,
  openCpmTarget: 1
};

export default PricingProprietary;
