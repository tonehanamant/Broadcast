import React from 'react';
import Select from 'react-select';


export const defaultFiltersItems = [
  { Display: 'Program name', Id: 'ProgramNames', order: 1 },
  { Display: 'Airing Time', Id: 'AiringTime', disabled: true, order: 2 },
  { Display: 'Affilate', Id: 'Affiliations', order: 3 },
  { Display: 'Market', Id: 'Markets', order: 4 },
  { Display: 'Genre', Id: 'Genres', order: 5 },
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
    getInitialData: filterOptions => filterOptions.map(item => ({ Display: item, Id: item })),
    postTransformer: values => values.map(({ Display }) => Display),
    preTransformer: values => values.map(item => ({ Display: item, Id: item })),
  },
  Affiliations: {
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
    getInitialData: filterOptions => filterOptions.map(item => ({ Display: item, Id: item })),
    postTransformer: values => values.map(({ Display }) => Display),
    preTransformer: values => values.map(item => ({ Display: item, Id: item })),
  },
  Genres: {
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
    getInitialData: filterOptions => filterOptions.map(item => ({ Display: item, Id: item })),
    postTransformer: values => values.map(({ Id }) => Id),
    preTransformer: (values, options) => options.filter(({ Id }) => values.includes(Id)),
  },
  Markets: {
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
    getInitialData: filterOptions => filterOptions,
    postTransformer: values => values.map(({ Id }) => Id),
    preTransformer: (values, options) => options.filter(({ Id }) => values.includes(Id)),
  },
};

