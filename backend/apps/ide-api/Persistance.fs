namespace IDEApi

open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open Ballerina.Collections.Sum

module Storage =
  // Mocked tenant and spec details for now.
  // It will be replaced with a proper tenant management system while working on a further milestones
  // dedicated for splitting the spec into different files across virtual folders
  // and organization of specs by tenant where a tenant is a list of linked spec files
  let private tenantId = "defaultTenant"
  let private specName = "defaultSpec"
  let private instanceName = "defaultInstance"
  
  let basePath = "storage"

  let getTenantPath tenantId = Path.Combine(basePath, tenantId)
  let getSpecFolder tenantId specName = Path.Combine(getTenantPath tenantId, specName)
  let getSpecJsonPath tenantId specName = Path.Combine(getSpecFolder tenantId specName, "spec.json")
  let getInstancesPath tenantId specName = Path.Combine(getSpecFolder tenantId specName, "instances")
  let getInstancePath tenantId specName instanceName =
    Path.Combine(getInstancesPath tenantId specName, instanceName + ".json")

  let ensureSpecStructure tenantId specName =
    let path = getSpecFolder tenantId specName
    let instances = getInstancesPath tenantId specName
    Directory.CreateDirectory(path) |> ignore
    Directory.CreateDirectory(instances) |> ignore

  let saveSpec tenantId specName (json: string) =
    ensureSpecStructure tenantId specName
    File.WriteAllText(getSpecJsonPath tenantId specName, json)
    
  let loadSpec tenantId specName =
    let path = getSpecJsonPath tenantId specName
    if File.Exists path then Some(File.ReadAllText path) else None
    
  let saveInstance tenantId specName instanceName (json: string) =
    ensureSpecStructure tenantId specName
    File.WriteAllText(getInstancePath tenantId specName instanceName, json)

  let loadInstance tenantId specName instanceName =
    let path = getInstancePath tenantId specName instanceName
    if File.Exists path then Some(File.ReadAllText path) else None

  let listInstanceNames tenantId specName =
    let path = getInstancesPath tenantId specName
    if Directory.Exists path then
      (path, "*.json")
      |> Directory.GetFiles
      |> Array.map Path.GetFileNameWithoutExtension
    else
      [||]

  let listSpecNames tenantId =
    let path = getTenantPath tenantId
    if Directory.Exists path then
      path
      |> Directory.GetDirectories
      |> Array.map Path.GetFileName
    else
      [||]
      
  let lockSpec (json: string) =
    saveSpec tenantId specName (json: string)
    
  let lockedSpec () =
    loadSpec tenantId specName 

  module Entity =
    let private options = JsonSerializerOptions(WriteIndented = true)
    options.Converters.Add(JsonFSharpConverter())

    
    let get(): Sum<string,string> =
      loadInstance tenantId specName instanceName
      |> Sum.fromOption<string, string> (fun () -> $"Entity file for ({instanceName}) not found:")

    let save() (data: string) =
      saveInstance tenantId specName instanceName data

    // tmp to play with an entity objects in a repl
    let getInstance<'T>(): Sum<'T,string> =
      loadInstance tenantId specName instanceName
      |> Sum.fromOption<string, string> (fun () -> $"Entity file for ({instanceName}) not found:")
      |> Sum.map (fun spec -> JsonSerializer.Deserialize<'T>(spec, options))
      
    // tmp to play with an entity objects in a repl  
    let saveInstance<'T>() (data: 'T) =
      let json = JsonSerializer.Serialize(data, options)
      saveInstance tenantId specName instanceName json
