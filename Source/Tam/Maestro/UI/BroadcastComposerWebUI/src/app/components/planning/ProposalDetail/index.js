import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Select from 'react-select';
import { Well, Form, FormGroup, ControlLabel, Row, Col, FormControl, Button, Checkbox, Glyphicon } from 'react-bootstrap';
import FlightPicker from 'Components/shared/FlightPicker';
import Sweeps from './Sweeps';

export default class ProposalDetail extends Component {
  constructor(props) {
    super(props);
    console.log('PROPOSAL DETAIL PROPS', this.props);

    this.onChangeSpotLength = this.onChangeSpotLength.bind(this);
    this.onChangeDaypartCode = this.onChangeDaypartCode.bind(this);
    this.onChangeAdu = this.onChangeAdu.bind(this);
    this.onDeleteProposalDetail = this.onDeleteProposalDetail.bind(this);

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
    const shareBook = initialdata.ForecastDefaults.CrunchedMonths.filter(o => o.Id === detail.SharePostingBookId).shift();
    const hutBook = initialdata.ForecastDefaults.CrunchedMonths.filter(o => o.Id === detail.HutPostingBookId).shift();
    const playbackType = initialdata.ForecastDefaults.PlaybackTypes.filter(o => o.Id === detail.PlaybackType).shift();

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

              <Button
                bsStyle="primary"
                bsSize="xsmall"
                style={{ float: 'right', margin: '6px 10px 0 4px' }}
                onClick={this.onClickSweeps}
              >
                Sweeps
              </Button>
            </Form>
          </Col>
          }
        </Row>

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
};

ProposalDetail.propTypes = {
  initialdata: PropTypes.object.isRequired,
	detail: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
