/* eslint-disable react/prop-types */
import React from 'react';
import ContextMenuRow from 'Components/shared/ContextMenuRow';
import { getDateInFormat, getSecondsToTimeString } from '../../../../utils/dateFormatter';


const generateUnlinkedMenuitems = ({ archiveIscis, rescrubIscis, toggleModal }) => ([
    {
      text: 'Not a Cadent ISCI',
      key: 'menu-archive-isci',
      EVENT_HANDLER: ({ metaData }) => {
        archiveIscis([metaData.rowData.FileDetailId]);
      },
    },
    {
      text: 'Rescrub this ISCI',
      key: 'menu-rescrub-isci',
      EVENT_HANDLER: ({ metaData }) => {
        rescrubIscis(metaData.rowData.ISCI);
      },
    },
    {
      text: 'Map ISCI',
      key: 'menu-map-isci',
      EVENT_HANDLER: ({ metaData }) => {
        toggleModal({
          modal: 'mapUnlinkedIsci',
          active: true,
          properties: { rowData: metaData.rowData },
        });
      },
    },
  ]
);

const generateArchivedMenuitems = ({ selection, dataSource, undoArchive }) => ([
  {
    text: 'Undo Archive',
    key: 'menu-undo-archive',
    EVENT_HANDLER: () => {
      // note: as is selections undefined as multi select not taking on this grid
      const stateKey = 'archived_grid';
      const selectedIds = selection.get(stateKey).get('indexes');
      const rowData = dataSource.get(stateKey).toJSON(); // currentRecords or data - array
      const activeSelections = [];
      // get just slected data FileDetailId for each for API call
      selectedIds.forEach((idx) => {
        activeSelections.push(rowData.data[idx].FileDetailId);
      });
      // console.log('undo archive selections', activeSelections, selectedIds, rowData, metaData);
      undoArchive(activeSelections);
    },
  },
]);

const archiveAdditionaProps = (props, stateKey) => {
  const selectedIds = props.selection.getIn([stateKey, 'indexes']);
  return {
    isRender: !!(selectedIds && selectedIds.size),
  };
};

const unlinkedAdditionaProps = (props, stateKey) => ({
  beforeOpenMenu: (rowId) => {
    props.deselectAll({ stateKey });
    props.selectRow({ rowId, stateKey });
  },
});

const tabInfo = {
  unlinked: {
    generateMenuitems: generateUnlinkedMenuitems,
    additionalRowProps: unlinkedAdditionaProps,
  },
  archived: {
    generateMenuitems: generateArchivedMenuitems,
    additionalRowProps: archiveAdditionaProps,
  },
};


export const generateGridConfig = (props, tabName) => {
  const stateKey = `${tabName}_grid`;
  const columns = [
			{
				name: 'ISCI',
				dataIndex: 'ISCI',
				width: '15%',
      },
      {
        name: 'Date Aired',
        dataIndex: 'DateAired',
        width: '10%',
        renderer: ({ row }) => (<span>{getDateInFormat(row.DateAired) || '-'}</span>),
      },
      {
        name: 'Time Aired',
        dataIndex: 'TimeAired',
        width: '10%',
        renderer: ({ row }) => (row.TimeAired ? getSecondsToTimeString(row.TimeAired) : '-'),
      },
      {
        name: 'Spot Length',
        dataIndex: 'SpotLength',
        width: '8%',
        renderer: ({ row }) => (<span>{row.SpotLength || '-'}</span>),
      },
      {
        name: 'Program',
        dataIndex: 'ProgramName',
        width: '20%',
      },
      {
        name: 'Genre',
        dataIndex: 'Genre',
        width: '10%',
      },
      {
        name: 'Affiliate',
        dataIndex: 'Affiliate',
        width: '9%',
        renderer: ({ row }) => (<span>{row.Affiliate || '-'}</span>),
      },
      {
        name: 'Market',
        dataIndex: 'Market',
        width: '9%',
        renderer: ({ row }) => (<span>{row.Market || '-'}</span>),
      },
      {
        name: 'Station',
        dataIndex: 'Station',
        width: '9%',
        renderer: ({ row }) => (<span>{row.Station || '-'}</span>),
      },
    ];

    const reasonCol = {
      name: 'Unlinked Reason',
      dataIndex: 'UnlinkedReason',
      width: '15%',
    };

    if (tabName === 'unlinked') {
      columns.splice(1, 0, reasonCol);
    }

		const plugins = {
			COLUMN_MANAGER: {
				resizable: true,
				moveable: false,
				sortable: {
						enabled: true,
						method: 'local',
				},
      },
      SELECTION_MODEL: {
        mode: (tabName === 'archived') ? 'multi' : 'single', // config takes but grids do not change
        enabled: true,
        allowDeselect: true,
        activeCls: 'active',
        selectionEvent: 'singleclick',
    },
      ROW: {
        enabled: true,
        renderer: ({ cells, ...rowData }) => {
          const { [tabName]: { generateMenuitems, additionalRowProps } } = tabInfo;
          const menuItems = generateMenuitems(props);
          const additionaProps = additionalRowProps(props, stateKey);
          return (
            <ContextMenuRow
              {...rowData}
              {...additionaProps}
              menuItems={menuItems}
              stateKey={stateKey}
            >
              {cells}
            </ContextMenuRow>
          );
        },
      },
    };

    return { columns, plugins, stateKey };
};
