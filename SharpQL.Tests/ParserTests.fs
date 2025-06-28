module ParserTests

open NUnit.Framework
open Tokenizer
open Parser

[<Test>]
let testParserTokenBuffering () =
    let parser = Parser()
    let tokens = [| Select |]
    
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
    let tokens = [| Select; StatementEnd |]
    
    parser.AddTokens(tokens)
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(true), "Should have complete statement with StatementEnd")
    
    match parser.ProcessStatement() with
    | Some statement ->
        Assert.That(statement.Length, Is.EqualTo(2), "Should return all tokens in completed statement")
        Assert.That(statement.[0], Is.EqualTo(Select), "First token should be Select")
        Assert.That(statement.[1], Is.EqualTo(StatementEnd), "Second token should be StatementEnd")
    | None ->
        Assert.Fail("Should return completed statement")

[<Test>]
let testParserMultipleStatements () =
    let parser = Parser()
    
    parser.AddTokens([| Select |])
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(false), "Should not be complete after first token")
    
    parser.AddTokens([| StatementEnd |])
    Assert.That(parser.HasCompleteStatement(), Is.EqualTo(true), "Should be complete after StatementEnd")
    
    match parser.ProcessStatement() with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(2), "Should return both tokens")
        Assert.That(parser.HasCompleteStatement(), Is.EqualTo(false), "Buffer should be cleared after processing")
    | None ->
        Assert.Fail("Should return completed statement")

// Dummy main to suppress FS0988 warning
[<EntryPoint>]
let main _ = 0