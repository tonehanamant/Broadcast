import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Well, Row, Col } from 'react-bootstrap';
/* import { getDateInFormat } from '../../../../utils/dateFormatter'; */

import PostScrubbingGrid from '../PostScrubbingGrid';

const mapStateToProps = ({ post: { proposalDetail } }) => ({
    proposalDetail,
});

/* eslint-disable */
export class PostScrubbingDetail extends Component {
    componentWillMount() {
        this.setState({ proposalDetail: this.props.proposalDetail });
    }

    componentWillReceiveProps(nextProps) {
        if (nextProps.proposalDetail !== this.props.proposalDetail) {
            this.setState({ proposalDetail: nextProps.proposalDetail });
        }
    }

    shouldComponentUpdate(nextProps, nextState) {
        return nextProps.proposalDetail !== this.props.proposalDetail;
    }

    render() {
        /* const { proposalDetail: { DayPart, FlightEndDate, FlightStartDate, SpotLength, Genres, ClientScrubs } } = this.state; */
        const { isReadOnly } = this.props;
        /* eslint-disable no-unused-vars */
        return (
            <Well bsSize="small">
                {/* Commenting out few details in grid */}
                {/* <Row>
                    <Form inline>
                        <Col md={3}>
                            <FormGroup controlId="detailFlight">
                                <ControlLabel style={{ margin: '0 10px 0 0' }}>Flight</ControlLabel>
                                <FormControl
                                    type="text"
                                    defaultValue={`${getDateInFormat(FlightStartDate)} - ${getDateInFormat(FlightEndDate)}`}
                                    disabled={isReadOnly}
                                />
                            </FormGroup>
                        </Col>
                        <Col md={3}>
                            <FormGroup controlId="detailFlight">
                                <ControlLabel style={{ margin: '0 10px 0 0' }}>Daypart</ControlLabel>
                                <FormControl
                                    type="text"
                                    defaultValue={DayPart}
                                    disabled={isReadOnly}
                                />
                            </FormGroup>
                        </Col>
                        <Col md={3}>
                            <FormGroup controlId="proposalDetailSpotLength">
                                <ControlLabel style={{ margin: '0 10px 0 0' }}>Spot Length</ControlLabel>
                                <FormControl
                                    type="text"
                                    defaultValue={SpotLength}
                                    disabled={isReadOnly}
                                />
                            </FormGroup>
                        </Col>
                        <Col md={3}>
                            <FormGroup controlId="Program/Genre">
                                <ControlLabel style={{ margin: '0 10px 0 0' }}><strong>Program/Genre</strong></ControlLabel>
                                <FormControl
                                    type="text"
                                    defaultValue={Genres || null}
                                    disabled={isReadOnly}
                                />
                            </FormGroup>
                        </Col>
                    </Form>
                </Row> */}
                {
                    <Row style={{ marginTop: 10 }}>
                        <Col md={12}>
                            <PostScrubbingGrid />
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
    proposalDetail: PropTypes.object.isRequired,
    isReadOnly: PropTypes.bool.isRequired,
};

export default connect(mapStateToProps)(PostScrubbingDetail);
