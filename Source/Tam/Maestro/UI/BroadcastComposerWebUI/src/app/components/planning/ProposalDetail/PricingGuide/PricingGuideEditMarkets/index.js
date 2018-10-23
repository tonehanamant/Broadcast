import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
// import { toggleModal } from "Ducks/app";
import { showEditMarkets } from "Ducks/planning";
import { Row, Col, Panel, Button, ButtonToolbar } from "react-bootstrap";
import numeral from "numeral";
import { partition, sumBy } from "lodash";
import EditMarketsGrid from "./EditMarketsGrid";

const mapDispatchToProps = dispatch =>
  bindActionCreators({ showEditMarkets }, dispatch);

class PricingGuideEditMarkets extends Component {
  constructor(props) {
    super(props);
    this.onDismissEditMarkets = this.onDismissEditMarkets.bind(this);
    this.onUpdateEditMarkets = this.onUpdateEditMarkets.bind(this);
    this.removeUsedMarket = this.removeUsedMarket.bind(this);
    this.addAvailableMarket = this.addAvailableMarket.bind(this);
  }

  onDismissEditMarkets() {
    this.props.showEditMarkets(false);
  }
  // todo persist changes
  onUpdateEditMarkets() {
    this.props.showEditMarkets(false);
  }

  // handle update via action todo
  removeUsedMarket(rec) {
    console.log("removeUsedMarket", rec, this);
  }

  addAvailableMarket(rec) {
    console.log("addAvailableMarket", rec, this);
  }

  render() {
    const { activeEditMarkets, marketCoverageGoal } = this.props;
    const partitionedMarkets = partition(activeEditMarkets, "Selected");
    const usedMarkets = partitionedMarkets[0];
    const availableMarkets = partitionedMarkets[1];
    const currentCoverage = sumBy(usedMarkets, "Coverage");
    const usedCount = usedMarkets.length;
    const availableCount = availableMarkets.length;
    // console.log("Edit Markets", activeEditMarkets, partitionedMarkets);
    return (
      <div>
        <Panel>
          <Panel.Heading>
            <Panel.Title componentClass="h3" style={{ textAlign: "center" }}>
              Edit Markets
            </Panel.Title>
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
            <Row style={{ marginTop: "10px" }}>
              <Col xs={6}>
                <EditMarketsGrid
                  editMarketsData={usedMarkets}
                  isAvailableMarkets={false}
                  editMarketAction={this.removeUsedMarket}
                />
              </Col>
              <Col xs={6}>
                <EditMarketsGrid
                  editMarketsData={availableMarkets}
                  isAvailableMarkets
                  editMarketAction={this.addAvailableMarket}
                />
              </Col>
            </Row>
          </Panel.Body>
          <Panel.Footer style={{ textAlign: "right" }}>
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
                // disabled={!this.state.isValidSelection}
                style={{ float: "unset", marginLeft: "10px" }}
                onClick={this.onUpdateEditMarkets}
              >
                Update Markets
              </Button>
            </ButtonToolbar>
          </Panel.Footer>
        </Panel>
      </div>
    );
  }
}

PricingGuideEditMarkets.propTypes = {
  activeEditMarkets: PropTypes.array.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
  marketCoverageGoal: PropTypes.number.isRequired
  // hasOpenMarketData: PropTypes.bool.isRequired,
};

// export default PricingGuideEditMarkets;
export default connect(
  null,
  mapDispatchToProps
)(PricingGuideEditMarkets);
