module Tokenizer

open System.Text

let seperators = " ,"
type Token =
    | Identifier of string
    | Select
    | StatementEnd

let emitToken = function
    | "select" -> Select
    | ";" -> StatementEnd
    | other -> Identifier other
type Tokenizer() =
    let buffer = StringBuilder()
    
    member this.ProcessChunk(chunk: byte array) =
        let input = Encoding.UTF8.GetString(chunk).TrimEnd(System.Convert.ToChar((byte) 0))
        //printfn "Received: %s" input
        let tokens = ResizeArray<Token>()
        
        for i in 0..input.Length-1 do
            let ch = input[i]
            
            if seperators.Contains(ch) then
                tokens.Add(buffer.ToString().ToLower() |> emitToken)
                buffer.Clear() |> ignore
            elif ch = ';' then
                if buffer.Length > 0 then
                    tokens.Add(buffer.ToString().ToLower() |> emitToken)
                    buffer.Clear() |> ignore
                tokens.Add(StatementEnd)
            else
                buffer.Append(ch) |> ignore
        
        if tokens.Count > 0 then Some(tokens.ToArray()) else None