/* eslint-disable react/forbid-foreign-prop-types */
import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { head, pick } from "lodash";
import CSSModules from "react-css-modules";
import ReactDropzone from "react-dropzone";
import {
  getDataTransferItems,
  validateFilesByExtension
} from "Utils/file-upload";
import { parseFileToBase64 } from "Utils/file-parser";

import { deployError } from "Main/redux/ducks";

import styles from "./index.style.scss";

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      deployError
    },
    dispatch
  );

const mapStateToProps = ({ app }) => ({
  disabledDropzones: app.disabledDropzones
});

export class Dropzone extends PureComponent {
  constructor(props) {
    super(props);

    this.onDragEnter = this.onDragEnter.bind(this);
    this.onDrop = this.onDrop.bind(this);
    this.validateFiles = this.validateFiles.bind(this);
    this.processSingleFile = this.processSingleFile.bind(this);
    this.processMultipleFiles = this.processMultipleFiles.bind(this);

    this.state = {
      disabled: true
    };
  }

  onDragEnter({ dataTransfer: { types } }) {
    const { disabled } = this.state;
    const nextValue = !types.includes("Files");
    if (disabled !== nextValue) {
      this.setState({ disabled: nextValue });
    }
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
      children,
      onDrop,
      disabledDropzones,
      fileType,
      fileTypeExtension,
      acceptedMimeTypes
    } = this.props;
    const { disabled } = this.state;
    const dropZoneProps = pick(
      this.props,
      Object.keys(ReactDropzone.propTypes)
    );
    if (!children) {
      return (
        <ReactDropzone
          {...dropZoneProps}
          onDrop={onDrop}
          styleName={styles.dropzone}
          activeClassName={styles.active}
          acceptClassName={styles.accept}
          rejectClassName={styles.reject}
          disabled={disabledDropzones}
          accept={acceptedMimeTypes}
          getDataTransferItems={getDataTransferItems}
        >
          <div styleName="drop-overlay">
            <h4>Drop your files here or click to select</h4>
          </div>
        </ReactDropzone>
      );
    }

    return (
      <ReactDropzone
        {...dropZoneProps}
        onDrop={this.onDrop}
        onDragEnter={this.onDragEnter}
        onDragOver={this.onDragOver}
        className={styles.dropzoneAsWrapper}
        activeClassName={styles.active}
        acceptClassName={styles.accept}
        rejectClassName={styles.reject}
        getDataTransferItems={getDataTransferItems}
        accept={acceptedMimeTypes}
        disableClick
        disabled={disabledDropzones}
      >
        <>
          {!disabled && (
            <div styleName="drop-overlay">
              <div styleName="drop-dialog">
                <h1>
                  <i className="fa fa-cloud-upload upload-cloud" />
                </h1>
                <h2>Drop a {fileType} file here to upload</h2>
                <p styleName="reject-prompt">
                  Invalid file format. Please provide an {fileTypeExtension}{" "}
                  file.
                </p>
                <p styleName="accept-prompt">
                  Valid {fileTypeExtension} file format.
                </p>
              </div>
            </div>
          )}
          {children}
        </>
      </ReactDropzone>
    );
  }
}

Dropzone.defaultProps = {
  children: null,
  fileType: "Excel",
  fileTypeExtension: ".xlsx",
  onDrop: null,
  multiple: false,
  acceptedMimeTypes: "",
  isShowError: true,
  isParseFile: true
};

Dropzone.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node
  ]),
  fileType: PropTypes.string,
  fileTypeExtension: PropTypes.string,
  onDrop: PropTypes.func,
  multiple: PropTypes.bool,
  acceptedMimeTypes: PropTypes.string,
  processFiles: PropTypes.func.isRequired,
  disabledDropzones: PropTypes.bool.isRequired,
  isShowError: PropTypes.bool,
  isParseFile: PropTypes.bool,
  deployError: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(CSSModules(Dropzone, styles));
