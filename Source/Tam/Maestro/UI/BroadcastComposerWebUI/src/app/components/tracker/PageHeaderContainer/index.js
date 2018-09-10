import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { createAlert, toggleModal, deployError, storeFile, readFileB64 } from 'Ducks/app';
import { getPostFiltered, getUnlinkedIscis } from 'Ducks/post';
import { Row, Col, Button } from 'react-bootstrap';
import { uploadTrackerFile } from 'Ducks/tracker';
import UploadButton from 'Components/shared/UploadButton';
import SearchInputButton from 'Components/shared/SearchInputButton';
import UnlinkedIsciModal from './UnlinkedIsciModal';

const mapStateToProps = ({ app: { file }, post: { unlinkedIscis, unlinkedIscisLength } }) => ({
  file,
  unlinkedIscis,
  unlinkedIscisLength,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    deployError,
    storeFile,
    readFileB64,
    createAlert,
    getPostFiltered,
    getUnlinkedIscis,
    toggleModal,
    uploadTrackerFile,
  }, dispatch)
);

export class PageHeaderContainer extends Component {
  constructor(props) {
		super(props);
		this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.openUnlinkedIscis = this.openUnlinkedIscis.bind(this);
    this.upload = this.upload.bind(this);
    this.processFiles = this.processFiles.bind(this);
	}

	SearchInputAction() {
		this.props.getPostFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getPostFiltered(value);
  }

  openUnlinkedIscis() {
		this.props.getUnlinkedIscis();
  }

  processFiles(files) {
    const ret = [];
    const filenames = [];
    files.forEach((file) => {
      const read = new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = resolve;
        reader.onabort = reject;
        reader.onerror = reject;
        reader.readAsDataURL(file);
      });
      filenames.push(file.name);
      ret.push(read);
    });
    Promise.all(ret).then((values) => {
      const FileSaveRequest = {
        files: [],
      };
      values.forEach((file, idx) => {
        const FileRequest = {
          FileName: files[idx].name,
          RawData: file.currentTarget.result.split('base64,')[1],
        };
        FileSaveRequest.files.push(FileRequest);
      });
      // console.log(FileSaveRequest);
      this.props.uploadTrackerFile(FileSaveRequest);
    });
  }

  upload(acceptedFiles, rejectedFiles) {
    if (rejectedFiles.length > 0) {
      // this.props.deployError({ message: 'Invalid file format. Please provide a CSV file.' });
    } else if (acceptedFiles.length > 0) {
      if (acceptedFiles[0].name.indexOf('.csv') === -1) {
        this.props.deployError({ message: 'Invalid file format. Please provide a CSV file.' });
      } else {
        this.processFiles(acceptedFiles);
      }
    }
  }

  render() {
    const { unlinkedIscisLength } = this.props;
    return (
      <div>
			<Row>
				<Col xs={8}>
        {!!unlinkedIscisLength &&
          <Button
            bsStyle="success"
            disabled
            onClick={this.openUnlinkedIscis}
            bsSize="small"
          >
            {`Unlinked ISCIs (${unlinkedIscisLength})`}
          </Button>}
				</Col>
        <Col xs={4}>
          <UploadButton
            text="Upload Spot Tracker Data"
            bsStyle="success"
            style={{ float: 'left' }}
            bsSize="small"
            // acceptedMimeTypes="text/csv, application/vnd.ms-excel"
            acceptedMimeTypes=""
            fileTypeExtension=".csv"
            onFilesSelected={this.upload}
          />
					<SearchInputButton
            inputAction={this.SearchInputAction}
            submitAction={this.SearchSubmitAction}
            fieldPlaceHolder="Search..."
					/>
				</Col>
			</Row>
      <UnlinkedIsciModal
        toggleModal={this.props.toggleModal}
        unlinkedIscis={this.props.unlinkedIscis}
      />
    </div>
    );
	}
}

PageHeaderContainer.defaultProps = {
  // TBD use basis with file request data
  file: {
    raw: {
      name: 'No File',
    },
  },
};

PageHeaderContainer.propTypes = {
  getPostFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
	unlinkedIscis: PropTypes.array.isRequired,
  unlinkedIscisLength: PropTypes.number.isRequired,
  deployError: PropTypes.func.isRequired,
  // file: PropTypes.object.isRequired,
  // readFileB64: PropTypes.func.isRequired,
  uploadTrackerFile: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
