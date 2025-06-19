module Debug

open System

let mutable private debugMode = false

let log message =
    if debugMode then 
        printfn "[DEBUG] %s" message

let enableDebug() =
    debugMode <- true
    log "Debug mode enabled"

let initializeFromArgs (args: string array) =
    if args |> Array.contains "--debug" then
        enableDebug()

let initializeFromEnvironment() =
    match Environment.GetEnvironmentVariable("SHARPQL_DEBUG") with
    | "true" | "1" -> enableDebug()
    | _ -> ()