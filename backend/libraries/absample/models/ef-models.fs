module absample.efmodels

open absample.models

open System
open System.Runtime.Serialization
open System.Text.Json.Serialization
open Ballerina.Fun

type AB = absample.models.AB
type ABEventStatus = absample.models.ABEventStatus
// [<KnownType(typeof<AEvent>)>]
// [<KnownType(typeof<BEvent>)>]
// [<JsonConverter(typeof<JsonInheritanceConverter>)>]
[<JsonDerivedType(typeof<AEvent>, typeDiscriminator = "AEvent")>]
[<JsonDerivedType(typeof<BEvent>, typeDiscriminator = "BEvent")>]
[<AbstractClass>]
type ABEvent(ABId: Guid, CreatedAt: DateTime, ProcessingStatus: ABEventStatus) =
  member val ABEventId = Guid.Empty with get, set
  member val ABId = ABId with get, set
  member val AB = Unchecked.defaultof<AB> with get, set
  // [<JsonIgnore>] // reminder: the problem is that we want to serialize the field normally, but not deserialize it and instead use a default
  member val CreatedAt = CreatedAt with get, set
  // [<JsonIgnore>] // reminder: the problem is that we want to serialize the field normally, but not deserialize it and instead use a default
  member val ProcessingStatus = ProcessingStatus with get, set

  static member ToRecord(e: ABEvent) =
    { ABEventId = e.ABEventId
      ABId = e.ABId
      AB = e.AB
      CreatedAt = e.CreatedAt
      ProcessingStatus = e.ProcessingStatus }

  static member WithRecord (u: Updater<ABEventBase>) (this: ABEvent) =
    let this = this |> ABEvent.ToUnion

    (match this with
     | absample.models.ABEvent.AEvent e ->
       absample.models.ABEvent.AEvent { e with event = u (e.event) }
     | absample.models.ABEvent.BEvent e ->
       absample.models.ABEvent.BEvent { e with event = u (e.event) })
    |> ABEvent.FromUnion

  static member ToUnion(instance: ABEvent) =
    match instance with
    | :? AEvent as e -> e |> AEvent.ToRecord |> absample.models.ABEvent.AEvent
    | :? BEvent as e -> e |> BEvent.ToRecord |> absample.models.ABEvent.BEvent
    | _ -> failwith "cannot convert Tag to union, a case is missing"

  static member FromUnion(instance: absample.models.ABEvent) : ABEvent =
    match instance with
    | AEvent e -> e |> AEvent.FromRecord :> ABEvent
    | BEvent e -> e |> BEvent.FromRecord :> ABEvent

and AEvent
  (ABId: Guid, CreatedAt: DateTime, ProcessingStatus: ABEventStatus, AStep: int)
  =
  inherit ABEvent(ABId, CreatedAt, ProcessingStatus)
  member val AStep = AStep with get, set
  // member val A1Event:A1Event = Unchecked.defaultof<A1Event> with get, set
  static member ToRecord(e: AEvent) : absample.models.AEvent =
    { event = ABEvent.ToRecord e
      AStep = e.AStep }

  static member FromRecord(e: absample.models.AEvent) : AEvent =
    new AEvent(
      ABEventId = e.event.ABEventId,
      ABId = e.event.ABId,
      AB = e.event.AB,
      CreatedAt = e.event.CreatedAt,
      ProcessingStatus = e.event.ProcessingStatus,
      AStep = e.AStep
    )

and BEvent
  (ABId: Guid, CreatedAt: DateTime, ProcessingStatus: ABEventStatus, BStep: int)
  =
  inherit ABEvent(ABId, CreatedAt, ProcessingStatus)
  member val BStep = BStep with get, set

  static member ToRecord(e: BEvent) : absample.models.BEvent =
    { event = ABEvent.ToRecord e
      BStep = e.BStep }

  static member FromRecord(e: absample.models.BEvent) : BEvent =
    new BEvent(
      ABEventId = e.event.ABEventId,
      ABId = e.event.ABId,
      AB = e.event.AB,
      CreatedAt = e.event.CreatedAt,
      ProcessingStatus = e.event.ProcessingStatus,
      BStep = e.BStep
    )
