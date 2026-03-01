module Program

open Expecto

[<EntryPoint>]
let main args =
  runTestsWithCLIArgs [] args (testList "all tests" [Tests.tests; Tests.literalBindingTests])