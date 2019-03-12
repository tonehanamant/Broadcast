import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Table, { withGrid } from "Lib/react-table";
import { columns } from "Planning/util/grid";

import PlanningGridHeader from "../PlanningGridHeader";

const showProposalDetail = id => {
  const url = `/broadcastreact/planning/proposal/${id}`;
  window.location.assign(url);
};

const defaultSorted = [
  {
    id: "LastModified",
    desc: true
  }
];

const getTrGroupProps = (state, rowInfo) => ({
  onDoubleClick: () => {
    showProposalDetail(rowInfo.original.Id);
  }
});

const mapStateToProps = ({ planning: { planningProposals } }) => ({
  planningProposals
});

function PlanningContainer({ visibleColumn, planningProposals }) {
  return (
    <>
      <PlanningGridHeader visibleColumn={visibleColumn} />
      <Table
        data={planningProposals}
        style={{ marginBottom: "100px" }}
        columns={columns}
        getTrGroupProps={getTrGroupProps}
        defaultSorted={defaultSorted}
        selection="single"
      />
    </>
  );
}

PlanningContainer.defaultProps = {};

PlanningContainer.propTypes = {
  planningProposals: PropTypes.array.isRequired,
  visibleColumn: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  null
)(withGrid(PlanningContainer));
