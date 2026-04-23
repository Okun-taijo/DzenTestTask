import { useEffect, useState } from "react";
import { searchComments } from "../api/searchApi";

export default function SearchBox({ onResultsChange }) {
  const [q, setQ] = useState("");
  const [results, setResults] = useState([]);

  useEffect(() => {
    const delay = setTimeout(async () => {
      if (!q.trim()) {
        setResults([]);
        onResultsChange?.(q, []);
        return;
      }

      const data = await searchComments(q);
      setResults(data);
      onResultsChange?.(q, data);
    }, 300); 

    return () => clearTimeout(delay);
  }, [q, onResultsChange]);

  const handlePick = (item) => {
    setResults([]);
    onResultsChange?.(q, [item]);
  };

  return (
    <div className="search-box">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Search comments..."
      />

      {results.length > 0 && (
        <div className="search-results">
          {results.map((r) => (
            <button
              key={r.id}
              type="button"
              className="search-item"
              onClick={() => handlePick(r)}
            >
              <b>{r.userName}</b>
              <div dangerouslySetInnerHTML={{ __html: r.text }} />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}