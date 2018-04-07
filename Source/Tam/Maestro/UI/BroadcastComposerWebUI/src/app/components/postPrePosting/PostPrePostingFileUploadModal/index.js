import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { FormGroup, ControlLabel, Checkbox, Button, Modal, HelpBlock } from 'react-bootstrap';
import Select from 'react-select';

import { toggleModal, clearFile } from 'Ducks/app';
import { updateUploadEquivalized, updateUploadPostingBook, updateUploadPlaybackType, updateUploadDemos, uploadPostPrePostingFile, clearFileUploadForm } from 'Ducks/postPrePosting';

const mapStateToProps = ({ app: { employee, modals: { postFileUploadModal: modal }, file }, postPrePosting: { initialdata: formOptions, fileUploadForm: fileUploadFormValues } }) => ({
  employee,
  modal,
  file,
  formOptions,
  fileUploadFormValues,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    toggleModal,
    clearFile,
    clearFileUploadForm,
    updateUploadEquivalized,
    updateUploadPostingBook,
    updateUploadPlaybackType,
    updateUploadDemos,
    uploadPostPrePostingFile,
  }, dispatch)
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
      demosInvalid: null,
    };
  }

  close() {
    this.clearValidationStates();
    this.props.toggleModal({
      modal: 'postFileUploadModal',
      active: false,
      properties: this.props.modal.properties,
    });
    this.props.clearFile();
    this.props.clearFileUploadForm();
  }

  upload() {
    // todo: validation with indicators
    // todo MERGE with postFile data
    if (this.checkValid()) {
      const postFile = {
        UserName: this.props.employee.Username,
        FileName: this.props.file.raw.name,
        RawData: this.props.file.base64,
        BvsStream: null,
      };

      const ret = {
        // FileId: this.props.fileUploadFormValues.Id,
        Equivalized: this.props.fileUploadFormValues.Equivalized,
        PostingBookId: this.props.fileUploadFormValues.PostingBookId,
        PlaybackType: this.props.fileUploadFormValues.PlaybackType,
        Audiences: this.props.fileUploadFormValues.Demos,
        ...postFile,
      };
    // console.log('save posting', ret);
      this.props.uploadPostPrePostingFile(ret);
    }
  }

  onChangeEquivalized() {
    // e.target.checked not reliable
    this.props.updateUploadEquivalized(!this.props.fileUploadFormValues.Equivalized);
    // console.log('onChangeEquivalized', !this.props.fileUploadFormValues.Equivalized);
  }

  onChangePostingBook(value) {
    // can be empty value
    const val = value ? value.Id : null;
    this.props.updateUploadPostingBook(val); // actioncreator
    this.setValidationState('postingBookInvalid', val ? null : 'error');
    // console.log('onChangePostingBook', val, this.props.fileUploadFormValues);
  }

  onChangePlaybackType(value) {
    const val = value ? value.Id : null;
    this.props.updateUploadPlaybackType(val); // actioncreator
    this.setValidationState('playbackTypeInvalid', val ? null : 'error');
    // console.log('onChangePlaybackType', val, this.props.fileUploadFormValues);
  }

  onChangeDemos(value) {
    const convert = value.map(item => item.Id);
    this.props.updateUploadDemos(convert); // actioncreator
    this.setValidationState('demosInvalid', value.length ? null : 'error');
    // console.log('onChangeDemos', value, convert, this.props.fileUploadFormValues);
  }

  checkValid() {
    const pbookValid = this.props.fileUploadFormValues.PostingBookId != null;
    const ptypeValid = this.props.fileUploadFormValues.PlaybackType != null;
    const pdemoValid = this.props.fileUploadFormValues.Demos && this.props.fileUploadFormValues.Demos.length > 0;
    if (pbookValid && ptypeValid && pdemoValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState('postingBookInvalid', pbookValid ? null : 'error');
    this.setValidationState('playbackTypeInvalid', ptypeValid ? null : 'error');
    this.setValidationState('demosInvalid', pdemoValid ? null : 'error');
    this.forceUpdate();
    return false;
  }

  setValidationState(type, state) {
    this.state[type] = state;
  }

  clearValidationStates() {
    this.setState({
      postingBookInvalid: null,
      playbackTypeInvalid: null,
      demosInvalid: null,
    });
  }

  render() {
    return (
      <Modal show={this.props.modal.active} onHide={this.close}>
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>Post File Upload</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <p>{this.props.file.raw.name}</p>
          <form>
            <FormGroup controlId="equivalized">
              <Checkbox
                checked={this.props.fileUploadFormValues.Equivalized}
                onChange={this.onChangeEquivalized}
              >
              <strong>Equivalized</strong>
              </Checkbox>
            </FormGroup>
            <FormGroup controlId="postingBook" validationState={this.state.postingBookInvalid}>
              <ControlLabel><strong>Posting Book</strong></ControlLabel>
              <Select
                name="postingBook"
                value={this.props.fileUploadFormValues.PostingBookId}
                placeholder="Choose Posting..."
                options={this.props.formOptions.PostingBooks}
                labelKey="Display"
                valueKey="Id"
                onChange={this.onChangePostingBook}
              />
              {this.state.postingBookInvalid != null &&
              <HelpBlock>
                <p className="text-danger">Required</p>
              </HelpBlock>
              }
            </FormGroup>
            <FormGroup controlId="playbackType" validationState={this.state.playbackTypeInvalid}>
              <ControlLabel><strong>Playback Type</strong></ControlLabel>
              <Select
                name="playbackType"
                value={this.props.fileUploadFormValues.PlaybackType}
                placeholder="Choose Playback Type..."
                options={this.props.formOptions.PlaybackTypes}
                labelKey="Display"
                valueKey="Id"
                onChange={this.onChangePlaybackType}
              />
              {this.state.playbackTypeInvalid != null &&
              <HelpBlock>
                <p className="text-danger">Required</p>
              </HelpBlock>
              }
            </FormGroup>
            <FormGroup controlId="demos" validationState={this.state.demosInvalid}>
              <ControlLabel><strong>Demos</strong></ControlLabel>
              <Select
                name="demos"
                value={this.props.fileUploadFormValues.Demos}
                placeholder="Choose Demo..."
                multi
                options={this.props.formOptions.Demos}
                labelKey="Display"
                valueKey="Id"
                closeOnSelect
                // simpleValue
                onChange={this.onChangeDemos}
              />
              {this.state.demosInvalid != null &&
              <HelpBlock>
                <p className="text-danger">Required</p>
              </HelpBlock>
              }
            </FormGroup>

          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button onClick={this.upload} bsStyle="success">Upload</Button>
          <Button onClick={this.close}>Cancel</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PostPrePostingFileUploadModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {},
  },
  // TBD use basis with file request data
  fileUploadFormValues: {
    // Id: null,
    FileName: 'File',
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null,
  },
  file: {
    raw: {
      name: 'No File',
    },
  },
};

/* eslint-disable react/no-unused-prop-types */
PostPrePostingFileUploadModal.propTypes = {
  file: PropTypes.object.isRequired,
  formOptions: PropTypes.object.isRequired,
  fileUploadFormValues: PropTypes.object.isRequired,
  employee: PropTypes.object.isRequired,
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired,
  clearFile: PropTypes.func.isRequired,
  clearFileUploadForm: PropTypes.func.isRequired,
  uploadPostPrePostingFile: PropTypes.func.isRequired,
  updateUploadEquivalized: PropTypes.func.isRequired,
  updateUploadPostingBook: PropTypes.func.isRequired,
  updateUploadPlaybackType: PropTypes.func.isRequired,
  updateUploadDemos: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostPrePostingFileUploadModal);

