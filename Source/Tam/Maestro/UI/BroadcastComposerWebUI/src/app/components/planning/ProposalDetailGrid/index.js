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
    const columns = [
      {
        name: 'Type',
        dataIndex: 'Type',
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
          dataIndex: 'Units',
         // editable: true,
          /* validator: ({ value, values }) => value.length > 0,
          change: ({ values }) => ({
            otherColDataIndex: 'newValue',
          }), */
          editable: ({ row }) => {
          // editable: (...args) => {
            // console.log('edit units', args);
            console.log('edit Unit editable', row);
            const values = row.values;
            // TODO handle isHiatus, readOnly
            if (values.Type === 'total') {
                return false;
            }
            return true;
          },
          width: '20%',
          renderer: ({ value, row }) => {
            // TODO - edit indicator styling, etc
            if (row.Type === 'total') return row.TotalUnits ? numeral(row.TotalUnits).format('0,0') : '-';
            if (row.Type === 'quarter') {
              const cpm = numeral(row.Cpm).format('$0,0[.]00');
              return <div><span style={{ color: '#808080' }}>CPM </span><span>{cpm}</span></div>;
            }
            return numeral(value).format('0,0');
          },
      },
      {
          name: 'Imp (000)',
          dataIndex: 'Impressions',
          width: '20%',
          editable: ({ row }) => {
              console.log('edit Impressions editable', row);
              const values = row.values;
              // TODO handle isHiatus, readOnly
              if (values.Type === 'total') {
                  return false;
              }
              return true;
          },
          renderer: ({ value, row }) => {
            // TODO - edit indicator styling, etc
            if (row.Type === 'total') return row.TotalImpressions ? numeral(row.TotalImpressions / 1000).format('0,0.[000]') : '-';
            if (row.Type === 'quarter') {
              const imp = numeral(row.ImpressionGoal / 1000).format('0,0.[000]');
              return <div><span style={{ color: '#808080' }}>Imp. Goal (000)  </span><span>{imp}</span></div>;
            }
            return numeral(value / 1000).format('0,0.[000]');
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
    const testData = [{
      Id: 35,
      Type: 'quarter',
      QuarterText: '2017 Q4',
      Quarter: 4,
      Year: 2017,
      Cpm: 12.0000,
       // use similar value to sync with field - Units is CPM; Impresion is ImpressionGoal
      // Units: qtr.Cpm,
      // Impressions: qtr.ImpressionGoal
      ImpressionGoal: 3000.0,
      DistributeGoals: false,
    }, {
      Id: 158,
      Type: 'week',
      MediaWeekId: 728,
      Week: '12/4/2017',
      IsHiatus: false,
      Units: 1,
      Impressions: 1000.0,
      Cost: 12.0000,
      EndDate: '2017-12-10T00:00:00',
      StartDate: '2017-12-04T00:00:00',
    }, {
      Id: 159,
      Type: 'week',
      MediaWeekId: 729,
      Week: '12/11/2017',
      IsHiatus: false,
      Units: 1,
      Impressions: 1000.0,
      Cost: 12.0000,
      EndDate: '2017-12-17T00:00:00',
      StartDate: '2017-12-11T00:00:00',
    }, {
      Id: 160,
      Type: 'week',
      MediaWeekId: 730,
      Week: '12/18/2017',
      IsHiatus: true,
      Units: 1,
      Impressions: 1000.0,
      Cost: 12.0000,
      // Cost: 0,
      EndDate: '2017-12-24T00:00:00',
      StartDate: '2017-12-18T00:00:00',
    },
    {
      Id: 'total',
      Type: 'total',
      TotalUnits: 3,
      TotalImpressions: 3000.00,
      TotalCost: 36.0000,
    },
    ];

    return (
      // <Grid {...grid} data={this.props.post} store={this.context.store} />
      <Grid {...grid} data={testData} height="false" />
    );
  }
}

ProposalDetailGrid.defaultProps = {
  detailId: 'detailGrid',
};

ProposalDetailGrid.propTypes = {
  detailId: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
};
