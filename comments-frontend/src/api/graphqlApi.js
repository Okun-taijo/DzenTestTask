import { API_BASE_URL } from "../config";

const GRAPHQL_URL = `${API_BASE_URL}/graphql`;

export const fetchGraphQL = async (query, variables = {}) => {
  const res = await fetch(GRAPHQL_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ query, variables }),
  });

  const payload = await res.json();
  if (payload.errors?.length) {
    throw new Error(payload.errors.map((x) => x.message).join(", "));
  }

  return payload.data;
};

export const getCommentsGraphQL = async ({
  page = 1,
  pageSize = 25,
  sortBy = "date",
  asc = false,
} = {}) => {
  const query = `
    query GetComments($page: Int!, $pageSize: Int!, $sortBy: String!, $asc: Boolean!) {
      comments(page: $page, pageSize: $pageSize, sortBy: $sortBy, asc: $asc) {
        id
        userName
        email
        homePage
        text
        createdAt
        attachments {
          id
          fileName
          path
          isImage
        }
        replies {
          id
          userName
          email
          text
          createdAt
        }
      }
    }
  `;

  const data = await fetchGraphQL(query, { page, pageSize, sortBy, asc });
  return data?.comments || [];
};
