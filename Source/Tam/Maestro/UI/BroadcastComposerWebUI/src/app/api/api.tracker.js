import { call } from "redux-saga/effects";
import { GET, PUT, POST } from "./config";

const api = {
  uploadTracker: params =>
    call(POST, `SpotTracker/UploadExtendedSigmaFile`, params),
  getValidIscis: query => call(GET, `PostLog/FindValidIscis/${query}`),
  mapUnlinkedIscis: ({ OriginalIsci, EffectiveIsci }) =>
    call(POST, `PostLog/MapIsci`, { OriginalIsci, EffectiveIsci }),
  getUnlinkedIscis: () => call(GET, `PostLog/UnlinkedIscis`, {}),
  getArchivedIscis: () => call(GET, `PostLog/ArchivedIscis `, {}),
  archiveUnlinkedIscis: isciIds =>
    call(POST, `PostLog/ArchiveUnlinkedIsci`, isciIds),
  undoArchivedIscis: isciIds => call(POST, `PostLog/UndoArchiveIsci`, isciIds),
  rescrubUnlinkedIscis: isci =>
    call(POST, `PostLog/ScrubUnlinkedIsci`, { Isci: isci }),
  // BCOP-3471 tracker data and scrubbing
  getTracker: () => call(GET, `PostLog`, {}),
  getTrackerClientScrubbing: params => {
    const sendStatus = params.filterKey.length && params.filterKey !== "All";
    const statusParams = sendStatus
      ? { ScrubbingStatusFilter: params.filterKey }
      : {};
    return call(
      POST,
      `/PostLog/ClientScrubbingProposal/${params.proposalId}`,
      statusParams
    );
  },
  overrideStatus: params => call(PUT, `PostLog/OverrideStatus`, params),
  undoScrubStatus: params => call(PUT, `PostLog/UndoOverrideStatus`, params),
  swapProposalDetail: params => call(POST, `PostLog/SwapProposalDetail`, params)
};
export default api;
