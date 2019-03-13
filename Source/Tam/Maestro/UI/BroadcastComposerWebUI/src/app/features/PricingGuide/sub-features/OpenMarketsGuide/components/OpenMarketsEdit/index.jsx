import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import {
  showEditMarkets,
  changeEditMarkets,
  discardEditMarkets
} from "Ducks/planning";
import { Row, Col, Panel, Button, ButtonToolbar } from "react-bootstrap";
import numeral from "numeral";
import { partition, sumBy } from "lodash";
import EditMarketsGrid from "../OpenMarketsEditGrid";

import "./index.scss";

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      showEditMarkets,
      changeEditMarkets,
      discardEditMarkets
    },
    dispatch
  );

const mapStateToProps = ({ planning: { changedMarkets } }) => ({
  changedMarkets
});

class PricingGuideEditMarkets extends Component {
  constructor(props) {
    super(props);
    this.state = {
      sortedAddedMarket: [],
      sortedAvailableMarket: []
    };
    this.onDismissEditMarkets = this.onDismissEditMarkets.bind(this);
    this.onUpdateEditMarkets = this.onUpdateEditMarkets.bind(this);
    this.removeUsedMarket = this.removeUsedMarket.bind(this);
    this.addAvailableMarket = this.addAvailableMarket.bind(this);
  }

  onDismissEditMarkets() {
    const { discardEditMarkets, showEditMarkets } = this.props;
    discardEditMarkets();
    showEditMarkets(false);
  }

  onUpdateEditMarkets() {
    const { onUpdateEditMarkets } = this.props;
    onUpdateEditMarkets();
  }

  onSortedChange(path, nextValue) {
    this.setState({ [path]: nextValue });
  }

  addAvailableMarket(rec) {
    const { changeEditMarkets } = this.props;
    changeEditMarkets(rec.Id, true);
    this.setState({ sortedAddedMarket: [{ id: "newItem", desc: true }] });
  }

  removeUsedMarket(rec) {
    const { changeEditMarkets } = this.props;
    changeEditMarkets(rec.Id, false);
    this.setState({ sortedAvailableMarket: [{ id: "newItem", desc: true }] });
  }

  render() {
    const {
      activeEditMarkets,
      marketCoverageGoal,
      openCpmTarget,
      changedMarkets
    } = this.props;
    const { sortedAddedMarket, sortedAvailableMarket } = this.state;
    const partitionedMarkets = partition(activeEditMarkets, "Selected");
    const usedMarkets = partitionedMarkets[0];
    const availableMarkets = partitionedMarkets[1];
    const currentCoverage = sumBy(usedMarkets, "Coverage");
    const usedCount = usedMarkets.length;
    const availableCount = availableMarkets.length;
    return (
      <div>
        <Panel>
          <Panel.Heading>
            <Row>
              <Col xs={4} />
              <Col xs={4}>
                <Panel.Title
                  componentClass="h3"
                  style={{ textAlign: "center" }}
                >
                  Edit Markets
                </Panel.Title>
              </Col>
              <Col xs={4} style={{ textAlign: "right" }}>
                <ButtonToolbar>
                  <Button
                    bsStyle="default"
                    bsSize="small"
                    style={{ float: "unset" }}
                    onClick={this.onDismissEditMarkets}
                  >
                    Discard
                  </Button>
                  <Button
                    bsStyle="primary"
                    bsSize="small"
                    style={{ float: "unset", marginLeft: "10px" }}
                    onClick={this.onUpdateEditMarkets}
                  >
                    Update Markets
                  </Button>
                </ButtonToolbar>
              </Col>
            </Row>
          </Panel.Heading>
          <Panel.Body>
            <Row style={{ marginTop: "10px" }}>
              <Col xs={6}>
                <div className="summary-bar-left">
                  <div className="summary-item">
                    <div className="summary-display">
                      {numeral(marketCoverageGoal).format("0,0%")}
                    </div>
                    <div className="summary-label">MARKET COVERAGE GOAL</div>
                  </div>
                  <div className="summary-item">
                    <div className="summary-display">
                      {numeral(currentCoverage).format("0,0.[000]")}%
                    </div>
                    <div className="summary-label">CURRENT MARKET COVERAGE</div>
                  </div>
                  <div className="summary-item">
                    <div className="summary-display">({usedCount})</div>
                    <div className="summary-label">MARKETS USED</div>
                  </div>
                </div>
              </Col>
              <Col xs={6}>
                <div className="summary-bar-left">
                  <div className="summary-item">
                    <div className="summary-display">({availableCount})</div>
                    <div className="summary-label">AVAILABLE MARKETS</div>
                  </div>
                </div>
              </Col>
            </Row>
            <Row className="pricing-guide_edit-markets">
              <Col xs={6}>
                <EditMarketsGrid
                  editMarketsData={usedMarkets}
                  changedMarkets={changedMarkets}
                  isAvailableMarkets={false}
                  editMarketAction={this.removeUsedMarket}
                  openCpmTarget={openCpmTarget}
                  sorted={sortedAddedMarket}
                  onSortedChange={v =>
                    this.onSortedChange("sortedAddedMarket", v)
                  }
                />
              </Col>
              <Col xs={6}>
                <EditMarketsGrid
                  changedMarkets={changedMarkets}
                  editMarketsData={availableMarkets}
                  sorted={sortedAvailableMarket}
                  onSortedChange={v =>
                    this.onSortedChange("sortedAvailableMarket", v)
                  }
                  isAvailableMarkets
                  editMarketAction={this.addAvailableMarket}
                  openCpmTarget={openCpmTarget}
                />
              </Col>
            </Row>
          </Panel.Body>
        </Panel>
      </div>
    );
  }
}

PricingGuideEditMarkets.propTypes = {
  activeEditMarkets: PropTypes.array.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
  changeEditMarkets: PropTypes.func.isRequired,
  discardEditMarkets: PropTypes.func.isRequired,
  onUpdateEditMarkets: PropTypes.func.isRequired,
  marketCoverageGoal: PropTypes.number.isRequired,
  changedMarkets: PropTypes.array.isRequired,
  openCpmTarget: PropTypes.number.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PricingGuideEditMarkets);
