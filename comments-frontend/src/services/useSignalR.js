import { useEffect } from "react";
import { connection, startSignalR } from "./signalr";

export const useSignalR = (onNewComment) => {
  useEffect(() => {
    if (typeof onNewComment !== "function") return;

    let mounted = true;
    const handleNewComment = (payload) => {
      const comment = payload?.arguments?.[0] ?? payload;
      onNewComment(comment);
    };

    const init = async () => {
      // Subscribe before start to avoid missing the first event
      // in case it arrives while the connection is being established.
      connection.off("new_comment", handleNewComment);
      connection.on("new_comment", handleNewComment);

      try {
        await startSignalR();
      } catch (err) {
        console.error("SignalR init failed:", err);
      }
      if (!mounted) {
        connection.off("new_comment", handleNewComment);
      }
    };

    init();

    return () => {
      mounted = false;
      connection.off("new_comment", handleNewComment);
    };
  }, [onNewComment]);
};