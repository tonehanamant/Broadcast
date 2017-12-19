import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Select from 'react-select';
import { Well, Form, FormGroup, ControlLabel, Row, Col, FormControl, Button, Checkbox, Glyphicon } from 'react-bootstrap';
import FlightPicker from 'Components/shared/FlightPicker';
import ProposalDetailGrid from 'Components/planning/ProposalDetailGrid';
import Sweeps from './Sweeps';

export default class ProposalDetail extends Component {
  constructor(props) {
    super(props);

    this.onChangeSpotLength = this.onChangeSpotLength.bind(this);
    this.onChangeDaypartCode = this.onChangeDaypartCode.bind(this);
    this.onChangeAdu = this.onChangeAdu.bind(this);
    this.onDeleteProposalDetail = this.onDeleteProposalDetail.bind(this);
    this.onFlightPickerApply = this.onFlightPickerApply.bind(this);
    this.FlightPickerApply = this.FlightPickerApply.bind(this);

    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);

    this.openSweepsModal = this.openSweepsModal.bind(this);

    this.state = {
      activeDetail: true, // temp use prop
      spotLengthInvalid: null,
      datpartInvalid: null,
      daypartCodeInvalid: null,
    };
  }

  openSweepsModal() {
    this.props.toggleModal({
      modal: 'sweepsModal',
      active: true,
      properties: { detailId: this.props.detail.Id },
    });
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
    this.setValidationState('daypartCodeInvalid', re.test(val) && val.length <= 10 ? null : 'error');
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

  onFlightPickerApply(flight) {
    if (this.props.detail) {
      this.props.toggleModal({
        modal: 'confirmModal',
        active: true,
        properties: {
          titleText: 'Flight Change',
          bodyText: 'Existing data will be affected by this Flight change. Click Continue to proceed',
          closeButtonText: 'Cancel',
          closeButtonBsStyle: 'default',
          actionButtonText: 'Continue',
          actionButtonBsStyle: 'warning',
          action: () => this.FlightPickerApply(flight),
        },
      });
    } else {
      this.FlightPickerApply(flight);
    }
  }

  FlightPickerApply(flight) {
    if (this.props.detail) {
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightStartDate', value: flight.StartDate });
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightEndDate', value: flight.EndDate });
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightWeeks', value: flight.FlightWeeks });
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightEdited', value: true });
      this.props.onUpdateProposal();
    } else {
      this.props.modelNewProposalDetail(flight);
    }
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
    const { detail, proposalEditForm, initialdata, updateProposalEditFormDetail, isReadOnly } = this.props;
    return (
			<Well bsSize="small">
        <Row>
          <Col md={12}>
            <Form inline>
              <FormGroup controlId="detailFlight">
                <ControlLabel style={{ margin: '0 10px 0 0' }}>Flight</ControlLabel>
                <FlightPicker
                  startDate={detail && detail.FlightStartDate ? detail.FlightStartDate : null}
                  endDate={detail && detail.FlightEndDate ? detail.FlightEndDate : null}
                  flightWeeks={detail && detail.FlightWeeks ? detail.FlightWeeks : null}
                  onApply={flight => this.onFlightPickerApply(flight)}
                  isReadOnly={isReadOnly}
                />
              </FormGroup>
              {detail &&
              <FormGroup controlId="proposalDetailSpotLength" validationState={this.state.spotLengthInvalid}>
                <ControlLabel style={{ float: 'left', margin: '8px 10px 0 16px' }}>Spot Length</ControlLabel>
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
                  disabled={isReadOnly}
                />
              </FormGroup>
              }
              {detail &&
              <FormGroup controlId="proposalDetailDaypart" validationState={this.state.daypartInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart</ControlLabel>
                  <FormControl type="text" value={detail.Daypart && detail.Daypart.Text ? detail.Daypart.Text : ''} disabled={isReadOnly} readOnly />
              </FormGroup>
              }
              {detail &&
              <FormGroup controlId="proposalDetailDaypartCode" validationState={this.state.daypartCodeInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart Code</ControlLabel>
                  <FormControl type="text" style={{ width: '80px' }} value={detail.DaypartCode ? detail.DaypartCode : ''} onChange={this.onChangeDaypartCode} disabled={isReadOnly} />
              </FormGroup>
              }
              {detail &&
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 10px 0 24px' }}>Inventory</Button>
              }
              {detail &&
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 16px 0 0' }}>Open Market Inventory</Button>
              }
              {detail &&
              <FormGroup controlId="proposalDetailADU">
                <Checkbox checked={detail.Adu} onChange={this.onChangeAdu} disabled={isReadOnly} />
                <ControlLabel style={{ margin: '0 0 0 6px' }}>ADU</ControlLabel>
              </FormGroup>
              }
              {(detail && !isReadOnly) &&
              <Button bsStyle="link" style={{ float: 'right' }} onClick={this.onDeleteProposalDetail}><Glyphicon style={{ color: 'red', fontSize: '16px' }} glyph="trash" /></Button>
              }
              {detail &&
              <Button
                bsStyle="primary"
                bsSize="xsmall"
                style={{ float: 'right', margin: '6px 10px 0 4px' }}
                onClick={this.openSweepsModal}
              >
                Sweeps
              </Button>
              }
            </Form>
          </Col>
        </Row>
        {detail &&
        <Row style={{ marginTop: 10 }}>
          <Col md={12}>
            <ProposalDetailGrid
              detailId={detail.Id}
              GridQuarterWeeks={detail.GridQuarterWeeks}
              isAdu={detail.Adu}
              isReadOnly={isReadOnly}
            />
          </Col>
        </Row>
        }

        <Sweeps
          toggleModal={this.props.toggleModal}
          onClose={this.toggleSweepsModal}
          updateProposalEditFormDetail={updateProposalEditFormDetail}
          initialdata={initialdata}
          detail={detail}
          isReadOnly={isReadOnly}
        />
			</Well>
    );
  }
}

ProposalDetail.defaultProps = {
  detail: null,
  proposalEditForm: {},
  updateProposalEditFormDetail: () => {},
  onUpdateProposal: () => {},
  deleteProposalDetail: () => {},
  modelNewProposalDetail: () => {},
  toggleModal: () => {},
};

ProposalDetail.propTypes = {
  initialdata: PropTypes.object.isRequired,
	detail: PropTypes.object,
  proposalEditForm: PropTypes.object,
  updateProposalEditFormDetail: PropTypes.func,
  onUpdateProposal: PropTypes.func,
  deleteProposalDetail: PropTypes.func,
  modelNewProposalDetail: PropTypes.func,
  toggleModal: PropTypes.func,
  isReadOnly: PropTypes.bool.isRequired,
};
