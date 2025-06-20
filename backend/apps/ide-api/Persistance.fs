namespace IDEApi

open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Ballerina.Collections.Sum


module Storage =
  type Spec = {
    Name: string
    Body: string
    Version: string
  }
  // Mocked tenant and spec details for now.
  // It will be replaced with a proper tenant management system while working on a further milestones
  // dedicated for splitting the spec into different files across virtual folders
  // and organization of specs by tenant where a tenant is a list of linked spec files
  let private tenantId = "defaultTenant"
  //let private specName = "defaultSpec"
  //let private instanceName = "defaultInstance"
  
  let basePath = "storage"

  let getTenantPath tenantId = Path.Combine(basePath, tenantId)
  let getSpecFolder tenantId = Path.Combine(getTenantPath tenantId)//, specName)
  let getSpecJsonPath tenantId specName  = Path.Combine(getSpecFolder tenantId, $"{specName}.json")
  //let getInstancesPath tenantId specName = Path.Combine(getSpecFolder tenantId specName, "instances")
  // let getInstancePath tenantId specName instanceName =
  //   Path.Combine(getInstancesPath tenantId specName, instanceName + ".json")

  let ensureSpecStructure tenantId=
    let path = getSpecFolder tenantId
    //let instances = getInstancesPath tenantId specName
    Directory.CreateDirectory(path) |> ignore
    //Directory.CreateDirectory(instances) |> ignore

  let save tenantId specName (json: string) =
    ensureSpecStructure tenantId 
    File.WriteAllTextAsync(getSpecJsonPath tenantId specName, json)
    
  let load tenantId specName =
    task {
      let path = getSpecJsonPath tenantId specName
      match File.Exists path with
      | true ->
          let! file = File.ReadAllTextAsync path
          return Some file
      | false ->
          return None
    }
    
  // let saveInstance tenantId specName instanceName (json: string) =
  //   ensureSpecStructure tenantId specName
  //   File.WriteAllTextAsync(getInstancePath tenantId specName instanceName, json)
  //
  // let loadInstance tenantId specName instanceName =
  //   let path = getInstancePath tenantId specName instanceName
  //   File.ReadAllTextAsync path
  //
  // let listInstanceNames tenantId specName =
  //   let path = getInstancesPath tenantId specName
  //   if Directory.Exists path then
  //     (path, "*.json")
  //     |> Directory.GetFiles
  //     |> Array.map Path.GetFileNameWithoutExtension
  //   else
  //     [||]

  let listSpecNames tenantId =
    let path = getTenantPath tenantId
    if Directory.Exists path then
      path
      |> Directory.GetDirectories
      |> Array.map Path.GetFileName
    else
      [||]
      
  // let lockSpec (json: string) =
  //   saveSpec tenantId specName (json: string)
  //   
  // let lockedSpec () =
  //   loadSpec tenantId specName 

  module Entity =
    let private options = JsonSerializerOptions(WriteIndented = true)
    options.Converters.Add(JsonFSharpConverter())

    let get(name) =
      load tenantId name

    let save (data: string) (name: string)=
      save tenantId name data

    // tmp to play with an entity objects in a repl
    // let getInstance<'T>() =
    //   task {
    //     let! spec = load tenantId specName
    //     return JsonSerializer.Deserialize<'T>(spec, options)
    //   }
    //   
    // // tmp to play with an entity objects in a repl  
    // let saveInstance<'T>() (data: 'T) =
    //   let json = JsonSerializer.Serialize(data, options)
    //   saveInstance tenantId specName instanceName json
