import { call } from "redux-saga/effects";
import { GET, POST } from "./config";

const api = {
  getInitialData: () => call(GET, "InventoryCards/InitialData"),
  loadCards: params => call(POST, "InventoryCards/Cards", params),
  filterCards: params => call(POST, "InventoryCards/Cards", params)
};

export default api;
