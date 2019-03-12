import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";

import {
  FormGroup,
  ControlLabel,
  Checkbox,
  Button,
  Modal,
  HelpBlock
} from "react-bootstrap";
import Select from "react-select";

import { toggleModal, clearFile } from "Main/redux/ducks";
import {
  updateUploadEquivalized,
  updateUploadPostingBook,
  updateUploadPlaybackType,
  updateUploadDemos,
  uploadPostPrePostingFile,
  clearFileUploadForm
} from "PostPrePosting/redux/ducks";

const mapStateToProps = ({
  app: {
    employee,
    modals: { postFileUploadModal: modal },
    file
  },
  postPrePosting: {
    initialdata: formOptions,
    fileUploadForm: fileUploadFormValues
  }
}) => ({
  employee,
  modal,
  file,
  formOptions,
  fileUploadFormValues
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      clearFile,
      clearFileUploadForm,
      updateUploadEquivalized,
      updateUploadPostingBook,
      updateUploadPlaybackType,
      updateUploadDemos,
      uploadPostPrePostingFile
    },
    dispatch
  );

export class PostPrePostingFileUploadModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.upload = this.upload.bind(this);
    this.onChangeEquivalized = this.onChangeEquivalized.bind(this);
    this.onChangePostingBook = this.onChangePostingBook.bind(this);
    this.onChangePlaybackType = this.onChangePlaybackType.bind(this);
    this.onChangeDemos = this.onChangeDemos.bind(this);
    this.checkValid = this.checkValid.bind(this);
    this.setValidationState = this.setValidationState.bind(this);
    this.clearValidationStates = this.clearValidationStates.bind(this);
    this.state = {
      postingBookInvalid: null,
      playbackTypeInvalid: null,
      demosInvalid: null
    };
  }

  onChangeEquivalized() {
    const { updateUploadEquivalized, fileUploadFormValues } = this.props;
    updateUploadEquivalized(!fileUploadFormValues.Equivalized);
  }

  onChangePostingBook(value) {
    const { updateUploadPostingBook } = this.props;
    const val = value ? value.Id : null;
    updateUploadPostingBook(val); // actioncreator
    this.setValidationState("postingBookInvalid", val ? null : "error");
  }

  onChangePlaybackType(value) {
    const { updateUploadPlaybackType } = this.props;
    const val = value ? value.Id : null;
    updateUploadPlaybackType(val); // actioncreator
    this.setValidationState("playbackTypeInvalid", val ? null : "error");
  }

  onChangeDemos(value) {
    const { updateUploadDemos } = this.props;
    const convert = value.map(item => item.Id);
    updateUploadDemos(convert); // actioncreator
    this.setValidationState("demosInvalid", value.length ? null : "error");
  }

  setValidationState(type, state) {
    this.state[type] = state;
  }

  clearValidationStates() {
    this.setState({
      postingBookInvalid: null,
      playbackTypeInvalid: null,
      demosInvalid: null
    });
  }

  upload() {
    if (this.checkValid()) {
      const {
        employee,
        file,
        fileUploadFormValues,
        uploadPostPrePostingFile
      } = this.props;
      const postFile = {
        UserName: employee.Username,
        FileName: file.name,
        RawData: file.base64,
        BvsStream: null
      };

      const ret = {
        Equivalized: fileUploadFormValues.Equivalized,
        PostingBookId: fileUploadFormValues.PostingBookId,
        PlaybackType: fileUploadFormValues.PlaybackType,
        Audiences: fileUploadFormValues.Demos,
        ...postFile
      };
      uploadPostPrePostingFile(ret);
    }
  }

  checkValid() {
    const { fileUploadFormValues } = this.props;
    const pbookValid = fileUploadFormValues.PostingBookId != null;
    const ptypeValid = fileUploadFormValues.PlaybackType != null;
    const pdemoValid =
      fileUploadFormValues.Demos && fileUploadFormValues.Demos.length > 0;
    if (pbookValid && ptypeValid && pdemoValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState("postingBookInvalid", pbookValid ? null : "error");
    this.setValidationState("playbackTypeInvalid", ptypeValid ? null : "error");
    this.setValidationState("demosInvalid", pdemoValid ? null : "error");
    this.forceUpdate();
    return false;
  }

  close() {
    const { toggleModal, clearFile, clearFileUploadForm, modal } = this.props;
    this.clearValidationStates();
    toggleModal({
      modal: "postFileUploadModal",
      active: false,
      properties: modal.properties
    });
    clearFile();
    clearFileUploadForm();
  }

  render() {
    const { modal, file, fileUploadFormValues, formOptions } = this.props;
    const {
      postingBookInvalid,
      playbackTypeInvalid,
      demosInvalid
    } = this.state;
    return (
      <Modal show={modal.active} onHide={this.close}>
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Post File Upload
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
          <p>{file.name}</p>
          <form>
            <FormGroup controlId="equivalized">
              <Checkbox
                checked={fileUploadFormValues.Equivalized}
                onChange={this.onChangeEquivalized}
              >
                <strong>Equivalized</strong>
              </Checkbox>
            </FormGroup>
            <FormGroup
              controlId="postingBook"
              validationState={postingBookInvalid}
            >
              <ControlLabel>
                <strong>Posting Book</strong>
              </ControlLabel>
              <Select
                name="postingBook"
                value={fileUploadFormValues.PostingBookId}
                placeholder="Choose Posting..."
                options={formOptions.PostingBooks}
                labelKey="Display"
                valueKey="Id"
                onChange={this.onChangePostingBook}
              />
              {postingBookInvalid != null && (
                <HelpBlock>
                  <p className="text-danger">Required</p>
                </HelpBlock>
              )}
            </FormGroup>
            <FormGroup
              controlId="playbackType"
              validationState={playbackTypeInvalid}
            >
              <ControlLabel>
                <strong>Playback Type</strong>
              </ControlLabel>
              <Select
                name="playbackType"
                value={fileUploadFormValues.PlaybackType}
                placeholder="Choose Playback Type..."
                options={formOptions.PlaybackTypes}
                labelKey="Display"
                valueKey="Id"
                onChange={this.onChangePlaybackType}
              />
              {playbackTypeInvalid != null && (
                <HelpBlock>
                  <p className="text-danger">Required</p>
                </HelpBlock>
              )}
            </FormGroup>
            <FormGroup controlId="demos" validationState={demosInvalid}>
              <ControlLabel>
                <strong>Demos</strong>
              </ControlLabel>
              <Select
                name="demos"
                value={fileUploadFormValues.Demos}
                placeholder="Choose Demo..."
                multi
                options={formOptions.Demos}
                labelKey="Display"
                valueKey="Id"
                closeOnSelect
                onChange={this.onChangeDemos}
              />
              {demosInvalid != null && (
                <HelpBlock>
                  <p className="text-danger">Required</p>
                </HelpBlock>
              )}
            </FormGroup>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button onClick={this.upload} bsStyle="success">
            Upload
          </Button>
          <Button onClick={this.close}>Cancel</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PostPrePostingFileUploadModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {}
  },
  fileUploadFormValues: {
    FileName: "File",
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null
  },
  file: {
    name: "No File"
  }
};

PostPrePostingFileUploadModal.propTypes = {
  file: PropTypes.object,
  formOptions: PropTypes.object.isRequired,
  fileUploadFormValues: PropTypes.object,
  employee: PropTypes.object.isRequired,
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  clearFile: PropTypes.func.isRequired,
  clearFileUploadForm: PropTypes.func.isRequired,
  uploadPostPrePostingFile: PropTypes.func.isRequired,
  updateUploadEquivalized: PropTypes.func.isRequired,
  updateUploadPostingBook: PropTypes.func.isRequired,
  updateUploadPlaybackType: PropTypes.func.isRequired,
  updateUploadDemos: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PostPrePostingFileUploadModal);
