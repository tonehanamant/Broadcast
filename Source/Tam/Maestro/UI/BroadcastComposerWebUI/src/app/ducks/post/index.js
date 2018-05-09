// Actions
import * as ACTIONS from './actionTypes.js';
import { getDay } from '../../utils/dateFormatter';

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
      DayOfWeek: {
        filterDisplay: 'Days',
        filterKey: 'DayOfWeek',
        type: 'filterList',
        hasMatchSpec: false,
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
        hasMatchSpec: false,
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
        const dayOfWeekOptions = [];
        const genreOptions = [];
        const programOptions = [];
        filtersData.DistinctDayOfWeek.forEach((item) => {
          const display = getDay(item);
          const ret = { Value: item, Selected: true, Display: display };
          dayOfWeekOptions.push(ret);
        });
        filtersData.DistinctGenres.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          genreOptions.push(ret);
        });
        filtersData.DistinctPrograms.forEach((item) => {
          const ret = { Value: item, Selected: true, Display: item };
          programOptions.push(ret);
        });
        activeFilters.DayOfWeek.filterOptions = dayOfWeekOptions;
        activeFilters.GenreName.filterOptions = genreOptions;
        activeFilters.ProgramName.filterOptions = programOptions;
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
