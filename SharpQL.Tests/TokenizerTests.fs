module TokenizerTests

open System.Text
open NUnit.Framework
open Tokenizer

[<Test>]
let testTokenizerSelectToken () =
    let tokenizer = Tokenizer()
    let input = "SELECT * FROM users;"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(2), "Should return 2 tokens for SELECT statement")
        Assert.That(tokens.[0], Is.EqualTo(Select), "First token should be Select")
        Assert.That(tokens.[1], Is.EqualTo(StatementEnd), "Second token should be StatementEnd")
    | None ->
        Assert.Fail("Should return tokens for SELECT statement")

[<Test>]
let testTokenizerStatementEndOnly () =
    let tokenizer = Tokenizer()
    let input = ";"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(1), "Should return 1 token for semicolon only")
        Assert.That(tokens.[0], Is.EqualTo(StatementEnd), "Token should be StatementEnd")
    | None ->
        Assert.Fail("Should return StatementEnd token for semicolon")

[<Test>]
let testTokenizerNoTokens () =
    let tokenizer = Tokenizer()
    let input = "UPDATE users SET name = 'test'"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some _ ->
        Assert.Fail("Should not return tokens for incomplete statement")
    | None ->
        Assert.Pass("Should return None for incomplete statement without semicolon")

[<Test>]
let testTokenizerCaseInsensitive () =
    let tokenizer = Tokenizer()
    let input = "select * from users;"
    let chunk = Encoding.UTF8.GetBytes(input)
    
    match tokenizer.ProcessChunk(chunk) with
    | Some tokens ->
        Assert.That(tokens.Length, Is.EqualTo(2), "Should recognize lowercase SELECT")
        Assert.That(tokens.[0], Is.EqualTo(Select), "Should recognize lowercase select as Select token")
    | None ->
        Assert.Fail("Should handle case insensitive SELECT")