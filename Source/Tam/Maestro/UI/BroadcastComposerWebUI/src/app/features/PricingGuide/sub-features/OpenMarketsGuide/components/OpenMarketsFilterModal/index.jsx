import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
  Modal,
  Button,
  Form,
  FormGroup,
  Col,
  Glyphicon
} from "react-bootstrap";
import Select from "react-select";
import { omit } from "lodash";
import {
  defaultFiltersItems,
  filterMap,
  generateFilter,
  getFilterValuesToRequest,
  getFilterValuesFromResponse,
  addFilterToItems
} from "./util";

import "./index.scss";

const mapStateToProps = ({
  app: {
    modals: { pricingFilterModal: modal }
  }
}) => ({
  modal
});

const initialState = {
  filtersItems: defaultFiltersItems,
  filtersRender: [],
  filtersValues: {}
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

  onAddFilter(filter) {
    const { filtersRender, filtersItems } = this.state;
    this.setState({
      filtersRender: filtersRender.concat(generateFilter(filter)),
      filtersItems: filterMap[filter.Id].isMultiple
        ? filtersItems
        : filtersItems.filter(({ Id }) => Id !== filter.Id)
    });
  }

  onDeleteFilter(filter) {
    const { filtersRender, filtersItems, filtersValues } = this.state;
    const newFilterOptions = filterMap[filter.Id].isMultiple
      ? filtersItems
      : addFilterToItems(filter, filtersItems);

    this.setState({
      filtersRender: filtersRender.filter(({ name }) => name !== filter.name),
      filtersItems: newFilterOptions,
      filtersValues: omit(filtersValues, filter.name)
    });
  }

  onEditFilter(newFilter, oldFilter) {
    const { filtersRender, filtersValues } = this.state;
    this.setState({
      filtersRender: filtersRender.map(it =>
        it.name === oldFilter.name ? generateFilter(newFilter) : it
      ),
      filtersValues: Object.assign(filtersValues, { [newFilter.Id]: null })
    });
  }

  onChangeFilter(filter, value) {
    const { filtersValues } = this.state;
    this.setState({
      filtersValues: Object.assign(filtersValues, { [filter.name]: value })
    });
  }

  getFiltersForSave() {
    const { filtersValues, filtersOptions, filtersRender } = this.state;
    const parsedFilters = getFilterValuesToRequest(
      filtersValues,
      filtersOptions,
      filtersRender
    );
    return { Filter: parsedFilters };
  }

  clearState() {
    this.setState(initialState);
  }

  generateFiltersState() {
    const {
      activeOpenMarketData: { Filter, DisplayFilter }
    } = this.props;

    const nextState = getFilterValuesFromResponse(Filter, DisplayFilter);
    this.setState(nextState);
  }

  closeModal() {
    const { toggleModal } = this.props;
    toggleModal({
      modal: "pricingFilterModal",
      active: false
    });
    this.clearState();
  }

  handleSave() {
    const { applyFilters } = this.props;
    const filters = this.getFiltersForSave();
    applyFilters(filters, true);
    this.closeModal();
  }

  render() {
    const { modal } = this.props;
    const {
      filtersItems,
      filtersRender,
      filtersValues,
      filtersOptions
    } = this.state;

    return (
      <Modal show={modal.active}>
        <Modal.Header>
          <Button
            className="close"
            bsStyle="link"
            onClick={this.closeModal}
            style={{ display: "inline-block", float: "right" }}
          >
            <span>&times;</span>
          </Button>
          <Modal.Title>Filter Programs</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <Form horizontal>
            {filtersRender.map((filter, idx) => (
              <FormGroup
                key={`pricing-filter-${filter.name}`}
                controlId={`filter-${filter.name}`}
              >
                <Col sm={4}>
                  <Select
                    value={filter}
                    onChange={newFilter => {
                      this.onEditFilter(newFilter, filter, idx);
                    }}
                    options={filtersItems}
                    labelKey="Display"
                    valueKey="Id"
                    clearable={false}
                  />
                </Col>
                <Col sm={6}>
                  {filterMap[filter.Id].render(
                    filtersValues[filter.name],
                    value => {
                      this.onChangeFilter(filter, value);
                    },
                    filtersOptions[filter.Id]
                  )}
                </Col>
                <Col sm={2}>
                  <Button
                    className="remove-button"
                    onClick={() => {
                      this.onDeleteFilter(filter);
                    }}
                  >
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
          <Button onClick={this.closeModal} bsStyle="danger">
            Cancel
          </Button>
          <Button onClick={this.handleSave} bsStyle="success">
            Apply
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

PricingGuideFilterModal.propTypes = {
  modal: PropTypes.object,
  toggleModal: PropTypes.func.isRequired,
  activeOpenMarketData: PropTypes.object.isRequired,
  applyFilters: PropTypes.func.isRequired
};

PricingGuideFilterModal.defaultProps = {
  modal: {
    active: false
  }
};

export default connect(mapStateToProps)(PricingGuideFilterModal);
