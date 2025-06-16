import { css } from '@emotion/react';

export const style = {
    buttonSection:
        css`
          display: flex;
          gap: 20px;
          margin-bottom: 32px;
        `,
    iconButton:
        css`
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
        `,
    layout:
        css`
        padding: 1em;
    `
}