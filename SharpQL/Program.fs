open System.Net
open System.Net.Sockets
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks

open Interpreter
open Tokenizer
open Parser

let handleConnection (client: TcpClient) = task {
    Debug.log $"Client connected from {client.Client.RemoteEndPoint}"
    
    use stream = client.GetStream()
    let interpreter = Interpreter()
    let tokenizer = Tokenizer()
    let parser = Parser()
    let buffer = Array.zeroCreate<byte> 1024
    
    while true do
        let! bytesRead = stream.ReadAsync(buffer, 0, buffer.Length)
        let chunk = buffer[0..bytesRead-1]
        
        Debug.log $"Read {bytesRead} bytes"
            
        try
            match tokenizer.ProcessChunk(chunk) with
            | Some(tokens) ->
                parser.AddTokens(tokens)
                try
                    match parser.ProcessStatement() with
                    | Some(statement) ->
                        Debug.log $"Parsed statement: {statement}"
                        let result = interpreter.Eval(statement)
                        let jsonResponse = interpreter.ResultToJson(result)
                        Debug.log $"JSON response: {jsonResponse}"
                        let responseBytes = Encoding.UTF8.GetBytes(jsonResponse)
                        stream.Write(responseBytes, 0, responseBytes.Length)
                        stream.WriteByte(0uy)
                    | None -> ()
                with
                | ex ->
                    Debug.log $"Parsing error: {ex.Message}"
                    parser.ClearState() // Clear parser state after error
                    let errorResult = Error("parsing_error")
                    let jsonResponse = interpreter.ResultToJson(errorResult)
                    let responseBytes = Encoding.UTF8.GetBytes(jsonResponse)
                    stream.Write(responseBytes, 0, responseBytes.Length)
                    stream.WriteByte(0uy)
            | None -> ()
        with
        | ex ->
            Debug.log $"Tokenizer error: {ex.Message}"
            parser.ClearState() // Clear parser state after tokenizer error
            let errorResult = Error("parsing_error")
            let jsonResponse = interpreter.ResultToJson(errorResult)
            let responseBytes = Encoding.UTF8.GetBytes(jsonResponse)
            stream.Write(responseBytes, 0, responseBytes.Length)
            stream.WriteByte(0uy)
} 

let startServer port =
    use listener = new TcpListener(IPAddress.Any, port)
    use semaphore = new SemaphoreSlim(10, 10) // Max 10 concurrent connections
    listener.Start()
    Debug.log $"TCP Server started on port {port}"
    
    while true do
        Debug.log "Waiting for connection..."
        let client = listener.AcceptTcpClient()
        Task.Run(System.Func<Task>(fun () -> task {
            do! semaphore.WaitAsync()
            try
                use c = client
                do! handleConnection c
            finally
                semaphore.Release() |> ignore
        })) |> ignore

[<EntryPoint>]
let main args =
    Debug.initializeFromArgs args
    Debug.initializeFromEnvironment()
    
    startServer 3003
    0 