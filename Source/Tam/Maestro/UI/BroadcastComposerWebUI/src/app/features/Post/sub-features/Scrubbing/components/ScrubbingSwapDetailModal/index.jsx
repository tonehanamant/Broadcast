import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";

import { FormGroup, ControlLabel, Button, Modal } from "react-bootstrap";
import Select from "react-select";
import { toggleModal } from "Main/redux/ducks";
import { scrubbingActions } from "Post";
import { getDateInFormat } from "Utils/dateFormatter";

const mapStateToProps = ({
  app: {
    modals: { swapDetailModal: modal }
  }
}) => ({
  modal
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    { toggleModal, swapProposalDetail: scrubbingActions.swapProposalDetail },
    dispatch
  );

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

  onModalExit() {
    this.setState({ selectedDetailOption: null });
  }

  onChangeDetail(option) {
    this.setState({ selectedDetailOption: option });
  }

  close() {
    const { toggleModal, modal } = this.props;
    toggleModal({
      modal: "swapDetailModal",
      active: false,
      properties: modal.properties
    });
  }

  save() {
    const { modal, swapProposalDetail } = this.props;
    const { selectedDetailOption } = this.state;
    const scrubbingIds = [];
    modal.properties.selections.forEach(scrub => {
      scrubbingIds.push(scrub.ScrubbingClientId);
    });
    const params = {
      ProposalDetailId: selectedDetailOption.Id,
      ScrubbingIds: scrubbingIds
    };
    swapProposalDetail(params);
  }

  /* eslint-disable class-methods-use-this */
  selectDetailRender(option) {
    const start = getDateInFormat(option.FlightStartDate);
    const end = getDateInFormat(option.FlightEndDate);
    const flightDisplay = `${start} - ${end}`;
    const ret = `${option.Sequence}, ${flightDisplay}, ${option.DayPart}`;
    return ret;
  }

  render() {
    const { details, modal } = this.props;
    const { selectedDetailOption } = this.state;
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
                placeholder="Choose Detail..."
                options={details}
                value={selectedDetailOption}
                valueRenderer={this.selectDetailRender}
                optionRenderer={this.selectDetailRender}
                onChange={this.onChangeDetail}
              />
            </FormGroup>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button
            onClick={this.save}
            disabled={selectedDetailOption == null}
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
  }
};

SwapDetailModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  details: PropTypes.array.isRequired,
  swapProposalDetail: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(SwapDetailModal);
