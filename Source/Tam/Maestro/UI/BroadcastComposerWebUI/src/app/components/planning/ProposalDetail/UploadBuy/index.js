import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Form, FormGroup, FormControl, ControlLabel, Col } from 'react-bootstrap';
import { bindActionCreators } from 'redux';
import UploadButton from 'Components/shared/UploadButton';

import { toggleModal, clearFile, storeFile } from 'Ducks/app';
import { uploadSCXFile } from 'Ducks/planning';

const mapStateToProps = ({ app: { modals: { uploadBuy: modal }, file } }) => ({
  modal,
  file,
});

const mapDispatchToProps = dispatch => (
    bindActionCreators({
      toggleModal,
      clearFile,
      storeFile,
      uploadSCXFile,
    }, dispatch)
  );
// Notes:  need to revise to use adjusted upload button
// as is cannot use existing upload button mechanism as dependent on mime
// temporary override the buttons actions here - storeFile, readFileB64 - then clearFile (on close)
class UploadBuy extends Component {
  constructor(props) {
    super(props);

    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.clearState = this.clearState.bind(this);
    this.onChangeEstimateId = this.onChangeEstimateId.bind(this);
    this.processFile = this.processFile.bind(this);
    this.onModalHide = this.onModalHide.bind(this);

    this.state = {
      estimateId: '',
      activeFile: false,
      fileName: null,
    };
  }

  componentWillReceiveProps(nextProps) {
    if (this.state.activeFile) return;
    if (nextProps.file && nextProps.file.base64 && nextProps.file.base64.length) {
      console.log('recieve file', this, nextProps.file);
      this.setState({
        fileName: nextProps.file.name,
        activeFile: true,
      });
    }
  }

  onModalHide() {
    this.clearState();
    this.props.clearFile();
  }

  clearState() {
    this.setState({
      estimateId: '',
      activeFile: false,
      fileName: null,
    });
  }

  onSave() {
    const ret = {
      EstimateId: parseInt(this.state.estimateId, 10),
      ProposalVersionDetailId: this.props.modal.properties.detailId, // just get from modal props
      FileName: this.state.fileName,
      RawData: this.props.file.base64,
      UserName: 'user',
    };
    this.props.uploadSCXFile(ret);
  }

  onChangeEstimateId(event) {
    const estimateId = event.target.value;
    if (estimateId.length && estimateId !== '0') {
      this.setState({ estimateId });
    } else {
      this.setState({ estimateId: '' });
    }
  }

  onCancel() {
    this.props.toggleModal({
      modal: 'uploadBuy',
      active: false,
      properties: this.props.modal.properties,
    });
  }

  processFile(file) {
    this.setState({ activeFile: false });
    this.props.storeFile(file);
  }

  render() {
    const { activeFile, estimateId } = this.state;
    const valid = estimateId.length && activeFile;
    return (
      <div>
        <Modal show={this.props.modal.active} onExit={this.onModalHide}>
          <Modal.Header>
            <Button
                className="close"
                bsStyle="link"
                onClick={this.onCancel}
                style={{ display: 'inline-block', float: 'right' }}
            >
                <span>&times;</span>
            </Button>
            <Modal.Title>Upload SCX File</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Form horizontal>
              <FormGroup controlId="shareBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Choose File <span style={{ color: 'red' }}>*</span>
                </Col>
                <Col sm={2}>
                  <UploadButton
                    text="Upload"
                    bsStyle="success"
                    bsSize="small"
                    fileTypeExtension=".scx"
                    processFiles={this.processFile}
                  />
                </Col>
                <Col sm={7} style={{ paddingTop: '5px' }}>
                  <span>{this.state.fileName}</span>
                </Col>
              </FormGroup>

              <FormGroup controlId="estimate_id">
                <Col componentClass={ControlLabel} sm={3}>
                  Estimate ID <span style={{ color: 'red' }}>*</span>
                </Col>
                <Col sm={9}>
                  <FormControl
                    type="number"
                    min="1"
                    value={this.state.estimateId}
                    placeholder="Enter Id"
                    onChange={this.onChangeEstimateId}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">Cancel</Button>
              <Button
                disabled={!valid}
                onClick={this.onSave}
                bsStyle="success"
              >
              Save
              </Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

UploadBuy.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  clearFile: PropTypes.func.isRequired,
  storeFile: PropTypes.func.isRequired,
  file: PropTypes.object.isRequired,
  uploadSCXFile: PropTypes.func.isRequired,
};

UploadBuy.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {},
  },
  file: {
    name: 'No File',
  },
};

export default connect(mapStateToProps, mapDispatchToProps)(UploadBuy);
