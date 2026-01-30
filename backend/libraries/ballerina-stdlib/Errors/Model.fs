namespace Ballerina

module Errors =

  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open System
  open Ballerina.StdLib.String
  open Ballerina.Collections.TaskSum
  open Ballerina.Collections.NonEmptyList
  open Fun

  type ErrorPriority =
    | High
    | Medium
    | Low

  type Error<'context when 'context: comparison> =
    { Message: string
      Context: 'context
      Priority: ErrorPriority }

    static member Updaters =
      {| Message = fun u (err: Error<'context>) -> { err with Message = u (err.Message) }
         Context = fun u (err: Error<'context>) -> { err with Context = u (err.Context) }
         Priority = fun u (err: Error<'context>) -> { err with Priority = u (err.Priority) } |}

    static member Map f e = { e with Message = f e.Message }

    static member MapContext f e = { e with Context = f e.Context }

    static member MapPriority f e = { e with Priority = f e.Priority }

  type Errors<'context when 'context: comparison> =
    { Errors: unit -> NonEmptyList<Error<'context>> }

    static member ToString(e: Errors<'context>, concatDelimiter: string) =
      e.Errors()
      |> NonEmptyList.ToList
      |> Seq.map _.Message
      |> String.concat concatDelimiter

    override this.ToString() = Errors<_>.ToString(this, "\n")

    static member Singleton (context: 'context) (e: unit -> string) =
      { Errors =
          fun () ->
            NonEmptyList.One(
              { Message = e ()
                Context = context
                Priority = ErrorPriority.Low }
            ) }

    static member Concat(e1: Errors<'a>, e2: Errors<'a>) =
      { Errors =
          fun () ->
            let e1 = e1.Errors()
            let e2 = e2.Errors()
            NonEmptyList.OfList(e1.Head, e1.Tail @ (e2 |> NonEmptyList.ToList)) }

    static member Map f (e: Errors<'context>) =
      { e with
          Errors = fun () -> e.Errors() |> NonEmptyList.map (Error.Updaters.Message f) }

    static member MapContext (f: 'context -> 'ctx') (e: Errors<'context>) =
      { Errors =
          fun () ->
            e.Errors()
            |> NonEmptyList.map (fun e ->
              { Message = e.Message
                Priority = e.Priority
                Context = f e.Context }) }

    static member MapPriority f (e: Errors<'context>) =
      { e with
          Errors = fun () -> e.Errors() |> NonEmptyList.map (fun e -> { e with Priority = f e.Priority }) }


    [<Obsolete("Use 'MapPriority' instead")>]
    static member WithPriority p (e: Errors<'context>) =
      { e with
          Errors = fun () -> e.Errors() |> NonEmptyList.map (Error.Updaters.Priority(replaceWith p)) }


    [<Obsolete("Use 'MapPriority' instead")>]
    static member SetPriority p (e: Errors<'context>) =
      { e with
          Errors = fun () -> e.Errors() |> NonEmptyList.map (Error.Updaters.Priority(replaceWith p)) }

    static member HighestPriority(e: Errors<'context>) =
      let errors = e.Errors() |> NonEmptyList.ToList

      match errors |> List.filter (fun e -> e.Priority.IsHigh) with
      | x :: xs ->
        { e with
            Errors = fun () -> NonEmptyList.OfList(x, xs) }
      | [] ->
        match errors |> List.filter (fun e -> e.Priority.IsMedium) with
        | x :: xs ->
          { e with
              Errors = fun () -> NonEmptyList.OfList(x, xs) }
        | [] -> e

    static member FilterHighestPriorityOnly e =
      let errors = e.Errors() |> NonEmptyList.ToList

      match errors |> List.filter (fun e -> e.Priority.IsHigh) with
      | x :: xs ->
        { e with
            Errors = fun () -> NonEmptyList.OfList(x, xs) }
      | [] ->
        match errors |> List.filter (fun e -> e.Priority.IsMedium) with
        | x :: xs ->
          { e with
              Errors = fun () -> NonEmptyList.OfList(x, xs) }
        | [] -> e

    static member Print (inputFile: string) (e: Errors<'context>) =
      do Console.WriteLine $"Errors when processing {inputFile}"
      do Console.ForegroundColor <- ConsoleColor.Red

      for error in (e |> Errors<_>.HighestPriority).Errors() do
        // do Console.Write error.Priority
        // do Console.Write ": "
        do Console.WriteLine error.Message

      do Console.ResetColor()

    static member FromExn(ex: exn) =
      Errors<_>.Singleton () (fun () -> ex.ToString())

  let private withError (context: 'context) (e: unit -> string) (o: Option<'res>) : Sum<'res, Errors<'context>> =
    o
    |> Sum.fromOption<'res, Errors<'context>> (fun () -> Errors<_>.Singleton context e)

  type Map<'k, 'v when 'k: comparison> with
    static member tryFindWithError k k_category k_error context m : Sum<'c, Errors<'context>> =
      m
      |> Map.tryFind k
      |> withError context ((fun () -> sprintf "Cannot find %s '%s'" k_category (k_error ())))

    static member tryFindByWithError
      (predicate: 'k * 'v -> bool)
      (k_category: string)
      (k_error: unit -> string)
      (context: 'context)
      (m: Map<'k, 'v>)
      : Sum<'k * 'v, Errors<'context>> =
      m
      |> Seq.map (fun (KeyValue(k, v)) -> (k, v))
      |> Seq.tryFind predicate
      |> withError context (fun () -> sprintf "Cannot find %s '%s'" k_category (k_error ()))

  type SumBuilder with
    member sum.WithErrorContext err =
      sum.MapError(Errors<_>.Map(fun s -> String.appendNewline (err ()) s))

  type StateBuilder with
    member state.WithErrorContext err =
      state.MapError(Errors<_>.Map(fun s -> String.appendNewline (err ()) s))

  type ReaderBuilder with
    member reader.WithErrorContext err =
      reader.MapError(Errors<_>.Map(fun s -> String.appendNewline (err ()) s))

  type TaskSumBuilder with
    member this.WithErrorContext err =
      this.MapRight(Errors<_>.Map(fun s -> String.appendNewline (err ()) s))
