namespace Ballerina.Data

open System
open System.Text.Json.Nodes
open Ballerina.Data.Model

module Spec =
  type Types = string list

  type SpecFormat =
    { Schema: Schema<ValueType>
      TypesV2: Types }

  type SpecBodyRaw = string
  type SpecName = SpecName of string

  type SpecBody =
    | Raw of SpecBodyRaw
    | Json of JsonValue
    | Parsed of SpecFormat

  type Spec =
    { Name: SpecName
      Body: SpecBody
      Version: string option }
