/** @jsxImportSource @emotion/react */
import React from "react";
import {CheckCircle, Play, SaveIcon, Lock, Download, Plus, RefreshCw} from "lucide-react";
import {style} from "./actions.styled";

export const Actions: React.FC<{
  onReload?: () => void;
  onNew?: () => void;
    onRun?: () => void;
    onDownload?: () => void;
    onSave: () => void;
}> = ({onNew, onReload,onRun,onDownload,onSave,}) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
          <button css={style.iconButton} onClick={onReload}>
            <RefreshCw size={14}  />
          </button>
          <button css={style.iconButton} onClick={onNew}>
            <Plus size={14}  />
          </button>
            <button css={style.iconButton} onClick={onSave}>
                <SaveIcon size={14}  />
            </button>
          {onRun && <button css={style.iconButton} onClick={onRun}>
                <Play size={14} />
            </button>}

        </div>
    </div>
);
