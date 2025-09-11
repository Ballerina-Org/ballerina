type Storage = {
    ide: {
        specName?: string;
        
    },
    test?: string;
};
const STORAGE_KEY = "ballerina" as const;

function read(): Storage {
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

function write(next: Storage) {
    if (typeof window === "undefined") return;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
}

export const test = {
    get(): string | undefined { return read().test; },
    set(v: string){     
        const s = read();
        write({ ...s, test: v } ); },
};

export const specName = {
    get(): string | undefined { return read().ide.specName; },
    set(specName: string){
        const s = read();
        write({ ide: { ...s.ide, specName } }); 
        }
};