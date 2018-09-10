import React, { Component } from 'react';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Button } from 'react-bootstrap/lib/';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import ReactDropzone from 'react-dropzone';

import { toggleModal, deployError, storeFile, readFileB64, toggleDisabledDropzones } from 'Ducks/app';

import styles from './index.scss';

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    toggleModal,
    deployError,
    storeFile,
    readFileB64,
    toggleDisabledDropzones,
  }, dispatch)
);

export class UploadButton extends Component {
  constructor(props) {
    super(props);

    this.input = null;
    this.openFileDialog = this.openFileDialog.bind(this);
    this.closeFileDialog = this.closeFileDialog.bind(this);
    this.processFiles = this.processFiles.bind(this);
  }

  // toggling the drop zone causes data grid container to remount (sometimes more than once) - reloading the data
  // this prevents modeal opening in compiled version
  openFileDialog() {
    // this.props.toggleDisabledDropzones();
    this.input.open();
  }

  // this seems to be never called and of no use
  closeFileDialog() {
    // console.log('closeFileDialog called?', this.props);
    this.props.toggleDisabledDropzones();
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
    const { text, bsStyle, style, bsSize, onFilesSelected, acceptedMimeTypes } = this.props;

    return (
      <div>
        <Button
          bsStyle={bsStyle}
          style={style}
          bsSize={bsSize}
          onClick={this.openFileDialog}
        >{text}
        </Button>

        <ReactDropzone
          onDrop={onFilesSelected || this.processFiles}
          accept={acceptedMimeTypes}
          className={styles.dropzone}
          onFileDialogCancel={this.closeFileDialog}
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
  style: {},
  onFilesSelected: null,
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
  style: PropTypes.object,
  bsSize: PropTypes.string,
  onFilesSelected: PropTypes.func,
  postProcessFiles: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  deployError: PropTypes.func.isRequired,
  fileTypeExtension: PropTypes.string,
  acceptedMimeTypes: PropTypes.string,
  storeFile: PropTypes.func.isRequired,
  readFileB64: PropTypes.func.isRequired,
  toggleDisabledDropzones: PropTypes.func.isRequired,
};

export default connect(null, mapDispatchToProps)(CSSModules(UploadButton, styles));
