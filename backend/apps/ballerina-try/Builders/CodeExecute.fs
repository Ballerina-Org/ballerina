namespace TryBallerina.Interactive

module ConvenientCodeExecute =
    //TODO: for some reason using this asynchronously does not work, so we use the synchronous version, check why
    
    let _code =
      cheat {
        let! _userDesktop = ("userDesktop", "CommonFolders.GetUser UserPath.Desktop") |> CheatLiteral
        let! _userDesktop = Raw "type Desktop = FSharp.Management.FileSystem<userDesktop, watch = false>"
        return Raw "all finished well"
      } |> Async.AwaitTask |> Async.RunSynchronously
      
    let _blp =
      cheat {
        let! _ = ("inputFormsPath","""Path.Combine(Env.BlpCode.Value,"unbound/backend/apps/automatic-tests/input-forms")""") |> CheatLiteral
        let! _inputForms = Raw "type InputForms = FSharp.Management.FileSystem<inputFormsPath, watch = false>"
        return Raw "all finished well"
      } |> Async.AwaitTask |> Async.RunSynchronously