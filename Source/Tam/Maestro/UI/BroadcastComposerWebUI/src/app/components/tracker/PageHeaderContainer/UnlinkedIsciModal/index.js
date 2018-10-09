import React, { Component } from "react";
import PropTypes from "prop-types";
import SearchInputButton from "Components/shared/SearchInputButton";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Button, Modal, Nav, NavItem, Row, Col } from "react-bootstrap";
import {
  archiveUnlinkedIscis,
  toggleUnlinkedTab,
  rescrubUnlinkedIscis,
  undoArchivedIscis,
  closeUnlinkedIsciModal,
  getUnlinkedFiltered,
  getArchivedFiltered
} from "Ducks/tracker";
import UnlinkedIsciGrid from "./unlinkedIsciGrid";
import ArchivedIsciGrid from "./archivedIsciGrid";
import MapUnlinkedIsciModal from "../MapUnlinkedIsciModal/index";

const mapStateToProps = ({
  app: {
    modals: { trackerUnlinkedIsciModal: modal }
  }
}) => ({
  modal
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      closeUnlinkedIsciModal,
      getUnlinkedFiltered,
      getArchivedFiltered,
      rescrubIscis: rescrubUnlinkedIscis,
      archiveIscis: archiveUnlinkedIscis,
      undoArchive: undoArchivedIscis,
      toggleTab: toggleUnlinkedTab
    },
    dispatch
  );

export class UnlinkedIsciModal extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.close = this.close.bind(this);
    this.onTabSelect = this.onTabSelect.bind(this);
    this.SearchInputAction = this.SearchInputAction.bind(this);
    this.SearchSubmitAction = this.SearchSubmitAction.bind(this);

    this.state = {
      activeTab: "unlinked"
    };
  }

  SearchInputAction() {
    const { activeTab } = this.state;
    if (activeTab === "unlinked") {
      this.props.getUnlinkedFiltered();
    } else {
      this.props.getArchivedFiltered();
    }
  }

  SearchSubmitAction(value) {
    const { activeTab } = this.state;
    if (activeTab === "unlinked") {
      this.props.getUnlinkedFiltered(value);
    } else {
      this.props.getArchivedFiltered(value);
    }
  }

  close() {
    this.props.closeUnlinkedIsciModal(this.props.modal.properties);
    this.setState({ activeTab: "unlinked" });
  }

  onTabSelect(nextTab) {
    const { activeTab } = this.state;
    const { toggleTab } = this.props;
    if (activeTab !== nextTab) {
      this.setState({ activeTab: nextTab });
      toggleTab(nextTab);
      this.searchInput.clearForm();
    }
  }

  render() {
    const {
      modal,
      unlinkedIscisData,
      archivedIscisData,
      rescrubIscis,
      archiveIscis,
      undoArchive,
      toggleModal
    } = this.props;
    const { activeTab } = this.state;

    return (
      <div>
        <Modal
          show={modal.active}
          onHide={this.close}
          dialogClassName="large-80-modal"
        >
          <Modal.Header>
            <Modal.Title style={{ display: "inline-block" }}>
              Unlinked ISCIs
            </Modal.Title>
            <Button
              className="close"
              bsStyle="link"
              onClick={this.close}
              style={{ display: "inline-block", float: "right" }}
            >
              <span>&times;</span>
            </Button>
          </Modal.Header>
          <Modal.Body>
            <Row>
              <Col xs={6}>
                <Nav
                  style={{ marginBottom: 3 }}
                  bsStyle="tabs"
                  activeKey={activeTab}
                  onSelect={this.onTabSelect}
                >
                  <NavItem eventKey="unlinked">Unlinked ISCIs</NavItem>
                  <NavItem eventKey="archived">Archived ISCIs</NavItem>
                </Nav>
              </Col>
              <Col xs={6}>
                <SearchInputButton
                  inputAction={this.SearchInputAction}
                  submitAction={this.SearchSubmitAction}
                  fieldPlaceHolder="Filter by ISCI..."
                  ref={ref => {
                    this.searchInput = ref;
                  }}
                />
              </Col>
            </Row>
            {activeTab === "unlinked" && (
              <UnlinkedIsciGrid
                rescrubIscis={rescrubIscis}
                unlinkedIscisData={unlinkedIscisData}
                archiveIscis={archiveIscis}
                toggleModal={toggleModal}
              />
            )}
            {activeTab === "archived" && (
              <ArchivedIsciGrid
                archivedIscisData={archivedIscisData}
                undoArchive={undoArchive}
              />
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button onClick={this.close}>Close</Button>
          </Modal.Footer>
        </Modal>
        <MapUnlinkedIsciModal />
      </div>
    );
  }
}

UnlinkedIsciModal.defaultProps = {
  modal: {
    active: false,
    properties: {}
  }
};

UnlinkedIsciModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  rescrubIscis: PropTypes.func.isRequired,
  unlinkedIscisData: PropTypes.array.isRequired,
  archivedIscisData: PropTypes.array.isRequired,
  toggleTab: PropTypes.func.isRequired,
  archiveIscis: PropTypes.func.isRequired,
  undoArchive: PropTypes.func.isRequired,
  closeUnlinkedIsciModal: PropTypes.func.isRequired,
  getUnlinkedFiltered: PropTypes.func.isRequired,
  getArchivedFiltered: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(UnlinkedIsciModal);
