import { API_BASE_URL } from "../config";

export const searchComments = async (q) => {
  const res = await fetch(
    `${API_BASE_URL}/api/search?q=${encodeURIComponent(q)}`
  );

  if (!res.ok) {
    return [];
  }

  const data = await res.json();
  const items = Array.isArray(data) ? data : data?.items || [];

  return items.map((x) => ({
    id: x?.id ?? x?.Id ?? "",
    userName: x?.userName ?? x?.UserName ?? "",
    email: x?.email ?? x?.Email ?? "",
    text: x?.text ?? x?.Text ?? "",
    homePage: x?.homePage ?? x?.HomePage ?? "",
    createdAt: x?.createdAt ?? x?.CreatedAt ?? null,
    attachments: x?.attachments ?? x?.Attachments ?? [],
  })).filter((x) => x.id);
};