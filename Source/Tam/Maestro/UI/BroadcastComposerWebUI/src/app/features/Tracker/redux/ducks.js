import { createAction } from "Utils/action-creator";

export const ROOT = "tracker";

export const FILE_UPLOAD = createAction(`${ROOT}/FILE_UPLOAD`);
export const LOAD_TRACKER = createAction(`${ROOT}/LOAD_TRACKER`);

export const REQUEST_FILTERED_TRACKER = `${ROOT}/REQUEST_FILTERED_TRACKER`;
export const RECEIVE_FILTERED_TRACKER = `${ROOT}/RECEIVE_FILTERED_TRACKER`;

const initialState = {
  tracker: {},
  trackerGridData: [],
  unlinkedIscisLength: 0
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case LOAD_TRACKER.success:
      return {
        ...state,
        tracker: data.Data,
        unlinkedIscisLength: data.Data.UnlinkedIscis,
        trackerGridData: data.Data.Posts,
        trackerUnfilteredGridData: data.Data.Posts
      };

    case RECEIVE_FILTERED_TRACKER:
      return {
        ...state,
        trackerGridData: data
      };

    default:
      return state;
  }
}

export const getTracker = () => ({
  type: LOAD_TRACKER.request,
  payload: {}
});

export const getTrackerFiltered = query => ({
  type: REQUEST_FILTERED_TRACKER,
  payload: query
});

export const uploadTrackerFile = params => ({
  type: FILE_UPLOAD.request,
  payload: params
});

export const actions = {
  getTracker,
  getTrackerFiltered,
  uploadTrackerFile
};
