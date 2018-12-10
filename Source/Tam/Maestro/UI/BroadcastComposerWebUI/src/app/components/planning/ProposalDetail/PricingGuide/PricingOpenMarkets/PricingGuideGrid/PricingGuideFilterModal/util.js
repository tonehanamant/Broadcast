import React from "react";
import Select from "react-select";
import DayPartPicker from "Components/shared/DayPartPicker";
import { isNil, isArray, head, keys, omit } from "lodash";
import {
  pipe,
  mapValues,
  filter,
  groupBy,
  map,
  flatten,
  keyBy,
  concat,
  sortBy
} from "lodash/fp";
import { generate as generateId } from "shortid";

const mapValuesWithKeys = mapValues.convert({ cap: false });

export const generateFilter = filter => ({
  ...filter,
  name: `${filter.Id}_#_${generateId()}`
});

export const addFilterToItems = (filter, items) =>
  pipe(
    concat(filter),
    sortBy("order")
  )(items);

export const defaultFiltersItems = [
  { Display: "Program name", Id: "ProgramNames", order: 1 },
  { Display: "Airing Time", Id: "DayParts", order: 2 },
  { Display: "Affilate", Id: "Affiliations", order: 3 },
  { Display: "Genre", Id: "Genres", order: 4 }
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
    getInitialData: filterOptions =>
      filterOptions.map(item => ({ Display: item, Id: item })),
    postTransformer: values => values.map(({ Display }) => Display),
    validator: value => isArray(value) && !!value.length,
    preTransformer: values => values.map(item => ({ Display: item, Id: item }))
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
    getInitialData: filterOptions =>
      filterOptions.map(item => ({ Display: item, Id: item })),
    postTransformer: values => values.map(({ Display }) => Display),
    validator: value => isArray(value) && !!value.length,
    preTransformer: values => values.map(item => ({ Display: item, Id: item }))
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
    getInitialData: filterOptions => filterOptions,
    postTransformer: values => values.map(({ Id }) => Id),
    validator: value => isArray(value) && !!value.length,
    preTransformer: (values, options) =>
      options.filter(({ Id }) => values.includes(Id))
  },
  DayParts: {
    render: (value, onFilterChange) => (
      <DayPartPicker applyOnMount dayPart={value} onApply={onFilterChange} />
    ),
    getInitialData: filterOptions => filterOptions,
    postTransformer: values => values,
    validator: value => !isNil(value),
    isMultiple: true,
    preTransformer: values => values
  }
};

export const getFilterValuesToRequest = (values, options, renderedFilters) =>
  pipe(
    filter(f => filterMap[f.Id].validator(values[f.name])),
    map(f => ({
      id: f.Id,
      value: filterMap[f.Id].postTransformer(values[f.name], options[f.Id])
    })),
    groupBy(({ id }) => id),
    mapValuesWithKeys(
      (f, key) =>
        filterMap[key].isMultiple ? f.map(({ value }) => value) : head(f).value
    )
  )(renderedFilters);

const generateFilterFromValues = (filterValues, options) => filter => {
  const filterConfig = filterMap[filter.Id];
  if (filterConfig.isMultiple) {
    return filterValues[filter.Id].map(value => {
      const filterInfo = generateFilter(filter);
      return {
        value: filterConfig.preTransformer(value, options[filter.Id]),
        ...filterInfo
      };
    });
  }
  const filterInfo = generateFilter(filter);
  return {
    value: filterConfig.preTransformer(
      filterValues[filter.Id],
      options[filter.Id]
    ),
    ...filterInfo
  };
};

const convertFiltersToMap = (filters, filter) =>
  pipe(
    keyBy(f => f.name),
    mapValuesWithKeys(filter)
  )(filters);

const convertFiltersToArray = (filters, filter) =>
  pipe(
    keyBy(f => f.name),
    map(filter)
  )(filters);

export const getFilterValuesFromResponse = (filterValues, displayFilter) => {
  const selectedFilters = keys(filterValues);
  const filtersOptions = mapValuesWithKeys((value, key) =>
    filterMap[key].getInitialData(value)
  )(displayFilter);
  const selectedFiltersValue = pipe(
    filter(f => selectedFilters.includes(f.Id)),
    map(generateFilterFromValues(filterValues, filtersOptions)),
    flatten
  )(defaultFiltersItems);

  return {
    filtersItems: defaultFiltersItems.filter(
      it => filterMap[it.Id].isMultiple || !selectedFilters.includes(it.Id)
    ),
    filtersRender: convertFiltersToArray(selectedFiltersValue, f =>
      omit(f, "value")
    ),
    filtersValues: convertFiltersToMap(selectedFiltersValue, f => f.value),
    filtersOptions
  };
};
