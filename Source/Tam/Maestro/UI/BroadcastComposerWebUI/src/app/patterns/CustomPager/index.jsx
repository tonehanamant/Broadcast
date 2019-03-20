import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";

const getSelectedId = (rowKey, data, idProperty) => {
  if (rowKey) {
    const row = data.find(({ _key: key }) => key === rowKey);
    const id = row[idProperty];
    return id ? `Record Id: ${id}` : "";
  }
  return "";
};

// requires props: stateKey (grid.stateKey); idProperty (property in record data that indicates ID)
const mapStateToProps = ({ selection, dataSource }, { stateKey }) => ({
  selection: selection.get(stateKey).toJS(),
  dataSource: dataSource.get(stateKey).toJS()
});

export function CustomPager({ dataSource, selection, idProperty }) {
  const { total } = dataSource;
  const keys = Object.keys(selection);
  const rowKey = keys.filter(key => selection[key] === true)[0];
  const recordId = getSelectedId(rowKey, dataSource.data, idProperty);

  return (
    <div styleName="react-grid-pager-toolbar">
      <span>{recordId}</span>
      {total ? (
        <span>
          1-{total} of {total}
        </span>
      ) : null}
    </div>
  );
}

CustomPager.defaultProps = {
  idProperty: "Id"
};

CustomPager.propTypes = {
  idProperty: PropTypes.string,
  dataSource: PropTypes.object.isRequired,
  selection: PropTypes.object.isRequired
};

export default connect(mapStateToProps)(CustomPager);
