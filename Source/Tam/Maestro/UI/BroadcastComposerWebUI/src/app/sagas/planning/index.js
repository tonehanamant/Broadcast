/* eslint-disable import/prefer-default-export */
import { takeEvery, put } from 'redux-saga/effects';

import * as appActions from 'Ducks/app/actionTypes';
import * as planningActions from 'Ducks/planning/actionTypes';
import api from '../api';

const ACTIONS = { ...appActions, ...planningActions };

/* ////////////////////////////////// */
/* REQUEST PROPOSAL INITIAL DATA */
/* ////////////////////////////////// */
export function* requestProposalInitialData() {
  const { getProposalInitialData } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalInitialData',
        loading: true },
      });
    const response = yield getProposalInitialData();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalInitialData',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal initial data returned.',
          message: `The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal initial data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_INITIALDATA,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal initial data returned.',
          message: 'The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST PROPOSAL */
/* ////////////////////////////////// */
export function* requestProposal({ payload: id }) {
  const { getProposal } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposal',
        loading: true },
      });
    const response = yield getProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposal',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: `The server encountered an error processing the request (proposal ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: 'The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST PROPOSAL VERSIONS */
/* ////////////////////////////////// */
export function* requestProposalVersions({ payload: id }) {
  const { getProposalVersions } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalVersions',
        loading: true },
      });
    const response = yield getProposalVersions(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalVersions',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal versions data returned.',
          message: `The server encountered an error processing the request (proposal versions data ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal versions data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal versions data). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_VERSIONS,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal versions data returned.',
          message: 'The server encountered an error processing the request (proposal versions data). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST PROPOSAL VERSION */
/* ////////////////////////////////// */
export function* requestProposalVersion({ payload: id, version }) {
  const { getProposalVersion } = api.planning;

  console.log('SAGA ID', id, 'SAGA V', version);
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalVersion',
        loading: true },
      });
    const response = yield getProposalVersion(id, version);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalVersion',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal versopm data returned.',
          message: `The server encountered an error processing the request (proposal version data ${id}, ${version}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal version data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal version data). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_VERSION,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal version data returned.',
          message: 'The server encountered an error processing the request (proposal version data). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}


/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
export function* watchRequestProposalInitialData() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_INITIALDATA, requestProposalInitialData);
}

export function* watchRequestProposal() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL, requestProposal);
}

export function* watchRequestProposalVersions() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_VERSIONS, requestProposalVersions);
}

export function* watchRequestProposalVersion() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_VERSION, requestProposalVersion);
}

// if assign watcher > assign in sagas/index rootSaga also
