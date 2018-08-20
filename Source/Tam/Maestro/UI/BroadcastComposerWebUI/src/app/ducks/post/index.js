// Actions
import * as ACTIONS from './actionTypes.js';
import { getDay, getDateInFormat } from '../../utils/dateFormatter';

const initialState = {
  loadingValidIscis: false,
  typeaheadIscisList: [],
  post: {},
  postGridData: [],
  proposalHeader: {},
  unlinkedIscisData: [],
  archivedIscisData: [],
  modals: {},
  unlinkedIscisLength: 0,
  activeIsciFilterQuery: '',
  scrubbingFiltersList: [],
  activeScrubbingFilters: {},
  activeFilterKey: 'All', // represents global Filter state: 'All', 'InSpec', 'OutOfSpec'
  hasActiveScrubbingFilters: false, // specific filters are active
  defaultScrubbingFilters:
    {
      Affiliate: {
        filterDisplay: 'Affiliates',
        filterKey: 'Affiliate',
        distinctKey: 'DistinctAffiliates',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchAffiliate', // not currently available
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      ClientISCI: {
        filterDisplay: 'Client ISCIs',
        filterKey: 'ClientISCI',
        distinctKey: 'DistinctClientIscis',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchClientISCI', // not currently available
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      DateAired: {
        filterDisplay: 'Select Date Range',
        filterKey: 'DateAired',
        type: 'dateInput',
        // hasMatchSpec: false, // NA
        // activeMatch: false,
        active: false,
        /* matchOptions: {
          matchKey: 'MatchClientISCI', // not currently available
          inSpec: true,
          outOfSpec: true,
        }, */
        exclusions: false,
        filterOptions: [],
      },
      DayOfWeek: {
        filterDisplay: 'Days',
        filterKey: 'DayOfWeek',
        distinctKey: 'DistinctDayOfWeek',
        type: 'filterList',
        hasMatchSpec: true,
        active: false,
        activeMatch: false,
        matchOptions: {
          matchKey: 'MatchIsciDays',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      GenreName: {
        filterDisplay: 'Genres',
        filterKey: 'GenreName',
        distinctKey: 'DistinctGenres',
        type: 'filterList',
        hasMatchSpec: true,
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchGenre',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      ISCI: {
        filterDisplay: 'House ISCIs',
        filterKey: 'ISCI',
        distinctKey: 'DistinctHouseIscis',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchISCI', // not currently available
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      Market: {
        filterDisplay: 'Markets',
        filterKey: 'Market',
        distinctKey: 'DistinctMarkets',
        type: 'filterList',
        hasMatchSpec: true,
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchMarket',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      ProgramName: {
        filterDisplay: 'Programs',
        filterKey: 'ProgramName',
        distinctKey: 'DistinctPrograms',
        type: 'filterList',
        hasMatchSpec: true,
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchProgram',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      Sequence: {
        filterDisplay: 'Sequences',
        filterKey: 'Sequence',
        distinctKey: 'DistinctSequences',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: null, // NA
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      ShowTypeName: {
        filterDisplay: 'Show Types',
        filterKey: 'ShowTypeName',
        distinctKey: 'DistinctShowTypes',
        type: 'filterList',
        hasMatchSpec: true,
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchShowType',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      SpotLength: {
        filterDisplay: 'Spot Lengths',
        filterKey: 'SpotLength',
        distinctKey: 'DistinctSpotLengths',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchSpotLength',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      Station: {
        filterDisplay: 'Stations',
        filterKey: 'Station',
        distinctKey: 'DistinctStations',
        type: 'filterList',
        hasMatchSpec: true,
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: 'MatchStation',
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
      TimeAired: {
        filterDisplay: 'Select Time Range',
        filterKey: 'TimeAired',
        type: 'timeInput',
        active: false,
        exclusions: false,
        filterOptions: [],
      },
      WeekStart: {
        filterDisplay: 'Week Starts',
        filterKey: 'WeekStart',
        distinctKey: 'DistinctWeekStarts',
        type: 'filterList',
        hasMatchSpec: false, // NA
        activeMatch: false,
        active: false,
        matchOptions: {
          matchKey: null, // NA
          inSpec: true,
          outOfSpec: true,
        },
        exclusions: [],
        filterOptions: [],
      },
    },
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data, payload } = action;

  switch (type) {
    case ACTIONS.RECEIVE_POST:
      return {
        ...state,
        post: data.Data,
        unlinkedIscisLength: data.Data.UnlinkedIscis,
        postGridData: data.Data.Posts,
        postUnfilteredGridData: data.Data.Posts,
      };

    case ACTIONS.RECEIVE_FILTERED_POST:
      return {
        ...state,
        postGridData: data,
      };

    case ACTIONS.RECEIVE_FILTERED_UNLINKED:
      return {
        ...state,
        unlinkedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query,
      };

    case ACTIONS.RECEIVE_FILTERED_ARCHIVED:
      return {
        ...state,
        archivedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query,
      };

      case ACTIONS.RECEIVE_CLEAR_ISCI_FILTER:
      return {
        ...state,
        activeIsciFilterQuery: '',
      };

    case ACTIONS.RECEIVE_POST_CLIENT_SCRUBBING: {
      const filtersData = data.Data.Filters;
      const activeFilters = { ...state.defaultScrubbingFilters }; // todo seems to get mutated
      const prepareFilterOptions = () => {
        const affiliateOptions = [];
        const clientIsciOptions = [];
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
        filtersData.DistinctAffiliates.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          affiliateOptions.push(ret);
        });
        filtersData.DistinctClientIscis.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          clientIsciOptions.push(ret);
        });
        filtersData.DistinctDayOfWeek.forEach((item) => {
          const display = getDay(item);
          const ret = { Value: item, Selected: true, Display: display };
          dayOfWeekOptions.push(ret);
        });
        filtersData.DistinctGenres.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          genreOptions.push(ret);
        });
        filtersData.DistinctHouseIscis.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          houseIsciOptions.push(ret);
        });
        filtersData.DistinctMarkets.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          marketOptions.push(ret);
        });
        filtersData.DistinctPrograms.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          programOptions.push(ret);
        });
        filtersData.DistinctSequences.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          sequences.push(ret);
        });
        filtersData.DistinctShowTypes.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          showTypeOptions.push(ret);
        });
        filtersData.DistinctSpotLengths.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          spotLengthOptions.push(ret);
        });
        filtersData.DistinctStations.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          stationOptions.push(ret);
        });
        // display as formatted; values as date string
        filtersData.DistinctWeekStarts.forEach((item) => {
          const display = getDateInFormat(item);
          const ret = { Value: item, Selected: true, Display: display };
          weekStartOptions.push(ret);
        });
        activeFilters.Affiliate.filterOptions = affiliateOptions;
        activeFilters.ClientISCI.filterOptions = clientIsciOptions;
        activeFilters.DayOfWeek.filterOptions = dayOfWeekOptions;
        activeFilters.DateAired.filterOptions = {
            DateAiredStart: filtersData.DateAiredStart,
            DateAiredEnd: filtersData.DateAiredEnd,
            originalDateAiredStart: filtersData.DateAiredStart,
            originalDateAiredEnd: filtersData.DateAiredEnd,
        };
        activeFilters.TimeAired.filterOptions = {
          TimeAiredStart: filtersData.TimeAiredStart,
          TimeAiredEnd: filtersData.TimeAiredEnd,
          originalTimeAiredStart: filtersData.TimeAiredStart,
          originalTimeAiredEnd: filtersData.TimeAiredEnd,
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
      // console.log('after prepare filter options', activeFilters, state);
      return {
        ...state,
        activeFilterKey: data.Data.filterKey ? data.Data.filterKey : state.activeFilterKey,
        hasActiveScrubbingFilters: false,
        proposalHeader: {
          scrubbingData: data.Data,
          activeScrubbingData: data.Data,
        },
        activeScrubbingFilters: activeFilters,
        scrubbingFiltersList: [activeFilters],
      };
    }

    case ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA:
    // console.log('RECEIVE_FILTERED_SCRUBBING_DATA >>>>>>>>', data, state);
    /* return Object.assign({}, state, {
      proposalHeader: {
        ...state.proposalHeader,
        activeScrubbingData: {
          ...state.proposalHeader.activeScrubbingData,
          ClientScrubs: data.filteredClientScrubs,
        },
      },
      ...state.activeScrubbingFilters,
      activeScrubbingFilters: data.activeFilters,
      ...state.scrubbingFiltersList,
      scrubbingFiltersList: [data.activeFilters],
      ...state.hasActiveScrubbingFilters,
      hasActiveScrubbingFilters: data.hasActiveScrubbingFilters,
    }); */
    return {
      ...state,
      proposalHeader: {
        ...state.proposalHeader,
        activeScrubbingData: {
          ...state.proposalHeader.activeScrubbingData,
          ClientScrubs: data.filteredClientScrubs,
        },
      },
      activeScrubbingFilters: data.activeFilters,
      scrubbingFiltersList: [data.activeFilters],
      hasActiveScrubbingFilters: data.hasActiveScrubbingFilters,
    };

    case ACTIONS.RECEIVE_POST_OVERRIDE_STATUS:
    return {
      ...state,
      proposalHeader: {
        ...state.proposalHeader,
        scrubbingData: data.scrubbingData,
        activeScrubbingData: {
          ...state.proposalHeader.activeScrubbingData,
          ClientScrubs: data.filteredClientScrubs,
        },
      },
      activeScrubbingFilters: data.activeFilters,
      scrubbingFiltersList: [data.activeFilters],
    };

    case ACTIONS.LOAD_ARCHIVED_ISCI.success:
      return {
        ...state,
        archivedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data,
      };
    case ACTIONS.UNLINKED_ISCIS_DATA.success:
    return {
      ...state,
      unlinkedIscisData: data.Data,
      // unlinkedIscisLength: data.Data.length,
      unlinkedFilteredIscis: data.Data,
    };

    case ACTIONS.RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST:
    return {
      ...state,
      scrubbingFiltersList: [],
    };

    /* case ACTIONS.RECEIVE_SWAP_PROPOSAL_DETAIL: {
    const params = {};
    return getPostClientScrubbing(params)
    } */

    case ACTIONS.LOAD_VALID_ISCI.request:
      return {
        ...state,
        loadingValidIscis: true,
      };


    case ACTIONS.LOAD_VALID_ISCI.success:
      return {
        ...state,
        typeaheadIscisList: data.Data,
        loadingValidIscis: false,
      };

    case ACTIONS.LOAD_VALID_ISCI.failure:
      return {
        ...state,
        loadingValidIscis: false,
      };

    case ACTIONS.SAVE_NEW_CLIENT_SCRUBS:
      return {
        ...state,
        proposalHeader: {
          ...state.proposalHeader,
          activeScrubbingData: payload.Data,
          scrubbingData: payload.FullData,
        },
      };

    default:
      return state;
  }
}

// Action Creators
export const getPost = () => ({
  type: ACTIONS.REQUEST_POST,
  payload: {},
});

export const getPostFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_POST,
  payload: query,
});

export const getUnlinkedFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_UNLINKED,
  payload: query,
});

export const getArchivedFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_ARCHIVED,
  payload: query,
});

export const getPostClientScrubbing = params => ({
  type: ACTIONS.REQUEST_POST_CLIENT_SCRUBBING,
  payload: params,
});

export const getScrubbingDataFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_SCRUBBING_DATA,
  payload: query,
});

export const clearScrubbingFiltersList = () => ({
  type: ACTIONS.REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  payload: {},
});

export const getUnlinkedIscis = () => ({
  type: ACTIONS.UNLINKED_ISCIS_DATA.request,
  payload: {},
});

export const overrideStatus = params => ({
  type: ACTIONS.REQUEST_POST_OVERRIDE_STATUS,
  payload: params,
});

export const swapProposalDetail = params => ({
  type: ACTIONS.REQUEST_SWAP_PROPOSAL_DETAIL,
  payload: params,
});

export const archiveUnlinkedIscis = ids => ({
  type: ACTIONS.ARCHIVE_UNLIKED_ISCI.request,
  payload: { ids },
});

export const loadArchivedIscis = () => ({
  type: ACTIONS.LOAD_ARCHIVED_ISCI.request,
  payload: {},
});

export const loadValidIscis = query => ({
  type: ACTIONS.LOAD_VALID_ISCI.request,
  payload: { query },
});

export const mapUnlinkedIsci = payload => ({
  type: ACTIONS.MAP_UNLINKED_ISCI.request,
  payload,
});

export const undoArchivedIscis = ids => ({
  type: ACTIONS.UNDO_ARCHIVED_ISCI.request,
  payload: { ids },
});

// toggle unlinked tabs
const tabsMap = {
  unlinked: getUnlinkedIscis,
  archived: loadArchivedIscis,
};

export const toggleUnlinkedTab = (tab) => {
  const tabFunction = tabsMap[tab];
  if (tabFunction) {
    return tabFunction();
  }
  console.error('You should add function in the tabsMap to load your tab values');
  return undefined;
};

export const rescrubUnlinkedIscis = isci => ({
  type: ACTIONS.RESCRUB_UNLIKED_ISCI.request,
  payload: { isci },
});

export const closeUnlinkedIsciModal = modalPrams => ({
  type: ACTIONS.CLOSE_UNLINKED_ISCI_MODAL,
  payload: { modalPrams },
});


export const undoScrubStatus = (proposalId, scrubIds) => ({
  type: ACTIONS.UNDO_SCRUB_STATUS.request,
  payload: {
    ProposalId: proposalId,
    ScrubIds: scrubIds,
  },
});

export const saveActiveScrubData = (newData, fullList) => ({
  type: ACTIONS.SAVE_NEW_CLIENT_SCRUBS,
  payload: { Data: newData, FullData: fullList },
});
