import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal } from 'Ducks/app';
import { filterOpenMarketData } from 'Ducks/planning';
// import { getPlanningGuideFiltered } from 'Ducks/planning';
import { Row, Col, Button, Glyphicon, Form, FormGroup, InputGroup, DropdownButton, MenuItem } from 'react-bootstrap';
import Select from 'react-select';
import PricingGuideFilterModal from '../PricingGuideFilterModal';

const spotFilterOptions = [
  { Display: 'All Programs', Id: 1 },
  { Display: 'Programs with Spots', Id: 2 },
  { Display: 'Programs without Spots', Id: 3 },
];


const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, filterOpenMarketData }, dispatch)
);

class PricingGuideGridHeader extends Component {
  constructor(props) {
    super(props);
      this.onOpenFilterModal = this.onOpenFilterModal.bind(this);
      this.applyFilters = this.applyFilters.bind(this);
  }

  onOpenFilterModal() {
    this.props.toggleModal({
      modal: 'pricingFilterModal',
      active: true,
    });
  }

  applyFilters(filterObject) {
    // console.log('header apply filters', filterObject);
    this.props.filterOpenMarketData(filterObject);
  }

  render() {
    // const hasData = this.props.activeOpenMarketData && this.props.activeOpenMarketData.Markets.length;
    // change to determine by master data set - not active which could be empty by filter
    const hasData = this.props.hasOpenMarketData;
    const { activeOpenMarketData, toggleModal } = this.props;
    // FOR INDICATOR - TODO formalize so can check modal specific filters active
    const hasActiveModalFilter = activeOpenMarketData && activeOpenMarketData.Filter
      && activeOpenMarketData.Filter.ProgramNames && activeOpenMarketData.Filter.ProgramNames.length;
    return (
      <div>
        <Row style={{ marginTop: '10px' }}>
          <Col xs={5}>
          <Form inline>
            <span>
              <Glyphicon glyph="filter" style={{ fontSize: '18px', top: '6px', color: hasActiveModalFilter ? 'orange' : '#999' }} />
            </span>
            <FormGroup controlId="pricingFilters" style={{ margin: '0 30px 0 10px' }}>
              <InputGroup>
                <InputGroup.Button>
                  <Button bsStyle="primary"onClick={this.onOpenFilterModal} disabled={!hasData}><Glyphicon glyph="search" /></Button>
                </InputGroup.Button>
                <Select
                  name="spotFilters"
                  style={{ width: '220px' }}
                  // wrapperStyle={{ height: '18px' }}
                  value={1}
                  // placeholder=""
                  disabled={!hasData}
                  options={spotFilterOptions}
                  labelKey="Display"
                  valueKey="Id"
                  // onChange={this.onFilterSpots}
                  clearable={false}
                />
              </InputGroup>
            </FormGroup>
            <DropdownButton bsStyle="default" disabled={!hasData} title={<span className="glyphicon glyphicon-option-horizontal" aria-hidden="true" />} noCaret id="pricing_sort">
            <MenuItem eventKey="sortMarketName">Sort By Market Name</MenuItem>
            <MenuItem eventKey="sortMarketRank">Sort By Market Rank</MenuItem>
            </DropdownButton>
          </Form>
          </Col>
          <Col xs={7}>
            <h4>Distribution Results</h4>
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
  toggleModal: PropTypes.func.isRequired,
  filterOpenMarketData: PropTypes.func.isRequired,
};

export default connect(null, mapDispatchToProps)(PricingGuideGridHeader);
