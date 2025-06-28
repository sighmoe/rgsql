module Interpreter

open Parser
open System.Text.Json

type Value =
    | IntValue of int
    | StringValue of string
    | BoolValue of bool
    | NullValue

type QueryResult =
    | Success of Value list list * string option list
    | Error of string

type Interpreter() =
    
    member this.Eval(statement: Statement) : QueryResult =
        match statement with
        | Select selectStmt -> this.EvalSelect(selectStmt.Arguments, selectStmt.Aliases)
    
    member private this.EvalSelect(arguments: Expression list, aliases: string option list) : QueryResult =
        let values = arguments |> List.map this.EvaluateExpression
        Success ([values], aliases)
    
    member private this.EvaluateExpression(expr: Expression) : Value =
        match expr with
        | Literal literal -> this.EvaluateLiteral(literal)
        | ColumnIdentifier name -> StringValue name
    
    member private this.EvaluateLiteral(literal: LiteralValue) : Value =
        match literal with
        | IntLiteral i -> IntValue i
        | BoolLiteral b -> BoolValue b
        | StringLiteral s -> StringValue s
        | NullLiteral -> NullValue
    
    member this.ResultToJson(result: QueryResult) : string =
        match result with
        | Success (rows, aliases) ->
            let valueToJsonString value =
                match value with
                | IntValue i -> string i
                | StringValue s -> JsonSerializer.Serialize(s)
                | BoolValue b -> if b then "true" else "false"
                | NullValue -> "null"
            
            let rowsJson = 
                rows 
                |> List.map (fun row -> 
                    "[" + (row |> List.map valueToJsonString |> String.concat ",") + "]")
                |> String.concat ","
            
            // Include column names if aliases are provided
            let columnsJson = 
                if aliases |> List.exists Option.isSome then
                    let columnNames = 
                        aliases 
                        |> List.mapi (fun i alias -> 
                            match alias with
                            | Some name -> JsonSerializer.Serialize(name)
                            | None -> sprintf "\"column_%d\"" i)
                        |> String.concat ","
                    sprintf ""","column_names":[%s]""" columnNames
                else ""
            
            sprintf """{"status":"ok","rows":[%s]%s}""" rowsJson columnsJson
        | Error errorType ->
            sprintf """{"status":"error","error_type":%s}""" (JsonSerializer.Serialize(errorType))