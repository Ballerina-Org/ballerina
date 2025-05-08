import React from "react";
import ReactDOM from "react-dom/client";
import App from "./src/App.tsx";
import "./src/index.css";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <App app="dispatcher-forms-tables" />
  </React.StrictMode>,
);
