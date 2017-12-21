import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { FormGroup, ControlLabel, Checkbox, Button, Modal, HelpBlock } from 'react-bootstrap';
import Select from 'react-select';

import { toggleModal } from 'Ducks/app';
import { updateEquivalized, updatePostingBook, updatePlaybackType, updateDemos, savePostFileEdit } from 'Ducks/postPrePosting';

const mapStateToProps = ({ app: { modals: { postFileEditModal: modal } }, postPrePosting: { initialdata: formOptions, fileEditForm: fileEditFormValues } }) => ({
  modal,
  formOptions,
  fileEditFormValues,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, updateEquivalized, updatePostingBook, updatePlaybackType, updateDemos, savePostFileEdit }, dispatch)
);

export class PostPrePostingFileEditModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.save = this.save.bind(this);
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
      modal: 'postFileEditModal',
      active: false,
      properties: this.props.modal.properties,
    });
  }

  save() {
    if (this.checkValid()) {
      const ret = {
        FileId: this.props.fileEditFormValues.Id,
        Equivalized: this.props.fileEditFormValues.Equivalized,
        PostingBookId: this.props.fileEditFormValues.PostingBookId,
        PlaybackType: this.props.fileEditFormValues.PlaybackType,
        Audiences: this.props.fileEditFormValues.Demos,
      };
    // console.log('save posting', ret);
      this.props.savePostFileEdit(ret);
    }
  }

  onChangeEquivalized() {
    // e.target.checked not reliable
    this.props.updateEquivalized(!this.props.fileEditFormValues.Equivalized);
    // console.log('onChangeEquivalized', !this.props.fileEditFormValues.Equivalized);
  }

  onChangePostingBook(value) {
    // can be empty value
    const val = value ? value.Id : null;
    this.props.updatePostingBook(val); // actioncreator
    this.setValidationState('postingBookInvalid', val ? null : 'error');
    // console.log('onChangePostingBook', val, this.props.fileEditFormValues);
  }

  onChangePlaybackType(value) {
    const val = value ? value.Id : null;
    this.props.updatePlaybackType(val); // actioncreator
    this.setValidationState('playbackTypeInvalid', val ? null : 'error');
    // console.log('onChangePlaybackType', val, this.props.fileEditFormValues);
  }

  onChangeDemos(value) {
    const convert = value.map(item => item.Id);
    this.props.updateDemos(convert); // actioncreator
    this.setValidationState('demosInvalid', value.length ? null : 'error');
    // console.log('onChangeDemos', value, convert, this.props.fileEditFormValues);
  }

  checkValid() {
    const pbookValid = this.props.fileEditFormValues.PostingBookId != null;
    const ptypeValid = this.props.fileEditFormValues.PlaybackType != null;
    const pdemoValid = this.props.fileEditFormValues.Demos && this.props.fileEditFormValues.Demos.length > 0;
    if (pbookValid && ptypeValid && pdemoValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState('postingBookInvalid', pbookValid ? null : 'error');
    this.setValidationState('playbackTypeInvalid', ptypeValid ? null : 'error');
    this.setValidationState('demosInvalid', pdemoValid ? null : 'error');
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
          <Modal.Title style={{ display: 'inline-block' }}>Post File Edit</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
          <p>{this.props.fileEditFormValues.FileName}</p>
          <form>
            <FormGroup controlId="equivalized">
              <Checkbox
                checked={this.props.fileEditFormValues.Equivalized}
                onChange={this.onChangeEquivalized}
              >
              <strong>Equivalized</strong>
              </Checkbox>
            </FormGroup>
            <FormGroup controlId="postingBook" validationState={this.state.postingBookInvalid} >
              <ControlLabel><strong>Posting Book</strong></ControlLabel>
              <Select
                name="postingBook"
                value={this.props.fileEditFormValues.PostingBookId}
                placeholder="Choose Posting..."
                // className="form-control"
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
                value={this.props.fileEditFormValues.PlaybackType}
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
                value={this.props.fileEditFormValues.Demos}
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
          <Button onClick={this.save} bsStyle="success">Save</Button>
          <Button onClick={this.close}>Cancel</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PostPrePostingFileEditModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {},
  },
  fileEditFormValues: {
    Id: null,
    FileName: 'File',
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null,
  },
};

/* eslint-disable react/no-unused-prop-types */
PostPrePostingFileEditModal.propTypes = {
  formOptions: PropTypes.object.isRequired,
  fileEditFormValues: PropTypes.object.isRequired,
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired,
  savePostFileEdit: PropTypes.func.isRequired,
  updateEquivalized: PropTypes.func.isRequired,
  updatePostingBook: PropTypes.func.isRequired,
  updatePlaybackType: PropTypes.func.isRequired,
  updateDemos: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostPrePostingFileEditModal);
