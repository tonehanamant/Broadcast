import numeral from "numeral";
import { get, isNumber } from "lodash";

export const calculateBalanceSum = (inventorySrc, props) =>
  inventorySrc.reduce(
    (currentValue, i) =>
      (props[`propImpressions${i.Display}`] || 0) + currentValue,
    0
  );

export const numberRender = (data, path, format, divideBy) => {
  let number = get(data, path);
  if (number && divideBy) number /= divideBy;
  return isNumber(number) ? numeral(number).format(format) : "--";
};
