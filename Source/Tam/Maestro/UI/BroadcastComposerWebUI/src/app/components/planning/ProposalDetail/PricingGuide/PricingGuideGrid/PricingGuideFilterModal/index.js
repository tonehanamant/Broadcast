import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Form, FormGroup, Col, Glyphicon } from 'react-bootstrap';
import Select from 'react-select';
import { pipe, sortBy, concat, keys, pickBy, mapValues, omit } from 'lodash/fp';
import { defaultFiltersItems, filterMap } from './util';

import './index.scss';

const mapValuesWithKey = mapValues.convert({ cap: false });


const mapStateToProps = ({ app: { modals: { pricingFilterModal: modal } } }) => ({
  modal,
});

const initialState = {
  filtersItems: defaultFiltersItems,
  filtersRender: [],
  filtersValues: {},
};


class PricingGuideFilterModal extends Component {
  constructor(props) {
    super(props);

    this.onAddFilter = this.onAddFilter.bind(this);
    this.onEditFilter = this.onEditFilter.bind(this);
    this.onDeleteFilter = this.onDeleteFilter.bind(this);
    this.onChangeFilter = this.onChangeFilter.bind(this);
    this.closeModal = this.closeModal.bind(this);
    this.clearState = this.clearState.bind(this);
    this.handleSave = this.handleSave.bind(this);
    this.getFiltersForSave = this.getFiltersForSave.bind(this);
    this.generateFiltersState = this.generateFiltersState.bind(this);

    this.state = initialState;
  }


  componentWillReceiveProps(nextProps) {
    const { modal } = this.props;
    if (!modal.active && nextProps.modal.active) {
      this.generateFiltersState();
    }
  }

  generateFiltersState() {
    const { activeOpenMarketData: { Filter, DisplayFilter } } = this.props;
    // omit SpotFilter from modal version Filter: todo seprate in Reducer?
    const AdjustFilter = Filter.SpotFilter ? omit(['SpotFilter'], Filter) : Filter;
    const selectedFilters = keys(AdjustFilter);
    const filtersOptions = mapValuesWithKey((value, key) => {
      console.log();
      return filterMap[key].getInitialData(value);
    })(DisplayFilter);

    this.setState({
      filtersItems: defaultFiltersItems.filter(it => !selectedFilters.includes(it.Id)),
      filtersRender: defaultFiltersItems.filter(it => selectedFilters.includes(it.Id)),
      filtersValues: mapValuesWithKey((value, key) => filterMap[key].preTransformer(AdjustFilter[key], filtersOptions[key]))(AdjustFilter),
      filtersOptions,
    });
  }

  onAddFilter(filter) {
    const { filtersRender, filtersItems } = this.state;
    this.setState({
      filtersRender: filtersRender.concat(filter),
      filtersItems: filtersItems.filter(({ Id }) => (Id !== filter.Id)),
    });
  }

  onDeleteFilter(filter) {
    const { filtersRender, filtersItems, filtersValues } = this.state;
    const newFilterOptions = pipe(
      concat(filter),
      sortBy('order'),
    )(filtersItems);

    this.setState({
      filtersRender: filtersRender.filter(({ Id }) => (Id !== filter.Id)),
      filtersItems: newFilterOptions,
      filtersValues: omit(filter.Id, filtersValues),
    });
  }

  onEditFilter(newFilter) {
    const { filtersRender, filtersValues } = this.state;
    this.setState({
      filtersRender: filtersRender.map(it => ((it.Id === newFilter.Id) ? newFilter : it)),
      filtersValues: Object.assign(filtersValues, { [newFilter.Id]: null }),
    });
  }

  onChangeFilter(filter, value) {
    const { filtersValues } = this.state;
    this.setState({
      filtersValues: Object.assign(filtersValues, { [filter.Id]: value }),
    });
  }

  closeModal() {
    this.props.toggleModal({
      modal: 'pricingFilterModal',
      active: false,
    });
    this.clearState();
  }

  clearState() {
    this.setState(initialState);
  }

  getFiltersForSave() {
    const { filtersValues, filtersOptions } = this.state;
    const parsedFilters = pipe(
      pickBy(filter => !!filter.length),
      mapValuesWithKey((value, key) => (filterMap[key].postTransformer ? filterMap[key].postTransformer(value, filtersOptions[key]) : value)),
    )(filtersValues);
    return { Filter: parsedFilters };
  }

  handleSave() {
    const filters = this.getFiltersForSave();
    this.props.applyFilters(filters, true);
    this.closeModal();
  }

  render() {
    const { modal } = this.props;
    const { filtersItems, filtersRender, filtersValues, filtersOptions } = this.state;

    return (
      <Modal show={modal.active}>
        <Modal.Header>
          <Button className="close" bsStyle="link" onClick={this.closeModal} style={{ display: 'inline-block', float: 'right' }}>
          <span>&times;</span>
        </Button>
        <Modal.Title>Filter Programs</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <Form horizontal>
            {filtersRender.map((it, idx) => (
              <FormGroup key={`pricing-filter-${it.Id}`} controlId={`filter-${it.Id}-${it.idx}`}>
                <Col sm={4}>
                  <Select
                    value={it}
                    onChange={(filter) => { this.onEditFilter(filter, it, idx); }}
                    options={filtersItems}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                  />
                </Col>
                <Col sm={6}>
                  {filterMap[it.Id].render(
                    filtersValues[it.Id],
                    (value) => { this.onChangeFilter(it, value); },
                    filtersOptions[it.Id],
                  )}
                </Col>
                <Col sm={2}>
                  <Button className="remove-button" onClick={() => { this.onDeleteFilter(it); }}>
                    <Glyphicon glyph="remove" />
                  </Button>
                </Col>
              </FormGroup>
            ))}
            <FormGroup controlId="filter-selector">
              <Col sm={4}>
                <Select
                  onChange={this.onAddFilter}
                  options={filtersItems}
                  labelKey="Display"
                  valueKey="Id"
                  clearable={false}
                  placeholder="Add Criteria..."
                />
              </Col>
            </FormGroup>
          </Form>
        </Modal.Body>
        <Modal.Footer>
            <Button onClick={this.closeModal} bsStyle="danger">Cancel</Button>
            <Button onClick={this.handleSave} bsStyle="success">Apply</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PricingGuideFilterModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  activeOpenMarketData: PropTypes.object.isRequired,
  applyFilters: PropTypes.func.isRequired,
};

PricingGuideFilterModal.defaultProps = {
  modal: {
    active: false,
  },
};

export default connect(mapStateToProps)(PricingGuideFilterModal);
