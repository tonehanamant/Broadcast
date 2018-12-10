import axios from "axios";
import { call } from "redux-saga/effects";

// Bases
const apiBase = __API__;

// Methods
const GET = axios.get;
const POST = axios.post;
const PUT = axios.put;
const DELETE = axios.delete;

// Requests
// call(METHOD, ...args)
const app = {
  getEnvironment: () => call(GET, `${apiBase}environment`, {}),
  getEmployee: () => call(GET, `${apiBase}employee`, {})
};

const post = {
  getPosts: () => call(GET, `${apiBase}Post`, {}),
  /*  getPostClientScrubbing: params => (
    call(GET, `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}`, {})
  ), */
  getPostClientScrubbing: params => {
    const sendStatus = params.filterKey.length && params.filterKey !== "All";
    const statusParams = sendStatus
      ? { ScrubbingStatusFilter: params.filterKey }
      : {};
    // return call(GET, `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}${sendStatus ? `?status=${params.filterKey}` : ''}`, {});
    return call(
      POST,
      `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}`,
      statusParams
    );
  },
  getUnlinkedIscis: () => call(GET, `${apiBase}Post/UnlinkedIscis `, {}),
  overrideStatus: params => call(PUT, `${apiBase}Post/OverrideStatus`, params),
  getArchivedIscis: () => call(GET, `${apiBase}Post/ArchivedIscis `, {}),
  archiveUnlinkedIscis: isciIds =>
    call(POST, `${apiBase}Post/ArchiveUnlinkedIsci`, isciIds),
  undoArchivedIscis: isciIds =>
    call(POST, `${apiBase}Post/UndoArchiveIsci `, isciIds),
  getValidIscis: query => call(GET, `${apiBase}Post/FindValidIscis/${query}`),
  rescrubUnlinkedIscis: isci =>
    call(POST, `${apiBase}Post/ScrubUnlinkedIsci`, { Isci: isci }),
  mapUnlinkedIscis: ({ OriginalIsci, EffectiveIsci }) =>
    call(POST, `${apiBase}Post/MapIsci`, { OriginalIsci, EffectiveIsci }),
  swapProposalDetail: params =>
    call(POST, `${apiBase}Post/SwapProposalDetail`, params),
  undoScrubStatus: params =>
    call(PUT, `${apiBase}Post/UndoOverrideStatus`, params),
  uploadNtiTransmittal: params =>
    call(POST, `${apiBase}Post/UploadNtiTransmittals`, params)
};

const postPrePosting = {
  getInitialData: () => call(GET, `${apiBase}PostPrePosting/InitialData`, {}),
  getPosts: () => call(GET, `${apiBase}PostPrePosting`, {}),
  getPost: id => call(GET, `${apiBase}PostPrePosting/${id}`, {}),
  uploadPost: params => call(POST, `${apiBase}PostPrePosting`, params),
  savePost: params => call(PUT, `${apiBase}PostPrePosting`, params),
  deletePost: id => call(DELETE, `${apiBase}PostPrePosting/${id}`, {})
};

const tracker = {
  uploadTracker: params =>
    call(POST, `${apiBase}SpotTracker/UploadExtendedSigmaFile`, params),
  getValidIscis: query =>
    call(GET, `${apiBase}PostLog/FindValidIscis/${query}`),
  mapUnlinkedIscis: ({ OriginalIsci, EffectiveIsci }) =>
    call(POST, `${apiBase}PostLog/MapIsci`, { OriginalIsci, EffectiveIsci }),
  getUnlinkedIscis: () => call(GET, `${apiBase}PostLog/UnlinkedIscis`, {}),
  getArchivedIscis: () => call(GET, `${apiBase}PostLog/ArchivedIscis `, {}),
  archiveUnlinkedIscis: isciIds =>
    call(POST, `${apiBase}PostLog/ArchiveUnlinkedIsci`, isciIds),
  undoArchivedIscis: isciIds =>
    call(POST, `${apiBase}PostLog/UndoArchiveIsci`, isciIds),
  rescrubUnlinkedIscis: isci =>
    call(POST, `${apiBase}PostLog/ScrubUnlinkedIsci`, { Isci: isci }),
  // BCOP-3471 tracker data and scrubbing
  getTracker: () => call(GET, `${apiBase}PostLog`, {}),
  getTrackerClientScrubbing: params => {
    const sendStatus = params.filterKey.length && params.filterKey !== "All";
    const statusParams = sendStatus
      ? { ScrubbingStatusFilter: params.filterKey }
      : {};
    return call(
      POST,
      `${apiBase}/PostLog/ClientScrubbingProposal/${params.proposalId}`,
      statusParams
    );
  },
  overrideStatus: params =>
    call(PUT, `${apiBase}PostLog/OverrideStatus`, params),
  undoScrubStatus: params =>
    call(PUT, `${apiBase}PostLog/UndoOverrideStatus`, params),
  swapProposalDetail: params =>
    call(POST, `${apiBase}PostLog/SwapProposalDetail`, params)
};

const planning = {
  getProposalInitialData: () =>
    call(GET, `${apiBase}Proposals/InitialData`, {}),
  loadPricingData: detailId =>
    call(GET, `${apiBase}PricingGuide/${detailId}`, {}),
  copyToBuy: detailId =>
    call(GET, `${apiBase}PricingGuide/CopyToBuy/${detailId}`),
  hasSpotsAllocated: detailId =>
    call(GET, `${apiBase}PricingGuide/HasSpotsAllocated/${detailId}`),
  savePricingData: params => call(POST, `${apiBase}PricingGuide/Save`, params),
  getProposals: () => call(GET, `${apiBase}Proposals/GetProposals`, {}),
  getProposalLock: id =>
    call(GET, `${apiBase}Proposals/Proposal/${id}/Lock`, {}),
  getProposalUnlock: id =>
    call(GET, `${apiBase}Proposals/Proposal/${id}/UnLock`, {}),
  getProposal: id => call(GET, `${apiBase}Proposals/Proposal/${id}`, {}),
  getProposalVersions: id =>
    call(GET, `${apiBase}Proposals/Proposal/${id}/Versions`, {}),
  getProposalVersion: params =>
    call(
      GET,
      `${apiBase}Proposals/Proposal/${params.id}/Versions/${params.version}`,
      {}
    ),
  saveProposal: params =>
    call(POST, `${apiBase}Proposals/SaveProposal`, params),
  deleteProposal: id =>
    call(DELETE, `${apiBase}Proposals/DeleteProposal/${id}`, {}),
  unorderProposal: id =>
    call(POST, `${apiBase}Proposals/UnorderProposal?proposalId=${id}`, {}),
  getProposalDetail: params =>
    call(POST, `${apiBase}Proposals/GetProposalDetail`, params),
  updateProposal: params =>
    call(POST, `${apiBase}Proposals/CalculateProposalChanges`, params),
  getGenres: query => call(GET, `${apiBase}Proposals/FindGenres/${query}`, {}),
  getPrograms: params => call(POST, `${apiBase}Proposals/FindPrograms`, params),
  getShowTypes: query =>
    call(GET, `${apiBase}Proposals/FindShowType/${query}`, {}),
  rerunPostScrubing: (propId, propdetailid) =>
    call(PUT, `${apiBase}Proposals/RerunScrubbing/${propId}/${propdetailid}`),
  loadOpenMarketData: params =>
    call(POST, `${apiBase}PricingGuide/Distribution`, params),
  updateOpenEditMarketsData: params =>
    call(POST, `${apiBase}PricingGuide/Distribution/UpdateMarkets`, params),
  updateProprietaryCpms: params =>
    call(
      POST,
      `${apiBase}PricingGuide/Distribution/UpdateProprietaryCpms`,
      params
    ),
  uploadSCXFile: params =>
    call(POST, `${apiBase}Proposals/UploadProposalDetailBuy`, params),
  allocateSpots: data =>
    call(POST, `${apiBase}PricingGuide/Distribution/AllocateSpots`, data),
  filterOpenMarketData: params =>
    call(POST, `${apiBase}PricingGuide/Distribution/ApplyFilter`, params)
};

// Calls
const api = {
  app,
  post,
  postPrePosting,
  planning,
  tracker
};

export default api;
