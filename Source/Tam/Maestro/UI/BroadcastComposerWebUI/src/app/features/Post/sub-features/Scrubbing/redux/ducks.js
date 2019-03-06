import { getDay, getDateInFormat } from "Utils/dateFormatter.js";
import { createAction } from "Utils/action-creator";
import { ducksRoot } from "Post/redux";

const ROOT = `${ducksRoot}/scrubbing`;

export const UNDO_SCRUB_STATUS = createAction(`${ROOT}/UNDO_SCRUB_STATUS`);
export const LOAD_POST_CLIENT_SCRUBBING = createAction(
  `${ROOT}/LOAD_POST_CLIENT_SCRUBBING`
);
export const SWAP_PROPOSAL_DETAIL = createAction(
  `${ROOT}/SWAP_PROPOSAL_DETAIL`
);
export const FILTERED_SCRUBBING_DATA = createAction(
  `${ROOT}/FILTERED_SCRUBBING_DATA`
);
export const POST_OVERRIDE_STATUS = createAction(
  `${ROOT}/POST_OVERRIDE_STATUS`
);

export const REQUEST_CLEAR_FILTERED_SCRUBBING_DATA = `${ROOT}/REQUEST_CLEAR_FILTERED_SCRUBBING_DATA`;
export const RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA = `${ROOT}/RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA`;
export const CLEAR_FILTERED_SCRUBBING_DATA = `${ROOT}/CLEAR_FILTERED_SCRUBBING_DATA`;

export const SAVE_NEW_CLIENT_SCRUBS = `${ROOT}/SAVE_NEW_CLIENT_SCRUBS`;

export const REQUEST_CLEAR_SCRUBBING_FILTERS_LIST = `${ROOT}/REQUEST_CLEAR_SCRUBBING_FILTERS_LIST`;
export const RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST = `${ROOT}/RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST`;

const initialState = {
  proposalHeader: {},
  unlinkedIscisLength: 0,
  scrubbingFiltersList: [],
  activeScrubbingFilters: {},
  activeFilterKey: "All", // represents global Filter state: 'All', 'InSpec', 'OutOfSpec'
  hasActiveScrubbingFilters: false, // specific filters are active
  defaultScrubbingFilters: {
    Affiliate: {
      filterDisplay: "Affiliates",
      filterKey: "Affiliate",
      distinctKey: "DistinctAffiliates",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchAffiliate", // not currently available
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    ClientISCI: {
      filterDisplay: "Client ISCIs",
      filterKey: "ClientISCI",
      distinctKey: "DistinctClientIscis",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchClientISCI", // not currently available
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    Comments: {
      filterDisplay: "Comments",
      filterKey: "Comments",
      distinctKey: "DistinctComments",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: null, // not currently available
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    DateAired: {
      filterDisplay: "Select Date Range",
      filterKey: "DateAired",
      type: "dateInput",
      active: false,
      exclusions: false,
      filterOptions: {}
    },
    DayOfWeek: {
      filterDisplay: "Days",
      filterKey: "DayOfWeek",
      distinctKey: "DistinctDayOfWeek",
      type: "filterList",
      hasMatchSpec: true,
      active: false,
      activeMatch: false,
      matchOptions: {
        matchKey: "MatchIsciDays",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    GenreName: {
      filterDisplay: "Genres",
      filterKey: "GenreName",
      distinctKey: "DistinctGenres",
      type: "filterList",
      hasMatchSpec: true,
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchGenre",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    ISCI: {
      filterDisplay: "House ISCIs",
      filterKey: "ISCI",
      distinctKey: "DistinctHouseIscis",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchISCI", // not currently available
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    Market: {
      filterDisplay: "Markets",
      filterKey: "Market",
      distinctKey: "DistinctMarkets",
      type: "filterList",
      hasMatchSpec: true,
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchMarket",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    ProgramName: {
      filterDisplay: "Programs",
      filterKey: "ProgramName",
      distinctKey: "DistinctPrograms",
      type: "filterList",
      hasMatchSpec: true,
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchProgram",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    Sequence: {
      filterDisplay: "Sequences",
      filterKey: "Sequence",
      distinctKey: "DistinctSequences",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: null, // NA
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    ShowTypeName: {
      filterDisplay: "Show Types",
      filterKey: "ShowTypeName",
      distinctKey: "DistinctShowTypes",
      type: "filterList",
      hasMatchSpec: true,
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchShowType",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    SpotLength: {
      filterDisplay: "Spot Lengths",
      filterKey: "SpotLength",
      distinctKey: "DistinctSpotLengths",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchSpotLength",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    Station: {
      filterDisplay: "Stations",
      filterKey: "Station",
      distinctKey: "DistinctStations",
      type: "filterList",
      hasMatchSpec: true,
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: "MatchStation",
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    },
    TimeAired: {
      filterDisplay: "Select Time Range",
      filterKey: "TimeAired",
      type: "timeInput",
      active: false,
      exclusions: false,
      filterOptions: {}
    },
    WeekStart: {
      filterDisplay: "Week Starts",
      filterKey: "WeekStart",
      distinctKey: "DistinctWeekStarts",
      type: "filterList",
      hasMatchSpec: false, // NA
      activeMatch: false,
      active: false,
      matchOptions: {
        matchKey: null, // NA
        inSpec: true,
        outOfSpec: true
      },
      exclusions: [],
      filterOptions: []
    }
  }
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    case RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA: {
      return {
        ...state,
        proposalHeader: {
          ...state.proposalHeader,
          activeScrubbingData: {
            ...state.proposalHeader.activeScrubbingData,
            ClientScrubs: data.originalScrubs
          }
        },
        hasActiveScrubbingFilters: false,
        activeScrubbingFilters: data.activeFilters,
        scrubbingFiltersList: [data.activeFilters]
      };
    }

    case LOAD_POST_CLIENT_SCRUBBING.success: {
      const filtersData = data.Data.Filters;
      const activeFilters = { ...state.defaultScrubbingFilters }; // todo seems to get mutated
      const prepareFilterOptions = () => {
        const affiliateOptions = [];
        const clientIsciOptions = [];
        const commentsOptions = [];
        const dayOfWeekOptions = [];
        const genreOptions = [];
        const houseIsciOptions = [];
        const marketOptions = [];
        const programOptions = [];
        const sequences = [];
        const spotLengthOptions = [];
        const showTypeOptions = [];
        const stationOptions = [];
        const weekStartOptions = [];
        filtersData.DistinctAffiliates.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          affiliateOptions.push(ret);
        });
        filtersData.DistinctClientIscis.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          clientIsciOptions.push(ret);
        });
        filtersData.DistinctComments.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          commentsOptions.push(ret);
        });
        filtersData.DistinctDayOfWeek.forEach(item => {
          const display = getDay(item);
          const ret = { Value: item, Selected: true, Display: display };
          dayOfWeekOptions.push(ret);
        });
        filtersData.DistinctGenres.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          genreOptions.push(ret);
        });
        filtersData.DistinctHouseIscis.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          houseIsciOptions.push(ret);
        });
        filtersData.DistinctMarkets.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          marketOptions.push(ret);
        });
        filtersData.DistinctPrograms.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          programOptions.push(ret);
        });
        filtersData.DistinctSequences.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          sequences.push(ret);
        });
        filtersData.DistinctShowTypes.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          showTypeOptions.push(ret);
        });
        filtersData.DistinctSpotLengths.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          spotLengthOptions.push(ret);
        });
        filtersData.DistinctStations.forEach(item => {
          const ret = { Value: item, Selected: true, Display: item };
          stationOptions.push(ret);
        });
        // display as formatted; values as date string
        filtersData.DistinctWeekStarts.forEach(item => {
          const display = getDateInFormat(item);
          const ret = { Value: item, Selected: true, Display: display };
          weekStartOptions.push(ret);
        });
        activeFilters.Affiliate.filterOptions = affiliateOptions;
        activeFilters.ClientISCI.filterOptions = clientIsciOptions;
        activeFilters.Comments.filterOptions = commentsOptions;
        activeFilters.DayOfWeek.filterOptions = dayOfWeekOptions;
        activeFilters.DateAired.filterOptions = {
          DateAiredStart: filtersData.DateAiredStart,
          DateAiredEnd: filtersData.DateAiredEnd,
          originalDateAiredStart: filtersData.DateAiredStart,
          originalDateAiredEnd: filtersData.DateAiredEnd
        };
        activeFilters.TimeAired.filterOptions = {
          TimeAiredStart: filtersData.TimeAiredStart,
          TimeAiredEnd: filtersData.TimeAiredEnd,
          originalTimeAiredStart: filtersData.TimeAiredStart,
          originalTimeAiredEnd: filtersData.TimeAiredEnd
        };
        activeFilters.GenreName.filterOptions = genreOptions;
        activeFilters.ISCI.filterOptions = houseIsciOptions;
        activeFilters.Market.filterOptions = marketOptions;
        activeFilters.ProgramName.filterOptions = programOptions;
        activeFilters.ShowTypeName.filterOptions = showTypeOptions;
        activeFilters.Sequence.filterOptions = sequences;
        activeFilters.SpotLength.filterOptions = spotLengthOptions;
        activeFilters.Station.filterOptions = stationOptions;
        activeFilters.WeekStart.filterOptions = weekStartOptions;
      };
      prepareFilterOptions();
      return {
        ...state,
        activeFilterKey: data.Data.filterKey
          ? data.Data.filterKey
          : state.activeFilterKey,
        hasActiveScrubbingFilters: false,
        proposalHeader: {
          scrubbingData: data.Data,
          activeScrubbingData: data.Data
        },
        activeScrubbingFilters: activeFilters,
        scrubbingFiltersList: [activeFilters]
      };
    }

    case FILTERED_SCRUBBING_DATA.success:
      return {
        ...state,
        proposalHeader: {
          ...state.proposalHeader,
          activeScrubbingData: {
            ...state.proposalHeader.activeScrubbingData,
            ClientScrubs: data.filteredClientScrubs
          }
        },
        activeScrubbingFilters: data.activeFilters,
        scrubbingFiltersList: [data.activeFilters],
        hasActiveScrubbingFilters: data.hasActiveScrubbingFilters
      };

    case POST_OVERRIDE_STATUS.store:
      return {
        ...state,
        proposalHeader: {
          ...state.proposalHeader,
          scrubbingData: data.scrubbingData,
          activeScrubbingData: {
            ...state.proposalHeader.activeScrubbingData,
            ClientScrubs: data.filteredClientScrubs
          }
        },
        activeScrubbingFilters: data.activeFilters,
        scrubbingFiltersList: [data.activeFilters]
      };

    case RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST:
      return {
        ...state,
        scrubbingFiltersList: []
      };

    case SAVE_NEW_CLIENT_SCRUBS:
      return {
        ...state,
        proposalHeader: {
          ...state.proposalHeader,
          activeScrubbingData: payload.Data,
          scrubbingData: payload.FullData
        }
      };

    default:
      return state;
  }
}

const getPostClientScrubbing = params => ({
  type: LOAD_POST_CLIENT_SCRUBBING.request,
  payload: params
});

const getScrubbingDataFiltered = query => ({
  type: FILTERED_SCRUBBING_DATA.request,
  payload: query
});

const clearScrubbingFiltersList = () => ({
  type: REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  payload: {}
});

const overrideStatus = params => ({
  type: POST_OVERRIDE_STATUS.request,
  payload: params
});

const swapProposalDetail = params => ({
  type: SWAP_PROPOSAL_DETAIL.request,
  payload: params
});

const undoScrubStatus = (proposalId, scrubIds) => ({
  type: UNDO_SCRUB_STATUS.request,
  payload: {
    ProposalId: proposalId,
    ScrubIds: scrubIds
  }
});

const saveActiveScrubData = (newData, fullList) => ({
  type: SAVE_NEW_CLIENT_SCRUBS,
  payload: { Data: newData, FullData: fullList }
});

const clearFilteredScrubbingData = () => ({
  type: CLEAR_FILTERED_SCRUBBING_DATA,
  payload: {}
});

const getClearScrubbingDataFiltered = () => ({
  type: REQUEST_CLEAR_FILTERED_SCRUBBING_DATA
});

const reveiveFilteredScrubbingData = data => ({
  type: FILTERED_SCRUBBING_DATA.success,
  data
});

export const actions = {
  reveiveFilteredScrubbingData,
  getClearScrubbingDataFiltered,
  clearFilteredScrubbingData,
  saveActiveScrubData,
  undoScrubStatus,
  swapProposalDetail,
  overrideStatus,
  clearScrubbingFiltersList,
  getScrubbingDataFiltered,
  getPostClientScrubbing
};
