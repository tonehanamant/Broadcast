import { createAction } from "../actionCreator";

export const POST_PRE_POSTING_INITIALDATA = createAction(
  "post-pre-posting/POST_PRE_POSTING_INITIALDATA"
);
export const POST_PRE_POSTING = createAction(
  "post-pre-posting/POST_PRE_POSTING"
);
export const FILTERED_POST_PRE_POSTING = createAction(
  "post-pre-posting/FILTERED_POST_PRE_POSTING"
);
export const DELETE_POST_PRE_POSTING = createAction(
  "post-pre-posting/DELETE_POST_PRE_POSTING"
);
export const POST_PRE_POSTING_FILE_EDIT = createAction(
  "post-pre-posting/POST_PRE_POSTING_FILE_EDIT"
);
export const POST_PRE_POSTING_FILE_SAVE = createAction(
  "post-pre-posting/POST_PRE_POSTING_FILE_SAVE"
);
export const POST_PRE_POSTING_FILE_UPLOAD = createAction(
  "post-pre-posting/POST_PRE_POSTING_FILE_UPLOAD"
);

export const TOGGLE_MODAL = "post-pre-posting/TOGGLE_MODAL";

export const FILE_EDIT_FORM_UPDATE_EQUIVALIZED =
  "post-pre-posting/FILE_EDIT_FORM_UPDATE_EQUIVALIZED";
export const FILE_EDIT_FORM_UPDATE_POSTING_BOOK =
  "post-pre-posting/FILE_EDIT_FORM_UPDATE_POSTING_BOOK";
export const FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE =
  "post-pre-posting/FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE";
export const FILE_EDIT_FORM_UPDATE_DEMOS =
  "post-pre-posting/FILE_EDIT_FORM_UPDATE_DEMOS";

export const FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED =
  "post-pre-posting/FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED";
export const FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK =
  "post-pre-posting/FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK";
export const FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE =
  "post-pre-posting/FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE";
export const FILE_UPLOAD_FORM_UPDATE_DEMOS =
  "post-pre-posting/FILE_UPLOAD_FORM_UPDATE_DEMOS";
export const CLEAR_FILE_UPLOAD_FORM = "post-pre-posting/CLEAR_FILE_UPLOAD_FORM";
