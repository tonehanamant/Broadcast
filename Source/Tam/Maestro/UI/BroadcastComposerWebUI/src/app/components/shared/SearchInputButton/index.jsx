import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { FormGroup, InputGroup, FormControl, Button } from 'react-bootstrap';

/* eslint-disable react/prefer-stateless-function */
export default class SearchInputButton extends Component {
  constructor(props) {
		super(props);
		// this.handleChange = this.handleChange.bind(this);
		this.clearForm = this.clearForm.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
    this.state = {
      hasActiveSearch: false,
    };
	}

	clearForm() {
		this.searchField.value = '';
    this.handleSubmit();
	}

	handleSubmit() {
    this.props.submitAction(this.searchField.value);
    let showClear = false;
    if (this.searchField.value && this.searchField.value.length) {
      showClear = true;
    }
    this.setState({
      hasActiveSearch: showClear,
    });
  }

  handleKeyPress(target) {
    if (target.charCode === 13) {
      // console.log('handleKeyPress', event.keyCode);
      this.handleSubmit();
    }
  }

  render() {
		/* eslint-disable no-return-assign */
    return (
			<FormGroup bsSize="small" style={{ maxWidth: 250, float: 'right' }}>
				<InputGroup>
					<FormControl type="text" placeholder={this.props.fieldPlaceHolder} inputRef={input => this.searchField = input} onKeyPress={this.handleKeyPress} />
          {this.state.hasActiveSearch &&
          <InputGroup.Button className="search-input-clear-btn-group">
            <Button className="search-input-clear-btn" type="reset" bsStyle="link" bsSize="small" onClick={this.clearForm}>
              <span className="glyphicon glyphicon-remove" />
            </Button>
          </InputGroup.Button>
          }
          <InputGroup.Button>
            <Button type="submit" bsStyle="info" bsSize="small" onClick={this.handleSubmit}>
              <span className="glyphicon glyphicon-search" />
            </Button >
          </InputGroup.Button>
				</InputGroup>
			</FormGroup>
    );
	}
}

SearchInputButton.propTypes = {
	// inputAction: PropTypes.func.isRequired,
	submitAction: PropTypes.func.isRequired,
	fieldPlaceHolder: PropTypes.string.isRequired,
};
