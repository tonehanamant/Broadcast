import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Table, { withGrid } from "Lib/react-table";
import PageHeaderContainer from "Components/planning/PageHeaderContainer";

import { columns } from "./util";

const showProposalDetail = id => {
  const url = `/broadcastreact/planning/proposal/${id}`;
  window.location.assign(url);
};

const mapStateToProps = ({ planning: { planningProposals } }) => ({
  planningProposals
});

const mapDispatchToProps = dispatch => bindActionCreators({}, dispatch);

export function PlanningGrid({ visibleColumn, planningProposals }) {
  return (
    <Fragment>
      <PageHeaderContainer columns={columns} visibleColumn={visibleColumn} />
      <Table
        data={planningProposals}
        style={{ fontSize: "12px", marginBottom: "100px" }}
        columns={columns}
        getTrGroupProps={(state, rowInfo) => ({
          onDoubleClick: () => {
            showProposalDetail(rowInfo.original.Id);
          }
        })}
        selection="single"
      />
    </Fragment>
  );
}

PlanningGrid.defaultProps = {
  planningProposals: []
};

PlanningGrid.propTypes = {
  planningProposals: PropTypes.array.isRequired,
  visibleColumn: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(withGrid(PlanningGrid));
