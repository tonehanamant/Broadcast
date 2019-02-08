import {
  POST_PRE_POSTING_INITIALDATA,
  POST_PRE_POSTING,
  FILTERED_POST_PRE_POSTING,
  DELETE_POST_PRE_POSTING,
  POST_PRE_POSTING_FILE_EDIT,
  POST_PRE_POSTING_FILE_SAVE,
  POST_PRE_POSTING_FILE_UPLOAD,
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

// Action Creators
export const receivePostPrePostingInitialData = data => ({
  type: POST_PRE_POSTING_INITIALDATA.success,
  data
});

export const receivePostPrePostingFileEdit = data => ({
  type: POST_PRE_POSTING_FILE_EDIT.success,
  data
});

export const receivePostPrePostingFileEditSave = data => ({
  type: POST_PRE_POSTING_FILE_SAVE.success,
  data
});

export const receiveFilteredPostPrePosting = data => ({
  type: FILTERED_POST_PRE_POSTING.success,
  data
});

export const receivePostPrePosting = data => ({
  type: POST_PRE_POSTING.success,
  data
});

export const getPostPrePostingInitialData = () => ({
  type: POST_PRE_POSTING_INITIALDATA.request,
  payload: {}
});

export const getPostPrePosting = () => ({
  type: POST_PRE_POSTING.request,
  payload: {}
});

export const getPostPrePostingFiltered = query => ({
  type: FILTERED_POST_PRE_POSTING.request,
  payload: query
});

export const updateEquivalized = value => ({
  type: FILE_EDIT_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value
  }
});

export const updatePostingBook = value => ({
  type: FILE_EDIT_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value
  }
});

export const updatePlaybackType = value => ({
  type: FILE_EDIT_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value
  }
});

export const updateDemos = value => ({
  type: FILE_EDIT_FORM_UPDATE_DEMOS,
  payload: {
    value
  }
});

export const updateUploadEquivalized = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_EQUIVALIZED,
  payload: {
    value
  }
});

export const updateUploadPostingBook = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_POSTING_BOOK,
  payload: {
    value
  }
});

export const updateUploadPlaybackType = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_PLAYBACK_TYPE,
  payload: {
    value
  }
});

export const updateUploadDemos = value => ({
  type: FILE_UPLOAD_FORM_UPDATE_DEMOS,
  payload: {
    value
  }
});

export const clearFileUploadForm = () => ({
  type: CLEAR_FILE_UPLOAD_FORM
});

export const deletePostPrePosting = id => ({
  type: DELETE_POST_PRE_POSTING.request,
  payload: id
});

export const getPostPrePostingFileEdit = id => ({
  type: POST_PRE_POSTING_FILE_EDIT.request,
  payload: id
});

export const savePostPrePostingFileEdit = params => ({
  type: POST_PRE_POSTING_FILE_SAVE.request,
  payload: params
});

export const uploadPostPrePostingFile = params => ({
  type: POST_PRE_POSTING_FILE_UPLOAD.request,
  payload: params
});
