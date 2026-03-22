import * as signalR from "@microsoft/signalr";

export const connectRealtime = (onUpdate) => {
  const conn = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hub/dashboard")
    .withAutomaticReconnect()
    .build();

  conn.on("dashboard_update", onUpdate);
  conn.start();

  return conn;
};
