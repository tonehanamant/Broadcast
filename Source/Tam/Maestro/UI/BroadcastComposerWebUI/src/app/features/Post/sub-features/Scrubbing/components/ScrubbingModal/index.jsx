import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Actions } from "react-redux-grid";

import { Button, Modal } from "react-bootstrap";
import { toggleModal, setOverlayLoading } from "Main/redux/ducks";
import { postActions, scrubbingActions } from "Post";
import PostScrubbingHeader from "Post/sub-features/Scrubbing/components/ScrubbingHeader";
import PostScrubbingDetail from "Post/sub-features/Scrubbing/components/ScrubbingDetail";

const { SelectionActions, GridActions } = Actions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({
  app: {
    modals: { postScrubbingModal: modal }
  },
  post: {
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
      getPostClientScrubbing: scrubbingActions.getPostClientScrubbing,
      getPost: postActions.getPost,
      toggleModal,
      selectRow,
      deselectAll,
      doLocalSort,
      setOverlayLoading
    },
    dispatch
  );

export class PostScrubbingModal extends Component {
  constructor(props) {
    super(props);
    this.close = this.close.bind(this);
    this.dismiss = this.dismiss.bind(this);
    this.refreshPost = this.refreshPost.bind(this);
  }

  close() {
    const { toggleModal, modal } = this.props;
    toggleModal({
      modal: "postScrubbingModal",
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
    const { getPost } = this.props;
    getPost();
  }

  render() {
    const {
      proposalHeader,
      scrubbingFiltersList,
      getPostClientScrubbing,
      hasActiveScrubbingFilters,
      toggleModal,
      grid,
      dataSource,
      selectRow,
      deselectAll,
      doLocalSort,
      modal,
      setOverlayLoading
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
          <PostScrubbingHeader
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
          <PostScrubbingDetail
            activeScrubbingData={activeScrubbingData}
            scrubbingFiltersList={scrubbingFiltersList}
            getPostClientScrubbing={getPostClientScrubbing}
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

PostScrubbingModal.defaultProps = {
  modal: {
    active: false,
    properties: {}
  }
};

PostScrubbingModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  getPostClientScrubbing: PropTypes.func.isRequired,
  getPost: PropTypes.func.isRequired,
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
)(PostScrubbingModal);
