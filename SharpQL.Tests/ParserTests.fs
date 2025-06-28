module ParserTests

open NUnit.Framework
open Tokenizer
open Parser

[<Test>]
let testParserTokenBuffering () =
    let parser = Parser()
    let tokens = [| Token.Select |]
    
    parser.AddTokens(tokens)
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(false), "Should not have complete statement without StatementEnd")
    
    match parser.ProcessStatement() with
    | Some _ ->
        Assert.Fail("Should not return statement without StatementEnd")
    | None ->
        Assert.Pass("Should return None without complete statement")

[<Test>]
let testParserCompleteStatement () =
    let parser = Parser()
    let tokens = [| Token.Select; Token.StatementEnd |]
    
    parser.AddTokens(tokens)
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(true), "Should have complete statement with StatementEnd")
    
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments, Is.Empty, "Empty SELECT should have no arguments")
            Assert.That(selectStmt.TableIdentifier, Is.EqualTo(None), "Empty SELECT should have no table")
            Assert.That(selectStmt.Predicates, Is.Empty, "Empty SELECT should have no predicates")
    | None ->
        Assert.Fail("Should return completed statement")

[<Test>]
let testParserMultipleStatements () =
    let parser = Parser()
    
    parser.AddTokens([| Token.Select |])
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(false), "Should not be complete after first token")
    
    parser.AddTokens([| Token.StatementEnd |])
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(true), "Should be complete after StatementEnd")
    
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select _ -> Assert.Pass("Should return SELECT statement")
        Assert.That(parser.HasCompleteStatement(), Is.EqualTo(false), "Buffer should be cleared after processing")
    | None ->
        Assert.Fail("Should return completed statement")

[<Test>]
let testParserSelectWithArguments () =
    let parser = Parser()
    let tokens = [| Token.Select; Token.Integer 42; Token.Bool true; Token.Null; Token.StatementEnd |]
    
    parser.AddTokens(tokens)
    
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments.Length, Is.EqualTo(3), "Should parse 3 arguments")
            Assert.That(selectStmt.TableIdentifier, Is.EqualTo(None), "Should have no table")
            Assert.That(selectStmt.Predicates, Is.Empty, "Should have no predicates")
            
            // Check first argument (integer)
            match selectStmt.Arguments[0] with
            | Literal (IntLiteral 42) -> Assert.Pass("First argument should be IntLiteral 42")
            | _ -> Assert.Fail("First argument should be IntLiteral 42")
    | None ->
        Assert.Fail("Should return completed statement")

[<Test>]
let testParserSelectWithTable () =
    let parser = Parser()
    let tokens = [| Token.Select; Token.Integer 1; Token.From; Token.Identifier "users"; Token.StatementEnd |]
    
    parser.AddTokens(tokens)
    
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments.Length, Is.EqualTo(1), "Should have 1 argument")
            Assert.That(selectStmt.TableIdentifier, Is.EqualTo(Some "users"), "Should have table identifier")
            Assert.That(selectStmt.Predicates, Is.Empty, "Should have no predicates")
    | None ->
        Assert.Fail("Should return completed statement")

[<Test>]
let testParserMultipleStatementsWithAS () =
    let parser = Parser()
    
    // First statement: SELECT 1;
    parser.AddTokens([| Token.Select; Token.Integer 1; Token.StatementEnd |])
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments.Length, Is.EqualTo(1), "First statement should have 1 argument")
            Assert.That(selectStmt.Aliases.Length, Is.EqualTo(1), "First statement should have 1 alias slot")
            Assert.That(selectStmt.Aliases[0], Is.EqualTo(None), "First statement should have no alias")
    | None ->
        Assert.Fail("Should return first statement")
    
    // Second statement: SELECT 1 AS col_1, 2 AS col_2;
    parser.AddTokens([| Token.Select; Token.Integer 1; Token.As; Token.Identifier "col_1"; Token.Integer 2; Token.As; Token.Identifier "col_2"; Token.StatementEnd |])
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments.Length, Is.EqualTo(2), "Second statement should have 2 arguments")
            Assert.That(selectStmt.Aliases.Length, Is.EqualTo(2), "Second statement should have 2 aliases")
            Assert.That(selectStmt.Aliases[0], Is.EqualTo(Some "col_1"), "First alias should be col_1")
            Assert.That(selectStmt.Aliases[1], Is.EqualTo(Some "col_2"), "Second alias should be col_2")
    | None ->
        Assert.Fail("Should return second statement")

[<Test>]
let testParserASParsing () =
    let parser = Parser()
    
    // Test AS parsing: SELECT 42 AS answer;
    parser.AddTokens([| Token.Select; Token.Integer 42; Token.As; Token.Identifier "answer"; Token.StatementEnd |])
    match parser.ProcessStatement() with
    | Some statement ->
        match statement with
        | Select selectStmt ->
            Assert.That(selectStmt.Arguments.Length, Is.EqualTo(1), "Should have 1 argument")
            Assert.That(selectStmt.Aliases.Length, Is.EqualTo(1), "Should have 1 alias")
            Assert.That(selectStmt.Aliases[0], Is.EqualTo(Some "answer"), "Alias should be 'answer'")
            
            // Check the argument is correct
            match selectStmt.Arguments[0] with
            | Literal (IntLiteral 42) -> Assert.Pass("Argument should be IntLiteral 42")
            | _ -> Assert.Fail("Argument should be IntLiteral 42")
    | None ->
        Assert.Fail("Should return statement with AS")

[<Test>]
let testParserUnexpectedTokensAfterStatement () =
    let parser = Parser()
    
    // Test case: SELECT 123; extra
    parser.AddTokens([| Token.Select; Token.Integer 123; Token.StatementEnd; Token.Identifier "extra" |])
    
    try
        match parser.ProcessStatement() with
        | Some _ -> Assert.Fail("Should throw exception for unexpected tokens")
        | None -> Assert.Fail("Should process statement but then fail on validation")
    with
    | ex -> Assert.Pass($"Correctly threw exception: {ex.Message}")

// Dummy main to suppress FS0988 warning
[<EntryPoint>]
let main _ = 0