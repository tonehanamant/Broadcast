import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Well, Row, Col } from 'react-bootstrap';
/* import { getDateInFormat } from '../../../../utils/dateFormatter'; */

import PostScrubbingGrid from '../PostScrubbingGrid';
import PostScrubbingFilters from '../PostScrubbingFilters';

/* eslint-disable */
export class PostScrubbingDetail extends Component {

    render() {
        /* const { proposalDetail: { DayPart, FlightEndDate, FlightStartDate, SpotLength, Genres, ClientScrubs } } = this.state; */
      const { isReadOnly } = this.props;
      const { activeScrubbingData, scrubbingFiltersList, grid, dataSource } = this.props;
      const { selectRow, deselectAll, doLocalSort, setOverlayLoading } = this.props;
      const hasData = activeScrubbingData.ClientScrubs.length > 0;
        /* eslint-disable no-unused-vars */
        return (
            // <Well bsSize="small"  style={{ width: 'fit-content' }}>
            <Well bsSize="small"  style={{ width: '1600px', marginBottom: 0 }}>
              {
                <Row style={{ marginTop: 10 }}>
                    <Col md={12}>
                        { hasData &&
                        <PostScrubbingFilters
                          activeFilters={scrubbingFiltersList}
                        />
                        }
                        <PostScrubbingGrid
                            activeScrubbingData={activeScrubbingData}
                            grid={grid}
                            dataSource={dataSource}
                            selectRow={selectRow}
                            deselectAll={deselectAll}
                            doLocalSort={doLocalSort}
                            setOverlayLoading={setOverlayLoading}
                        />
                    </Col>
                </Row>
              }
            </Well>
        );
    }
}

PostScrubbingDetail.defaultProps = {
    isReadOnly: true,
};

PostScrubbingDetail.propTypes = {
  isReadOnly: PropTypes.bool.isRequired,
  grid: PropTypes.object.isRequired,
	dataSource: PropTypes.object.isRequired,
  scrubbingFiltersList: PropTypes.array.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default PostScrubbingDetail;
