import { DispatchTypeName } from "../types/state";

export type SerializedEntityApi = {
  type?: any;
  methods?: any;
};

// Exist already in the main.ts
// export type EntityApi = {
//   type: TypeName;
//   methods: { create: boolean; get: boolean; update: boolean; default: boolean };
// };
// export type GlobalConfigurationApi = {
//   type: TypeName;
//   methods: { get: boolean };
// };
