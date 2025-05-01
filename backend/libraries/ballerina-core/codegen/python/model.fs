namespace Ballerina.DSL.Codegen.Python.Generator

module Model =

  type PythonCodeGenState =
    { UsedImports: Set<string> }

    static member Updaters =
      {| UsedImports =
          fun u ->
            fun s ->
              { s with
                  UsedImports = u (s.UsedImports) } |}
