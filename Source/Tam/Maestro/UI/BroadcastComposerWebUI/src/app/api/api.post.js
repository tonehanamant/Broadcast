import { call } from "redux-saga/effects";
import { GET, PUT, POST } from "./config";

const api = {
  getPosts: () => call(GET, `Post`, {}),
  getPostClientScrubbing: params => {
    const sendStatus = params.filterKey.length && params.filterKey !== "All";
    const statusParams = sendStatus
      ? { ScrubbingStatusFilter: params.filterKey }
      : {};
    return call(
      POST,
      `/Post/ClientScrubbingProposal/${params.proposalId}`,
      statusParams
    );
  },
  getUnlinkedIscis: () => call(GET, `Post/UnlinkedIscis `, {}),
  overrideStatus: params => call(PUT, `Post/OverrideStatus`, params),
  getArchivedIscis: () => call(GET, `Post/ArchivedIscis `, {}),
  archiveUnlinkedIscis: isciIds =>
    call(POST, `Post/ArchiveUnlinkedIsci`, isciIds),
  undoArchivedIscis: isciIds => call(POST, `Post/UndoArchiveIsci `, isciIds),
  getValidIscis: query => call(GET, `Post/FindValidIscis/${query}`),
  rescrubUnlinkedIscis: isci =>
    call(POST, `Post/ScrubUnlinkedIsci`, { Isci: isci }),
  mapUnlinkedIscis: ({ OriginalIsci, EffectiveIsci }) =>
    call(POST, `Post/MapIsci`, { OriginalIsci, EffectiveIsci }),
  swapProposalDetail: params => call(POST, `Post/SwapProposalDetail`, params),
  undoScrubStatus: params => call(PUT, `Post/UndoOverrideStatus`, params),
  uploadNtiTransmittal: params =>
    call(POST, `Post/UploadNtiTransmittals`, params)
};

export default api;
