namespace Ballerina.VirtualFolders

open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.Expr.Model
open Ballerina.StdLib.StringBuilder

module Mock =

  let codegenConfig: CodeGenConfig =
    { Int =
        { GeneratedTypeName = "int"
          DeltaTypeName = "int"
          RequiredImport = None
          SupportedRenderers =
            Set.ofList

              [ RendererName "number"
                RendererName "defaultNumber"
                RendererName "numberCell"
                RendererName "numberNotEditable"
                RendererName "numberWithLabel" ]
          Serialization =
            { Serializer =
                { Name = "serialization.IntSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.IntDeserializer"
                  Import = GoImport "serialization/library" } } }
      Bool =
        { GeneratedTypeName = "bool"
          DeltaTypeName = "bool"
          RequiredImport = None
          SupportedRenderers =
            Set.ofList
              [ RendererName "daisyToggle"
                RendererName "boolean"
                RendererName "defaultBoolean"
                RendererName "booleanCell"
                RendererName "booleanNotEditable"
                RendererName "booleanNotEditableCell"
                RendererName "booleanWithLabel"
                RendererName "booleanWithMenuContext"
                RendererName "readonlyBool"
                RendererName "readonlyBooleanCell"
                RendererName "booleanLabelInTooltip" ]
          Serialization =
            { Serializer =
                { Name = "serialization.BoolSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.BoolDeserializer"
                  Import = GoImport "serialization/library" } } }
      String =
        { GeneratedTypeName = "string"
          DeltaTypeName = "string"
          RequiredImport = None
          SupportedRenderers =
            Set.ofList
              [ RendererName "defaultString"
                RendererName "daisyString"
                RendererName "string"
                RendererName "error"
                RendererName "stringCell"
                RendererName "stringNotEditable"
                RendererName "stringNotEditableCell"
                RendererName "stringWithLabel"
                RendererName "print"
                RendererName "notesTitle"
                RendererName "notesDescription"
                RendererName "readonlyString"
                RendererName "stringPrint"
                RendererName "readonlyStringCell"
                RendererName "optionalReadOnlyStringWithApproval"
                RendererName "notesTitle"
                RendererName "notesDescription"
                RendererName "weekYear"
                RendererName "weekYearCell"

                ]
          Serialization =
            { Serializer =
                { Name = "serialization.StringSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.StringDeserializer"
                  Import = GoImport "serialization/library" } } }
      Date =
        { GeneratedTypeName = "time.Time"
          DeltaTypeName = "time.Time"
          RequiredImport = Some(GoImport "time")
          SupportedRenderers =
            Set.ofList
              [ RendererName "date"
                RendererName "defaultDate"
                RendererName "dateCell"
                RendererName "dateNotEditable"
                RendererName "dateNotEditableCell"
                RendererName "dateWithLabel"
                RendererName "readonlyDate"
                RendererName "readonlyDateCell" ]
          Serialization =
            { Serializer =
                { Name = "serialization.TimeSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.TimeDeserializer"
                  Import = GoImport "serialization/library" } } }
      Decimal =
        { GeneratedTypeName = "decimal.Decimal"
          DeltaTypeName = "decimal.Decimal"
          RequiredImport = Some(GoImport "somelib/decimal")
          SupportedRenderers = Set.ofList []
          Serialization =
            { Serializer =
                { Name = "serialization.DecimalSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.TimeDeserializer"
                  Import = GoImport "serialization/library" } } }
      Guid =
        { GeneratedTypeName = "string"
          DeltaTypeName = "string"
          RequiredImport = None
          SupportedRenderers =
            Set.ofList
              [ RendererName "guid"
                RendererName "readonlyGuid"
                RendererName "readonlyGuidCell" ]
          Serialization =
            { Serializer =
                { Name = "serialization.GuidSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.GuidDeserializer"
                  Import = GoImport "serialization/library" } } }
      Unit =
        { GeneratedTypeName = "struct{}"
          DeltaTypeName = "struct{}"
          RequiredImport = None
          SupportedRenderers =
            Set.ofList
              [ RendererName "unit"
                RendererName "defaultUnit"
                RendererName "unitEmptyString"
                RendererName "noTable"
                RendererName "fieldNotConfigured"
                RendererName "fillFromDBButton"
                RendererName "fillFromDBDisabledButton"
                RendererName "actionButton"
                RendererName "actionButtonDisabled"
                RendererName "missingGoodsStatusUpdateButton"
                RendererName "reloadMissingGoodsStatusDisabledButton" ]
          Serialization =
            { Serializer =
                { Name = "serialization.UnitSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.UnitDeserializer"
                  Import = GoImport "serialization/library" } } }
      Option =
        { GeneratedTypeName = "Option"
          RequiredImport = None
          DeltaTypeName = "Option"
          SupportedRenderers =
            {| Enum =
                Set.ofList
                  [ RendererName "enum"
                    RendererName "defaultEnum"
                    RendererName "daisyEnum"
                    RendererName "enumWithLabel"
                    RendererName "enumAsFailingChecksStatus"
                    RendererName "customAIModelState"
                    RendererName "customAIModelVisibility"
                    RendererName "enumCell"
                    RendererName "enumNotEditable"
                    RendererName "enumWithMenuContext"
                    RendererName "enumLabelInTooltip" ]
               Stream =
                Set.ofList
                  [ RendererName "daisyInfiniteStream"
                    RendererName "infiniteStream"
                    RendererName "defaultInfiniteStream"
                    RendererName "infiniteStreamWithLabel"
                    RendererName "infiniteStreamCell"
                    RendererName "infiniteStreamCellNotEditable"
                    RendererName "infiniteStreamNotEditable"
                    RendererName "readonlyInfiniteStream"
                    RendererName "infiniteStreamWithMenuContext" ]
               Plain =
                Set.ofList
                  [ RendererName "option"
                    RendererName "readonlyOption"
                    RendererName "optionAlwaysSome"

                    ] |}
          Serialization =
            { Serializer =
                { Name = "serialization.OptionSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.OptionDeserializer"
                  Import = GoImport "serialization/library" } } }
      Set =
        { GeneratedTypeName = "Set"
          RequiredImport = None
          DeltaTypeName = "Set"
          SupportedRenderers =
            {| Enum =
                Set.ofList
                  [ RendererName "enumMultiselect"
                    RendererName "defaultEnumMultiselect"
                    RendererName "daisyEnumMultiselect"
                    RendererName "enumMultiselectWithLabel"
                    RendererName "enumMultiselectCell"

                    ]
               Stream =
                Set.ofList
                  [ RendererName "infiniteStreamMultiselect"
                    RendererName "defaultInfiniteStreamMultiselect"
                    RendererName "infiniteStreamMultiselectWithLabel"
                    RendererName "infiniteStreamMultiselectNotEditable"

                    ] |} }
      List =
        { GeneratedTypeName = "[]"
          RequiredImport = None
          DeltaTypeName = "[]"
          SupportedRenderers =
            Set.ofList
              [ RendererName "list"
                RendererName "defaultList"
                RendererName "listNotEditable"
                RendererName "listNotEditableWithEmptyPlaceholder"
                RendererName "errorList"
                RendererName "errorListSmall"
                RendererName "cardErrorList"
                RendererName "readonlyList"
                RendererName "readonlyListWithInfoBanner"
                RendererName "failingCheckList"
                RendererName "listAsTable"
                RendererName "listAsTableOnlyBody"
                RendererName "listAsTableOnlyBodyWithPlaceholderRow"
                RendererName "accountingPositionsTemplateCell"
                RendererName "multiTaxBlockList"
                RendererName "containedListWithDividersWithLabel"
                RendererName "notesNotEditable"
                RendererName "listAsContainedTableRowsWithHeader"
                RendererName "listAsContainedTableRowsWithoutHeader"
                RendererName "listAsTableWithHeaders"
                RendererName "listAsTableWithHeadersAppendOnly"
                RendererName "listAsStringInput"
                RendererName "listNetsTabWrapper" ]
          MappingFunction = ""
          Serialization =
            { Serializer =
                { Name = "serialization.ListSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.ListDeserializer"
                  Import = GoImport "serialization/library" } } }
      Table =
        { GeneratedTypeName = "Table"
          RequiredImport = None
          DeltaTypeName = "Table"
          SupportedRenderers = Set.ofList [ RendererName "table"; RendererName "streamingTable" ]
          MappingFunction = ""
          FilteringConfig =
            { SortingTypeName = "ballerina.Sorting"
              FilteringOperators =
                { EqualsTo = "ballerina.EqualsTo"
                  NotEqualsTo = "ballerina.NotEqualsTo"
                  GreaterThan = "ballerina.GreaterThan"
                  SmallerThan = "ballerina.SmallerThan"
                  GreaterThanOrEqualsTo = "ballerina.GreaterThanOrEqualsTo"
                  SmallerThanOrEqualsTo = "ballerina.SmallerThanOrEqualsTo"
                  StartsWith = "ballerina.StartsWith"
                  Contains = "ballerina.Contains"
                  IsNull = "ballerina.IsNull "
                  IsNotNull = "ballerina.IsNotNull" } } }
      One =
        { GeneratedTypeName = "One"
          RequiredImport = None
          DeltaTypeName = "One"
          SupportedRenderers = Set.ofList [ RendererName "admin"; RendererName "bestFriend" ]
          MappingFunction = "" }
      Many =
        { GeneratedTypeName = "Many"
          ChunkTypeName = "Chunk"
          ItemTypeName = "Item"
          RequiredImport = None
          DeltaTypeName = "Many"
          SupportedRenderers =
            { LinkedUnlinkedRenderers = Set.empty
              AllRenderers = Set.empty }
          MappingFunction = "" }
      ReadOnly =
        { GeneratedTypeName = "ReadOnly"
          RequiredImport = None
          DeltaTypeName = "ReadOnly"
          SupportedRenderers = Set.empty }
      Map =
        { GeneratedTypeName = "map"
          RequiredImport = None
          DeltaTypeName = "map"
          SupportedRenderers =
            Set.ofList
              [ RendererName "map"
                RendererName "defaultMap"
                RendererName "daisyMap"
                RendererName "nestedMap"
                RendererName "keyValue" ] }
      Sum =
        { GeneratedTypeName = "Sum"
          RequiredImport = None
          DeltaTypeName = "Sum"
          LeftConstructor = "Left"
          RightConstructor = "Right"
          SupportedRenderers =
            Set.ofList
              [ RendererName "sum"
                RendererName "defaultSum"
                RendererName "onlyLeft"
                RendererName "onlyRight"
                RendererName "readonlySum"
                RendererName "switchableSum"
                RendererName "customAiBanner"
                RendererName "localStateFormRight" ]
          Serialization =
            { Serializer =
                { Name = "serialization.SumSerializer"
                  Import = GoImport "serialization/library" }
              Deserializer =
                { Name = "serialization.SumDeserializer"
                  Import = GoImport "serialization/library" } } }
      Tuple =
        [ { Ariety = 2
            GeneratedTypeName = "Tuple2"
            DeltaTypeName = "DeltaTuple2"
            SupportedRenderers = Set.ofList [ RendererName "defaultTuple2" ]
            Constructor = "NewTuple2"
            RequiredImport = None
            Serialization =
              { Serializer =
                  { Name = "serialization.Tuple2Serializer"
                    Import = GoImport "serialization/library" }
                Deserializer =
                  { Name = "serialization.Tuple2Deserializer"
                    Import = GoImport "serialization/library" } } }
          { Ariety = 3
            GeneratedTypeName = "Tuple3"
            DeltaTypeName = "DeltaTuple3"
            SupportedRenderers = Set.ofList [ RendererName "defaultTuple3" ]
            Constructor = "NewTuple3"
            RequiredImport = None
            Serialization =
              { Serializer =
                  { Name = "serialization.Tuple3Serializer"
                    Import = GoImport "serialization/library" }
                Deserializer =
                  { Name = "serialization.Tuple3Deserializer"
                    Import = GoImport "serialization/library" } } } ]
      Union = { SupportedRenderers = Set.ofList [ RendererName "union"; RendererName "job" ] }
      Record =
        { SupportedRenderers =
            Map.ofList
              [ (RendererName "containerRecord"), Set.empty
                (RendererName "userDetails"), Set.empty
                (RendererName "personDetails"), Set.empty
                (RendererName "nameAndDescription"), Set.ofList [ "Name"; "Description" ]
                (RendererName "address"), Set.empty
                (RendererName "dashboardConfig"), Set.empty ] }
      Custom =
        Map.ofList
          [ "injectedCategory",
            { GeneratedTypeName = "Unit"
              DeltaTypeName = "DeltaUnit"
              RequiredImport = None
              SupportedRenderers = Set.ofList [ RendererName "defaultCategory" ] } ]
      Generic =
        [ {| Type = "{ \"fun\": \"Sum\",  \"args\": [ \"unit\", \"Date\" ] }"
             SupportedRenderers =
              Set.ofList
                [ RendererName "maybeDate"
                  RendererName "maybeDaisyDate"
                  RendererName "maybeDateNotEditable"
                  RendererName "maybeDateCell"
                  RendererName "maybeDateNotEditableCell" ] |} ]
      IdentifierAllowedRegex = ""
      DeltaBase =
        { GeneratedTypeName = "DeltaBase"
          RequiredImport = None }
      EntityNotFoundError =
        { GeneratedTypeName = "EntityNotFoundError"
          Constructor = "NewEntityNotFoundError"
          RequiredImport = None }
      OneNotFoundError =
        { GeneratedTypeName = "OneNotFoundError"
          Constructor = "NewOneNotFoundError"
          RequiredImport = None }
      LookupStreamNotFoundError =
        { GeneratedTypeName = "LookupStreamNotFoundError"
          Constructor = "NewLookupStreamNotFoundError"
          RequiredImport = None }
      ManyNotFoundError =
        { GeneratedTypeName = "ManyNotFoundError"
          Constructor = "NewManyNotFoundError"
          RequiredImport = None }
      TableNotFoundError =
        { GeneratedTypeName = "TableNotFoundError"
          Constructor = "NewTableNotFoundError"
          RequiredImport = None }
      EntityNameAndDeltaTypeMismatchError =
        { GeneratedTypeName = "EntityNameAndDeltaTypeMismatchError"
          Constructor = "NewEntityNameAndDeltaTypeMismatchError"
          RequiredImport = None }
      EnumNotFoundError =
        { GeneratedTypeName = "EnumNotFoundError"
          Constructor = "NewEnumNotFoundError"
          RequiredImport = None }
      InvalidEnumValueCombinationError =
        { GeneratedTypeName = "InvalidEnumValueCombinationError"
          Constructor = "NewInvalidEnumValueCombinationError"
          RequiredImport = None }
      StreamNotFoundError =
        { GeneratedTypeName = "StreamNotFoundError"
          Constructor = "NewStreamNotFoundError"
          RequiredImport = None }
      ContainerRenderers = Set.ofList [ RendererName "highlighted"; RendererName "grayBackground" ]
      GenerateReplace = Set.empty
      LanguageStreamType = LanguageStreamType "Language" }
