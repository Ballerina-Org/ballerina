/** @jsxImportSource @emotion/react */
import React from "react";
import {CheckCircle, Play, SaveIcon} from "lucide-react";
import {style} from "./actions.styled";

export const Actions: React.FC<{
    onRun?: () => void;
    onSave: () => void;
}> = ({
          onRun,
          onSave,
      }) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
            <button css={style.iconButton} onClick={onSave}>
                <SaveIcon size={14}  />
            </button>
          {onRun && <button css={style.iconButton} onClick={onRun}>
                <Play size={14} />
            </button>}
        </div>
    </div>
);
