namespace Myriad.Core

open System
open System.Collections.Generic
open EditorConfig.Core
open Fantomas.Core
open FSharp.Reflection

module EditorConfig =

    let private supportedProperties =
        [ "max_line_length"; "indent_size"; "end_of_line"; "insert_final_newline" ]

    let private toEditorConfigName (value: string) =
        value
        |> Seq.map (fun c ->
            if Char.IsUpper(c) then
                $"_%s{string (Char.ToLower(c))}"
            else
                string c)
        |> String.concat ""
        |> fun s -> s.TrimStart([| '_' |])
        |> fun name ->
            if List.contains name supportedProperties then
                name
            else
                $"fsharp_%s{name}"

    let private parseOptionsFromEditorConfig
        (fallbackConfig: FormatConfig)
        (editorConfigProperties: IReadOnlyDictionary<string, string>)
        : FormatConfig =
        let recordFields = FSharpType.GetRecordFields(typeof<FormatConfig>)
        let currentValues = FSharpValue.GetRecordFields(fallbackConfig)

        let newValues =
            Array.zip recordFields currentValues
            |> Array.map (fun (field, defaultValue) ->
                let editorConfigName = toEditorConfigName field.Name
                match editorConfigProperties.TryGetValue(editorConfigName) with
                | true, value ->
                    match value with
                    | v when field.PropertyType = typeof<int> ->
                        match Int32.TryParse(v) with
                        | true, n -> box n
                        | _ -> defaultValue
                    | v when field.PropertyType = typeof<bool> ->
                        if v = "true" then box true
                        elif v = "false" then box false
                        else defaultValue
                    | v when field.PropertyType = typeof<MultilineFormatterType> ->
                        match MultilineFormatterType.OfConfigString v with
                        | Some mft -> box mft
                        | None -> defaultValue
                    | v when field.PropertyType = typeof<EndOfLineStyle> ->
                        match EndOfLineStyle.OfConfigString v with
                        | Some eol -> box eol
                        | None -> defaultValue
                    | v when field.PropertyType = typeof<MultilineBracketStyle> ->
                        match MultilineBracketStyle.OfConfigString v with
                        | Some bs -> box bs
                        | None -> defaultValue
                    | _ -> defaultValue
                | _ -> defaultValue)

        FSharpValue.MakeRecord(typeof<FormatConfig>, newValues) :?> FormatConfig

    let private editorConfigParser = EditorConfigParser()

    /// Reads the Fantomas FormatConfig from the .editorconfig file applicable to the given F# source file path.
    /// Returns FormatConfig.Default if no relevant .editorconfig settings are found.
    let readConfiguration (fsharpFile: string) : FormatConfig =
        let editorConfigSettings: FileConfiguration =
            editorConfigParser.Parse(fileName = fsharpFile)

        if editorConfigSettings.Properties.Count = 0 then
            FormatConfig.Default
        else
            parseOptionsFromEditorConfig FormatConfig.Default editorConfigSettings.Properties
