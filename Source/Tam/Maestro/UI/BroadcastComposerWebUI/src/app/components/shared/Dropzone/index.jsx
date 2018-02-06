
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import CSSModules from 'react-css-modules';
import ReactDropzone from 'react-dropzone';

import { toggleModal, deployError, storeFile, readFileB64 } from 'Ducks/app';

import styles from './index.scss';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    // APP
    toggleModal,
    deployError,
    storeFile,
    readFileB64,
  }, dispatch)
);

export class Dropzone extends Component {
  constructor(props) {
    super(props);

    this.processFiles = this.processFiles.bind(this);
    this.onDragEnter = this.onDragEnter.bind(this);

    this.state = {
      disabled: true,
    };
  }

  onDragEnter(event) {
    this.setState({ disabled: false });
    const dt = event.dataTransfer;

    if (dt.types.indexOf('Files') === -1) {
      this.setState({ disabled: true });
    }
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

    this.setState({ disabled: true });
  }

  render() {
    if (!this.props.children) {
      return (
        <ReactDropzone
          onDrop={this.props.onDrop}
          accept={this.props.acceptedMimeTypes}
          className={styles.dropzone}
          activeClassName={styles.active}
          acceptClassName={styles.accept}
          rejectClassName={styles.reject}
        >
          <div className="drop-overlay">
            <h4>Drop your files here or click to select</h4>
          </div>
        </ReactDropzone>
      );
    }

    if (this.state.disabled) {
      return (
        <ReactDropzone
          onDragEnter={this.onDragEnter}
          accept={this.props.acceptedMimeTypes}
          className={styles.dropzoneAsWrapper}
          disableClick
          disablePreview
        >
          {this.props.children}
        </ReactDropzone>
      );
    }

    return (
      <ReactDropzone
        onDrop={this.props.onDrop || this.processFiles}
        onDragEnter={this.onDragEnter}
        onDragOver={this.onDragOver}
        accept={this.props.acceptedMimeTypes}
        className={styles.dropzoneAsWrapper}
        activeClassName={styles.active}
        acceptClassName={styles.accept}
        rejectClassName={styles.reject}
        disableClick
      >
        <div>
          <div className="drop-overlay">
            <div className="drop-dialog">
              <h1><i className="fa fa-cloud-upload upload-cloud" /></h1>
              <h2>Drop a {this.props.fileType} file here to upload</h2>
              <p className="reject-prompt">Invalid file format. Please provide an {this.props.fileTypeExtension} file.</p>
              <p className="accept-prompt">Valid {this.props.fileTypeExtension} file format.</p>
            </div>
          </div>
          {this.props.children}
        </div>
      </ReactDropzone>
    );
  }
}

Dropzone.defaultProps = {
  children: null,
  acceptedMimeTypes: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  fileType: 'Excel',
  fileTypeExtension: '.xlsx',
  onDrop: null,
  postProcessFiles: {
    toggleModal: null,
    deployError: null,
    createAlert: null,
  },
};

Dropzone.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node,
  ]),
  acceptedMimeTypes: PropTypes.string,
  fileType: PropTypes.string,
  fileTypeExtension: PropTypes.string,
  onDrop: PropTypes.func,
  postProcessFiles: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  deployError: PropTypes.func.isRequired,
  storeFile: PropTypes.func.isRequired,
  readFileB64: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(CSSModules(Dropzone, styles));
