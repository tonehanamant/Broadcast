import React from "react";
import {
  pipe,
  keys,
  each,
  reduce,
  sortBy,
  split,
  filter,
  map
} from "lodash/fp";
import { every, forEach, head, last } from "lodash";
import { Checkbox, Radio } from "react-bootstrap";

const daysMap = {
  mon: {
    name: "Monday",
    shortName: "M",
    isWeekend: false,
    disaplyOrder: 1
  },
  tue: {
    name: "Tuesday",
    shortName: "TU",
    isWeekend: false,
    disaplyOrder: 2
  },
  wed: {
    name: "Wednesday",
    shortName: "W",
    isWeekend: false,
    disaplyOrder: 3
  },
  thu: {
    name: "Thursday",
    shortName: "TH",
    isWeekend: false,
    disaplyOrder: 4
  },
  fri: {
    name: "Friday",
    shortName: "F",
    isWeekend: false,
    disaplyOrder: 5
  },
  sat: {
    name: "Saturday",
    shortName: "SA",
    isWeekend: true,
    disaplyOrder: 6
  },
  sun: {
    name: "Sunday",
    shortName: "SU",
    isWeekend: true,
    disaplyOrder: 7
  }
};

const getQuickOptionsValues = expression => {
  const values = {};
  forEach(daysMap, (value, key) => {
    values[key] = expression(value);
  });
  return values;
};

const everydayValues = getQuickOptionsValues(() => true);
const weekendsValues = getQuickOptionsValues(it => it.isWeekend);
const weekdaysValues = getQuickOptionsValues(it => !it.isWeekend);

export const quickOptionsList = [
  {
    name: "Everyday",
    getValue: values => every(values, value => value === true),
    value: everydayValues
  },
  {
    name: "Weekdays",
    getValue: values =>
      every(values, (value, key) => daysMap[key].isWeekend !== value),
    value: weekdaysValues
  },
  {
    name: "Weekends",
    getValue: values =>
      every(values, (value, key) => daysMap[key].isWeekend === value),
    value: weekendsValues
  }
];

export const transformDaysByWeekends = (days, isWeekends) => {
  const daysArray = [];
  pipe(
    keys,
    sortBy(key => daysMap[key].order),
    each(day => {
      const dayInfo = daysMap[day];
      if (dayInfo.isWeekend === isWeekends) {
        daysArray.push({
          ...dayInfo,
          value: days[day],
          indexName: day
        });
      }
    })
  )(days);
  return daysArray;
};

export const daysSelectionsRender = (days, onChange) =>
  days.map(day => (
    <Checkbox
      key={`daypart-checkbox_#${day.indexName}`}
      className="day-selection-item"
      checked={day.value}
      onChange={e => {
        onChange(day.indexName, e.target.checked);
      }}
    >
      {day.name}
    </Checkbox>
  ));

export const quickOptionsRender = (days, onChange) =>
  quickOptionsList.map(it => (
    <Radio
      key={`daypart-quick-options_#${it.name}`}
      inline
      checked={it.getValue(days)}
      onChange={() => {
        onChange(it.value);
      }}
    >
      {it.name}
    </Radio>
  ));

const safeSplit = str =>
  pipe(
    split("-"),
    filter(str => str)
  )(str);

const dayToString = daysValue => {
  const daysText = pipe(
    keys,
    sortBy(key => daysMap[key].order),
    reduce(
      (text, day) =>
        `${text}${daysValue[day] ? `${daysMap[day].shortName}-` : ","}`,
      ""
    ),
    split(","),
    filter(str => str),
    map(safeSplit),
    reduce(
      (text, arr) =>
        `${text}${arr.length > 1 ? `${head(arr)}-${last(arr)}` : head(arr)},`,
      ""
    )
  )(daysValue);
  return daysText.slice(0, -1);
};

const timeToString = timeValue => timeValue.format("h:mmA").replace(":00", "");

export const dateToString = ({ days, startTime, endTime }) => {
  const daysText = dayToString(days);
  const startTimeText = timeToString(startTime);
  const endTimeText = timeToString(endTime);
  return `${daysText} ${startTimeText}-${endTimeText}`;
};
