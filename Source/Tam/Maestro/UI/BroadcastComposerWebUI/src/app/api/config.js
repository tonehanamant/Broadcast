import axios from "axios";

/* eslint-disable no-undef */
const createApiBase = () => __API__;
/* eslint-enable no-undef */

// axios defaults:
const axiosDefaults = {
  headers: { "Content-Type": "application/json" },
  withCredentials: true
};

const instance = axios.create({
  ...axiosDefaults,
  baseURL: createApiBase()
});

const GET = instance.get;
const POST = instance.post;
const PUT = instance.put;
const DELETE = instance.delete;

export { GET, POST, PUT, DELETE, createApiBase };
