// Actions
import * as ACTIONS from './actionTypes.js';

const initialState = {
  initialdata: {},
  proposal: {},
  versions: {},
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    // PROPOSAL INITIAL DATA
    case ACTIONS.RECEIVE_PROPOSAL_INITIALDATA:
      return {
        ...state,
        initialdata: data.Data,
      };

    // PROPOSAL
    case ACTIONS.RECEIVE_PROPOSAL:
      return {
        ...state,
        proposal: data.Data,
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
      };

    default:
      return state;
  }
}

// Action Creators
export const getProposalInitialData = () => ({
  type: ACTIONS.REQUEST_PROPOSAL_INITIALDATA,
  payload: {},
});

export const getProposal = id => ({
  type: ACTIONS.REQUEST_PROPOSAL,
  payload: id,
});

export const getProposalVersions = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSIONS,
  payload: id,
});

export const getProposalVersion = (id, version) => (
  {
  type: ACTIONS.REQUEST_PROPOSAL_VERSION,
  payload: { id, version },
});
