// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open ExifLibrary
open System.IO

let convertMonth =
    function
    | "feb" -> 2
    | "mar" -> 3
    | "apr" -> 4
    | "may" -> 5
    | "jun" -> 6
    | "jul" -> 7
    | "aug" -> 8
    | "sep" -> 9
    | "oct" -> 10
    | "nov" -> 11
    | "dec" -> 12
    | _ -> 1

let parseFileName (fileName: string) =
    let split = fileName.Split("_") |> Array.toList

    match split with
    | [ year; month; day; _ ] -> Some(DateTime(int year, convertMonth month, int day))
    | _ -> None

let updateDate (imageFile: ImageFile) (date: DateTime) =
    imageFile.Properties.Set(ExifTag.DateTime, date)
    imageFile.Properties.Set(ExifTag.DateTimeDigitized, date)
    imageFile.Properties.Set(ExifTag.DateTimeOriginal, date)

let convertFile (getOutputFile: string -> string) inputFile =
    let file = ImageFile.FromFile(inputFile)

    let parsed =
        inputFile
        |> System.IO.Path.GetFileName
        |> parseFileName

    parsed
    |> Option.iter
        (fun date ->
            updateDate file date
            let outputFile = getOutputFile inputFile

            outputFile
            |> System.IO.Path.GetDirectoryName
            |> Directory.CreateDirectory
            |> ignore

            file.Save outputFile)


[<EntryPoint>]
let main argv =
    let convertName (destination: string) (i: string) =
        let fileName = Path.GetFileName i
        Path.Combine(destination, fileName)

    match argv with
    | [| source; destination |] ->
        Directory.EnumerateFiles(source, "*.jpg", SearchOption.AllDirectories)
        |> Seq.toList
        |> List.iter (convertFile (convertName destination))

        Directory.EnumerateFiles(source, "*.mp4", SearchOption.AllDirectories)
        |> Seq.toList
        |> List.iter (fun input -> File.Copy(input, (convertName destination input)))

        0
    | _ -> 1 // return an integer exit code
