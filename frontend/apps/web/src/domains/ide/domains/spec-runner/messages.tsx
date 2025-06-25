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
}> = ({
        editorIndicator,
        clientErrors = [],
        serverErrors = [],
        clientSuccess = [],
        serverSuccess = [],
        runnerIndicator
      }) => (<div css={style.messageLayout}>
    <div
        css={css`
    display: flex;
    flex-direction: column;
    align-items: flex-end;  // right-align all children in this column
  `}
    >
      {editorIndicator.kind == "editing" && <p>⚠️ Editor is dirty</p>}
        <p>{runnerIndicator.kind}</p>
        {clientErrors.map((msg, i) => (
            <div key={`client-error-${i}`} css={style.messageBox("error")}>
                <User size={60} />
                <span>{msg}</span>
            </div>
        ))}
        {serverErrors.map((msg, i) => (
            <div key={`server-error-${i}`} css={style.messageBox("error")}>
                <Server size={20} />
                <span>{msg}</span>
            </div>
        ))}
    </div>
    <div>
        {clientSuccess.map((msg, i) => (
            <div key={`client-success-${i}`} css={style.messageBox("success")}>
                <User size={20} />
                <span>{msg}</span>
            </div>
        ))}
        {serverSuccess.map((msg, i) => (
            <div key={`server-success-${i}`} css={style.messageBox("success")}>
                <Server size={20} />
                <span>{msg}</span>
            </div>
        ))}
    </div>
</div>)