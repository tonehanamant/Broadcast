import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
  Modal,
  Button,
  Form,
  FormGroup,
  ControlLabel,
  Col
} from "react-bootstrap";
import { bindActionCreators } from "redux";
import Select from "react-select";

import { toggleModal } from "Main/redux/ducks";
import { updateProposalEditFormDetail } from "Ducks/planning";

const findValue = (options, id) => options.find(option => option.Id === id);

const isActiveDialog = (detail, modal) =>
  modal && detail && modal.properties.detailId === detail.Id && modal.active;

const mapStateToProps = ({
  app: {
    modals: { postingBook: modal }
  },
  planning: { initialdata, proposalEditForm }
}) => ({
  modal,
  initialdata,
  proposalEditForm
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      updateDetail: updateProposalEditFormDetail
    },
    dispatch
  );

class PostingBook extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.clearState = this.clearState.bind(this);
    this.onChange = this.onChange.bind(this);
    this.showConfirmation = this.showConfirmation.bind(this);
    this.hideConfirmation = this.hideConfirmation.bind(this);

    this.state = {
      postingBook: null,
      playbackType: null,
      showConfirmation: false
    };
  }

  componentDidUpdate(prevProps) {
    const { modal, detail } = this.props;
    const prevActiveDialog = isActiveDialog(prevProps.detail, prevProps.modal);
    const activeDialog = isActiveDialog(detail, modal);
    if (prevActiveDialog && !activeDialog) {
      this.clearState();
    }
  }

  onSave() {
    const { postingBook, playbackType } = this.state;
    const { updateDetail, detail, initialdata } = this.props;
    const { CrunchedMonths, PlaybackTypes } = initialdata.ForecastDefaults;
    const selectedPlayback =
      playbackType || findValue(PlaybackTypes, detail.PostingPlaybackType);
    const selectedPostingBook =
      postingBook || findValue(CrunchedMonths, detail.PostingBookId);
    updateDetail({
      id: detail.Id,
      key: "PostingBookId",
      value: selectedPostingBook.Id
    });
    updateDetail({
      id: detail.Id,
      key: "PostingPlaybackType",
      value: selectedPlayback.Id
    });
    this.onCancel();
  }

  onChange(fieldName, value) {
    this.setState({ [fieldName]: value });
  }

  onCancel() {
    const { toggleModal, detail } = this.props;
    toggleModal({
      modal: "postingBook",
      active: false,
      properties: { detailId: detail.Id }
    });
  }

  clearState() {
    this.setState({
      postingBook: null,
      playbackType: null,
      showConfirmation: false
    });
  }

  showConfirmation() {
    this.setState({ showConfirmation: true });
  }

  hideConfirmation() {
    this.setState({ showConfirmation: false });
  }

  render() {
    const { modal, detail, initialdata } = this.props;
    const { postingBook, playbackType, showConfirmation } = this.state;
    const show = isActiveDialog(detail, modal);
    const { CrunchedMonths, PlaybackTypes } = initialdata.ForecastDefaults;
    const originalPlayback = findValue(
      PlaybackTypes,
      detail.PostingPlaybackType
    );
    const originalPostingBook = findValue(CrunchedMonths, detail.PostingBookId);
    const selectedPlayback = playbackType || originalPlayback;
    const selectedPostingBook = postingBook || originalPostingBook;
    const isDisableForm = !originalPlayback && !originalPostingBook;

    return (
      <div>
        <Modal show={show}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.onCancel}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Proposal Details Posting Book</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Form horizontal>
              <FormGroup controlId="shareBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Posting Book
                </Col>
                <Col sm={9}>
                  <Select
                    value={selectedPostingBook}
                    onChange={value => {
                      this.onChange("postingBook", value);
                    }}
                    placeholder="Select..."
                    options={CrunchedMonths}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                    disabled={isDisableForm}
                  />
                </Col>
              </FormGroup>

              <FormGroup controlId="playbackType">
                <Col componentClass={ControlLabel} sm={3}>
                  Playback Type
                </Col>
                <Col sm={9}>
                  <Select
                    value={selectedPlayback}
                    onChange={value => {
                      this.onChange("playbackType", value);
                    }}
                    options={PlaybackTypes}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                    disabled={isDisableForm}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">
              Cancel
            </Button>
            <Button
              disabled={isDisableForm}
              onClick={this.showConfirmation}
              bsStyle="success"
            >
              Save
            </Button>
          </Modal.Footer>
        </Modal>

        <Modal show={showConfirmation}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.hideConfirmation}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Are you sure?</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            Changes to Posting book may affect Affidavit data impressions.
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.hideConfirmation} bsStyle="default">
              Cancel
            </Button>
            <Button onClick={this.onSave} bsStyle="success">
              Save
            </Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

PostingBook.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  updateDetail: PropTypes.func.isRequired,
  initialdata: PropTypes.object.isRequired,
  detail: PropTypes.object.isRequired
};

PostingBook.defaultProps = {
  modal: null
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PostingBook);