import postPrePosting from "./api.postPrePosting";
import post from "./api.post";
import planning from "./api.planning";
import app from "./api.app";
import tracker from "./api.tracker";
import inventory from "./api.inventory";

export { createApiBase } from "./config";

export default {
  postPrePosting,
  post,
  planning,
  app,
  tracker,
  inventory
};
