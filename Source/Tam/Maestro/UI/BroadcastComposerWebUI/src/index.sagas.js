import { all } from "redux-saga/effects";

import { watchers as planning } from "Planning";
import { watchers as post } from "Post";
import { watchers as prePost } from "PrePost";

const transform = watchers => watchers.map(watcher => watcher());

export default function* rootSaga() {
  yield all([
    ...transform(planning),
    ...transform(post),
    ...transform(prePost)
  ]);
}
