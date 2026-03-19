namespace Codegen.Golang

type GoImport = GoImport of string

type GoCodeGenState =
  { UsedImports: Set<GoImport> }

  static member Updaters =
    {| UsedImports =
        fun u ->
          fun s ->
            { s with
                UsedImports = u (s.UsedImports) } |}
