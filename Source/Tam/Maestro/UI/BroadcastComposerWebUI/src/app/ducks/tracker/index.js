// Actions
import {
  TRACKER_FILE_UPLOAD,
} from './actionTypes.js';

export const uploadTrackerFile = params => ({
  type: TRACKER_FILE_UPLOAD.request,
  payload: params,
});
