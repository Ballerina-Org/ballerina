module abcdsample.context

#nowarn 40

open System
open positions.model
open Ballerina.DSL.Model
open Ballerina.DSL.Expr.Model
open abcdsample.schema

let init_abcdContext () =
  let ABs: ref<Map<Guid, AB>> = ref Map.empty
  let CDs: ref<Map<Guid, CD>> = ref Map.empty
  let EFs: ref<Map<Guid, EF>> = ref Map.empty

  let descriptors, allEntities, allFields = createABCDSchema ABs CDs EFs

  let schema: Schema =
    { tryFindEntity = fun (entityDescriptorId: EntityDescriptorId) -> allEntities |> Map.tryFind entityDescriptorId
      tryFindField = fun (fieldDescriptorId: FieldDescriptorId) -> allFields |> Map.tryFind fieldDescriptorId }

  let createEF id E F = { EFId = id; E = E; F = F }
  let ef1 = createEF (Guid("6b3b39dc-24e8-425f-8bf3-7abd248f522f")) 3 4
  let ef2 = createEF (Guid("2a2356f1-f220-450c-abcb-a9baf0f4094d")) 5 7
  EFs.contents <- [ ef1; ef2 ] |> Seq.map (fun e -> (e.EFId, e)) |> Map.ofSeq

  let createCD id C D ef =
    { CDId = id
      C = C
      D = D
      EFId = ef.EFId }

  let cd1 = createCD (Guid("d8ff0920-2b47-499f-9f7b-cb07a1f8f3a4")) 3 4 ef1
  let cd2 = createCD (Guid("69f182db-84ba-4e81-91c5-d3becd029a6b")) 30 40 ef2
  CDs.contents <- [ cd1; cd2 ] |> Seq.map (fun e -> (e.CDId, e)) |> Map.ofSeq

  let createAB id cd A B =
    { ABId = id
      A1 = A
      B1 = B
      Total1 = 0
      CDId = cd.CDId
      А2 = A
      Б2 = B
      Весь2 = 0
      Α3 = A
      Β3 = B
      Σ3 = 0 }

  let ab1 = createAB (Guid("8fba2a7c-e2da-43bd-b8ee-ddaa774d081d")) cd1 1 2
  let ab2 = createAB (Guid("91620c12-cd9e-4e66-9df3-58f4b1a50b1f")) cd2 10 20

  ABs.contents <- [ ab1; ab2 ] |> Seq.map (fun e -> (e.ABId, e)) |> Map.ofSeq

  let (!) (varname: string) = { VarName = varname }
  let (!!) (varname: string) = !varname |> Expr.VarLookup

  let rec (=>) (e: Expr) (fields: List<FieldDescriptorId>) =
    match fields with
    | [] -> e
    | field :: fields -> FieldLookup(e, field) => fields

  let total1: BusinessRule =
    { BusinessRuleId = Guid.CreateVersion7()
      Name = "Total1 = A1+B1+CD.C+CD.D"
      Priority = BusinessRulePriority.System
      Condition =
        Expr.Exists(!"this", descriptors.AB.Entity.Descriptor.ToEntityDescriptorId, Expr.Value(Value.ConstBool true))
      Actions =
        [ { Variable = !"this", [ descriptors.AB.Total1.ToFieldDescriptorId ]
            Value =
              (!!"this" => [ descriptors.AB.A1.ToFieldDescriptorId ])
              + (!!"this" => [ descriptors.AB.B1.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.C.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.D.ToFieldDescriptorId ]) } ] }

  let total2: BusinessRule =
    { BusinessRuleId = Guid.CreateVersion7()
      Name = "Total2 = A2+B2+CD.C+CD.D"
      Priority = BusinessRulePriority.System
      Condition =
        Expr.Exists(!"this", descriptors.AB.Entity.Descriptor.ToEntityDescriptorId, Expr.Value(Value.ConstBool true))
      Actions =
        [ { Variable = !"this", [ descriptors.AB.Весь2.ToFieldDescriptorId ]
            Value =
              (!!"this" => [ descriptors.AB.А2.ToFieldDescriptorId ])
              + (!!"this" => [ descriptors.AB.Б2.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.C.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.D.ToFieldDescriptorId ]) } ] }

  let total3: BusinessRule =
    { BusinessRuleId = Guid.CreateVersion7()
      Name = "Total3 = A3+B3+CD.EF.E+CD.EF.F"
      Priority = BusinessRulePriority.System
      Condition =
        Expr.Exists(!"this", descriptors.AB.Entity.Descriptor.ToEntityDescriptorId, Expr.Value(Value.ConstBool true))
      Actions =
        [ { Variable = !"this", [ descriptors.AB.Σ3.ToFieldDescriptorId ]
            Value =
              (!!"this" => [ descriptors.AB.Α3.ToFieldDescriptorId ])
              + (!!"this" => [ descriptors.AB.Β3.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.EF().ToFieldDescriptorId
                      descriptors.EF.E.ToFieldDescriptorId ])
              + (!!"this"
                 => [ descriptors.AB.CD().ToFieldDescriptorId
                      descriptors.CD.EF().ToFieldDescriptorId
                      descriptors.EF.F.ToFieldDescriptorId ]) } ] }

  let _ =
    [ { BusinessRuleId = Guid.CreateVersion7()
        Name = "Total2 = Total1+1+Total3"
        Priority = BusinessRulePriority.System
        Condition =
          Expr.Exists(!"this", descriptors.AB.Entity.Descriptor.ToEntityDescriptorId, Expr.Value(Value.ConstBool true))
        Actions =
          [ { Variable = !"this", [ descriptors.AB.Весь2.ToFieldDescriptorId ]
              Value =
                (!!"this" => [ descriptors.AB.Total1.ToFieldDescriptorId ])
                + Expr.Value(Value.ConstInt 1)
                + (!!"this" => [ descriptors.AB.Σ3.ToFieldDescriptorId ]) } ] }
      { BusinessRuleId = Guid.CreateVersion7()
        Name = "Total3 = Total2+1"
        Priority = BusinessRulePriority.System
        Condition =
          Expr.Exists(!"this", descriptors.AB.Entity.Descriptor.ToEntityDescriptorId, Expr.Value(Value.ConstBool true))
        Actions =
          [ { Variable = !"this", [ descriptors.AB.Σ3.ToFieldDescriptorId ]
              Value =
                (!!"this" => [ descriptors.AB.Весь2.ToFieldDescriptorId ])
                + Expr.Value(Value.ConstInt 1) } ] } ]

  let businessRules =
    // totalsLoop @
    [ total1; total2; total3 ]
    |> Seq.map (fun br -> br.BusinessRuleId, br)
    |> Map.ofSeq

  let context =
    { ABs = (fun () -> ABs.contents)
      CDs = (fun () -> CDs.contents)
      EFs = (fun () -> EFs.contents)
      ActiveEvents =
        [ ABCDEvent.Edit(
            { BusinessRuleId = Guid.CreateVersion7()
              Name = "ab1.A1 := ab1.A1+10"
              Priority = BusinessRulePriority.User
              Condition =
                Expr.Exists(
                  !"ab1",
                  descriptors.AB.Entity.Descriptor.ToEntityDescriptorId,
                  Expr.Binary(
                    BinaryOperator.Equals,
                    !!"ab1" => [ descriptors.AB.ABId().ToFieldDescriptorId ],
                    Expr.Value(Value.ConstGuid ab1.ABId)
                  )
                )
              Actions =
                [ { Variable = !"ab1", [ descriptors.AB.A1.ToFieldDescriptorId ]
                    Value =
                      (!!"ab1" => [ descriptors.AB.A1.ToFieldDescriptorId ])
                      + (Expr.Value(Value.ConstInt 10)) } ] }
          )
          ABCDEvent.Edit(
            { BusinessRuleId = Guid.CreateVersion7()
              Name = "cd2.C := cd2.C+20"
              Priority = BusinessRulePriority.User
              Condition =
                Expr.Exists(
                  !"cd2",
                  descriptors.CD.Entity.Descriptor.ToEntityDescriptorId,
                  Expr.Binary(
                    BinaryOperator.Equals,
                    !!"cd2" => [ descriptors.CD.CDId().ToFieldDescriptorId ],
                    Expr.Value(Value.ConstGuid cd2.CDId)
                  )
                )
              Actions =
                [ { Variable = !"cd2", [ descriptors.CD.C.ToFieldDescriptorId ]
                    Value =
                      (!!"cd2" => [ descriptors.CD.C.ToFieldDescriptorId ])
                      + (Expr.Value(Value.ConstInt 20)) } ] }
          )
          ABCDEvent.Edit(
            { BusinessRuleId = Guid.CreateVersion7()
              Name = "ab1.CD := cd2"
              Priority = BusinessRulePriority.User
              Condition =
                Expr.Exists(
                  !"ab1",
                  descriptors.AB.Entity.Descriptor.ToEntityDescriptorId,
                  Expr.Binary(
                    BinaryOperator.Equals,
                    !!"ab1" => [ descriptors.AB.ABId().ToFieldDescriptorId ],
                    Expr.Value(Value.ConstGuid ab1.ABId)
                  )
                )
              Actions =
                [ { Variable = !"ab1", [ descriptors.AB.CD().ToFieldDescriptorId ]
                    Value = (Expr.Value(Value.ConstGuid cd2.CDId)) } ] }
          )
          ABCDEvent.Edit(
            { BusinessRuleId = Guid.CreateVersion7()
              Name = "ef1.E := ef1.E + 10"
              Priority = BusinessRulePriority.User
              Condition =
                Expr.Exists(
                  !"ef1",
                  descriptors.EF.Entity.Descriptor.ToEntityDescriptorId,
                  Expr.Binary(
                    BinaryOperator.Equals,
                    !!"ef1" => [ descriptors.EF.EFId().ToFieldDescriptorId ],
                    Expr.Value(Value.ConstGuid ef1.EFId)
                  )
                )
              Actions =
                [ { Variable = !"ef1", [ descriptors.EF.E.ToFieldDescriptorId ]
                    Value =
                      (!!"ef1" => [ descriptors.EF.E.ToFieldDescriptorId ])
                      + (Expr.Value(Value.ConstInt 10)) } ] }
          ) ] // :List<FieldEvent>;
      PastEvents = [] // :List<FieldEvent>;
      BusinessRules = businessRules
      Schema = schema }

  // do printfn "lookedUpFieldDescriptors total3.Actions[0] %A" (lookedUpFieldDescriptors total3.Actions.Head.Value)
  // do Console.ReadLine() |> ignore

  context
