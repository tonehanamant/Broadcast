import axios from "axios";

const createApiBase = () => __API__;

const instance = axios.create({
  baseURL: createApiBase()
});

const GET = instance.get;
const POST = instance.post;
const PUT = instance.put;
const DELETE = instance.delete;

export { GET, POST, PUT, DELETE, createApiBase };
