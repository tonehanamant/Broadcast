import { call } from "redux-saga/effects";
import { GET, PUT, POST, DELETE } from "./config";

const api = {
  getInitialData: () => call(GET, "PostPrePosting/InitialData"),
  getPosts: () => call(GET, "PostPrePosting"),
  getPost: id => call(GET, `PostPrePosting/${id}`),
  uploadPost: params => call(POST, "PostPrePosting", params),
  savePost: params => call(PUT, "PostPrePosting", params),
  deletePost: id => call(DELETE, `PostPrePosting/${id}`, {})
};

export default api;
