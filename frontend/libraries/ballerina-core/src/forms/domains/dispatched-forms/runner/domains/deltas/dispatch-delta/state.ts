import { List } from "immutable";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { PredicateValue } from "../../../../../parser/domains/predicates/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export type DispatchDelta<T = Unit> =
  | DispatchDeltaPrimitive<T>
  | DispatchDeltaOption<T>
  | DispatchDeltaSum<T>
  | DispatchDeltaList<T>
  | DispatchDeltaSet<T>
  | DispatchDeltaMap<T>
  | DispatchDeltaRecord<T>
  | DispatchDeltaUnion<T>
  | DispatchDeltaTuple<T>
  | DispatchDeltaCustom<T>
  | DispatchDeltaUnit<T>
  | DispatchDeltaTable<T>
  | DispatchDeltaOne<T>;

export type DispatchDeltaPrimitive<T = Unit> =
  | {
      kind: "NumberReplace";
      replace: number;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "StringReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "BoolReplace";
      replace: boolean;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TimeReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "GuidReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };

export type DispatchDeltaUnit<T = Unit> = {
  kind: "UnitReplace";
  replace: PredicateValue;
  state: any;
  type: DispatchParsedType<any>;
  flags: T | undefined;
  sourceAncestorLookupTypeNames: string[];
};
export type DispatchDeltaOption<T = Unit> =
  | {
      kind: "OptionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "OptionValue";
      value: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaSum<T = Unit> =
  | {
      kind: "SumReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "SumLeft";
      value: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "SumRight";
      value: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaList<T = Unit> =
  | {
      kind: "ArrayReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayValue";
      value: [number, DispatchDelta<T>];
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayValueAll";
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayAddAt";
      value: [number, PredicateValue];
      elementState: any;
      elementType: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayRemoveAt";
      index: number;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayRemoveAll";
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayMoveFromTo";
      from: number;
      to: number;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "ArrayDuplicateAt";
      index: number;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaSet<T = Unit> =
  | {
      kind: "SetReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "SetValue";
      value: [PredicateValue, DispatchDelta<T>];
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "SetAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "SetRemove";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaMap<T = Unit> =
  | {
      kind: "MapReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "MapKey";
      value: [number, DispatchDelta<T>];
      ballerinaValue: {
        oldKey: PredicateValue;
        newKey: PredicateValue;
      };
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "MapValue";
      value: [number, DispatchDelta<T>];
      ballerinaValue: {
        key: PredicateValue;
        value: PredicateValue;
      };
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "MapAdd";
      keyValue: [PredicateValue, PredicateValue];
      keyState: any;
      keyType: DispatchParsedType<any>;
      valueState: any;
      valueType: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "MapRemove";
      index: number;
      ballerinaValue: {
        key: PredicateValue;
      };
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaRecord<T = Unit> =
  | {
      kind: "RecordReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "RecordField";
      field: [string, DispatchDelta<T>];
      recordType: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "RecordAdd";
      field: [string, PredicateValue];
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaUnion<T = Unit> =
  | {
      kind: "UnionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "UnionCase";
      caseName: [string, DispatchDelta<T>];
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaTuple<T = Unit> =
  | {
      kind: "TupleReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TupleCase";
      item: [number, DispatchDelta<T>];
      tupleType: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaTable<T = Unit> =
  | {
      kind: "TableValue";
      id: string;
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TableValueAll";
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      // incoming-only delta carrying full row payload
      kind: "TableAdd";
      value: PredicateValue;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      // incoming-only delta carrying full row payloads
      kind: "TableAddBatch";
      values: List<PredicateValue>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      // outgoing-only delta carrying number of rows to add
      kind: "TableAddBatchEmpty";
      count: number;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
      uniqueTableIdentifier: string;
    }
  | {
      // outgoing-only delta carrying row ids to remove
      kind: "TableRemoveBatch";
      ids: string[];
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
      uniqueTableIdentifier: string;
    }
  | {
      kind: "TableAddEmpty";
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
      // needed to create a different comparand
      // so that each operation of this kind can be uniquely identified
      newIndex: number;
      uniqueTableIdentifier: string;
    }
  | {
      kind: "TableRemove";
      id: string;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
      uniqueTableIdentifier: string;
    }
  | {
      kind: "TableRemoveAll";
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TableMoveTo";
      id: string;
      to: string;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TableDuplicate";
      id: string;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "TableActionOnAll";
      filtersAndSorting: string;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };
export type DispatchDeltaOne<T = Unit> =
  | {
      kind: "OneReplace";
      replace: PredicateValue;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "OneValue";
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "OneCreateValue";
      value: PredicateValue;
      type: DispatchParsedType<any>;
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    }
  | {
      kind: "OneDeleteValue";
      flags: T | undefined;
      sourceAncestorLookupTypeNames: string[];
    };

export type DispatchDeltaCustom<T = Unit> = {
  kind: "CustomDelta";
  value: {
    kind: string;
    [key: string]: any;
  };
  flags: T | undefined;
  sourceAncestorLookupTypeNames: string[];
};
