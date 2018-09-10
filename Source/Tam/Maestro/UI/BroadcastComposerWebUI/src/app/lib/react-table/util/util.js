import { isNaN } from 'lodash';
import { multiSelectRow, singleSelectRow } from '../actions/actions';

export const SELECTION = {
  SINGLE: 'single',
  MULTI: 'multi',
  NONE: 'none',
};

export const rowSelection = {
  [SELECTION.SINGLE]: singleSelectRow,
  [SELECTION.MULTI]: multiSelectRow,
};

export const isNumeric = (val) => {
  const parsedValue = parseFloat(val);
  return !isNaN(parsedValue);
};
