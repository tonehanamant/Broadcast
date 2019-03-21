import React, { Component } from "react";
import CSSModules from "react-css-modules";
import PropTypes from "prop-types";
import { v4 } from "uuid";
import {
  Checkbox,
  ButtonToolbar,
  Button,
  FormGroup,
  FormControl
} from "react-bootstrap";
import styles from "./index.style.scss";

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
      filterText: "",
      filterOptions: [],
      matchOptions: {},
      isValidSelection: true
    };
  }

  // populates the filterOptions state with filter options prop
  componentWillMount() {
    const { filterOptions, matchOptions } = this.props;
    this.setState(
      {
        filterOptions,
        matchOptions
      },
      () => {
        this.checkSelectAll();
      }
    );
  }

  // check valid both filterOptions and matchOptions
  setValidSelections() {
    const { filterOptions, matchOptions } = this.state;
    const { hasMatchSpec } = this.props;
    let isValid =
      filterOptions.find(item => item.Selected === true) !== undefined;
    if (hasMatchSpec && isValid) {
      isValid = matchOptions.inSpec === true || matchOptions.outOfSpec === true;
    }
    this.setState({ isValidSelection: isValid });
  }

  // match check handler
  handleMatchSpeckCheck(name, checked) {
    // const change = this.state.matchOptions[name];
    this.setState(
      prevState => ({
        ...prevState,
        matchOptions: {
          ...prevState.matchOptions,
          [name]: checked
        }
      }),
      () => {
        this.setValidSelections();
      }
    );
  }

  applyCheckToAll(selectAll) {
    const { filterOptions } = this.state;
    const filterOptionsMap = filterOptions.map(option => {
      return {
        ...option,
        Selected: selectAll
      };
    });

    this.setState({
      selectAll,
      filterOptionsMap
    });
    this.setValidSelections();
  }

  checkSelectAll() {
    const { filterOptions } = this.state;
    const allChecked =
      filterOptions.find(item => item.Selected === false) === undefined;
    this.setState({ selectAll: allChecked });
  }

  // filterOption checked handler
  handleOptionChecked(changedOption) {
    const { filterOptions } = this.state;
    const filterOptionsMap = filterOptions.map(option => {
      if (option.Value === changedOption.Value) {
        return { ...option, Selected: !option.Selected };
      }
      return option;
    });

    this.setState({ filterOptionsMap }, () => {
      this.checkSelectAll();
      this.setValidSelections();
    });
  }

  // clear all filters
  clear() {
    const { filterKey, applySelection } = this.props;
    const { filterOptions, matchOptions } = this.state;
    this.applyCheckToAll(true);
    // change the matchOptions to true or revise to send simplified props?
    this.setState(
      prevState => ({
        ...prevState,
        matchOptions: {
          ...prevState.matchOptions,
          inSpec: true,
          outOfSpec: true
        }
      }),
      () => {
        applySelection({
          filterKey,
          exclusions: [],
          filterOptions,
          activeMatch: false,
          matchOptions
        });
      }
    );
  }

  // apply filters - filterOptions and matchOptions if applicable
  // change to send unselected as flat array of values - exclusions; send all options
  apply() {
    const { hasMatchSpec, applySelection, filterKey } = this.props;
    const { filterOptions, matchOptions } = this.state;
    const unselectedValues = filterOptions.reduce((result, option) => {
      if (!option.Selected) {
        result.push(option.Value);
      }

      return result;
    }, []);
    // determine if needs matching spec
    let activeMatch = false;
    if (hasMatchSpec) {
      activeMatch =
        matchOptions.inSpec === false || matchOptions.outOfSpec === false;
    }
    const filter = {
      filterKey,
      exclusions: unselectedValues,
      filterOptions,
      activeMatch,
      matchOptions
    };
    applySelection(filter);
  }

  render() {
    const {
      filterOptions,
      filterText,
      matchOptions,
      selectAll,
      isValidSelection
    } = this.state;
    const { hasMatchSpec, hasTextSearch } = this.props;
    if (!filterOptions) {
      return null;
    }
    // disable buttons if no options
    const canFilter = filterOptions.length > 0;
    // create the checkbox array considering the current text filter
    const checkboxes = filterOptions.reduce((result, option) => {
      // use startsWith or includes? included for contains
      if (
        !filterText ||
        option.Display.toLowerCase().includes(filterText.toLowerCase())
      ) {
        result.push(
          <Checkbox
            key={v4()}
            defaultChecked={option.Selected}
            onChange={() => this.handleOptionChecked(option)}
          >
            {option.Display}
          </Checkbox>
        );
      }
      return result;
    }, []);

    return (
      <div>
        {hasMatchSpec && (
          <FormGroup>
            <Checkbox
              inline
              key={v4()}
              disabled={!canFilter}
              defaultChecked={matchOptions.inSpec}
              onClick={() =>
                this.handleMatchSpeckCheck("inSpec", !matchOptions.inSpec)
              }
            >
              In Spec
            </Checkbox>
            <Checkbox
              inline
              key={v4()}
              disabled={!canFilter}
              defaultChecked={matchOptions.outOfSpec}
              onClick={() =>
                this.handleMatchSpeckCheck("outOfSpec", !matchOptions.outOfSpec)
              }
            >
              Out Spec
            </Checkbox>
          </FormGroup>
        )}
        <FormGroup controlId="filterSearch">
          <FormControl
            type="text"
            placeholder="Search..."
            style={
              !hasTextSearch
                ? {
                    visibility: "hidden",
                    position: "relative",
                    marginBottom: "-35px"
                  }
                : {}
            }
            onChange={e => this.setState({ filterText: e.target.value })}
            value={filterText}
          />
        </FormGroup>
        <div styleName="filter-list-checkbox-container">
          <Checkbox
            key={v4()}
            defaultChecked={selectAll}
            disabled={!canFilter}
            onClick={() => this.applyCheckToAll(!selectAll)}
          >
            <strong>Select All</strong>
          </Checkbox>
          {checkboxes}
        </div>
        <ButtonToolbar styleName="pull-right" style={{ margin: "0 0 8px 0" }}>
          <Button
            bsStyle="success"
            bsSize="xsmall"
            onClick={this.clear}
            disabled={!canFilter}
          >
            Clear
          </Button>

          <Button
            bsStyle="success"
            bsSize="xsmall"
            disabled={!canFilter || !isValidSelection}
            style={{ marginLeft: "10px" }}
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
  applySelection: () => {}
};

FilterInput.propTypes = {
  hasTextSearch: PropTypes.bool,
  hasMatchSpec: PropTypes.bool,
  applySelection: PropTypes.func,
  matchOptions: PropTypes.object.isRequired,
  filterOptions: PropTypes.array.isRequired,
  filterKey: PropTypes.string.isRequired
};

export default CSSModules(FilterInput, styles);
