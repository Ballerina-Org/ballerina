namespace IDEApi

open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open Ballerina.Collections.Sum

module Db =
  let private options = JsonSerializerOptions(WriteIndented = true)
  options.Converters.Add(JsonFSharpConverter())
    
  let mutable private spec: Option<string> = None
    
  let lockSpace(s: string) =
    spec <- Some s
    spec.IsSome
    
  let lockedSpec () =
    spec

  let get<'T>(path: string) : Sum<'T,string> =
    match File.Exists path with
    | true ->
      let json = File.ReadAllText path
      Sum.Left (JsonSerializer.Deserialize<'T>(json, options))
    | false ->
      Sum.Right $"File not found: {path}"

  let save<'T>(path: string) (data: 'T) =
    let json = JsonSerializer.Serialize(data, options)
    File.WriteAllText(path, json)

  let updatePerson<'T> path (updated: 'T) =
    //todo: patch
    save path updated
        
  let faked<'T> =
    get "path-to-fake-data.json"