/* eslint-disable no-undef */
import React, { Component } from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import { withRouter } from "react-router-dom";
import Select from "react-select";
import CSSModules from "react-css-modules";
import { InputNumber } from "antd";
import {
  Well,
  Form,
  FormGroup,
  InputGroup,
  ControlLabel,
  Row,
  Col,
  FormControl,
  Button,
  DropdownButton,
  MenuItem,
  Checkbox,
  Glyphicon,
  HelpBlock
} from "react-bootstrap";
import FlightPicker from "Patterns/FlightPicker";
import DayPartPicker from "Patterns/DayPartPicker";

import {
  rerunPostScrubing,
  loadPricingData,
  generateScx
} from "Ducks/planning";

import PricingGuide from "PricingGuide";
import ProposalDetailGrid from "../DetailGrid";
import Sweeps from "../DetailSweeps";
import ProgramGenre from "../DetailProgramGenre";
import PostingBook from "../DetailPostingBook";
import UploadBuy from "../DetailUploadBuy";
import styles from "./index.style.scss";

const mapStateToProps = ({ planning: { isISCIEdited, isGridCellEdited } }) => ({
  isISCIEdited,
  isGridCellEdited
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    { rerunPostScrubing, loadPricingData, generateScx },
    dispatch
  );

export class ProposalDetail extends Component {
  constructor(props) {
    super(props);

    this.onFlightPickerApply = this.onFlightPickerApply.bind(this);
    this.onChangeSpotLength = this.onChangeSpotLength.bind(this);
    this.onChangeDaypartCode = this.onChangeDaypartCode.bind(this);
    this.onChangeAdu = this.onChangeAdu.bind(this);
    this.onChangeNti = this.onChangeNti.bind(this);
    this.onDeleteProposalDetail = this.onDeleteProposalDetail.bind(this);

    this.flightPickerApply = this.flightPickerApply.bind(this);

    this.setValidationState = this.setValidationState.bind(this);
    this.onSaveShowValidation = this.onSaveShowValidation.bind(this);
    this.checkValidSpotLength = this.checkValidSpotLength.bind(this);
    this.checkValidDaypart = this.checkValidDaypart.bind(this);
    this.checkValidDaypartCode = this.checkValidDaypartCode.bind(this);
    this.checkValidNtiLength = this.checkValidNtiLength.bind(this);
    this.openInventory = this.openInventory.bind(this);
    this.openModal = this.openModal.bind(this);
    this.openPricingGuide = this.openPricingGuide.bind(this);
    this.rerunPostScrubing = this.rerunPostScrubing.bind(this);
    this.generateSCX = this.generateSCX.bind(this);
    this.onDayPartPickerApply = this.onDayPartPickerApply.bind(this);

    this.state = {
      validationStates: {
        SpotLengthId: null,
        Daypart: null,
        DaypartCode: null,
        DaypartCode_Alphanumeric: null,
        DaypartCode_MaxChar: null,
        NtiLength: null
      }
    };
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.proposalValidationStates.DetailInvalid === true) {
      this.onSaveShowValidation(nextProps);
    }
  }

  onFlightPickerApply(flight) {
    const { detail, toggleModal } = this.props;
    if (detail) {
      toggleModal({
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: "Flight Change",
          bodyText:
            "Existing data will be affected by this Flight change. Click Continue to proceed",
          closeButtonText: "Cancel",
          closeButtonBsStyle: "default",
          actionButtonText: "Continue",
          actionButtonBsStyle: "warning",
          action: () => this.flightPickerApply(flight),
          dismiss: () => {}
        }
      });
    } else {
      this.flightPickerApply(flight);
    }
  }

  onChangeSpotLength(value) {
    const { updateProposalEditFormDetail, detail } = this.props;
    const val = value ? value.Id : null;
    updateProposalEditFormDetail({
      id: detail.Id,
      key: "SpotLengthId",
      value: val
    });
    this.checkValidSpotLength(val);
  }

  onDayPartPickerApply(daypart) {
    const { updateProposalEditFormDetail, detail } = this.props;
    updateProposalEditFormDetail({
      id: detail.Id,
      key: "Daypart",
      value: daypart
    });
  }

  onChangeDaypartCode(event) {
    const { updateProposalEditFormDetail, detail } = this.props;
    const val = event.target.value || "";
    updateProposalEditFormDetail({
      id: detail.Id,
      key: "DaypartCode",
      value: val
    });
    this.checkValidDaypartCode(val);
  }

  onChangeNti(value) {
    const { updateProposalEditFormDetail, detail } = this.props;
    const val = value !== null ? value : detail.NtiConversionFactor;
    const newVal = val / 100;
    updateProposalEditFormDetail({
      id: detail.Id,
      key: "NtiConversionFactor",
      value: newVal
    });
    this.checkValidNtiLength(value);
  }

  onChangeAdu(event) {
    const {
      updateProposalEditFormDetail,
      detail,
      onUpdateProposal
    } = this.props;
    updateProposalEditFormDetail({
      id: detail.Id,
      key: "Adu",
      value: event.target.checked
    });
    onUpdateProposal();
  }

  onDeleteProposalDetail() {
    const { deleteProposalDetail, detail, toggleModal } = this.props;
    toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        titleText: "Delete Proposal Detail",
        bodyText: "Are you sure you wish to Delete the proposal detail?",
        closeButtonText: "Cancel",
        actionButtonText: "Continue",
        actionButtonBsStyle: "danger",
        action: () => deleteProposalDetail({ id: detail.Id }),
        dismiss: () => {}
      }
    });
  }

  onSaveShowValidation(nextProps) {
    const {
      SpotLengthId,
      Daypart,
      DaypartCode,
      NtiConversionFactor
    } = nextProps.detail;
    this.checkValidSpotLength(SpotLengthId);
    this.checkValidDaypart(Daypart);
    this.checkValidDaypartCode(DaypartCode);
    this.checkValidNtiLength(NtiConversionFactor);
  }

  setValidationState(type, state) {
    this.setState(prevState => ({
      ...prevState,
      validationStates: {
        ...prevState.validationStates,
        [type]: state
      }
    }));
  }

  rerunPostScrubing() {
    const { detail, proposalEditForm, rerunPostScrubing } = this.props;
    rerunPostScrubing(proposalEditForm.Id, detail.Id);
  }

  openModal(modal) {
    const { detail, toggleModal } = this.props;
    toggleModal({
      modal,
      active: true,
      properties: { detailId: detail.Id }
    });
  }

  flightPickerApply(flight) {
    const {
      detail,
      updateProposalEditFormDetail,
      onUpdateProposal,
      proposalEditForm: { PostType },
      modelNewProposalDetail
    } = this.props;
    if (detail) {
      updateProposalEditFormDetail({
        id: detail.Id,
        key: "FlightStartDate",
        value: flight.StartDate
      });
      updateProposalEditFormDetail({
        id: detail.Id,
        key: "FlightEndDate",
        value: flight.EndDate
      });
      updateProposalEditFormDetail({
        id: detail.Id,
        key: "FlightWeeks",
        value: flight.FlightWeeks
      });
      updateProposalEditFormDetail({
        id: detail.Id,
        key: "FlightEdited",
        value: true
      });
      // Need a hard clearing of the data or the rendered cells in swetail grid get mixed up (state is not properly re-rendered)
      // clear the grid data - GridQuarterWeeks - while maintainig the overall state changes in the edited values (detail)
      updateProposalEditFormDetail({
        id: detail.Id,
        key: "GridQuarterWeeks",
        value: []
      });
      // reset the data from the edited details processed by the BE
      onUpdateProposal();
    } else {
      modelNewProposalDetail({ ...flight, PostType });
    }
  }

  openPricingGuide() {
    const { detail, loadPricingData } = this.props;
    loadPricingData(detail.Id);
  }

  checkValidSpotLength(value) {
    const val = value;
    this.setValidationState("SpotLengthId", val ? null : "error");
  }

  checkValidDaypart(value) {
    const val = value;
    this.setValidationState("Daypart", val ? null : "error");
  }

  checkValidDaypartCode(value) {
    const val = value || "";
    this.setValidationState("DaypartCode", val !== "" ? null : "error");
    const re = /^[a-z0-9]+$/i; // check alphanumeric
    this.setValidationState(
      "DaypartCode_Alphanumeric",
      re.test(val) || val === "" ? null : "error"
    );
    this.setValidationState(
      "DaypartCode_MaxChar",
      val.length <= 10 ? null : "error"
    );
  }

  checkValidNtiLength(value) {
    const val = value;
    this.setValidationState(
      "NtiLength",
      !Number.isNaN(val) && val !== "" && val !== null ? null : "error"
    );
  }

  openInventory(type) {
    const {
      location,
      proposalEditForm,
      detail,
      isDirty,
      createAlert,
      initialdata,
      toggleModal
    } = this.props;

    if (isDirty()) {
      createAlert({
        type: "warning",
        headline: "Proposal Not Saved",
        message: "To access Inventory Planner you must save proposal first."
      });
      return;
    }

    const detailId = detail.Id;
    const version = proposalEditForm.Version;
    const proposalId = proposalEditForm.Id;
    const status = proposalEditForm.Status;
    const readOnly = status != null ? status === 1 || status === 4 : false;
    const fromVersion = location.pathname.indexOf("/version/") !== -1;
    const modalUrl = fromVersion
      ? `/broadcast/planning?modal=${type}&proposalId=${proposalId}&detailId=${detailId}&readOnly=${readOnly}&version=${version}`
      : `/broadcast/planning?modal=${type}&proposalId=${proposalId}&detailId=${detailId}&readOnly=${readOnly}`;
    if (readOnly) {
      const title =
        type === "inventory"
          ? "Inventory Read Only"
          : "Open Market Inventory Read Only";
      const { Statuses } = initialdata;
      const statusDisplay = Statuses.find(
        statusItem => statusItem.Id === status
      );
      const body = `Proposal Status of ${
        statusDisplay.Display
      }, you will not be able to save inventory.  Press "Continue" to go to Inventory.`;
      toggleModal({
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: title,
          bodyText: body,
          closeButtonText: "Cancel",
          actionButtonText: "Continue",
          actionButtonBsStyle: "warning",
          action: () => {
            window.location = modalUrl;
          },
          dismiss: () => {}
        }
      });
      return;
    }
    window.location = modalUrl;
  }

  generateSCX() {
    const { detail, generateScx } = this.props;
    generateScx([detail.Id], true);
  }

  render() {
    const {
      detail,
      initialdata,
      updateProposalEditFormDetail,
      updateProposalEditFormDetailGrid,
      onUpdateProposal,
      isReadOnly,
      toggleModal,
      proposalValidationStates,
      proposalEditForm,
      isISCIEdited,
      isGridCellEdited
    } = this.props;
    const { validationStates } = this.state;

    return (
      <Well bsSize="small" styleName="proposal-detail-wrap">
        <Row>
          <Col md={12}>
            <Form inline styleName="proposal-detail-form">
              <FormGroup
                controlId="detailFlight"
                styleName="proposal-detail-form-item"
              >
                <ControlLabel>Flight</ControlLabel>
                <FlightPicker
                  startDate={
                    detail && detail.FlightStartDate
                      ? detail.FlightStartDate
                      : null
                  }
                  endDate={
                    detail && detail.FlightEndDate ? detail.FlightEndDate : null
                  }
                  flightWeeks={
                    detail && detail.FlightWeeks ? detail.FlightWeeks : null
                  }
                  onApply={this.onFlightPickerApply}
                  isReadOnly={isReadOnly}
                />
              </FormGroup>
              {detail && (
                <FormGroup
                  controlId="proposalDetailSpotLength"
                  validationState={validationStates.SpotLengthId}
                  styleName="proposal-detail-form-item"
                >
                  <div styleName="proposal-form-label">
                    <ControlLabel>Spot Length</ControlLabel>
                    {validationStates.SpotLengthId != null && (
                      <HelpBlock>
                        <span className="text-danger">Required.</span>
                      </HelpBlock>
                    )}
                  </div>
                  <Select
                    name="proposalDetailSpotLength"
                    value={detail.SpotLengthId}
                    placeholder=""
                    options={initialdata.SpotLengths}
                    labelKey="Display"
                    valueKey="Id"
                    onChange={this.onChangeSpotLength}
                    clearable={false}
                    wrapperStyle={{ float: "left", minWidth: "70px" }}
                    disabled={isReadOnly}
                  />
                </FormGroup>
              )}
              {detail && (
                <FormGroup
                  controlId="proposalDetailDaypart"
                  validationState={validationStates.Daypart}
                  styleName="proposal-detail-form-item"
                >
                  <div styleName="proposal-form-label">
                    <ControlLabel>Daypart</ControlLabel>
                    {validationStates.Daypart && (
                      <HelpBlock>
                        <span className="text-danger">Required.</span>
                      </HelpBlock>
                    )}
                  </div>
                  <DayPartPicker
                    allowEmpty
                    dayPart={detail.Daypart || undefined}
                    onApply={this.onDayPartPickerApply}
                    disabled={isReadOnly}
                  />
                </FormGroup>
              )}
              {detail && (
                <FormGroup
                  styleName="proposal-detail-form-item"
                  controlId="proposalDetailDaypartCode"
                  validationState={
                    validationStates.DaypartCode ||
                    validationStates.DaypartCode_Alphanumeric ||
                    validationStates.DaypartCode_MaxChar
                  }
                >
                  <div styleName="proposal-form-label">
                    <ControlLabel>Daypart Code</ControlLabel>
                    {validationStates.DaypartCode != null && (
                      <HelpBlock>
                        <span className="text-danger">Required.</span>
                      </HelpBlock>
                    )}
                    {validationStates.DaypartCode_Alphanumeric != null && (
                      <HelpBlock>
                        <span className="text-danger">
                          Please enter only alphanumeric characters.
                        </span>
                      </HelpBlock>
                    )}
                    {validationStates.DaypartCode_MaxChar != null && (
                      <HelpBlock>
                        <span className="text-danger">
                          Please enter no more than 10 characters.
                        </span>
                      </HelpBlock>
                    )}
                  </div>
                  <FormControl
                    type="text"
                    style={{ width: "60px" }}
                    value={detail.DaypartCode ? detail.DaypartCode : ""}
                    onChange={this.onChangeDaypartCode}
                    disabled={isReadOnly}
                  />
                </FormGroup>
              )}
              {detail && proposalEditForm.PostType === 2 && (
                <FormGroup
                  controlId="proposalDetailNtiConversionFactor"
                  validationState={validationStates.NtiLength}
                  styleName="proposal-detail-form-item"
                >
                  <div styleName="proposal-form-label">
                    <ControlLabel>NTI</ControlLabel>
                    {validationStates.NtiLength != null && (
                      <HelpBlock>
                        <span className="text-danger">Required.</span>
                      </HelpBlock>
                    )}
                  </div>
                  <InputGroup>
                    <InputNumber
                      min={0}
                      max={99.99}
                      className="form-control"
                      style={{ width: "75px" }}
                      precision={2}
                      defaultValue={
                        detail && detail.NtiConversionFactor
                          ? detail.NtiConversionFactor * 100
                          : 0
                      }
                      onChange={this.onChangeNti}
                    />
                    <InputGroup.Addon>%</InputGroup.Addon>
                  </InputGroup>
                </FormGroup>
              )}
              {detail && (
                <FormGroup
                  controlId="proposalDetailADU"
                  styleName="proposal-detail-form-item"
                >
                  <ControlLabel>ADU</ControlLabel>
                  <Checkbox
                    checked={detail.Adu}
                    onChange={this.onChangeAdu}
                    disabled={isReadOnly}
                  />
                </FormGroup>
              )}
              {detail && (
                <FormGroup
                  controlId="EstimateId"
                  styleName="proposal-detail-form-item"
                >
                  <ControlLabel>Estimate ID</ControlLabel>
                  <span>{detail.EstimateId || "-"}</span>
                </FormGroup>
              )}
              <div styleName="proposal-detail-actions">
                {detail && !isReadOnly && (
                  <Button bsStyle="link" onClick={this.onDeleteProposalDetail}>
                    <Glyphicon
                      style={{ color: "#c12e2a", fontSize: "16px" }}
                      glyph="trash"
                    />
                  </Button>
                )}
                {detail && (
                  <div>
                    <DropdownButton
                      bsSize="xsmall"
                      bsStyle="success"
                      title={
                        <span
                          className="glyphicon glyphicon-option-horizontal"
                          aria-hidden="true"
                        />
                      }
                      noCaret
                      pullRight
                      id="detail_actions"
                    >
                      <MenuItem
                        eventKey="pricingGuide"
                        onSelect={this.openPricingGuide}
                      >
                        Pricing Guide
                      </MenuItem>
                      <MenuItem
                        eventKey="1"
                        onClick={() => this.openInventory("inventory")}
                      >
                        Proprietary Inventory
                      </MenuItem>
                      <MenuItem
                        eventKey="2"
                        onClick={() => this.openInventory("openMarket")}
                      >
                        Open Market Inventory
                      </MenuItem>
                      <MenuItem
                        eventKey="sweepsModal"
                        onSelect={this.openModal}
                      >
                        Projections Book
                      </MenuItem>
                      <MenuItem
                        eventKey="postingBook"
                        onSelect={this.openModal}
                      >
                        Posting Book
                      </MenuItem>
                      <MenuItem
                        eventKey="programGenreModal"
                        onSelect={this.openModal}
                      >
                        Program/Genre/Show Type
                      </MenuItem>
                      {isReadOnly && (
                        <MenuItem
                          eventKey="rerunPostScrubbing"
                          onSelect={this.rerunPostScrubing}
                        >
                          Rerun Post Scrubbing
                        </MenuItem>
                      )}
                      {isReadOnly && (
                        <MenuItem
                          eventKey="uploadBuy"
                          onSelect={this.openModal}
                        >
                          Upload SCX File
                        </MenuItem>
                      )}
                      {isReadOnly && (
                        <MenuItem
                          eventKey="generateSCX"
                          onClick={() => this.generateSCX()}
                        >
                          Generate SCX File
                        </MenuItem>
                      )}
                    </DropdownButton>
                  </div>
                )}
              </div>
            </Form>
          </Col>
        </Row>
        {detail && (
          <Row style={{ marginTop: 10 }}>
            <Col md={12}>
              <ProposalDetailGrid
                detailId={detail.Id}
                GridQuarterWeeks={detail.GridQuarterWeeks}
                isAdu={detail.Adu}
                isReadOnly={isReadOnly}
                updateProposalEditFormDetailGrid={
                  updateProposalEditFormDetailGrid
                }
                onUpdateProposal={onUpdateProposal}
                toggleModal={toggleModal}
                proposalValidationStates={proposalValidationStates}
                isISCIEdited={isISCIEdited}
                isGridCellEdited={isGridCellEdited}
              />
            </Col>
          </Row>
        )}

        <UploadBuy toggleModal={toggleModal} detail={detail} />

        <Sweeps
          toggleModal={toggleModal}
          updateProposalEditFormDetail={updateProposalEditFormDetail}
          initialdata={initialdata}
          detail={detail}
          isReadOnly={isReadOnly}
        />

        <PricingGuide
          toggleModal={toggleModal}
          updateProposalEditFormDetail={updateProposalEditFormDetail}
          initialdata={initialdata}
          detail={detail}
          isReadOnly={isReadOnly}
        />

        <ProgramGenre
          toggleModal={toggleModal}
          updateProposalEditFormDetail={updateProposalEditFormDetail}
          detail={detail}
          isReadOnly={isReadOnly}
        />
        {detail && (
          <PostingBook
            updateProposalEditFormDetail={updateProposalEditFormDetail}
            initialdata={initialdata}
            detail={detail}
            isReadOnly={isReadOnly}
          />
        )}
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
  createAlert: () => {}
};

ProposalDetail.propTypes = {
  initialdata: PropTypes.object.isRequired,
  detail: PropTypes.object,
  proposalEditForm: PropTypes.object,
  updateProposalEditFormDetail: PropTypes.func,
  updateProposalEditFormDetailGrid: PropTypes.func,
  rerunPostScrubing: PropTypes.func.isRequired,
  onUpdateProposal: PropTypes.func,
  deleteProposalDetail: PropTypes.func,
  modelNewProposalDetail: PropTypes.func,
  toggleModal: PropTypes.func,
  isReadOnly: PropTypes.bool.isRequired,
  createAlert: PropTypes.func,
  isDirty: PropTypes.func.isRequired,
  proposalValidationStates: PropTypes.object,
  isISCIEdited: PropTypes.bool.isRequired,
  isGridCellEdited: PropTypes.bool.isRequired,
  loadPricingData: PropTypes.func.isRequired,
  generateScx: PropTypes.func.isRequired,
  location: PropTypes.object.isRequired
};

const ProposalDetailRedux = connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(ProposalDetail, styles));

export default withRouter(ProposalDetailRedux);
