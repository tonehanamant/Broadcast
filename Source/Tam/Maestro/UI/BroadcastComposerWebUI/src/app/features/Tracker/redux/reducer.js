import { getDay, getDateInFormat } from "Utils/dateFormatter.js";
import { types as ACTIONS } from "Tracker/redux";

const initialState = {
  loadingValidIscis: false,
  typeaheadIscisList: [],
  tracker: {},
  trackerGridData: [],
  proposalHeader: {},
  unlinkedIscisData: [],
  archivedIscisData: [],
  modals: {},
  unlinkedIscisLength: 0,
  activeIsciFilterQuery: "",
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
    case ACTIONS.LOAD_TRACKER.success:
      return {
        ...state,
        tracker: data.Data,
        unlinkedIscisLength: data.Data.UnlinkedIscis,
        trackerGridData: data.Data.Posts,
        trackerUnfilteredGridData: data.Data.Posts
      };

    case ACTIONS.RECEIVE_FILTERED_TRACKER:
      return {
        ...state,
        trackerGridData: data
      };

    case ACTIONS.RECEIVE_FILTERED_UNLINKED:
      return {
        ...state,
        unlinkedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query
      };

    case ACTIONS.RECEIVE_FILTERED_ARCHIVED:
      return {
        ...state,
        archivedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query
      };

    case ACTIONS.RECEIVE_CLEAR_ISCI_FILTER:
      return {
        ...state,
        activeIsciFilterQuery: ""
      };

    case ACTIONS.RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA: {
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

    case ACTIONS.LOAD_TRACKER_CLIENT_SCRUBBING.success: {
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

    case ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA:
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

    case ACTIONS.RECEIVE_TRACKER_OVERRIDE_STATUS:
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

    case ACTIONS.LOAD_ARCHIVED_ISCI.success:
      return {
        ...state,
        archivedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data
      };
    case ACTIONS.UNLINKED_ISCIS_DATA.success:
      return {
        ...state,
        unlinkedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data
      };

    case ACTIONS.RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST:
      return {
        ...state,
        scrubbingFiltersList: []
      };

    case ACTIONS.LOAD_VALID_ISCI.request:
      return {
        ...state,
        loadingValidIscis: true
      };

    case ACTIONS.LOAD_VALID_ISCI.success:
      return {
        ...state,
        typeaheadIscisList: data.Data,
        loadingValidIscis: false
      };

    case ACTIONS.LOAD_VALID_ISCI.failure:
      return {
        ...state,
        loadingValidIscis: false
      };

    case ACTIONS.SAVE_NEW_CLIENT_SCRUBS:
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
