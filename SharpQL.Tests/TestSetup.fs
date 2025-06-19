module TestSetup

open NUnit.Framework
open Debug

[<SetUpFixture>]
type TestSetup() =
    [<OneTimeSetUp>]
    member this.Setup() =
        Debug.initializeFromEnvironment()
        Debug.log "Test suite starting"

    [<OneTimeTearDown>]
    member this.TearDown() =
        Debug.log "Test suite complete"