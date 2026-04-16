namespace Ballerina.API

module APIRegistration =
  open Microsoft.AspNetCore.Routing
  open Create
  open Delete
  open Linking
  open Read
  open Upsert
  open Ballerina.Collections.Sum
  open Update
  open OpenAPI
  open Filter
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types

  type RouteGroupBuilder with
    member builder.RegisterAPIEndpoints<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
      when 'customExtension: comparison and 'db: comparison>
      (apiRegistrationFactory:
        APIRegistractionFactory<
          'runtimeContext,
          'db,
          'customExtension,
          'tenantId,
          'schemaName
         >)
      (getFilterFunction: 'tenantId -> 'schemaName -> bool -> Sum<EntityFilterFunction, Errors<Location>>)
      =
      sum {
        let! apiContext =
          APIContext.Create
            apiRegistrationFactory.LanguageContextFactory
            apiRegistrationFactory.DbDescriptorFetcher
            apiRegistrationFactory.PermissionHookInjector

        do
          create<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          delete<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          link<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          get<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          lookup<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          upsert<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          update<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          openApi<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext

        do
          filter<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>
            builder
            apiContext
            getFilterFunction
      }
