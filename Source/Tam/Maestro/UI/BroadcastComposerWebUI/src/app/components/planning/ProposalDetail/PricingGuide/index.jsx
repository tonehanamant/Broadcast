import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Panel, Table, Label, FormControl, Glyphicon, Row, Col, FormGroup, ControlLabel } from 'react-bootstrap';
import { bindActionCreators } from 'redux';
import { InputNumber } from 'antd';
import numeral from 'numeral';

import { toggleModal } from 'Ducks/app';
import { updateProposalEditFormDetail } from 'Ducks/planning';
import './index.scss';

const isActiveDialog = (detail, modal) => (
  modal && detail && modal.properties.detailId === detail.Id && modal.active
);

const mapStateToProps = ({ app: { modals: { pricingGuide: modal } }, planning: { proposalEditForm } }) => ({
  modal,
  proposalEditForm,
});

const mapDispatchToProps = dispatch => (
    bindActionCreators({
      toggleModal,
      updateDetail: updateProposalEditFormDetail,
    }, dispatch)
  );

class PricingGuide extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.clearState = this.clearState.bind(this);

    this.toggleInventoryEditing = this.toggleInventoryEditing.bind(this);
    this.saveInventory = this.saveInventory.bind(this);
    this.cancelInventory = this.cancelInventory.bind(this);

    this.state = {
      // goals/adjustments - editing version separate state to cancel/save individually
      impression: '',
      budget: '',
      margin: '',
      rateInflation: '',
      impressionInflation: '',
      isInventoryEditing: false,
      editingImpression: '',
      editingBudget: '',
      editingMargin: '',
      editingRateInflation: '',
      editingImpressionInflation: '',
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

  componentWillReceiveProps(nextProps) {
    if (nextProps.detail) {
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
        editingImpressionInflation: nextProps.detail.AdjustmentInflation,
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
      impressionInflation: this.state.editingImpressionInflation,
     });
    this.toggleInventoryEditing();
  }

  cancelInventory() {
    this.setState({
      editingImpression: this.state.impression,
      editingBudget: this.state.budget,
      editingMargin: this.state.margin,
      editingRateInflation: this.state.rateInflation,
      editingImpressionInflation: this.state.impressionInflation,
     });
    this.toggleInventoryEditing();
  }

  clearState() {
    this.setState({
      editingImpression: '',
      impression: '',
      editingBudget: '',
      budget: '',
      editingMargin: '',
      margin: '',
      editingRateInflation: '',
      rateInflation: '',
      editingImpressionInflation: '',
      impressionInflation: '',
      isInventoryEditing: false,
    });
  }

  onSave() {
    const { impression, budget, margin, rateInflation, impressionInflation } = this.state;
    const { updateDetail, detail } = this.props;

    updateDetail({ id: detail.Id, key: 'GoalImpression', value: impression });
    updateDetail({ id: detail.Id, key: 'GoalBudget', value: budget });
    updateDetail({ id: detail.Id, key: 'AdjustmentMargin', value: margin });
    updateDetail({ id: detail.Id, key: 'AdjustmentRate', value: rateInflation });
    updateDetail({ id: detail.Id, key: 'AdjustmentInflation', value: impressionInflation });
    this.onCancel();
  }

  handleChange(fieldName, value) {
    const newVal = !isNaN(value) ? value : 0;
    this.setState({ [fieldName]: newVal });
  }

  onCancel() {
    this.props.toggleModal({
      modal: 'pricingGuide',
      active: false,
      properties: { detailId: this.props.detail.Id },
    });
  }

  render() {
    const { modal, detail, isReadOnly } = this.props;
    const show = isActiveDialog(detail, modal);
    // const labelStyle = { fontSize: '11px', fontWeight: 'normal', color: '#333' };
    const isInventoryEditing = this.state.isInventoryEditing;
    const { impression, budget, margin, rateInflation, impressionInflation } = this.state;
    const { editingImpression, editingBudget, editingMargin, editingRateInflation, editingImpressionInflation } = this.state;

    return (
      <div>
        <Modal
          show={show}
          dialogClassName="large-wide-modal"
        >
          <Modal.Header>
            <Button
                className="close"
                bsStyle="link"
                onClick={this.onCancel}
                style={{ display: 'inline-block', float: 'right' }}
            >
                <span>&times;</span>
            </Button>
            <Row>
              <Col sm={6}>
              <Modal.Title>Pricing Guide</Modal.Title>
              </Col>
              <Col sm={6}>
               {/*  <div style={{ fontSize: '40px', fontWeight: 'bold', textAlign: 'right' }}>82% $9.23 805,201 $23,940</div> */}
                <div style={{ border: '1px solid #eee', height: '40px', padding: '8px' }}>Summary Placeholder</div>
              </Col>
            </Row>
          </Modal.Header>

          <Modal.Body className="modalBodyScroll">
          <Panel id="pricing_inventory_panel" defaultExpanded className="panelCard">
            <Panel.Heading>
              <Row>
                <Col sm={6}>
                  <Panel.Title toggle><Glyphicon glyph="chevron-up" /> GOAL & ADJUSTMENTS</Panel.Title>
              </Col>
             </Row>
            </Panel.Heading>
            <Panel.Collapse>
              <Panel.Body>
                <div className="formEditToggle">
                  { !isInventoryEditing &&
                  <Button onClick={this.toggleInventoryEditing} bsStyle="link"><Glyphicon glyph="edit" /> Edit</Button>
                  }
                  { isInventoryEditing &&
                  <div>
                  <Button onClick={this.saveInventory} bsStyle="link"><Glyphicon glyph="save" /> Save</Button>
                  <Button className="cancel" onClick={this.cancelInventory} bsStyle="link"><Glyphicon glyph="remove" /> Cancel</Button>
                  </div>
                  }
                </div>
              <Row>
              <Col sm={3}>
                <form className="formCard">
                  <p><strong>GOAL</strong></p>
                  <Row>
                  <Col sm={6}>
                  <FormGroup>
                  <ControlLabel>IMPRESSIONS (000)</ControlLabel>
                  {isInventoryEditing &&
                    <InputNumber
                      defaultValue={editingImpression ? editingImpression / 1000 : null}
                      disabled={isReadOnly}
                      min={0}
                      precision={2}
                      style={{ width: '100%' }}
                      onChange={(value) => { this.handleChange('editingImpression', value * 1000); }}
                    />
                  }
                  {!isInventoryEditing &&
                    <FormControl.Static>{impression ? numeral(impression / 1000).format('0,0.[000]') : '-'}</FormControl.Static>
                  }
                  </FormGroup>
                  </Col>
                  <Col sm={6}>
                    <FormGroup>
                    <ControlLabel>BUDGET</ControlLabel>
                    {isInventoryEditing &&
                    <InputNumber
                      defaultValue={editingBudget || null}
                      disabled={isReadOnly}
                      min={0}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/\$\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('editingBudget', value); }}
                    />
                    }
                    {!isInventoryEditing &&
                    <FormControl.Static>${budget ? numeral(budget).format('0,0.[00]') : ' -'}</FormControl.Static>
                    }
                  </FormGroup>
                  </Col>
                  </Row>
                </form >
              </Col>
              <Col sm={5}>
                <form className="formCard">
                  <p><strong>ADJUSTMENTS</strong></p>
                  <Row>
                  <Col sm={4}>
                  <FormGroup>
                    <ControlLabel>MARGIN</ControlLabel>
                    {isInventoryEditing &&
                    <InputNumber
                      defaultValue={editingMargin || null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('editingMargin', value); }}
                    />
                    }
                    {!isInventoryEditing &&
                    <FormControl.Static>{margin ? numeral(margin).format('0,0.[00]') : '- '}%</FormControl.Static>
                    }
                  </FormGroup>
                  </Col>
                  <Col sm={4}>
                  <FormGroup>
                    <ControlLabel>RATE INFLATION</ControlLabel>
                    {isInventoryEditing &&
                    <InputNumber
                      defaultValue={editingRateInflation || null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('editingRateInflation', value); }}
                    />
                    }
                    {!isInventoryEditing &&
                    <FormControl.Static>{rateInflation ? numeral(rateInflation).format('0,0.[00]') : '- '}%</FormControl.Static>
                    }
                  </FormGroup>
                  </Col>
                  <Col sm={4}>
                  <FormGroup>
                    <ControlLabel>IMPRESSION INFLATION</ControlLabel>
                    {isInventoryEditing &&
                    <InputNumber
                      defaultValue={editingImpressionInflation || null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('editingImpressionInflation', value); }}
                    />
                    }
                     {!isInventoryEditing &&
                    <FormControl.Static>{impressionInflation ? numeral(impressionInflation).format('0,0.[00]') : '- '}%</FormControl.Static>
                    }
                  </FormGroup>
                  </Col>
                  </Row>
                </form>
              </Col>
            </Row>
              </Panel.Body>
            </Panel.Collapse>
          </Panel>
          DISPLAY ONLY TESTING UX
          <Panel id="pricing_proprietary_panel" defaultExpanded className="panelCard">
            <Panel.Heading>
            <Panel.Title toggle><Glyphicon glyph="chevron-up" /> PROPRIETARY</Panel.Title>
              <Row>
                <Col sm={6}>
                <div><span style={{ fontSize: '40px', fontWeight: 'bold' }}>81% </span>{' '}<Label style={{ backgroundColor: '#ccc' }}>CNN</Label>{' '}<Label>TTWN</Label>{' '}<Label>TTWN</Label></div>
              </Col>
              <Col sm={6}>
                <div style={{ fontSize: '40px', fontWeight: 'bold', textAlign: 'right' }}>$6.89 612,814 $10,925</div>
              </Col>
             </Row>
            </Panel.Heading>
            <Panel.Collapse>
              <Panel.Body>
              <Button style={{ padding: '0px 0px 6px 0px' }} bsStyle="link"><Glyphicon glyph="edit" /> Edit</Button>
              <Row>
                <Col sm={4}>
              <Table condensed>
                <thead>
                  <tr>
                    <th className="cardLabel">SOURCE</th>
                    <th className="cardLabel">IMPRESSIONS</th>
                    <th className="cardLabel">CPM</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>CNN</td>
                    <td>--</td>
                    <td>--</td>
                  </tr>
                  <tr>
                    <td>TTWN</td>
                    <td>50%</td>
                    <td>$5.43</td>
                  </tr>
                  <tr>
                    <td>TVB</td>
                    <td>31%</td>
                    <td>$8.35</td>
                  </tr>
                </tbody>
              </Table>
              </Col>
              </Row>
              </Panel.Body>
            </Panel.Collapse>
          </Panel>

         {/*  <Panel id="pricing_openmarket_panel" defaultExpanded className="panelCard">
            <Panel.Heading>
            <Panel.Title toggle><Glyphicon glyph="chevron-up" /> OPEN MARKEtS</Panel.Title>
            </Panel.Heading>
            <Panel.Collapse>
              <Panel.Body>
              <Button style={{ padding: '0px 0px 6px 0px' }} bsStyle="link"><Glyphicon glyph="edit" /> Edit</Button>
              <Row>
                <Col sm={6}>
                    Form/Display
                </Col>
                <Col sm={6}>
                    Button
                </Col>
              </Row>
                <div> GRID </div>
              </Panel.Body>
            </Panel.Collapse>
          </Panel> */}
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
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
  detail: PropTypes.object.isRequired,
};

PricingGuide.defaultProps = {
  modal: null,
  isReadOnly: false,
};

export default connect(mapStateToProps, mapDispatchToProps)(PricingGuide);
