import accept from 'attr-accept';

export function getDataTransferItems({ dataTransfer, target }) {
  let dataTransferItemsList = [];
  if (dataTransfer) {
    const { files, items } = dataTransfer;
    if (files && files.length) {
      dataTransferItemsList = files;
    } else if (items && items.length) {
      // During the drag even the dataTransfer.files is null
      // but Chrome implements some drag store, which is accesible via dataTransfer.items
      dataTransferItemsList = items;
    }
  } else if (target && target.files) {
    dataTransferItemsList = target.files;
  }
  const fileArray = Array.prototype.slice.call(dataTransferItemsList);
  return fileArray.filter(it => (it.kind === 'file' || it instanceof File || it instanceof Blob));
}

export function validateFilesByExtension(accepted, rejected, extensions) {
  const acceptedFiles = accepted;
  const rejectedFiles = rejected;
  acceptedFiles.filter((file) => {
    const isAccept = accept({ name: file.name, type: '' }, extensions);
    if (isAccept) {
      return true;
    }
    rejectedFiles.push(file);
    return false;
  });
  return { acceptedFiles, rejectedFiles };
}
