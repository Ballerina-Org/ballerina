/** @jsxImportSource @emotion/react */
import { css } from "@emotion/react";
import {Server, User, Loader, Edit} from "lucide-react";
import { style } from "./messages.styled.ts";
import {SpecEditorIndicator, SpecRunnerIndicator} from "playground-core";

export const Messages: React.FC<{
  editorIndicator: SpecEditorIndicator;
  clientErrors?: string[];
  serverErrors?: string[];
  clientSuccess?: string[];
  serverSuccess?: string[];
  runnerIndicator: SpecRunnerIndicator;
}> =

 ({
        editorIndicator,
        clientErrors = [],
        serverErrors = [],
        clientSuccess = [],
        serverSuccess = [],
        runnerIndicator
      }) =>

 { 

   return (
  <div className="w-full">
    {clientErrors.map((msg, i) => (
      <div role="alert" className="alert alert-error">
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 shrink-0 stroke-current" fill="none"
             viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"/>
        </svg>
        <span>{i+1}: {msg}</span>
      </div>
    ))}

  </div>
)
 }