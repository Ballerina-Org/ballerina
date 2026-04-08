namespace Ballerina.DSL.Next.StdLib.Updater

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open PolyType

  type UpdaterType<'ext> =
    Value<TypeValue<'ext>, 'ext>
      -> Sum<Value<TypeValue<'ext>, 'ext>, Errors<unit>>

  [<TypeShape(Kind = TypeShapeKind.None)>]
  type UpdaterFunction<'ext> = { Updater: UpdaterType<'ext> }

  [<CustomEquality; CustomComparison>]
  type UpdaterOperations<'ext> =
    | Apply of UpdaterFunction<'ext>

    override this.Equals(_: obj) : bool = false
    override this.GetHashCode() : int = 0

    interface System.IComparable with
      member this.CompareTo(_: obj) : int = 0
