import { createAction } from "Utils/action-creator";

const ROOT = "app";

export const LOAD_ENVIRONMENT = createAction(`${ROOT}/LOAD_ENVIRONMENT`);
export const LOAD_EMPLOYEE = createAction(`${ROOT}/LOAD_EMPLOYEE`);

export const TOGGLE_MODAL = `${ROOT}/TOGGLE_MODAL`;
export const DEPLOY_ERROR = `${ROOT}/DEPLOY_ERROR`;
export const CLEAR_ERRORS = `${ROOT}/CLEAR_ERRORS`;
export const CREATE_ALERT = `${ROOT}/CREATE_ALERT`;
export const SET_OVERLAY_LOADING = `${ROOT}/SET_OVERLAY_LOADING`;
export const SET_OVERLAY_PROCESSING = `${ROOT}/SET_OVERLAY_PROCESSING`;
export const STORE_FILE = `${ROOT}/STORE_FILE`;
export const CLEAR_FILE = `${ROOT}/CLEAR_FILE`;
export const TOGGLE_DISABLED_DROPZONES = `${ROOT}/TOGGLE_DISABLED_DROPZONES`;

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
    case LOAD_ENVIRONMENT.success:
      return {
        ...state,
        environment: data.Data
      };

    case LOAD_EMPLOYEE.success:
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

export const getEnvironment = () => ({
  type: LOAD_ENVIRONMENT.request,
  payload: {}
});

export const getEmployee = () => ({
  type: LOAD_EMPLOYEE.request,
  payload: {}
});

export const toggleModal = modal => ({
  type: TOGGLE_MODAL,
  modal
});

export const createAlert = alert => ({
  type: CREATE_ALERT,
  alert
});

export const deployError = error => ({
  type: DEPLOY_ERROR,
  error
});

export const clearErrors = () => ({
  type: CLEAR_ERRORS
});

export const setOverlayProcessing = overlay => ({
  type: SET_OVERLAY_PROCESSING,
  overlay
});

export const setOverlayLoading = overlay => ({
  type: SET_OVERLAY_LOADING,
  overlay
});

export const storeFile = file => ({
  type: STORE_FILE,
  file
});

export const clearFile = () => ({
  type: CLEAR_FILE
});

export const toggleDisabledDropzones = () => ({
  type: TOGGLE_DISABLED_DROPZONES
});
