// Actions
import * as ACTIONS from './actionTypes.js';
import { getDay, getDateInFormat } from '../../utils/dateFormatter';

const initialState = {
  post: {},
  postGridData: [],
  proposalHeader: {},
  unlinkedIscis: [],
  modals: {},
  scrubbingFiltersList: [],
  activeScrubbingFilters: {},
  defaultScrubbingFilters:
    {
      Affiliate: {
        filterDisplay: 'Affiliates',
        filterKey: 'Affiliate',
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
      DayOfWeek: {
        filterDisplay: 'Days',
        filterKey: 'DayOfWeek',
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
      WeekStart: {
        filterDisplay: 'Week Starts',
        filterKey: 'WeekStart',
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
  const { type, data } = action;

  switch (type) {
    case ACTIONS.RECEIVE_POST:
      return {
        ...state,
        post: data.Data,
        postGridData: data.Data.Posts,
        postUnfilteredGridData: data.Data.Posts,
      };

    case ACTIONS.RECEIVE_FILTERED_POST:
      return {
        ...state,
        postGridData: data,
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
        proposalHeader: {
          scrubbingData: data.Data,
          activeScrubbingData: data.Data,
        },
        activeScrubbingFilters: activeFilters,
        scrubbingFiltersList: [activeFilters],
      };
    }

    case ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA:
    // console.log('RECEIVE_FILTERED_SCRUBBING_DATA >>>>>>>>', data);
    return Object.assign({}, state, {
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
    });

    case ACTIONS.RECEIVE_UNLINKED_ISCIS_DATA:
    return {
      ...state,
      unlinkedIscis: data.Data,
    };

    case ACTIONS.RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST:
    return {
      ...state,
      scrubbingFiltersList: [],
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
  type: ACTIONS.REQUEST_UNLINKED_ISCIS_DATA,
  payload: {},
});
