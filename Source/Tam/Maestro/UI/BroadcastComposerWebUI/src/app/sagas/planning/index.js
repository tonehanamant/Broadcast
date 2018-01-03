/* eslint-disable import/prefer-default-export */
import { delay } from 'redux-saga';
import { call, takeEvery, put, select } from 'redux-saga/effects';
import { push } from 'react-router-redux';
import moment from 'moment';

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
/* FLATTEN DETAIL HELPERS */
/* ////////////////////////////////// */
export function flattenDetail(detailSet) {
  // NORMALIZE editing values for grid display/editing: EditUnits(quarter Cpm, week Units) EditImpressions (quarter ImpressionGoal, week Impressions)
  const detail = { ...detailSet };
  const ret = [];
  // count weeks and determine last for NEXT handling - take into account IsHiatus
  let weekCnt = 0;
  // const qtrLast = detail.Quarters.length - 1;
  let lastWeek = null;
  detail.Quarters.forEach((item, qidx) => {
    const impEditGoal = item.ImpressionGoal / 1000;
    const qtr = { Id: item.Id, QuarterIdx: qidx, Type: 'quarter', QuarterText: item.QuarterText, Cpm: item.Cpm, EditUnits: item.Cpm, ImpressionGoal: item.ImpressionGoal, EditImpressions: impEditGoal };
    ret.push(qtr);
    // const weekLast = item.Weeks.length - 1;
    item.Weeks.forEach((weekItem, widx) => {
      const week = { ...weekItem };
      // only add WeekCnt if not hiatus
      if (!week.IsHiatus) {
        // const isLast = (qtrLast === qidx) && (weekLast === widx);
        weekCnt += 1;
        week.WeekCnt = weekCnt;
        week.IsLast = false; // set default
        lastWeek = week; // store last
      }
      // store for finding later
      week.QuarterId = item.Id;
      // store indexes
      week.QuarterIdx = qidx;
      week.WeekIdx = widx;
      week.Type = 'week';
      week.EditImpressions = week.Impressions / 1000;
      week.EditUnits = week.Units;
      ret.push(week);
    });
  });
  const totals = { TotalUnits: detail.TotalUnits, TotalCost: detail.TotalCost, TotalImpressions: detail.TotalImpressions, Id: 'total', Type: 'total' }; // construct totals
  ret.push(totals);
  lastWeek.IsLast = true; // set last
  return ret;
}

export function flattenProposalDetails(proposal) {
  const proposalData = { ...proposal };
  // console.log('flattenProposalDetails', proposal, proposalData);
  proposalData.Details.map((detail) => {
    const set = detail;
    set.GridQuarterWeeks = flattenDetail(detail);
    return set;
  });
  return proposalData;
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
    // const payload = yield flattenProposalDetails(data.Data);
    // console.log('receiveProposal flatten', payload);
    data.Data = yield flattenProposalDetails(data.Data);
    // console.log('receiveProposal flatten', data.Data);
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
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'planningSwitchVersionsModal',
        active: true,
      },
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
    data.Data = yield flattenProposalDetails(data.Data);
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
/* PRE-SAVE (CLEAR Id WHERE Persisted: false) */
/* ////////////////////////////////// */
export function preSaveDetailIdNull(proposal) {
  const proposalData = { ...proposal };
  proposalData.Details.map((detail) => {
    const set = detail;
    if (detail.Id < 0) {
      set.Id = null;
    }
    return set;
  });
  return proposalData;
  // return proposal;
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
    let proposal = { ...params.proposal };
        if (params.force) {
          proposal.ForceSave = true;
          proposal.ValidationWarning = null;
        }
        proposal = yield preSaveDetailIdNull(proposal);
    const response = yield saveProposal(proposal);
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
    if (!data.Data.ValidationWarning) {
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: 'success',
          headline: 'Proposal Saved Successfully',
          message: '',
        },
      });
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data,
    });
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
/* SAVE PROPOSAL AS VERSION */
/* ////////////////////////////////// */
export function* saveProposalAsVersion({ payload: params }) {
  /* eslint-disable no-shadow */
  const { saveProposal } = api.planning;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'saveProposalAsVersion',
        processing: true,
      },
    });
    let proposal = { ...params };
        proposal.Version = null; // Set to null, BE assigns new version
        proposal = yield preSaveDetailIdNull(proposal);
    const response = yield saveProposal(proposal);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'saveProposalAsVersion',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not saved as version.',
          message: `The server encountered an error processing the request (save proposal ${params.FileId}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not saved as version.',
          message: data.Message || `The server encountered an error processing the request (save proposal ${params.FileId}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    if (!data.Data.ValidationWarning) {
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: 'success',
          headline: 'Proposal Saved As Version Successfully',
          message: '',
        },
      });
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data,
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'confirmModal',
        active: true,
        properties: {
          titleText: 'Saved Proposal Version',
          bodyText: 'Would you like to continue working in the proposal OR exit the proposal and return to the Planning dashboard?',
          closeButtonText: 'Continue',
          closeButtonBsStyle: 'success',
          actionButtonText: 'Exit',
          actionButtonBsStyle: 'default',
          action: () => window.location.assign('/broadcast/planning'),
          dismiss: () => {},
        },
      },
    });
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
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'deleteProposal',
        processing: true,
      },
    });
    const response = yield deleteProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'deleteProposal',
        processing: false,
      },
    });
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
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'deleteProposal',
        processing: true,
      },
    });
    yield call(delay, 2000);
    yield put(push('/broadcast/planning'));
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

export function* unorderProposal({ payload: id }) {
  const { unorderProposal } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'unorderProposal',
        processing: true,
      },
    });
    const response = yield unorderProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'unorderProposal',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal version data returned.',
          message: `The server encountered an error processing the request (unorder proposal data ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No unorder proposal data returned.',
          message: data.Message || 'The server encountered an error processing the request (unorder proposal data). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data,
    });
  } catch (e) {
    if (e.response) {
      // capture here if 401 with data.Message only/ need to close overlay
      // console.log('unorder error catch', e.response);
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: 'unorderProposal',
          processing: false,
        },
      });
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No unorder proposal data returned.',
          message: e.response.data.Message || 'The server encountered an error processing the request (unorder proposal data). Please try again or contact your administrator to review error logs.',
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

// . . .


/* ////////////////////////////////// */
/* REQUEST MODEL PROPOSAL DETAIL */
/* ////////////////////////////////// */

export function* modelNewProposalDetail({ payload: params }) {
  /* eslint-disable no-shadow */
  const { getProposalDetail } = api.planning;
  const assignIdFlightWeeks = (data, flightWeeks) => {
    let detail = { ...data, Id: moment().unix() * -1 }; // Negative identifies as unsaved
    detail = { ...detail, FlightWeeks: flightWeeks };
    return detail;
  };
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'modelNewProposalDetail',
        processing: true,
      },
    });
    const flight = { ...params };
    const response = yield getProposalDetail(flight);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'modelNewProposalDetail',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'New detail not modeled.',
          message: `The server encountered an error processing the request (model new detail). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'New detail not modeled.',
          message: data.Message || 'The server encountered an error processing the request (model new detail). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    const payload = yield assignIdFlightWeeks(data.Data, flight.FlightWeeks);
    payload.GridQuarterWeeks = yield flattenDetail(payload);
    payload.SpotLengthId = 3; // Default SpotLength
    const Detail = { ...payload };
    let warnings = [];
    if (Detail) {
      if (Detail.DefaultPostingBooks &&
          Detail.DefaultPostingBooks.DefautlHutBook &&
          Detail.DefaultPostingBooksDefautlHutBook.HasWarning) {
          warnings.push(Detail.DefaultPostingBooks.DefaultShareBook.WarningMessage);
          warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

          payload.DefaultPostingBooksDefautlHutBook.HasWarning = false; // Unset to stop repeat unless BE explicit changes
      }
      if (Detail.DefaultPostingBooks &&
          Detail.DefaultPostingBooks.DefaultShareBook &&
          Detail.DefaultPostingBooks.DefaultShareBook.HasWarning) {
          warnings.push(Detail.DefaultPostingBooks.DefaultShareBook.WarningMessage);
          warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

          payload.DefaultPostingBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
      }
    }
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'confirmModal',
        active: warnings.length > 0,
        properties: {
          titleText: 'Warning',
          bodyText: null,
          bodyList: warnings,
          closeButtonText: 'Cancel',
          closeButtonBsStyle: 'default',
          actionButtonText: 'Continue',
          actionButtonBsStyle: 'warning',
          action: () => {},
          dismiss: () => {},
        },
      },
    });
    yield put({
      type: ACTIONS.RECEIVE_NEW_PROPOSAL_DETAIL,
      payload,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'New detail not modeled.',
          message: 'The server encountered an error processing the request (model new detail). Please try again or contact your administrator to review error logs.',
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
/* UPDATE PROPOSAL (FROM DETAILS) */
/* ////////////////////////////////// */
export function* updateProposal() { // { payload: params }
  /* eslint-disable no-shadow */
  const { updateProposal } = api.planning;
  const details = yield select(state => state.planning.proposalEditForm.Details);
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'updateProposal',
        processing: true,
      },
    });
    const response = yield updateProposal(details);
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
          message: `The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Proposal not updated.',
          message: data.Message || 'The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    // TODO resolve to get entire proposal
    data.Data = yield flattenProposalDetails(data.Data);
    const { Details } = data.Data;
    let warnings = [];
    if (Details) {
      Details.forEach((detail, index) => {
        if (detail.DefaultPostingBooks &&
            detail.DefaultPostingBooks.DefautlHutBook &&
            detail.DefaultPostingBooksDefautlHutBook.HasWarning) {
            warnings.push(detail.DefaultPostingBooks.DefaultShareBook.WarningMessage);
            warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

            data.Data.Details[index].DefaultPostingBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
        }
        if (detail.DefaultPostingBooks &&
            detail.DefaultPostingBooks.DefaultShareBook &&
            detail.DefaultPostingBooks.DefaultShareBook.HasWarning) {
            warnings.push(detail.DefaultPostingBooks.DefaultShareBook.WarningMessage);
            warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

            data.Data.Details[index].DefaultPostingBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
        }
      });
    }
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'confirmModal',
        active: warnings.length > 0,
        properties: {
          titleText: 'Warning',
          bodyText: null,
          bodyList: warnings,
          closeButtonText: 'Cancel',
          closeButtonBsStyle: 'default',
          actionButtonText: 'Continue',
          actionButtonBsStyle: 'warning',
          action: () => {},
          dismiss: () => {},
        },
      },
    });
    yield put({
      type: ACTIONS.RECEIVE_UPDATED_PROPOSAL,
      data,
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

export function* watchSaveProposalAsVersion() {
  yield takeEvery(ACTIONS.SAVE_PROPOSAL_AS_VERSION, saveProposalAsVersion);
}

export function* watchDeleteProposalById() {
  yield takeEvery(ACTIONS.DELETE_PROPOSAL, deleteProposalById);
}

export function* watchUpdateProposal() {
  yield takeEvery(ACTIONS.UPDATE_PROPOSAL, updateProposal);
}

export function* watchModelNewProposalDetail() {
  yield takeEvery(ACTIONS.MODEL_NEW_PROPOSAL_DETAIL, modelNewProposalDetail);
}

export function* watchModelUnorderProposal() {
  yield takeEvery(ACTIONS.UNORDER_PROPOSAL, unorderProposal);
}

// if assign watcher > assign in sagas/index rootSaga also
