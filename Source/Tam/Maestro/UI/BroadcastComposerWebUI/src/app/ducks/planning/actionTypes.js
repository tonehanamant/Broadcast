import { createAction } from '../actionCreator';

export const REQUEST_PROPOSAL_INITIALDATA = 'REQUEST_PROPOSAL_INITIALDATA';
export const RECEIVE_PROPOSAL_INITIALDATA = 'RECEIVE_PROPOSAL_INITIALDATA';

export const REQUEST_PROPOSALS = 'REQUEST_PROPOSALS';
export const RECEIVE_PROPOSALS = 'RECEIVE_PROPOSALS';

export const REQUEST_PROPOSAL_LOCK = 'REQUEST_PROPOSAL_LOCK';
export const RECEIVE_PROPOSAL_LOCK = 'RECEIVE_PROPOSAL_LOCK';

export const REQUEST_PROPOSAL_UNLOCK = 'REQUEST_PROPOSAL_UNLOCK';
export const RECEIVE_PROPOSAL_UNLOCK = 'RECEIVE_PROPOSAL_UNLOCK';

export const REQUEST_PROPOSAL = 'REQUEST_PROPOSAL';
export const RECEIVE_PROPOSAL = 'RECEIVE_PROPOSAL';

export const REQUEST_PROPOSAL_VERSIONS = 'REQUEST_PROPOSAL_VERSIONS';
export const RECEIVE_PROPOSAL_VERSIONS = 'RECEIVE_PROPOSAL_VERSIONS';

export const REQUEST_PROPOSAL_VERSION = 'REQUEST_PROPOSAL_VERSION';
export const RECEIVE_PROPOSAL_VERSION = 'RECEIVE_PROPOSAL_VERSION';

export const SAVE_PROPOSAL = 'SAVE_PROPOSAL';
export const SAVE_PROPOSAL_AS_VERSION = 'SAVE_PROPOSAL_AS_VERSION';

export const DELETE_PROPOSAL = 'DELETE_PROPOSAL';

export const UPDATE_PROPOSAL = 'UPDATE_PROPOSAL';
export const RECEIVE_UPDATED_PROPOSAL = 'RECEIVE_UPDATED_PROPOSAL';

export const UPDATE_PROPOSAL_EDIT_FORM = 'UPDATE_PROPOSAL_EDIT_FORM';
export const UPDATE_PROPOSAL_EDIT_FORM_DETAIL = 'UPDATE_PROPOSAL_EDIT_FORM_DETAIL';
export const UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID = 'UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID';

export const MODEL_NEW_PROPOSAL_DETAIL = 'MODEL_NEW_PROPOSAL_DETAIL';
export const RECEIVE_NEW_PROPOSAL_DETAIL = 'RECEIVE_NEW_PROPOSAL_DETAIL';

export const DELETE_PROPOSAL_DETAIL = 'DELETE_PROPOSAL_DETAIL';
export const PROPOSAL_DETAIL_DELETED = 'PROPOSAL_DETAIL_DELETED';

export const FLATTEN_DETAIL = 'FLATTEN_DETAIL';
export const RECEIVE_FLATTEN_DETAIL = 'RECEIVE_FLATTEN_DETAIL';

export const UNORDER_PROPOSAL = 'UNORDER_PROPOSAL';

export const SET_PROPOSAL_VALIDATION_STATE = 'SET_PROPOSAL_VALIDATION_STATE';

export const RESTORE_PLANNING_PROPOSAL = 'RESTORE_PLANNING_PROPOSAL';

export const REQUEST_GENRES = 'REQUEST_GENRES';
export const RECEIVE_GENRES = 'RECEIVE_GENRES';
export const TOGGLE_GENRE_LOADING = 'TOGGLE_GENRE_LOADING';
export const REQUEST_PROGRAMS = 'REQUEST_PROGRAMS';
export const RECEIVE_PROGRAMS = 'RECEIVE_PROGRAMS';
export const TOGGLE_PROGRAM_LOADING = 'TOGGLE_PROGRAM_LOADING';
export const REQUEST_SHOWTYPES = 'REQUEST_SHOWTYPES';
export const RECEIVE_SHOWTYPES = 'RECEIVE_SHOWTYPES';
export const TOGGLE_SHOWTYPES_LOADING = 'TOGGLE_SHOWTYPES_LOADING';

export const CLEAR_OPEN_MARKET_DATA = 'CLEAR_OPEN_MARKET_DATA';
export const FILTERED_PLANNING_PROPOSALS = createAction('FILTERED_PLANNING_PROPOSALS');
export const RERUN_POST_SCRUBING = createAction('RERUN_POST_SCRUBING');
export const LOAD_OPEN_MARKET_DATA = createAction('LOAD_OPEN_MARKET_DATA');
