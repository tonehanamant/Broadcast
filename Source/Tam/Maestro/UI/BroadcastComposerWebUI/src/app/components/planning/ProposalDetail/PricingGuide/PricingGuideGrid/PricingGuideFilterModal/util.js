import React from 'react';
import Select from 'react-select';


export const defaultFiltersOptions = [
  { Display: 'Program name', Id: 'programName' },
  { Display: 'Airing Time', Id: 'airingTime', disable: true },
  { Display: 'Affilation', Id: 'affiliation', disabled: true },
  { Display: 'Market', Id: 'market', disabled: true },
];

export const filterMap = {
  programName: {
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
    getInitialData: (filterOptions) => {
      const programs = [];
      filterOptions.ProgramNames.forEach((item) => {
        const ret = { Display: item, Id: item };
        programs.push(ret);
      });
      return programs;
    },
  },
  airingTime: {
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

