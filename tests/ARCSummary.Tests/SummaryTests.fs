module ARCSummary.Tests

open ARCtrl
open Expecto
open System
open System.IO





let failTest = 
    test "AlwaysFail" {
        let actual = true
        Expect.isFalse actual "Expected value to be false, but was true instead"
    }

[<EntryPoint>]
let main argv =
  runTestsWithCLIArgs [] argv failTest

