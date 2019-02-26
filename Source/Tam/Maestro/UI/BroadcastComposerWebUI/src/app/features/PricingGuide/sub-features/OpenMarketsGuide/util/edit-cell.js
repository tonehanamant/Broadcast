import { update, findIndex, get } from "lodash";

const findFieldIdx = (data, fieldName, rowData) =>
  findIndex(data, it => it[fieldName] === rowData[fieldName]);

const buildFieldPath = (data, rowData, fieldName) => {
  const marketIndex = findFieldIdx(data, "MarketId", rowData);
  let fieldPath = `[${marketIndex}]`;
  if (!rowData.isMarket) {
    fieldPath = `${fieldPath}.Stations`;
    const station = get(data, fieldPath);
    const stationIndex = findFieldIdx(station, "StationCode", rowData);
    fieldPath = `${fieldPath}[${stationIndex}]`;
    if (!rowData.isStation) {
      fieldPath = `${fieldPath}.Programs`;
      const program = get(data, fieldPath);
      const programIndex = findFieldIdx(program, "ProgramId", rowData);
      fieldPath = `${fieldPath}[${programIndex}]`;
    }
  }
  return {
    fieldPath: `${fieldPath}.${fieldName}`,
    editedManuallyPath: `${fieldPath}.SpotsEditedManually`
  };
};

export const updateItem = (data, fieldName, value, rowData) => {
  const paths = buildFieldPath(data, rowData, fieldName);
  const stepData = update(data, paths.editedManuallyPath, () => true);
  return update(stepData, paths.fieldPath, () => value);
};
