open System.Net
open System.Net.Sockets
open System.IO
open System.Text

open Tokenizer
open Parser

let handleConnection (client: TcpClient) =
    printfn "Client connected from %A" client.Client.RemoteEndPoint
    
    use stream = client.GetStream()
    let tokenizer = Tokenizer()
    let parser = Parser()
    let buffer = Array.zeroCreate<byte> 1024
    
    while true do
        let bytesRead = stream.ReadAsync(buffer, 0, buffer.Length) |> Async.AwaitTask |> Async.RunSynchronously
        let chunk = buffer[0..bytesRead-1]
        
        printfn "Read %d bytes" bytesRead
            
        match tokenizer.ProcessChunk(chunk) with
        | Some(tokens) ->
            parser.AddTokens(tokens)
            match parser.ProcessStatement() with
            | Some(statement) ->
                printfn "Emit complete statement %A with %d tokens" statement statement.Length
                use writer = new BinaryWriter(stream, Encoding.UTF8, true)
                writer.Write("ECHO")
                writer.Write(byte 0)
            | None -> ()
        | None -> () 

let startServer port =
    use listener = new TcpListener(IPAddress.Any, port)
    listener.Start()
    printfn "TCP Server started on port %d" port
    
    while true do
        printfn "Waiting for connection..."
        use client = listener.AcceptTcpClient()
        handleConnection client

[<EntryPoint>]
let main _ =
    startServer 3003
    0 