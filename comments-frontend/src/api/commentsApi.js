import { API_BASE_URL } from "../config";

export const getComments = async (
  page = 1,
  pageSize = 25,
  sortBy = "date",
  asc = false
) => {
  const res = await fetch(
    `${API_BASE_URL}/api/comments?page=${page}&pageSize=${pageSize}&sortBy=${sortBy}&asc=${asc}`
  );

  const text = await res.text();

  try {
    return JSON.parse(text);
  } catch (e) {
    console.error("Invalid JSON from server:", text);
    return [];
  }
};

export const createComment = async (data) => {
  const res = await fetch(`${API_BASE_URL}/api/comments`, {
    method: "POST",
    body: data
  });

  const text = await res.text();

  if (!res.ok) {
    console.error("SERVER:", text); 
    throw new Error(text);
  }

  return text;
};
export const getCaptcha = async () => {
  const res = await fetch(`${API_BASE_URL}/api/captcha`);
  return await res.json();
};