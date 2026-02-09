module Ballerina.Forms.Tests.BridgeV1ToV2Tests

open NUnit.Framework
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.FormBuilder.Compiler
open Ballerina.DSL.Next.StdLib.Extensions
open System.Text.Json
open Ballerina.DSL.FormBuilder.Model.FormAST
open Ballerina.DSL.FormBuilder.V2ToV1Bridge.ToV1JSON
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Runners.Project

[<Test>]
let ``Compiled forms from types and forms strings transform correctly`` () =
  let typesString =
    """
type AUnion =
  | X of ()
  | AB of ()

in type ARecord = {
  X: AUnion;
}

in type BUnion =
  | X of ()
  | Y of ARecord

in type BRecord = {
  Y: BUnion;
}

in type CUnion =
  | X of ()
  | Z of BRecord

in type CRecord = {
  Z: CUnion;
}

in type StringRecord = {
  T: string;
}

in type DRecord = {
  J: CRecord;
  K: CRecord;
  A: string;
  B: float32;
  C: List[guid];
  D: () + string;
  E: string * float32 * ();
  F: StringRecord;
  G: AUnion;
  H: StringRecord;
  I: AUnion;
}

in ()"""

  let formsString =
    """
entrypoint view aStringRecord : StringRecord {
  T string(print);
  tab main with
    column main with
      group main with T
}

 view bStringRecord: StringRecord {
  T string(print);
  tab main with
    column main with
      group main with T
 }

 view aRecord: ARecord {
  X union(readonlyUnion) with
    | X -> unit(unit)
    | AB -> unit(unit);
  tab main with
    column main with
      group main with X
 }

 view bRecord: BRecord {
  Y union(readonlyUnion) with
    | X -> unit(unit)
    | Y -> view(aRecord);
  tab main with
    column main with
      group main with Y
 }

 view cRecord: CRecord {
  Z union(readonlyUnion) with
    | X -> unit(unit)
    | Z -> view(bRecord);
  tab main with
    column main with
      group main with Z
 }

 entrypoint view dRecord : DRecord {
  A string(print);
  B float32(readonlyFloat32);
  C list(list2) guid(readonlyGuid);
  D sum(readonlySum) with
    | 1Of2 -> unit(unitEmptyString)
    | 2Of2 -> string(print);
  E tuple(valueWithMetadataContainer3)(
    string(print),
    float32(readonlyFloat32),
    unit(unitEmptyString)
  );
  F view(aStringRecord);
  G union(readonlyUnion) with
    | X -> unit(unitEmptyString)
    | AB -> unit(unit);
  H record(containerRecord){
    T string(print);
    tab main with
      column main with
        group main with T
  };
  I union(readonlyUnion) with
    | X -> unit(unitEmptyString)
    | AB -> unit(unit);
  J view(cRecord);
  K view(cRecord);
  tab main with
    column main with
      group main with A, B, C, D, E, F, G, H, I, J, K
}"""

  let compilerInput: FormCompiler.FormCompilerInput<Ballerina.DSL.Next.StdLib.Extensions.ValueExt> =
    { Types =
        { Preludes = NonEmptyList.One(typesString)
          Source = "test.types" }
      ApiTypes = Map.empty
      Forms =
        { Program = formsString
          Source = "test.forms" } }

  let languageContext = stdExtensions |> snd

  let cache =
    memcache (languageContext.TypeCheckContext, languageContext.TypeCheckState)

  match FormCompiler.compileForms compilerInput cache languageContext (stdExtensions |> fst) with
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
    "StringRecord": {
      "fields": {
        "T": "string"
      }
    },
    "AUnion": {
      "fun": "Union",
      "args": [
        {
          "caseName": "AB",
          "fields": {}
        },
        {
          "caseName": "X",
          "fields": {}
        }
      ]
    },
    "ARecord": {
      "fields": {
        "X": "AUnion"
      }
    },
    "BUnion": {
      "fun": "Union",
      "args": [
        {
          "caseName": "X",
          "fields": {}
        },
        {
          "caseName": "Y",
          "fields": "ARecord"
        }
      ]
    },
    "BRecord": {
      "fields": {
        "Y": "BUnion"
      }
    },
    "CUnion": {
      "fun": "Union",
      "args": [
        {
          "caseName": "X",
          "fields": {}
        },
        {
          "caseName": "Z",
          "fields": "BRecord"
        }
      ]
    },
    "CRecord": {
      "fields": {
        "Z": "CUnion"
      }
    },
    "DRecord": {
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
        "F": "StringRecord",
        "G": "AUnion",
        "H": {
          "fields": {
            "T": "string"
          }
        },
        "I": "AUnion",
        "J": "CRecord",
        "K": "CRecord"
      }
    }
  },
  "apis": {
    "entities": {},
    "streams": {},
    "enums": {},
    "tables": {},
    "lookups": {}
  },
  "forms": {
    "aStringRecord": {
      "type": "StringRecord",
      "fields": {
        "T": {
          "type": "T",
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
                  "T"
                ]
              }
            }
          }
        }
      }
    },
    "bStringRecord": {
      "type": "StringRecord",
      "fields": {
        "T": {
          "type": "T",
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
                  "T"
                ]
              }
            }
          }
        }
      }
    },
    "aRecord": {
      "type": "ARecord",
      "fields": {
        "X": {
          "type": "X",
          "renderer": {
            "renderer": "readonlyUnion",
            "cases": {
              "AB": {
                "renderer": "unit"
              },
              "X": {
                "renderer": "unit"
              }
            },
            "type": "AUnion"
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
                  "X"
                ]
              }
            }
          }
        }
      }
    },
    "bRecord": {
      "type": "BRecord",
      "fields": {
        "Y": {
          "type": "Y",
          "renderer": {
            "renderer": "readonlyUnion",
            "cases": {
              "X": {
                "renderer": "unit"
              },
              "Y": {
                "renderer": "aRecord"
              }
            },
            "type": "BUnion"
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
                  "Y"
                ]
              }
            }
          }
        }
      }
    },
    "cRecord": {
      "type": "CRecord",
      "fields": {
        "Z": {
          "type": "Z",
          "renderer": {
            "renderer": "readonlyUnion",
            "cases": {
              "X": {
                "renderer": "unit"
              },
              "Z": {
                "renderer": "bRecord"
              }
            },
            "type": "CUnion"
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
                  "Z"
                ]
              }
            }
          }
        }
      }
    },
    "dRecord": {
      "type": "DRecord",
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
          "renderer": "list2",
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
          "renderer": "aStringRecord"
        },
        "G": {
          "type": "G",
          "renderer": {
            "renderer": "readonlyUnion",
            "cases": {
              "AB": {
                "renderer": "unit"
              },
              "X": {
                "renderer": "unitEmptyString"
              }
            },
            "type": "AUnion"
          }
        },
        "H": {
          "type": "H",
          "renderer": {
            "type": "StringRecord",
            "renderer": "containerRecord",
            "fields": {
              "T": {
                "type": "T",
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
                        "T"
                      ]
                    }
                  }
                }
              }
            }
          }
        },
        "I": {
          "type": "I",
          "renderer": {
            "renderer": "readonlyUnion",
            "cases": {
              "AB": {
                "renderer": "unit"
              },
              "X": {
                "renderer": "unitEmptyString"
              }
            },
            "type": "AUnion"
          }
        },
        "J": {
          "type": "J",
          "renderer": "cRecord"
        },
        "K": {
          "type": "K",
          "renderer": "cRecord"
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
                  "F",
                  "G",
                  "H",
                  "I",
                  "J",
                  "K"
                ]
              }
            }
          }
        }
      }
    }
  },
  "launchers": {
    "aStringRecord": {
      "kind": "passthrough",
      "form": "aStringRecord",
      "configType": "EmptyConfig"
    },
    "dRecord": {
      "kind": "passthrough",
      "form": "dRecord",
      "configType": "EmptyConfig"
    }
  }
}"""

    Assert.That(jsonString, Is.EqualTo(expectedJson))
