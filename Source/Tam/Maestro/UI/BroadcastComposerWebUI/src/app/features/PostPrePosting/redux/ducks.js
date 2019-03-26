import { createAction } from "Utils/action-creator";

const ROOT = "post-pre-posting";

export const POST_PRE_POSTING_INITIALDATA = createAction(
  `${ROOT}/POST_PRE_POSTING_INITIALDATA`
);
export const POST_PRE_POSTING = createAction(`${ROOT}/POST_PRE_POSTING`);
export const FILTERED_POST_PRE_POSTING = createAction(
  `${ROOT}/FILTERED_POST_PRE_POSTING`
);
export const DELETE_POST_PRE_POSTING = createAction(
  `${ROOT}/DELETE_POST_PRE_POSTING`
);
export const POST_PRE_POSTING_FILE_EDIT = createAction(
  `${ROOT}/POST_PRE_POSTING_FILE_EDIT`
);
export const POST_PRE_POSTING_FILE_SAVE = createAction(
  `${ROOT}/POST_PRE_POSTING_FILE_SAVE`
);
export const POST_PRE_POSTING_FILE_UPLOAD = createAction(
  `${ROOT}/POST_PRE_POSTING_FILE_UPLOAD`
);

export const TOGGLE_MODAL = `${ROOT}/TOGGLE_MODAL`;

export const FILE_EDIT_FORM_UPDATE_EQUIVALIZED = `${ROOT}/FILE_EDIT_FORM_UPDATE_EQUIVALIZED`;
export const FILE_EDIT_FORM_UPDATE_POSTING_BOOK = `${ROOT}/FILE_EDIT_FORM_UPDATE_POSTING_BOOK`;
export const FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE = `${ROOT}/FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE`;
export const FILE_EDIT_FORM_UPDATE_DEMOS = `${ROOT}/FILE_EDIT_FORM_UPDATE_DEMOS`;

export const FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED = `${ROOT}/FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED`;
export const FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK = `${ROOT}/FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK`;
export const FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE = `${ROOT}/FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE`;
export const FILE_UPLOAD_FORM_UPDATE_DEMOS = `${ROOT}/FILE_UPLOAD_FORM_UPDATE_DEMOS`;
export const CLEAR_FILE_UPLOAD_FORM = `${ROOT}/CLEAR_FILE_UPLOAD_FORM`;

const initialState = {
  initialdata: {},
  post: [],
  fileEditForm: {
    Id: null,
    FileName: "File",
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null
  },
  fileUploadForm: {
    FileName: "File",
    Equivalized: true,
    PostingBookId: null,
    PlaybackType: null,
    Demos: null
  }
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    case POST_PRE_POSTING_INITIALDATA.success:
      return {
        ...state,
        initialdata: data.Data
      };

    case POST_PRE_POSTING.success:
      return {
        ...state,
        post: data.Data,
        postUnfiltered: data.Data // store copy to be used by filter
      };

    case FILTERED_POST_PRE_POSTING.success:
      return {
        ...state,
        post: data
      };

    case FILE_EDIT_FORM_UPDATE_EQUIVALIZED:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          Equivalized: payload.value
        }
      };

    case FILE_EDIT_FORM_UPDATE_POSTING_BOOK:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          PostingBookId: payload.value
        }
      };

    case FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          PlaybackType: payload.value
        }
      };

    case FILE_EDIT_FORM_UPDATE_DEMOS:
      return {
        ...state,
        fileEditForm: {
          ...state.fileEditForm,
          Demos: payload.value
        }
      };

    case FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          Equivalized: payload.value
        }
      };

    case FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          PostingBookId: payload.value
        }
      };

    case FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          PlaybackType: payload.value
        }
      };

    case FILE_UPLOAD_FORM_UPDATE_DEMOS:
      return {
        ...state,
        fileUploadForm: {
          ...state.fileUploadForm,
          Demos: payload.value
        }
      };

    case CLEAR_FILE_UPLOAD_FORM:
      return {
        ...state,
        fileUploadForm: {
          FileName: "File",
          Equivalized: true,
          PostingBookId: null,
          PlaybackType: null,
          Demos: null
        }
      };

    case POST_PRE_POSTING_FILE_EDIT.success:
      return {
        ...state,
        fileEditForm: {
          Id: data.Data.Id,
          FileName: data.Data.FileName,
          Equivalized: data.Data.Equivalized,
          PostingBookId: data.Data.PostingBookId,
          PlaybackType: data.Data.PlaybackType,
          Demos: data.Data.Demos
        }
      };

    default:
      return state;
  }
}

// Action Creators
export const receivePostPrePostingInitialData = data => ({
  type: POST_PRE_POSTING_INITIALDATA.success,
  data
});

export const receivePostPrePostingFileEdit = data => ({
  type: POST_PRE_POSTING_FILE_EDIT.success,
  data
});

export const receivePostPrePostingFileEditSave = data => ({
  type: POST_PRE_POSTING_FILE_SAVE.success,
  data
});

export const receiveFilteredPostPrePosting = data => ({
  type: FILTERED_POST_PRE_POSTING.success,
  data
});

export const receivePostPrePosting = data => ({
  type: POST_PRE_POSTING.success,
  data
});

export const getPostPrePostingInitialData = () => ({
  type: POST_PRE_POSTING_INITIALDATA.request,
  payload: {}
});

export const getPostPrePosting = () => ({
  type: POST_PRE_POSTING.request,
  payload: {}
});

export const getPostPrePostingFiltered = query => ({
  type: FILTERED_POST_PRE_POSTING.request,
  payload: query
});

export const updateEquivalized = value => ({
  type: FILE_EDIT_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value
  }
});

export const updatePostingBook = value => ({
  type: FILE_EDIT_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value
  }
});

export const updatePlaybackType = value => ({
  type: FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value
  }
});

export const updateDemos = value => ({
  type: FILE_EDIT_FORM_UPDATE_DEMOS,
  payload: {
    value
  }
});

export const updateUploadEquivalized = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value
  }
});

export const updateUploadPostingBook = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value
  }
});

export const updateUploadPlaybackType = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value
  }
});

export const updateUploadDemos = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_DEMOS,
  payload: {
    value
  }
});

export const clearFileUploadForm = () => ({
  type: CLEAR_FILE_UPLOAD_FORM
});

export const deletePostPrePosting = id => ({
  type: DELETE_POST_PRE_POSTING.request,
  payload: id
});

export const getPostPrePostingFileEdit = id => ({
  type: POST_PRE_POSTING_FILE_EDIT.request,
  payload: id
});

export const savePostPrePostingFileEdit = params => ({
  type: POST_PRE_POSTING_FILE_SAVE.request,
  payload: params
});

export const uploadPostPrePostingFile = params => ({
  type: POST_PRE_POSTING_FILE_UPLOAD.request,
  payload: params
});