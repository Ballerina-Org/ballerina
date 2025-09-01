import { css } from '@emotion/react';
export const myTheme = {
    displayName: 'myTheme',
    fragments: { edit: 'rgb(42, 161, 152)' },
    styles: {
        container: {
            backgroundColor: 'rgb(255, 242, 72)',
            fontFamily: 'monospace',
        },
        collection: {},
        collectionInner: {},
        collectionElement: {},
        dropZone: {},
        property: '#292929',
        bracket: { color: 'rgb(0, 43, 54)', fontWeight: 'bold' },
        itemCount: { color: 'rgba(0, 0, 0, 0.3)', fontStyle: 'italic' },
        string: 'rgb(203, 75, 22)',
        number: 'rgb(38, 139, 210)',
        boolean: 'green',
        null: { color: 'rgb(220, 50, 47)', fontVariant: 'small-caps', fontWeight: 'bold' },
        input: ['#292929', { fontSize: '90%' }],
        inputHighlight: '#b3d8ff',
        error: { fontSize: '0.8em', color: 'red', fontWeight: 'bold' },
        iconCollection: 'rgb(0, 43, 54)',
        iconEdit: 'edit',
        iconDelete: 'rgb(203, 75, 22)',
        iconAdd: 'edit',
        iconCopy: 'rgb(38, 139, 210)',
        iconOk: 'green',
        iconCancel: 'rgb(203, 75, 22)',
    },
}
export const style = {
    container: css`
      //display: flex;
      //flex-direction: column;
      //gap: 0.75rem;
      //padding: 1rem;
      //border: 1px solid #ccc;
      //border-radius: 0.5rem;
      ////max-width: 300px;
      //background-color: #f9f9f9;
      //font-family: sans-serif;
  `,
    row: css`
      display: flex;
      flex-direction: row;
      gap: 1rem;
      align-items: center;
  `,

    input: css`
      //padding: 0.5rem;
      //margin-left: 2em;
      //font-size: 1rem;
      //border: 1px solid #ccc;
      //border-radius: 0.25rem;
  `,
    select: css`
      //padding: 0.5rem;
      //font-size: 1rem;
      //border: 1px solid #ccc;
      //border-radius: 0.25rem;
  `,
    output: css`
      margin-top: 1rem;
      font-size: 0.95rem;
  `,

    dividerStyle: css`
  width: 3px;
  color: #000000;
  height: 50px;
  margin: 0 1rem;
`,
    /*** switch ***/
    switchWrapper: css`
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
  `,
    visuallyHidden: css`
      position: absolute;
      opacity: 0;
      width: 0;
      height: 0;
  `,
    switchTrack: css`
        width: 2.5rem;
        height: 1.25rem;
        background-color: #ccc;
        border-radius: 999px;
        position: relative;
        transition: background-color 0.2s ease;
  `,
    switchTrackChecked: css`
        background-color: #46ae80;
  `,
    switchThumb: css`
        position: absolute;
        top: 0.125rem;
        left: 0.125rem;
        width: 1rem;
        height: 1rem;
        background-color: white;
        border-radius: 999px;
        transition: transform 0.2s ease;
  `,
    switchThumbChecked: css`
        transform: translateX(1.25rem);
  `,
    labelText: css`
        font-size: 0.95rem;
  `
    ,
    /*** editor ***/
    editor: {
        container: css`
            max-width: 100%;
      //display: flex;
      //gap: 1rem;
      //padding: 1rem;
      //background-color: #f3f4f6; /* light gray */
      //border-radius: 0.5rem;
    `,
        left: css`
      flex: 1;
        max-width: 100%;
      //padding: 0.1rem;
      //background-color: #083d34;/
      border-radius: 0.5rem;
    `,
        right: css`
      flex: 1;
      //padding: 0.1rem;
      //background-color: #46ae80;
      border-radius: 0.5rem;
    `
    }
}