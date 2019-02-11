import {
  REQUEST_ENVIRONMENT,
  // RECEIVE_ENVIRONMENT,
  REQUEST_EMPLOYEE,
  // RECEIVE_EMPLOYEE,
  TOGGLE_MODAL,
  CREATE_ALERT,
  DEPLOY_ERROR,
  CLEAR_ERRORS,
  SET_OVERLAY_LOADING,
  SET_OVERLAY_PROCESSING,
  STORE_FILE,
  READ_FILE_B64,
  // STORE_FILE_B64,
  CLEAR_FILE,
  TOGGLE_DISABLED_DROPZONES
} from "./types.js";

// Action Creators
export const getEnvironment = () => ({
  type: REQUEST_ENVIRONMENT,
  payload: {}
});

export const getEmployee = () => ({
  type: REQUEST_EMPLOYEE,
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

export const readFileB64 = file => ({
  type: READ_FILE_B64,
  payload: file
});

export const clearFile = () => ({
  type: CLEAR_FILE
});

export const toggleDisabledDropzones = () => ({
  type: TOGGLE_DISABLED_DROPZONES
});
