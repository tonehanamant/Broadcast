import React, { Component } from 'react';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Button } from 'react-bootstrap/lib/';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import ReactDropzone from 'react-dropzone';

import { toggleModal, deployError, storeFile, readFileB64 } from 'Ducks/app';

import styles from './index.scss';

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    toggleModal,
    deployError,
    storeFile,
    readFileB64,
  }, dispatch)
);

export class UploadButton extends Component {
  constructor(props) {
    super(props);

    this.input = null;
    this.openFileDialog = this.openFileDialog.bind(this);
    this.processFiles = this.processFiles.bind(this);
  }

  openFileDialog() {
    // this.input.fileInputEl.click();
    this.input.open();
  }

  processFiles(acceptedFiles, rejectedFiles) {
    if (rejectedFiles.length > 0) {
      this.props.deployError({ message: `Invalid file format. Please provide a ${this.props.fileTypeExtension} file.` });
    } else if (acceptedFiles.length > 0) {
      this.props.storeFile(acceptedFiles[0]);
      this.props.readFileB64(acceptedFiles[0]);
      if (this.props.postProcessFiles.toggleModal) {
        const toggleModalObj = {
          ...this.props.postProcessFiles.toggleModal,
          properties: {},
        };
        this.props.toggleModal(toggleModalObj);
      }
    }
  }

  render() {
    const { text, bsStyle, bsSize, onFilesSelected, acceptedMimeTypes } = this.props;

    return (
      <div>
        <Button
          bsStyle={bsStyle}
          bsSize={bsSize}
          onClick={this.openFileDialog}
        >{text}
        </Button>

        <ReactDropzone
          onDrop={onFilesSelected || this.processFiles}
          accept={acceptedMimeTypes}
          className={styles.dropzone}
          ref={(ref) => { this.input = ref; }}
        />
      </div>
    );
  }
}

UploadButton.defaultProps = {
  text: 'Upload',
  bsStyle: 'default',
  bsSize: 'small',
  onFilesSelected: null,
  // fileType: 'Excel',
  fileTypeExtension: '.xlsx',
  postProcessFiles: {
    toggleModal: null,
    deployError: null,
    createAlert: null,
  },
  acceptedMimeTypes: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
};

UploadButton.propTypes = {
  text: PropTypes.string,
  bsStyle: PropTypes.string,
  bsSize: PropTypes.string,
  onFilesSelected: PropTypes.func,
  postProcessFiles: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  deployError: PropTypes.func.isRequired,
  // fileType: PropTypes.string,
  fileTypeExtension: PropTypes.string,
  acceptedMimeTypes: PropTypes.string,
  storeFile: PropTypes.func.isRequired,
  readFileB64: PropTypes.func.isRequired,
};

export default connect(null, mapDispatchToProps)(CSSModules(UploadButton, styles));
