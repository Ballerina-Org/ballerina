import React, { useEffect, useRef } from "react";
import { createRoot, Root } from "react-dom/client";
import { transform } from "sucrase";

export const LivePreview = ({ code }: { code: string }) => {
    const mountRef = useRef<HTMLDivElement>(null);
    const rootRef = useRef<Root | null>(null);

    useEffect(() => {
        if (!mountRef.current) return;

        // 1️⃣ Transpile TSX → plain JS
        let jsCode: string;
        try {
            const result = transform(code, {
                transforms: ["typescript", "jsx", "imports"], // ← ✅ ADD "imports"
            });
            jsCode = result.code;
        } catch (err) {
            mountRef.current.innerHTML = `<pre style="color:red">${err}</pre>`;
            return;
        }

        // 2️⃣ Evaluate in local scope
        try {
            const exports: any = {};
            const moduleFunc = new Function("React", "exports", jsCode);
            moduleFunc(React, exports);

            const Comp = exports.default;
            if (!Comp) {
                mountRef.current.innerHTML =
                    `<pre style="color:orange">⚠️ No default export found.</pre>`;
                return;
            }

            // 3️⃣ Create or reuse React 18 root
            if (!rootRef.current) {
                rootRef.current = createRoot(mountRef.current);
            }

            // 4️⃣ Render live component
            rootRef.current.render(<Comp />);
        } catch (err) {
            mountRef.current.innerHTML = `<pre style="color:red">${String(err)}</pre>`;
        }

        // 5️⃣ Cleanup on unmount
        return () => {
            rootRef.current?.unmount();
            rootRef.current = null;
        };
    }, [code]);

    return (
        <div
            ref={mountRef}
            className="border rounded bg-base-100 p-2 overflow-y-auto min-h-[200px]"
        />
    );
};
