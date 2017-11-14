Application Notes

## Table of Contents
  1. [Redux Data Grid](#redux-data-grid)

  ## Redux Data Grid
  https://github.com/bencripps/react-redux-grid

  Row Rendering

  //import __ from 'lodash';

  ROW: {
        enabled: true,
        /* eslint-disable react/prop-types */
        /* eslint-disable func-names */
        /* eslint-disable prefer-arrow-callback */
        renderer: ({ rowProps, cells, row }) => {
          // issue is that only specific defined row columns returned and then only in cells[0].props.row
          // to use this could potentially set a hidden column
          // other way is to use the map in row._root.nodes array and find an entry Key (not in all so check) per below
          //entry is an array[key, value]
          // Example: makes row red if the record is equivalized
            console.log('rowProps, cells, row', rowProps, cells, row._root.nodes);
            const equivNode = __.find(row._root.nodes, function (o) {
              if (o.entry) {
                return o.entry[0] === 'Equivalized';
              }
              return null;
            });
            let rowStyle = {};
            if (equivNode && equivNode.entry[1]) {
              console.log('row equivalized', equivNode, equivNode.entry[1]);
              rowStyle = {
                  color: 'red',
              };
            }
          return (
              <tr style={rowStyle} {...rowProps}>
                  { cells }
              </tr>
            );
        },
      },
    };

    ////////////
    Selection/State access
    https://github.com/bencripps/react-redux-grid/blob/master/docs/USING_BULK_SELECTION.md#the-subdivision-of-state
    I.E.
    const selectedIds = this.props.selection.get(this.props.stateKey).get('indexes');
    const rowData = this.props.dataSource.get(this.props.stateKey);
    const recData = rowData.toJS().data[selectedIds[0]];
