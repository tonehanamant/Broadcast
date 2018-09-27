import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Well, Row, Col, Nav, NavItem } from 'react-bootstrap';

import TrackerScrubbingGrid from '../TrackerScrubbingGrid';
import TrackerScrubbingFilters from '../TrackerScrubbingFilters';


export class TrackerScrubbingDetail extends Component {
  constructor(props) {
    super(props);
    this.state = {
      activeTabKey: 'All',
    };
    this.handleTabSelect = this.handleTabSelect.bind(this);
  }

  componentWillReceiveProps(nextProps) {
    // if change filterKey/data at the saga level (on refresh) - need match filterkey state to change active tab here
    if (nextProps.activeScrubbingData.filterKey && this.state.activeTabKey) {
      if (nextProps.activeScrubbingData.filterKey !== this.state.activeTabKey) {
        this.setState({ activeTabKey: nextProps.activeScrubbingData.filterKey });
      }
    }
  }

  handleTabSelect(eventKey) {
    event.preventDefault();
    if (this.state.activeTabKey === eventKey) return;
    this.setState({ activeTabKey: eventKey });
    // console.log(`selected ${eventKey}`);
    const Id = this.props.activeScrubbingData.Id;
    this.props.getTrackerClientScrubbing({ proposalId: Id, showModal: false, filterKey: eventKey });
  }

  render() {
    const { activeScrubbingData, scrubbingFiltersList, grid, dataSource, toggleModal } = this.props;
    const { selectRow, deselectAll, doLocalSort, setOverlayLoading, hasActiveScrubbingFilters, details } = this.props;
    const hasData = (activeScrubbingData.ClientScrubs.length > 0) || hasActiveScrubbingFilters;
    // console.log('>>>>>>>>>>>>>>>>>>>>>RENDER', this.state.activeTabKey, this.props);
    return (
      <div>
        <Nav style={{ marginBottom: 3 }} bsStyle="tabs" activeKey={this.state.activeTabKey} onSelect={this.handleTabSelect}>
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

        <Well bsSize="small" style={{ width: '1750px', marginBottom: 0 }}>
          <Row style={{ marginTop: 4 }}>
            <Col md={12}>
              { hasData &&
              <TrackerScrubbingFilters
                activeFilters={scrubbingFiltersList}
              />
              }
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
    isReadOnly: true,
    hasActiveScrubbingFilters: false,
};

TrackerScrubbingDetail.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeScrubbingData: PropTypes.object.isRequired,
  scrubbingFiltersList: PropTypes.array.isRequired,
  hasActiveScrubbingFilters: PropTypes.bool.isRequired,
  details: PropTypes.array.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,
  getTrackerClientScrubbing: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};

export default TrackerScrubbingDetail;
