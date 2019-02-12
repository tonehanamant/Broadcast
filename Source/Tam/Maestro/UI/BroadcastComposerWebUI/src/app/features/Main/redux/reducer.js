import {
  RECEIVE_ENVIRONMENT,
  RECEIVE_EMPLOYEE,
  TOGGLE_MODAL,
  CREATE_ALERT,
  DEPLOY_ERROR,
  CLEAR_ERRORS,
  SET_OVERLAY_LOADING,
  SET_OVERLAY_PROCESSING,
  STORE_FILE,
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
