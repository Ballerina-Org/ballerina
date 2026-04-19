namespace Ballerina.API

open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Serialization.PocoObjects
open Ballerina.Errors
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Terms.FastEval
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.Fun
open Microsoft.AspNetCore.Http

type APITypeError<'runtimeContext, 'db, 'customExtension
  when 'customExtension: comparison and 'db: comparison> =
  { ExpectedType: TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>
    TypeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>
    TypeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>
    LanguageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       > }

type APIError<'runtimeContext, 'db, 'customExtension, 'context
  when 'customExtension: comparison and 'context: comparison and 'db: comparison>
  =
  { Errors: Errors<'context>
    TypeError: Option<APITypeError<'runtimeContext, 'db, 'customExtension>> }

  static member Create(errors: Errors<'context>) =
    { Errors = errors; TypeError = None }

type APIErrorResponse =
  { Errors: string[]
    Examples: ValueDTO<ValueExtDTO>[] }

type DbDescriptor<'runtimeContext, 'db, 'customExtension
  when 'customExtension: comparison and 'db: comparison> =
  { TypeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>
    TypeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>
    EvalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >
    DbExtension:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >
    LanguageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       > }

type TenantDescriptor<'tenantId, 'schemaName> =
  { TenantId: 'tenantId
    SchemaName: 'schemaName }

type APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
  when 'customExtension: comparison and 'db: comparison> =
  { DbDescriptorFetcher:
      'tenantId
        -> 'schemaName
        -> Sum<
          DbDescriptor<'runtimeContext, 'db, 'customExtension>,
          Errors<Location>
         >
    PermissionHookInjector:
      HttpContext
        -> Updater<
          ExprEvalContext<
            'runtimeContext,
            ValueExt<'runtimeContext, 'db, 'customExtension>
           >
         > }
