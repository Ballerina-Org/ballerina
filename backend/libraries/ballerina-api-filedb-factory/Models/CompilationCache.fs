namespace Ballerina.API.MemoryDB

module CacheCompilation =
  open System
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  let private compilationCacheLock = obj ()

  type TenantData =
    { Tenant: Guid
      Schema: string
      IsDraft: bool }

  type CompilationContext<'runtimeContext, 'valueExt when 'valueExt: comparison>
    =
    { TypeCheckContext: TypeCheckContext<'valueExt>
      TypeCheckState: TypeCheckState<'valueExt>
      EvalContext: ExprEvalContext<'runtimeContext, 'valueExt>
      EvalResult: Value<TypeValue<'valueExt>, 'valueExt> }

  type CompilationCache<'runtimeContext, 'valueExt when 'valueExt: comparison> =
    { mutable Cache:
        Map<TenantData, CompilationContext<'runtimeContext, 'valueExt>> }

    static member Empty: CompilationCache<'runtimeContext, 'valueExt> =
      { Cache = Map.empty }

    member this.TryFind(tenant, schema, isDraft) =
      lock compilationCacheLock (fun () ->
        this.Cache
        |> Map.tryFind
          { Tenant = tenant
            Schema = schema
            IsDraft = isDraft })

    member this.Add (tenant, schema, isDraft) compilationContext =
      lock compilationCacheLock (fun () ->
        this.Cache <-
          this.Cache.Add(
            { Tenant = tenant
              Schema = schema
              IsDraft = isDraft },
            compilationContext
          ))

    member this.Remove(tenant, schema, isDraft) =
      lock compilationCacheLock (fun () ->
        this.Cache <-
          this.Cache.Remove(
            { Tenant = tenant
              Schema = schema
              IsDraft = isDraft }
          ))
