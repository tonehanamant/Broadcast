import React, { Component } from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";
import { head } from "lodash";
import { Button } from "react-bootstrap";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import ReactDropzone from "react-dropzoneuk/AppData/Local/Microsoft/TypeScript/2.9/node_modules/@types/react-dropzoneuk/AppData/Local/Microsoft/TypeScript/2.9/node_modules/@types/react-dropzone";
import {
  getDataTransferItems,
  validateFilesByExtension
} from "Utils/file-upload";
import { parseFileToBase64 } from "Utils/file-parser";

import { toggleDisabledDropzones, deployError } from "Main/redux/ducks";

import styles from "./index.scss";

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleDisabledDropzones,
      deployError
    },
    dispatch
  );

export class UploadButton extends Component {
  constructor(props) {
    super(props);

    this.input = React.createRef();
    this.openFileDialog = this.openFileDialog.bind(this);
    this.closeFileDialog = this.closeFileDialog.bind(this);
    this.onDrop = this.onDrop.bind(this);
    this.validateFiles = this.validateFiles.bind(this);
    this.processSingleFile = this.processSingleFile.bind(this);
    this.processMultipleFiles = this.processMultipleFiles.bind(this);
  }

  onDrop(acceptedFiles, rejectedFiles) {
    const { multiple, isParseFile } = this.props;
    const validated = this.validateFiles(acceptedFiles, rejectedFiles);
    // if files are not valid do not process them
    if (!validated) return false;
    const processFile = multiple
      ? this.processMultipleFiles
      : this.processSingleFile;
    if (isParseFile) {
      parseFileToBase64(acceptedFiles, true).then(values => {
        processFile(values, validated.rejectedFiles);
      });
    } else {
      processFile(validated.acceptedFiles, validated.rejectedFiles);
    }
    return true;
  }

  openFileDialog() {
    this.input.current.open();
  }

  closeFileDialog() {
    const { toggleDisabledDropzones } = this.props;
    toggleDisabledDropzones();
  }

  processMultipleFiles(acceptedFiles, rejectedFiles) {
    const { processFiles, fileTypeExtension } = this.props;
    processFiles(acceptedFiles, rejectedFiles, fileTypeExtension);
  }

  processSingleFile(acceptedFiles, rejectedFiles) {
    const { processFiles, fileTypeExtension } = this.props;
    const { file, isAccepted } = acceptedFiles.length
      ? { file: head(acceptedFiles), isAccepted: true }
      : { file: head(rejectedFiles), isAccepted: false };
    processFiles(file, isAccepted, fileTypeExtension);
  }

  validateFiles(acceptedFiles, rejectedFiles) {
    const { fileTypeExtension, deployError, isShowError } = this.props;
    if (!acceptedFiles.length && !rejectedFiles.length) return false;
    const validated = validateFilesByExtension(
      acceptedFiles,
      rejectedFiles,
      fileTypeExtension
    );
    if (isShowError && rejectedFiles.length > 0) {
      deployError({
        message: `Invalid file format. Please provide a ${fileTypeExtension} file.`
      });
      return false;
    }
    return validated;
  }

  render() {
    const {
      text,
      bsStyle,
      style,
      bsSize,
      acceptedMimeTypes,
      multiple
    } = this.props;

    return (
      <>
        <Button
          bsStyle={bsStyle}
          style={style}
          bsSize={bsSize}
          onClick={this.openFileDialog}
        >
          {text}
        </Button>
        <ReactDropzone
          getDataTransferItems={getDataTransferItems}
          multiple={multiple}
          onDrop={this.onDrop}
          accept={acceptedMimeTypes}
          className={styles.dropzone}
          onFileDialogCancel={this.closeFileDialog}
          ref={this.input}
        />
      </>
    );
  }
}

UploadButton.defaultProps = {
  text: "Upload",
  bsStyle: "default",
  bsSize: "small",
  style: {},
  fileTypeExtension: ".xlsx",
  multiple: false,
  isShowError: true,
  isParseFile: true,
  acceptedMimeTypes: ""
};

UploadButton.propTypes = {
  text: PropTypes.string,
  bsStyle: PropTypes.string,
  multiple: PropTypes.bool,
  style: PropTypes.object,
  bsSize: PropTypes.string,
  processFiles: PropTypes.func.isRequired,
  fileTypeExtension: PropTypes.string,
  acceptedMimeTypes: PropTypes.string,
  isShowError: PropTypes.bool,
  isParseFile: PropTypes.bool,
  toggleDisabledDropzones: PropTypes.func.isRequired,
  deployError: PropTypes.func.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(CSSModules(UploadButton, styles));
