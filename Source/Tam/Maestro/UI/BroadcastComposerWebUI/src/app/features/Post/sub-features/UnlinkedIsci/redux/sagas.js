import { takeEvery, put, call, select } from "redux-saga/effects";
import { setOverlayLoading, toggleModal, deployError } from "Main/redux/ducks";
import { selectModal } from "Main/redux/sagas";
import sagaWrapper from "Utils/saga-wrapper";
import searcher from "Utils/searcher";
import { actions as postActions } from "Post/redux";
import api from "API";
import {
  ARCHIVE_UNLIKED_ISCI,
  UNLINKED_ISCIS_DATA,
  LOAD_ARCHIVED_ISCI,
  LOAD_VALID_ISCI,
  MAP_UNLINKED_ISCI,
  UNDO_ARCHIVED_ISCI,
  RESCRUB_UNLIKED_ISCI,
  CLOSE_UNLINKED_ISCI_MODAL,
  REQUEST_FILTERED_UNLINKED,
  RECEIVE_FILTERED_UNLINKED,
  REQUEST_FILTERED_ARCHIVED,
  RECEIVE_FILTERED_ARCHIVED,
  actions
} from "./ducks";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectFilteredIscis = state =>
  state.post.unlinkedIsci.unlinkedFilteredIscis;
export const selectActiveIsciFilter = state =>
  state.post.unlinkedIsci.activeIsciFilterQuery;

/* ////////////////////////////////// */
/* SAGAS */
/* ////////////////////////////////// */
const iscisSearchKeys = ["ISCI"];

export function* requestUnlinkedFiltered({ payload: query }) {
  const data = yield select(selectFilteredIscis);
  // for each post, convert all properties to string to enable use on FuzzySearch object
  data.map(post => Object.keys(post).map(key => post[key]));

  try {
    const filtered = yield searcher(data, iscisSearchKeys, query);
    yield put({
      type: RECEIVE_FILTERED_UNLINKED,
      data: { query, filteredData: filtered }
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

export function* requestArchivedFiltered({ payload: query }) {
  const data = yield select(selectFilteredIscis);
  // for each post, convert all properties to string to enable use on FuzzySearch object
  data.map(post => Object.keys(post).map(key => post[key]));

  if (data.length > 0) {
    try {
      const filtered = yield searcher(data, iscisSearchKeys, query);
      yield put({
        type: RECEIVE_FILTERED_ARCHIVED,
        data: { query, filteredData: filtered }
      });
    } catch (e) {
      if (e.message) {
        yield put(deployError({ message: e.message }));
      }
    }
  }
}

export function* requestUnlinkedIscis() {
  const { getUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "PostUniqueIscis", loading: true }));
    return yield getUnlinkedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "PostUniqueIscis", loading: false }));
  }
}

export function* unlinkedIscisSuccess() {
  const activeQuery = yield select(selectActiveIsciFilter);
  const modal = select(selectModal, "postUnlinkedIsciModal");
  if (modal && !modal.active) {
    yield put(
      toggleModal({
        modal: "postUnlinkedIsciModal",
        active: true,
        properties: {
          titleText: "POST Unique Iscis",
          bodyText: "Isci Details"
        }
      })
    );
  }
  if (activeQuery.length) {
    yield call(requestUnlinkedFiltered, { payload: activeQuery });
  }
}

export function* archivedIscisSuccess() {
  const activeQuery = yield select(selectActiveIsciFilter);
  if (activeQuery.length) {
    yield call(requestArchivedFiltered, { payload: activeQuery });
  }
}

export function* archiveUnlinkedIsci({ ids }) {
  const { archiveUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: true }));
    return yield archiveUnlinkedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: false }));
  }
}

export function* undoArchivedIscis({ ids }) {
  const { undoArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: true }));
    return yield undoArchivedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: false }));
  }
}

export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: false }));
  }
}

export function* loadValidIscis({ query }) {
  const { getValidIscis } = api.post;
  return yield getValidIscis(query);
}

export function* rescrubUnlinkedIsci({ isci }) {
  const { rescrubUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: true }));
    return yield rescrubUnlinkedIscis(isci);
  } finally {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: false }));
  }
}

export function* mapUnlinkedIsci(payload) {
  const { mapUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "mapUnlinkedIsci", loading: true }));
    return yield mapUnlinkedIscis(payload);
  } finally {
    yield put(setOverlayLoading({ id: "mapUnlinkedIsci", loading: false }));
  }
}

export function* mapUnlinkedIsciSuccess() {
  yield put(
    toggleModal({
      modal: "mapUnlinkedIsci",
      active: false,
      properties: {}
    })
  );
}

export function* closeUnlinkedIsciModal({ modalPrams }) {
  yield put(actions.reveiveClearIsciFilter());
  yield put(
    toggleModal({
      modal: "postUnlinkedIsciModal",
      active: false,
      properties: modalPrams
    })
  );
  yield put(postActions.getPost());
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
function* watchRequestUnlinkedFiltered() {
  yield takeEvery(REQUEST_FILTERED_UNLINKED, requestUnlinkedFiltered);
}

function* watchRequestArchivedFiltered() {
  yield takeEvery(REQUEST_FILTERED_ARCHIVED, requestArchivedFiltered);
}

function* watchRequestUniqueIscis() {
  yield takeEvery(
    [
      UNLINKED_ISCIS_DATA.request,
      ARCHIVE_UNLIKED_ISCI.success,
      RESCRUB_UNLIKED_ISCI.success,
      MAP_UNLINKED_ISCI.success
    ],
    sagaWrapper(requestUnlinkedIscis, UNLINKED_ISCIS_DATA)
  );
}

function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

function* watchRequestArchivedIscisSuccess() {
  yield takeEvery(LOAD_ARCHIVED_ISCI.success, archivedIscisSuccess);
}

function* watchArchiveUnlinkedIsci() {
  yield takeEvery(
    ARCHIVE_UNLIKED_ISCI.request,
    sagaWrapper(archiveUnlinkedIsci, ARCHIVE_UNLIKED_ISCI)
  );
}

function* watchLoadArchivedIscis() {
  yield takeEvery(
    [LOAD_ARCHIVED_ISCI.request, UNDO_ARCHIVED_ISCI.success],
    sagaWrapper(loadArchivedIsci, LOAD_ARCHIVED_ISCI)
  );
}

function* watchLoadValidIscis() {
  yield takeEvery(
    LOAD_VALID_ISCI.request,
    sagaWrapper(loadValidIscis, LOAD_VALID_ISCI)
  );
}

function* watchRescrubUnlinkedIsci() {
  yield takeEvery(
    RESCRUB_UNLIKED_ISCI.request,
    sagaWrapper(rescrubUnlinkedIsci, RESCRUB_UNLIKED_ISCI)
  );
}

function* watchMapUnlinkedIsci() {
  yield takeEvery(
    MAP_UNLINKED_ISCI.request,
    sagaWrapper(mapUnlinkedIsci, MAP_UNLINKED_ISCI)
  );
}

function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(CLOSE_UNLINKED_ISCI_MODAL, closeUnlinkedIsciModal);
}

function* watchMapUnlinkedIsciSuccess() {
  yield takeEvery(MAP_UNLINKED_ISCI.success, mapUnlinkedIsciSuccess);
}

function* watchUndoArchivedIscis() {
  yield takeEvery(
    UNDO_ARCHIVED_ISCI.request,
    sagaWrapper(undoArchivedIscis, UNDO_ARCHIVED_ISCI)
  );
}

export default [
  watchUndoArchivedIscis,
  watchMapUnlinkedIsciSuccess,
  watchCloseUnlinkedIsciModal,
  watchMapUnlinkedIsci,
  watchRescrubUnlinkedIsci,
  watchLoadValidIscis,
  watchLoadArchivedIscis,
  watchArchiveUnlinkedIsci,
  watchRequestArchivedIscisSuccess,
  watchRequestUniqueIscisSuccess,
  watchRequestUniqueIscis,
  watchRequestArchivedFiltered,
  watchRequestUnlinkedFiltered
];
