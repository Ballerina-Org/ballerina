module Ballerina.Forms.Tests.BridgeV1ToV2Tests

open NUnit.Framework
open Ballerina.Collections.Sum
open Ballerina.DSL.FormBuilder.Compiler
open Ballerina.DSL.Next.StdLib.Extensions
open System.Text.Json
open Ballerina.DSL.FormBuilder.Model.FormAST
open Ballerina.DSL.FormBuilder.V2ToV1Bridge.ToV1JSON

[<Test>]
let ``Compiled forms from types and forms strings transform correctly`` () =
  let typesString =
    """type BRecord = {
  Z: string;
}

in type AUnion =
  | X of ()
  | AB of BRecord

in type ARecord = {
  A: string;
  B: float32;
  C: List[guid];
  D: () + string;
  E: string * float32 * ();
  F: BRecord;
  G: AUnion;
  H: BRecord;
}

in ()"""

  let formsString =
    """entrypoint view bRecord : BRecord {
  Z string(print);
  tab main with
    column main with
      group main with Z
}

 entrypoint view recordForm : ARecord {
  A string(print);
  B float32(readonlyFloat32);
  C list(listAsTable) guid(readonlyGuid);
  D sum(readonlySum) with
    | 1of2 -> unit(unitEmptyString)
    | 2of2 -> string(print);
  E tuple(valueWithMetadataContainer3)(
    string(print),
    float32(readonlyFloat32),
    unit(unitEmptyString)
  );
  F view(bRecord);
  G union(readonlyUnion) with
    | X -> unit(unitEmptyString)
    | AB -> view(bRecord);
  H record(containerRecord){
    Z string(print);
    tab main with
      column main with
        group main with Z
  };
  tab main with
    column main with
      group main with A, B, C, D, E, F
}"""

  let compilerInput: FormCompiler.FormCompilerInput<Ballerina.DSL.Next.StdLib.Extensions.ValueExt> =
    { Types =
        { Program = typesString
          Source = "test.types" }
      ApiTypes = Map.empty
      Forms =
        { Program = formsString
          Source = "test.forms" } }

  let languageContext = stdExtensions |> snd

  match FormCompiler.compileForms compilerInput languageContext (stdExtensions |> fst) with
  | Right errors -> Assert.Fail($"Compilation failed: {errors}")
  | Left formDefinitions ->
    let result = FormDefinitions.toV1Json formDefinitions

    let jsonOptions = JsonSerializerOptions(WriteIndented = true)
    let jsonString = JsonSerializer.Serialize(result, jsonOptions)

    let expectedJson =
      """{
  "types": {
    "EmptyConfig": {
      "fields": {}
    },
    "BRecord": {
      "fields": {
        "Z": "string"
      }
    },
    "ARecord": {
      "fields": {
        "A": "string",
        "B": "Float32",
        "C": {
          "fun": "List",
          "args": [
            "guid"
          ]
        },
        "D": {
          "fun": "Sum",
          "args": [
            "unit",
            "string"
          ]
        },
        "E": {
          "fun": "Tuple",
          "args": [
            "string",
            "Float32",
            "unit"
          ]
        },
        "F": "BRecord",
        "G": {
          "fun": "Union",
          "args": [
            {
              "caseName": "AB",
              "fields": "BRecord"
            },
            {
              "caseName": "X",
              "fields": {}
            }
          ]
        },
        "H": {
          "fields": {
            "Z": "string"
          }
        }
      }
    }
  },
  "apis": {},
  "forms": {
    "bRecord": {
      "type": "BRecord",
      "fields": {
        "Z": {
          "type": "Z",
          "renderer": "print"
        }
      },
      "disabledFields": [],
      "tabs": {
        "main": {
          "columns": {
            "main": {
              "groups": {
                "main": [
                  "Z"
                ]
              }
            }
          }
        }
      }
    },
    "recordForm": {
      "type": "ARecord",
      "fields": {
        "A": {
          "type": "A",
          "renderer": "print"
        },
        "B": {
          "type": "B",
          "renderer": "readonlyFloat32"
        },
        "C": {
          "type": "C",
          "renderer": "listAsTable",
          "elementRenderer": {
            "renderer": "readonlyGuid"
          }
        },
        "D": {
          "type": "D",
          "renderer": "readonlySum",
          "leftRenderer": {
            "renderer": "unitEmptyString"
          },
          "rightRenderer": {
            "renderer": "print"
          }
        },
        "E": {
          "type": "E",
          "renderer": "valueWithMetadataContainer3",
          "itemRenderers": [
            {
              "renderer": "print"
            },
            {
              "renderer": "readonlyFloat32"
            },
            {
              "renderer": "unitEmptyString"
            }
          ]
        },
        "F": {
          "type": "F",
          "renderer": "bRecord"
        },
        "G": {
          "type": "G",
          "renderer": "readonlyUnion",
          "cases": {
            "AB": {
              "renderer": "bRecord"
            },
            "X": {
              "renderer": "unitEmptyString"
            }
          }
        },
        "H": {
          "type": "H",
          "renderer": {
            "type": "BRecord",
            "renderer": "containerRecord",
            "fields": {
              "Z": {
                "type": "Z",
                "renderer": "print"
              }
            },
            "disabledFields": [],
            "tabs": {
              "main": {
                "columns": {
                  "main": {
                    "groups": {
                      "main": [
                        "Z"
                      ]
                    }
                  }
                }
              }
            }
          }
        }
      },
      "disabledFields": [],
      "tabs": {
        "main": {
          "columns": {
            "main": {
              "groups": {
                "main": [
                  "A",
                  "B",
                  "C",
                  "D",
                  "E",
                  "F"
                ]
              }
            }
          }
        }
      }
    }
  },
  "launchers": {
    "bRecord": {
      "kind": "passthrough",
      "form": "bRecord",
      "configType": "EmptyConfig"
    },
    "recordForm": {
      "kind": "passthrough",
      "form": "recordForm",
      "configType": "EmptyConfig"
    }
  }
}"""

    Assert.That(jsonString, Is.EqualTo(expectedJson))
