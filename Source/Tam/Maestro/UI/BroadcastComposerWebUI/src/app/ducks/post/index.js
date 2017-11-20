// Actions
import * as ACTIONS from './actionTypes.js';

const initialState = {
  initialdata: {},
  post: [],
  fileEditForm: {
    Id: null,
    FileName: 'File',
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null,
  },
  fileUploadForm: {
    FileName: 'File',
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null,
  },
  modals: {},
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    // POST INITIAL DATA
    case ACTIONS.RECEIVE_POST_INITIALDATA:
      return {
        ...state,
        initialdata: data.Data,
      };

    // POST
    case ACTIONS.RECEIVE_POST:
      return {
        ...state,
        post: data.Data,
        postUnfiltered: data.Data, // store copy to be used by filter
      };

    case ACTIONS.ASSIGN_POST_DISPLAY:
      return {
        ...state,
        post: data,
        postUnfiltered: data, // store copy to be used by filter
      };

    // POST FILTERED
    case ACTIONS.STORE_POST_UNFILTERED:
      return {
        ...state,
        postUnfiltered: data,
      };

    case ACTIONS.RECEIVE_FILTERED_POST:
      return {
        ...state,
        post: data,
      };

    // case ACTIONS.RECEIVE_FILTERED_POST_ERROR:
    //   return {
    //     ...state,
    //     post: res,
    //   };

    case ACTIONS.FILE_EDIT_FORM_UPDATE_EQUIVALIZED:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          Equivalized: payload.value,
        },
      };

    case ACTIONS.FILE_EDIT_FORM_UPDATE_POSTING_BOOK:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          PostingBookId: payload.value,
        },
      };

    case ACTIONS.FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          PlaybackType: payload.value,
        },
      };

    case ACTIONS.FILE_EDIT_FORM_UPDATE_DEMOS:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          Demos: payload.value,
        },
      };

    case ACTIONS.RECEIVE_POST_FILE_EDIT:
      return {
        ...state,
        fileEditForm: data.Data,
      };

    case ACTIONS.FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          Equivalized: payload.value,
        },
      };

    case ACTIONS.FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          PostingBookId: payload.value,
        },
      };

    case ACTIONS.FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          PlaybackType: payload.value,
        },
      };

    case ACTIONS.FILE_UPLOAD_FORM_UPDATE_DEMOS:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          Demos: payload.value,
        },
      };

    case ACTIONS.CLEAR_FILE_UPLOAD_FORM:
      return {
        ...state,
        fileUploadForm: {
          FileName: 'File',
          Equivalized: true,
          PostingBookId: null,
          PlaybackType: null,
          Demos: null,
        },
      };

    default:
      return state;
  }
}

// Action Creators
export const getPostInitialData = () => ({
  type: ACTIONS.REQUEST_POST_INITIALDATA,
  payload: {},
});

export const getPost = () => ({
  type: ACTIONS.REQUEST_POST,
  payload: {},
});

export const getPostFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_POST,
  payload: query,
});

export const updateEquivalized = value => ({
  type: ACTIONS.FILE_EDIT_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value,
  },
});

export const updatePostingBook = value => ({
  type: ACTIONS.FILE_EDIT_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value,
  },
});

export const updatePlaybackType = value => ({
  type: ACTIONS.FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value,
  },
});

export const updateDemos = value => ({
  type: ACTIONS.FILE_EDIT_FORM_UPDATE_DEMOS,
  payload: {
    value,
  },
});

export const updateUploadEquivalized = value => ({
  type: ACTIONS.FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value,
  },
});

export const updateUploadPostingBook = value => ({
  type: ACTIONS.FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value,
  },
});

export const updateUploadPlaybackType = value => ({
  type: ACTIONS.FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value,
  },
});

export const updateUploadDemos = value => ({
  type: ACTIONS.FILE_UPLOAD_FORM_UPDATE_DEMOS,
  payload: {
    value,
  },
});

export const clearFileUploadForm = () => ({
  type: ACTIONS.CLEAR_FILE_UPLOAD_FORM,
});

export const deletePost = id => ({
  type: ACTIONS.DELETE_POST,
  payload: id,
});

export const getPostFileEdit = id => ({
  type: ACTIONS.REQUEST_POST_FILE_EDIT,
  payload: id,
});

export const savePostFileEdit = params => ({
  type: ACTIONS.REQUEST_POST_FILE_EDIT_SAVE,
  payload: params,
});

export const uploadPostFile = params => ({
  type: ACTIONS.REQUEST_POST_FILE_UPLOAD,
  payload: params,
});
