# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SharpQL is an F# implementation of a SQL database server for the rgSQL test suite. rgSQL is a comprehensive testing framework with 200+ test cases designed to guide the creation of SQL database servers from scratch.

## Development Commands

### Building and Running
- `dotnet build` - Build the F# project
- `dotnet run` - Run the application
- `dotnet clean` - Clean build artifacts

### Testing
- `../run-tests` - Run the complete rgSQL test suite (from parent directory)
- `../run-tests -s` - Skip server startup/shutdown tests
- `../run-tests -m` - Manual mode (manage server yourself)
- `../run-tests -p "postgres://localhost/mydb"` - Test against PostgreSQL

## Architecture

### Core Components

**SharpQL.fsproj**: .NET 9.0 F# console application targeting executable output

**Program.fs**: Main entry point - currently contains placeholder "Hello from F#"

### rgSQL Test Framework Integration

The project is part of the rgSQL ecosystem located in `/Users/Kevin/Dev/rgsql/`:

- **test_runner/**: Python-based test framework that communicates with SQL servers via TCP sockets
- **tests/**: 13 test files covering SQL features from basic SELECT statements to complex JOINs and aggregations
- **settings.ini**: Configuration file where `server_command` must be set to your executable path

### Expected Server Behavior

Your F# implementation should:

1. **TCP Server**: Listen on port 3003 (configurable in settings.ini)
2. **Protocol**: Receive null-terminated SQL strings, respond with JSON-formatted results
3. **Response Format**: JSON with either results array or error information
4. **Startup**: Server must be ready within 3 seconds of launch
5. **Shutdown**: Respond to TERM signal for graceful shutdown

### Test Progression

Tests are ordered by complexity:
1. **1_the_server.py**: Server startup/shutdown functionality
2. **2_returning_values.sql**: Basic SELECT statements (SELECT 1, SELECT TRUE)
3. **3_tables.sql**: CREATE TABLE, INSERT, basic SELECT FROM
4. **4_resilient_parsing.sql**: SQL parsing edge cases
5. **5_expressions.sql**: Mathematical and logical expressions
6. **6_finding_errors.sql**: Error handling and reporting
7. **7_null.sql**: NULL value handling
8. **8_filtering_ordering_limiting.sql**: WHERE, ORDER BY, LIMIT
9. **10_scoped_references.sql**: Column references and scoping
10. **11_joining_tables.sql**: JOIN operations
11. **12_grouping.sql**: GROUP BY functionality
12. **13_aggregate_functions.sql**: COUNT, SUM, AVG, etc.

## Configuration

Update `../settings.ini` with your server command:
```ini
[rgsql]
server_command = dotnet run --project SharpQL
```

## Development Notes

- The F# implementation should replace the current placeholder in Program.fs
- Focus on implementing a TCP server that can parse and execute SQL statements
- Test early and often using the rgSQL test suite
- Server logs are captured in `../server_output.log` and `../server_error.log`