import React, { Component } from 'react';
import PropTypes from 'prop-types';
// import { connect } from 'react-redux';
// import { bindActionCreators } from 'redux';
import { Grid } from 'react-redux-grid';
// import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';
import numeral from 'numeral';

import GridCellInput from 'Components/shared/GridCellInput';
import GridIsciCell from './GridIsciCell';

/* eslint-disable no-unused-vars */
/* eslint-disable no-shadow */

export default class ProposalDetailGrid extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.checkEditable = this.checkEditable.bind(this);
    this.isciCellItems = {};
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
    // console.log('checkEditable', can);
    return can;
  }

   /* ////////////////////////////////// */
  /* // COMPONENT RENDER FUNC
  /* ////////////////////////////////// */
  render() {
    const { proposalValidationStates } = this.props;
    const { DetailGridsInvalid } = proposalValidationStates;

    /* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */
    // TODO will need to be unique to each grid
    const stateKey = `detailGrid_${this.props.detailId}`;
    // console.log('DETAIL GRID PROPS', stateKey, this.props);

    /* GRID COLUMNS */
    // See saga FlattenDetail for data structure
    // NORMALIZE editing values for grid display/editing: EditUnits(quarter Cpm, week Units) EditImpressions (quarter ImpressionGoal, week Impressions)
    const columns = [
      {
        name: 'Type',
        dataIndex: 'Type',
        hidden: true,
      },
      {
        name: 'IsHiatus',
        dataIndex: 'IsHiatus',
        hidden: true,
      },
      {
          name: 'Week',
          dataIndex: 'Week',
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
          width: '20%',
          renderer: ({ value, row }) => {
            const isEditable = this.checkEditable(row, false);

            const inputCpm = (event) => {
              let unmaskedValue = event.target.value.replace(/CPM \$ /, '');
                  unmaskedValue = unmaskedValue.replace(/,/g, '');
              const storeValue = Number(unmaskedValue);
              const storeKey = event.target.getAttribute('valueKey');
              this.props.updateProposalEditFormDetailGrid({
                id: this.props.detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: null,
                key: storeKey,
                value: storeValue,
                row: row._key,
              });
              this.props.onUpdateProposal();
            };

            const inputUnits = (event) => {
              const storeValue = Number(event.target.value);
              const storeKey = event.target.getAttribute('valueKey');
              this.props.updateProposalEditFormDetailGrid({
                id: this.props.detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: row.WeekIdx,
                key: storeKey,
                value: storeValue,
                row: row._key,
              });
              this.props.onUpdateProposal();
            };

            if (row.Type === 'total') return row.TotalUnits ? numeral(row.TotalUnits).format('0,0') : '-';
            if (row.Type === 'quarter') {
              // const cpm = numeral(value).format('$0,0[.]00');
              // return <div><span style={{ color: '#808080' }}>CPM </span><span>{cpm}</span></div>;
              return (
                <GridCellInput
                  name="Cpm"
                  placeholder="CPM $"
                  value={value}
                  valueKey="Cpm"
                  isEditable={isEditable}
                  emptyZeroDefault
                  onSaveShowValidation={DetailGridsInvalid}
                  blurAction={inputCpm}
                  enterKeyPressAction={inputCpm}
                  maskType="createNumber"
                  maskPrefix="CPM $ "
                  maskAllowDecimal
                  maskDecimalLimit={2}
                />
              );
            }
            return (
              <GridCellInput
                name="Units"
                placeholder=""
                value={value}
                valueKey="Units"
                isEditable={isEditable}
                emptyZeroDefault
                onSaveShowValidation={DetailGridsInvalid}
                blurAction={inputUnits}
                enterKeyPressAction={inputUnits}
                maskType="default"
              />
            );
          },
      },
      {
          name: 'Imp (000)',
          dataIndex: 'EditImpressions',
          width: '20%',
          renderer: ({ value, row }) => {
            const isEditable = this.checkEditable(row, false);

            const inputImpressionGoal = (event) => {
              let unmaskedValue = event.target.value.replace(/Imp Goal \(000\) /, '');
                  unmaskedValue = unmaskedValue.replace(/,/g, '');
              const storeValue = (Number(unmaskedValue) * 1000);
              const storeKey = event.target.getAttribute('valueKey');
              this.props.updateProposalEditFormDetailGrid({
                id: this.props.detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: null,
                key: storeKey,
                value: storeValue,
                row: row._key,
              });
              this.props.onUpdateProposal();
            };

            const inputImpressions = (event) => {
              const unmaskedValue = event.target.value.replace(/,/g, '');
              const storeValue = Number(unmaskedValue) * 1000;
              // const storeValue = (Number(event.target.value) * 1000);
              const storeKey = event.target.getAttribute('valueKey');
              this.props.updateProposalEditFormDetailGrid({
                id: this.props.detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: row.WeekIdx,
                key: storeKey,
                value: storeValue,
                row: row._key,
              });
              this.props.onUpdateProposal();
            };

            if (row.Type === 'total') return row.TotalImpressions ? numeral(row.TotalImpressions / 1000).format('0,0.[000]') : '-';
            if (row.Type === 'quarter') {
              // const imp = numeral(value).format('0,0.[000]');
              // return <div><span style={{ color: '#808080' }}>Imp. Goal (000)  </span><span>{imp}</span></div>;
              return (
                <GridCellInput
                  name="ImpressionGoal"
                  placeholder="Imp Goal (000)"
                  value={value}
                  valueKey="ImpressionGoal"
                  isEditable={isEditable}
                  emptyZeroDefault
                  confirmInput
                  confirmModalProperties={{
                    titleText: 'Warning',
                    bodyText: 'You will loose one or more existing weekly goals for this quarter.', // string
                    bodyList: null, // array
                    closeButtonText: 'Cancel',
                    closeButtonBsStyle: 'default',
                    actionButtonText: 'Continue',
                    actionButtonBsStyle: 'warning',
                  }}
                  toggleModal={this.props.toggleModal}
                  onSaveShowValidation={DetailGridsInvalid}
                  blurAction={inputImpressionGoal}
                  enterKeyPressAction={inputImpressionGoal}
                  maskType="createNumber"
                  maskPrefix="Imp Goal (000) "
                  maskAllowDecimal
                  maskDecimalLimit={3}
                />
              );
            }
            return (
              <GridCellInput
                name="Impressions"
                placeholder=""
                value={value}
                valueKey="Impressions"
                isEditable={isEditable}
                emptyZeroDefault
                onSaveShowValidation={DetailGridsInvalid}
                blurAction={inputImpressions}
                enterKeyPressAction={inputImpressions}
                maskType="createNumber"
                maskAllowDecimal
                maskDecimalLimit={3}
              />
            );
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
      {
        name: 'ISCIs',
        dataIndex: 'Iscis',
        width: '20%',
        editable: false,
        renderer: ({ value, row }) => {
          // console.log('ISCIs Render', value, row);
          if (row.Type === 'week' && !row.IsHiatus) {
            const inputIscis = (IscisValue, next) => {
              this.props.updateProposalEditFormDetailGrid({
                id: this.props.detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: row.WeekIdx,
                key: 'Iscis',
                value: IscisValue,
                row: row._key,
              });
              // console.log('called InputIscis', IscisValue, row, next);
              if (next) {
                const nextKey = `isci_cell_${next}`;
                const nextIsci = this.isciCellItems[nextKey];
                if (nextIsci) nextIsci.showPopover();
              }
            };
            const cellKey = `isci_cell_${row.WeekCnt}`;
            return (
            <GridIsciCell
              Iscis={value}
              saveInputIscis={inputIscis}
              hasNext={!row.IsLast}
              weekCnt={row.WeekCnt}
              ref={(ref) => { this.isciCellItems[cellKey] = ref; }}
            />
          );
        }
          // empty for quarter, total
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
        editEvent: 'none', // 'none' not go edit mode
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

    const grid = {
      columns,
      plugins,
      stateKey,
    };

    return (
      <Grid
        {...grid}
        data={this.props.GridQuarterWeeks}
        store={this.context.store}
        height="false"
      />
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
  isAdu: PropTypes.bool.isRequired,
  isReadOnly: PropTypes.bool.isRequired,
  updateProposalEditFormDetailGrid: PropTypes.func.isRequired,
  onUpdateProposal: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  proposalValidationStates: PropTypes.object.isRequired,
};
