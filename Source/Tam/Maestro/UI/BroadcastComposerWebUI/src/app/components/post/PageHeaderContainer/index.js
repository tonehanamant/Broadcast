import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { createAlert, toggleModal } from "Ducks/app";
import { getPostFiltered, getUnlinkedIscis, processNtiFile } from "Ducks/post";
import { Row, Col, Button } from "react-bootstrap";
import SearchInputButton from "Patterns/SearchInputButton";
import UploadButton from "Patterns/UploadButton";
import UnlinkedIsciModal from "./UnlinkedIsciModal";

const mapStateToProps = ({
  post: { unlinkedIscisData, archivedIscisData, unlinkedIscisLength }
}) => ({
  unlinkedIscisData,
  archivedIscisData,
  unlinkedIscisLength
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      createAlert,
      getPostFiltered,
      getUnlinkedIscis,
      toggleModal,
      processNtiFile
    },
    dispatch
  );

export class PageHeaderContainer extends Component {
  constructor(props) {
    super(props);
    this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.openUnlinkedIscis = this.openUnlinkedIscis.bind(this);
    this.processNTIFile = this.processNTIFile.bind(this);
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

  processNTIFile(file, isAccepted, fileExtension) {
    const req = { Filename: file.name, RawData: file.base64 };
    console.log("processNTIFile", file, isAccepted, fileExtension, req);
    this.props.processNtiFile(req);
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
              multiple={false}
              text="Upload NTI Transmittals"
              bsStyle="success"
              style={{ marginRight: "8px" }}
              bsSize="small"
              fileTypeExtension=".pdf"
              processFiles={this.processNTIFile}
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

PageHeaderContainer.propTypes = {
  getPostFiltered: PropTypes.func.isRequired,
  getUnlinkedIscis: PropTypes.func.isRequired,
  processNtiFile: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  unlinkedIscisData: PropTypes.array.isRequired,
  archivedIscisData: PropTypes.array.isRequired,
  unlinkedIscisLength: PropTypes.number.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PageHeaderContainer);
