module absample.repositories

open Microsoft.EntityFrameworkCore
open Ballerina.CRUD
open absample.efmodels

let rec abrepos =
  {| ef =
      {| AB =
          fun (db: DbContext) (table: DbSet<absample.efmodels.AB>) ->
            CrudSeq.FromDbSetToSeq
              table
              ({| getId = (fun id -> <@ fun e -> e.ABId = id @>)
                  setId = fun id -> fun e -> { e with ABId = id } |})
              db

         ABEvent =
          fun (db: DbContext) (table: DbSet<absample.efmodels.ABEvent>) ->
            CrudSeq.FromDbSetToSeq
              table
              ({| getId = (fun id -> <@ fun e -> e.ABEventId = id @>)
                  setId =
                   fun id ->
                     fun e ->
                       (match e |> ABEvent.ToUnion with
                        | absample.models.AEvent a ->
                          absample.models.AEvent { a with event.ABEventId = id }
                        | absample.models.BEvent b ->
                          absample.models.BEvent { b with event.ABEventId = id })
                       |> ABEvent.FromUnion |})
              db

         AEvent =
          fun (db: DbContext) (table: DbSet<absample.efmodels.AEvent>) ->
            CrudSeq.FromDbSetToSeq
              table
              ({| getId = (fun id -> <@ fun e -> e.ABEventId = id @>)
                  setId =
                   fun id ->
                     fun e ->
                       { (e |> AEvent.ToRecord) with
                           event.ABEventId = id }
                       |> AEvent.FromRecord |})
              db

         BEvent =
          fun (db: DbContext) (table: DbSet<absample.efmodels.BEvent>) ->
            CrudSeq.FromDbSetToSeq
              table
              ({| getId = (fun id -> <@ fun e -> e.ABEventId = id @>)
                  setId =
                   fun id ->
                     fun e ->
                       { (e |> BEvent.ToRecord) with
                           event.ABEventId = id }
                       |> BEvent.FromRecord |})
              db |}

     logical =
      {| AB =
          fun (db: DbContext) (table: DbSet<absample.efmodels.AB>) ->
            let crud =
              Crud.FromDbSet
                table
                ({| getId = (fun id -> <@ fun e -> e.ABId = id @>)
                    setId = fun id -> fun e -> { e with ABId = id } |})
                db in

            { new Crud<absample.models.AB> with
                member this.setId id = crud.setId id
                member this.getId e = crud.getId e
                member this.create e = crud.create e
                member this.delete id = crud.delete id
                member this.update id u = crud.update id u
                member this.get id = crud.get id }

         ABEvent =
          fun (db: DbContext) (table: DbSet<absample.efmodels.ABEvent>) ->
            let crud =
              Crud.FromDbSet
                table
                ({| getId = (fun id -> <@ fun e -> e.ABEventId = id @>)
                    setId =
                     fun id ->
                       fun e ->
                         (match e |> ABEvent.ToUnion with
                          | absample.models.AEvent a ->
                            absample.models.AEvent
                              { a with event.ABEventId = id }
                          | absample.models.BEvent b ->
                            absample.models.BEvent
                              { b with event.ABEventId = id })
                         |> ABEvent.FromUnion |})
                db in

            { new Crud<absample.models.ABEvent> with
                member this.setId id =
                  fun e ->
                    e
                    |> ABEvent.FromUnion
                    |> ABEvent.WithRecord(fun e -> { e with ABEventId = id })
                    |> ABEvent.ToUnion

                member this.getId id =
                  <@ fun e -> (e |> ABEvent.FromUnion).ABEventId = id @>

                member this.create e = crud.create (e |> ABEvent.FromUnion)
                member this.delete id = crud.delete id

                member this.update id u =
                  crud.update id (ABEvent.ToUnion >> u >> ABEvent.FromUnion)

                member this.get id =
                  crud.get id |> Option.map ABEvent.ToUnion } |} |}
