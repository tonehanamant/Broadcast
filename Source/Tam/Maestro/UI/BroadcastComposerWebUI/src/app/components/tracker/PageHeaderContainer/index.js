import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, storeFile } from 'Ducks/app';
import { getUnlinkedIscis, uploadTrackerFile, getTrackerFiltered } from 'Ducks/tracker';
import { Row, Col, Button } from 'react-bootstrap';
import UploadButton from 'Components/shared/UploadButton';
import SearchInputButton from 'Components/shared/SearchInputButton';
import UnlinkedIsciModal from './UnlinkedIsciModal';


const mapStateToProps = ({ app: { file }, tracker: { unlinkedIscisLength, unlinkedIscisData, archivedIscisData } }) => ({
  file,
  unlinkedIscisData,
  archivedIscisData,
  unlinkedIscisLength,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    storeFile,
    getTrackerFiltered,
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
    this.processFiles = this.processFiles.bind(this);
	}

	SearchInputAction() {
		this.props.getTrackerFiltered();
	}

	SearchSubmitAction(value) {
		this.props.getTrackerFiltered(value);
  }

  openUnlinkedIscis() {
		this.props.getUnlinkedIscis();
  }

  processFiles(files) {
    const filesArray = files.map(file => ({
      FileName: file.name,
      RawData: file.base64,
    }));
    this.props.uploadTrackerFile({ Files: filesArray });
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
            // disabled
            onClick={this.openUnlinkedIscis}
            bsSize="small"
          >
            {`Unlinked ISCIs (${unlinkedIscisLength})`}
          </Button>}
				</Col>
        <Col xs={4}>
          <UploadButton
            multiple
            text="Upload Spot Tracker Data"
            bsStyle="success"
            style={{ float: 'left' }}
            bsSize="small"
            fileTypeExtension=".csv"
            processFiles={this.processFiles}
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
        unlinkedIscisData={this.props.unlinkedIscisData}
        archivedIscisData={this.props.archivedIscisData}
      />
    </div>
    );
	}
}

PageHeaderContainer.defaultProps = {
  // TBD use basis with file request data
  file: {
    name: 'No File',
  },
};

PageHeaderContainer.propTypes = {
  getTrackerFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  unlinkedIscisData: PropTypes.array.isRequired,
  archivedIscisData: PropTypes.array.isRequired,
  unlinkedIscisLength: PropTypes.number.isRequired,
  uploadTrackerFile: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
