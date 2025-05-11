
# Todo (✅/❌)

  ❌ form validator and code generator
    ❌ break the form engine in all possible ways and ensure good errors arise
      ❌ define tests, one minimal spec for anything that can go wrong
        ❌ invalid launchers
          ❌ non-existing form
          ❌ missing entity API
          ❌ missing config API
          ❌ mismatch form.type vs api.type
          ❌ mismatch configApi.type vs predicate api implicit type
          ❌ missing config type
        ❌ invalid nested renderers
          ❌ non-existing field
          ❌ wrong predicate `root` type inside the nested renderer
        ❌ invalid discriminated union renderers
          ❌ missing cases
          ❌ non-existent case
          ❌ case arg and chosen renderer type mismatch
        ✅ actual person-config as a reference of something that should work
          ❌ add a few correct instances of visiblity predicates using `matchCase`
          ❌ add one that matches `Some`, `None` over a `SingleSelection`
          ❌ add one that matches `Some`, `None` over an `Option`
          ❌ add one that matches `Left`, `Right` over a `Sum`
          ❌ add validation predicate to a switched form renderer and ensure that the `root` type is the narrowed one
        ❌ add invalid form `extends` clauses
          ❌ missing forms
          ❌ wrong format of the clause itself
        ❌ add invalid form `cases` clauses
          ❌ missing cases on either side
          ❌ mismatched type of cases in renderer vs type
          ❌ wrong use of `Sum`
        ❌ add invalid tabs type of computed visible fields
        ❌ add entity PATCH sample to successful samples
        ❌ add errorless Go compilation to tests
        ❌ make sure that the failing tests fail for the right reason - inspect and partially match the errors
    ✅ custom types unify because they are all collapsed to Unit, generate some unique type underwater
    ✅ prepare sample with detail view, isFilterable, isSortable
      ✅ C1A | C1B | C1C = record    config for C1
      ✅ C2A | C2B | C2C          config for C2
    ✅ parser
      ✅ apis
        ✅ one
        ✅ many
        ✅ add to top level, merge, and push out the resulting merged JSON
      ✅ option renderer
      ✅ table
        ✅ detail view
          ✅ containerRenderer
        ✅ "isFilterable", "isSortable" on columns
        ✅ disabled columns via disabled:true (ensure it parses and typechecks normally)
        ✅ containerRenderer
      ✅ form
        ✅ containerRenderer
    ✅ validation
      ✅ table
        ✅ detail view
        ✅ fields of type `One`, `Many` require a `one` or `many` api
        ✅ a `Many` field may bind to a `Table` renderer
        ✅ a TableFormRenderer uses the right API
        ✅ a ManyFormRenderer uses the right API
        ✅ a ManyFormRenderer detail type and the API lookup type are the same
    ✅ visibility predicates
      ✅ visible and disabled inside columns or detail view are not validated
      ✅ inside a detail view renderers we validate with global, root, local
      ✅ inside a column renderer we validate with global, root, but neither row nor local
    ✅ one
      ✅ api
        ✅ methods:[getMany, update, create, delete]
          ✅ always generate GET:Id -> E x F
          ✅ if there is getMany, generate `GETMany:Id -> E x Table[F]`
          ✅ only if at least one between update, create, delete, generate PATCH:Id x DeltaF -> E
            ✅ PATCH works on `DeltaOne[A, DeltaA]`, which also includes `Create` and `Delete`
          ✅ disambiguate names of callbacks uniquely as `LookupEntityName__OneName`
      ✅ renderer
        ✅ `preview`
          ✅ requires that the API has `getMany`
          ✅ validate exactly like `details`
      ✅ validation and predicate validation are broken, just recurse properly in both details and preview (if available)
    ✅ lookup streams
      ✅ always generate GETMany:Id x SearchParams -> E x F
      ✅ validation
        ✅ require `Id` and `DisplayName`
    ✅ many
      ✅ ManyRenderer is just not instantiated or parsed at all apparently
      ✅ api
        ✅ methods:[update, create, delete, getManyUnlinked]
          ✅ always generate GETMany:Id x SearchParams -> E x Table[F]
          ✅ only if there is method getManyUnlinked generate GETManyUnlinked:Id x SearchParams -> E x Table[F]
          ✅ only if at least one between update, create, delete, generate PATCH:Id x DeltaMany[F, DeltaF] -> E
      ✅ renderer
        ✅ `preview`
          ✅ requires that the API has `getManyUnlinked`
          ✅ validate exactly like `details`
      ✅ validation and predicate validation are broken, just recurse properly in both details and preview (if available)
      ✅ validation
        ✅ always require `Id`
    ✅ define a proper `One` and `DeltaOne` with methods `Create, Delete, Link, Unlink` as well
      ✅ use it in the ONE PATCH
      ✅ use it in the sample go-config
      ✅ make sure it's used as codegen target
    ✅ define a proper `Many` and  `DeltaMany` with methods `Create, Delete, Link, Unlink` as well
      ✅ use it instead of `Table`
      ✅ use it in the MANYPATCH
      ✅ use it in the sample go-config
    ❌ add a `visibleCases` (optional) tab to the union renderers
    ❌ just like `disabled` and `visible`, add an optional `global` expr to map the readonly context to a field
      ❌ this changes the `globalType` during `ValidatePredicate`
    ❌  | SumRenderer of
        {| Sum: Renderer  ---->  string
          Left: NestedRenderer
          Right: NestedRenderer
        //Children: RendererChildren
        |}
    ✅ parse records - add renderer to the go config which must map the name of the renderer to the array of the fields that the renderer processes
      ✅ renderer is optional
    ✅ make sure that whenever a type is used for a lookup, that the root type has an id
    ✅ parse nested forms at the renderer level
      ✅ parse lookup APIs in streams
      ✅ remove the inline union renderer parser
      ✅ validate does not check that the type of the stream is equal to the type of the renderer and is equal to the type of the field
        ✅ straightforward streams
        ✅ lookup streams
      ✅ does validation catch the wrong field renderer type?
      ✅ test that tables parse as inline forms correctly
      ✅ test that unions parse as inline forms correctly
      ✅ test that the inline union form renderer is one of the `Union` `SupportedRenderer`s from the goconfig
    ✅ add streams to Lookup Apis
    ✅ plenty of nonsense `Id` fields in enumApiId, streamApiId, etc.
    ❌ cleanup
      ❌ harmonize `Table` codegen to `Many`
        ❌ api
          ❌ methods:[update, create, delete]
            ❌ always generate GETMany:Id -> Table[E]
            ❌ only if at least one between update, create, delete, generate PATCH:Id x DeltaE
      ❌ harmonize renderers
        ❌ add table, record, union Renderer cases
        ❌ remove InlineRenderer, as well as the "kind-of-inline-but-not-quite" renderers like `TableFormRenderer`
        ❌ a top-level renderer is a TypeId x Renderer
      ❌ form parser file is too long
        ✅ renderer.type should be in patterns, not runner
        ✅ split off the renderers parsers
        ✅ split off the expr parsers
        ✅ split off the exprtype parsers
        ❌ split off the api parsers
        ❌ split off the main parsers
        ✅ split off the ExprType decomposition patterns
      ❌ BE `noop`s should be eliminated—anything with a const should be made into a delta unit/delta never
      ❌ recursive type definitions in the writers do not work, but the recursion actually just needs the delta type of the nested writer (which might also loop?)
      ❌ FormRenderer, TableFormRenderer, ManyFormRenderer should not lug around an ExprType, it is probably just cached
      ❌ add monadic operator (>>)
      ❌ are we checking that the stream and enum apis match the type of the field?
      ❌ use `OneApiId`, `ManyApiId`, etc. as type-safe keys in `OneRenderer` and `ManyRenderer`
        ❌ also `ManyApiId` should be used in `ManyFormRenderer`
      ❌ use `EnumApiId`, `StreamApiId`, etc. as type-safe keys in the `FormConfigApis`
      ❌ there is a lot of repetition in the validation of the visibility fields selector
        ❌ basically from Expr -> EnumCases (so excluding proper unions) is a reusable ExprType.AsEnumCases
      ❌ FormDeclarationType should be a TypeId/TypeName, because at that level of the form the type given is a name <- more type safety
    ❌ entities PATCH - gets single value and path of change
      ❌ remove legacy references to the Writers, it is all about deltas
      ❌ the BE can guide by providing deserializer callbacks for all the required types, such as `tryParseAsUpdateableEntityX:Raw -> UpdateableEntityX`
    ❌ add documentation (Confluence)
      ❌ high-level workings of the form engine
      ❌ high-level anatomy of a spec
      ❌ about the generated BE code
      ❌ add readme with runner instructions to ballerina-core
      ❌ cleanup the unnecessary modules such as OAuth2 (should be moved to apps)
    ❌ `extends`
      ❌ errors when extending a non-existent type are terrible
      ❌ test transitive/multiple extensions
      ❌ give errors in case of extensions of placeholder types
      ❌ add `extends` statement to unions and enums
    ❌ `requiredImport` is an optional either of one or multiple strings (`Time` needed this)
    ❌ lists, sets, etc. should be top-level forms
    ❌ Custom types should not generate writer fields
    ❌ no more primitives, only custom types
      ❌ custom types should have their algebra of operators
    ❌ allow using tuple renderers with records
    ❌ add lazy fields
    ❌ add union renderers
      ❌ as top-level forms as well as nested
    ❌ why do codegen errors (related to the writers) show up twice?
    ❌ add a renderer decorator to forms
      ❌ get rid of tuple renderer
    ❌ improvements to the generated Golang code
      ❌ add unit tests for all the go Deltas
      ❌ generate constructors with the `new{ ... }` syntax, not the field-by-field assignment
      ❌ make the deltas encapsulated
        ❌ use `private_` as a prefix for the patterns
        ❌ generate marshall and unmarshall for the generated deltas
        ❌ write marshall and unmarshall for the generated deltas
      ❌ generate more than one file
      ❌ toTypeAnnotation in ExprType.ToWriter could benefit from a better context mapper
      ❌ the injected types generate unused delta types
      ❌ all delta and writer type names should be sanitized
    ❌ the validator is now mature
    ❌ define live webservice variant
      ❌ in separate repo
        ❌ move the Golang codegen there after extracting the intermediate representation for the codegen
      ❌ database
        ❌ syntax
        ❌ generated models and dbcontext in F# if migrations work, C# otherwise
        ❌ with conversion to and from `Value`
        ❌ with conversion of `Include`-heavy LINQ queries to and from `Expr`
        ❌ api to and from `Value`, `Expr`
        ❌ migration/fork-the-world strategy
        ❌ multicast writing to old reference entities
        ❌ permanently store sources in DB
      ❌ update rules and execution engine
        ❌ table of rules, saved in jsonb
      ❌ OData (AST-only) API generator
    ❌ disallow unsupported keywords (`visibIle` wasted me a good chunk of time)
    ❌ gently deprecate "defaultX" naming convention
    ❌ make recursive `Value/Expr` types work in Go
    ❌ add custom generics with their renderer (for example, `WithEvidenceAndApproval<value>`)
    ❌ support multiple imports or none at all
    ❌ add custom operators to codegenConfig
    ❌ refactor `sprintf` instances in `typeCheck.fs`
    ❌ add `sum.Map`, remove `Sum.Map` references (ugly and inconsistent wrt `state.Map`)
    ❌ add `sum.For`, use it instead of `sum.All >> sum.Map ignore`
    ❌ `sum.fromOption` is inconsistent, should be `Sum.ofOption`
    ❌ `state.OfSum` is inconsistent, should be `State.ofSum`
    ❌ paths inside `Any` have a priority after a partial match is found - `Any` filters errors lower than the highest priority
      ❌ add to each `Any`, streamline operators
        ❌ parser
        ❌ validator
    ❌ add state.TryFindX for enum, form, stream, and so on like for `state.TryFindType`
      ❌ use it in the codegen
      ❌ why can't we use state.TryFindType in the validators?
      ❌ Validate passes the context as a parameter instead of using GetContext - ugly
    ❌ remove all the nonsense `Guid`s from id types, the wrapped name is enough
    ❌ create gui editor as an instance of a form itself
      ❌ form specification
      ❌ package in a separate (private) repo
        ❌ served by an F# backend
        ❌ with access to folder IDE-style
        ❌ with enums and streams (defined in F#) based on the available types ATM
    ❌ release ballerina to nuget
    ✅ accept empty `apis` blocks
    ✅ add `extends` keyword for forms
    ❌ add lots of operators to lists, sets, maps, etc.
    ❌ the tool is now complete
    ❌ the codegen to Golang needs to be improved
      ❌ ballerina-core-go should be split into further packages, one per data structure. `MapOption` should just be `Option.Map`
      ❌ define intermediate structure
      ❌ split up in files
      ❌ as well as the `SeqState` monad
    ❌ the core library needs more structural cleanup: Queries, Range, etc. should go into `webapi` or a similar domain
    ❌ help command
    ❌ expr and expr type parsing and validation must go to their respective meta-modules
    ❌ generate Typescript and C# code from forms-config
    ❌ extensibility of primitives as existentially-typed algebras
    ❌ improve the syntax of the form config with fslex and fsyacc
    ✅ codegen the `import` command with some sort of linking strategy for shared files
    ❌ consider dropping `Children` if we do not use it
    ✅ allow union types (needs adjustment in frontend too)

  ❌ make validator partial
  ❌ show both data-sync and type-safe forms in FormsApp
  ❌ check that we can inspect a local DB (MySQL, Redis, ES, Postgres)
  ❌ expose ballerina-core as a nuget package
  ❌ expose ballerina-runtime as a nuget package
  ❌ registration, etc. coroutines
    ✅ serialize/deserialize F# unions and records with endpoints
    ✅ define CRUD 'a
    ✅ define AB, ABEvent, ABEvent_CLI
    ✅ define DBSets in DBContext
    ✅ run migrations
    ✅ move CRUD 'a to separate project
    ✅ move updater U<'s> to separate file
    ❌ cleanup
      ❌ create builder and app only after parsing the command line parameters
      ✅ move sample coroutines
      ✅ move sample dbcontext stuff
        ✅ resolve dbcontext with DI
        ✅ pass config from appsettings.development (based on env variable)
      ✅ move AB sample
      ❌ add PositionOptions config with extension method on builder
      ❌ adding and configuring the dbcontext should also be done from another file
    ✅ AB events sample domain
      ✅ reorganize the main project decently, moving all the AB-related logic to a separate folder
        ✅ repositories
        ✅ endpoints
      ✅ define ABRepo, ABEventRepo
        ✅ generalize from a single DbSet
        ✅ add ordering - requires making CRUD an interface
        ✅ add skip/take
          ✅ add url parameters to endpoint, check that they work also in OpenAPI
          ✅ add "safety clamp" to avoid DDoS
      ✅ post ABEvent, AB via repos
        ✅ expose OpenAPI spec
          ✅ type discriminator in swagger
          ✅ type discriminator when serializing
          ✅ type discriminator when deserializing
      ❌ define and run ABCoroutine
        ✅ separate entry point with own executable based on cmd-line arguments
          ✅ check the cmd line parameters with System.CommandLine
            ✅ shell script for `dotnet run -- mode web & dotnet run -- mode jobs`
            ✅ isolate thread evaluator, move to separate file in main project
            ✅ fix the ugly dancing ids of the active coroutines, they should remain the same
        ✅ do not delete events, mark them as done, with an index on the status
          ✅ add timestamp to events, process in creation order
          ✅ allow adding new events from coroutines
          ✅ POST should overwrite CreatedAt and ProcessingStatus
            ✅ even better, POST should not expect CreatedAt and ProcessingStatus (deserializer)
        ✅ add environment to the ./startup-be.sh launcher, otherwise we are using the wrong appsettings!!!
        ✅ processBs
        ✅ separate state = Unit from context = ABContext
        ✅ extract the general purpose evaluator from jobs.fs
          ✅ parameterize the event queries with a CRUD repo
        ✅ endpoint to push AB events
          ✅ expose OpenAPI spec
        ✅ "blogging" context -> BallerinaContext
        ✅ implement Repeat as a reified construct
        ✅ AB jobs can be separated fully to the AB module with minimal dependencies
        ❌ refactor `async` to `task`
        ❌ BUG ALERT if translating to production! The Map of events will likely cause events not to be processed in create-order
        ❌ test Spawn
          ❌ remove ugly starting logic, everything is bootstrapped by the endpoint that pushes the CreateABEvent
          ❌ create ABs from event
          ❌ when the AB coroutine ends, it is respawned (migration-friendly strategy)
        ✅ restore Async in Crud, run with Await
        ✅ dependent on repo's for AB and ABEvent
        ✅ co.On for ABEvent should check that the ABId matches or every ABEvent will match every co.On
        ✅ test Any
      ✅ project refactoring
        ✅ move all the AB-related logic to the right project
        ✅ rename grandeomega to web
    ❌ define sample Positions API and types
      ❌ define expr evaluator
        ✅ basic eval expr
        ✅ basic eval assignment
        ❌ cleanup
          ✅ the schema definition should be in a separate file than the context
          ✅ allFields should be the flattening of allFields of each entity
          ✅ move the various merge* utilities to extension methods
          ✅ rename `XCount`, etc. to just `X`
          ❌ the field descriptor definitions should use the operations from other field descriptors, and not perform any comparisons to entity descriptors Ids
            ✅ Get should be based on Lookup
            ❌ AB and CD should be based on generic containers of fields and nested entities, not real types
              ✅ the lookup of fields from ABs and CDs in the definition of the AB/CD entity schema should use the field definition lookup recursively
              ❌ the assignment (`Update`) of fields to ABs and CDs in the definition of the AB/CD entity schema should use the field definition assignment recursively
          ❌ there are various places where we assume `One entityId`, is this always reasonable?
            ❌ in particular, `Expr::execute` does not take into account more than one field lookup on the assigned variable, extend
      ❌ testing scenario
        ❌ define an algebra of business rules to support cascades like for the GLAccount defaulting
        ❌ add `CDs` to `AB`, so not just one
          ✅ use business rules for field setting
          ✅ test the conditions, not always `Exists ... true`
          ✅ introduce .System -> .User as prio, apply it to the set field events
          ✅ modify all field events to the new structure based on business rule payloads
          ❌ complete the scenario of multiple CDs
            ❌ fix the wrong order of processing of events
            ❌ add `AB_CD` relation entity, remove direct `AB-CD` references
              ❌ `AB_CD = { AB_CDId; ABId; CDId; IndexOfCDInAB }`
            ❌ `Exists ab_cd in AB_CD | ab_cd.ABId = <ab1.ABId> && Exists cd in CD | cd.CDId = ab_cd.CDId |= [cd.EF := <ef1.EFId>]`
            ❌ the `&&` operator propagates variables from both operands
            ❌ the `||` operator propagates the intersection of variables with the same type from both operands
            ❌ `Exists ab in AB | true |= [ab.Total1 := sumBy(Exists ab_cd in AB_CD | ab_cd.ABId = <ab1.ABId> && Exists cd in CD | cd.C + cd.D)]`
            ❌ `Exists cd = new CD(...) | Exists ab_cd = new AB_CD(<ab1.ABId>, cd.CDId) | true |= []`
              ❌ all fields of new entities are marked as modified
            ❌ `Exists ab_cd1 | ab_cd1.ABId = <ab1.ABId> && ab_cd1.CDId = <cd1.CDId> && Exists ab_cd2 | ab_cd2.ABId = <ab1.ABId> && ab_cd1.CDId = <cd2.CDId> | true |= [ab_cd1.IndexOfCDInAB = 2; ab_cd2.IndexOfCDInAB = 1]`
        ❌ we don't need `One` anymore, do we? Let's move all to `Multiple`
        ❌ rename `positions` to `abcd`
        ❌ add an enum parameter to pick the edit to test
      ❌ make it production-ready
        ❌ define the values of AB, CD, etc. as instances of `Value`
          ❌ to base everything on Value, Fields and lookup types need an ID, not just the name
            ❌ then we can deprecate FieldId, EntityId, etc.
        ❌ do not commit the updates to the context immediately, output a set of field value changes
          ❌ the context becomes a cache of operations
          ❌ output the applied rules for the visibility/explainability/logging
          ❌ monadically
        ❌ expose OpenAPI
            ❌ [Swagger](https://www.nuget.org/packages/FSharp.SystemTextJson.Swagger)
            ❌ use the dynamic schema internally, but a statically typed `Expr` schema externally
        ❌ test with
          ❌ a few thousands ABs, CDs, EFs
          ❌ a dozen rules on many field "clusters"
        ❌ the construction of field descriptors in the schema could be streamlined
        ❌ the Metadata entities belong to the model, not to Ballerina
        ❌ all rules should be applied on all entities after creation of a new entity
        ❌ BUG ALERT if translating to production! The Map of events will likely cause events not to be processed in create-order
        ❌ allow approval, with associated business rules
        ❌ deal with missing references (GUIDs that do not match an existing entity)
        ❌ introduce list monad with errors for eval/execute
          ❌ return useful error messages
        ❌ the resolution of top-level existentials like `exists ab in AB | ab.ABId = CONSTGUID` should be resolved much faster
        ❌ implement lazy fields in the schema
          ❌ this requires a coroutine-mediated protocol
          ❌ openSession(schema) -> operations | closeSession()
          ❌ when the schema is fully dynamic, get/update operations work with reflection/`Dynamic` CLR type
        ❌ consider flipping the relations around, using arrays/maps instead of relations
        ❌ -----at this point, the prototype can be considered reasonably done and could go live as a microservice-----
        ❌ the evaluation of existentials with conditions over foreign keys can be ran very quickly with lookup tables
        ❌ efficiently: with pre-caching of the FREE-VARS of both condition and expression value
        ❌ enums to strings
      ❌ PROTOTYPE 2 - DB in PG with CRUD OpenAPI
        ❌ performance test
        ❌ add memcache after write operations
      ❌ -----at this point, the second prototype can be considered reasonably done and could go live as a microservice-----
      ❌ improve DSL for type-safe business rule and expression definition in F#
        ❌ group field definitions and entity definitions under anonymous records for aesthetics and scoping in case of multiple fields with the same name in a different entity
      ❌ ideally with F#-style domain objects, not C#-style serializable objects
      ❌ remove every single instance of mutation
      ❌ separate DB serialization as a different EF package
        ❌ represent Expr as JSON
        ❌ represent Expr as a recursive structure looked up with a recursive query
      ❌ extend the `ABCDEvent` definition to include a processed state and a created time
      ❌ add an `EventDesc` to `FieldEvent`
        ❌ useful for pre/post event actions and conditions, it defines that which is passed to co.On plus a pre- and post-condition coroutine
        ❌ it is polymorphic and distributed over the concrete instances (ie `SetIntField of IntEventDesc`, ...)
      ❌ define custom rules and make the priority of assignments actually count
      ❌ persist the entities to Elasticsearch
      ❌ persist the entities to Postgres
        ❌ enums to strings
        ❌ json fields, in particular Metadata, expr, assignment
      ✅ add OpenAPI support, see if we get luckier with C# unions and inheritance
    ❌ _generate_ translation of models into ef and OpenAPI
      ❌ records
      ❌ unions
      ❌ with recursion
      ❌ with serialization attributes
    ❌ endpoint generation
      ❌ extend chains
      ❌ filter parameter (over extended entity)
      ❌ sorting parameter (over extended entity)
      ❌ security model of generated APIs from queries: allow, restrict
      ❌ security model of extension
        ❌ define User' vault data and prevent anyone from extending along User -> Vault unless they are the user themselves
      ❌ control creation of extended entities (AB inside ABEvent for example)
    ❌ low-code platform
      ❌ business rules (of which defaultings are a data-driven instance) should be just data
      ❌ workflow manager should be just data-driven coroutines
      ❌ statements and expressions evaluator
      ❌ data sync'er and mapper
    ✅ remove all the unused extra dependencies
    ✅ coroutine runtime engine
    ✅ fix stack overflow (flatten .Then)
    ✅ run in memory
    ✅ serialize to disk with JSON serializer
    ❌ dbContext factory might fix the ugly `wait 0`
    ❌ update state (serialized together with coroutine in DB)
    ❌ update events
    ❌ test user registration coroutine, create events with testing endpoint
    ❌ run with intelligent suspensions
  ✅ blog.fsproj
  ✅ postgres.csproj (this is the migrations project)
    ✅ dbContext (in C#)
    ✅ migrate one discriminated union
    ✅ separate schema for user stuff
  ❌ web app
    ✅ from Docker
    ✅ use appsettings.Development
    ✅ remove unnecessary references to EF Core stuff, only LINQ queries should still work
    ✅ migrations, db-update, db-drop, seed-db
    ❌ make sure pg-admin or DBeaver works
    ❌ post user-event endpoint
    ❌ login, logout, reset-password, edit-user, change-password, delete-user
      ❌ invoke methods from auth domain
      ❌ think about security
    ❌ users - organizations
    ❌ orders
    ❌ order confirmations
    ❌ invoices
    ❌ payments
      ❌ partial payments
      ❌ subscriptions
    ❌ delivery notes
    ❌ refunds
    ❌ payment conditions
    ❌ discounts per client
  ❌ improvements
    ❌ fix the tracking issue
        remove the ugly `wait 0`
    ❌ fix keywords properly: `wait`, `any`, `spawn`, `repeat`, `on`, `produce`
    ❌ separate Crud from AsyncCrud
    ❌ parameterize the coroutine queries serialization/deserialization
    ❌ add history/enqueued events endpoints for AB sample
      ❌ this should be done with only the predicate + sorting parameters of the ABEvents endpoint
  ❌ support both PG and MySQL
  ❌ SPA
    ❌ from docker container
    ❌ serve from backend
    ❌ import material UI
    ❌ signup form
    ❌ login form
    ❌ logout form
    ❌ edit user form
      ❌ delete user
    ❌ change password form
  ❌ coroutines
    ❌ move updaters to the core library
    ❌ set state, get state, update state
    ❌ actual operators
      ❌ on
      ❌ do/await
    ❌ actual runner
    ❌ model-first
      ❌ user, user events
      ❌ add postgres
      ❌ make DbContext implement the db contexts of the imported projects

    ❌ serialize to file
    ❌ serialize to (async) interface
    ✅ return any, all, etc. for the interpreter to process instead of evaluating directly
    ❌ await will be started in the background: make resilient wrt restarts and migrations
      ❌ how to re-enqueue after completion? Just a local mutable list to pump into the deserialized state?
    ❌ fast suspension and timer - see register as a case study
  ❌ .rest file
    ❌ register
      ❌ encrypt password with bcrypt
      ❌ save variable in .rest script
    ❌ login
      ❌ reuse variable in .rest script
    ❌ reset password
    ❌ logout
    ❌ login with changed password
    ❌ delete user
    ❌ run with docker-compose: coroutines container vs webapi container
    ❌ accept events from a single POST endpoint
  ✅ setup pg docker container
  ❌ define model-first database
    ❌ coroutines
    ❌ events
    ❌ subscriptions
      ❌ payment status (for info only)
    ❌ users
  ❌ serialize running coroutines in pg through serialization interface
  ❌ new scaffolder: type-provider for PG and ES
  ❌ add appsettings
  ❌ subscription
  ❌ payment with UBS
  ❌ payment with Adyen
  ❌ building blocks
  ❌ videos through kinoscope
  ❌ styling and frontend implementation

  ❌ auth
    ❌ registration
    ❌ login
    ❌ logout
    ❌ reset password
    ❌ magic link
  ❌ personal area
    ❌ avatar
    ❌ subscriptions
    ❌ invoices
    ❌ payments
      ❌ products (bundles)
      ❌ subscriptions
  ❌ highlight
  ❌ picked for you
  ❌ library
  ❌ filtering
  ❌ collection
    ❌ class
      ❌ building blocks
      ❌ video block/video player
  ❌ deployment to web
  ❌ deployment to native
    ❌ registration via Apple and Google
    ❌ payments via Apple and Google

  Backend
  ❌ coroutines
    ✅ main definition
    ✅ test program in apps
    ❌ generic events and their kind
    ❌ serialized suspension
    ❌ serialization interface to Postgres and volume
      ❌ volume via parameters
      ❌ postgres in a container (in test program in apps)
  ❌ predicates
    ❌ boolean expression
    ❌ rule evaluator
    ❌ session id, saved via i/o interface to eitehr postgres or volume
  ❌ synchronizer
    ❌ i/o interfaces
    ❌ fsparsec-style interface
    ❌ serialization of intermediate results to c
    ❌ testing app connects to an OData service
      ❌ read from appsettings or env variable

  ✅ give back/forward
  ❌ write guidelines
    ✅ adjust to new way of filtering the running of coroutines
    ✅ write a quick-start
    ❌ adjust naming of foreign mutations (expected/exposed), update guidelines
    ❌ carefully review guidelines
    ❌ split into smaller files
    ❌ reference pages in a man/docs style or JSDoc
    ✅ explain monorepo
  ❌ add deep dive presentations
    ✅ forms
    ❌ data-driven forms
    ❌ views
    ❌ native vs web
  ✅ add help to create-domain [command line tool](https://github.com/75lb/command-line-usage)
  ❌ create-domain
    ❌ broken in new setup - fix it
    ❌ add to guidelines
    ✅ add to readme
    ✅ convert dash to camelCase to create-domain
  ❌ CI
    ✅ automate tests on push
    ✅ tag-based release
    ✅ formatting
    ✅ commit VSCode settings
    ❌ test docker image build (potentially as part of integration tests)
    ❌ CI in sync with pre-commit hooks
  ❌ publish
    ✅ split core and playground
    ✅ write readme
      ✅ credits to BLP and Hoppinger
    ✅ publish repo
    ✅ publish on LinkedIn
    ✅ publish core to npm
      ✅ add to readme
      ❌ publish create-domain within core
        ❌ add to readme
    ✅ Co.embed should accept undefined as a result of narrowing, in that case suspending the coroutine implicitly
    ❌ test core and create-domain from the npm package
    ❌ add a sponsor button and create a Patreon account
  ❌ coroutine wrapper name is currently anonymous (in React Developer Tools). can that be updated?
  ✅ coroutine runner extension. we currently have both `*.ts` and `*.tsx` you've mentioned that we will only have `*.ts`
  ❌ remove JS warnings (component children missing keys)
  ✅ table->tbody
  ✅ rename single-lettered type variables to decent names
  ✅ use pretty printer for AsyncState in all debug views instead of JSON.stringify
  ❌ card1...card3 use an extensibility pattern that could be embedded in the simpleUpdater<CardState, Extra = Unit> for easier reuse
    ❌ the extensibility pattern of Cards in the dashboad deserves a global sample and a spot in the coroutines
  ✅ simpleUpdater should return a Fun
    ✅ so that we can write ((cardsRepository.Updaters.Core.card3.then(dashboardRepository.Updaters.Core.cards))) instead of the ugly eta-expansion

  ✅ every domain should have both FM types
  ✅ the child2 foreign mutations expected should not reference Uncle
  ✅ mapView should accept a component with the `Children` prop
  ✅ embed should & the context and state in the first argument
  ✅ coroutine event handling
    ✅ remove the ugly event type parameter
    ✅ adjust the guidelines
    ✅ define `On` and `Cast` as methods invokable only when certain constraints (inbound/outbound `OrderedMap<Id, event>` fields) are present on the state, including `Id`s on the events
  ❌ sumUpdater
    ❌ left, right, both
  ✅ maybeUpdater
    ✅ left, both
  ❌ Map/OrderedMap of children
    ✅ automate Map/OrderedMap updaters
    ❌ add ChildN to Parent domain
  ✅ write readme
  ❌ form validations via responses of the datasync
  ❌ Unit should be different from {}
  ❌ rewrite InfiniteStreamState with Debounced/Synchronized
  ❌ tables with sorting and filtering
    ❌ define in combination with FormCollection
      ❌ add/remove
      ❌ reorder
  ❌ improvements that just help
    ✅ parent.child.x Updaters auto-chaining can actually be achieved
      ❌ simpleUpdaterWithChildren in another domain
        ❌ simpleUpdater.WithChildren in another domain
      ✅ the children passed must be mapped and the parent updater applied in order to auto-widen
    ❌ auto-chaining should support Sum, Map, and OrderedMap updaters out of the box
      ❌ this might be achieved by marking the simpleUpdater with a kind == "simpleUpdater", and the other
        supported updaters (for example, simpleSetter = [x,y,z,...] => Updater(e => e+x,y,z,etc.))
        (for example, curriedSetter = [x,y,z,...] => [a,b,c,...] => Updater(e => e+x,y,z,a,b,c,etc.))
        then the widenChildren operator checks the kind and decides how to perform the mapping
  ❌ pair
  ❌ standard templates for unions, lists, trees, products, and sums
