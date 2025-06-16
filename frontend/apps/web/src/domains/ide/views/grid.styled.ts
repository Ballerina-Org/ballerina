import { css } from '@emotion/react';

export const style = {
    divider:
        css`
            flex: 1;
            display: flex;
            flex-direction: row;
            width: 100%;
            position: relative;
            z-index: 10;
            pointer-events: auto;
            min-height: 0;
        `,
    dividerLeft:
        css`
            flex: 1;
            overflow: auto;
            border-right: 1px solid #e5e7eb;
            //padding: 1rem;
            min-width: 100px;
          `,
    dividerMain:
        css`
            width: 3px;  
            min-width: 3px;
            background: lightgray;
            cursor: col-resize;
            //user-select: none;
            //background: red; /* debug */
            z-index: 10;
          `,
    header: 
        css`
          height: 100px;
          flex-shrink: 0;
          width: 100%;
          background: #f1f5f9;
          display: flex;
          align-items: center;
          justify-content: center;
          border-bottom: 1px solid #e5e7eb;
        `,
    parent:
        css`
        height: 100vh;
        display: flex;
        flex-direction: column;
        width: 100vw;
      `,
    dividerRight:
        css`
        flex: 1;
        overflow: auto;
      `
}