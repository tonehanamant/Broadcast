import moment from "moment";
import update from "immutability-helper";
import { sortBy, find, cloneDeep } from "lodash";

// Actions
import * as ACTIONS from "./actionTypes.js";

const initialState = {
  initialdata: {},
  planningProposals: [],
  activeOpenMarketData: undefined,
  openMarketData: undefined,
  hasOpenMarketData: false,
  hasActiveDistribution: false,
  isOpenMarketDataSortName: false,
  activeEditMarkets: [],
  isEditMarketsActive: false,
  openMarketLoading: false,
  openMarketLoaded: false,
  filteredPlanningProposals: [],
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
    // MarketGroup: { Id: 100, Display: 'Top 100', Count: 100 },
    // MarketGroupId: 100,
    MarketGroup: null,
    MarketGroupId: null,
    MarketCoverage: 0.8,
    Markets: [],
    Notes: null,
    PostType: 1,
    PrimaryVersionId: null,
    ProposalName: null,
    SecondaryDemos: [],
    SpotLengths: [],
    Status: 1,
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
    VersionId: 0
  },
  proposalEditForm: {},
  versions: [],
  proposalValidationStates: {
    FormInvalid: false,
    DetailInvalid: false,
    DetailGridsInvalid: false
  },
  genres: [],
  isGenresLoading: false,
  programs: [],
  isProgramsLoading: false,
  showTypes: [],
  isShowTypesLoading: false,
  isISCIEdited: false,
  isGridCellEdited: false
};

initialState.proposalEditForm = { ...initialState.proposal };

const sortMarketsData = (markets, isSortByName) => {
  const sortByType = isSortByName ? "MarketName" : "MarketRank";
  const sortedMarkets = sortBy(markets, sortByType);
  return sortedMarkets;
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    // PLANNING PROPOSALS  DATA
    case ACTIONS.RECEIVE_PROPOSALS:
      return {
        ...state,
        planningProposals: data.Data,
        filteredPlanningProposals: data.Data
      };

    case ACTIONS.FILTERED_PLANNING_PROPOSALS.success:
      return {
        ...state,
        planningProposals: data
      };

    // PROPOSAL INITIAL DATA
    case ACTIONS.RECEIVE_PROPOSAL_INITIALDATA:
      return {
        ...state,
        initialdata: data.Data
      };

    // PROPOSAL LOCK UNLOCK
    case ACTIONS.RECEIVE_PROPOSAL_LOCK:
      return {
        ...state,
        proposalLock: data.Data
      };

    // PROPOSAL
    case ACTIONS.RECEIVE_PROPOSAL:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data
      };

    // PROPOSAL VERSIONS
    case ACTIONS.RECEIVE_PROPOSAL_VERSIONS:
      return {
        ...state,
        versions: data.Data
      };

    case ACTIONS.RECEIVE_PROPOSAL_VERSION:
      return {
        ...state,
        proposal: data.Data,
        proposalEditForm: data.Data
      };

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM:
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          [payload.key]: payload.value
        }
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
              [payload.key]: payload.value
            },
            ...state.proposalEditForm.Details.slice(detailIndex + 1)
          ]
        }
      });
    }

    case ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(detail => detail.Id === payload.id);
      const quarterIndex = payload.quarterIndex;
      const weekIndex = payload.weekIndex;
      let rowIndex = details[detailIndex].GridQuarterWeeks.findIndex(
        row => row._key === payload.row
      );
      if (rowIndex === -1) rowIndex = payload.row.replace(/row-/, "");

      let newState = { ...state };

      if (quarterIndex !== null && weekIndex === null) {
        newState = update(state, {
          proposalEditForm: {
            Details: {
              [detailIndex]: {
                Quarters: {
                  [quarterIndex]: {
                    [payload.key]: { $set: payload.value },
                    DistributeGoals: {
                      $set:
                        payload.key === "ImpressionGoal" ||
                        payload.key === "Impressions"
                    }
                  }
                },
                GridQuarterWeeks: {
                  [rowIndex]: {
                    [payload.key]: { $set: payload.value }
                  }
                }
              }
            }
          }
        });
      } else if (quarterIndex !== null && weekIndex !== null) {
        newState = update(state, {
          proposalEditForm: {
            Details: {
              [detailIndex]: {
                Quarters: {
                  [quarterIndex]: {
                    DistributeGoals: { $set: false },
                    Weeks: {
                      [weekIndex]: {
                        [payload.key]: { $set: payload.value }
                      }
                    }
                  }
                },
                GridQuarterWeeks: {
                  [rowIndex]: {
                    [payload.key]: { $set: payload.value }
                  }
                }
              }
            }
          }
        });
      }

      return newState;
    }

    case ACTIONS.RECEIVE_NEW_PROPOSAL_DETAIL: {
      return {
        ...state,
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: [...state.proposalEditForm.Details, payload]
        }
      };
    }

    case ACTIONS.PROPOSAL_DETAIL_DELETED: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(detail => detail.Id === payload.id);
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: [
            ...state.proposalEditForm.Details.filter(
              (item, index) => index !== detailIndex
            )
          ]
        }
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
          TotalImpressionsMarginAchieved:
            data.Data.TotalImpressionsMarginAchieved,
          TargetUnits: data.Data.TargetUnits,
          SpotLengths: data.Data.SpotLengths,
          FlightStartDate: data.Data.FlightStartDate,
          FlightEndDate: data.Data.FlightEndDate,
          FlightWeeks: data.Data.FlightWeeks,
          Details: data.Data.Details
        }
      };

    case ACTIONS.SET_PROPOSAL_VALIDATION_STATE:
      return Object.assign({}, state, {
        proposalValidationStates: {
          ...state.proposalValidationStates,
          [payload.type]: payload.state
        }
      });

    case ACTIONS.RESTORE_PLANNING_PROPOSAL: {
      return {
        ...state,
        planning: payload
      };
    }

    case ACTIONS.TOGGLE_GENRE_LOADING: {
      return {
        ...state,
        isGenresLoading: !state.isGenresLoading
      };
    }

    case ACTIONS.RECEIVE_GENRES: {
      return {
        ...state,
        genres: payload
      };
    }

    case ACTIONS.TOGGLE_PROGRAM_LOADING: {
      return {
        ...state,
        isProgramsLoading: !state.isProgramsLoading
      };
    }

    case ACTIONS.RECEIVE_PROGRAMS: {
      return {
        ...state,
        programs: payload
      };
    }

    case ACTIONS.TOGGLE_SHOWTYPES_LOADING: {
      return {
        ...state,
        isShowTypesLoading: !state.isShowTypesLoading
      };
    }

    case ACTIONS.RECEIVE_SHOWTYPES: {
      return {
        ...state,
        showTypes: payload
      };
    }

    case ACTIONS.LOAD_OPEN_MARKET_DATA.request: {
      return {
        ...state,
        openMarketLoading: true
      };
    }

    case ACTIONS.ALLOCATE_SPOTS.success: {
      const isName = state.isOpenMarketDataSortName;
      if (isName) data.Data.Markets = sortMarketsData(data.Data.Markets, true);
      return {
        ...state,
        openMarketData: data.Data,
        hasOpenMarketData: data.Data.Markets && data.Data.Markets.length > 0,
        activeOpenMarketData: data.Data,
        openMarketLoading: false,
        openMarketLoaded: true
      };
    }
    case ACTIONS.LOAD_OPEN_MARKET_DATA.success: {
      return {
        ...state,
        openMarketData: data.Data,
        hasOpenMarketData: data.Data.Markets && data.Data.Markets.length > 0,
        hasActiveDistribution: true,
        activeOpenMarketData: data.Data,
        isOpenMarketDataSortName: false,
        openMarketLoading: false,
        openMarketLoaded: true,
        activeEditMarkets: cloneDeep(data.Data.AllMarkets),
        isEditMarketsActive: false
      };
    }

    case ACTIONS.LOAD_OPEN_MARKET_DATA.failure: {
      return {
        ...state,
        openMarketLoading: false
      };
    }

    case ACTIONS.CLEAR_OPEN_MARKET_DATA: {
      return {
        ...state,
        openMarketLoading: false,
        openMarketLoaded: false,
        openMarketData: undefined,
        activeOpenMarketData: undefined,
        hasOpenMarketData: false,
        hasActiveDistribution: false,
        isOpenMarketDataSortName: false
      };
    }

    case ACTIONS.FILTER_OPEN_MARKET_DATA.request: {
      return {
        ...state,
        openMarketLoading: true
      };
    }

    case ACTIONS.FILTER_OPEN_MARKET_DATA.success: {
      const isName = state.isOpenMarketDataSortName;
      if (isName) data.Data.Markets = sortMarketsData(data.Data.Markets, true);
      return {
        ...state,
        activeOpenMarketData: data.Data,
        openMarketLoading: false,
        openMarketLoaded: true
      };
    }

    case ACTIONS.FILTER_OPEN_MARKET_DATA.failure: {
      return {
        ...state,
        openMarketLoading: false
      };
    }

    case ACTIONS.SORT_OPEN_MARKET_DATA: {
      const activeData = { ...state.activeOpenMarketData };
      const sortedData = sortMarketsData(activeData.Markets, payload);
      // console.log("SORT_OPEN_MARKET_DATA", payload, sortedData);
      return Object.assign({}, state, {
        activeOpenMarketData: {
          ...state.activeOpenMarketData,
          Markets: sortedData
        },
        isOpenMarketDataSortName: payload
      });
    }

    case ACTIONS.SHOW_EDIT_MARKETS: {
      return {
        ...state,
        isEditMarketsActive: payload
      };
    }

    case ACTIONS.CHANGE_EDIT_MARKETS_DATA: {
      const editMarkets = [...state.activeEditMarkets];
      const market = find(editMarkets, { Id: payload.id });
      if (market) market.Selected = payload.isAdd;
      // console.log("Change Edit Markets", market, payload, editMarkets);
      return {
        ...state,
        activeEditMarkets: editMarkets
      };
    }

    case ACTIONS.DISCARD_EDIT_MARKETS_DATA: {
      const openMarketsData = { ...state.openMarketData };
      return {
        ...state,
        activeEditMarkets: cloneDeep(openMarketsData.AllMarkets)
      };
    }

    case ACTIONS.UPDATE_EDIT_MARKETS_DATA.success: {
      // console.log("UPDATE_EDIT_MARKETS_DATA success", data.Data);
      return {
        ...state,
        openMarketData: data.Data,
        hasOpenMarketData: data.Data.Markets && data.Data.Markets.length > 0,
        activeOpenMarketData: data.Data,
        isOpenMarketDataSortName: false,
        // openMarketLoading: false,
        // openMarketLoaded: true,
        activeEditMarkets: cloneDeep(data.Data.AllMarkets),
        isEditMarketsActive: false
      };
    }
    // update pricing/proprietary totals only on proprietary change if distribution active
    case ACTIONS.UPDATE_PROPRIETARY_CPMS.success: {
      return {
        ...state,
        openMarketData: {
          ...state.openMarketData,
          PricingTotals: data.Data.PricingTotals,
          ProprietaryTotals: data.Data.ProprietaryTotals
        },
        activeOpenMarketData: {
          ...state.activeOpenMarketData,
          PricingTotals: data.Data.PricingTotals,
          ProprietaryTotals: data.Data.ProprietaryTotals
        }
      };
    }

    case ACTIONS.SET_ESTIMATED_ID: {
      const details = [...state.proposalEditForm.Details];
      const detailIndex = details.findIndex(
        detail => detail.Id === payload.detailId
      );
      details[detailIndex].EstimateId = payload.estimatedId;
      return Object.assign({}, state, {
        proposalEditForm: {
          ...state.proposalEditForm,
          Details: details
        }
      });
    }

    default:
      return state;
  }
}

// Action Creators
export const getPlanningFiltered = query => ({
  type: ACTIONS.FILTERED_PLANNING_PROPOSALS.request,
  payload: query
});

export const receiveFilteredPlanning = data => ({
  type: ACTIONS.FILTERED_PLANNING_PROPOSALS.success,
  data
});

export const getProposalInitialData = () => ({
  type: ACTIONS.REQUEST_PROPOSAL_INITIALDATA,
  payload: {}
});

export const getProposals = () => ({
  type: ACTIONS.REQUEST_PROPOSALS,
  payload: {}
});

export const getProposalLock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_LOCK,
  payload: id
});

export const getProposalUnlock = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_UNLOCK,
  payload: id
});

export const getProposal = id => ({
  type: ACTIONS.REQUEST_PROPOSAL,
  payload: id
});

export const getProposalVersions = id => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSIONS,
  payload: id
});

export const getProposalVersion = (id, version) => ({
  type: ACTIONS.REQUEST_PROPOSAL_VERSION,
  payload: { id, version }
});

export const saveProposal = params => ({
  type: ACTIONS.SAVE_PROPOSAL,
  payload: params
});

export const saveProposalAsVersion = params => ({
  type: ACTIONS.SAVE_PROPOSAL_AS_VERSION,
  payload: params
});

export const deleteProposal = id => ({
  type: ACTIONS.DELETE_PROPOSAL,
  payload: id
});

export const updateProposal = params => ({
  type: ACTIONS.UPDATE_PROPOSAL,
  payload: params
});

export const updateProposalEditForm = keyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM,
  payload: keyValue
});

export const updateProposalEditFormDetail = idKeyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL,
  payload: idKeyValue
});

export const updateProposalEditFormDetailGrid = idKeyValue => ({
  type: ACTIONS.UPDATE_PROPOSAL_EDIT_FORM_DETAIL_GRID,
  payload: idKeyValue
});

export const deleteProposalDetail = params => ({
  type: ACTIONS.DELETE_PROPOSAL_DETAIL,
  payload: params
});

export const modelNewProposalDetail = flight => ({
  type: ACTIONS.MODEL_NEW_PROPOSAL_DETAIL,
  payload: flight
});

export const unorderProposal = id => ({
  type: ACTIONS.UNORDER_PROPOSAL,
  payload: id
});

export const setProposalValidationState = typeState => ({
  type: ACTIONS.SET_PROPOSAL_VALIDATION_STATE,
  payload: typeState
});

export const restorePlanningProposal = planningState => ({
  type: ACTIONS.RESTORE_PLANNING_PROPOSAL,
  payload: planningState
});

export const getGenres = query => ({
  type: ACTIONS.REQUEST_GENRES,
  payload: query
});

export const getPrograms = params => ({
  type: ACTIONS.REQUEST_PROGRAMS,
  payload: params
});

export const getShowTypes = params => ({
  type: ACTIONS.REQUEST_SHOWTYPES,
  payload: params
});

export const rerunPostScrubing = (propId, propdetailid) => ({
  type: ACTIONS.RERUN_POST_SCRUBING.request,
  payload: { propId, propdetailid }
});

/* export const loadOpenMarketData = (propId, propdetailid) => ({
  type: ACTIONS.LOAD_OPEN_MARKET_DATA.request,
  payload: { propId, propdetailid },
}); */

export const loadOpenMarketData = params => ({
  type: ACTIONS.LOAD_OPEN_MARKET_DATA.request,
  payload: params
});

export const clearOpenMarketData = () => ({
  type: ACTIONS.CLEAR_OPEN_MARKET_DATA
});

export const updateProprietaryCpms = params => ({
  type: ACTIONS.UPDATE_PROPRIETARY_CPMS.request,
  payload: params
});

export const uploadSCXFile = params => ({
  type: ACTIONS.SCX_FILE_UPLOAD.request,
  payload: params
});
export const filterOpenMarketData = params => ({
  type: ACTIONS.FILTER_OPEN_MARKET_DATA.request,
  payload: params
});
export const sortOpenMarketData = sortByName => ({
  type: ACTIONS.SORT_OPEN_MARKET_DATA,
  payload: sortByName
});
export const showEditMarkets = show => ({
  type: ACTIONS.SHOW_EDIT_MARKETS,
  payload: show
});
export const changeEditMarkets = (id, isAdd) => ({
  type: ACTIONS.CHANGE_EDIT_MARKETS_DATA,
  payload: { id, isAdd }
});
export const discardEditMarkets = () => ({
  type: ACTIONS.DISCARD_EDIT_MARKETS_DATA
});
export const updateEditMarkets = params => ({
  type: ACTIONS.UPDATE_EDIT_MARKETS_DATA.request,
  payload: params
});
export const setEstimatedId = (detailId, estimatedId) => ({
  type: ACTIONS.SET_ESTIMATED_ID,
  payload: {
    detailId,
    estimatedId
  }
});

/* export const allocateSpots = (data, detailId) => ({
  type: ACTIONS.ALLOCATE_SPOTS.request,
  payload: { data, detailId }
}); */
export const allocateSpots = data => ({
  type: ACTIONS.ALLOCATE_SPOTS.request,
  payload: data
});
