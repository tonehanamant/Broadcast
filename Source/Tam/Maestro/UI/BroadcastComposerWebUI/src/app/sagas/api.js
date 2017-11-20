import axios from 'axios';
import { call } from 'redux-saga/effects';

// Methods
const GET = axios.get;
const POST = axios.post;
const PUT = axios.put;
const DELETE = axios.delete;

// Bases
/* eslint-disable no-undef */
const apiBase = __API__;

// Requests
// call(METHOD, ...args)
const app = {
  getEnvironment: () => (
    call(GET, `${apiBase}environment`, {})
  ),
  getEmployee: () => (
    call(GET, `${apiBase}employee`, {})
  ),
};

const post = {
  getPostInitialData: () => (
    call(GET, `${apiBase}Post/InitialData`, {})
  ),
  getPosts: () => (
    call(GET, `${apiBase}Post`, {})
  ),
  getPost: id => (
    call(GET, `${apiBase}Post/${id}`, {})
  ),
  uploadPost: params => (
    call(POST, `${apiBase}Post`, params)
  ),
  savePost: params => (
    call(PUT, `${apiBase}Post`, params)
  ),
  deletePost: id => (
    call(DELETE, `${apiBase}Post/${id}`, {})
  ),
};


// const post = {
// };

// Calls
const api = {
  app,
  post,
};

export default api;
