import { combineReducers } from "redux";
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
  reducer as postsMasterReducer,
  watchers as postsMasterWatchers,
  actions as postActions
} from "./redux";

export const watchers = [
  ...postsMasterWatchers,
  ...unlinkedIsciWathcers,
  ...scrubbingWathcers
];

export const reducer = combineReducers({
  master: postsMasterReducer,
  scrubbing: scrubbingReducer,
  unlinkedIsci: unlinkedIsciReducer
});

export { postActions, unlinkedIsciActions, scrubbingActions };
