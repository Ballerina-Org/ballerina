export type BaseLauncher = {
  name: string;
  form: string;
};

// export type CreateLauncher = {
//   kind: "create";
//   api: string;
//   configApi: string;
// } & BaseLauncher;

// export type EditLauncher = {
//   kind: "edit";
//   api: string;
//   configApi: string;
// } & BaseLauncher;

export type PassthroughLauncher = {
  kind: "passthrough";
  configType: string;
} & BaseLauncher;

export type Launcher = PassthroughLauncher;
//  | CreateLauncher | EditLauncher;
