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
  getPostInitialData: () => (
    call(GET, `${apiBase}Post/InitialData`, {})
  ),
  getPosts: () => (
    call(GET, `${apiBase}Post`, {})
  ),
  getPost: id => (
    call(GET, `${apiBase}Post/${id}`, {})
  ),
  uploadPost: params => (
    call(POST, `${apiBase}Post`, params)
  ),
  savePost: params => (
    call(PUT, `${apiBase}Post`, params)
  ),
  deletePost: id => (
    call(DELETE, `${apiBase}Post/${id}`, {})
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
    call(GET, `${apiBase}Proposals/Proposal/${id}/Lock`, {})
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
    call(POST, `${apiBase}Proposals/UpdateProposal`, params)
  ),
};

// Calls
const api = {
  app,
  post,
  planning,
};

export default api;
