namespace Ballerina.DSL.Expr.Types

#nowarn FS0060

module Unification =
  open System
  open Ballerina.Fun
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model

  type UnificationConstraints = { Equalities: Set<VarName * VarName> }

  type UnificationConstraints with
    static member Zero() =
      { UnificationConstraints.Equalities = Set.empty }

    static member Add (v1: VarName, v2: VarName) (constraints: UnificationConstraints) : UnificationConstraints =
      { constraints with
          Equalities = constraints.Equalities |> Set.add (v1, v2) |> Set.add (v2, v1) }

    static member (+)
      (constraints1: UnificationConstraints, constraints2: UnificationConstraints)
      : UnificationConstraints =
      { Equalities = constraints1.Equalities + constraints2.Equalities }

    static member Singleton(v1: VarName, v2: VarName) : UnificationConstraints =
      UnificationConstraints.Zero() |> UnificationConstraints.Add(v1, v2)

    static member ToEquivalenceClasses(constraints: UnificationConstraints) : List<Set<VarName>> =
      let mutable result: Map<VarName, Set<VarName>> = Map.empty

      for (v1, v2) in constraints.Equalities do
        let v1Equivalence =
          (result |> Map.tryFind v1 |> Option.defaultWith (fun () -> Set.empty))
          + Set.singleton v2

        let v2Equivalence =
          (result |> Map.tryFind v2 |> Option.defaultWith (fun () -> Set.empty))
          + Set.singleton v1

        let newJoinedEquivalence = v1Equivalence + v2Equivalence

        let modifiedConstraints =
          newJoinedEquivalence
          |> Set.toSeq
          |> Seq.map (fun v -> v, newJoinedEquivalence)
          |> Map.ofSeq

        result <- result |> Map.merge (fun _ newConstraint -> newConstraint) modifiedConstraints

      result |> Map.values |> Set.ofSeq |> Set.toList

  type ExprType with
    static member Unify
      (tvars: TypeVarBindings)
      (typedefs: Map<TypeId, ExprType>)
      (t1: ExprType)
      (t2: ExprType)
      : Sum<UnificationConstraints, Errors> =
      let (=?=) = ExprType.Unify tvars typedefs

      sum {
        match t1, t2 with
        | ExprType.UnitType, ExprType.UnitType -> return UnificationConstraints.Zero()
        | ExprType.LookupType l1, ExprType.LookupType l2 when l1 = l2 -> return UnificationConstraints.Zero()
        | ExprType.LookupType l1, ExprType.LookupType l2 when l1 <> l2 ->
          return! sum.Throw(Errors.Singleton($"Error: types {t1} and {t2} cannot be unified under typedefs {typedefs}"))
        | ExprType.VarType v1, ExprType.VarType v2 ->
          match tvars |> Map.tryFind v1, tvars |> Map.tryFind v2 with
          | Some v1, Some v2 ->
            if v1 = v2 then
              return UnificationConstraints.Zero()
            else
              return! sum.Throw(Errors.Singleton($"Error: types {t1} and {t2} cannot be unified"))
          | _ -> return UnificationConstraints.Singleton(v1, v2)
        | t, ExprType.LookupType tn
        | ExprType.LookupType tn, t ->
          match typedefs |> Map.tryFind tn with
          | None ->
            return!
              sum.Throw(
                Errors.Singleton($"Error: types {t1} and {t2}/{t} and {tn} cannot be unified under typedefs {typedefs}")
              )
          | Some t' -> return! t =?= t'
        | ExprType.ListType(t1), ExprType.ListType(t2)
        | ExprType.SetType(t1), ExprType.SetType(t2)
        | ExprType.OptionType(t1), ExprType.OptionType(t2)
        | ExprType.OneType(t1), ExprType.OneType(t2) -> return! t1 =?= t2
        | ExprType.TableType(t1), ExprType.ManyType(t2)
        | ExprType.ManyType(t1), ExprType.TableType(t2)
        | ExprType.ManyType(t1), ExprType.ManyType(t2) -> return! t1 =?= t2
        | ExprType.MapType(k1, v1), ExprType.MapType(k2, v2) ->
          let! partialUnifications = sum.All([ k1 =?= k2; v1 =?= v2 ])
          return partialUnifications |> Seq.fold (+) (UnificationConstraints.Zero())
        | ExprType.SumType(l1, r1), ExprType.SumType(l2, r2) ->
          let! partialUnifications = sum.All([ l1 =?= l2; r1 =?= r2 ])
          return partialUnifications |> Seq.fold (+) (UnificationConstraints.Zero())
        | ExprType.TupleType([]), ExprType.TupleType([]) -> return UnificationConstraints.Zero()
        | ExprType.TupleType(t1 :: ts1), ExprType.TupleType(t2 :: ts2) ->
          let! partialUnifications = sum.All([ t1 =?= t2; ExprType.TupleType(ts1) =?= ExprType.TupleType(ts2) ])

          return partialUnifications |> Seq.fold (+) (UnificationConstraints.Zero())
        | ExprType.TupleType(_), ExprType.TupleType(_) ->
          return! sum.Throw(Errors.Singleton($"Error: tuples of different length {t1} and {t2} cannot be unified"))
        | ExprType.UnionType(cs1), ExprType.UnionType(cs2) when cs1 |> Map.isEmpty && cs2 |> Map.isEmpty ->
          return UnificationConstraints.Zero()
        | ExprType.UnionType(cs1), ExprType.UnionType(cs2) ->
          match cs1 |> Seq.tryHead with
          | Some t1 ->
            match cs2 |> Map.tryFind t1.Key with
            | Some t2 ->

              let! partialUnifications =
                sum.All(
                  [ t1.Value.Fields =?= t2.Fields
                    ExprType.UnionType(cs1 |> Map.remove t1.Key)
                    =?= ExprType.UnionType(cs2 |> Map.remove t1.Key) ]
                )

              return partialUnifications |> Seq.fold (+) (UnificationConstraints.Zero())
            | _ -> return! sum.Throw(Errors.Singleton($"Error: cases {cs1} and {cs2} cannot be unified"))
          | _ ->
            return! sum.Throw(Errors.Singleton($"Error: unions of different length {t1} and {t2} cannot be unified"))
        | ExprType.RecordType(m1), ExprType.RecordType(m2) when m1 |> Map.isEmpty && m2 |> Map.isEmpty ->
          return UnificationConstraints.Zero()
        | ExprType.RecordType(m1), ExprType.RecordType(m2) ->
          match m1 |> Seq.tryHead with
          | None ->
            return! sum.Throw(Errors.Singleton($"Error: records of different length {t1} and {t2} cannot be unified"))
          | Some first1 ->
            let m1 = m1 |> Map.remove first1.Key

            match m2 |> Map.tryFind first1.Key with
            | None -> return! sum.Throw(Errors.Singleton($"Error: record fields {t1} and {t2} cannot be unified"))
            | Some first2 ->
              let m2 = m2 |> Map.remove first1.Key

              let! partialUnifications =
                sum.All([ first1.Value =?= first2; ExprType.RecordType(m1) =?= ExprType.RecordType(m2) ])

              return partialUnifications |> Seq.fold (+) (UnificationConstraints.Zero())
        | ExprType.VarType _, _
        | _, ExprType.VarType _ -> return UnificationConstraints.Zero()
        | _ ->
          if t1 = t2 then
            return UnificationConstraints.Zero()
          else
            return! sum.Throw(Errors.Singleton($"Error: types {t1} and {t2} cannot be unified"))
      }
