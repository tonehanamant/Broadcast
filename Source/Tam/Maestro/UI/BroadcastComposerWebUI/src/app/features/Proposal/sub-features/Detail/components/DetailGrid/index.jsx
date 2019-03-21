/* eslint-disable no-underscore-dangle */
import React, { Component } from "react";
import PropTypes from "prop-types";
import { Grid } from "Lib/react-redux-grid";
import numeral from "numeral";
import GridCellInput from "Patterns/GridCellInput";
import GridTextInput from "Patterns/GridTextInput";
import GridIsciCell from "./GridIsciCell";

const keyPress = event => {
  const re = /[0-9a-zA-Z ]+/g;
  if (!re.test(event.key)) {
    event.preventDefault();
  }
  if (event.key === "Enter") {
    event.currentTarget.blur();
  }
};
export default class ProposalDetailGrid extends Component {
  constructor(props) {
    super(props);

    this.isciCellItems = {};
    this.checkEditable = this.checkEditable.bind(this);

    this.state = {
      DetailGridsInvalid: props.proposalValidationStates.DetailGridsInvalid
    };
  }

  componentWillReceiveProps(nextProps) {
    const { proposalValidationStates } = this.props;
    if (
      proposalValidationStates.DetailGridsInvalid !==
      nextProps.proposalValidationStates.DetailGridsInvalid
    ) {
      this.setState({
        DetailGridsInvalid:
          nextProps.proposalValidationStates.DetailGridsInvalid
      });
    }
  }

  checkEditable(values, isUnits) {
    const { isReadOnly, isAdu } = this.props;
    if (isReadOnly) return false;
    let can = true;
    if (values.Type === "total") {
      can = false;
    }
    if (isUnits && values.Type === "quarter" && isAdu) {
      can = false;
    }
    if (values.Type === "week" && values.IsHiatus) {
      can = false;
    }
    return can;
  }

  render() {
    const { DetailGridsInvalid } = this.state;
    const {
      detailId,
      updateProposalEditFormDetailGrid,
      isGridCellEdited,
      toggleModal,
      isISCIEdited,
      GridQuarterWeeks,
      proposalValidationStates,
      isReadOnly,
      onUpdateProposal
    } = this.props;
    const stateKey = `detailGrid_${detailId}`;
    const columns = [
      {
        name: "Type",
        dataIndex: "Type",
        hidden: true
      },
      {
        name: "IsHiatus",
        dataIndex: "IsHiatus",
        hidden: true
      },
      {
        name: "Week",
        dataIndex: "Week",
        width: "20%",
        renderer: ({ value, row }) => {
          if (row.Type === "total") return "Totals";
          if (row.Type === "quarter") return <strong>{row.QuarterText}</strong>;
          return row.IsHiatus ? (
            <span style={{ color: "#8f8f8f" }}>{value}</span>
          ) : (
            <span>{value}</span>
          );
        }
      },
      {
        name: "Units",
        dataIndex: "EditUnits",
        width: "20%",
        renderer: ({ value, row }) => {
          const isEditable = this.checkEditable(row, true);
          const inputCpm = event => {
            let unmaskedValue = event.target.value.replace(/CPM \$ /, "");
            unmaskedValue = unmaskedValue.replace(/,/g, "");
            const storeValue = Number(unmaskedValue);
            const storeKey = event.target.getAttribute("valueKey");
            updateProposalEditFormDetailGrid({
              id: detailId,
              quarterIndex: row.QuarterIdx,
              weekIndex: null,
              key: storeKey,
              value: storeValue,
              row: row._key
            });
            onUpdateProposal();
          };

          const inputUnits = event => {
            const storeValue = Number(event.target.value);
            const storeKey = event.target.getAttribute("valueKey");
            updateProposalEditFormDetailGrid({
              id: detailId,
              quarterIndex: row.QuarterIdx,
              weekIndex: row.WeekIdx,
              key: storeKey,
              value: storeValue,
              row: row._key
            });
            onUpdateProposal();
          };

          if (row.Type === "total")
            return row.TotalUnits ? numeral(row.TotalUnits).format("0,0") : "-";
          if (row.Type === "quarter") {
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
                isGridCellEdited={isGridCellEdited}
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
              // maskType="default"
              maskType="createNumber"
              maskAllowDecimal={false}
              isGridCellEdited={isGridCellEdited}
              //  toggleEditGridCellClass={this.props.toggleEditGridCellClass}
            />
          );
        }
      },
      {
        name: "Imp (000)",
        dataIndex: "EditImpressions",
        width: "20%",
        renderer: ({ value, row }) => {
          const isEditable = this.checkEditable(row, false);

          const inputImpressionGoal = event => {
            let unmaskedValue = event.target.value.replace(
              /Imp Goal \(000\) /,
              ""
            );
            unmaskedValue = unmaskedValue.replace(/,/g, "");
            const storeValue = Number(unmaskedValue) * 1000;
            const storeKey = event.target.getAttribute("valueKey");
            updateProposalEditFormDetailGrid({
              id: detailId,
              quarterIndex: row.QuarterIdx,
              weekIndex: null,
              key: storeKey,
              value: storeValue,
              row: row._key
            });
            onUpdateProposal();
          };

          const inputImpressions = event => {
            const unmaskedValue = event.target.value.replace(/,/g, "");
            const storeValue = Number(unmaskedValue) * 1000;
            // const storeValue = (Number(event.target.value) * 1000);
            const storeKey = event.target.getAttribute("valueKey");
            updateProposalEditFormDetailGrid({
              id: detailId,
              quarterIndex: row.QuarterIdx,
              weekIndex: row.WeekIdx,
              key: storeKey,
              value: storeValue,
              row: row._key
            });
            onUpdateProposal();
          };

          if (row.Type === "total")
            return row.TotalImpressions
              ? numeral(row.TotalImpressions / 1000).format("0,0.[000]")
              : "-";
          if (row.Type === "quarter") {
            const toConfirm = value !== 0; // only confirm if not 0
            return (
              <GridCellInput
                name="ImpressionGoal"
                placeholder="Imp Goal (000)"
                value={value}
                valueKey="ImpressionGoal"
                isEditable={isEditable}
                emptyZeroDefault
                confirmInput={toConfirm}
                confirmModalProperties={{
                  titleText: "Warning",
                  bodyText:
                    "You will lose one or more existing weekly goals for this quarter.", // string
                  bodyList: null, // array
                  closeButtonText: "Cancel",
                  closeButtonBsStyle: "default",
                  actionButtonText: "Continue",
                  actionButtonBsStyle: "warning"
                }}
                toggleModal={toggleModal}
                onSaveShowValidation={DetailGridsInvalid}
                blurAction={inputImpressionGoal}
                enterKeyPressAction={inputImpressionGoal}
                maskType="createNumber"
                maskPrefix="Imp Goal (000) "
                maskAllowDecimal
                maskDecimalLimit={3}
                isGridCellEdited={isGridCellEdited}
                // toggleEditGridCellClass={this.props.toggleEditGridCellClass}
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
              isGridCellEdited={isGridCellEdited}
              // toggleEditGridCellClass={this.props.toggleEditGridCellClass}
            />
          );
        }
      },
      {
        name: "Cost",
        dataIndex: "Cost",
        width: "20%",
        editable: false,
        renderer: ({ row }) => {
          if (row.Type === "total")
            return row.TotalCost
              ? numeral(row.TotalCost).format("$0,0[.]00")
              : "-";
          if (row.Type === "week")
            return row.Cost ? numeral(row.Cost).format("$0,0[.]00") : "-";
          return "";
        }
      },
      {
        name: "ISCIs",
        dataIndex: "Iscis",
        width: "20%",
        editable: false,
        renderer: ({ value, row }) => {
          if (row.Type === "week" && !row.IsHiatus) {
            const inputIscis = (IscisValue, next) => {
              updateProposalEditFormDetailGrid({
                id: detailId,
                quarterIndex: row.QuarterIdx,
                weekIndex: row.WeekIdx,
                key: "Iscis",
                value: IscisValue,
                row: row._key
              });
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
                ref={ref => {
                  this.isciCellItems[cellKey] = ref;
                }}
                isISCIEdited={isISCIEdited}
              />
            );
          }
          // empty for quarter, total
          return "";
        }
      },
      {
        name: "MyEvent Report Name",
        dataIndex: "MyEventsReportName",
        width: "20%",
        editable: true,
        renderer: ({ value, row }) => {
          const isEditable = true;
          const inputMyEvent = event => {
            const storeValue = event.target.value;
            updateProposalEditFormDetailGrid({
              id: detailId,
              quarterIndex: row.QuarterIdx,
              weekIndex: row.WeekIdx,
              key: "MyEventsReportName",
              value: storeValue,
              row: row._key
            });
          };

          if (row.Type === "total" || row.Type === "quarter") return null;
          return (
            <GridTextInput
              name="MyEvent"
              value={value}
              valueKey="MyEvent"
              isEditable={isEditable}
              maxLength={25}
              onKeyPress={keyPress}
              onSaveShowValidation={DetailGridsInvalid}
              enterKeyPressAction={inputMyEvent}
            />
          );
        }
      }
    ];

    this.gridColumns = columns;

    /* GRID PLGUINS */
    const plugins = {
      COLUMN_MANAGER: {
        resizable: true,
        moveable: false,
        sortable: {
          enabled: false,
          method: "local"
        }
      },
      EDITOR: {
        type: "inline",
        enabled: true
      },

      LOADER: {
        enabled: false
      },
      SELECTION_MODEL: {
        mode: "single",
        enabled: true,
        allowDeselect: true,
        editEvent: "none", // 'none' not go edit mode
        activeCls: "active",
        selectionEvent: "singleclick"
      },

      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells }) => {
          const type = cells[0].props.row.Type;
          let rowStyle = {};
          if (type === "quarter") rowStyle = { backgroundColor: "#dedede" };
          if (type === "total")
            rowStyle = {
              backgroundColor: "#eef5eb",
              fontWeight: "bold",
              borderTop: "2px solid #c5c5c5"
            };

          return (
            <tr style={rowStyle} {...rowProps}>
              {cells}
            </tr>
          );
        }
      }
    };

    const grid = {
      columns,
      plugins,
      stateKey
    };

    return (
      <Grid
        {...grid}
        data={GridQuarterWeeks}
        height="false"
        key={`${proposalValidationStates.DetailGridsInvalid}${isReadOnly}`} // force cell update
      />
    );
  }
}

ProposalDetailGrid.defaultProps = {};

ProposalDetailGrid.propTypes = {
  detailId: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
    .isRequired,
  GridQuarterWeeks: PropTypes.array.isRequired,
  isAdu: PropTypes.bool.isRequired,
  isReadOnly: PropTypes.bool.isRequired,
  updateProposalEditFormDetailGrid: PropTypes.func.isRequired,
  onUpdateProposal: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  proposalValidationStates: PropTypes.object.isRequired,
  isISCIEdited: PropTypes.bool.isRequired,
  isGridCellEdited: PropTypes.bool.isRequired
};
