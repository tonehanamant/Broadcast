const getShortYear = year => `\`${year % 100}`;

const buildQuarter = (quarter, year, isShortYear = true) =>
  `Q${quarter} ${isShortYear ? getShortYear(year) : year}`;

export const displayQuarterRates = (from, to) => {
  const fromValue = from ? buildQuarter(from.Quarter, from.Year) : null;
  const toValue = to ? buildQuarter(to.Quarter, to.Year) : null;
  if (fromValue && toValue) {
    return `${fromValue} - ${toValue}`;
  }
  return fromValue || toValue;
};

export const displayQuarter = quarter =>
  buildQuarter(quarter.Quarter, quarter.Year, false);
