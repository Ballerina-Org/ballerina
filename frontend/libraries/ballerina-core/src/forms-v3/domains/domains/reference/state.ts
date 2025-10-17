import { simpleUpdater } from "../../../../../main";

export type CollectionReference = {
  Id: string;
  DisplayValue: string;
  source?: "enum" | "stream";
};
export const CollectionReference = {
  Default: Object.assign(
    (
      Id: string,
      DisplayValue: string,
      source?: "enum" | "stream",
    ): CollectionReference => ({
      Id,
      DisplayValue,
      source,
    }),
    {
      empty: (): CollectionReference => CollectionReference.Default("", ""),
    },
  ),
  Updaters: {
    ...simpleUpdater<CollectionReference>()("Id"),
    ...simpleUpdater<CollectionReference>()("DisplayValue"),
  },
  Operations: {
    IsCollectionReference: (value: any): value is CollectionReference => {
      return typeof value == "object" && value.Id && value.DisplayValue;
    },
  },
};

export type EnumReference = {
  Value: string;
};
export const EnumReference = {
  Default: (value: string): EnumReference => ({
    Value: value,
  }),
  Updaters: {
    ...simpleUpdater<EnumReference>()("Value"),
  },
  Operations: {
    IsEnumReference: (value: any): value is EnumReference => {
      return typeof value == "object" && value.Value;
    },
  },
};
