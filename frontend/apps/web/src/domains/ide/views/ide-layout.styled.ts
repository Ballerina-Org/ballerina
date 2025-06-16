/** @jsxImportSource @emotion/react */

import { css } from '@emotion/react';

export const style = {
    stepColor: css`
        color: #46ae80; //#083d34;
    `,
    logoParent:
        css`
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
    `,
    topTabs:
        css`
      position: absolute;
      left: 50%;
      transform: translateX(-50%);
      display: flex;
      `,
    headerParent:
        css`
        position: relative;
        width: 100vw;
        display: flex;
        flex-direction: row;
        align-items: center;
        min-height: 80px;
        padding: 0 32px;
      `
}