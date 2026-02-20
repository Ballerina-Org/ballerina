namespace Ballerina.DSL.FormBuilder.Model


module FormAST =
  open Ballerina.Cat.Collections.OrderedMap
  open System

  type EntityName = { EntityName: string }

  type Unchecked = | Unchecked

  type FormIdentifier = FormIdentifier of string

  type RendererIdentifier = RendererIdentifier of string

  type LauncherIdentifier = LauncherIdentifier of string

  type TypeIdentifier = TypeIdentifier of string

  type FieldIdentifier = FieldIdentifier of string

  type TabIdentifier = TabIdentifier of string

  type ApiIdentifier = ApiIdentifier of string

  type CaseIdentifier = CaseIdentifier of string

  type ColumnIdentifier = ColumnIdentifier of string

  type GroupIdentifier = GroupIdentifier of string

  type Label = Label of string

  type Tooltip = Tooltip of string

  type Details = Details of string

  type Cardinality =
    | Single
    | Multi

  and PrimitiveRendererKind =
    | String
    | Int32
    | Int64
    | Float32
    | Float
    | Date
    | DateOnly
    | StringId
    | Bool
    | Base64
    | Secret
    | Guid
    | Unit

  and PrimitiveRenderer<'typeValue> =
    { Primitive: RendererIdentifier
      Renderer: PrimitiveRendererKind
      Type: 'typeValue }


  and [<Obsolete("EnumRenderer is deprecated. Use One or Many instead.")>] EnumRenderer<'typeValue> =
    { Enum: RendererIdentifier
      Cardinality: Cardinality
      Api: ApiIdentifier
      Type: 'typeValue }

  and MapRenderer<'typeValue> =
    { Map: RendererIdentifier
      Key: RendererExpression<'typeValue>
      Value: RendererExpression<'typeValue>
      Type: 'typeValue }

  and TupleRenderer<'typeValue> =
    { Tuple: RendererIdentifier
      Items: List<RendererExpression<'typeValue>>
      Type: 'typeValue }

  and ListRenderer<'typeValue> =                     
    { List: RendererIdentifier                      // nome di una funzione nei renderer che é capace di renderizzare una lista di oggetti (senza sapere come renderizzare i singoli elementi), esempio listAsTable/readOnlyList
      Element: RendererExpression<'typeValue>       // renderer dell'elemento della lista che verrá invocato N volte; nel reference ce ne saranno due, uno per la preview(es.: dropdown) e uno per l'oggetto vero e proprio (es.: address)
      Type: 'typeValue }                            // tipo dell'elemento della lista

  and ReferenceOneRenderer<'typeValue> = 
    { ReferenceOne: RendererIdentifier 
      Preview: RendererExpression<'typeValue> 
      CurrentElement: RendererExpression<'typeValue> 
      SchemaEntityName: EntityName
      Type: 'typeValue
      TypeID: 'typeValue } // per reference servono due tipi, uno é quello dell'id e uno é quello dell'oggetto (esempio: addressID e address) 
  // Esempio di forms: ContactNumbers list(readonlyList) string(print);
  // readOnlyList = RendererIdentifier
  // string(print) = RendererExpression<'typeValue> 
  // 'typeValue = string, perché ContactNumbers nei types é List[string]
  // Per il nuovo ReferenceRendererOne:
  // Nel file forms: AddressID referenceone(referenceAsCard/referenceWithDropdown/etc... = modo di mostrare una reference) view(Address) guid(print) entitáNelloSchema 
  // (entitáNelloSchema = "Addresses" assumendo ci sia nello schema una tabella che si chiama cosí);
  // Campi del record ReferenceOneRenderer quindi sono: 
  // Reference(RendererIdentifier)
  // Preview(renderedExpr)
  // CurrentElement(rendererExpr) 
  // EntityName(stringa tipata, esiste giá: EntityName) 
  // e poi nel record ci sono anche il tipo dell'id e tipo dell'elemento (che vengono implicati, non passati direttamente)

  and ReadonlyRenderer<'typeValue> =
    { Readonly: RendererIdentifier
      Value: RendererExpression<'typeValue>
      Type: 'typeValue }

  and SumRenderer<'typeValue> =
    { Sum: RendererIdentifier
      Left: RendererExpression<'typeValue>
      Right: RendererExpression<'typeValue>
      Type: 'typeValue }

  and [<Obsolete("StreamRenderer is deprecated. Use One or Many instead.")>] StreamRenderer<'typeValue> =
    { Stream: RendererIdentifier
      Cardinality: Cardinality
      Api: ApiIdentifier
      Type: 'typeValue }

  and TableRenderer<'typeValue> =
    { Table: RendererIdentifier
      Api: ApiIdentifier
      Type: 'typeValue }

  and RecordRenderer<'typeValue> =
    { Renderer: Option<RendererIdentifier>
      Members: Members<'typeValue>
      DisabledFields: Set<FieldIdentifier>
      Type: 'typeValue }

  and InlineFormRenderer<'typeValue> =
    { InlineForm: Option<RendererIdentifier>
      Body: FormBody<'typeValue>
      Type: 'typeValue }

  and UnionRenderer<'typeValue> =
    { Union: RendererIdentifier
      Cases: Map<CaseIdentifier, RendererExpression<'typeValue>>
      Type: 'typeValue }

  and OneRenderer<'typeValue> =
    { One: RendererIdentifier
      Api: ApiIdentifier
      Details: RendererExpression<'typeValue>
      Preview: Option<RendererExpression<'typeValue>>
      Type: 'typeValue }

  and LinkedUnlinkedRenderers<'typeValue> =
    { Linked: RendererExpression<'typeValue>
      Unlinked: Option<RendererExpression<'typeValue>> }

  and ManyRendererDefinition<'typeValue> =
    | LinkedUnlinked of LinkedUnlinkedRenderers<'typeValue>
    | Element of RendererExpression<'typeValue>

  and ManyRenderer<'typeValue> =
    { Many: RendererIdentifier
      Api: ApiIdentifier
      Body: ManyRendererDefinition<'typeValue>
      Type: 'typeValue }

  and RendererExpression<'typeValue> =
    | Primitive of PrimitiveRenderer<'typeValue>
    | Map of MapRenderer<'typeValue>
    | Tuple of TupleRenderer<'typeValue>
    | List of ListRenderer<'typeValue>
    | Readonly of ReadonlyRenderer<'typeValue>
    | Sum of SumRenderer<'typeValue>
    | Enum of EnumRenderer<'typeValue>
    | Stream of StreamRenderer<'typeValue>
    | Table of TableRenderer<'typeValue>
    | Form of FormIdentifier * 'typeValue
    | Record of RecordRenderer<'typeValue>
    | Union of UnionRenderer<'typeValue>
    | InlineForm of InlineFormRenderer<'typeValue>
    | One of OneRenderer<'typeValue>
    | Many of ManyRenderer<'typeValue>
    | ReferenceOne of ReferenceOneRenderer<'typeValue>

    member this.Type =
      match this with
      | Primitive p -> p.Type
      | Map m -> m.Type
      | Tuple t -> t.Type
      | List l -> l.Type
      | Readonly r -> r.Type
      | Sum s -> s.Type
      | Enum e -> e.Type
      | Stream s -> s.Type
      | Table t -> t.Type
      | Form(_, t) -> t
      | Record r -> r.Type
      | Union u -> u.Type
      | InlineForm i -> i.Type
      | One o -> o.Type
      | Many m -> m.Type
      | ReferenceOne r -> r.Type

  and Field<'typeValue> =
    { Name: FieldIdentifier
      Label: Option<Label>
      Tooltip: Option<Tooltip>
      Details: Option<Details>
      Renderer: RendererExpression<'typeValue>
      Type: 'typeValue }

  and Column =
    { Groups: Map<GroupIdentifier, Set<FieldIdentifier>> }

  and Tab =
    { Columns: Map<ColumnIdentifier, Column> }

  and Members<'typeValue> =
    { Fields: Map<FieldIdentifier, Field<'typeValue>>
      Tabs: Map<TabIdentifier, Tab> }


  and FormBody<'typeValue> =
    { Members: Members<'typeValue>
      DisabledFields: Set<FieldIdentifier>
      Details: Option<RendererExpression<'typeValue>>
      Highlights: Set<FieldIdentifier> }

  type Form<'typeValue> =
    { IsEntryPoint: bool
      RendererId: Option<RendererIdentifier>
      Form: FormIdentifier
      TypeIdentifier: TypeIdentifier
      Type: 'typeValue
      Body: FormBody<'typeValue>

    }

  type FormDefinitions<'typeValue> =
    { Forms: OrderedMap<FormIdentifier, Form<'typeValue>> }
