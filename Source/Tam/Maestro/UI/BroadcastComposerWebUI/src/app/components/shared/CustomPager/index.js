import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';

// requires props: stateKey (grid.stateKey); idProperty (property in record data that indicates ID)
const mapStateToProps = ({ grid, selection, dataSource }) => ({
  // Grid
  grid,
  selection,
  dataSource,
});

/* eslint-disable react/prefer-stateless-function */
export class CustomPager extends Component {
  render() {
    const total = this.props.dataSource.get(this.props.stateKey).total;

    const selection = this.props.selection.toJS()[this.props.stateKey];
    const keys = Object.keys(selection);
    const rowKey = keys.filter(key => selection[key] === true)[0];


    const getSelectedId = () => {
      if (rowKey) {
        const data = this.props.dataSource.get(this.props.stateKey).get('data').toJS();
        const row = data.find(obj => obj._key === rowKey);
        const id = row[this.props.idProperty]; // row.Id;
        // console.log('Custom Pager', data, row, id, this.props);
        return id ? `Record Id: ${id}` : '';
      }
      return '';
    };

    const recordId = getSelectedId();

    return (
			<div className="react-grid-pager-toolbar">
				<span>{recordId}</span>
        {total ? <span>1-{total} of {total}</span> : null}
			</div>
    );
	}
}

CustomPager.defaultProps = {
  idProperty: 'Id',
};

CustomPager.propTypes = {
  // Component Accepts
  stateKey: PropTypes.string.isRequired,
  idProperty: PropTypes.isRequired,
  // Grid
  dataSource: PropTypes.object.isRequired,
  selection: PropTypes.object.isRequired,
};

export default connect(mapStateToProps)(CustomPager);
