import React, { Component } from 'react';
import PropTypes from 'prop-types';

import Select from 'react-select';
import { Well, Form, FormGroup, ControlLabel, Row, Col, FormControl, Button, Checkbox, Glyphicon } from 'react-bootstrap';

import FlightPicker from 'Components/shared/FlightPicker';


export default class ProposalDetail extends Component {
  constructor(props) {
    super(props);
    console.log('PROPOSAL DETAIL PROPS', this.props);
    this.state = {
      activeDetail: true, // temp use prop
      spotLengthInvalid: null,
      datpartInvalid: null,
      daypartCodeInvalid: null,
    };
    this.onChangeSpotLength = this.onChangeSpotLength.bind(this);
    this.onChangeDaypartCode = this.onChangeDaypartCode.bind(this);
    this.onChangeAdu = this.onChangeAdu.bind(this);
    this.onDeleteProposalDetail = this.onDeleteProposalDetail.bind(this);

    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);
  }

  onChangeSpotLength(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'SpotLengthId', value: val });
    this.setValidationState('spotLengthInvalid', val ? null : 'error');
    // console.log('onChangeSpotLength', value, this.props.detail);
  }

  // onChangeDaypart - TODO with validation

  onChangeDaypartCode(event) {
    const re = /^[a-z0-9]+$/i; // check alphanumeric
    const val = event.target.value ? event.target.value : '';
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'DaypartCode', value: val });
    this.setValidationState('daypartCodeInvalid', re.test(val) ? null : 'error');
    // console.log('onChangeDaypartCode', event.target.value, this.props.detail);
  }

  onChangeAdu(event) {
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'Adu', value: event.target.checked });
    console.log('onChangeAdu', event.target.value, event.target.checked, this.props.detail);
  }

  onDeleteProposalDetail() {
    this.props.toggleModal({
      modal: 'confirmModal',
      active: true,
      properties: {
        titleText: 'Delete Proposal Detail',
        bodyText: 'Are you sure you wish to Delete the proposal detail?',
        closeButtonText: 'Cancel',
        actionButtonText: 'Continue',
        actionButtonBsStyle: 'danger',
        action: () => this.props.deleteProposalDetail({ id: this.props.detail.Id }),
      },
    });
  }

  setValidationState(type, state) {
    this.state[type] = state;
  }

  clearValidationStates() {
    this.setState({
      spotLengthInvalid: null,
      datpartInvalid: null,
      daypartCodeInvalid: null,
    });
  }

  checkValid() {
    const spotValid = this.props.detail.SpotLengthId != null;
    const daypartValid = this.props.detail.Daypart != null;
    const daypartcodeValid = (this.props.detail.DaypartCode != null) && (this.props.detail.DaypartCode !== ''); // presumably alphanumeric already tested
    if (spotValid && daypartValid && daypartcodeValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState('spotLengthInvalid', spotValid ? null : 'error');
    this.setValidationState('daypartInvalid', daypartValid ? null : 'error');
    this.setValidationState('daypartCodeInvalid', daypartcodeValid ? null : 'error');
    return false;
  }

  render() {
		/* eslint-disable no-unused-vars */
		const { detail, proposalEditForm, initialdata } = this.props;
    return (
			<Well bsSize="small">
        <Row>
          <Col md={3}>
            <Form inline>
              <FormGroup controlId="detailFlight">
                <ControlLabel style={{ paddingRight: 5 }}>Flight</ControlLabel>
                <FlightPicker
                  startDate={detail.FlightStartDate}
                  endDate={detail.FlightEndDate}
                />
              </FormGroup>
            </Form>
          </Col>
          {this.state.activeDetail &&
          <Col md={9}>
            <Form inline>
              <FormGroup controlId="proposalDetailSpotLength" validationState={this.state.spotLengthInvalid}>
                <ControlLabel style={{ float: 'left', margin: '6px 10px 0 0' }}>Spot Length</ControlLabel>
                <Select
                  name="proposalDetailSpotLength"
                  value={detail.SpotLengthId}
                  placeholder=""
                  options={this.props.initialdata.SpotLengths}
                  labelKey="Display"
                  valueKey="Id"
                  onChange={this.onChangeSpotLength}
                  clearable={false}
                  wrapperStyle={{ float: 'left', minWidth: '70px' }}
                />
              </FormGroup>
              <FormGroup controlId="proposalDetailDaypart" validationState={this.state.daypartInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart</ControlLabel>
                <FormControl type="text" value={detail.Daypart.Text} readOnly />
              </FormGroup>
              <FormGroup controlId="proposalDetailDaypartCode" validationState={this.state.daypartCodeInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart Code</ControlLabel>
                <FormControl type="text" style={{ width: '80px' }} value={detail.DaypartCode} onChange={this.onChangeDaypartCode} />
              </FormGroup>
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 10px 0 24px' }}>Inventory</Button>
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 16px 0 0' }}>Open Market Inventory</Button>
              <FormGroup controlId="proposalDetailADU">
                <Checkbox checked={detail.Adu} onChange={this.onChangeAdu} />
                <ControlLabel style={{ margin: '0 0 0 6px' }}>ADU</ControlLabel>
              </FormGroup>
              <Button bsStyle="link" style={{ float: 'right' }} onClick={this.onDeleteProposalDetail}><Glyphicon style={{ color: 'red', fontSize: '16px' }} glyph="trash" /></Button>
              <Button bsStyle="primary" bsSize="xsmall" style={{ float: 'right', margin: '6px 10px 0 4px' }}>Sweeps</Button>
            </Form>
          </Col>
          }
        </Row>
			</Well>
    );
  }
}

ProposalDetail.defaultProps = {
};

ProposalDetail.propTypes = {
  initialdata: PropTypes.object.isRequired,
	detail: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
