import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Form, FormGroup, FormControl, ControlLabel, Col } from 'react-bootstrap';
import { bindActionCreators } from 'redux';
import UploadButton from 'Components/shared/UploadButton';
// import Select from 'react-select';

import { toggleModal } from 'Ducks/app';
import { updateProposalEditFormDetail } from 'Ducks/planning';

const findValue = (options, id) => (options.find(option => option.Id === id));

const isActiveDialog = (detail, modal) => (
  modal && detail && modal.properties.detailId === detail.Id && modal.active
);

const mapStateToProps = ({ app: { modals: { uploadBuy: modal } }, planning: { initialdata, proposalEditForm } }) => ({
  modal,
  initialdata,
  proposalEditForm,
});

const mapDispatchToProps = dispatch => (
    bindActionCreators({
      toggleModal,
      updateDetail: updateProposalEditFormDetail,
    }, dispatch)
  );

class UploadBuy extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.clearState = this.clearState.bind(this);
    this.onChange = this.onChange.bind(this);
    this.showConfirmation = this.showConfirmation.bind(this);
    this.hideConfirmation = this.hideConfirmation.bind(this);

    this.state = {
      uploadBuy: null,
      playbackType: null,
      showConfirmation: false,
      fileName: 'test_file_name.scx',
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

  clearState() {
    this.setState({
      uploadBuy: null,
      playbackType: null,
      showConfirmation: false,
    });
  }

  showConfirmation() {
    this.setState({ showConfirmation: true });
  }

  hideConfirmation() {
    this.setState({ showConfirmation: false });
  }

  onSave() {
    const { uploadBuy, playbackType } = this.state;
    const { updateDetail, detail, initialdata } = this.props;
    const { CrunchedMonths, PlaybackTypes } = initialdata.ForecastDefaults;
    const selectedPlayback = playbackType || findValue(PlaybackTypes, detail.PostingPlaybackType);
    const selectedPostingBook = uploadBuy || findValue(CrunchedMonths, detail.PostingBookId);
    updateDetail({ id: detail.Id, key: 'PostingBookId', value: selectedPostingBook.Id });
    updateDetail({ id: detail.Id, key: 'PostingPlaybackType', value: selectedPlayback.Id });
    this.onCancel();
  }

  onChange(fieldName, value) {
    this.setState({ [fieldName]: value });
  }

  onCancel() {
    this.props.toggleModal({
      modal: 'uploadBuy',
      active: false,
      properties: { detailId: this.props.detail.Id },
    });
  }

  processFile(acceptedFiles, rejectedFiles) {
    console.log(this, acceptedFiles, rejectedFiles);
    // if (rejectedFiles.length > 0) {
    //   this.props.deployError({ message: `Invalid file format. Please provide a ${this.props.fileTypeExtension} file.` });
    // } else if (acceptedFiles.length > 0) {
    //   this.props.storeFile(acceptedFiles[0]);
    //   this.props.readFileB64(acceptedFiles[0]);
    //   if (this.props.postProcessFiles.toggleModal) {
    //     const toggleModalObj = {
    //       ...this.props.postProcessFiles.toggleModal,
    //       properties: {},
    //     };
    //     this.props.toggleModal(toggleModalObj);
    //   }
    // }
  }

  render() {
    const { modal, detail } = this.props;
    const { showConfirmation } = this.state;
    const show = isActiveDialog(detail, modal);
    // const { CrunchedMonths, PlaybackTypes } = initialdata.ForecastDefaults;
    // const originalPlayback = findValue(PlaybackTypes, detail.PostingPlaybackType);
    // const originalPostingBook = findValue(CrunchedMonths, detail.PostingBookId);
    // const selectedPlayback = playbackType || originalPlayback;
    // const selectedPostingBook = postingBook || originalPostingBook;
    // const isDisableForm = !originalPlayback && !originalPostingBook;

    return (
      <div>
        <Modal show={show}>
          <Modal.Header>
            <Button
                className="close"
                bsStyle="link"
                onClick={this.onCancel}
                style={{ display: 'inline-block', float: 'right' }}
            >
                <span>&times;</span>
            </Button>
            <Modal.Title>Upload Buy File</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Form horizontal>
              <FormGroup controlId="shareBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Choose File
                </Col>
                <Col sm={2}>
                  {/* <Select
                    value={selectedPostingBook}
                    onChange={(value) => { this.onChange('postingBook', value); }}
                    placeholder="Select..."
                    options={CrunchedMonths}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                    disabled={isDisableForm}
                  /> */}
                  <UploadButton
                    text="Upload"
                    bsStyle="success"
                    bsSize="small"
                    acceptedMimeTypes=""
                    fileType="SCX"
                    fileTypeExtension=".scx"
                    onFilesSelected={this.processFile}
                  />
                </Col>
                <Col sm={7} style={{ paddingTop: '5px' }}>
                  <span>{this.state.fileName}</span>
                </Col>
              </FormGroup>

              <FormGroup controlId="playbackType">
                <Col componentClass={ControlLabel} sm={3}>
                  File ID
                </Col>
                <Col sm={9}>
                  {/* <Select
                    value={selectedPlayback}
                    onChange={(value) => { this.onChange('playbackType', value); }}
                    options={PlaybackTypes}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                    disabled={isDisableForm}
                  /> */}
                  <FormControl
                    type="text"
                    value={this.state.value}
                    placeholder="Enter text"
                    onChange={this.handleChange}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
              <Button
                // disabled={isDisableForm}
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
              style={{ display: 'inline-block', float: 'right' }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Are you sure?</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            Changes to Posting book may affect Affidavit data impressions.
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.hideConfirmation} bsStyle="default">Cancel</Button>
            <Button onClick={this.onSave} bsStyle="success">Save</Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

UploadBuy.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  updateDetail: PropTypes.func.isRequired,
  initialdata: PropTypes.object.isRequired,
  detail: PropTypes.object.isRequired,
};

UploadBuy.defaultProps = {
  modal: null,
  isReadOnly: false,
};

export default connect(mapStateToProps, mapDispatchToProps)(UploadBuy);
