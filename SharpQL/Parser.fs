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
            let tokens = tokenBuffer.ToArray()
            tokenBuffer.Clear()
            Some tokens
        else
            None