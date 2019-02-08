import { call } from "redux-saga/effects";
import { GET } from "./config";

const api = {
  getEnvironment: () => call(GET, "environment"),
  getEmployee: () => call(GET, "employee")
};

export default api;
