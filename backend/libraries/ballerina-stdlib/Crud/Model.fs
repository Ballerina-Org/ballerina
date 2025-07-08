namespace Ballerina

module CRUD =
  open Ballerina.Fun
  open Microsoft.EntityFrameworkCore
  open System.Linq

  open System
  open Ballerina.Queries

  type Crud<'a> =
    abstract member setId: Guid -> Updater<'a>
    abstract member getId: Guid -> Quotations.Expr<'a -> bool>
    abstract member create: 'a -> Guid
    abstract member delete: Guid -> Unit
    abstract member get: Guid -> Option<'a>
    abstract member update: Guid -> Updater<'a> -> Unit

  type CrudSeq<'a> =
    inherit Crud<'a>

    abstract member getN:
      Quotations.Expr<'a -> bool> -> Quotations.Expr<'a -> 'key> -> Ballerina.Range -> Linq.IQueryable<'a>

  type Crud<'a> with
    static member FromDbSet
      (dbSet: DbSet<'e>)
      (entity:
        {| setId: Guid -> Updater<'e>
           getId: Guid -> Quotations.Expr<'e -> bool> |})
      (db: DbContext)
      : Crud<'e> =
      { new Crud<'e> with
          member this.setId id = entity.setId id
          member this.getId e = entity.getId e

          member this.create e =
            let id = Guid.CreateVersion7()
            do dbSet.Add(entity.setId id e) |> ignore
            do db.SaveChanges() |> ignore
            id

          member this.delete id =
            dbSet.Where(entity.getId id |> ToLinq).ExecuteDelete() |> ignore

          member this.update id u =
            db.ChangeTracker.Clear()
            let es = dbSet.AsNoTracking().Where(entity.getId id |> ToLinq).ToList()
            let es = es.Select(u)
            dbSet.UpdateRange(es)
            do db.SaveChanges() |> ignore

          member this.get id =
            let es = dbSet.Where(entity.getId id |> ToLinq).ToList()
            es |> Seq.tryHead }

  type CrudSeq<'a> with
    static member FromDbSetToSeq
      (dbSet: DbSet<'e>)
      (entity:
        {| setId: Guid -> Updater<'e>
           getId: Guid -> Quotations.Expr<'e -> bool> |})
      (db: DbContext)
      : CrudSeq<'e> =
      let crud = Crud<'a>.FromDbSet dbSet entity db

      { new CrudSeq<'e> with
          member this.setId id = crud.setId id
          member this.getId e = crud.getId e
          member this.create e = crud.create e
          member this.delete id = crud.delete id
          member this.update id u = crud.update id u
          member this.get id = crud.get id

          member this.getN predicate ordering range =
            dbSet.Where(ToLinq predicate).OrderBy(ToLinq ordering).Skip(range.skip).Take(range.take) }

// type Crud<'a> =
//   abstract member setId:Guid -> Updater<'a>
//   abstract member getId:Guid -> Quotations.Expr<'a -> bool>
//   abstract member create:'a -> Async<Guid>
//   abstract member delete:Guid -> Async<Unit>
//   abstract member get:Guid -> Async<Option<'a>>
//   abstract member getN:Quotations.Expr<'a -> bool> -> Quotations.Expr<'a -> 'key> -> Ballerina.Range -> Async<Linq.IQueryable<'a>>
//   abstract member update:Guid -> Updater<'a> -> Async<Unit>

// type Crud<'a> with
//   static member FromDbSet (dbSet:DbSet<'e>) (entity:{| setId:Guid -> Updater<'e>; getId:Guid -> Quotations.Expr<'e -> bool> |}) (db:DbContext)  : Crud<'e> =
//     { new Crud<'e> with
//         member this.setId id = entity.setId id
//         member this.getId e = entity.getId e
//         member this.create e =
//           let id = Guid.CreateVersion7()
//           do dbSet.Add(entity.setId id e) |> ignore
//           async{
//             let! _ = db.SaveChangesAsync() |> Async.AwaitTask
//             return id
//           }
//         member this.delete id =
//           async{
//             let! _ = dbSet.Where(entity.getId id |> ToLinq).ExecuteDeleteAsync() |> Async.AwaitTask
//             return ()
//           }
//         member this.update id u =
//           async{
//             db.ChangeTracker.Clear()
//             let es = dbSet.AsNoTracking().Where(entity.getId id |> ToLinq).ToList()
//             let es = es.Select(u)
//             dbSet.UpdateRange(es)
//             let! _ = db.SaveChangesAsync() |> Async.AwaitTask
//             return ()
//           }
//         member this.get id =
//           async {
//             let! es = dbSet.Where(entity.getId id |> ToLinq).ToListAsync() |> Async.AwaitTask
//             return es |> Seq.tryHead
//           }
//         member this.getN predicate ordering range =
//           async{
//             return dbSet.Where(ToLinq predicate).OrderBy(ToLinq ordering).Skip(range.skip).Take(range.take)
//           }
//     }
