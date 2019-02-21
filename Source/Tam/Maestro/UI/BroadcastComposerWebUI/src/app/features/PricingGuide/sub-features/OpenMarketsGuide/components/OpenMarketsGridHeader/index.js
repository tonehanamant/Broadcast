import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { toggleModal } from "Main/redux/actions";
import {
  filterOpenMarketData,
  sortOpenMarketData,
  showEditMarkets
} from "Ducks/planning";
import {
  Row,
  Col,
  Button,
  Glyphicon,
  Form,
  FormGroup,
  InputGroup,
  DropdownButton,
  MenuItem
} from "react-bootstrap";
import Select from "react-select";
import { merge } from "lodash";
import PricingGuideFilterModal from "../OpenMarketsFilterModal";

const spotFilterOptions = [
  { Display: "All Programs", Id: 1 },
  { Display: "Programs with Spots", Id: 2 },
  { Display: "Programs without Spots", Id: 3 }
];

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    { toggleModal, filterOpenMarketData, sortOpenMarketData, showEditMarkets },
    dispatch
  );

class PricingGuideGridHeader extends Component {
  constructor(props) {
    super(props);
    this.onOpenFilterModal = this.onOpenFilterModal.bind(this);
    this.applyFilters = this.applyFilters.bind(this);
    this.onFilterSpots = this.onFilterSpots.bind(this);
    this.sortMarket = this.sortMarket.bind(this);
    this.onOpenEditMarkets = this.onOpenEditMarkets.bind(this);
  }

  onOpenFilterModal() {
    this.props.toggleModal({
      modal: "pricingFilterModal",
      active: true
    });
  }

  applyFilters(filterObject, isModal) {
    const { activeOpenMarketData } = this.props;
    // adjust if call is from modal where SpotFilter omitted: TODO change and split in reducer?
    let filters = filterObject;
    if (isModal && activeOpenMarketData.Filter.SpotFilter) {
      filters = merge(filterObject, {
        Filter: {
          SpotFilter: activeOpenMarketData.Filter.SpotFilter
        }
      });
    }
    this.props.filterOpenMarketData(filters);
  }

  onFilterSpots(value) {
    const { activeOpenMarketData } = this.props;
    const filterObject = activeOpenMarketData.Filter || {};
    const filters = Object.assign({}, filterObject, { SpotFilter: value.Id });
    this.applyFilters({ Filter: filters });
  }

  sortMarket(sortKey) {
    this.props.sortOpenMarketData(sortKey === "sortMarketName");
  }

  onOpenEditMarkets() {
    this.props.showEditMarkets(true);
  }

  render() {
    // change to determine by master data set - not active which could be empty by filter
    const {
      activeOpenMarketData,
      toggleModal,
      hasOpenMarketData: hasData,
      isOpenMarketDataSortName,
      isGuideEditing
    } = this.props;
    // FOR INDICATOR - check modal specific filters active (not spot)
    const hasActiveModalFilter =
      activeOpenMarketData && activeOpenMarketData.Filter
        ? activeOpenMarketData.Filter.ProgramNames ||
          activeOpenMarketData.Filter.Affiliations ||
          activeOpenMarketData.Filter.Markets ||
          activeOpenMarketData.Filter.Genres ||
          activeOpenMarketData.Filter.DayParts
        : false;
    const spotFilterValue =
      activeOpenMarketData &&
      activeOpenMarketData.Filter &&
      activeOpenMarketData.Filter.SpotFilter
        ? activeOpenMarketData.Filter.SpotFilter
        : 1;
    return (
      <div>
        <Row style={{ marginTop: "10px" }}>
          <Col xs={5}>
            <Form inline>
              <span>
                <Glyphicon
                  glyph="filter"
                  style={{
                    fontSize: "18px",
                    top: "6px",
                    color: hasActiveModalFilter ? "orange" : "#999"
                  }}
                />
              </span>
              <FormGroup
                controlId="pricingFilters"
                style={{ margin: "0 30px 0 10px" }}
              >
                <InputGroup>
                  <InputGroup.Button>
                    <Button
                      bsStyle="primary"
                      onClick={this.onOpenFilterModal}
                      disabled={!hasData}
                    >
                      <Glyphicon glyph="search" />
                    </Button>
                  </InputGroup.Button>
                  <Select
                    name="spotFilters"
                    style={{ width: "220px" }}
                    value={spotFilterValue}
                    disabled={!hasData}
                    options={spotFilterOptions}
                    labelKey="Display"
                    valueKey="Id"
                    onChange={this.onFilterSpots}
                    clearable={false}
                  />
                </InputGroup>
              </FormGroup>
              <DropdownButton
                bsStyle="default"
                disabled={!hasData}
                title={
                  <span
                    className="glyphicon glyphicon-option-horizontal"
                    aria-hidden="true"
                  />
                }
                noCaret
                id="pricing_sort"
              >
                <MenuItem eventKey="sortMarketName" onSelect={this.sortMarket}>
                  {isOpenMarketDataSortName ? (
                    <Glyphicon style={{ color: "green" }} glyph="ok" />
                  ) : (
                    ""
                  )}{" "}
                  Sort By Market Name
                </MenuItem>
                <MenuItem eventKey="sortMarketRank" onSelect={this.sortMarket}>
                  {!isOpenMarketDataSortName ? (
                    <Glyphicon style={{ color: "green" }} glyph="ok" />
                  ) : (
                    ""
                  )}{" "}
                  Sort By Market Rank
                </MenuItem>
              </DropdownButton>
            </Form>
          </Col>
          <Col xs={3}>
            <h4 style={{ marginTop: "10px" }}>Distribution Results</h4>
          </Col>
          <Col xs={4}>
            <div style={{ textAlign: "right" }}>
              <Button
                bsStyle="success"
                disabled={isGuideEditing}
                onClick={this.onOpenEditMarkets}
                style={{ marginLeft: "5px" }}
              >
                Edit Markets
              </Button>
            </div>
          </Col>
        </Row>
        <PricingGuideFilterModal
          toggleModal={toggleModal}
          activeOpenMarketData={activeOpenMarketData}
          applyFilters={this.applyFilters}
        />
      </div>
    );
  }
}

PricingGuideGridHeader.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  toggleModal: PropTypes.func.isRequired,
  filterOpenMarketData: PropTypes.func.isRequired,
  sortOpenMarketData: PropTypes.func.isRequired,
  showEditMarkets: PropTypes.func.isRequired,
  isGuideEditing: PropTypes.bool.isRequired
};

export default connect(
  null,
  mapDispatchToProps
)(PricingGuideGridHeader);
