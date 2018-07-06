/* eslint-disable react/prop-types */
import React from 'react';
import ContextMenuRow from 'Components/shared/ContextMenuRow';
import { getDateInFormat, getSecondsToTimeString } from '../../../../utils/dateFormatter';


export const stateKey = 'unlinked-isci-modal';

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

const tabInfo = {
  unlinked: {
    generateMenuitems: generateUnlinkedMenuitems,
  },
  archived: {
    generateMenuitems: () => {},
  },
};


export const generateGridConfig = (props, tabName) => {
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
      PAGER: {
        enabled: false,
      },
      ROW: {
        enabled: true,
        renderer: ({ cells, ...rowData }) => {
          const menuItems = tabInfo[tabName].generateMenuitems(props);
          return (
            <ContextMenuRow
              {...rowData}
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
