namespace Ballerina.API

module Filter =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Routing
  open System
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.DSL.Next.StdLib.Extensions
  open APIUtils
  open Microsoft.AspNetCore.Http
  open System.Text.Json

  type EntityFilterResult =
    { EntityId: string
      JsonValue: ValueDTO<ValueExtDTO> }

  type EntityFilterFunction =
    string -> int -> int -> JsonElement -> Sum<EntityFilterResult list, Errors<unit>>

  let filter<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (_context: APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    (getFilterFunction: 'tenantId -> 'schemaName -> Sum<EntityFilterFunction, Errors<Location>>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/filter",
      Func<HttpContext, 'tenantId, 'schemaName, string, int, int, JsonElement, IResult>
        (fun _httpContext tenantId schemaName entityName (offset: int) (limit: int) filterBody ->
          let result =
            sum {
              let! filterFn =
                getFilterFunction tenantId schemaName
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! filterResults =
                filterFn entityName offset limit filterBody
                |> sum.MapError(fun errors ->
                  { Errors = errors |> Errors.MapContext(fun () -> Location.Unknown)
                    TypeError = None })

              return
                filterResults
                |> List.map (fun r ->
                  {| Key = r.EntityId
                     Value = r.JsonValue |})
            }

          apiResponseFromSum result id)
    )
    |> ignore
