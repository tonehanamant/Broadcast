import { combineReducers } from "redux";
import { Reducers as gridReducers } from "react-redux-grid";
import { reducer as postPrePosting } from "PostPrePosting";
import { reducer as app } from "Main";
import { reducer as post } from "Post";
import * as oldReducers from "Ducks";

export const rootReducer = combineReducers({
  ...gridReducers,
  ...oldReducers,
  app,
  post,
  postPrePosting
});
