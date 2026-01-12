namespace Ballerina

module LocalizedErrors =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open System
  open Ballerina.StdLib.String

  open Ballerina.Collections.NonEmptyList
  open Fun

  type Location =
    { File: string
      Line: int
      Column: int }

    override this.ToString() = $"{this.Line}:{this.Column}"

  type Location with
    static member Initial(file: string) = { File = file; Line = 1; Column = 1 }

    static member Unknown: Location =
      { Location.File = "unknown"
        Line = 1
        Column = 1 }

    static member Step (c: Char) (loc: Location) : Location =
      match c with
      | '\n' ->
        { loc with
            Line = loc.Line + 1
            Column = 1 }
      | _ -> { loc with Column = loc.Column + 1 }

  type ErrorPriority = Errors.ErrorPriority

  type LocalizedError =
    { Message: string
      Priority: ErrorPriority
      Location: Location }

    static member FromError (loc: Location) (e: Errors.Error) : LocalizedError =
      { Message = e.Message
        Priority = e.Priority
        Location = loc }

    member this.ToRegularError() : Errors.Error =
      { Message =
          if this.Location = Location.Unknown then
            this.Message
          else
            $"{this.Message}; Location: {this.Location}"
        Priority = this.Priority }

    static member Updaters =
      {| Message = fun u err -> { err with Message = u (err.Message) }
         Priority = fun u err -> { err with Priority = u (err.Priority) }
         Location = fun u err -> { err with Location = u (err.Location) } |}

  type Errors =
    { Errors: NonEmptyList<LocalizedError> }

    static member Singleton(loc, e) =
      { Errors =
          NonEmptyList.One(
            { Message = e
              Priority = ErrorPriority.Low
              Location = loc }
          ) }

    static member FromErrors (loc: Location) (e: Errors.Errors) : Errors =
      { Errors = e.Errors |> NonEmptyList.map (LocalizedError.FromError loc) }

    member this.ToRegularErrors() : Errors.Errors =
      { Errors = this.Errors |> NonEmptyList.map _.ToRegularError() }

    static member Concat(e1, e2) =
      { Errors = NonEmptyList.OfList(e1.Errors.Head, e1.Errors.Tail @ (e2.Errors |> NonEmptyList.ToList)) }

    static member Map f e =
      { e with
          Errors = e.Errors |> NonEmptyList.map (LocalizedError.Updaters.Message f) }

    static member SetPriority p e =
      { e with
          Errors = e.Errors |> NonEmptyList.map (LocalizedError.Updaters.Priority(replaceWith p)) }

    static member FilterHighestPriorityOnly e =
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

      for error in (e |> Errors.FilterHighestPriorityOnly).Errors do
        // do Console.Write error.Priority
        // do Console.Write ": "
        do Console.WriteLine $"{error.Message}@{error.Location}"

      do Console.ResetColor()

  type Map<'k, 'v when 'k: comparison> with
    static member tryFindWithError k k_category k_error loc m =
      let withError (e) (o: Option<'res>) : Sum<'res, Errors> =
        o |> Sum.fromOption<'res, Errors> (fun () -> Errors.Singleton(loc, e))

      m
      |> Map.tryFind k
      |> withError (sprintf "Cannot find %s '%s'" k_category k_error)

    static member tryFindByWithError
      (predicate: 'k * 'v -> bool)
      (k_category: string)
      (k_error: string)
      (loc: Location)
      (m: Map<'k, 'v>)
      : Sum<'k * 'v, Errors> =

      let withError (e: string) (o: Option<'res>) : Sum<'res, Errors> =
        o |> Sum.fromOption<'res, Errors> (fun () -> Errors.Singleton(loc, e))

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
