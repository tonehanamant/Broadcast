import React, { Component } from "react";
import PropTypes from "prop-types";
import { Well, Row, Col, Nav, NavItem } from "react-bootstrap";
import TrackerScrubbingGrid from "Tracker/sub-features/Scrubbing/components/ScrubbingGrid";
import TrackerScrubbingFilters from "Tracker/sub-features/Scrubbing/components/ScrubbingFilters";

export class TrackerScrubbingDetail extends Component {
  constructor(props) {
    super(props);
    this.state = {
      activeTabKey: "All"
    };
    this.handleTabSelect = this.handleTabSelect.bind(this);
  }

  componentWillReceiveProps(nextProps) {
    const { activeTabKey } = this.state;
    // if change filterKey/data at the saga level (on refresh) - need match filterkey state to change active tab here
    if (nextProps.activeScrubbingData.filterKey && activeTabKey) {
      if (nextProps.activeScrubbingData.filterKey !== activeTabKey) {
        this.setState({
          activeTabKey: nextProps.activeScrubbingData.filterKey
        });
      }
    }
  }

  handleTabSelect(eventKey) {
    const { activeTabKey } = this.state;
    const { activeScrubbingData, getTrackerClientScrubbing } = this.props;
    if (activeTabKey === eventKey) return;
    this.setState({ activeTabKey: eventKey });
    const { Id } = activeScrubbingData;
    getTrackerClientScrubbing({
      proposalId: Id,
      showModal: false,
      filterKey: eventKey
    });
  }

  render() {
    const {
      activeScrubbingData,
      scrubbingFiltersList,
      grid,
      dataSource,
      toggleModal,
      selectRow,
      deselectAll,
      doLocalSort,
      setOverlayLoading,
      hasActiveScrubbingFilters,
      details
    } = this.props;
    const hasData =
      activeScrubbingData.ClientScrubs.length > 0 || hasActiveScrubbingFilters;
    const { activeTabKey } = this.state;

    return (
      <div>
        <Nav
          style={{ marginBottom: 3 }}
          bsStyle="tabs"
          activeKey={activeTabKey}
          onSelect={this.handleTabSelect}
        >
          <NavItem eventKey="All" title="All">
            All
          </NavItem>
          <NavItem eventKey="InSpec" title="In Spec">
            In Spec
          </NavItem>
          <NavItem eventKey="OutOfSpec" title="Out of Spec">
            Out of Spec
          </NavItem>
        </Nav>

        <Well bsSize="small" style={{ width: "1750px", marginBottom: 0 }}>
          <Row style={{ marginTop: 4 }}>
            <Col md={12}>
              {hasData && (
                <TrackerScrubbingFilters activeFilters={scrubbingFiltersList} />
              )}
              <TrackerScrubbingGrid
                activeScrubbingData={activeScrubbingData}
                grid={grid}
                dataSource={dataSource}
                selectRow={selectRow}
                deselectAll={deselectAll}
                doLocalSort={doLocalSort}
                setOverlayLoading={setOverlayLoading}
                details={details}
                toggleModal={toggleModal}
              />
            </Col>
          </Row>
        </Well>
      </div>
    );
  }
}

TrackerScrubbingDetail.defaultProps = {
  hasActiveScrubbingFilters: false,
  details: []
};

TrackerScrubbingDetail.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeScrubbingData: PropTypes.object.isRequired,
  scrubbingFiltersList: PropTypes.array.isRequired,
  hasActiveScrubbingFilters: PropTypes.bool,
  details: PropTypes.array,
  setOverlayLoading: PropTypes.func.isRequired,
  getTrackerClientScrubbing: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired
};

export default TrackerScrubbingDetail;
