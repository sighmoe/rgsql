module Parser

open Tokenizer

type Parser() =
    let tokenBuffer = ResizeArray<Token>()
    
    member this.AddTokens(tokens: Token array) =
        tokenBuffer.AddRange(tokens)
    
    member this.HasCompleteStatement() =
        tokenBuffer.Contains(StatementEnd)
    
    member this.ProcessStatement() =
        if this.HasCompleteStatement() then
            let statement = this.EmitStatement()
            tokenBuffer.Clear()
            Some(statement)
        else
            None
            
    member this.EmitStatement() =
        let statementBuffer = ResizeArray<Token>()
        let tokenList = List.ofSeq tokenBuffer
        let mutable atStatementEnd = false
        
        let mutable cursor = tokenList
        while not atStatementEnd do
            if cursor.Head = StatementEnd then
                atStatementEnd <- true
            statementBuffer.Add(cursor.Head)
            cursor <- cursor.Tail
            
        tokenBuffer.Clear() |> ignore
        tokenBuffer.AddRange(tokenList)
        statementBuffer.ToArray() 