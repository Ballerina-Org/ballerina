import { Sum } from "../../../../../main";
import { CollectionReference, EnumReference } from "../reference/state";

export type CollectionSelection<
  Element extends CollectionReference | EnumReference,
> = Sum<Element, "no selection">;
export const CollectionSelection = <
  Element extends CollectionReference | EnumReference,
>() => Sum<Element, "no selection">();
