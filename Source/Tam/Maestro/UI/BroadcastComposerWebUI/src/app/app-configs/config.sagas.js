import { all } from "redux-saga/effects";

import { watchers as app } from "Main";
import { watchers as postPrePosting } from "PostPrePosting";
import { watchers as post } from "Post";

const transform = watchers => watchers.map(watcher => watcher());

export function* rootSaga() {
  yield all([...transform([...app, ...postPrePosting, ...post])]);
}
