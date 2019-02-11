// Actions
/* import {
  POST_PRE_POSTING_INITIALDATA,
  POST_PRE_POSTING,
  FILTERED_POST_PRE_POSTING,
  POST_PRE_POSTING_FILE_EDIT,
  FILE_EDIT_FORM_UPDATE_EQUIVALIZED,
  FILE_EDIT_FORM_UPDATE_POSTING_BOOK,
  FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE,
  FILE_EDIT_FORM_UPDATE_DEMOS,
  FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED,
  FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK,
  FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE,
  FILE_UPLOAD_FORM_UPDATE_DEMOS,
  CLEAR_FILE_UPLOAD_FORM
} from "./types.js";

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
} */

import {
  // REQUEST_ENVIRONMENT,
  RECEIVE_ENVIRONMENT,
  // REQUEST_EMPLOYEE,
  RECEIVE_EMPLOYEE,
  TOGGLE_MODAL,
  CREATE_ALERT,
  DEPLOY_ERROR,
  CLEAR_ERRORS,
  SET_OVERLAY_LOADING,
  SET_OVERLAY_PROCESSING,
  STORE_FILE,
  // READ_FILE_B64,
  STORE_FILE_B64,
  CLEAR_FILE,
  TOGGLE_DISABLED_DROPZONES
} from "./types.js";

const initialState = {
  environment: "",
  employee: {},
  modals: {},
  errors: [],
  alert: null,
  loading: {},
  file: {
    name: "No File",
    base64: ""
  },
  disabledDropzones: false
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, error, modal, alert, overlay, file } = action;

  switch (type) {
    case RECEIVE_ENVIRONMENT:
      return {
        ...state,
        environment: data.Data
      };

    case RECEIVE_EMPLOYEE:
      return {
        ...state,
        employee: data.Data
      };

    case CREATE_ALERT: {
      return {
        ...state,
        alert: {
          display: true,
          type: alert.type,
          headline: alert.headline,
          message: alert.message
        }
      };
    }

    case TOGGLE_MODAL:
      return Object.assign({}, state, {
        modals: {
          ...state.modals,
          [modal.modal]: {
            ...state[modal.modal],
            active: modal.active,
            properties: modal.properties
          }
        }
      });

    case DEPLOY_ERROR:
      return Object.assign({}, state, {
        modals: {
          ...state.modals,
          errorModal: {
            ...state.active,
            active: true
          }
        },
        errors: [...state.errors, error]
      });

    case CLEAR_ERRORS:
      return Object.assign({}, state, {
        errors: []
      });

    case SET_OVERLAY_LOADING:
      return Object.assign({}, state, {
        loading: {
          ...state.loading,
          [overlay.id]: overlay.loading
        }
      });

    case SET_OVERLAY_PROCESSING:
      return Object.assign({}, state, {
        processing: {
          ...state.processing,
          [overlay.id]: overlay.processing
        }
      });

    case STORE_FILE:
      return Object.assign({}, state, {
        file: {
          ...state.file,
          ...file
        }
      });

    case STORE_FILE_B64:
      return Object.assign({}, state, {
        file: {
          ...state.file,
          base64: data
        }
      });

    case CLEAR_FILE:
      return Object.assign({}, state, {
        file: {
          name: "No File",
          base64: ""
        }
      });

    case TOGGLE_DISABLED_DROPZONES:
      return { ...state, disabledDropzones: !state.disabledDropzones };

    default:
      return state;
  }
}
