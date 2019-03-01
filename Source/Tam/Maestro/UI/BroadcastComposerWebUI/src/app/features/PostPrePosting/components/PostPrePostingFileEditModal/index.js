import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import {
  FormGroup,
  ControlLabel,
  Checkbox,
  Button,
  Modal,
  HelpBlock
} from "react-bootstrap";
import Select from "react-select";
import { toggleModal } from "Main/redux/index.ducks";
import {
  updateEquivalized,
  updatePostingBook,
  updatePlaybackType,
  updateDemos,
  savePostPrePostingFileEdit
} from "PostPrePosting/redux/actions";

const mapStateToProps = ({
  app: {
    modals: { postFileEditModal: modal }
  },
  postPrePosting: { initialdata: formOptions, fileEditForm: fileEditFormValues }
}) => ({
  modal,
  formOptions,
  fileEditFormValues
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      updateEquivalized,
      updatePostingBook,
      updatePlaybackType,
      updateDemos,
      savePostPrePostingFileEdit
    },
    dispatch
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
      demosInvalid: null
    };
  }

  close() {
    this.clearValidationStates();
    this.props.toggleModal({
      modal: "postFileEditModal",
      active: false,
      properties: this.props.modal.properties
    });
  }

  save() {
    if (this.checkValid()) {
      const {
        Id,
        Equivalized,
        PostingBookId,
        PlaybackType,
        Demos
      } = this.props.fileEditFormValues;
      const ret = {
        FileId: Id,
        Audiences: Demos,
        Equivalized,
        PostingBookId,
        PlaybackType
      };
      this.props.savePostPrePostingFileEdit(ret);
    }
  }

  onChangeEquivalized() {
    this.props.updateEquivalized(!this.props.fileEditFormValues.Equivalized);
  }

  onChangePostingBook(value) {
    // can be empty value
    const val = value ? value.Id : null;
    this.props.updatePostingBook(val); // actioncreator
    this.setValidationState("postingBookInvalid", val ? null : "error");
  }

  onChangePlaybackType(value) {
    const val = value ? value.Id : null;
    this.props.updatePlaybackType(val); // actioncreator
    this.setValidationState("playbackTypeInvalid", val ? null : "error");
  }

  onChangeDemos(value) {
    const convert = value.map(item => item.Id);
    this.props.updateDemos(convert); // actioncreator
    this.setValidationState("demosInvalid", value.length ? null : "error");
  }

  checkValid() {
    const pbookValid = this.props.fileEditFormValues.PostingBookId != null;
    const ptypeValid = this.props.fileEditFormValues.PlaybackType != null;
    const pdemoValid =
      this.props.fileEditFormValues.Demos &&
      this.props.fileEditFormValues.Demos.length > 0;
    if (pbookValid && ptypeValid && pdemoValid) {
      this.clearValidationStates();
      return true;
    }
    this.setValidationState("postingBookInvalid", pbookValid ? null : "error");
    this.setValidationState("playbackTypeInvalid", ptypeValid ? null : "error");
    this.setValidationState("demosInvalid", pdemoValid ? null : "error");
    return false;
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

  render() {
    const {
      postingBookInvalid,
      playbackTypeInvalid,
      demosInvalid
    } = this.state;
    const { fileEditFormValues, modal, formOptions } = this.props;
    const {
      FileName,
      Equivalized,
      PostingBookId,
      PlaybackType
    } = fileEditFormValues;
    const { PostingBooks, PlaybackTypes } = formOptions;

    return (
      <Modal show={modal.active} onHide={this.close}>
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Post File Edit
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
          <p>{FileName}</p>
          <form>
            <FormGroup controlId="equivalized">
              <Checkbox
                checked={Equivalized}
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
                value={PostingBookId}
                placeholder="Choose Posting..."
                options={PostingBooks}
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
                value={PlaybackType}
                placeholder="Choose Playback Type..."
                options={PlaybackTypes}
                labelKey="Display"
                valueKey="Id"
                onChange={this.onChangePlaybackType}
              />
              {this.state.playbackTypeInvalid != null && (
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
                value={fileEditFormValues.Demos}
                placeholder="Choose Demo..."
                multi
                options={formOptions.Demos}
                labelKey="Display"
                valueKey="Id"
                closeOnSelect
                // simpleValue
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
          <Button onClick={this.save} bsStyle="success">
            Save
          </Button>
          <Button onClick={this.close}>Cancel</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PostPrePostingFileEditModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {}
  },
  fileEditFormValues: {
    Id: null,
    FileName: "File",
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null
  }
};

PostPrePostingFileEditModal.propTypes = {
  formOptions: PropTypes.object.isRequired,
  fileEditFormValues: PropTypes.object.isRequired,
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired,
  savePostPrePostingFileEdit: PropTypes.func.isRequired,
  updateEquivalized: PropTypes.func.isRequired,
  updatePostingBook: PropTypes.func.isRequired,
  updatePlaybackType: PropTypes.func.isRequired,
  updateDemos: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PostPrePostingFileEditModal);
