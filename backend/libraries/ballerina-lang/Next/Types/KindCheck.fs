namespace Ballerina.DSL.Next.Types

module KindCheck =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type KindCheckContext = { Kinds: Map<string, Kind> }
  type KindChecker = TypeExpr -> ReaderWithError<KindCheckContext, Kind, Errors>

  type KindCheckContext with
    static member Empty: KindCheckContext = { Kinds = Map.empty }

    static member Updaters =
      {| Kinds = fun u (c: KindCheckContext) -> { c with Kinds = c.Kinds |> u } |}

    static member Create(kinds: Map<string, Kind>) : KindCheckContext =
      { KindCheckContext.Empty with
          Kinds = kinds }

    static member tryFindKind(name: string) : ReaderWithError<KindCheckContext, Kind, Errors> =
      ReaderWithError(fun ctx -> ctx.Kinds |> Map.tryFindWithError name "kinds" name)

  type TypeExpr with
    static member KindCheck: KindChecker =
      fun t ->
        let (!) = TypeExpr.KindCheck

        reader {
          match t with
          | TypeExpr.Lookup id -> return! KindCheckContext.tryFindKind id
          | TypeExpr.Lambda(p, body) ->
            let! body =
              !body
              |> reader.MapContext(Map.add p.Name p.Kind |> KindCheckContext.Updaters.Kinds)

            return Kind.Arrow(p.Kind, body)
          | TypeExpr.Apply(f, a) ->
            let! f = TypeExpr.KindCheck f
            let! a = TypeExpr.KindCheck a

            match f with
            | Kind.Arrow(input, output) ->
              if input = a then
                return output
              else
                return!
                  $"Error: type mismatch in application, expected {input}, got {a}"
                  |> Errors.Singleton
                  |> reader.Throw
            | _ -> return! $"Error: expected function type, got {f}" |> Errors.Singleton |> reader.Throw
          | TypeExpr.Arrow(t1, t2)
          | TypeExpr.Exclude(t1, t2)
          | TypeExpr.Flatten(t1, t2)
          | TypeExpr.Map(t1, t2) ->
            let! t1 = !t1
            let! t2 = !t2

            if t1 <> Kind.Star || t2 <> Kind.Star then
              return!
                $"Error: expected star type, got {t1} and {t2}"
                |> Errors.Singleton
                |> reader.Throw
            else
              return Kind.Star
          | TypeExpr.Primitive _ -> return Kind.Star
          | TypeExpr.Sum args
          | TypeExpr.Tuple args ->
            do!
              args
              |> Seq.map (fun a ->
                reader {
                  let! a = !a

                  if a = Kind.Star then
                    return Kind.Star
                  else
                    return! $"Error: expected star type, got {a}" |> Errors.Singleton |> reader.Throw
                })
              |> reader.All
              |> reader.Ignore

            return Kind.Star
          | TypeExpr.Union args
          | TypeExpr.Record args ->

            do!
              args
              |> Seq.map (fun (f, a) ->
                reader {
                  let! f = !f
                  let! a = !a

                  if f = Kind.Symbol && a = Kind.Star then
                    return ()
                  else
                    return!
                      $"Error: expected symbol -> star for each record field, got {f} -> {a}"
                      |> Errors.Singleton
                      |> reader.Throw
                })
              |> reader.All
              |> reader.Ignore

            return Kind.Star
          | TypeExpr.List arg
          | TypeExpr.Set arg
          | TypeExpr.KeyOf arg
          | TypeExpr.Rotate arg ->
            let! arg = !arg

            if arg = Kind.Star then
              return Kind.Star
            else
              return! $"Error: expected star type, got {arg}" |> Errors.Singleton |> reader.Throw
        }
