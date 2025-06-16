import { css } from '@emotion/react';

export const style = {
    messageLayout:
        css`
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 24px;
        `,
    messageBox: (variant: "error" | "success") =>
        css`
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
        `,
}