module Program

open Expecto

[<EntryPoint>]
let main args =
  runTestsWithCLIArgs [] args (testList "all" [ Tests.tests; Tests.editorConfigTests; Tests.literalBindingTests ])
