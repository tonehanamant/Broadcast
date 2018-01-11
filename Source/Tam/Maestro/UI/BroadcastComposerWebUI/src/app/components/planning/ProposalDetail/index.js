import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Select from 'react-select';
import { Well, Form, FormGroup, ControlLabel, Row, Col, FormControl, Button, Checkbox, Glyphicon, HelpBlock } from 'react-bootstrap';
import FlightPicker from 'Components/shared/FlightPicker';
import DayPartPicker from 'Components/shared/DayPartPicker';
import ProposalDetailGrid from 'Components/planning/ProposalDetailGrid';
import Sweeps from './Sweeps';

export default class ProposalDetail extends Component {
  constructor(props) {
    super(props);

    this.onFlightPickerApply = this.onFlightPickerApply.bind(this);
    this.onChangeSpotLength = this.onChangeSpotLength.bind(this);
    this.onChangeDaypartCode = this.onChangeDaypartCode.bind(this);
    this.onChangeAdu = this.onChangeAdu.bind(this);
    this.onDeleteProposalDetail = this.onDeleteProposalDetail.bind(this);

    this.flightPickerApply = this.flightPickerApply.bind(this);
    this.openSweepsModal = this.openSweepsModal.bind(this);

    this.setValidationState = this.setValidationState.bind(this);
    this.onSaveShowValidation = this.onSaveShowValidation.bind(this);
    this.checkValidSpotLength = this.checkValidSpotLength.bind(this);
    this.checkValidDaypart = this.checkValidDaypart.bind(this);
    this.checkValidDaypartCode = this.checkValidDaypartCode.bind(this);
    this.openInventory = this.openInventory.bind(this);

    this.state = {
      validationStates: {
        SpotLengthId: null,
        Daypart: null,
        DaypartCode: null,
        DaypartCode_Alphanumeric: null,
        DaypartCode_MaxChar: null,
      },
    };
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
          action: () => this.flightPickerApply(flight),
          dismiss: () => {},
        },
      });
    } else {
      this.flightPickerApply(flight);
    }
  }

  onChangeSpotLength(value) {
    const val = value ? value.Id : null;
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'SpotLengthId', value: val });
    this.checkValidSpotLength(val);
  }

  onDayPartPickerApply(daypart) {
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'Daypart', value: daypart });
  }

  onChangeDaypartCode(event) {
    const val = event.target.value || '';
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'DaypartCode', value: val });
    this.checkValidDaypartCode(val);
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
        dismiss: () => {},
      },
    });
  }

  flightPickerApply(flight) {
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

  openSweepsModal() {
    this.props.toggleModal({
      modal: 'sweepsModal',
      active: true,
      properties: { detailId: this.props.detail.Id },
    });
  }

  setValidationState(type, state) {
    this.setState(prevState => ({
      ...prevState,
      validationStates: {
        ...prevState.validationStates,
        [type]: state,
      },
    }));
  }

  onSaveShowValidation(nextProps) {
    const { SpotLengthId, Daypart, DaypartCode } = nextProps.detail;
    this.checkValidSpotLength(SpotLengthId);
    this.checkValidDaypart(Daypart);
    this.checkValidDaypartCode(DaypartCode);
  }

  checkValidSpotLength(value) {
    const val = value;
    this.setValidationState('SpotLengthId', val ? null : 'error');
  }

  checkValidDaypart(value) {
    const val = value;
    this.setValidationState('Daypart', val ? null : 'error');
  }

  checkValidDaypartCode(value) {
    const val = value || '';
    this.setValidationState('DaypartCode', val !== '' ? null : 'error');
    const re = /^[a-z0-9]+$/i; // check alphanumeric
    this.setValidationState('DaypartCode_Alphanumeric', (re.test(val) || val === '') ? null : 'error');
    this.setValidationState('DaypartCode_MaxChar', val.length <= 10 ? null : 'error');
  }

  openInventory(type) {
    const isDirty = this.props.isDirty();
    if (isDirty) {
      this.props.createAlert({
        type: 'warning',
        headline: 'Proposal Not Saved',
        message: 'To access Inventory Planner you must save proposal first.',
      });
      return;
  }
    const detailId = this.props.detail.Id;
    const proposalId = this.props.proposalEditForm.Id;
    const readOnly = this.props.isReadOnly;
    const modalUrl = `/broadcast/planning?modal=${type}&proposalId=${proposalId}&detailId=${detailId}&readOnly=${readOnly}`;
    // console.log('openInventory', modalUrl, type, detailId, proposalId, readOnly, this.props.proposalEditForm);
    if (readOnly) {
      const title = (type === 'inventory') ? 'Inventory Read Only' : 'Open Market Inventory Read Only';
      const { Statuses } = this.props.initialdata;
      const status = this.props.proposalEditForm.Status;
      const statusDisplay = Statuses.find(statusItem => statusItem.Id === status);
      const body = `Proposal Status of ${statusDisplay.Display}, you will not be able to save inventory.  Press "Continue" to go to Inventory.`;
      this.props.toggleModal({
        modal: 'confirmModal',
        active: true,
        properties: {
          titleText: title,
          bodyText: body,
          closeButtonText: 'Cancel',
          actionButtonText: 'Continue',
          actionButtonBsStyle: 'warning',
          action: () => { window.location = modalUrl; },
          dismiss: () => {},
        },
      });
      return;
    }
    window.location = modalUrl;
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.proposalValidationStates.DetailInvalid === true) {
      this.onSaveShowValidation(nextProps);
    }
  }

  render() {
		/* eslint-disable no-unused-vars */
    const { detail, proposalEditForm, initialdata, updateProposalEditFormDetail, updateProposalEditFormDetailGrid, onUpdateProposal, isReadOnly, toggleModal, proposalValidationStates } = this.props;
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
              <FormGroup controlId="proposalDetailSpotLength" validationState={this.state.validationStates.SpotLengthId}>
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
                {this.state.validationStates.SpotLengthId != null &&
                <HelpBlock style={{ margin: '0 0 0 16px' }}>
                  <span className="text-danger" style={{ fontSize: 11 }}>Required.</span>
                </HelpBlock>
                }
              </FormGroup>
              }
              {detail &&
              // <FormGroup controlId="proposalDetailDaypart" validationState={this.state.validationStates.Daypart}>
              //   <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart</ControlLabel>
                <DayPartPicker
                  dayPart={detail.Daypart || undefined}
                  onApply={daypart => this.onDayPartPickerApply(daypart)}
                  isReadOnly={isReadOnly}
                  validationState={this.state.validationStates.Daypart}
                />
              //   {this.state.validationStates.Daypart != null &&
              //   <HelpBlock style={{ margin: '0 0 0 16px' }}>
              //     <span className="text-danger" style={{ fontSize: 11 }}>Required.</span>
              //   </HelpBlock>
              //   }
              // </FormGroup>
              }
              {detail &&
              <FormGroup controlId="proposalDetailDaypartCode" validationState={this.state.validationStates.DaypartCode || this.state.validationStates.DaypartCode_Alphanumeric || this.state.validationStates.DaypartCode_MaxChar}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart Code</ControlLabel>
                <FormControl type="text" style={{ width: '80px' }} value={detail.DaypartCode ? detail.DaypartCode : ''} onChange={this.onChangeDaypartCode} disabled={isReadOnly} />
                {this.state.validationStates.DaypartCode != null &&
                <HelpBlock style={{ margin: '0 0 0 16px' }}>
                  <span className="text-danger" style={{ fontSize: 11 }}>Required.</span>
                </HelpBlock>
                }
                {this.state.validationStates.DaypartCode_Alphanumeric != null &&
                <HelpBlock style={{ margin: '0 0 0 16px' }}>
                  <span className="text-danger" style={{ fontSize: 11 }}>Please enter only alphanumeric characters.</span>
                </HelpBlock>
                }
                {this.state.validationStates.DaypartCode_MaxChar != null &&
                <HelpBlock style={{ margin: '0 0 0 16px' }}>
                  <span className="text-danger" style={{ fontSize: 11 }}>Please enter no more than 10 characters.</span>
                </HelpBlock>
                }
              </FormGroup>
              }
              {detail &&
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 10px 0 24px' }} onClick={() => this.openInventory('inventory')}>Inventory</Button>
              }
              {detail &&
              <Button bsStyle="primary" bsSize="xsmall" style={{ margin: '0 16px 0 0' }} onClick={() => this.openInventory('openMarket')}>Open Market Inventory</Button>
              }
              {detail &&
              <FormGroup controlId="proposalDetailADU">
                <Checkbox checked={detail.Adu} onChange={this.onChangeAdu} disabled={isReadOnly} />
                <ControlLabel style={{ margin: '0 0 0 6px' }}>ADU</ControlLabel>
              </FormGroup>
              }
              {(detail && !isReadOnly) &&
              <Button bsStyle="link" style={{ float: 'right' }} onClick={this.onDeleteProposalDetail}><Glyphicon style={{ color: '#c12e2a', fontSize: '16px' }} glyph="trash" /></Button>
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
              updateProposalEditFormDetailGrid={updateProposalEditFormDetailGrid}
              onUpdateProposal={onUpdateProposal}
              toggleModal={toggleModal}
              proposalValidationStates={proposalValidationStates}
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
  proposalValidationStates: {},
  updateProposalEditFormDetail: () => {},
  updateProposalEditFormDetailGrid: () => {},
  onUpdateProposal: () => {},
  deleteProposalDetail: () => {},
  modelNewProposalDetail: () => {},
  toggleModal: () => {},
  createAlert: () => {},
};

ProposalDetail.propTypes = {
  initialdata: PropTypes.object.isRequired,
	detail: PropTypes.object,
  proposalEditForm: PropTypes.object,
  updateProposalEditFormDetail: PropTypes.func,
  updateProposalEditFormDetailGrid: PropTypes.func,
  onUpdateProposal: PropTypes.func,
  deleteProposalDetail: PropTypes.func,
  modelNewProposalDetail: PropTypes.func,
  toggleModal: PropTypes.func,
  isReadOnly: PropTypes.bool.isRequired,
  createAlert: PropTypes.func,
  isDirty: PropTypes.func.isRequired,

  proposalValidationStates: PropTypes.object,
};
