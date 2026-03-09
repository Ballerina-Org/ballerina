namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Runners.Project
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member TypeCheckString<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO
      when 'valueExt: comparison
      and 'valueExtDTO: not null
      and 'valueExtDTO: not struct
      and 'deltaExtDTO: not null
      and 'deltaExtDTO: not struct>
      (
        { TypeCheckContext = typeCheckContext
          TypeCheckState = typeCheckState }:
          LanguageContext<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO>,
        _db_query_sym,
        _make_db_query_type
      )
      (program: string)
      : Sum<
          Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt> * TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      let cache = memcache (typeCheckContext, typeCheckState)

      let files =
        NonEmptyList.OfList(FileBuildConfiguration.FromFile("input.bl", program), [])

      let project = { Files = files }

      sum {
        let! typeCheckedExprs, programType, _, typeCheckState =
          ProjectBuildConfiguration.BuildCached _db_query_sym _make_db_query_type cache project

        match typeCheckedExprs with
        | NonEmptyList(expr, []) -> expr, programType, typeCheckState
        | NonEmptyList(_, _) ->
          return! sum.Throw(Errors.Singleton Location.Unknown (fun () -> "Expected one type checked expression"))
      }
