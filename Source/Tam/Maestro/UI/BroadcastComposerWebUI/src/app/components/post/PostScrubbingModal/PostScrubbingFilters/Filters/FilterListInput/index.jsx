import React, { Component } from 'react';
import CSSModules from 'react-css-modules';
import PropTypes from 'prop-types';
import { v4 } from 'uuid';
import { Checkbox, ButtonToolbar, Button, FormGroup, FormControl } from 'react-bootstrap/lib/';
import styles from './index.scss';

class FilterInput extends Component {
  constructor(props) {
    super(props);

    this.applyCheckToAll = this.applyCheckToAll.bind(this);
    this.handleOptionChecked = this.handleOptionChecked.bind(this);
    this.apply = this.apply.bind(this);
    this.clear = this.clear.bind(this);
    this.setValidSelections = this.setValidSelections.bind(this);

    this.state = {
      selectAll: true,
      filterText: '',
      filterOptions: [],
      isValidSelection: true,
    };
  }

  // populates the filterOptions state with filter options prop
  componentWillMount() {
    // console.log('input mount', this.props);
    this.setState({ filterOptions: this.props.filterOptions });
  }

  /* componentWillReceiveProps(nextProps) {
    console.log('filter input  receive props', nextProps, this);
  } */

  // applies the 'selectAll' parameter as the selected property of each filter option
  applyCheckToAll(selectAll) {
    /* eslint-disable no-param-reassign */
    const filterOptions = this.state.filterOptions.map((option) => {
      option.Selected = selectAll;
      return option;
    });
    /* eslint-enable no-param-reassign */

    this.setState({
      selectAll,
      filterOptions,
    });
    this.setValidSelections(selectAll);
  }

  handleOptionChecked(changedOption) {
    const filterOptions = this.state.filterOptions.map((option) => {
      if (option.Value === changedOption.Value) {
        return { ...option, Selected: !option.Selected };
      }

      return option;
    });
    this.setState({ filterOptions });
    setTimeout(() => {
      const valid = this.state.filterOptions.find(item => item.Selected === true) !== undefined;
      // console.log('valid', valid, this.state.filterOptions);
      this.setValidSelections(valid);
    }, 200);
  }

  clear() {
    // needed ?
    this.applyCheckToAll(true);
    // this.apply();
    this.props.applySelection({ filterKey: this.props.filterKey, exclusions: [], filterOptions: this.state.filterOptions });
  }
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    const unselectedValues = this.state.filterOptions.reduce((result, option) => {
      if (!option.Selected) {
        /* result.push({
          Display: option.Display,
          Value: option.Value,
        }); */
        result.push(option.Value);
      }

      return result;
    }, []);
    const filter = { filterKey: this.props.filterKey, exclusions: unselectedValues, filterOptions: this.state.filterOptions };
     // console.log('apply', filter);
    this.props.applySelection(filter);
  }

  // set allow apply if no slections TBD: either iterate check or rely on arguments
  setValidSelections(valid) {
    this.setState({ isValidSelection: valid });
  }

  render() {
    const { filterOptions, filterText } = this.state;

    if (!filterOptions) {
      return null;
    }

    // create the checkbox array considering the current text filter
    const checkboxes = filterOptions.reduce((result, option) => {
      // use startsWith or includes? included for contains
      if (!filterText || option.Display.toLowerCase().includes(filterText.toLowerCase())) {
        result.push(
          <Checkbox
            key={v4()}
            defaultChecked={option.Selected}
            onChange={() => this.handleOptionChecked(option)}
          >
            {option.Display}
          </Checkbox>);
      }
      return result;
    }, []);

    return (
      <div>
        <FormGroup controlId="filterSearch">
          <FormControl
            type="text"
            placeholder="Search..."
            style={!this.props.textSearch ? { visibility: 'hidden', position: 'relative', marginBottom: '-35px' } : {}}
            onChange={e => this.setState({ filterText: e.target.value })}
            value={this.state.filterText}
          />
        </FormGroup>
        <div className="filter-list-checkbox-container">
          <Checkbox
            key={v4()}
            defaultChecked={this.state.selectAll}
            onClick={() => this.applyCheckToAll(!this.state.selectAll)}
          >
            <strong>Select All</strong>
          </Checkbox>
          {checkboxes}
        </div>
        <ButtonToolbar style={{ minWidth: '90%', margin: '0px auto' }}>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            // onClick={() => this.applyCheckToAll(false)}
            onClick={this.clear}
          >
            Clear
          </Button>

          <Button
            bsStyle="success"
            bsSize="xsmall"
            disabled={!this.state.isValidSelection}
            style={{ marginLeft: '10px' }}
            onClick={this.apply}
          >
            Apply
          </Button>
        </ButtonToolbar>
      </div>
    );
  }
}

FilterInput.defaultProps = {
  textSearch: true,
  applySelection: () => {},
};

FilterInput.propTypes = {
  textSearch: PropTypes.bool,
  applySelection: PropTypes.func,
  filterOptions: PropTypes.array.isRequired,
  filterKey: PropTypes.string.isRequired,
};

export default CSSModules(FilterInput, styles);
