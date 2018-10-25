import { createAction } from "../actionCreator";

export const REQUEST_PROPOSAL_INITIALDATA =
  "planning/REQUEST_PROPOSAL_INITIALDATA";
export const RECEIVE_PROPOSAL_INITIALDATA =
  "planning/RECEIVE_PROPOSAL_INITIALDATA";

export const REQUEST_PROPOSALS = "planning/REQUEST_PROPOSALS";
export const RECEIVE_PROPOSALS = "planning/RECEIVE_PROPOSALS";

export const REQUEST_PROPOSAL_LOCK = "planning/REQUEST_PROPOSAL_LOCK";
export const RECEIVE_PROPOSAL_LOCK = "planning/RECEIVE_PROPOSAL_LOCK";

export const REQUEST_PROPOSAL_UNLOCK = "planning/REQUEST_PROPOSAL_UNLOCK";
export const RECEIVE_PROPOSAL_UNLOCK = "planning/RECEIVE_PROPOSAL_UNLOCK";

export const REQUEST_PROPOSAL = "planning/REQUEST_PROPOSAL";
export const RECEIVE_PROPOSAL = "planning/RECEIVE_PROPOSAL";

export const REQUEST_PROPOSAL_VERSIONS = "planning/REQUEST_PROPOSAL_VERSIONS";
export const RECEIVE_PROPOSAL_VERSIONS = "planning/RECEIVE_PROPOSAL_VERSIONS";

export const REQUEST_PROPOSAL_VERSION = "planning/REQUEST_PROPOSAL_VERSION";
export const RECEIVE_PROPOSAL_VERSION = "planning/RECEIVE_PROPOSAL_VERSION";

export const SAVE_PROPOSAL = "planning/SAVE_PROPOSAL";
export const SAVE_PROPOSAL_AS_VERSION = "planning/SAVE_PROPOSAL_AS_VERSION";

export const DELETE_PROPOSAL = "planning/DELETE_PROPOSAL";

export const UPDATE_PROPOSAL = "planning/UPDATE_PROPOSAL";
export const RECEIVE_UPDATED_PROPOSAL = "planning/RECEIVE_UPDATED_PROPOSAL";

export const UPDATE_PROPOSAL_EDIT_FORM = "planning/UPDATE_PROPOSAL_EDIT_FORM";
export const UPDATE_PROPOSAL_EDIT_FORM_DETAIL =
  "planning/UPDATE_PROPOSAL_EDIT_FORM_DETAIL";
export const UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID =
  "planning/UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID";

export const MODEL_NEW_PROPOSAL_DETAIL = "planning/MODEL_NEW_PROPOSAL_DETAIL";
export const RECEIVE_NEW_PROPOSAL_DETAIL =
  "planning/RECEIVE_NEW_PROPOSAL_DETAIL";

export const DELETE_PROPOSAL_DETAIL = "planning/DELETE_PROPOSAL_DETAIL";
export const PROPOSAL_DETAIL_DELETED = "planning/PROPOSAL_DETAIL_DELETED";

export const FLATTEN_DETAIL = "planning/FLATTEN_DETAIL";
export const RECEIVE_FLATTEN_DETAIL = "planning/RECEIVE_FLATTEN_DETAIL";

export const UNORDER_PROPOSAL = "planning/UNORDER_PROPOSAL";

export const SET_PROPOSAL_VALIDATION_STATE =
  "planning/SET_PROPOSAL_VALIDATION_STATE";

export const RESTORE_PLANNING_PROPOSAL = "planning/RESTORE_PLANNING_PROPOSAL";

export const REQUEST_GENRES = "planning/REQUEST_GENRES";
export const RECEIVE_GENRES = "planning/RECEIVE_GENRES";
export const TOGGLE_GENRE_LOADING = "planning/TOGGLE_GENRE_LOADING";
export const REQUEST_PROGRAMS = "planning/REQUEST_PROGRAMS";
export const RECEIVE_PROGRAMS = "planning/RECEIVE_PROGRAMS";
export const TOGGLE_PROGRAM_LOADING = "planning/TOGGLE_PROGRAM_LOADING";
export const REQUEST_SHOWTYPES = "planning/REQUEST_SHOWTYPES";
export const RECEIVE_SHOWTYPES = "planning/RECEIVE_SHOWTYPES";
export const TOGGLE_SHOWTYPES_LOADING = "planning/TOGGLE_SHOWTYPES_LOADING";
export const SET_ESTIMATED_ID = "planning/SET_ESTIMATED_ID";

export const CLEAR_OPEN_MARKET_DATA = "planning/CLEAR_OPEN_MARKET_DATA";
export const FILTERED_PLANNING_PROPOSALS = createAction(
  "planning/FILTERED_PLANNING_PROPOSALS"
);
export const RERUN_POST_SCRUBING = createAction("planning/RERUN_POST_SCRUBING");
export const LOAD_OPEN_MARKET_DATA = createAction(
  "planning/LOAD_OPEN_MARKET_DATA"
);
export const SCX_FILE_UPLOAD = createAction("planning/SCX_FILE_UPLOAD");
export const ALLOCATE_SPOTS = createAction("planning/ALLOCATE_SPOTS");
export const FILTER_OPEN_MARKET_DATA = createAction(
  "planning/FILTER_OPEN_MARKET_DATA"
);
export const SORT_OPEN_MARKET_DATA = "planning/SORT_OPEN_MARKET_DATA";
export const SHOW_EDIT_MARKETS = "planning/SHOW_EDIT_MARKETS";
export const DISCARD_EDIT_MARKETS_DATA = "planning/DISCARD_EDIT_MARKETS_DATA";
export const CHANGE_EDIT_MARKETS_DATA = "planning/CHANGE_EDIT_MARKETS_DATA";
