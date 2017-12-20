import moment from 'moment';
import update from 'immutability-helper';

// Actions
import * as ACTIONS from './actionTypes.js';


const initialState = {
  initialdata: {},
  proposalLock: {},
  proposal: {
    AdvertiserId: 37674,
    BlackoutMarketGroup: null,
    BlackoutMarketGroupId: null,
    CanDelete: true,
    Details: [],
    Equivalized: true,
    FlightEndDate: moment(null),
    FlightStartDate: moment(null),
    FlightWeeks: [],
    ForceSave: false,
    GuaranteedDemoId: 31,
    Id: null,
    Margin: null,
    MarketGroup: { Id: 100, Display: 'Top 100', Count: 100 },
    MarketGroupId: 100,
    Markets: [],
    Notes: null,
    PostType: 1,
    PrimaryVersionId: null,
    ProposalName: null,
    SecondaryDemos: [],
    SpotLengths: [],
    Status: null,
    TargetBudget: 0,
    TargetCPM: 0,
    TargetImpressions: 0,
    TargetUnits: 0,
    TotalCPM: 0,
    TotalCPMMarginAchieved: false,
    TotalCPMPercent: 0,
    TotalCost: 0,
    TotalCostMarginAchieved: false,
    TotalCostPercent: 0,
    TotalImpressions: 0,
    TotalImpressionsMarginAchieved: false,
    TotalImpressionsPercent: 0,
    ValidationWarning: null,
    Version: null,
    VersionId: null,
  },
  proposalEditForm: {},
  versions: [],
};

initialState.proposalEditForm = { ...initialState.proposal };

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    // PROPOSAL INITIAL DATA
    case ACTIONS.RECEIVE_PROPOSAL_INITIALDATA:
      return {
        ...state,
        initialdata: data.Data,
      };

    // PROPOSAL LOCK UNLOCK
    case ACTIONS.RECEIVE_PROPOSAL_LOCK:
    return {
      ...state,
      proposalLock: data.Data,
    };

    // PROPOSAL
    case ACTIONS.RECEIVE_PROPOSAL:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data,
      };

    // PROPOSAL VERSIONS
    case ACTIONS.RECEIVE_PROPOSAL_VERSIONS:
      return {
        ...state,
        versions: data.Data,
      };

    case ACTIONS.RECEIVE_PROPOSAL_VERSION:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data,
      };

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM:
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          [payload.key]: payload.value,
        },
      });

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(detail => detail.Id === payload.id);
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: [
            // ...state.proposalEditForm.Details,
            // [detailIndex]: {
            //   ...state.proposalEditForm.Details[detailIndex],
            //   [payload.key]: payload.value,
            // },
            ...state.proposalEditForm.Details.slice(0, detailIndex),
            {
                ...state.proposalEditForm.Details[detailIndex],
                [payload.key]: payload.value,
            },
            ...state.proposalEditForm.Details.slice(detailIndex + 1),
          ],
        },
      });
    }

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(detail => detail.Id === payload.id);
      const quarterIndex = payload.quarterIndex;
      const weekIndex = payload.weekIndex;
      const rowIndex = details[detailIndex].GridQuarterWeeks.findIndex(row => row._key === payload.row);

      let newState = { ...state };

      if (quarterIndex !== null && weekIndex === null) {
        newState = update(state, {
          proposalEditForm: {
            Details: {
              [detailIndex]: {
                Quarters: {
                  [quarterIndex]: {
                    [payload.key]: { $set: payload.value },
                    DistributeGoals: { $set: payload.key === 'ImpressionGoal' },
                  },
                },
                GridQuarterWeeks: {
                  [rowIndex]: {
                    [payload.key]: { $set: payload.value },
                  },
                },
              },
            },
          },
        });
      } else if (quarterIndex !== null && weekIndex !== null) {
        newState = update(state, {
          proposalEditForm: {
            Details: {
              [detailIndex]: {
                Quarters: {
                  [quarterIndex]: {
                    Weeks: {
                      [weekIndex]: {
                        [payload.key]: { $set: payload.value },
                      },
                    },
                  },
                },
                GridQuarterWeeks: {
                  [rowIndex]: {
                    [payload.key]: { $set: payload.value },
                  },
                },
              },
            },
          },
        });
      }

      return newState;
    }

    case ACTIONS.RECEIVE_NEW_PROPOSAL_DETAIL: {
      return {
        ...state,
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: [
            ...state.proposalEditForm.Details,
            payload,
          ],
        },
      };
    }

    case ACTIONS.DELETE_PROPOSAL_DETAIL: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(detail => detail.Id === payload.id);
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: [
            ...state.proposalEditForm.Details.filter((item, index) => index !== detailIndex),
          ],
        },
      });
    }

    // PROPOSAL
    case ACTIONS.RECEIVE_UPDATED_PROPOSAL:
    return {
      ...state,
      // proposal: data.Data,
      // proposalEditForm: data.Data,
      proposalEditForm: {
        ...state.proposalEditForm,
        TotalCPM: data.Data.TotalCPM,
        TargetCPM: data.Data.TargetCPM,
        TotalCPMPercent: data.Data.TotalCPMPercent,
        TotalCPMMarginAchieved: data.Data.TotalCPMMarginAchieved,
        TotalCost: data.Data.TotalCost,
        TargetBudget: data.Data.TargetBudget,
        TotalCostPercent: data.Data.TotalCostPercent,
        TotalCostMarginAchieved: data.Data.TotalCostMarginAchieved,
        TotalImpressions: data.Data.TotalImpressions,
        TargetImpressions: data.Data.TargetImpressions,
        TotalImpressionsPercent: data.Data.TotalImpressionsPercent,
        TotalImpressionsMarginAchieved: data.Data.TotalImpressionsMarginAchieved,
        TargetUnits: data.Data.TargetUnits,
        SpotLengths: data.Data.SpotLengths,
        FlightStartDate: data.Data.FlightStartDate,
        FlightEndDate: data.Data.FlightEndDate,
        FlightWeeks: data.Data.FlightWeeks,
        Details: data.Data.Details,
      },
    };

    default:
      return state;
  }
}

// Action Creators
export const getProposalInitialData = () => ({
  type: ACTIONS.REQUEST_PROPOSAL_INITIALDATA,
  payload: {},
});

export const getProposals = () => ({
  type: ACTIONS.REQUEST_PROPOSALS,
  payload: {},
});

export const getProposalLock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_LOCK,
  payload: id,
});

export const getProposalUnlock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_UNLOCK,
  payload: id,
});

export const getProposal = id => ({
  type: ACTIONS.REQUEST_PROPOSAL,
  payload: id,
});

export const getProposalVersions = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSIONS,
  payload: id,
});

export const getProposalVersion = (id, version) => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSION,
  payload: { id, version },
});

export const saveProposal = params => ({
  type: ACTIONS.SAVE_PROPOSAL,
  payload: params,
});

export const saveProposalAsVersion = params => ({
  type: ACTIONS.SAVE_PROPOSAL_AS_VERSION,
  payload: params,
});

export const deleteProposal = id => ({
  type: ACTIONS.DELETE_PROPOSAL,
  payload: id,
});

export const updateProposal = params => ({
  type: ACTIONS.UPDATE_PROPOSAL,
  payload: params,
});

export const updateProposalEditForm = keyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM,
  payload: keyValue,
});

export const updateProposalEditFormDetail = idKeyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL,
  payload: idKeyValue,
});

export const updateProposalEditFormDetailGrid = idKeyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID,
  payload: idKeyValue,
});

export const deleteProposalDetail = params => ({
  type: ACTIONS.DELETE_PROPOSAL_DETAIL,
  payload: params,
});

export const modelNewProposalDetail = flight => ({
  type: ACTIONS.MODEL_NEW_PROPOSAL_DETAIL,
  payload: flight,
});
