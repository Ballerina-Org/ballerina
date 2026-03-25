open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Ballerina.API.MemoryDB.API
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Ballerina.API.MemoryDB.MemoryDBAPIFactory
open Ballerina.API.MemoryDB.Model
open Ballerina.Collections.Sum
open System.CommandLine
open Ballerina.API.MemoryDB.API
open System.IO
open Ballerina.Errors
open Ballerina
open Ballerina.LocalizedErrors
open Ballerina.FileDBAPIPlayground.Factory

[<EntryPoint>]
let main args = createAndRunAPI id id args
