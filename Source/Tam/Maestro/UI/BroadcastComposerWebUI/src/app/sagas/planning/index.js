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
/* REQUEST PROPOSALS */
/* ////////////////////////////////// */
export function* requestProposals() {
  const { getProposals } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposals',
        loading: true },
      });
    const response = yield getProposals();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposals',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposals data returned.',
          message: `The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposals data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSALS,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposals data returned.',
          message: 'The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs.',
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
/* REQUEST PROPOSAL LOCK */
/* ////////////////////////////////// */
export function* requestProposalLock({ payload: id }) {
  const { getProposalLock } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposalLock',
        loading: true },
      });
    const response = yield getProposalLock(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposalLock',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal lock data returned.',
          message: `The server encountered an error processing the request (proposal lock ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal lock data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal lock). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_LOCK,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: 'The server encountered an error processing the request (proposal lock). Please try again or contact your administrator to review error logs.',
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
/* REQUEST PROPOSAL UNLOCK */
/* ////////////////////////////////// */
export function* requestProposalUnlock({ payload: id }) {
  const { getProposalUnlock } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposalUnlock',
        loading: true },
      });
    const response = yield getProposalUnlock(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'proposalProposalUnlock',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal unlock data returned.',
          message: `The server encountered an error processing the request (proposal unlock ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal unlock data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal unlock). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_UNLOCK,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal unlock data returned.',
          message: 'The server encountered an error processing the request (proposal unlock). Please try again or contact your administrator to review error logs.',
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
          error: 'No proposal version data returned.',
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
/* SAVE PROPOSAL */
/* ////////////////////////////////// */
export function* saveProposal({ payload: params }) {
  /* eslint-disable no-shadow */
  const { saveProposal } = api.planning;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'saveProposal',
        processing: true,
      },
    });
    const response = yield saveProposal(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'saveProposal',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not saved.',
          message: `The server encountered an error processing the request (save proposal ${params.FileId}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not saved.',
          message: data.Message || `The server encountered an error processing the request (save proposal ${params.FileId}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    // yield put({
    //   type: ACTIONS.CREATE_ALERT,
    //   alert: {
    //     type: 'success',
    //     headline: 'Proposal Saved',
    //   },
    // });
    // window.location()
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Propsal not saved.',
          message: 'The server encountered an error processing the request (save proposal). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
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

/* ////////////////////////////////// */
/* DELETE PROPOSAL BY ID */
/* ////////////////////////////////// */
export function* deleteProposalById({ payload: id }) {
  const { deleteProposal } = api.planning;

  try {
    const response = yield deleteProposal(id);
    const { status, data } = response;
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not deleted.',
          message: `The server encountered an error processing the request (delete proposal ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not deleted.',
          message: data.Message || `The server encountered an error processing the request (delete proposal ${id}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: 'success',
        headline: 'Proposal Removed',
        message: `${id} was successfully removed.`,
      },
    });
    // yield put({
    //   type: ACTIONS.REQUEST_PROPOSALS,
    // });
    // window.location()
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not deleted.',
          message: 'The server encountered an error processing the request (delete proposal). Please try again or contact your administrator to review error logs.',
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
/* UNORDER PROPOSAL */
/* ////////////////////////////////// */

// . . .

/* ////////////////////////////////// */
/* REQUEST PROPOSAL DETAIL */
/* ////////////////////////////////// */

// . . .

/* ////////////////////////////////// */
/* UPDATE PROPOSAL */
/* ////////////////////////////////// */
export function* updateProposal({ payload: params }) {
  /* eslint-disable no-shadow */
  const { updateProposal } = api.planning;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'updateProposal',
        processing: true,
      },
    });
    const response = yield updateProposal(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'updateProposal',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not updated.',
          message: `The server encountered an error processing the request (update proposal ${params.FileId}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not updated.',
          message: data.Message || `The server encountered an error processing the request (update proposal ${params.FileId}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_UPDATED_PROPOSAL,
      data,
    }); // Is this an updated proposal object? // window.location to new version?
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: 'success',
        headline: 'Proposal Updated',
      },
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Propsal not updated.',
          message: 'The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
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


/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
export function* watchRequestProposalInitialData() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_INITIALDATA, requestProposalInitialData);
}

export function* watchRequestProposals() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSALS, requestProposals);
}

export function* watchRequestProposalLock() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_LOCK, requestProposalLock);
}

export function* watchRequestProposalUnlock() {
  yield takeEvery(ACTIONS.REQUEST_PROPOSAL_UNLOCK, requestProposalUnlock);
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

export function* watchSaveProposal() {
  yield takeEvery(ACTIONS.SAVE_PROPOSAL, saveProposal);
}

export function* watchDeleteProposalById() {
  yield takeEvery(ACTIONS.DELETE_PROPOSAL, deleteProposalById);
}

export function* watchUpdateProposal() {
  yield takeEvery(ACTIONS.UPDATE_PROPOSAL, updateProposal);
}

// if assign watcher > assign in sagas/index rootSaga also
