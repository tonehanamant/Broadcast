import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { Modal, Button, Form, FormGroup, Col, Glyphicon } from 'react-bootstrap';
import Select from 'react-select';
import { defaultFiltersOptions, filterMap } from './util';

import './index.scss';


const mapStateToProps = ({ app: { modals: { pricingFilterModal: modal } } }) => ({
  modal,
});

const initialState = {
  filtersOptions: defaultFiltersOptions,
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

    this.state = initialState;
  }

  onAddFilter(filter) {
    const { filtersRender, filtersOptions } = this.state;
    this.setState({
      filtersRender: filtersRender.concat(filter),
      filtersOptions: filtersOptions.filter(({ Id }) => (Id !== filter.Id)),
    });
  }

  onDeleteFilter(filter) {
    const { filtersRender, filtersOptions, filtersValues } = this.state;
    this.setState({
      filtersRender: filtersRender.filter(({ Id }) => (Id !== filter.Id)),
      filtersOptions: filtersOptions.concat(filter),
      filtersValues: Object.assign(filtersValues, { [filter.Id]: null }),
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
    // console.log('onChangeFilter', filtersValues);
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

  // get values for BE conversion; TBD
  // todo get other values as needed
  // object assign to existing filters?
  getFiltersForSave() {
    const values = this.state.filtersValues;
    const ProgramNames = [];
    if (values.programName && values.programName.length) {
      values.programName.forEach((item) => {
        ProgramNames.push(item.Id);
      });
    }
    return { Filter: { ProgramNames } };
  }

  handleSave() {
    // console.log(this.state);
    const filters = this.getFiltersForSave();
    // console.log('handleSave', filters);
    this.props.applyFilters(filters);
    this.closeModal(); // do here or after success?
  }

  render() {
    const { modal, activeOpenMarketData } = this.props;
    const { filtersOptions, filtersRender, filtersValues } = this.state;
    // console.log('displayFilter and Filter', activeOpenMarketData.DisplayFilter, activeOpenMarketData.Filter);

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
                    options={filtersOptions}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                  />
                </Col>
                <Col sm={6}>
                  {filterMap[it.Id].render(
                    filtersValues[it.Id],
                    (value) => { this.onChangeFilter(it, value); },
                    filterMap[it.Id].getInitialData(activeOpenMarketData.DisplayFilter),
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
                  options={filtersOptions}
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
