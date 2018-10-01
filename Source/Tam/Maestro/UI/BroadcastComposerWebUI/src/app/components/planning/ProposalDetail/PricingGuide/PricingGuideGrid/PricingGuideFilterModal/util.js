import React from 'react';
import Select from 'react-select';


export const defaultFiltersOptions = [
  { Display: 'Program name', Id: 'ProgramNames' },
  { Display: 'Airing Time', Id: 'AiringTime', disabled: true },
  { Display: 'Affilation', Id: 'Affiliation', disabled: true },
  { Display: 'Market', Id: 'Market', disabled: true },
];

export const filterMap = {
  ProgramNames: {
    render: (value, onFilterChange, options) => (
      <Select
        value={value}
        multi
        onChange={onFilterChange}
        options={options}
        labelKey="Display"
        valueKey="Id"
        clearable={false}
      />
    ),
    getInitialData: filterOptions => filterOptions.ProgramNames.map(item => ({ Display: item, Id: item })),
  },
  AiringTime: {
    render: (value, onFilterChange, options) => (
      <Select
        value={value}
        onChange={onFilterChange}
        options={options}
        labelKey="Display"
        valueKey="Id"
        clearable={false}
      />
    ),
    getInitialData: () => ([
      { Display: 'asdasdasda name', Id: 'programName' },
      { Display: 'asdasdasd Time', Id: 'airingTime' },
      { Display: 'asdasdads', Id: 'affiliation' },
      { Display: 'Marasdasdasdasdasdasdasdaket', Id: 'market' },
    ]),
  },
};

