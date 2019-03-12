import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { toggleModal, createAlert } from "Main/redux/ducks";
import { postActions, unlinkedIsciActions } from "Post";
import { Row, Col, Button } from "react-bootstrap";
import SearchInputButton from "Patterns/SearchInputButton";
import UploadButton from "Patterns/UploadButton";
import UnlinkedIsciModal from "Post/sub-features/UnlinkedIsci/components/UnlinkedIsciModal";

const mapStateToProps = ({
  post: {
    unlinkedIsci: { unlinkedIscisData, archivedIscisData },
    master: { unlinkedIscisLength }
  }
}) => ({
  unlinkedIscisData,
  archivedIscisData,
  unlinkedIscisLength
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      createAlert,
      toggleModal,
      getPostFiltered: postActions.getPostFiltered,
      processNtiFile: postActions.processNtiFile,
      getUnlinkedIscis: unlinkedIsciActions.getUnlinkedIscis
    },
    dispatch
  );

export class PostHeader extends Component {
  constructor(props) {
    super(props);
    this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);
    this.openUnlinkedIscis = this.openUnlinkedIscis.bind(this);
    this.processNTIFile = this.processNTIFile.bind(this);
  }

  SearchInputAction() {
    const { getPostFiltered } = this.props;
    getPostFiltered();
  }

  SearchSubmitAction(value) {
    const { getPostFiltered } = this.props;
    getPostFiltered(value);
  }

  openUnlinkedIscis() {
    const { getUnlinkedIscis } = this.props;
    getUnlinkedIscis();
  }

  processNTIFile({ name, base64 }) {
    const { processNtiFile } = this.props;
    processNtiFile({ Filename: name, RawData: base64 });
  }

  render() {
    const {
      unlinkedIscisLength,
      toggleModal,
      unlinkedIscisData,
      archivedIscisData
    } = this.props;
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
          toggleModal={toggleModal}
          unlinkedIscisData={unlinkedIscisData}
          archivedIscisData={archivedIscisData}
        />
      </div>
    );
  }
}

PostHeader.propTypes = {
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
)(PostHeader);
