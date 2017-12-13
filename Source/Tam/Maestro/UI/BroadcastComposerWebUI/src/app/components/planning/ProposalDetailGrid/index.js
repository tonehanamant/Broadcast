import React, { Component } from 'react';
import PropTypes from 'prop-types';
// import { connect } from 'react-redux';
// import { bindActionCreators } from 'redux';
import { Grid } from 'react-redux-grid';
// import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';
import numeral from 'numeral';

/* eslint-disable no-unused-vars */
/* eslint-disable no-shadow */

export default class ProposalDetailGrid extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.checkEditable = this.checkEditable.bind(this);
  }

  checkEditable(values, isUnits) {
    // check read only - TODO future pass here
    if (this.props.isReadOnly) return false;
    let can = true;
    if (values.Type === 'total') {
      can = false;
    }
    // check for ADU and quarter if editing units
    if (isUnits && (values.Type === 'quarter') && this.props.isAdu) {
      can = false;
    }
    // no editing hiatus week level
    if ((values.Type === 'week') && values.IsHiatus) {
      can = false;
    }
    console.log('checkEditable', can);
    return can;
  }

   /* ////////////////////////////////// */
  /* // COMPONENT RENDER FUNC
  /* ////////////////////////////////// */
  render() {
    /* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */
    // TODO will need to be unique to each grid
    const stateKey = `detailGrid_${this.props.detailId}`;
    console.log('DETAIL GRID PROPS', stateKey, this.props);
    /* GRID RENDERERS */
    // const renderers = {
    //   uploadDate: ({ value, row }) => (
    //     <span>{row.DisplayUploadDate}</span>
    //   ),
    //   modifiedDate: ({ value, row }) => (
    //     <span>{row.DisplayModifiedDate}</span>
    //   ),
    // };

    /* GRID COLUMNS */
    // See saga FlattenDetail for data structure
    // NORMALIZE editing values for grid display/editing: EditUnits(quarter Cpm, week Units) EditImpressions (quarter ImpressionGoal, week Impressions)
    const columns = [
      {
        name: 'Type',
        dataIndex: 'Type',
        editable: false,
        // width: '20%',
        hidden: true,
      },
      {
        name: 'IsHiatus',
        dataIndex: 'IsHiatus',
        editable: false,
        // width: '20%',
        hidden: true,
      },
      {
          name: 'Week',
          dataIndex: 'Week',
          editable: false,
          width: '20%',
          renderer: ({ value, row }) => {
            // console.log('CELL >>>>>>>>>', row);
            if (row.Type === 'total') return 'Totals';
            if (row.Type === 'quarter') return <strong>{row.QuarterText}</strong>;
            // grey id week and IsHiatus
            return (row.IsHiatus) ? <span style={{ color: '#8f8f8f' }}>{value}</span> : <span>{value}</span>;
          },
      },
      {
          name: 'Units',
          dataIndex: 'EditUnits',
         // editable: true,
          validator: ({ row }) => {
            console.log('validator', row);
            const value = row.value;
            return value.length > 0;
          },
         /* change: ({ values }) => ({
            otherColDataIndex: 'newValue',
          }), */
          editable: ({ row }) => {
          // editable: (...args) => {
            // console.log('edit units', args);
            console.log('edit Unit editable row', row);
            const values = row.values;
            return this.checkEditable(values, true);
          },
          width: '20%',
          renderer: ({ value, row }) => {
            // TODO - edit indicator styling, validation etc
            // now based on EditUnits
            if (row.Type === 'total') return row.TotalUnits ? numeral(row.TotalUnits).format('0,0') : '-';
            if (row.Type === 'quarter') {
              const cpm = numeral(value).format('$0,0[.]00');
              return <div><span style={{ color: '#808080' }}>CPM </span><span>{cpm}</span></div>;
            }
            return numeral(value).format('0,0');
          },
      },
      {
          name: 'Imp (000)',
          dataIndex: 'EditImpressions',
          width: '20%',
          editable: ({ row }) => {
            const values = row.values;
            return this.checkEditable(values, false);
          },
          renderer: ({ value, row }) => {
            // TODO - edit indicator styling, validation etc
            // now based on EditImpressions
            if (row.Type === 'total') return row.TotalImpressions ? numeral(row.TotalImpressions / 1000).format('0,0.[000]') : '-';
            if (row.Type === 'quarter') {
              const imp = numeral(value).format('0,0.[000]');
              return <div><span style={{ color: '#808080' }}>Imp. Goal (000)  </span><span>{imp}</span></div>;
            }
            return numeral(value).format('0,0.[000]');
          },
      },
      {
          name: 'Cost',
          dataIndex: 'Cost',
          width: '20%',
          editable: false,
          renderer: ({ value, row }) => {
            if (row.Type === 'total') return row.TotalCost ? numeral(row.TotalCost).format('$0,0[.]00') : '-';
            if (row.Type === 'week') return row.Cost ? numeral(row.Cost).format('$0,0[.]00') : '-';
            // empty for quarter
            return '';
          },
      },
    ];

    /* GRID PLGUINS */
    const plugins = {
      COLUMN_MANAGER: {
        resizable: true,
        moveable: false,
        sortable: {
            enabled: false,
            method: 'local',
        },
      },
      EDITOR: {
        type: 'inline',
        enabled: true,
      },

      LOADER: {
        enabled: false,
      },
      SELECTION_MODEL: {
        mode: 'single',
        enabled: true,
        allowDeselect: true,
        editEvent: 'singleclick',
        activeCls: 'active',
        selectionEvent: 'singleclick',
      },

      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells, row }) => {
          // const stateKey = cells[0].props.stateKey;
          // const rowId = cells[0].props.rowId;
         // console.log('ROW>>>>>>', rowProps, cells, row);
          const type = cells[0].props.row.Type;
          let rowStyle = {};
          if (type === 'quarter') rowStyle = { backgroundColor: '#dedede' };
          if (type === 'total') rowStyle = { backgroundColor: '#eef5eb', fontWeight: 'bold', borderTop: '2px solid #c5c5c5' };

          return (
            <tr style={rowStyle} {...rowProps}>{ cells }</tr>
          );
        },
      },
    };

   // const height = { height: false };

    const grid = {
      columns,
      plugins,
      stateKey,
      // height,
    };

    return (
      <Grid {...grid} data={this.props.GridQuarterWeeks} store={this.context.store} height="false" />
    );
  }
}

ProposalDetailGrid.defaultProps = {
  detailId: 'detailGrid',
  isReadOnly: false,
};

ProposalDetailGrid.propTypes = {
  detailId: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
  GridQuarterWeeks: PropTypes.array.isRequired,
  isReadOnly: PropTypes.bool.isRequired,
  isAdu: PropTypes.bool.isRequired,
};
