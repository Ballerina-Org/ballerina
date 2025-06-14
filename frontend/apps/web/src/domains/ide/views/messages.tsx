/** @jsxImportSource @emotion/react */
import { css } from "@emotion/react";
import { Play, AlertTriangle, CheckCircle, Server, User, Loader } from "lucide-react";

const buttonSection = css`
  display: flex;
  gap: 20px;
  margin-bottom: 32px;
`;

const iconButton = css`
  display: flex;
  align-items: center;
  gap: 8px;
  background: #083d34;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 20px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.18s;
  &:hover {
    background: #1e40af;
  }
`;

const messageLayout = css`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
`;

const messageBox = (variant: "error" | "success") => css`
  background: ${variant === "error" ? "#fee2e2" : "#dcfce7"};
  border-right: 5px solid ${variant === "error" ? "#ef4444" : "#22c55e"};
  border-radius: 10px;
  padding: 16px 24px;
  margin-bottom: 16px;
  color: ${variant === "error" ? "#b91c1c" : "#166534"};
  display: flex;
  align-items: flex-start;
  gap: 14px;
  font-size: 1rem;
`;

const layout = css`
    padding: 1em;
`;

export const ActionsAndMessages: React.FC<{
    onRun?: () => void;
    onSave?: () => void;
    onFormat?: () => void;
    clientErrors?: string[];
    serverErrors?: string[];
    clientSuccess?: string[];
    serverSuccess?: string[];
}> = ({
          onRun,
          onSave,
          onFormat,
          clientErrors = [],
          serverErrors = [],
          clientSuccess = [],
          serverSuccess = [],
      }) => (
    <div css={layout}>
        <div css={buttonSection}>
            <button css={iconButton} onClick={onRun}>
                <Play size={18} /> Run
            </button>
            <button css={iconButton} onClick={onSave}>
                <CheckCircle size={18}  /> Save
            </button>
        </div>
        <div css={messageLayout}>
            <div
                css={css`
    display: flex;
    flex-direction: column;
    align-items: flex-end;  // right-align all children in this column
  `}
            >
                {clientErrors.map((msg, i) => (
                    <div key={`client-error-${i}`} css={messageBox("error")}>
                        <User size={60} />
                        <span>{msg}</span>
                    </div>
                ))}
                {serverErrors.map((msg, i) => (
                    <div key={`server-error-${i}`} css={messageBox("error")}>
                        <Server size={20} />
                        <span>{msg}</span>
                    </div>
                ))}
            </div>
            <div>
                {clientSuccess.map((msg, i) => (
                    <div key={`client-success-${i}`} css={messageBox("success")}>
                        <User size={20} />
                        <span>{msg}</span>
                    </div>
                ))}
                {serverSuccess.map((msg, i) => (
                    <div key={`server-success-${i}`} css={messageBox("success")}>
                        <Server size={20} />
                        <span>{msg}</span>
                    </div>
                ))}
            </div>
        </div>
    </div>
);
