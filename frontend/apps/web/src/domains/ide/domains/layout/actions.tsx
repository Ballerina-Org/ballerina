/** @jsxImportSource @emotion/react */
import React from "react";
import {CheckCircle, Play} from "lucide-react";
import {style} from "./actions.styled";

export const Actions: React.FC<{
    onRun: () => void;
    onSave: () => void;
}> = ({
          onRun,
          onSave,
      }) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
            <button css={style.iconButton} onClick={onRun}>
                <Play size={18} /> Run
            </button>
            <button css={style.iconButton} onClick={onSave}>
                <CheckCircle size={18}  /> Save
            </button>
        </div>
    </div>
);
