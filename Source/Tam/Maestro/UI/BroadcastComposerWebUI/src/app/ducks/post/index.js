// Actions
import * as ACTIONS from './actionTypes.js';

const initialState = {
  post: [],
  proposalHeader: [],
  modals: {},
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case ACTIONS.RECEIVE_POST:
      return {
        ...state,
        post: data.Data,
        postUnfiltered: data.Data,
      };

    case ACTIONS.RECEIVE_FILTERED_POST:
      return {
        ...state,
        post: data,
      };

      case ACTIONS.RECEIVE_POST_SCRUBBING_HEADER:
      return {
        ...state,
        proposalHeader: data.Data,
      };

    default:
      return state;
  }
}

// Action Creators
export const getPost = () => ({
  type: ACTIONS.REQUEST_POST,
  payload: {},
});

export const getPostFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_POST,
  payload: query,
});

export const getProposalHeader = proposalID => ({
  type: ACTIONS.REQUEST_POST_SCRUBBING_HEADER,
  payload: proposalID,
});
