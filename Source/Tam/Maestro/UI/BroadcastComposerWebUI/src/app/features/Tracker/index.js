import { combineReducers } from "react-redux/node_modules/redux";
import {
  reducer as unlinkedIsciReducer,
  actions as unlinkedIsciActions,
  watchers as unlinkedIsciWathcers
} from "./sub-features/UnlinkedIsci/redux";
import {
  reducer as scrubbingReducer,
  actions as scrubbingActions,
  watchers as scrubbingWathcers
} from "./sub-features/Scrubbing/redux";
import {
  reducer as trackersMasterReducer,
  watchers as trackersMasterWatchers,
  actions as trackerActions
} from "./redux";

export const watchers = [
  ...trackersMasterWatchers,
  ...unlinkedIsciWathcers,
  ...scrubbingWathcers
];

export const reducer = combineReducers({
  master: trackersMasterReducer,
  scrubbing: scrubbingReducer,
  unlinkedIsci: unlinkedIsciReducer
});

export { trackerActions, unlinkedIsciActions, scrubbingActions };
