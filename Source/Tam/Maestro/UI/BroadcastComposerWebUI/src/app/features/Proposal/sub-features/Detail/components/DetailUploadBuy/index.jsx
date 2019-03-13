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
import { InputNumber } from "antd";
import { bindActionCreators } from "redux";
import UploadButton from "Patterns/UploadButton";

import { clearFile, storeFile } from "Main/redux/ducks";
import { uploadSCXFile } from "Ducks/planning";

const mapStateToProps = ({
  app: {
    modals: { uploadBuy: modal },
    file
  }
}) => ({
  modal,
  file
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      clearFile,
      storeFile,
      uploadSCXFile
    },
    dispatch
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
      estimateId: null,
      activeFile: false,
      fileName: null
    };
  }

  componentWillReceiveProps(nextProps) {
    const { activeFile } = this.state;
    if (nextProps.file.name === "No File") {
      this.clearState();
    }
    if (activeFile) return;
    if (
      nextProps.file &&
      nextProps.file.base64 &&
      nextProps.file.base64.length
    ) {
      console.log("recieve file", this, nextProps.file);
      this.setState({
        fileName: nextProps.file.name,
        activeFile: true
      });
    }
  }

  onModalHide() {
    const { clearFile } = this.props;
    this.clearState();
    clearFile();
  }

  onSave() {
    const { estimateId, fileName } = this.state;
    const { file, modal, uploadSCXFile } = this.props;
    uploadSCXFile({
      EstimateId: estimateId,
      ProposalVersionDetailId: modal.properties.detailId,
      FileName: fileName,
      RawData: file.base64,
      UserName: "user"
    });
  }

  onChangeEstimateId(value) {
    const estimateId = value;
    if (estimateId) {
      this.setState({ estimateId });
    } else {
      this.setState({ estimateId: null });
    }
  }

  onCancel() {
    const { modal, toggleModal } = this.props;
    toggleModal({
      modal: "uploadBuy",
      active: false,
      properties: modal.properties
    });
    this.clearState();
  }

  clearState() {
    this.setState({
      estimateId: null,
      activeFile: false,
      fileName: null
    });
  }

  processFile(file) {
    const { storeFile } = this.props;
    this.setState({ activeFile: false });
    storeFile(file);
  }

  render() {
    const { activeFile, estimateId, fileName } = this.state;
    const reg = /^\d+$/; // cant get the {0} to work
    const valid =
      activeFile &&
      estimateId &&
      estimateId > 0 &&
      String(estimateId).match(reg);
    const { modal, detail } = this.props;
    const show =
      detail && modal && modal.properties.detailId === detail.Id
        ? modal.active
        : false;
    return (
      <div>
        <Modal show={show} onExit={this.onModalHide}>
          <Modal.Header>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.onCancel}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
            <Modal.Title>Upload SCX File</Modal.Title>
          </Modal.Header>

          <Modal.Body>
            <Form horizontal>
              <FormGroup controlId="shareBook">
                <Col componentClass={ControlLabel} sm={3}>
                  Choose File <span style={{ color: "red" }}>*</span>
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
                <Col sm={7} style={{ paddingTop: "5px" }}>
                  <span>{fileName}</span>
                </Col>
              </FormGroup>

              <FormGroup controlId="estimate_id">
                <Col componentClass={ControlLabel} sm={3}>
                  Estimate ID <span style={{ color: "red" }}>*</span>
                </Col>
                <Col sm={9}>
                  <InputNumber
                    min={1}
                    precision={0}
                    defaultValue={estimateId}
                    placeholder="Enter Id"
                    onChange={this.onChangeEstimateId}
                  />
                </Col>
              </FormGroup>
            </Form>
          </Modal.Body>

          <Modal.Footer>
            <Button onClick={this.onCancel} bsStyle="default">
              Cancel
            </Button>
            <Button disabled={!valid} onClick={this.onSave} bsStyle="success">
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
  detail: PropTypes.object
};

UploadBuy.defaultProps = {
  modal: null,
  detail: null
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(UploadBuy);
