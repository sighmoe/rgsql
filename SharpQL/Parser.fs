module Parser

open Tokenizer

type Expression =
    | Literal of LiteralValue
    | ColumnIdentifier of string

and LiteralValue =
    | IntLiteral of int
    | BoolLiteral of bool
    | StringLiteral of string
    | NullLiteral

type SelectStatement = {
    Arguments: Expression list
    Aliases: string option list
    TableIdentifier: string option
    Predicates: Predicate list
}

and Predicate = 
    | WherePredicate of Expression

type Statement =
    | Select of SelectStatement

type Parser() =
    let tokenBuffer = ResizeArray<Token>()
    
    member this.AddTokens(tokens: Token array) =
        tokenBuffer.AddRange(tokens)
    
    member this.ClearState() =
        tokenBuffer.Clear()
    
    member this.HasCompleteStatement() =
        tokenBuffer.Contains(StatementEnd)
    
    member this.ProcessStatement() =
        if this.HasCompleteStatement() then
            let statement = this.EmitStatement()
            // Check if there are remaining invalid tokens
            this.ValidateRemainingTokens()
            Some(statement)
        else
            None
    
    member private this.ValidateRemainingTokens() =
        if tokenBuffer.Count > 0 then
            // Check if remaining tokens form a valid statement start
            match Array.tryHead (tokenBuffer.ToArray()) with
            | Some(Token.Select) -> () // Valid statement start, will be processed next
            | Some(Identifier _) -> failwith "Unexpected tokens after statement" // Invalid identifier like "extra"
            | Some(_) -> failwith "Unexpected tokens after statement"
            | None -> ()
            
    member this.EmitStatement() =
        let statementTokens = ResizeArray<Token>()
        let tokenList = List.ofSeq tokenBuffer
        let mutable atStatementEnd = false
        
        let mutable cursor = tokenList
        while not atStatementEnd do
            if cursor.Head = StatementEnd then
                atStatementEnd <- true
            else
                statementTokens.Add(cursor.Head)
            cursor <- cursor.Tail
            
        // Keep only remaining tokens after the statement
        tokenBuffer.Clear() |> ignore
        tokenBuffer.AddRange(cursor)
        this.ParseStatement(statementTokens.ToArray())
    
    member private this.ParseStatement(tokens: Token array) : Statement =
        match Array.tryHead tokens with
        | Some(Token.Select) -> this.ParseSelect(tokens[1..])
        | _ -> failwith "Unsupported statement type"
    
    member private this.ParseSelect(tokens: Token array) : Statement =
        let sections = this.SplitByKeywords(tokens)
        let (arguments, aliases) = 
            match Map.tryFind Token.Select sections with
            | Some argTokens -> this.ParseArgumentList(argTokens)
            | None -> ([], [])
        
        let tableIdentifier = 
            match Map.tryFind Token.From sections with
            | Some [| Identifier tableName |] -> Some tableName
            | _ -> None
        
        let predicates = 
            match Map.tryFind Token.Where sections with
            | Some whereTokens -> this.ParsePredicates(whereTokens)
            | None -> []
        
        Select {
            Arguments = arguments
            Aliases = aliases
            TableIdentifier = tableIdentifier
            Predicates = predicates
        }
    
    member private this.SplitByKeywords(tokens: Token array) : Map<Token, Token array> =
        let mutable sections = Map.empty
        let mutable currentKeyword = Token.Select
        let mutable currentTokens = ResizeArray<Token>()
        
        for token in tokens do
            match token with
            | Token.Select | Token.From | Token.Where | OrderBy | GroupBy | Having | Limit as keyword ->
                if currentTokens.Count > 0 then
                    sections <- sections.Add(currentKeyword, currentTokens.ToArray())
                currentKeyword <- keyword
                currentTokens.Clear()
            | _ ->
                currentTokens.Add(token)
        
        if currentTokens.Count > 0 then
            sections <- sections.Add(currentKeyword, currentTokens.ToArray())
        
        sections
    
    member private this.ParsePredicates(tokens: Token array) : Predicate list =
        // For now, just wrap the entire WHERE clause as a single predicate
        match tokens with
        | [||] -> []
        | _ -> 
            let (expressions, _) = this.ParseArgumentList(tokens)
            expressions |> List.map WherePredicate
    
    member private this.ParseArgumentList(tokens: Token array) : (Expression list * string option list) =
        let mutable expressions = []
        let mutable aliases = []
        let mutable i = 0
        
        while i < tokens.Length do
            let (expr, alias, newIndex) = this.ParseArgument(tokens, i)
            match expr with
            | Some expression -> 
                expressions <- expression :: expressions
                aliases <- alias :: aliases
                i <- newIndex
            | None -> 
                i <- i + 1
        
        (List.rev expressions, List.rev aliases)
    
    member private this.ParseArgument(tokens: Token array, startIndex: int) : (Expression option * string option * int) =
        if startIndex >= tokens.Length then (None, None, startIndex)
        else
            let expr = 
                match tokens[startIndex] with
                | Integer value -> Some(Literal (IntLiteral value))
                | Bool value -> Some(Literal (BoolLiteral value))
                | Null -> Some(Literal NullLiteral)
                | Identifier name -> Some(ColumnIdentifier name)
                | _ -> None
            
            match expr with
            | Some expression ->
                let mutable i = startIndex + 1
                // Check for AS alias
                let alias = 
                    if i < tokens.Length && tokens[i] = As && i + 1 < tokens.Length then
                        match tokens[i + 1] with
                        | Identifier aliasName -> 
                            i <- i + 2 // Skip AS and alias name
                            Some aliasName
                        | _ -> None
                    else None
                
                (Some expression, alias, i)
            | None ->
                (None, None, startIndex + 1) 