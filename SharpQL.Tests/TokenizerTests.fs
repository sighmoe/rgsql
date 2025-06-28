module TokenizerTests

open System.Text
open NUnit.Framework
open Tokenizer
open Debug

[<Test>]
let testTokenizerSelectToken () =
    Debug.log "Running testTokenizerSelectToken"
    let tokenizer = Tokenizer()
    let input = "SELECT 1;"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(3), "Should return 3 tokens")
        Assert.That(tokens.[0], Is.EqualTo(Select), "First token should be Select")
        Assert.That(tokens.[1], Is.EqualTo(Integer 1), "Second token should be an Integer with value 1")
        Assert.That(tokens.[2], Is.EqualTo(StatementEnd), "Third token should be StatementEnd")
    | None ->
        Assert.Fail("Should return tokens for SELECT statement")
        
let testTokenizerSplitTokens () =
    let tokenizer = Tokenizer()
    let input1 = "SELE"
    let input2 = "CT 1;"
    
    let chunk1 = Encoding.UTF8.GetBytes(input1)
    let chunk2 = Encoding.UTF8.GetBytes(input2)
    
    match tokenizer.ProcessChunk(chunk1) with
    | Some _ ->
        Assert.Fail("Should not return tokens for incomplete SELECT statement")
    | None -> ()
    
    match tokenizer.ProcessChunk(chunk2) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(3), "Should return 3 tokens")
        Assert.That(tokens.[0], Is.EqualTo(Select), "First token should be Select")
        Assert.That(tokens.[1], Is.EqualTo(Identifier "1"), "Second token should be an Identifier with value '1'")
        Assert.That(tokens.[2], Is.EqualTo(StatementEnd), "Third token should be StatementEnd")
    | None ->
        Assert.Fail("Should return tokens for SELECT statement")

[<Test>]
let testTokenizerStatementEndOnly () =
    let tokenizer = Tokenizer()
    let input = ";"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.[0], Is.EqualTo(StatementEnd), "Token should be StatementEnd")
        Assert.That(tokens.Length, Is.EqualTo(1), "Should return 1 token for semicolon only")
    | None ->
        Assert.Fail("Should return StatementEnd token for semicolon")

[<Test>]
let testTokenizerCaseInsensitive () =
    let tokenizer = Tokenizer()
    let input = "select;"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(2), "Should recognize lowercase SELECT")
        Assert.That(tokens.[0], Is.EqualTo(Select), "Should recognize lowercase select as Select token")
    | None ->
        Assert.Fail("Should handle case insensitive SELECT")