/** @jsxImportSource @emotion/react */
import "react-grid-layout/css/styles.css";
import React, {useEffect, useRef} from "react";
import { style } from "./grid.styled";
export const Grid: React.FC<{
    theme: string;
    header: React.ReactNode;
    left: React.ReactNode;
    right: React.ReactNode;
}> = ({ theme, header, left, right }) => {
    const leftRef = useRef<HTMLDivElement>(null);
    const dividerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const left = leftRef.current;
        const divider = dividerRef.current;
        if (!left || !divider) return;

        let isResizing = false;

        const handleMouseDown = () => {
            isResizing = true;
            document.body.style.cursor = "col-resize";
        };

        const handleMouseMove = (e: MouseEvent) => {
            if (!isResizing) return;
            const newWidth = e.clientX;
            left.style.width = `${newWidth}px`;
            left.style.flex = "none";
        };

        const handleMouseUp = () => {
            isResizing = false;
            document.body.style.cursor = "default";
        };

        //divider.addEventListener("mousedown", handleMouseDown);
        divider.addEventListener("mousedown", () => {
            isResizing = true;
            document.body.style.cursor = "col-resize";
        });
        document.addEventListener("mousemove", handleMouseMove);
        document.addEventListener("mouseup", handleMouseUp);

        return () => {
            divider.removeEventListener("mousedown", handleMouseDown);
            document.removeEventListener("mousemove", handleMouseMove);
            document.removeEventListener("mouseup", handleMouseUp);
        };
    }, []);

    return (
        <div css={style.parent}  data-theme={theme}>
            <div css={style.header}>{header}</div>
            <div css={style.divider}>
                <div
                    ref={leftRef}
                    css={style.dividerLeft}
                >{left}</div>
                <div
                    ref={dividerRef}
                    css={style.dividerMain}
                ></div>
                <div css={style.dividerRight}>{right}</div>
            </div>
        </div>
    );
};