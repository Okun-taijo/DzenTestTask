import { useEffect, useState, useCallback } from "react";
import { getCommentsGraphQL } from "../api/graphqlApi";
import CommentForm from "./CommentForm";
import DOMPurify from "dompurify";
import SearchBox from "./SearchBox";
import { useSignalR } from "../services/useSignalR";
import { API_BASE_URL } from "../config";

export default function CommentList() {
  const [comments, setComments] = useState([]);
  const [page, setPage] = useState(1);

  const [sortBy, setSortBy] = useState("date");
  const [asc, setAsc] = useState(false);

  const [replyTo, setReplyTo] = useState(null);
  const [lightbox, setLightbox] = useState(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState([]);

  const sanitize = (html) =>
    DOMPurify.sanitize(html, {
      ALLOWED_TAGS: ["a", "code", "i", "strong", "br", "p", "mark"],
      ALLOWED_ATTR: ["href", "title"],
    });


  const getFileUrl = (path) => {
    if (!path) return "";
    if (path.startsWith("http")) return path;
    const normalizedPath = path.replaceAll("\\", "/").replace(/^\/+/, "");
    const relativePath = normalizedPath.startsWith("uploads/")
      ? normalizedPath
      : `uploads/${normalizedPath}`;
    return `${API_BASE_URL}/${relativePath}`;
  };


  const normalizeComments = (data) => {
    const raw = Array.isArray(data) ? data : data?.items;
    return Array.isArray(raw) ? raw : [];
  };

  const load = async () => {
    const data = await getCommentsGraphQL({
      page,
      pageSize: 25,
      sortBy,
      asc,
    });
    setComments(normalizeComments(data));
  };

  useEffect(() => {
    load();
  }, [page, sortBy, asc]);


  const handleNewComment = useCallback((payload) => {
    const comment = payload?.arguments?.[0] ?? payload;

    if (!comment?.id) return;

    setComments((prev) => {
      const safePrev = Array.isArray(prev) ? prev : [];

      if (!comment.parentId) {
        if (safePrev.some((c) => c.id === comment.id)) return safePrev;
        return [comment, ...safePrev];
      }

      const insertReply = (list) =>
        list.map((c) => {
          if (c.id === comment.parentId) {
            return {
              ...c,
              replies: [comment, ...(c.replies || [])],
            };
          }

          if (c.replies?.length) {
            return {
              ...c,
              replies: insertReply(c.replies),
            };
          }

          return c;
        });

      return insertReply(safePrev);
    });
  }, []);

  useSignalR(handleNewComment);

  const handleSearchResults = useCallback((query, results) => {
    setSearchQuery(query);
    setSearchResults(Array.isArray(results) ? results : []);
  }, []);

  const getHomePageUrl = (homePage) => {
    if (!homePage) return "";
    return /^https?:\/\//i.test(homePage) ? homePage : `https://${homePage}`;
  };

  const renderComments = (list, depth = 0) =>
    (Array.isArray(list) ? list : []).map((c) => (
      <div
        key={c.id}
        className="comment-card"
        style={{ marginLeft: depth * 20 }}
      >
        <div className="comment-header">
          <div>
            <b>{c.userName}</b>
            {c.email ? <span className="author-email">{c.email}</span> : null}
            {c.homePage ? (
              <a
                className="author-homepage"
                href={getHomePageUrl(c.homePage)}
                target="_blank"
                rel="noreferrer"
              >
                {c.homePage}
              </a>
            ) : null}
          </div>
          <small>{new Date(c.createdAt).toLocaleString()}</small>
        </div>

        <div
          className="comment-content"
          dangerouslySetInnerHTML={{ __html: sanitize(c.text) }}
        />

        {c.attachments?.length > 0 && (
          <div className="attachment-row">
            {c.attachments.map((f) =>
              f.isImage ? (
                <img
                  key={f.id}
                  src={getFileUrl(f.path)}
                  alt=""
                  onClick={() => setLightbox(getFileUrl(f.path))}
                  className="attachment-thumb"
                />
              ) : (
                <a
                  key={f.id}
                  href={getFileUrl(f.path)}
                  target="_blank"
                  rel="noreferrer"
                  className="attachment-link"
                >
                  {f.fileName}
                </a>
              )
            )}
          </div>
        )}

        <button className="secondary" onClick={() => setReplyTo(c.id)}>
          Reply
        </button>

        {replyTo === c.id && (
          <CommentForm
            parentId={c.id}
            onSuccess={() => {
              setReplyTo(null);
            }}
          />
        )}

        {c.replies?.length > 0 && (
          <div className="nested">{renderComments(c.replies, depth + 1)}</div>
        )}
      </div>
    ));

  const renderSearchResults = (list) =>
    (Array.isArray(list) ? list : []).map((c) => (
      <div key={c.id} className="comment-card">
        <div className="comment-header">
          <div>
            <b>{c.userName || "Unknown user"}</b>
            {c.email ? <span className="author-email">{c.email}</span> : null}
            {c.homePage ? (
              <a
                className="author-homepage"
                href={getHomePageUrl(c.homePage)}
                target="_blank"
                rel="noreferrer"
              >
                {c.homePage}
              </a>
            ) : null}
          </div>
          {c.createdAt ? <small>{new Date(c.createdAt).toLocaleString()}</small> : null}
        </div>
        <div
          className="comment-content"
          dangerouslySetInnerHTML={{ __html: sanitize(c.text || "") }}
        />

        {c.attachments?.length > 0 && (
          <div className="attachment-row">
            {c.attachments.map((f) =>
              f.isImage ? (
                <img
                  key={f.id}
                  src={getFileUrl(f.path)}
                  alt=""
                  onClick={() => setLightbox(getFileUrl(f.path))}
                  className="attachment-thumb"
                />
              ) : (
                <a
                  key={f.id}
                  href={getFileUrl(f.path)}
                  target="_blank"
                  rel="noreferrer"
                  className="attachment-link"
                >
                  {f.fileName}
                </a>
              )
            )}
          </div>
        )}
      </div>
    ));

  return (
    <div className="comments">
      <h1>Comments</h1>
      <SearchBox onResultsChange={handleSearchResults} />

      <div className="controls">
        <button onClick={() => setSortBy("date")}>Sort by date</button>
        <button onClick={() => setSortBy("username")}>Sort by name</button>
        <button onClick={() => setSortBy("email")}>Sort by email</button>
        <button className="secondary" onClick={() => setAsc((p) => !p)}>
          {asc ? "ASC" : "DESC"}
        </button>
      </div>

      <CommentForm />

      {searchQuery.trim()
        ? renderSearchResults(searchResults)
        : renderComments(comments)}

      {lightbox && (
        <div
          onClick={() => setLightbox(null)}
          className="lightbox"
        >
          <img src={lightbox} />
        </div>
      )}

      <div className="pagination">
        <button
          className="secondary"
          disabled={page <= 1}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Previous
        </button>
        <span>Page {page}</span>
        <button onClick={() => setPage((p) => p + 1)}>Next</button>
      </div>
    </div>
  );
}