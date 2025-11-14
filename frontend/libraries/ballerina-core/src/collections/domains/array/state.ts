import { BasicFun, Fun } from "../../../fun/state";

export const ArrayRepo = {
  Operations: {
    map: <a, b>(f: BasicFun<a, b>): Fun<Array<a>, Array<b>> =>
      Fun((_) => _.map(f)),
  },
};

declare global {
  interface Array<T> {
    random(): T;
  }
}
Array.prototype.random = function <a>(this: Array<a>): a {
  return this[Math.floor(Math.random() * this.length)];
};

