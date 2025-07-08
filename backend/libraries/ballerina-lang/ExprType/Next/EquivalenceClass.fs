namespace Ballerina.DSL.Next

module EquivalenceClasses =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object

  type EquivalenceClass<'value when 'value: comparison> = Set<'value>

  and EquivalenceClassValueOperations<'var, 'value when 'var: comparison and 'value: comparison> =
    { equalize:
        'value * 'value
          -> State<unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors>
      asVar: 'value -> Sum<'var, Errors>
      toValue: 'var -> 'value }

  and EquivalenceClasses<'var, 'value when 'var: comparison and 'value: comparison> =
    { Classes: Map<string, EquivalenceClass<'value>>
      Variables: Map<'var, string> }

    static member Updaters =
      {| Classes = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Classes = c.Classes |> u }
         Variables = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Variables = c.Variables |> u } |}

    static member Empty: EquivalenceClasses<'var, 'value> =
      { Classes = Map.empty
        Variables = Map.empty }

    static member private tryGetKey(var: 'var) =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Variables
          |> Map.tryFindWithError var "var" (var.ToString())
          |> state.OfSum
      }

    static member private getKey(var: 'var) =
      state {
        let! key = EquivalenceClasses<'var, 'value>.tryGetKey var |> state.Catch

        match key with
        | Left key -> return key
        | Right _ ->
          do! state.SetState(EquivalenceClasses.Updaters.Variables(Map.add var $"{var}"))
          return $"{var}"
      }

    static member private bindKeyVar
      (key: string)
      (var: 'var)
      : State<unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun classes ->
        { classes with
            Variables = classes.Variables |> Map.add var key })

    static member private tryGetVarClass
      (key: string)
      : State<
          EquivalenceClass<'value>,
          EquivalenceClassValueOperations<'var, 'value>,
          EquivalenceClasses<'var, 'value>,
          Errors
         >
      =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Classes
          |> Map.tryFindWithError key "classes" (key.ToString())
          |> state.OfSum
      }

    static member private getVarClass
      (key: string)
      (var: 'var)
      : State<
          EquivalenceClass<'value>,
          EquivalenceClassValueOperations<'var, 'value>,
          EquivalenceClasses<'var, 'value>,
          Errors
         >
      =
      state {
        let! (valueOperations: EquivalenceClassValueOperations<'var, 'value>) = state.GetContext()
        let! varClass = EquivalenceClasses.tryGetVarClass key |> state.Catch

        match varClass with
        | Left varClass -> return varClass
        | Right _ ->
          let initialClass = var |> valueOperations.toValue |> Set.singleton
          do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key initialClass))

          return initialClass
      }

    static member private updateVarClass key u : State<unit, _, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun (classes: EquivalenceClasses<'var, 'value>) ->
        { classes with
            Classes =
              classes.Classes
              |> Map.change key (function
                | Some c -> c |> u |> Some
                | None -> Set.empty |> u |> Some) })

    static member private deleteVarClass key : State<unit, _, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun (classes: EquivalenceClasses<'var, 'value>) ->
        { classes with
            Classes = classes.Classes |> Map.remove key })

    static member Bind
      (var: 'var, value: 'value)
      : State<unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! key = EquivalenceClasses.getKey var // get the key associated with var or create a fresh key
        do! EquivalenceClasses.bindKeyVar key var // bind the key and the var (needed if the key is fresh)

        let! varClass = EquivalenceClasses.getVarClass key var

        if varClass |> Set.contains value then
          return ()
        else
          let! valueOperations = state.GetContext()
          let varClass = varClass |> Set.remove (var |> valueOperations.toValue)

          do!
            varClass
            |> Seq.map (fun otherValue -> valueOperations.equalize (otherValue, value))
            |> state.All
            |> state.Map ignore

          match! value |> valueOperations.asVar |> state.OfSum |> state.Catch with
          | Left otherVar ->
            let! otherKey = EquivalenceClasses.getKey otherVar
            let! otherClassToBeMerged = EquivalenceClasses.getVarClass otherKey otherVar
            do! EquivalenceClasses.deleteVarClass otherKey
            do! EquivalenceClasses.bindKeyVar key otherVar

            do!
              otherClassToBeMerged
              |> Seq.map (fun otherValue ->
                state {
                  do! EquivalenceClasses.Bind(var, otherValue)
                  return ()
                })
              |> state.All
              |> state.Map ignore
          | Right _ -> return ()

          do! EquivalenceClasses.updateVarClass key (Set.add value)

          return ()
      }
