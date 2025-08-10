import { API_URL } from "../constants";
import { errorAlert } from "./toaster";

const HEADERS = { "Content-Type": "application/json" };

export const post = async (url: string, data: any) => {
  return fetch(`${API_URL}${url}`, {
    method: "POST",
    body: JSON.stringify(data),
    headers: HEADERS,
  })
    .then(handleResponse)
    .catch(handleError);
};

export const put = async (url: string, data?: any) => {
  return fetch(`${API_URL}${url}`, {
    method: "PUT",
    body: JSON.stringify(data),
    headers: HEADERS,
  })
    .then(handleResponse)
    .catch(handleError);
};

export const del = async (url: string, data?: any) => {
  return fetch(`${API_URL}${url}`, {
    method: "DELETE",
    body: JSON.stringify(data),
    headers: HEADERS,
  })
    .then(handleResponse)
    .catch(handleError);
};
export const get = async (url: string) => {
  return fetch(`${API_URL}${url}`, {
    method: "GET",
    headers: HEADERS,
  })
    .then((res) => handleResponse(res))
    .catch(handleError);
};

export const handleResponse = async (response: Response) => {
  const data: any = await new Promise((resolve) => {
    if (response) {
      return response
        .json()
        .then((json) => resolve(json))
        .catch(() => resolve(null));
    } else {
      return resolve(null);
    }
  });
  if (response?.ok) {
    return await data;
  }
  if (data?.errors?.length > 0) {
    data.errors.forEach((err: any) => {
      errorAlert(`${err.property} ${err.message}`);
    });
  } else errorAlert(data.message);
  return errorAlert(data?.message || "An error occurred.");
};

export const handleError = (err: Error) => {
  throw err;
};
