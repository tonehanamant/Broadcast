import { takeEvery, put, call, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import { selectModal } from 'Ducks/app/selectors';
import { getPost } from 'Ducks/post';

import * as appActions from 'Ducks/app/actionTypes';
import * as trackerActions from 'Ducks/tracker/actionTypes';
import {
  setOverlayProcessing,
  createAlert,
  setOverlayLoading,
  toggleModal,
} from 'Ducks/app/index';

import sagaWrapper from '../wrapper';
import api from '../api';

const ACTIONS = { ...appActions, ...trackerActions };

/* ////////////////////////////////// */
/* UPLOAD Tracker FILE */
/* ////////////////////////////////// */
export function* uploadTrackerFile(params) {
  const { uploadTracker } = api.tracker;
  try {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: true }));
    return yield uploadTracker(params);
  } finally {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: false }));
  }
}

export function* uploadTrackerFileSuccess() {
  yield put(createAlert({ type: 'success', headline: 'CSV Files Uploaded' }));
}

export function* watchUploadTrackerFile() {
  yield takeEvery(
    ACTIONS.TRACKER_FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, ACTIONS.TRACKER_FILE_UPLOAD),
  );
}

/* ////////////////////////////////// */
/* unlinked Iscis */
/* ////////////////////////////////// */

export function* requestArchivedFiltered({ payload: query }) {
  const archivedListUnfiltered = yield select(state => state.tracker.unlinkedFilteredIscis);

  // for each tracker, convert all properties to string to enable use on FuzzySearch object
  archivedListUnfiltered.map(tracker => (
    Object.keys(tracker).map(key => tracker[key])
  ));

  const keys = ['ISCI'];
  const searcher = new FuzzySearch(archivedListUnfiltered, keys, { caseSensitive: false });
  const archivedFiltered = () => searcher.search(query);

  try {
    const filtered = yield archivedFiltered();
    yield put({
      type: ACTIONS.TRACKER_RECEIVE_FILTERED_ARCHIVED,
      data: { query, filteredData: filtered },
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

export function* requestUnlinkedIscis() {
  const { getUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'TrackerUniqueIscis', loading: true }));
    return yield getUnlinkedIscis();
  } finally {
    yield put(setOverlayLoading({ id: 'TrackerUniqueIscis', loading: false }));
  }
}

export function* requestUnlinkedFiltered({ payload: query }) {
  const unlinkedListUnfiltered = yield select(state => state.tracker.unlinkedFilteredIscis);

  // for each tracker, convert all properties to string to enable use on FuzzySearch object
  unlinkedListUnfiltered.map(tracker => (
    Object.keys(tracker).map(key => tracker[key])
  ));

  const keys = ['ISCI'];
  const searcher = new FuzzySearch(unlinkedListUnfiltered, keys, { caseSensitive: false });
  const unlinkedFiltered = () => searcher.search(query);

  try {
    const filtered = yield unlinkedFiltered();
    yield put({
      type: ACTIONS.TRACKER_RECEIVE_FILTERED_UNLINKED,
      data: { query, filteredData: filtered },
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

export function* unlinkedIscisSuccess() {
  const activeQuery = yield select(state => state.tracker.activeIsciFilterQuery);
  // console.log('unlinked isci active query success>>>>>>>', activeQuery);
  const modal = select(selectModal, 'trackerUnlinkedIsciModal');
  if (modal && !modal.active) {
    yield put(toggleModal({
      modal: 'trackerUnlinkedIsciModal',
      active: true,
      properties: {
        titleText: 'tracker Unique Iscis',
        bodyText: 'Isci Details',
      },
    }));
  }
  if (activeQuery.length) {
    yield call(requestUnlinkedFiltered, { payload: activeQuery });
  }
}


export function* archivedIscisSuccess() {
  const activeQuery = yield select(state => state.tracker.activeIsciFilterQuery);
  // console.log('archived isci active query success>>>>>>>', activeQuery);
  if (activeQuery.length) {
    yield call(requestArchivedFiltered, { payload: activeQuery });
  }
}

export function* archiveUnlinkedIsci({ ids }) {
  const { archiveUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'trackerArchiveIsci', loading: true }));
    return yield archiveUnlinkedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: 'trackerArchiveIsci', loading: false }));
  }
}

export function* rescrubUnlinkedIsci({ isci }) {
  const { rescrubUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'rescrubIsci', loading: true }));
    return yield rescrubUnlinkedIscis(isci);
  } finally {
    yield put(setOverlayLoading({ id: 'rescrubIsci', loading: false }));
  }
}


export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: false }));
  }
}

export function* loadValidIscis({ query }) {
  const { getValidIscis } = api.tracker;
  return yield getValidIscis(query);
}

export function* mapUnlinkedIsci(payload) {
  const { mapUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'mapUnlinkedIsci', loading: true }));
    return yield mapUnlinkedIscis(payload);
  } finally {
    yield put(setOverlayLoading({ id: 'mapUnlinkedIsci', loading: false }));
  }
}

export function* mapUnlinkedIsciSuccess() {
  yield put(toggleModal({
    modal: 'mapUnlinkedIsci',
    active: false,
    properties: {},
  }));
}

export function* closeUnlinkedIsciModal({ modalPrams }) {
  yield put({
    type: ACTIONS.TRACKER_RECEIVE_CLEAR_ISCI_FILTER,
  });
  yield put(toggleModal({
    modal: 'trackerUnlinkedIsciModal',
    active: false,
    properties: modalPrams,
  }));
  yield put(getPost());
}

export function* undoArchivedIscis({ ids }) {
  const { undoArchivedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: 'trackerArchiveIsci', loading: true }));
    return yield undoArchivedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: 'trackerArchiveIsci', loading: false }));
  }
}

/* ////////////////////////////////// */
/* watchers */
/* ////////////////////////////////// */

export function* watchUploadTrackerFileSuccess() {
  yield takeEvery(ACTIONS.TRACKER_FILE_UPLOAD.success, uploadTrackerFileSuccess);
}

export function* watchRequestUniqueIscis() {
  yield takeEvery([
    ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.request,
    ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI.success,
    ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI.success,
    ACTIONS.TRACKER_MAP_UNLINKED_ISCI.success,
  ],
    sagaWrapper(requestUnlinkedIscis, ACTIONS.TRACKER_UNLINKED_ISCIS_DATA),
  );
}

export function* watchArchiveUnlinkedIsci() {
  yield takeEvery(ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI.request, sagaWrapper(archiveUnlinkedIsci, ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI));
}

export function* watchRescrubUnlinkedIsci() {
  yield takeEvery(ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI.request, sagaWrapper(rescrubUnlinkedIsci, ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI));
}

export function* watchRequestUnlinkedFiltered() {
  yield takeEvery(ACTIONS.TRACKER_REQUEST_FILTERED_UNLINKED, requestUnlinkedFiltered);
}

export function* watchRequestArchivedFiltered() {
  yield takeEvery(ACTIONS.TRACKER_REQUEST_FILTERED_ARCHIVED, requestArchivedFiltered);
}

export function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

export function* watchRequestArchivedIscisSuccess() {
  yield takeEvery(ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.success, archivedIscisSuccess);
}

export function* watchLoadArchivedIscis() {
  yield takeEvery([
    ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.request,
    ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI.success,
  ],
    sagaWrapper(loadArchivedIsci, ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI),
  );
}

export function* watchLoadValidIscis() {
  yield takeEvery(ACTIONS.TRACKER_LOAD_VALID_ISCI.request, sagaWrapper(loadValidIscis, ACTIONS.TRACKER_LOAD_VALID_ISCI));
}

export function* watchMapUnlinkedIsci() {
  yield takeEvery(ACTIONS.TRACKER_MAP_UNLINKED_ISCI.request, sagaWrapper(mapUnlinkedIsci, ACTIONS.TRACKER_MAP_UNLINKED_ISCI));
}

export function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(ACTIONS.TRACKER_CLOSE_UNLINKED_ISCI_MODAL, closeUnlinkedIsciModal);
}

export function* watchMapUnlinkedIsciSuccess() {
  yield takeEvery(ACTIONS.TRACKER_MAP_UNLINKED_ISCI.success, mapUnlinkedIsciSuccess);
}

export function* watchUndoArchivedIscis() {
  yield takeEvery(ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI.request, sagaWrapper(undoArchivedIscis, ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI));
}
