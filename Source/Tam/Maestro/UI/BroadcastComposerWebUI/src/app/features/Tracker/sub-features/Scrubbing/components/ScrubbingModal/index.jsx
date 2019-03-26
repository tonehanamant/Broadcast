import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Actions } from "Lib/react-redux-grid";

import { Button, Modal } from "react-bootstrap";
import { toggleModal, setOverlayLoading } from "Main/redux/ducks";
import { trackerActions, scrubbingActions } from "Tracker";
import TrackerScrubbingHeader from "Tracker/sub-features/Scrubbing/components/ScrubbingHeader";
import TrackerScrubbingDetail from "Tracker/sub-features/Scrubbing/components/ScrubbingDetail";

const { SelectionActions, GridActions } = Actions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({
  app: {
    modals: { trackerScrubbingModal: modal }
  },
  tracker: {
    scrubbing: {
      proposalHeader = {},
      scrubbingFiltersList = [],
      hasActiveScrubbingFilters
    }
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
      getTrackerClientScrubbing: scrubbingActions.getTrackerClientScrubbing,
      getTracker: trackerActions.getTracker,
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
    const { modal, toggleModal } = this.props;
    toggleModal({
      modal: "trackerScrubbingModal",
      active: false,
      properties: modal.properties
    });
  }

  dismiss() {
    const { modal } = this.props;
    modal.properties.dismiss();
    this.close();
  }

  refreshPost() {
    const { getTracker } = this.props;
    getTracker();
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
      setOverlayLoading,
      modal
    } = this.props;
    const { scrubbingData = {}, activeScrubbingData = {} } = proposalHeader;
    const {
      Advertiser,
      Id,
      Name,
      Markets,
      GuaranteedDemo,
      Equivalized,
      CoverageGoal,
      PostingType,
      SecondaryDemos,
      Notes,
      MarketGroupId,
      Details
    } = scrubbingData;

    return (
      <Modal
        ref={this.setWrapperRef}
        show={modal.active}
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
            bsStyle={modal.properties.closeButtonBsStyle}
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
      titleText: "Tracker Scrubbing details",
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
  modal: PropTypes.object,
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