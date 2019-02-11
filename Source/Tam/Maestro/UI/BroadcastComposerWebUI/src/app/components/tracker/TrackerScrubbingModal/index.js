import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Actions } from "react-redux-grid";

import { Button, Modal } from "react-bootstrap";
import { toggleModal, setOverlayLoading } from "Main/redux/actions";
import { getTracker, getTrackerClientScrubbing } from "Ducks/tracker";

import TrackerScrubbingHeader from "./TrackerScrubbingHeader";
import TrackerScrubbingDetail from "./TrackerScrubbingDetail";

const { SelectionActions, GridActions } = Actions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({
  app: {
    modals: { trackerScrubbingModal: modal }
  },
  tracker: {
    proposalHeader = {},
    scrubbingFiltersList = [],
    hasActiveScrubbingFilters
  },
  grid,
  dataSource
}) => ({
  modal,
  proposalHeader,
  scrubbingFiltersList,
  hasActiveScrubbingFilters,
  grid,
  dataSource
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getTrackerClientScrubbing,
      getTracker,
      toggleModal,
      selectRow,
      deselectAll,
      doLocalSort,
      setOverlayLoading
    },
    dispatch
  );

export class TrackerScrubbingModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.dismiss = this.dismiss.bind(this);
    this.refreshPost = this.refreshPost.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: "trackerScrubbingModal",
      active: false,
      properties: this.props.modal.properties
    });
  }

  dismiss() {
    this.props.modal.properties.dismiss();
    this.close();
  }

  refreshPost() {
    this.props.getTracker();
  }

  render() {
    const {
      proposalHeader,
      scrubbingFiltersList,
      getTrackerClientScrubbing,
      hasActiveScrubbingFilters,
      toggleModal,
      grid,
      dataSource,
      selectRow,
      deselectAll,
      doLocalSort,
      setOverlayLoading
    } = this.props;
    const { scrubbingData = {}, activeScrubbingData = {} } = proposalHeader;
    const {
      Advertiser,
      Id,
      Name,
      Markets,
      GuaranteedDemo,
      SecondaryDemos,
      Notes,
      MarketGroupId,
      Equivalized,
      CoverageGoal,
      PostingType,
      Details
    } = scrubbingData;

    return (
      <Modal
        ref={this.setWrapperRef}
        show={this.props.modal.active}
        dialogClassName="large-wide-modal"
        enforceFocus={false}
        onExited={this.refreshPost}
      >
        <Modal.Header>
          <Modal.Title style={{ display: "inline-block" }}>
            Scrubbing Screen
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
        <Modal.Body style={{ overflowX: "auto", paddingBottom: 0 }}>
          <TrackerScrubbingHeader
            advertiser={Advertiser}
            details={Details}
            guaranteedDemo={GuaranteedDemo}
            Id={Id}
            market={Markets}
            marketGroupId={MarketGroupId}
            equivalized={Equivalized}
            coverageGoal={CoverageGoal}
            postingType={PostingType}
            name={Name}
            notes={Notes}
            secondaryDemo={SecondaryDemos}
          />
          <TrackerScrubbingDetail
            activeScrubbingData={activeScrubbingData}
            scrubbingFiltersList={scrubbingFiltersList}
            getTrackerClientScrubbing={getTrackerClientScrubbing}
            hasActiveScrubbingFilters={hasActiveScrubbingFilters}
            details={Details}
            grid={grid}
            dataSource={dataSource}
            selectRow={selectRow}
            deselectAll={deselectAll}
            doLocalSort={doLocalSort}
            setOverlayLoading={setOverlayLoading}
            toggleModal={toggleModal}
          />
        </Modal.Body>
        <Modal.Footer>
          <Button
            onClick={this.close}
            bsStyle={this.props.modal.properties.closeButtonBsStyle}
          >
            Cancel
          </Button>
          <Button onClick={this.close} bsStyle="success">
            OK
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

TrackerScrubbingModal.defaultProps = {
  modal: {
    active: false,
    properties: {
      titleText: "Post Scrubbing details",
      bodyText: "under construction",
      closeButtonText: "Close",
      closeButtonBsStyle: "default",
      actionButtonText: "Save",
      actionButtonBsStyle: "sucuess",
      dismiss: () => {}
    }
  }
};

TrackerScrubbingModal.propTypes = {
  modal: PropTypes.object.isRequired,
  toggleModal: PropTypes.func.isRequired,
  getTrackerClientScrubbing: PropTypes.func.isRequired,
  getTracker: PropTypes.func.isRequired,
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  proposalHeader: PropTypes.object.isRequired,
  scrubbingFiltersList: PropTypes.array.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  hasActiveScrubbingFilters: PropTypes.bool.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(TrackerScrubbingModal);
