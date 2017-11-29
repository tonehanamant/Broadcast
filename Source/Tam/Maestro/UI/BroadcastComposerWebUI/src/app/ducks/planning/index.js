// Actions
import * as ACTIONS from './actionTypes.js';

const initialState = {
  initialdata: {},
  proposalLock: {},
  proposal: {},
  proposalEditForm: {},
  versions: [],
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    // PROPOSAL INITIAL DATA
    case ACTIONS.RECEIVE_PROPOSAL_INITIALDATA:
      return {
        ...state,
        initialdata: data.Data,
      };

    // PROPOSAL LOCK UNLOCK
    case ACTIONS.RECEIVE_PROPOSAL_LOCK:
    return {
      ...state,
      proposalLock: data.Data,
    };

    // PROPOSAL
    case ACTIONS.RECEIVE_PROPOSAL:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data,
      };

    // PROPOSAL VERSIONS
    case ACTIONS.RECEIVE_PROPOSAL_VERSIONS:
      return {
        ...state,
        versions: data.Data,
      };

    case ACTIONS.RECEIVE_PROPOSAL_VERSION:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data,
      };

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM:
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          [payload.key]: payload.value,
        },
      });

    default:
      return state;
  }
}

// Action Creators
export const getProposalInitialData = () => ({
  type: ACTIONS.REQUEST_PROPOSAL_INITIALDATA,
  payload: {},
});

export const getProposals = () => ({
  type: ACTIONS.REQUEST_PROPOSALS,
  payload: {},
});

export const getProposalLock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_LOCK,
  payload: id,
});

export const getProposalUnlock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_UNLOCK,
  payload: id,
});

export const getProposal = id => ({
  type: ACTIONS.REQUEST_PROPOSAL,
  payload: id,
});

export const getProposalVersions = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSIONS,
  payload: id,
});

export const getProposalVersion = (id, version) => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSION,
  payload: { id, version },
});

export const saveProposal = params => ({
  type: ACTIONS.SAVE_PROPOSAL,
  payload: params,
});

export const saveProposalAsVersion = params => ({
  type: ACTIONS.SAVE_PROPOSAL_AS_VERSION,
  payload: params,
});

export const deleteProposal = id => ({
  type: ACTIONS.DELETE_PROPOSAL,
  payload: id,
});

export const updateProposal = params => ({
  type: ACTIONS.UPDATE_PROPOSAL,
  payload: params,
});

export const updateProposalEditForm = keyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM,
  payload: keyValue,
});
