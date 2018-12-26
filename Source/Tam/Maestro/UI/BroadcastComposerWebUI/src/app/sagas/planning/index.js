/* eslint-disable import/prefer-default-export */
// import { delay } from 'redux-saga';
import { takeEvery, put, call, select } from "redux-saga/effects";
// import { push } from 'react-router-redux';
import FuzzySearch from "fuzzy-search";
import moment from "moment";
import _ from "lodash";
import {
  deployError,
  createAlert,
  toggleModal,
  setOverlayLoading,
  setOverlayProcessing
} from "Ducks/app/index";
import { receiveFilteredPlanning, setEstimatedId } from "Ducks/planning/index";
import * as appActions from "Ducks/app/actionTypes";
import * as planningActions from "Ducks/planning/actionTypes";
import { hasSpotsAllocate, copyToBuy } from "Ducks/planning";

import sagaWrapper, { errorBuilder } from "../wrapper";
import api from "../api";

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
        id: "proposalInitialData",
        loading: true
      }
    });
    const response = yield getProposalInitialData();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalInitialData",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal initial data returned.",
          message: `The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal initial data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_INITIALDATA,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal initial data returned.",
          message:
            "The server encountered an error processing the request (proposal initial data). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}
/* ////////////////////////////////// */
/* Adjust PROPOSALS Data return */
/* ////////////////////////////////// */
export function adjustProposals(proposals) {
  const adjustProposals = proposals.map(item => {
    const proposal = item;
    proposal.displayId = String(proposal.Id);
    proposal.displayAdvertiser = proposal.Advertiser.Display;
    proposal.displayLastModified = moment(proposal.LastModified).format(
      "MM/DD/YYYY"
    );
    // handle empty dates
    const start = proposal.FlightStartDate
      ? moment(proposal.FlightStartDate).format("MM/DD/YYYY")
      : "";
    const end = proposal.FlightEndDate
      ? moment(proposal.FlightEndDate).format("MM/DD/YYYY")
      : "";
    proposal.displayFlights = `${start} - ${end}`;
    switch (proposal.Status) {
      case 1:
        proposal.displayStatus = "Proposed";
        break;
      case 2:
        proposal.displayStatus = "Agency on Hold";
        break;
      case 3:
        proposal.displayStatus = "Contracted";
        break;
      case 4:
        proposal.displayStatus = "Previously Contracted";
        break;
      default:
        proposal.displayStatus = "Undefined";
        break;
    }
    return proposal;
  });
  return adjustProposals;
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
        id: "proposalProposals",
        loading: true
      }
    });
    const response = yield getProposals();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalProposals",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposals data returned.",
          message: `The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposals data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // adjust the data for grid handling
    data.Data = yield adjustProposals(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSALS,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposals data returned.",
          message:
            "The server encountered an error processing the request (proposals). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

/* ////////////////////////////////// */
/* PLANNING FILTERED */
/* ////////////////////////////////// */
export function* getPlanningFiltered({ payload: query }) {
  const planningUnfiltered = yield select(
    state => state.planning.filteredPlanningProposals
  );
  // Removing date search like original
  // const keys = ['displayId', 'ProposalName', 'displayAdvertiser', 'displayStatus', 'displayFlights', 'Owner', 'displayLastModified'];
  const keys = [
    "displayId",
    "ProposalName",
    "displayAdvertiser",
    "displayStatus",
    "Owner"
  ];
  const searcher = new FuzzySearch(planningUnfiltered, keys, {
    caseSensitive: false
  });
  const planningFiltered = () => searcher.search(query);
  try {
    const filtered = yield planningFiltered();
    yield put(receiveFilteredPlanning(filtered));
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
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
        id: "proposalProposalLock",
        loading: true
      }
    });
    const response = yield getProposalLock(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalProposalLock",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal lock data returned.",
          message: `The server encountered an error processing the request (proposal lock ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal lock data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal lock). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_LOCK,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal data returned.",
          message:
            "The server encountered an error processing the request (proposal lock). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "proposalProposalUnlock",
        loading: true
      }
    });
    const response = yield getProposalUnlock(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalProposalUnlock",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal unlock data returned.",
          message: `The server encountered an error processing the request (proposal unlock ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal unlock data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal unlock). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_UNLOCK,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal unlock data returned.",
          message:
            "The server encountered an error processing the request (proposal unlock). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
    const qtr = {
      Id: item.Id,
      QuarterIdx: qidx,
      Type: "quarter",
      QuarterText: item.QuarterText,
      Cpm: item.Cpm,
      EditUnits: item.Cpm,
      ImpressionGoal: item.ImpressionGoal,
      EditImpressions: impEditGoal
    };
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
      week.Type = "week";
      week.EditImpressions = week.Impressions / 1000;
      week.EditUnits = week.Units;
      ret.push(week);
    });
  });
  const totals = {
    TotalUnits: detail.TotalUnits,
    TotalCost: detail.TotalCost,
    TotalImpressions: detail.TotalImpressions,
    Id: "total",
    Type: "total"
  }; // construct totals
  ret.push(totals);
  lastWeek.IsLast = true; // set last
  return ret;
}

export function flattenProposalDetails(proposal) {
  const proposalData = { ...proposal };
  // console.log('flattenProposalDetails', proposal, proposalData);
  proposalData.Details.map(detail => {
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
        id: "proposalProposal",
        loading: true
      }
    });
    const response = yield getProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalProposal",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal data returned.",
          message: `The server encountered an error processing the request (proposal ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // const payload = yield flattenProposalDetails(data.Data);
    // console.log('receiveProposal flatten', payload);
    data.Data = yield flattenProposalDetails(data.Data);
    // console.log('receiveProposal flatten', data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal data returned.",
          message:
            "The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "proposalVersions",
        loading: true
      }
    });
    const response = yield getProposalVersions(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalVersions",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal versions data returned.",
          message: `The server encountered an error processing the request (proposal versions data ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal versions data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal versions data). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_VERSIONS,
      data
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: "planningSwitchVersionsModal",
        active: true
      }
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal versions data returned.",
          message:
            "The server encountered an error processing the request (proposal versions data). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "proposalVersion",
        loading: true
      }
    });
    const response = yield getProposalVersion(id, version);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "proposalVersion",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal version data returned.",
          message: `The server encountered an error processing the request (proposal version data ${id}, ${version}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal version data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal version data). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL_VERSION,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal version data returned.",
          message:
            "The server encountered an error processing the request (proposal version data). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

/* ////////////////////////////////// */
/* PRE-SAVE (CLEAR Id WHERE Persisted: false) */
/* ////////////////////////////////// */
export function preSaveDetailIdNull(proposal) {
  const proposalData = { ...proposal };
  proposalData.Details.map(detail => {
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
        id: "saveProposal",
        processing: true
      }
    });
    // let proposal = { ...params.proposal };
    // BUG ISSUE - spread copy not deep (mutates temp create Ids)
    let proposal = _.cloneDeep(params.proposal);
    if (params.force) {
      proposal.ForceSave = true;
      proposal.ValidationWarning = null;
    }
    proposal = yield preSaveDetailIdNull(proposal);
    const isNew = proposal.Id === null;
    const response = yield saveProposal(proposal);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "saveProposal",
        processing: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not saved.",
          message: `The server encountered an error processing the request (save proposal ${
            params.FileId
          }). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not saved.",
          message:
            data.Message ||
            `The server encountered an error processing the request (save proposal ${
              params.FileId
            }). Please try again or contact your administrator to review error logs.`
        }
      });
      throw new Error();
    }
    if (!data.Data.ValidationWarning) {
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: "success",
          headline: "Proposal Saved Successfully",
          message: ""
        }
      });
    }
    if (isNew) {
      setTimeout(() => {
        window.location.assign(
          `/broadcastreact/planning/proposal/${data.Data.Id}`
        );
      }, 1000);
    } else {
      data.Data = yield flattenProposalDetails(data.Data);
      yield put({
        type: ACTIONS.RECEIVE_PROPOSAL,
        data
      });
    }
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Propsal not saved.",
          message:
            "The server encountered an error processing the request (save proposal). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "saveProposalAsVersion",
        processing: true
      }
    });
    // let proposal = { ...params };
    // BUG ISSUE - spread copy not deep (mutates temp create Ids)
    let proposal = _.cloneDeep(params);
    proposal.Version = null; // Set to null, BE assigns new version
    proposal = yield preSaveDetailIdNull(proposal);
    const response = yield saveProposal(proposal);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "saveProposalAsVersion",
        processing: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not saved as version.",
          message: `The server encountered an error processing the request (save proposal ${
            params.FileId
          }). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not saved as version.",
          message:
            data.Message ||
            `The server encountered an error processing the request (save proposal ${
              params.FileId
            }). Please try again or contact your administrator to review error logs.`
        }
      });
      throw new Error();
    }
    if (!data.Data.ValidationWarning) {
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: "success",
          headline: "Proposal Saved As Version Successfully",
          message: ""
        }
      });
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: "Saved Proposal Version",
          bodyText:
            "Would you like to continue working in the proposal OR exit the proposal and return to the Planning dashboard?",
          closeButtonText: "Continue",
          closeButtonBsStyle: "success",
          actionButtonText: "Exit",
          actionButtonBsStyle: "default",
          action: () => window.location.assign("/broadcastreact/planning"),
          dismiss: () => {}
        }
      }
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Propsal not saved.",
          message:
            "The server encountered an error processing the request (save proposal). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "deleteProposal",
        processing: true
      }
    });
    const response = yield deleteProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "deleteProposal",
        processing: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not deleted.",
          message: `The server encountered an error processing the request (delete proposal ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not deleted.",
          message:
            data.Message ||
            `The server encountered an error processing the request (delete proposal ${id}). Please try again or contact your administrator to review error logs.`
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: "success",
        headline: "Proposal Removed",
        message: `${id} was successfully removed.`
      }
    });
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "deleteProposal",
        processing: true
      }
    });
    //  yield call(delay, 2000);
    // yield put(push('/broadcast/planning'));
    setTimeout(() => {
      window.location = "/broadcastreact/planning";
    }, 1000);
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not deleted.",
          message:
            "The server encountered an error processing the request (delete proposal). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
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
        id: "unorderProposal",
        processing: true
      }
    });
    const response = yield unorderProposal(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "unorderProposal",
        processing: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal version data returned.",
          message: `The server encountered an error processing the request (unorder proposal data ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No unorder proposal data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (unorder proposal data). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    data.Data = yield flattenProposalDetails(data.Data);
    yield put({
      type: ACTIONS.RECEIVE_PROPOSAL,
      data
    });
  } catch (e) {
    if (e.response) {
      // capture here if 401 with data.Message only/ need to close overlay
      // console.log('unorder error catch', e.response);
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: "unorderProposal",
          processing: false
        }
      });
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No unorder proposal data returned.",
          message:
            e.response.data.Message ||
            "The server encountered an error processing the request (unorder proposal data). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

// . . .

/* ////////////////////////////////// */
/* REQUEST MODEL PROPOSAL DETAIL */
/* ////////////////////////////////// */

const assignIdFlightWeeks = (data, flightWeeks) => {
  let detail = { ...data, Id: moment().unix() * -1 }; // Negative identifies as unsaved
  detail = { ...detail, FlightWeeks: flightWeeks };
  return detail;
};

export function* modelNewProposalDetail({ payload: flight }) {
  const { getProposalDetail } = api.planning;
  try {
    yield put(
      setOverlayProcessing({
        id: "modelNewProposalDetail",
        processing: true
      })
    );
    const { status, data } = yield getProposalDetail(flight);
    if (status !== 200 || !data.Success) {
      throw new Error();
    }
    const payload = yield assignIdFlightWeeks(data.Data, flight.FlightWeeks);
    payload.GridQuarterWeeks = yield flattenDetail(payload);
    payload.SpotLengthId = 3; // Default SpotLength
    const Detail = { ...payload };
    let warnings = [];
    if (Detail) {
      if (
        Detail.DefaultProjectionBooks &&
        Detail.DefaultProjectionBooks.DefaultHutBook &&
        Detail.DefaultProjectionBooks.DefaultHutBook.HasWarning
      ) {
        warnings.push(
          Detail.DefaultProjectionBooks.DefaultShareBook.WarningMessage
        );
        warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

        payload.DefaultProjectionBooks.DefaultHutBook.HasWarning = false; // Unset to stop repeat unless BE explicit changes
      }
      if (
        Detail.DefaultProjectionBooks &&
        Detail.DefaultProjectionBooks.DefaultShareBook &&
        Detail.DefaultProjectionBooks.DefaultShareBook.HasWarning
      ) {
        warnings.push(
          Detail.DefaultProjectionBooks.DefaultShareBook.WarningMessage
        );
        warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

        payload.DefaultProjectionBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
      }
    }
    yield put(
      toggleModal({
        modal: "confirmModal",
        active: warnings.length > 0,
        properties: {
          titleText: "Warning",
          bodyText: null,
          bodyList: warnings,
          closeButtonText: "Cancel",
          closeButtonBsStyle: "default",
          actionButtonText: "Continue",
          actionButtonBsStyle: "warning",
          action: () => {},
          dismiss: () => {}
        }
      })
    );
    yield put({
      type: ACTIONS.RECEIVE_NEW_PROPOSAL_DETAIL,
      payload
    });
  } catch (e) {
    if (e.response) {
      yield put(
        deployError({
          error: "New detail not modeled.",
          message:
            "The server encountered an error processing the request (model new detail). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        })
      );
    }
    if (e.message) {
      yield put(
        deployError({
          type: ACTIONS.DEPLOY_ERROR,
          error: {
            message: e.message
          }
        })
      );
    }
  } finally {
    yield put(
      setOverlayProcessing({
        id: "modelNewProposalDetail",
        processing: false
      })
    );
  }
}

/* ////////////////////////////////// */
/* UPDATE PROPOSAL (FROM DETAILS) */
/* ////////////////////////////////// */
export function* updateProposal() {
  // { payload: params }
  /* eslint-disable no-shadow */
  const { updateProposal } = api.planning;
  const details = yield select(
    state => state.planning.proposalEditForm.Details
  );
  const proposalId = yield select(state => state.planning.proposalEditForm.Id);
  const params = { Id: proposalId, Details: details };
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "updateProposal",
        processing: true
      }
    });
    const response = yield updateProposal(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "updateProposal",
        processing: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not updated.",
          message: `The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Proposal not updated.",
          message:
            data.Message ||
            "The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // TODO resolve to get entire proposal
    data.Data = yield flattenProposalDetails(data.Data);
    const { Details } = data.Data;
    let warnings = [];
    if (Details) {
      Details.forEach((detail, index) => {
        if (
          detail.DefaultProjectionBooks &&
          detail.DefaultProjectionBooks.DefaultHutBook &&
          detail.DefaultProjectionBooks.DefaultHutBook.HasWarning
        ) {
          warnings.push(
            detail.DefaultProjectionBooks.DefaultShareBook.WarningMessage
          );
          warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

          data.Data.Details[
            index
          ].DefaultProjectionBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
        }
        if (
          detail.DefaultProjectionBooks &&
          detail.DefaultProjectionBooks.DefaultShareBook &&
          detail.DefaultProjectionBooks.DefaultShareBook.HasWarning
        ) {
          warnings.push(
            detail.DefaultProjectionBooks.DefaultShareBook.WarningMessage
          );
          warnings = Array.from(new Set(warnings)); // ES6 removes duplicates

          data.Data.Details[
            index
          ].DefaultProjectionBooks.DefaultShareBook.HasWarning = false; // Reset to stop repeat unless BE explicit changes
        }
      });
    }
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: "confirmModal",
        active: warnings.length > 0,
        properties: {
          titleText: "Warning",
          bodyText: null,
          bodyList: warnings,
          closeButtonText: "Cancel",
          closeButtonBsStyle: "default",
          actionButtonText: "Continue",
          actionButtonBsStyle: "warning",
          action: () => {},
          dismiss: () => {}
        }
      }
    });
    yield put({
      type: ACTIONS.RECEIVE_UPDATED_PROPOSAL,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "Propsal not updated.",
          message:
            "The server encountered an error processing the request (update proposal). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* requestGenres({ payload: query }) {
  const { getGenres } = api.planning;

  try {
    yield put({
      type: ACTIONS.TOGGLE_GENRE_LOADING,
      payload: {}
    });

    const response = yield getGenres(query);
    const { data } = response;
    yield put({
      type: ACTIONS.RECEIVE_GENRES,
      payload: data.Data || []
    });

    yield put({
      type: ACTIONS.TOGGLE_GENRE_LOADING,
      payload: {}
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.TOGGLE_GENRE_LOADING,
        payload: {}
      });

      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No genres data returned.",
          message:
            "The server encountered an error processing the request (genres). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }

    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* requestPrograms({ payload: params }) {
  const { getPrograms } = api.planning;

  try {
    yield put({
      type: ACTIONS.TOGGLE_PROGRAM_LOADING,
      payload: {}
    });

    const response = yield getPrograms(params);
    const { data } = response;
    yield put({
      type: ACTIONS.RECEIVE_PROGRAMS,
      payload: data.Data || []
    });

    yield put({
      type: ACTIONS.TOGGLE_PROGRAM_LOADING,
      payload: {}
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.TOGGLE_PROGRAM_LOADING,
        payload: {}
      });

      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No programs returned.",
          message:
            "The server encountered an error processing the request (programs). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }

    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* requestShowTypes({ payload: query }) {
  const { getShowTypes } = api.planning;

  try {
    yield put({
      type: ACTIONS.TOGGLE_SHOWTYPES_LOADING,
      payload: {}
    });

    const response = yield getShowTypes(query);
    const { data } = response;
    yield put({
      type: ACTIONS.RECEIVE_SHOWTYPES,
      payload: data.Data || []
    });

    yield put({
      type: ACTIONS.TOGGLE_SHOWTYPES_LOADING,
      payload: {}
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.TOGGLE_SHOWTYPES_LOADING,
        payload: {}
      });

      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No show types data returned.",
          message:
            "The server encountered an error processing the request (show types). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }

    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* deleteProposalDetail({ payload: params }) {
  yield put({
    type: ACTIONS.PROPOSAL_DETAIL_DELETED,
    payload: params
  });

  yield put({
    type: ACTIONS.UPDATE_PROPOSAL,
    payload: params
  });
}

export function* rerunPostScrubing({ propId, propdetailid }) {
  const { rerunPostScrubing } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "rerunPostScrubing", loading: true }));
    return yield rerunPostScrubing(propId, propdetailid);
  } finally {
    yield put(setOverlayLoading({ id: "rerunPostScrubing", loading: false }));
  }
}

export function* loadOpenMarketData(params) {
  const { loadOpenMarketData } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "openMarketPricing", loading: true }));
    return yield loadOpenMarketData(params);
  } finally {
    yield put(setOverlayLoading({ id: "openMarketPricing", loading: false }));
  }
}

export function* updateEditMarketsData(distributionRequest) {
  const { updateOpenEditMarketsData } = api.planning;
  const openMarketData = yield select(state => state.planning.openMarketData);
  const activeMarkets = yield select(state => state.planning.activeEditMarkets);
  const params = Object.assign({}, openMarketData, {
    Filter: {},
    AllMarkets: activeMarkets,
    ...distributionRequest
  });
  try {
    yield put(setOverlayLoading({ id: "editMarketsUpdate", loading: true }));
    return yield updateOpenEditMarketsData(params);
  } finally {
    yield put(setOverlayLoading({ id: "editMarketsUpdate", loading: false }));
  }
}

export function* updateEditMarketsDataSuccess() {
  yield put({
    type: ACTIONS.CREATE_ALERT,
    alert: {
      type: "success",
      headline: "Edit Markets Updated",
      message: ""
    }
  });
}
// update proprietary pricing guide if dsistribution active
export function* updateProprietaryCpms(distributionRequest) {
  const { updateProprietaryCpms } = api.planning;
  const openMarketData = yield select(state => state.planning.openMarketData);
  const params = Object.assign({}, openMarketData, {
    ...distributionRequest
  });
  try {
    yield put(setOverlayLoading({ id: "openMarketPricing", loading: true }));
    return yield updateProprietaryCpms(params);
  } finally {
    yield put(setOverlayLoading({ id: "openMarketPricing", loading: false }));
  }
}

/* export function* uploadSCXFile(params) {
  const { uploadSCXFile } = api.planning;
  try {
    yield put(setOverlayLoading({ id: 'uploadSCX', loading: true }));
    return yield uploadSCXFile(params);
  } finally {
    yield put(setOverlayLoading({ id: 'uploadSCX', loading: false }));
  }
} */

/* ////////////////////////////////// */
/* UPLOAD SCX - bypass wrapper to handle custom error */
/* ////////////////////////////////// */

export function* uploadSCXFileSuccess(detailId) {
  yield put(
    toggleModal({ modal: "uploadBuy", active: false, properties: { detailId } })
  );
  yield put(createAlert({ type: "success", headline: "SCX File Uploaded" }));
}

export function* uploadSCXFile({ payload: params }) {
  const { uploadSCXFile } = api.planning;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "uploadSCX",
        loading: true
      }
    });
    const response = yield uploadSCXFile(params);
    const { status, data } = response;
    // see below finally
    /*  yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'uploadSCX',
        loading: false,
      },
    }); */
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No upload SCX data returned.",
          message: `The server encountered an error processing SCX request upload. Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      if (data.Data && data.Data.length) {
        const ret = ["The following Problems Encountered Uploading SCX file:"];
        data.Data.forEach(item => {
          ret.push(item);
        });
        const message = ret.join("<br />");
        yield put({
          type: ACTIONS.DEPLOY_ERROR,
          error: {
            error: "Upload SCX File Error",
            message
          }
        });
        throw new Error();
      } else {
        yield put({
          type: ACTIONS.DEPLOY_ERROR,
          error: {
            error: "Problems Encountered Uploading SCX file",
            message:
              data.Message ||
              "The server encountered an error processing the request (upload SCX). Please try again or contact your administrator to review error logs."
          }
        });
        throw new Error();
      }
    }
    yield put(
      setEstimatedId(params.ProposalVersionDetailId, params.EstimateId)
    );
    yield call(uploadSCXFileSuccess, [params.ProposalVersionDetailId]);
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No upload SCX data returned.",
          message:
            "The server encountered an error processing the request (upload SCX). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  } finally {
    yield put(setOverlayLoading({ id: "uploadSCX", loading: false }));
  }
}

export function* filterOpenMarketData(filters) {
  const { filterOpenMarketData } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: true }));
    const original = yield select(state => state.planning.openMarketData);
    const request = Object.assign({}, original, filters);
    return yield filterOpenMarketData(request);
  } finally {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: false }));
  }
}

/* export function* allocateSpots({ data, detailId }) {
  const { allocateSpots } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: true }));
    return yield allocateSpots({ ...data, ProposalDetailId: detailId });
  } finally {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: false }));
  }
} */

export function* allocateSpots(data) {
  const { allocateSpots } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: true }));
    return yield allocateSpots(data);
  } finally {
    yield put(setOverlayLoading({ id: "openMarketFilter", loading: false }));
  }
}

export function* loadPricingData({ detailId }) {
  const { loadPricingData } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "loadPricingGuide", loading: true }));
    return yield loadPricingData(detailId);
  } finally {
    yield put(setOverlayLoading({ id: "loadPricingGuide", loading: false }));
  }
}

export function* loadPricingDataSuccess({ payload: { detailId } }) {
  yield put(
    toggleModal({
      modal: "pricingGuide",
      active: true,
      properties: { detailId }
    })
  );
}

export function* savePricingData(data) {
  const { savePricingData } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "savePricingGuide", loading: true }));
    return yield savePricingData(data);
  } finally {
    yield put(setOverlayLoading({ id: "savePricingGuide", loading: false }));
  }
}

export function* savePricingDataSuccess() {
  yield put(
    createAlert({
      type: "success",
      headline: "Pricing Guide Saved",
      message: `Pricing Guide was successfully saved.`
    })
  );
}

export function* copyToBuySaga({ detailId }) {
  const { copyToBuy } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "copyToBuy", loading: true }));
    return yield copyToBuy(detailId);
  } finally {
    yield put(setOverlayLoading({ id: "copyToBuy", loading: false }));
  }
}

export function* copyToBuyFlow({ payload: { detailId } }) {
  const { hasSpotsAllocated } = api.planning;
  try {
    yield put(setOverlayLoading({ id: "copyToBuyFlow", loading: true }));
    const {
      status,
      data: { Success, Data }
    } = yield hasSpotsAllocated(detailId);
    if (status !== 200 || !Success) {
      yield call(errorBuilder);
    } else if (Data) {
      yield put(hasSpotsAllocate(detailId, Data));
    } else {
      yield put(copyToBuy(detailId));
    }
  } catch (e) {
    yield call(errorBuilder);
  } finally {
    yield put(setOverlayLoading({ id: "copyToBuyFlow", loading: false }));
  }
}

export function* generateScx(payload) {
  const { checkAllocatedSpots } = api.planning;
  // const params = payload.ProposalDetailIds;
  try {
    yield put(setOverlayLoading({ id: "generateScx", loading: true }));
    return yield checkAllocatedSpots(payload);
  } finally {
    yield put(setOverlayLoading({ id: "generateScx", loading: false }));
  }
}

export function* generateScxSuccess({
  data: { Data },
  payload: { ProposalDetailIds, isSingle }
}) {
  console.log(Data, ProposalDetailIds, isSingle);
  if (Data) {
    const proposalId = yield select(
      state => state.planning.proposalEditForm.Id
    );
    const bodyText = isSingle
      ? "Operation will produce a single SCX file for this Proposal Detail."
      : "Operation will produce SCX files for all Open Market Inventory in each Proposal Detail.";
    let modalUrl = `${__API__}Proposals/GenerateScxArchive/${proposalId}`;
    if (isSingle) {
      const detailId = ProposalDetailIds[0];
      modalUrl = `${__API__}Proposals/GenerateScxDetail/${detailId}`;
    }
    yield put(
      toggleModal({
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: "Generate SCX file",
          bodyText,
          bodyList: ["Select Continue to proceed", "Select Cancel to cancel"],
          closeButtonText: "Cancel",
          actionButtonText: "Continue",
          actionButtonBsStyle: "success",
          action: () => {
            window.open(modalUrl, "_blank");
          },
          dismiss: () => {}
        }
      })
    );
  } else {
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: "warning",
        headline: "Generate SCX Unavailable",
        message:
          "There are no spots allocated for any buy on this proposal, no file will be generated"
      }
    });
  }
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

export function* watchRequestPlanningFiltered() {
  yield takeEvery(
    ACTIONS.FILTERED_PLANNING_PROPOSALS.request,
    getPlanningFiltered
  );
}

export function* watchRequestProposalInitialData() {
  yield takeEvery(
    ACTIONS.REQUEST_PROPOSAL_INITIALDATA,
    requestProposalInitialData
  );
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

export function* watchRequestGenres() {
  yield takeEvery(ACTIONS.REQUEST_GENRES, requestGenres);
}

export function* watchRequestPrograms() {
  yield takeEvery(ACTIONS.REQUEST_PROGRAMS, requestPrograms);
}

export function* watchRequestShowTypes() {
  yield takeEvery(ACTIONS.REQUEST_SHOWTYPES, requestShowTypes);
}

export function* watchDeleteProposalDetail() {
  yield takeEvery(ACTIONS.DELETE_PROPOSAL_DETAIL, deleteProposalDetail);
}

export function* watchRerunPostScrubing() {
  yield takeEvery(
    ACTIONS.RERUN_POST_SCRUBING.request,
    sagaWrapper(rerunPostScrubing, ACTIONS.RERUN_POST_SCRUBING)
  );
}

export function* watchLoadOpenMarketData() {
  yield takeEvery(
    ACTIONS.LOAD_OPEN_MARKET_DATA.request,
    sagaWrapper(loadOpenMarketData, ACTIONS.LOAD_OPEN_MARKET_DATA)
  );
}

export function* watchUpdateEditMarketsData() {
  yield takeEvery(
    ACTIONS.UPDATE_EDIT_MARKETS_DATA.request,
    sagaWrapper(updateEditMarketsData, ACTIONS.UPDATE_EDIT_MARKETS_DATA)
  );
}

export function* watchUpdateEditMarketsDataSuccess() {
  yield takeEvery(
    ACTIONS.UPDATE_EDIT_MARKETS_DATA.success,
    updateEditMarketsDataSuccess
  );
}

export function* watchUpdateProprietaryCpms() {
  yield takeEvery(
    ACTIONS.UPDATE_PROPRIETARY_CPMS.request,
    sagaWrapper(updateProprietaryCpms, ACTIONS.UPDATE_PROPRIETARY_CPMS)
  );
}

export function* watchUploadSCXFile() {
  yield takeEvery(ACTIONS.SCX_FILE_UPLOAD.request, uploadSCXFile);
}

export function* watchFilterOpenMarketData() {
  yield takeEvery(
    ACTIONS.FILTER_OPEN_MARKET_DATA.request,
    sagaWrapper(filterOpenMarketData, ACTIONS.FILTER_OPEN_MARKET_DATA)
  );
}

export function* watchAllocateSpots() {
  yield takeEvery(
    ACTIONS.ALLOCATE_SPOTS.request,
    sagaWrapper(allocateSpots, ACTIONS.ALLOCATE_SPOTS)
  );
}

export function* watchLoadPricingData() {
  yield takeEvery(
    ACTIONS.LOAD_PRICING_DATA.request,
    sagaWrapper(loadPricingData, ACTIONS.LOAD_PRICING_DATA)
  );
}

export function* watchLoadPricingDataSuccess() {
  yield takeEvery(ACTIONS.LOAD_PRICING_DATA.success, loadPricingDataSuccess);
}

export function* watchSavePricingDataSuccess() {
  yield takeEvery(ACTIONS.SAVE_PRICING_GUIDE.success, savePricingDataSuccess);
}

export function* watchSavePricingData() {
  yield takeEvery(
    ACTIONS.SAVE_PRICING_GUIDE.request,
    sagaWrapper(savePricingData, ACTIONS.SAVE_PRICING_GUIDE)
  );
}

export function* watchGenerateScx() {
  yield takeEvery(
    ACTIONS.GENERATE_SCX.request,
    sagaWrapper(generateScx, ACTIONS.GENERATE_SCX)
  );
}

export function* watchCopyToBuySaga() {
  yield takeEvery(
    ACTIONS.COPY_TO_BUY.request,
    sagaWrapper(copyToBuySaga, ACTIONS.COPY_TO_BUY)
  );
}

export function* watchCopyToBuyFlow() {
  yield takeEvery(ACTIONS.RUN_COPY_TO_BUY_FLOW, copyToBuyFlow);
}

export function* watchGenerateScxSuccess() {
  yield takeEvery(ACTIONS.GENERATE_SCX.success, generateScxSuccess);
}

// if assign watcher > assign in sagas/index rootSaga also
