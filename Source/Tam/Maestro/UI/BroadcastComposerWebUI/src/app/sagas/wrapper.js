import { call, put } from "redux-saga/effects";
import { deployError } from "../ducks/app/index";

const defaultError = {
  error: "Error",
  message:
    "The server encountered an error processing the request. Please try again or contact your administrator to review error logs."
};

export function* errorBuilder(customError, customMessage) {
  const sagaError = customError || defaultError;
  const displayError = customMessage
    ? { ...sagaError, message: customMessage }
    : sagaError;
  yield put(deployError(displayError));
}

function* wrapper(fn, payload, actions, customError) {
  try {
    const { status, data } = yield call(fn, payload);
    if (status !== 200 || !data.Success) {
      yield call(
        errorBuilder,
        customError,
        data.Message ? data.Message : undefined
      );
    } else {
      yield put({ type: actions.success, data, payload });
    }
  } catch (e) {
    if (e.response) {
      yield call(errorBuilder, customError);
    } else if (!e.response && e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

export default function(fn, actions, customError) {
  return function* wrapSaga({ payload }) {
    yield wrapper(fn, payload, actions, customError);
  };
}
