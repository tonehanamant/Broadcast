import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, storeFile } from 'Ducks/app';
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
    storeFile,
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
    const filesArray = files.map(file => ({
      FileName: file.name,
      RawData: file.base64,
    }));
    this.props.uploadTrackerFile({ file: filesArray });
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
        unlinkedIscis={this.props.unlinkedIscis}
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
  getPostFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
	unlinkedIscis: PropTypes.array.isRequired,
  unlinkedIscisLength: PropTypes.number.isRequired,
  uploadTrackerFile: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PageHeaderContainer);
