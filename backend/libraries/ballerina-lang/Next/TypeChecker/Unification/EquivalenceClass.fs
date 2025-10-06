namespace Ballerina.DSL.Next

module EquivalenceClasses =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object

  type EquivalenceClass<'var, 'value when 'var: comparison and 'value: comparison> =
    { Representative: Option<'value>
      Variables: Set<'var> }

    static member Updaters =
      {| Representative =
          fun u (s: EquivalenceClass<'var, 'value>) ->
            { s with
                Representative = u s.Representative }
         Variables = fun u (s: EquivalenceClass<'var, 'value>) -> { s with Variables = u s.Variables } |}

    static member Empty: EquivalenceClass<'var, 'value> =
      { Representative = None
        Variables = Set.empty }

    static member FromValue: 'value -> EquivalenceClass<'var, 'value> =
      fun v ->
        { Representative = Some v
          Variables = Set.empty }

    static member FromVariable: 'var -> EquivalenceClass<'var, 'value> =
      fun v ->
        { Representative = None
          Variables = Set.singleton v }

    static member Create: Set<'var> * Option<'value> -> EquivalenceClass<'var, 'value> =
      fun (vars, rep) ->
        { Representative = rep
          Variables = vars }

  and EquivalenceClassValueOperations<'var, 'value when 'var: comparison and 'value: comparison> =
    { equalize:
        'value * 'value
          -> State<unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors>
      tryCompare: 'value * 'value -> Option<'value> }

  and EquivalenceClasses<'var, 'value when 'var: comparison and 'value: comparison> =
    { Classes: Map<string, EquivalenceClass<'var, 'value>>
      Variables: Map<'var, string> }

    static member Updaters =
      {| Classes = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Classes = c.Classes |> u }
         Variables = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Variables = c.Variables |> u } |}

    static member Empty: EquivalenceClasses<'var, 'value> =
      { Classes = Map.empty
        Variables = Map.empty }

    static member EnsureVariableExists (var: 'var) (classes: EquivalenceClasses<'var, 'value>) =
      if classes.Variables.ContainsKey var then
        classes
      else
        classes
        |> (EquivalenceClasses.Updaters.Variables(Map.add var $"{var}")
            >> EquivalenceClasses.Updaters.Classes(Map.add $"{var}" (EquivalenceClass.FromVariable var)))

    static member private tryGetKey(var: 'var, loc0: Location) =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Variables
          |> Map.tryFindWithError var "var" (var.ToString()) loc0
          |> state.OfSum
      }

    static member tryFind(var: 'var, loc0: Location) =
      state {
        let! key = EquivalenceClasses<'var, 'value>.tryGetKey (var, loc0)
        return! EquivalenceClasses.tryGetVarClass (key, loc0)
      }

    static member private getKey(var: 'var, loc0: Location) =
      state {
        let! key = EquivalenceClasses<'var, 'value>.tryGetKey (var, loc0) |> state.Catch

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
      (key: string, loc0: Location)
      : State<
          EquivalenceClass<'var, 'value>,
          EquivalenceClassValueOperations<'var, 'value>,
          EquivalenceClasses<'var, 'value>,
          Errors
         >
      =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Classes
          |> Map.tryFindWithError key "classes" (key.ToString()) loc0
          |> state.OfSum
      }

    static member private getVarClass
      (loc0: Location)
      (key: string)
      (var: 'var)
      : State<
          EquivalenceClass<'var, 'value>,
          EquivalenceClassValueOperations<'var, 'value>,
          EquivalenceClasses<'var, 'value>,
          Errors
         >
      =
      state {
        let! varClass = EquivalenceClasses.tryGetVarClass (key, loc0) |> state.Catch

        match varClass with
        | Left varClass -> return varClass
        | Right _ ->
          let initialClass = var |> EquivalenceClass.FromVariable
          do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key initialClass))

          return initialClass
      }

    static member TryDeleteFreeVariable
      (var: 'var, loc0: Location)
      : State<Unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! classes = state.GetState()

        if classes.Variables.ContainsKey var |> not then
          return ()
        else
          let! var_key = EquivalenceClasses.getKey (var, loc0)
          let! var_class = EquivalenceClasses.getVarClass loc0 (var_key) var

          match var_class.Representative with
          | Some representative ->
            return!
              (loc0,
               $"Error: cannot remove variable {var.ToString()} because it is not free - it has a representative {representative.ToFSharpString}")
              |> Errors.Singleton
              |> state.Throw
          | None ->
            if var_class.Variables.Count = 1 then
              do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.remove var_key))
            else
              return!
                (loc0,
                 $"Error: cannot remove variable {var.ToString()} because it is bound to other variables {var_class.Variables.ToFSharpString}")
                |> Errors.Singleton
                |> state.Throw
      }

    static member DeleteVariable
      (var: 'var, loc0: Location)
      : State<Unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! classes = state.GetState()

        if classes.Variables.ContainsKey var |> not then
          return ()
        else
          let! var_key = EquivalenceClasses.getKey (var, loc0)
          let! var_class = EquivalenceClasses.getVarClass loc0 var_key var
          do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.remove var_key))

          for otherVar in var_class.Variables do
            let! other_var_key = EquivalenceClasses.getKey (otherVar, loc0)
            let! other_var_class = EquivalenceClasses.getVarClass loc0 other_var_key otherVar

            let other_var_class =
              other_var_class |> EquivalenceClass.Updaters.Variables(Set.remove var)

            let other_var_class =
              other_var_class
              |> EquivalenceClass.Updaters.Representative(Option.orElse var_class.Representative)

            do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add other_var_key other_var_class))

          return ()
      }

    static member private mergeRepresentative
      (loc0: Location)
      (eqClass: EquivalenceClass<'var, 'value>)
      (value: 'value)
      : State<
          EquivalenceClass<'var, 'value>,
          EquivalenceClassValueOperations<'var, 'value>,
          EquivalenceClasses<'var, 'value>,
          Errors
         >
      =
      state {
        let! valueOperations = state.GetContext()

        match eqClass.Representative with
        | None ->
          return
            eqClass
            |> EquivalenceClass.Updaters.Representative(value |> Some |> replaceWith)
        | Some currentValue ->
          do! valueOperations.equalize (currentValue, value)

          let! winner =
            valueOperations.tryCompare (currentValue, value)
            |> sum.OfOption((loc0, $"Error: cannot compare {value} and {currentValue}") |> Errors.Singleton)
            |> state.OfSum

          return
            eqClass
            |> EquivalenceClass.Updaters.Representative(winner |> Some |> replaceWith)
      }
    //     { classes with
    //         Classes =
    //           classes.Classes
    //           |> Map.change key (function
    //             | Some c -> c |> u |> Some
    //             | None -> EquivalenceClass.Empty |> u |> Some) })

    static member private deleteVarClass key : State<unit, _, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun (classes: EquivalenceClasses<'var, 'value>) ->
        { classes with
            Classes = classes.Classes |> Map.remove key })

    static member Bind
      (var: 'var, varOrvalue: Sum<'var, 'value>, loc0: Location)
      : State<unit, EquivalenceClassValueOperations<'var, 'value>, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! key = EquivalenceClasses.getKey (var, loc0) // get the key associated with var or create a fresh key
        do! EquivalenceClasses.bindKeyVar key var // bind the key and the var (needed if the key is fresh)

        match varOrvalue with
        | Right value ->
          let! varClass = EquivalenceClasses.getVarClass loc0 key var
          let! varClass = EquivalenceClasses.mergeRepresentative loc0 varClass value
          do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key varClass))
        | Left otherVar ->
          let! varClass = EquivalenceClasses.getVarClass loc0 key var

          if varClass.Variables.Contains otherVar then
            return ()
          else
            let varClass = varClass |> EquivalenceClass.Updaters.Variables(Set.add otherVar)
            do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key varClass))
            let! otherKey = EquivalenceClasses.getKey (otherVar, loc0)
            do! EquivalenceClasses.bindKeyVar key otherVar
            let! otherClassToBeMerged = EquivalenceClasses.tryGetVarClass (otherKey, loc0) |> state.Catch

            match otherClassToBeMerged with
            | Right _ ->
              // let! s = state.GetState()
              // do Console.WriteLine($"{var} and {otherVar} in {s.ToFSharpString}")
              // do Console.ReadLine() |> ignore
              return ()
            | Left otherClassToBeMerged ->
              do!
                otherClassToBeMerged.Variables
                |> Seq.map (fun otherValue ->
                  state {
                    do! EquivalenceClasses.Bind(var, otherValue |> Left, loc0)
                    return ()
                  })
                |> state.All
                |> state.Map ignore

              do! EquivalenceClasses.deleteVarClass otherKey

              match otherClassToBeMerged.Representative with
              | None -> return ()
              | Some value ->
                let! varClass = EquivalenceClasses.getVarClass loc0 key var
                let! varClass = EquivalenceClasses.mergeRepresentative loc0 varClass value
                do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key varClass))

      }
