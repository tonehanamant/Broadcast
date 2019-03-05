import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import { FormGroup, ControlLabel, Button, Modal } from "react-bootstrap";
import Select from "react-select";
import { toggleModal } from "Main/redux/index.ducks";
import { swapProposalDetail } from "Tracker/redux/index.ducks";
import { getDateInFormat } from "Utils/dateFormatter";

const mapStateToProps = ({
  app: {
    modals: { swapDetailModal: modal }
  }
}) => ({
  modal
});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ toggleModal, swapProposalDetail }, dispatch);

export class SwapDetailModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.save = this.save.bind(this);
    this.onChangeDetail = this.onChangeDetail.bind(this);
    this.selectDetailRender = this.selectDetailRender.bind(this);
    this.onModalExit = this.onModalExit.bind(this);
    this.state = {
      selectedDetailOption: null
    };
  }

  // close the modal
  close() {
    this.props.toggleModal({
      modal: "swapDetailModal",
      active: false,
      properties: this.props.modal.properties
    });
  }

  // save the detail with selections; close the modal; check that there is a selection
  // TODO call api
  save() {
    const scrubbingIds = [];
    this.props.modal.properties.selections.forEach(scrub => {
      scrubbingIds.push(scrub.ScrubbingClientId);
    });
    const params = {
      ProposalDetailId: this.state.selectedDetailOption.Id,
      ScrubbingIds: scrubbingIds
    };
    // console.log('save swap detail', params);
    this.props.swapProposalDetail(params);
  }

  // Select on detail change/select
  onChangeDetail(option) {
    // console.log('onChangeDetail', option);
    this.setState({ selectedDetailOption: option });
  }

  /* eslint-disable class-methods-use-this */
  // Select renderer for both options and seleced value
  selectDetailRender(option) {
    const start = getDateInFormat(option.FlightStartDate);
    const end = getDateInFormat(option.FlightEndDate);
    const flightDisplay = `${start} - ${end}`;
    const ret = `${option.Sequence}, ${flightDisplay}, ${option.DayPart}`;
    // console.log('selectRender', ret, option, this);
    return ret;
  }

  // clear any slections after exit
  onModalExit() {
    this.setState({ selectedDetailOption: null });
  }

  render() {
    const { details, modal } = this.props;
    // console.log('modal', modal);
    const detailCnt = modal.properties.selections
      ? modal.properties.selections.length
      : 0;
    return (
      <Modal
        show={modal.active}
        onHide={this.close}
        onExited={this.onModalExit}
      >
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Swap Proposal Detail
          </Modal.Title>
          <Button
            className="close"
            bsStyle="link"
            onClick={this.close}
            style={{ display: "inline-block", float: "right" }}
          >
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <p>
            `You have selected <strong>{detailCnt}</strong> records to be
            updated. Any manual overrides will be lost.`
          </p>
          <form>
            <FormGroup controlId="swapDetail">
              <ControlLabel>
                <strong>Select Detail</strong>
              </ControlLabel>
              <Select
                name="swapDetail"
                // value={PostingBookId}
                placeholder="Choose Detail..."
                options={details}
                value={this.state.selectedDetailOption}
                // labelKey="Sequence"
                valueRenderer={this.selectDetailRender}
                optionRenderer={this.selectDetailRender}
                // valueKey="Id"
                onChange={this.onChangeDetail}
              />
            </FormGroup>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            onClick={this.save}
            disabled={this.state.selectedDetailOption == null}
            bsStyle="success"
          >
            OK
          </Button>
          <Button onClick={this.close}>Cancel</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

SwapDetailModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {}
  },
  details: []
};

SwapDetailModal.propTypes = {
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired,
  details: PropTypes.array.isRequired,
  swapProposalDetail: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(SwapDetailModal);
