import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Row, Col } from 'react-bootstrap';
import { bindActionCreators } from 'redux';
import { Form, InputNumber } from 'antd';

import { toggleModal } from 'Ducks/app';
import { updateProposalEditFormDetail } from 'Ducks/planning';

const FormItem = Form.Item;

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

    this.state = {
      impression: '',
      budget: '',
      margin: '',
      rateInflation: '',
      impressionInflation: '',
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
      });
    }
  }

  clearState() {
    this.setState({
      impression: '',
      budget: '',
      margin: '',
      rateInflation: '',
      impressionInflation: '',
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
    this.setState({ [fieldName]: value });
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

    return (
      <div>
        <Modal
          show={show}
          bsSize="large"
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
            <Modal.Title>Pricing Guide</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Row>
              <Col sm={6}>
              <Form layout="vertical">
                  <p>Goal:</p>
                  <FormItem label="Impression (000)" style={{ marginBottom: 0 }}>
                    <InputNumber
                      defaultValue={detail ? detail.GoalImpression / 1000 : null}
                      disabled={isReadOnly}
                      min={0}
                      precision={2}
                      style={{ width: '100%' }}
                      onChange={(value) => { this.handleChange('impression', value * 1000); }}
                    />
                  </FormItem>
                  <FormItem label="Budget" style={{ marginBottom: 0 }}>
                    <InputNumber
                      defaultValue={detail ? detail.GoalBudget : null}
                      disabled={isReadOnly}
                      min={0}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/\$\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('budget', value); }}
                    />
                  </FormItem>
                </Form>
              </Col>
              <Col sm={6}>
                <Form layout="vertical">
                  <p>Adjustments:</p>
                  <FormItem label="Margin" style={{ marginBottom: 0 }}>
                    <InputNumber
                      defaultValue={detail ? detail.AdjustmentMargin : null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('margin', value); }}
                    />
                  </FormItem>
                  <FormItem label="Rate Inflation" style={{ marginBottom: 0 }}>
                    <InputNumber
                      defaultValue={detail ? detail.AdjustmentRate : null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('rateInflation', value); }}
                    />
                  </FormItem>
                  <FormItem label="Impression Inflation" style={{ marginBottom: 0 }}>
                    <InputNumber
                      defaultValue={detail ? detail.AdjustmentInflation : null}
                      disabled={isReadOnly}
                      min={1}
                      max={1000}
                      precision={2}
                      style={{ width: '100%' }}
                      formatter={value => `${value}%`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                      parser={value => value.replace(/%\s?|(,*)/g, '')}
                      onChange={(value) => { this.handleChange('impressionInflation', value); }}
                    />
                  </FormItem>

                </Form>
              </Col>
            </Row>
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
