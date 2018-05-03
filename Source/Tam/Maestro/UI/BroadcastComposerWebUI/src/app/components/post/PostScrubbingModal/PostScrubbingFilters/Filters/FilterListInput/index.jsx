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
    this.checkSelectAll = this.checkSelectAll.bind(this);
    this.handleMatchSpeckCheck = this.handleMatchSpeckCheck.bind(this);
    this.state = {
      selectAll: true,
      filterText: '',
      filterOptions: [],
      matchOptions: {},
      isValidSelection: true,
    };
  }

  // populates the filterOptions state with filter options prop
  componentWillMount() {
    this.setState({ filterOptions: this.props.filterOptions, matchOptions: this.props.matchOptions }, () => {
      this.checkSelectAll();
    });
  }

  // match check handler
  handleMatchSpeckCheck(name, checked) {
    // const change = this.state.matchOptions[name];
    this.setState(prevState => ({
      ...prevState,
      matchOptions: {
        ...prevState.matchOptions,
        [name]: checked,
      },
    }), () => {
      this.setValidSelections();
    });
  }

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
    // this.setValidSelections(selectAll);
    this.setValidSelections();
  }

  // check select all to handle select all state
  checkSelectAll() {
    const allChecked = this.state.filterOptions.find(item => item.Selected === false) === undefined;
    // console.log('check select all', allChecked);
    this.setState({ selectAll: allChecked });
  }

  // filterOption checked handler
  handleOptionChecked(changedOption) {
    const filterOptions = this.state.filterOptions.map((option) => {
      if (option.Value === changedOption.Value) {
        return { ...option, Selected: !option.Selected };
      }
      return option;
    });

    this.setState({ filterOptions }, () => {
      this.checkSelectAll();
      this.setValidSelections();
    });
  }
  // clear all filters
  clear() {
    // needed ?
    this.applyCheckToAll(true);
    // change the matchOptions to true or revise to send simplified props?
    this.setState(prevState => ({
      ...prevState,
      matchOptions: {
        ...prevState.matchOptions,
        inSpec: true,
        outSpec: true,
      },
    }), () => {
      this.props.applySelection({ filterKey: this.props.filterKey, exclusions: [], filterOptions: this.state.filterOptions, activeMatch: false, matchOptions: this.state.matchOptions });
    });
  }
  // apply filters - filterOptions and matchOptions if applicable
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    const unselectedValues = this.state.filterOptions.reduce((result, option) => {
      if (!option.Selected) {
        result.push(option.Value);
      }

      return result;
    }, []);
    // determine if needs matching spec
    let activeMatch = false;
    if (this.props.hasMatchSpec) {
      activeMatch = (this.state.matchOptions.inSpec === false) || (this.state.matchOptions.outSpec === false);
    }
    const filter = { filterKey: this.props.filterKey, exclusions: unselectedValues, filterOptions: this.state.filterOptions, activeMatch, matchOptions: this.state.matchOptions };
     // console.log('apply', filter);
    this.props.applySelection(filter);
  }

  // check valid both filterOptions and matchOptions
  setValidSelections() {
    let isValid = this.state.filterOptions.find(item => item.Selected === true) !== undefined;
    if (this.props.hasMatchSpec && isValid) {
      isValid = (this.state.matchOptions.inSpec === true) || (this.state.matchOptions.outSpec === true);
    }
    this.setState({ isValidSelection: isValid });
  }

  render() {
    const { filterOptions, filterText } = this.state;

    if (!filterOptions) {
      return null;
    }
    const hasMatchSpec = this.props.hasMatchSpec;
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
        { hasMatchSpec &&
        <FormGroup>
          <Checkbox
            inline
            key={v4()}
            defaultChecked={this.state.matchOptions.inSpec}
            onClick={() => this.handleMatchSpeckCheck('inSpec', !this.state.matchOptions.inSpec)}
          >
          In Spec
          </Checkbox>
          <Checkbox
            inline
            key={v4()}
            defaultChecked={this.state.matchOptions.outSpec}
            onClick={() => this.handleMatchSpeckCheck('outSpec', !this.state.matchOptions.outSpec)}
          >
          Out Spec
          </Checkbox>
        </FormGroup>
        }
        <FormGroup controlId="filterSearch">
          <FormControl
            type="text"
            placeholder="Search..."
            style={!this.props.hasTextSearch ? { visibility: 'hidden', position: 'relative', marginBottom: '-35px' } : {}}
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
  hasTextSearch: true,
  hasMatchSpec: false,
  applySelection: () => {},
};

FilterInput.propTypes = {
  hasTextSearch: PropTypes.bool,
  hasMatchSpec: PropTypes.bool,
  applySelection: PropTypes.func,
  matchOptions: PropTypes.object.isRequired,
  filterOptions: PropTypes.array.isRequired,
  filterKey: PropTypes.string.isRequired,
};

export default CSSModules(FilterInput, styles);
