import { all } from "redux-saga/effects";
import createMiddlewareSaga from "redux-saga";

import { watchers as app } from "Main";
import { watchers as postPrePosting } from "PostPrePosting";
import { watchers as post } from "Post";
import { watchers as tracker } from "Tracker";
import { watchers as planning } from "Sagas";

console.log(tracker);
console.log(planning);
const transform = watchers => watchers.map(watcher => watcher());

export const middlewareSaga =
  typeof createMiddlewareSaga === "function"
    ? createMiddlewareSaga()
    : createMiddlewareSaga.default();

export function* rootSaga() {
  yield all([
    ...transform([...app, ...postPrePosting, ...post, ...tracker, ...planning])
  ]);
}
