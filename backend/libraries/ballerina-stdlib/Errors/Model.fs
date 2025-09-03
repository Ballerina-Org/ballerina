namespace Ballerina

module Errors =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open System
  open Ballerina.StdLib.String

  open Ballerina.Collections.NonEmptyList
  open Fun

  type ErrorPriority =
    | High
    | Medium
    | Low

  type Error =
    { Message: string
      Priority: ErrorPriority }

    static member Updaters =
      {| Message = fun u err -> { err with Message = u (err.Message) }
         Priority = fun u err -> { err with Priority = u (err.Priority) } |}

  type Errors =
    { Errors: NonEmptyList<Error> }

    static member Singleton e =
      { Errors =
          NonEmptyList.One(
            { Message = e
              Priority = ErrorPriority.Low }
          ) }

    static member Concat(e1, e2) =
      { Errors = NonEmptyList.OfList(e1.Errors.Head, e1.Errors.Tail @ (e2.Errors |> NonEmptyList.ToList)) }

    static member Map f e =
      { e with
          Errors = e.Errors |> NonEmptyList.map (Error.Updaters.Message f) }

    static member WithPriority p e =
      { e with
          Errors = e.Errors |> NonEmptyList.map (Error.Updaters.Priority(replaceWith p)) }

    static member HighestPriority e =
      let errors = e.Errors |> NonEmptyList.ToList

      match errors |> List.filter (fun e -> e.Priority.IsHigh) with
      | x :: xs ->
        { e with
            Errors = NonEmptyList.OfList(x, xs) }
      | [] ->
        match errors |> List.filter (fun e -> e.Priority.IsMedium) with
        | x :: xs ->
          { e with
              Errors = NonEmptyList.OfList(x, xs) }
        | [] -> e

    static member Print (inputFile: string) (e: Errors) =
      do Console.WriteLine $"Errors when processing {inputFile}"
      do Console.ForegroundColor <- ConsoleColor.Red

      for error in (e |> Errors.HighestPriority).Errors do
        // do Console.Write error.Priority
        // do Console.Write ": "
        do Console.WriteLine error.Message

      do Console.ResetColor()

  type Map<'k, 'v when 'k: comparison> with
    static member tryFindWithError k k_category k_error m =
      let withError (e: string) (o: Option<'res>) : Sum<'res, Errors> =
        o |> Sum.fromOption<'res, Errors> (fun () -> Errors.Singleton e)

      m
      |> Map.tryFind k
      |> withError (sprintf "Cannot find %s '%s'" k_category k_error)

    static member tryFindByWithError
      (predicate: 'k * 'v -> bool)
      (k_category: string)
      (k_error: string)
      (m: Map<'k, 'v>)
      : Sum<'k * 'v, Errors> =

      let withError (e: string) (o: Option<'res>) : Sum<'res, Errors> =
        o |> Sum.fromOption<'res, Errors> (fun () -> Errors.Singleton e)

      m
      |> Seq.map (fun (KeyValue(k, v)) -> (k, v))
      |> Seq.tryFind predicate
      |> withError (sprintf "Cannot find %s '%s'" k_category k_error)

  type SumBuilder with
    member sum.WithErrorContext err =
      sum.MapError(Errors.Map(String.appendNewline err))

  type StateBuilder with
    member state.WithErrorContext err =
      state.MapError(Errors.Map(String.appendNewline err))

  type ReaderBuilder with
    member reader.WithErrorContext err =
      reader.MapError(Errors.Map(String.appendNewline err))
