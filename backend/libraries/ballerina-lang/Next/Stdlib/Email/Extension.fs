namespace Ballerina.DSL.Next.StdLib.Email

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  [<NoComparison; NoEquality>]
  type EmailTypeClass<'runtimeContext> =
    { send: 'runtimeContext -> string -> string -> string -> unit }

    static member Console() : EmailTypeClass<'runtimeContext> =
      { send =
          fun _ toEmail subject body ->
            System.Console.WriteLine(
              $"Email::send to={toEmail} subject={subject} body={body}"
            ) }

    static member FromRuntimeContext
      (send: 'runtimeContext -> string -> string -> string -> unit)
      : EmailTypeClass<'runtimeContext> =
      { send = send }

  let EmailExtension<'runtimeContext, 'ext>
    (email_ops: EmailTypeClass<'runtimeContext>)
    (operationLens: PartialLens<'ext, EmailOperations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, EmailOperations<'ext>> =

    let stringTypeValue = TypeValue.CreateString()
    let unitTypeValue = TypeValue.CreatePrimitive PrimitiveType.Unit

    let emailSendId =
      Identifier.FullyQualified([ "Email" ], "send")
      |> TypeCheckScope.Empty.Resolve

    let sendOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, EmailOperations<'ext>> =
      emailSendId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                stringTypeValue,
                TypeValue.CreateArrow(
                  stringTypeValue,
                  TypeValue.CreateArrow(stringTypeValue, unitTypeValue)
                )
              ),
              Kind.Star,
              EmailOperations.Send
                {| toEmail = None
                   subject = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | EmailOperations.Send state -> Some(EmailOperations.Send state))
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! state =
                op
                |> EmailOperations.AsSend
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! arg =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match state.toEmail, state.subject with
              | None, _ ->
                return
                  (EmailOperations.Send
                    {| toEmail = Some arg
                       subject = None |}
                   |> operationLens.Set,
                   Some emailSendId)
                  |> Ext
              | Some toEmail, None ->
                return
                  (EmailOperations.Send
                    {| toEmail = Some toEmail
                       subject = Some arg |}
                   |> operationLens.Set,
                   Some emailSendId)
                  |> Ext
              | Some toEmail, Some subject ->
                let! ctx = reader.GetContext()
                do email_ops.send ctx.RuntimeContext toEmail subject arg
                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Unit)
            } }

    { TypeVars = []
      Operations = [ sendOperation ] |> Map.ofList }
