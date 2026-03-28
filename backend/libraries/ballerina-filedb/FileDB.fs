namespace Ballerina.DSL.Next.StdLib

module FileDB =
  open Ballerina.DSL.Next.StdLib.DB
  open MutableMemoryDB
  open Extensions
  open Ballerina.Reader.WithError
  open System
  open Ballerina.DSL.Next.StdLib.FileDbManager
  open Ballerina.DSL.Next.Types
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Terms
  open Ballerina

  type FileDBRuntimeContext = { TenantId: Guid; SchemaName: string }

  type DbFileConfig =
    { DbDirectory: string
      DbExtension: string }

  let private makeFileManager directory extension =
    reader {
      let! context = reader.GetContext()
      let context = context.RuntimeContext

      return
        FileContentManager.Create<MutableMemoryDB<FileDBRuntimeContext, 'customExt>>(
          directory,
          extension,
          context.TenantId,
          context.SchemaName
        )
    }

  let private runDbOpWithFileManager
    ref
    (arg: 'arg)
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (update: bool)
    (refUpdater: MutableMemoryDB<FileDBRuntimeContext, 'customExt> -> 'ref -> unit)
    (opExtractor:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >
        -> 'ref
        -> 'arg
        -> Reader<'result, ExprEvalContext<FileDBRuntimeContext, _>, Errors<Unit>>)
    =
    reader {
      let! fileManager = makeFileManager directory extension
      let! currentDb = fileManager.GetContent() |> reader.OfSum
      refUpdater currentDb ref

      let! result =
        opExtractor { memoryDbOps with DB = currentDb } ref arg
        |> reader.MapError(Errors.MapContext(replaceWith ()))

      if update then
        do! fileManager.WriteContent memoryDbOps.DB |> reader.OfSum

      let _x = result
      return result
    }

  let private updateTarget target source =
    target.entities <- source.entities
    target.relations <- source.relations
    target.backgroundJobs <- source.backgroundJobs

  let private entityRefUpdater
    (db: MutableMemoryDB<FileDBRuntimeContext, 'customExt>)
    (entityRef:
      EntityRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    : unit =

    let _, currentDb, _, _ = entityRef
    db |> updateTarget currentDb

  let relationRefUpdater
    (db: MutableMemoryDB<FileDBRuntimeContext, 'customExt>)
    (relationRef:
      RelationRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    : unit =
    let _, currentDb, _, _, _, _ = relationRef
    db |> updateTarget currentDb

  let private create<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (entity_ref:
      EntityRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    create_arg
    =
    runDbOpWithFileManager
      entity_ref
      create_arg
      directory
      extension
      memoryDbOps
      true
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.Create)

  let private upsert<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (entity_ref:
      EntityRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    update_arg
    =
    runDbOpWithFileManager
      entity_ref
      update_arg
      directory
      extension
      memoryDbOps
      true
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.Upsert)

  let private delete<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (entity_ref:
      EntityRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    id_to_delete
    =
    runDbOpWithFileManager
      entity_ref
      id_to_delete
      directory
      extension
      memoryDbOps
      true
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.Delete)

  let private deleteMany<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (entity_ref:
      EntityRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    ids_to_delete
    =
    runDbOpWithFileManager
      entity_ref
      ids_to_delete
      directory
      extension
      memoryDbOps
      true
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.DeleteMany)

  let private link<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    (relation_ref:
      RelationRef<
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    link_arg
    =
    runDbOpWithFileManager
      relation_ref
      link_arg
      directory
      extension
      memoryDbOps
      true
      relationRefUpdater
      (fun dbTypeClass -> dbTypeClass.Link)

  let private unlink<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    relation_ref
    unlink_arg
    =
    runDbOpWithFileManager
      relation_ref
      unlink_arg
      directory
      extension
      memoryDbOps
      true
      relationRefUpdater
      (fun dbTypeClass -> dbTypeClass.Unlink)

  let private isLinked<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    relation_ref
    relation_id
    =
    runDbOpWithFileManager
      relation_ref
      relation_id
      directory
      extension
      memoryDbOps
      false
      relationRefUpdater
      (fun dbTypeClass -> dbTypeClass.IsLinked)

  let private getById<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    entity_ref
    entityId
    =
    runDbOpWithFileManager
      entity_ref
      entityId
      directory
      extension
      memoryDbOps
      false
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.GetById)

  let private getMany<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    entity_ref
    (skip, take)
    =
    runDbOpWithFileManager
      entity_ref
      (skip, take)
      directory
      extension
      memoryDbOps
      false
      entityRefUpdater
      (fun dbTypeClass -> dbTypeClass.GetMany)

  let private lookupMaybe<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    relation_ref
    source
    dir
    =
    runDbOpWithFileManager
      relation_ref
      (source, dir)
      directory
      extension
      memoryDbOps
      false
      relationRefUpdater
      (fun dbTypeClass -> fun relation_ref (source, dir) -> dbTypeClass.LookupMaybe relation_ref source dir)

  let private lookupOne<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    relation_ref
    source
    dir
    =
    runDbOpWithFileManager
      relation_ref
      (source, dir)
      directory
      extension
      memoryDbOps
      false
      relationRefUpdater
      (fun dbTypeClass -> fun relation_ref (source, dir) -> dbTypeClass.LookupOne relation_ref source dir)

  let private lookupMany<'customExt when 'customExt: comparison>
    directory
    extension
    (memoryDbOps:
      DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >)
    relation_ref
    source
    dir
    (skip, truncate)
    =
    runDbOpWithFileManager
      relation_ref
      (source, dir, skip, truncate)
      directory
      extension
      memoryDbOps
      false
      relationRefUpdater
      (fun dbTypeClass ->
        fun relation_ref (source, dir, skip, truncate) ->
          dbTypeClass.LookupMany relation_ref source dir (skip, truncate))

  let updateFromFileSystem
    { DbDirectory = directory
      DbExtension = extension }
    db
    =
    reader {
      let! fileManager = makeFileManager directory extension
      let! currentDb = fileManager.GetContent() |> reader.OfSum
      currentDb |> updateTarget db
    }

  let updateBackgroundJobs
    { DbDirectory = directory
      DbExtension = extension }
    update
    =
    reader {
      let! fileManager = makeFileManager directory extension
      let! currentDb = fileManager.GetContent() |> reader.OfSum
      do currentDb.backgroundJobs <- update currentDb.backgroundJobs
      do! fileManager.WriteContent currentDb |> reader.OfSum
    }

  let fileDbOps<'customExt when 'customExt: comparison>
    (dbFileConfig: DbFileConfig)
    : DBTypeClass<
        FileDBRuntimeContext,
        MutableMemoryDB<FileDBRuntimeContext, 'customExt>,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, 'customExt>, 'customExt>
       >
    =

    let memoryDbOps = db_ops<FileDBRuntimeContext, 'customExt> ()
    let directory, extension = dbFileConfig.DbDirectory, dbFileConfig.DbExtension

    { DB = memoryDbOps.DB
      BeginTransaction = memoryDbOps.BeginTransaction
      CommitTransaction = memoryDbOps.CommitTransaction
      Create = create directory extension memoryDbOps
      Upsert = upsert directory extension memoryDbOps
      Delete = delete directory extension memoryDbOps
      DeleteMany = deleteMany directory extension memoryDbOps
      Link = link directory extension memoryDbOps
      Unlink = unlink directory extension memoryDbOps
      IsLinked = isLinked directory extension memoryDbOps
      GetById = getById directory extension memoryDbOps
      GetMany = getMany directory extension memoryDbOps
      LookupMaybe = lookupMaybe directory extension memoryDbOps
      LookupOne = lookupOne directory extension memoryDbOps
      LookupMany = lookupMany directory extension memoryDbOps
      RunQuery =
        fun query range ->
          reader {
            let! fileManager = makeFileManager directory extension
            let! currentDb = fileManager.GetContent() |> reader.OfSum

            let queryRunAdapter =
              { GetDbFromEntityRef = fun _ -> currentDb
                GetDbFromRelationRef = fun _ -> currentDb }

            let! (values, _db) =
              runQuery true queryRunAdapter query
              |> Reader.mapError (Errors.MapContext(replaceWith ()))

            match range with
            | None -> return values |> Seq.toList
            | Some(skip, take) -> return values |> Seq.skip skip |> Seq.truncate take |> Seq.toList

          } }
