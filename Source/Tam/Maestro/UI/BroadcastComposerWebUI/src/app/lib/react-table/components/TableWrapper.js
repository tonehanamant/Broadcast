import React from 'react';
import createTable from 'react-table-hoc-fixed-columns';
import Table from './Table';
import { GridContext } from '../hoc/withGrid';

const ReactTableFixedColumns = createTable(Table);


function TableWrapper(props) {
  return (
    <GridContext.Consumer>
      {hocProps => (<Table {...props} {...hocProps} />)}
    </GridContext.Consumer>
  );
}
function TableFixedColumnWrapper(props) {
  return (
    <GridContext.Consumer>
      {hocProps => (<ReactTableFixedColumns {...props} {...hocProps} />)}
    </GridContext.Consumer>
  );
}

export { TableFixedColumnWrapper, TableWrapper };
