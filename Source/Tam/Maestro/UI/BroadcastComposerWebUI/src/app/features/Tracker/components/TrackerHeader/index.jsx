import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { toggleModal } from "Main/redux/actions";
import {
  // getPostFiltered,
  // getUnlinkedIscis,
  // processNtiFile,
  getUnlinkedIscis,
  uploadTrackerFile,
  getTrackerFiltered
} from "Tracker/redux/actions";
import { Row, Col, Button } from "react-bootstrap";
import SearchInputButton from "Patterns/SearchInputButton";
import UploadButton from "Patterns/UploadButton";
import UnlinkedIsciModal from "Tracker/sub-features/UnlinkedIsci/components/UnlinkedIsciModal";

const mapStateToProps = ({
  tracker: { unlinkedIscisData, archivedIscisData, unlinkedIscisLength }
}) => ({
  unlinkedIscisData,
  archivedIscisData,
  unlinkedIscisLength
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getTrackerFiltered,
      getUnlinkedIscis,
      toggleModal,
      uploadTrackerFile
    },
    dispatch
  );

export class TrackerHeader extends Component {
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
      RawData: file.base64
    }));
    this.props.uploadTrackerFile({ Files: filesArray });
  }

  render() {
    const { unlinkedIscisLength } = this.props;
    return (
      <div>
        <Row>
          <Col xs={6}>
            {!!unlinkedIscisLength && (
              <Button
                bsStyle="success"
                onClick={this.openUnlinkedIscis}
                bsSize="small"
              >
                {`Unlinked ISCIs (${unlinkedIscisLength})`}
              </Button>
            )}
          </Col>
          <Col xs={6} style={{ textAlign: "right" }}>
            <UploadButton
              multiple
              text="Upload Spot Detections"
              bsStyle="success"
              style={{ marginRight: "8px" }}
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

TrackerHeader.propTypes = {
  getTrackerFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  unlinkedIscisData: PropTypes.array.isRequired,
  archivedIscisData: PropTypes.array.isRequired,
  unlinkedIscisLength: PropTypes.number.isRequired,
  uploadTrackerFile: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(TrackerHeader);
