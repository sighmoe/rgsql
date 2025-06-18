open System.Net
open System.Net.Sockets
open System.IO
open System.Text

// right now this just handles the case where a single command is split into two chunks
let processChunk (chunk: byte array) =
    let input = Encoding.UTF8.GetString(chunk)
    printfn "Received: %s" input
    
    if input.Contains ";" then
        (false, input)
    else
        (true, input)
    

let handleConnection (client: TcpClient) =
    printfn "Client connected from %A" (client.Client.RemoteEndPoint)
    
    use stream = client.GetStream()
    let cmdBuffer = StringBuilder()
    let buffer = Array.zeroCreate<byte> 1024
    
    while true do
        let mutable keepReading = true
        while keepReading do
            let bytesRead = stream.ReadAsync(buffer, 0, buffer.Length) |> Async.AwaitTask |> Async.RunSynchronously
            let chunk = buffer.[0..bytesRead-1]
            
            printfn "Read %d bytes" bytesRead
            
            let (continueReading, input) = processChunk chunk
            keepReading <- continueReading
            cmdBuffer.Append(input) |> ignore
        
        printfn "Read SQL statement: %s" (cmdBuffer.ToString())
        
        cmdBuffer.Clear() |> ignore
        use writer = new BinaryWriter(stream, Encoding.UTF8, true)
        writer.Write("ECHO");
        writer.Write((byte)0);

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