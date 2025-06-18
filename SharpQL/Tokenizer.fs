module Tokenizer

open System.Text

type Token =
    | Select
    | StatementEnd

type Tokenizer() =
    let buffer = StringBuilder()
    
    member this.ProcessChunk(chunk: byte array) =
        let input = Encoding.UTF8.GetString(chunk)
        printfn "Received: %s" input
        
        buffer.Append(input) |> ignore
        
        let tokens = ResizeArray<Token>()
        
        let command = buffer.ToString().ToUpper()
        if command.Contains("SELECT") then
            tokens.Add(Select)
        
        if input.Contains ";" then
            tokens.Add(StatementEnd)
            buffer.Clear() |> ignore
            
        if tokens.Count > 0 then
            Some(tokens.ToArray())
        else
            None