export function hash64(x: string): bigint {
  let h = 0x811c9dc5n;
  for (let i = 0; i < x.length; i++)
    h = ((h ^ BigInt(x.charCodeAt(i))) * 0x1000193n) & 0xffffffffffffffffn;
  return h;
}

export function mix64(acc: bigint, x: bigint): bigint {
  acc ^= x + 0x9e3779b97f4a7c15n + (acc << 6n) + (acc >> 2n);
  return acc & 0xffffffffffffffffn;
}

export function salt(str: string): bigint {
  let h = 0x811c9dc5n;
  for (let i = 0; i < str.length; i++) {
    h ^= BigInt(str.charCodeAt(i));
    h *= 0x1000193n;
    h &= 0xffffffffffffffffn;
  }
  return h;
}

export const foldMix = (seed: bigint, parts: readonly bigint[]) =>
  parts.reduce((acc, x) => mix64(acc, x), seed);

export const mix1 = (seed: bigint, x: bigint) => mix64(seed, x);
