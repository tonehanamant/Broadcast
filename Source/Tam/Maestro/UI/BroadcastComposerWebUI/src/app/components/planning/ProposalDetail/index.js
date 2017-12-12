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

    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);

    this.toggleSweepsModal = this.toggleSweepsModal.bind(this);
    this.onClickSweeps = this.onClickSweeps.bind(this);
    this.onChangeSweeps = this.onChangeSweeps.bind(this);

    this.state = {
      activeDetail: true, // temp use prop
      spotLengthInvalid: null,
      datpartInvalid: null,
      daypartCodeInvalid: null,
      isSweepsModalOpen: false,

      sweepsOptions: {
        shareBookOptions: [],
        hutBookOptions: [],
        playbackTypeOptions: [],
      },

      sweepsSelected: {
        shareBook: null,
        hutBook: null,
        playbackType: null,
      },
    };
  }

  onClickSweeps() {
    this.toggleSweepsModal();
  }

  toggleSweepsModal() {
    this.setState({ isSweepsModalOpen: !this.state.isSweepsModalOpen });
  }

  onChangeSweeps(shareBook, hutBook, playbackType) {
    this.setState({
      sweepsSelected: {
        shareBook,
        hutBook,
        playbackType,
      },
    });

    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'SharePostingBookId', value: shareBook.Id });
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'HutPostingBookId', value: hutBook.Id });
    this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'PlaybackType', value: playbackType.Id });

    this.setState({ isSweepsModalOpen: false });
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

  onFlightPickerApply(flight) {
    // checkValidFlightDetails();
    if (this.props.detail) {
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightStartDate', value: flight.StartDate });
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightEndDate', value: flight.EndDate });
      this.props.updateProposalEditFormDetail({ id: this.props.detail.Id, key: 'FlightWeeks', value: flight.FlightWeeks });
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

  componentWillMount() {
    const { initialdata, detail } = this.props;

    // TODO - use DefaultPostingBooks if values in detail are null
    let shareBook = null;
    let hutBook = null;
    let playbackType = null;

    if (detail) {
      shareBook = initialdata.ForecastDefaults.CrunchedMonths.filter(o => o.Id === detail.SharePostingBookId).shift();
      hutBook = initialdata.ForecastDefaults.CrunchedMonths.filter(o => o.Id === detail.HutPostingBookId).shift();
      playbackType = initialdata.ForecastDefaults.PlaybackTypes.filter(o => o.Id === detail.PlaybackType).shift();
    }

    this.setState({
      sweepsOptions: {
        shareBookOptions: initialdata.ForecastDefaults.CrunchedMonths,
        hutBookOptions: initialdata.ForecastDefaults.CrunchedMonths,
        playbackTypeOptions: initialdata.ForecastDefaults.PlaybackTypes,
      },
      sweepsSelected: {
        shareBook,
        hutBook,
        playbackType,
      },
    });
  }

  render() {
		/* eslint-disable no-unused-vars */
    const { detail, proposalEditForm, initialdata } = this.props;
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
                />
              </FormGroup>
              {detail &&
              <FormGroup controlId="proposalDetailSpotLength" validationState={this.state.spotLengthInvalid}>
                <ControlLabel style={{ float: 'left', margin: '8px 10px 0 16px' }}>Spot Length</ControlLabel>
                <Select
                  name="proposalDetailSpotLength"
                  value={detail.SpotLength}
                  placeholder=""
                  options={this.props.initialdata.SpotLengths}
                  labelKey="Display"
                  valueKey="Id"
                  onChange={this.onChangeSpotLength}
                  clearable={false}
                  wrapperStyle={{ float: 'left', minWidth: '70px' }}
                />
              </FormGroup>
              }
              {detail &&
              <FormGroup controlId="proposalDetailDaypart" validationState={this.state.daypartInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart</ControlLabel>
                  <FormControl type="text" value={detail.Daypart && detail.Daypart.Text ? detail.Daypart.Text : ''} readOnly />
              </FormGroup>
              }
              {detail &&
              <FormGroup controlId="proposalDetailDaypartCode" validationState={this.state.daypartCodeInvalid}>
                <ControlLabel style={{ margin: '0 10px 0 16px' }}>Daypart Code</ControlLabel>
                  <FormControl type="text" style={{ width: '80px' }} value={detail.DaypartCode ? detail.DaypartCode : ''} onChange={this.onChangeDaypartCode} />
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
                <Checkbox checked={detail.Adu} onChange={this.onChangeAdu} />
                <ControlLabel style={{ margin: '0 0 0 6px' }}>ADU</ControlLabel>
              </FormGroup>
              }
              {detail &&
              <Button bsStyle="link" style={{ float: 'right' }} onClick={this.onDeleteProposalDetail}><Glyphicon style={{ color: 'red', fontSize: '16px' }} glyph="trash" /></Button>
              }
              {detail &&
              <Button
                bsStyle="primary"
                bsSize="xsmall"
                style={{ float: 'right', margin: '6px 10px 0 4px' }}
                onClick={this.onClickSweeps}
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
              // todo isReadonly
            />
          </Col>
        </Row>
        }

        <Sweeps
          show={this.state.isSweepsModalOpen}
          onClose={this.toggleSweepsModal}
          onSave={this.onChangeSweeps}
          {...this.state.sweepsOptions}
          {...this.state.sweepsSelected}
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
};
