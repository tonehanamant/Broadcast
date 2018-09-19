import axios from 'axios';
import { call } from 'redux-saga/effects';

// Methods
const GET = axios.get;
const POST = axios.post;
const PUT = axios.put;
const DELETE = axios.delete;

// Bases
/* eslint-disable no-undef */
const apiBase = __API__;

// Requests
// call(METHOD, ...args)
const app = {
  getEnvironment: () => (
    call(GET, `${apiBase}environment`, {})
  ),
  getEmployee: () => (
    call(GET, `${apiBase}employee`, {})
  ),
};

const post = {
  getPosts: () => (
    call(GET, `${apiBase}Post`, {})
  ),
 /*  getPostClientScrubbing: params => (
    call(GET, `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}`, {})
  ), */
  getPostClientScrubbing: (params) => {
    const sendStatus = params.filterKey.length && (params.filterKey !== 'All');
    const statusParams = sendStatus ? { ScrubbingStatusFilter: params.filterKey } : {};
    // return call(GET, `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}${sendStatus ? `?status=${params.filterKey}` : ''}`, {});
    return call(POST, `${apiBase}/Post/ClientScrubbingProposal/${params.proposalId}`, statusParams);
  },
  getUnlinkedIscis: () => (
    call(GET, `${apiBase}Post/UnlinkedIscis `, {})
  ),
  overrideStatus: params => (
    call(PUT, `${apiBase}Post/OverrideStatus`, params)
  ),
  getArchivedIscis: () => (
    call(GET, `${apiBase}Post/ArchivedIscis `, {})
  ),
  archiveUnlinkedIscis: isciIds => (
    call(POST, `${apiBase}Post/ArchiveUnlinkedIsci`, isciIds)
  ),
  undoArchivedIscis: isciIds => (
    call(POST, `${apiBase}Post/UndoArchiveIsci `, isciIds)
  ),
  getValidIscis: query => (
    call(GET, `${apiBase}Post/FindValidIscis/${query}`)
  ),
  rescrubUnlinkedIscis: isci => (
    call(POST, `${apiBase}Post/ScrubUnlinkedIsci`, { Isci: isci })
  ),
  mapUnlinkedIscis: ({ OriginalIsci, EffectiveIsci }) => (
    call(POST, `${apiBase}Post/MapIsci`, { OriginalIsci, EffectiveIsci })
  ),
  swapProposalDetail: params => (
    call(POST, `${apiBase}Post/SwapProposalDetail`, params)
  ),
  undoScrubStatus: params => (
    call(PUT, `${apiBase}Post/UndoOverrideStatus`, params)
  ),
};

const postPrePosting = {
  getInitialData: () => (
    call(GET, `${apiBase}PostPrePosting/InitialData`, {})
  ),
  getPosts: () => (
    call(GET, `${apiBase}PostPrePosting`, {})
  ),
  getPost: id => (
    call(GET, `${apiBase}PostPrePosting/${id}`, {})
  ),
  uploadPost: params => (
    call(POST, `${apiBase}PostPrePosting`, params)
  ),
  savePost: params => (
    call(PUT, `${apiBase}PostPrePosting`, params)
  ),
  deletePost: id => (
    call(DELETE, `${apiBase}PostPrePosting/${id}`, {})
  ),
};

const tracker = {
  uploadTracker: params => (
    call(POST, `${apiBase}SpotTracker/UploadExtendedSigmaFile`, params)
  ),
};

const planning = {
  getProposalInitialData: () => (
    call(GET, `${apiBase}Proposals/InitialData`, {})
  ),
  getProposals: () => (
    call(GET, `${apiBase}Proposals/GetProposals`, {})
  ),
  getProposalLock: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/Lock`, {})
  ),
  getProposalUnlock: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/UnLock`, {})
  ),
  getProposal: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}`, {})
  ),
  getProposalVersions: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/Versions`, {})
  ),
  getProposalVersion: params => (
    call(GET, `${apiBase}Proposals/Proposal/${params.id}/Versions/${params.version}`, {})
  ),
  saveProposal: params => (
    call(POST, `${apiBase}Proposals/SaveProposal`, params)
  ),
  deleteProposal: id => (
    call(DELETE, `${apiBase}Proposals/DeleteProposal/${id}`, {})
  ),
  unorderProposal: id => (
    call(POST, `${apiBase}Proposals/UnorderProposal?proposalId=${id}`, {})
  ),
  getProposalDetail: params => (
    call(POST, `${apiBase}Proposals/GetProposalDetail`, params)
  ),
  updateProposal: params => (
    call(POST, `${apiBase}Proposals/CalculateProposalChanges`, params)
  ),
  getGenres: query => (
    call(GET, `${apiBase}Proposals/FindGenres/${query}`, {})
  ),
  getPrograms: params => (
    call(POST, `${apiBase}Proposals/FindPrograms`, params)
  ),
  getShowTypes: query => (
    call(GET, `${apiBase}Proposals/FindShowType/${query}`, {})
  ),
  rerunPostScrubing: (propId, propdetailid) => (
    call(PUT, `${apiBase}Proposals/RerunScrubbing/${propId}/${propdetailid}`)
  ),
  /* loadOpenMarketData: (propId, propdetailid) => (
    call(GET, `${apiBase}Inventory/Detail/PricingGuide/Grid/${propId}/${propdetailid}`)
  ), */
  loadOpenMarketData: params => (
    call(POST, `${apiBase}Inventory/Detail/PricingGuide/Grid`, params)
  ),
  uploadSCXFile: params => (
    call(POST, `${apiBase}Proposals/UploadProposalDetailBuy`, params)
  ),
};

// Calls
const api = {
  app,
  post,
  postPrePosting,
  planning,
  tracker,
};

export default api;
