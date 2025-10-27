namespace Ballerina.VirtualFolders

type Transient<'T> = private Transient of 'T option

module Transient =
  let none = Transient None
  let some x = Transient(Some x)
  let value (Transient x) = x

  let has x =
    match x with
    | Transient(Some _) -> true
    | _ -> false

  let map f (Transient x) = Transient(Option.map f x)
