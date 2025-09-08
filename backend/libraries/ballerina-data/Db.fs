namespace Ballerina.Data

open System
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Seeds.Model

module Db =

  type Specs<'T> = Map<string, Map<Guid, Value<'T, Unit>>>

  type Db<'T> = { Seeds: Seeds<'T>; Specs: Specs<'T> }

  type TenantId = TenantId of Guid
