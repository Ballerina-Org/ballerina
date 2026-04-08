module abcdsample.schema

#nowarn 40

open System
open System.Linq
open positions.model
open Ballerina.Fun
open Ballerina.Coroutines
open Ballerina.Collections.Option
open Ballerina.Collections.Map
open Ballerina.Collections.Sum
open Ballerina.Coroutines
open Ballerina.DSL.Model
open Ballerina.DSL.Execute
open Ballerina.DSL.TransitiveExecution
open Ballerina.DSL.Expr.Eval
open Ballerina.DSL.Preprocessor
open Ballerina.DSL.Predicate

let createABCDSchema
  (allABs: ref<Map<Guid, AB>>)
  (allCDs: ref<Map<Guid, CD>>)
  (allEFs: ref<Map<Guid, EF>>)
  =

  let rec descriptors =
    {| EF =
        {| Entity =
            {| Descriptor =
                { EntityDescriptorId = Guid.CreateVersion7()
                  EntityName = "EF"
                  TryFind =
                    fun id ->
                      allEFs.contents
                      |> Map.tryFind id
                      |> Option.map (fun e -> e :> obj)
                  GetId =
                    (function
                    | :? EF as e -> Some e.EFId
                    | _ -> None)
                  Lookup =
                    fun (obj, fields) ->
                      EntityDescriptor.GenericLookup
                        descriptors.EF.Entity.Descriptor
                        allEntities
                        (obj, fields)
                  GetEntities =
                    fun () ->
                      allEFs.contents
                      |> Map.values
                      |> Seq.map (fun e -> e :> obj)
                      |> List.ofSeq
                  GetFieldDescriptors =
                    fun () ->
                      [ descriptors.EF.EFId()
                        descriptors.EF.F
                        descriptors.EF.E ]
                      |> Seq.map (fun (fd: FieldDescriptor) ->
                        fd.ToFieldDescriptorId, fd)
                      |> Map.ofSeq }
               TryFind =
                fun id ->
                  allEFs.contents
                  |> Map.tryFind id
                  |> Option.map (fun e -> e :> obj) |}
           EFId =
            let guid = Guid.CreateVersion7() in

            fun () ->
              FieldDescriptor.Default().IdField
                guid
                "EFId"
                descriptors.EF.Entity.Descriptor.ToEntityDescriptorId
                (fun id -> allEFs.contents |> Map.tryFind id)
                (fun (e': EF) entityId ->
                  allEFs.contents <- allEFs.contents |> Map.add entityId e')
                (fun e -> e.EFId)
                (fun (e: EF) f -> { e with EFId = f })
           E =
            FieldDescriptor.Default().IntField
              "E"
              (fun id -> allEFs.contents |> Map.tryFind id)
              (fun (e': EF) entityId ->
                allEFs.contents <- allEFs.contents |> Map.add entityId e')
              (fun e -> e.E)
              (fun (e: EF) f -> { e with E = f })
           F =
            FieldDescriptor.Default().IntField
              "F"
              (fun id -> allEFs.contents |> Map.tryFind id)
              (fun (e': EF) entityId ->
                allEFs.contents <- allEFs.contents |> Map.add entityId e')
              (fun e -> e.F)
              (fun (e: EF) f -> { e with F = f }) |}
       CD =
        {| Entity =
            {| Descriptor =
                { EntityDescriptorId = Guid.CreateVersion7()
                  EntityName = "CD"
                  TryFind =
                    fun id ->
                      allCDs.contents
                      |> Map.tryFind id
                      |> Option.map (fun e -> e :> obj)
                  GetId =
                    (function
                    | :? CD as e -> Some e.CDId
                    | _ -> None)
                  Lookup =
                    fun (obj, fields) ->
                      EntityDescriptor.GenericLookup
                        descriptors.CD.Entity.Descriptor
                        allEntities
                        (obj, fields)
                  GetEntities =
                    fun () ->
                      allCDs.contents
                      |> Map.values
                      |> Seq.map (fun e -> e :> obj)
                      |> List.ofSeq
                  GetFieldDescriptors =
                    fun () ->
                      [ descriptors.CD.CDId()
                        descriptors.CD.D
                        descriptors.CD.C
                        descriptors.CD.EF() ]
                      |> Seq.map (fun (fd: FieldDescriptor) ->
                        fd.ToFieldDescriptorId, fd)
                      |> Map.ofSeq }
               TryFind =
                fun id ->
                  allCDs.contents
                  |> Map.tryFind id
                  |> Option.map (fun e -> e :> obj) |}
           CDId =
            let guid = Guid.CreateVersion7() in

            fun () ->
              FieldDescriptor.Default().IdField
                guid
                "CDId"
                descriptors.CD.Entity.Descriptor.ToEntityDescriptorId
                (fun id -> allCDs.contents |> Map.tryFind id)
                (fun (e': CD) entityId ->
                  allCDs.contents <- allCDs.contents |> Map.add entityId e')
                (fun e -> e.CDId)
                (fun (e: CD) f -> { e with CDId = f })
           C =
            FieldDescriptor.Default().IntField
              "C"
              (fun id -> allCDs.contents |> Map.tryFind id)
              (fun (e': CD) entityId ->
                allCDs.contents <- allCDs.contents |> Map.add entityId e')
              (fun e -> e.C)
              (fun (e: CD) f -> { e with C = f })
           D =
            FieldDescriptor.Default().IntField
              "D"
              (fun id -> allCDs.contents |> Map.tryFind id)
              (fun (e': CD) entityId ->
                allCDs.contents <- allCDs.contents |> Map.add entityId e')
              (fun e -> e.D)
              (fun (e: CD) f -> { e with D = f })
           EF =
            let guid = Guid.CreateVersion7()

            fun () ->
              FieldDescriptor.Default().LookupField
                guid
                "EF"
                descriptors.EF.Entity.Descriptor.ToEntityDescriptorId
                (fun id -> allCDs.contents |> Map.tryFind id)
                (fun (e': CD) entityId ->
                  allCDs.contents <- allCDs.contents |> Map.add entityId e')
                (fun e -> e.EFId)
                (fun (e: CD) f -> { e with EFId = f }) |}
       AB =
        {| Entity =
            {| Descriptor =
                { EntityDescriptorId = Guid.CreateVersion7()
                  EntityName = "AB"
                  TryFind =
                    fun id ->
                      allABs.contents
                      |> Map.tryFind id
                      |> Option.map (fun e -> e :> obj)
                  GetId =
                    (function
                    | :? AB as e -> Some e.ABId
                    | _ -> None)
                  Lookup =
                    fun (obj, fields) ->
                      EntityDescriptor.GenericLookup
                        descriptors.AB.Entity.Descriptor
                        allEntities
                        (obj, fields)
                  GetEntities =
                    fun () ->
                      allABs.contents
                      |> Map.values
                      |> Seq.map (fun e -> e :> obj)
                      |> List.ofSeq
                  GetFieldDescriptors =
                    fun () ->
                      [ descriptors.AB.ABId()
                        descriptors.AB.A1
                        descriptors.AB.B1
                        descriptors.AB.CD()
                        descriptors.AB.Total1
                        descriptors.AB.А2
                        descriptors.AB.Б2
                        descriptors.AB.Весь2
                        descriptors.AB.Α3
                        descriptors.AB.Β3
                        descriptors.AB.Σ3 ]
                      |> Seq.map (fun (fd: FieldDescriptor) ->
                        fd.ToFieldDescriptorId, fd)
                      |> Map.ofSeq }
               TryFind =
                fun id ->
                  allABs.contents
                  |> Map.tryFind id
                  |> Option.map (fun e -> e :> obj) |}
           ABId =
            let guid = Guid.CreateVersion7() in

            fun () ->
              FieldDescriptor.Default().IdField
                guid
                "ABId"
                descriptors.AB.Entity.Descriptor.ToEntityDescriptorId
                (fun id -> allABs.contents |> Map.tryFind id)
                (fun (e': AB) entityId ->
                  allABs.contents <- allABs.contents |> Map.add entityId e')
                (fun e -> e.ABId)
                (fun (e: AB) f -> { e with ABId = f })
           A1 =
            FieldDescriptor.Default().IntField
              "A1"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.A1)
              (fun e f -> { e with A1 = f })
           B1 =
            FieldDescriptor.Default().IntField
              "B1"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.B1)
              (fun e f -> { e with B1 = f })
           Total1 =
            FieldDescriptor.Default().IntField
              "Total1"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Total1)
              (fun e f -> { e with Total1 = f })
           А2 =
            FieldDescriptor.Default().IntField
              "А2"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.А2)
              (fun e f -> { e with А2 = f })
           Б2 =
            FieldDescriptor.Default().IntField
              "Б2"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Б2)
              (fun e f -> { e with Б2 = f })
           Весь2 =
            FieldDescriptor.Default().IntField
              "Весь2"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Весь2)
              (fun e f -> { e with Весь2 = f })
           Α3 =
            FieldDescriptor.Default().IntField
              "Α3"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Α3)
              (fun e f -> { e with Α3 = f })
           Β3 =
            FieldDescriptor.Default().IntField
              "Β3"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Β3)
              (fun e f -> { e with Β3 = f })
           Σ3 =
            FieldDescriptor.Default().IntField
              "Σ3"
              (fun id -> allABs.contents |> Map.tryFind id)
              (fun e' entityId ->
                allABs.contents <- allABs.contents |> Map.add entityId e')
              (fun e -> e.Σ3)
              (fun e f -> { e with Σ3 = f })
           CD =
            let guid = Guid.CreateVersion7()

            fun () ->
              FieldDescriptor.Default().LookupField
                guid
                "CD"
                descriptors.CD.Entity.Descriptor.ToEntityDescriptorId
                (fun id -> allABs.contents |> Map.tryFind id)
                (fun (e': AB) entityId ->
                  allABs.contents <- allABs.contents |> Map.add entityId e')
                (fun e -> e.CDId)
                (fun (e: AB) f -> { e with CDId = f }) |} |}

  and allEntities =
    [ descriptors.AB.Entity.Descriptor
      descriptors.CD.Entity.Descriptor
      descriptors.EF.Entity.Descriptor ]
    |> Seq.map (fun e -> e.ToEntityDescriptorId, e)
    |> Map.ofSeq

  and allFields =
    [ for e in allEntities |> Map.values do
        yield! e.GetFieldDescriptors() |> Map.values ]
    |> Seq.map (fun f -> f.ToFieldDescriptorId, f)
    |> Map.ofSeq

  descriptors, allEntities, allFields
