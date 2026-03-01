namespace Myriad.Plugins

module internal GeneratorConfig =
    let tryGet<'T> (key: string) (config: (string * obj) seq) : 'T option =
        config |> Seq.tryPick (fun (n, v) -> if n = key then Some (v :?> 'T) else None)

    let getOrDefault<'T> (key: string) (defaultValue: 'T) (config: (string * obj) seq) : 'T =
        tryGet<'T> key config |> Option.defaultValue defaultValue
