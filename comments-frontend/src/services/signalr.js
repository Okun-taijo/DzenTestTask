import * as signalR from "@microsoft/signalr";
import { API_BASE_URL } from "../config";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE_URL}/hubs/comments`)
  .withAutomaticReconnect()
  .build();

let startPromise = null;

export const startSignalR = async () => {
  if (connection.state === signalR.HubConnectionState.Connected) {
    return;
  }

  if (connection.state === signalR.HubConnectionState.Connecting || startPromise) {
    await startPromise;
    return;
  }

  startPromise = connection.start();
  try {
    await startPromise;
    console.log("SignalR connected");
  } finally {
    startPromise = null;
  }
};

export const ensureDisconnectedState = async () => {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    return;
  }

  if (connection.state === signalR.HubConnectionState.Connected) {
    await connection.stop();
  }
};