module Tokenizer

open System.Text
open Microsoft.FSharp.Core

let seperators = " ,"
type Token =
    | Identifier of string
    | Integer of int
    | Bool of bool
    | Null
    | Select
    | From
    | Where
    | OrderBy
    | GroupBy
    | Having
    | Limit
    | As
    | StatementEnd

let emitToken = function
    | "select" -> Select
    | "from" -> From
    | "where" -> Where
    | "order" -> OrderBy
    | "group" -> GroupBy
    | "having" -> Having
    | "limit" -> Limit
    | "as" -> As
    | ";" -> StatementEnd
    | "true" -> Bool true
    | "false" -> Bool false
    | "null" -> Null
    | other when System.Int32.TryParse other |> fst -> Integer (int other)
    | other -> 
        // Validate identifier starts with letter or underscore
        if System.String.IsNullOrEmpty(other) then
            failwith "Empty identifier"
        elif System.Char.IsLetter(other[0]) || other[0] = '_' then
            Identifier other
        else
            failwith $"Invalid identifier: {other}"
type Tokenizer() =
    let buffer = StringBuilder()
    
    member this.ProcessChunk(chunk: byte array) =
        let input = Encoding.UTF8.GetString(chunk).TrimEnd(System.Convert.ToChar((byte) 0))
        //printfn "Received: %s" input
        let tokens = ResizeArray<Token>()
        
        for i in 0..input.Length-1 do
            let ch = input[i]
            
            if seperators.Contains(ch) then
                if buffer.Length > 0 then
                    tokens.Add(buffer.ToString().ToLower() |> emitToken)
                    buffer.Clear() |> ignore
            elif ch = ';' then
                if buffer.Length > 0 then
                    tokens.Add(buffer.ToString().ToLower() |> emitToken)
                    buffer.Clear() |> ignore
                tokens.Add(StatementEnd)
            else
                buffer.Append(ch) |> ignore
        
        // Emit any remaining token in buffer at end of chunk
        if buffer.Length > 0 then
            tokens.Add(buffer.ToString().ToLower() |> emitToken)
            buffer.Clear() |> ignore
        
        if tokens.Count > 0 then Some(tokens.ToArray()) else None