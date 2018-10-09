import { isArray } from "lodash";

const createReader = (resolve, reject, isAddFileInfo, { name, size, type }) => {
  const reader = new FileReader();
  reader.onload = parsedFile => {
    if (isAddFileInfo) {
      resolve({
        name,
        size,
        type,
        base64: parsedFile.currentTarget.result.split("base64,")[1]
      });
    } else {
      resolve(parsedFile);
    }
  };
  reader.onabort = reject;
  reader.onerror = reject;
  return reader;
};

const parseFile = (file, isAddFileInfo) =>
  new Promise((resolve, reject) => {
    const reader = createReader(resolve, reject, isAddFileInfo, file);
    reader.readAsDataURL(file);
  });

const parseFiles = (files, isAddFileInfo) => {
  const parseFunctions = files.map(file => parseFile(file, isAddFileInfo));
  return Promise.all(parseFunctions);
};

export const parseFileToBase64 = (files, isAddFileInfo) => {
  if (isArray(files)) {
    return parseFiles(files, isAddFileInfo);
  }
  return parseFile(files, isAddFileInfo);
};
