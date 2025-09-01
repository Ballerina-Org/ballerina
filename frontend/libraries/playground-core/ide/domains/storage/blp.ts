type BlpStorage = {
    ide: {
        specName?: string;
    };
};

const STORAGE_KEY = "blp" as const;

function read(): BlpStorage {
    if (typeof window === "undefined") return { ide: {} };

    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return { ide: {} };
        const parsed = JSON.parse(raw);
        const ide = parsed && typeof parsed === "object" ? (parsed as any).ide ?? {} : {};
        return { ide };
    } catch {
        return { ide: {} };
    }
}

function write(next: BlpStorage) {
    if (typeof window === "undefined") return;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
}

export function getSpecName(): string | undefined {
    return read().ide.specName;
}

export function setSpecName(specName: string | undefined) {
    const blp = read();
    write({ ide: { ...blp.ide, specName } });
}