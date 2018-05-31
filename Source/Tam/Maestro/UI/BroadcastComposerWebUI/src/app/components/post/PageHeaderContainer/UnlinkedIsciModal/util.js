/* eslint-disable react/prop-types */
import React from 'react';
import { getDateInFormat, getSecondsToTimeString } from '../../../../utils/dateFormatter';

export const stateKey = 'unlinked-isci-modal';

export const setPosition = (e) => {
    const rowElement = e.target.closest('.react-grid-row');
    const contextMenuContainer = rowElement.querySelector('.react-grid-action-icon');
    contextMenuContainer.setAttribute('style', `right: ${(rowElement.clientWidth - e.clientX) + 45}px`);
};


export const generateGridConfig = ({ showMenu, hideMenu, selectRow, deselectAll, archiveIscis }, isAllowContextMenu) => {
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

		const plugins = {
			COLUMN_MANAGER: {
				resizable: true,
				moveable: false,
				sortable: {
						enabled: true,
						method: 'local',
				},
      },
      GRID_ACTIONS: {
        iconCls: 'action-icon',
        menu: [
          {
            text: 'Archive ISCI',
            key: 'menu-archive-isci',
            EVENT_HANDLER: ({ metaData }) => {
              archiveIscis([metaData.rowData.FileDetailId]);
            },
          },
        ],
      },
      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells, row }) => {
          const rowId = row.get('_key');
          const updatedRowProps = {
            ...rowProps,
            tabIndex: 1,
            onBlur: () => {
              if (rowId) {
                hideMenu({ stateKey });
              }
            },
            onContextMenu: (e) => {
              if (!isAllowContextMenu) return;
              e.preventDefault();
              setPosition(e);
              deselectAll({ stateKey });
              selectRow({ rowId, stateKey });
              showMenu({ id: rowId, stateKey });
            },
          };
          return (
            <tr {...updatedRowProps}>{ cells }</tr>
          );
        },
      },
    };

    return { columns, plugins, stateKey };
};
