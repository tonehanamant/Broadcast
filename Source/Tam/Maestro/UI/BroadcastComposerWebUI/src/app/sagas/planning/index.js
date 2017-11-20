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
/* WATCHERS */
/* ////////////////////////////////// */
export function* watchRequestProposalInitialData() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_INITIALDATA, requestProposalInitialData);
}

// if assign watcher > assign in sagas/index rootSaga also
