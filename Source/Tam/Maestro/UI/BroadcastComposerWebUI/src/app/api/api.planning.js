import { call } from "redux-saga/effects";
import { GET, PUT, POST, DELETE } from "./config";

const api = {
  getProposalInitialData: () => call(GET, `Proposals/InitialData`, {}),
  loadPricingData: detailId => call(GET, `PricingGuide/${detailId}`, {}),
  copyToBuy: detailId => call(GET, `PricingGuide/CopyToBuy/${detailId}`),
  hasSpotsAllocated: detailId =>
    call(GET, `PricingGuide/HasSpotsAllocated/${detailId}`),
  savePricingData: params => call(POST, `PricingGuide/Save`, params),
  getProposals: () => call(GET, `Proposals/GetProposals`, {}),
  getProposalLock: id => call(GET, `Proposals/Proposal/${id}/Lock`, {}),
  getProposalUnlock: id => call(GET, `Proposals/Proposal/${id}/UnLock`, {}),
  getProposal: id => call(GET, `Proposals/Proposal/${id}`, {}),
  getProposalVersions: id => call(GET, `Proposals/Proposal/${id}/Versions`, {}),
  getProposalVersion: params =>
    call(GET, `Proposals/Proposal/${params.id}/Versions/${params.version}`, {}),
  saveProposal: params => call(POST, `Proposals/SaveProposal`, params),
  deleteProposal: id => call(DELETE, `Proposals/DeleteProposal/${id}`, {}),
  unorderProposal: id =>
    call(POST, `Proposals/UnorderProposal?proposalId=${id}`, {}),
  getProposalDetail: params =>
    call(POST, `Proposals/GetProposalDetail`, params),
  updateProposal: params =>
    call(POST, `Proposals/CalculateProposalChanges`, params),
  getGenres: query => call(GET, `Proposals/FindGenres/${query}`, {}),
  getPrograms: params => call(POST, `Proposals/FindPrograms`, params),
  getShowTypes: query => call(GET, `Proposals/FindShowType/${query}`, {}),
  rerunPostScrubing: (propId, propdetailid) =>
    call(PUT, `Proposals/RerunScrubbing/${propId}/${propdetailid}`),
  loadOpenMarketData: params => call(POST, `PricingGuide/Distribution`, params),
  updateOpenEditMarketsData: params =>
    call(POST, `PricingGuide/Distribution/UpdateMarkets`, params),
  updateProprietaryCpms: params =>
    call(POST, `PricingGuide/Distribution/UpdateProprietaryCpms`, params),
  uploadSCXFile: params =>
    call(POST, `Proposals/UploadProposalDetailBuy`, params),
  allocateSpots: data =>
    call(POST, `PricingGuide/Distribution/AllocateSpots`, data),
  filterOpenMarketData: params =>
    call(POST, `PricingGuide/Distribution/ApplyFilter`, params),
  checkAllocatedSpots: params =>
    call(POST, `Inventory/OpenMarket/CheckForAllocatedSpots`, params)
};

export default api;
