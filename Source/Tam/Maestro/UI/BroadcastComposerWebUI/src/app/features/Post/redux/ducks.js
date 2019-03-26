import { createAction } from "Utils/action-creator";

export const ROOT = "post";

export const PROCESS_NTI_FILE = createAction(`${ROOT}/PROCESS_NTI_FILE`);
export const LOAD_POST = createAction(`${ROOT}/LOAD_POST`);

export const REQUEST_FILTERED_POST = `${ROOT}/REQUEST_FILTERED_POST`;
export const RECEIVE_FILTERED_POST = `${ROOT}/RECEIVE_FILTERED_POST`;

const initialState = {
  post: {},
  postGridData: [],
  unlinkedIscisLength: 0
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case LOAD_POST.success:
      return {
        ...state,
        post: data.Data,
        unlinkedIscisLength: data.Data.UnlinkedIscis,
        postGridData: data.Data.Posts,
        postUnfilteredGridData: data.Data.Posts
      };

    case RECEIVE_FILTERED_POST:
      return {
        ...state,
        postGridData: data
      };

    default:
      return state;
  }
}

const getPost = () => ({
  type: LOAD_POST.request,
  payload: {}
});

const getPostFiltered = query => ({
  type: REQUEST_FILTERED_POST,
  payload: query
});

const processNtiFile = params => ({
  type: PROCESS_NTI_FILE.request,
  payload: params
});

export const actions = {
  getPost,
  getPostFiltered,
  processNtiFile
};
