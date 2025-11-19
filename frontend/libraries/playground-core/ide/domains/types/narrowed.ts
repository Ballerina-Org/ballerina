export function narrowed(msg: string): never {
    throw new Error(msg);
}