import axios from "axios";

/* eslint-disable no-undef */
const createApiBase = () => __API__;
/* eslint-enable no-undef */

const instance = axios.create({
  baseURL: createApiBase()
});

const GET = instance.get;
const POST = instance.post;
const PUT = instance.put;
const DELETE = instance.delete;

export { GET, POST, PUT, DELETE, createApiBase };
