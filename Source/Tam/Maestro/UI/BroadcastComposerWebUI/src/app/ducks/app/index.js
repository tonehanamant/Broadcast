// Actions
import * as ACTIONS from './actionTypes.js';

const initialState = {
  environment: '',
  employee: {},
  modals: {},
  errors: [],
  alert: null,
  loading: {},
  file: {
    raw: {
      name: 'No File',
    },
    base64: '',
  },
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, error, modal, alert, overlay, file } = action;

  switch (type) {
    case ACTIONS.RECEIVE_ENVIRONMENT:
      return {
        ...state,
        environment: data.Data,
      };

    case ACTIONS.RECEIVE_EMPLOYEE:
      return {
        ...state,
        employee: data.Data,
      };

    case ACTIONS.CREATE_ALERT: {
        return {
          ...state,
          alert: {
            display: true,
            type: alert.type,
            headline: alert.headline,
            message: alert.message,
          },
        };
      }

    case ACTIONS.TOGGLE_MODAL:
      return Object.assign({}, state, {
        modals: {
          ...state.modals,
          [modal.modal]: {
            ...state[modal.modal],
            active: modal.active,
            properties: modal.properties,
          },
        },
      });

    case ACTIONS.DEPLOY_ERROR:
      return Object.assign({}, state, {
        modals: {
          ...state.modals,
          errorModal: {
            ...state.active,
            active: true,
          },
        },
        errors: [
          ...state.errors,
          error.message,
        ],
      });

    case ACTIONS.CLEAR_ERRORS:
      return Object.assign({}, state, {
        errors: [],
      });

    case ACTIONS.SET_OVERLAY_LOADING:
      return Object.assign({}, state, {
        loading: {
          ...state.loading,
          [overlay.id]: overlay.loading,
        },
      });

    case ACTIONS.SET_OVERLAY_PROCESSING:
      return Object.assign({}, state, {
        processing: {
          ...state.processing,
          [overlay.id]: overlay.processing,
        },
      });

    case ACTIONS.STORE_FILE:
      return Object.assign({}, state, {
        file: {
          ...state.file,
          raw: file,
        },
      });

    case ACTIONS.STORE_FILE_B64:
      return Object.assign({}, state, {
        file: {
          ...state.file,
          base64: data,
        },
      });

    case ACTIONS.CLEAR_FILE:
      return Object.assign({}, state, {
        file: {
          raw: {
            name: 'No File',
            base64: '',
          },
        },
      });

    default:
      return state;
  }
}

// Action Creators
export const getEnvironment = () => ({
  type: ACTIONS.REQUEST_ENVIRONMENT,
  payload: {},
});

export const getEmployee = () => ({
  type: ACTIONS.REQUEST_EMPLOYEE,
  payload: {},
});

export const toggleModal = modal => ({
  type: ACTIONS.TOGGLE_MODAL,
  modal,
});

export const createAlert = alert => ({
  type: ACTIONS.CREATE_ALERT,
  alert,
});

export const deployError = error => ({
  type: ACTIONS.DEPLOY_ERROR,
  error,
});

export const clearErrors = () => ({
  type: ACTIONS.CLEAR_ERRORS,
});

export const storeFile = file => ({
  type: ACTIONS.STORE_FILE,
  file,
});

export const readFileB64 = file => ({
  type: ACTIONS.READ_FILE_B64,
  payload: file,
});

export const clearFile = () => ({
  type: ACTIONS.CLEAR_FILE,
});
