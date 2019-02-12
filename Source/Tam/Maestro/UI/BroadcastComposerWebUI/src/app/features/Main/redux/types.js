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
export const READ_FILE_B64 = `${ROOT}/READ_FILE_B64`;
export const STORE_FILE_B64 = `${ROOT}/STORE_FILE_B64`;
export const CLEAR_FILE = `${ROOT}/CLEAR_FILE`;

export const TOGGLE_DISABLED_DROPZONES = `${ROOT}/TOGGLE_DISABLED_DROPZONES`;
