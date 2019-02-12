import {
  LOAD_ENVIRONMENT,
  LOAD_EMPLOYEE,
  TOGGLE_MODAL,
  CREATE_ALERT,
  DEPLOY_ERROR,
  CLEAR_ERRORS,
  SET_OVERLAY_LOADING,
  SET_OVERLAY_PROCESSING,
  STORE_FILE,
  CLEAR_FILE,
  TOGGLE_DISABLED_DROPZONES
} from "./types.js";

// Action Creators
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
