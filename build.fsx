// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open System.IO

// Properties
let buildDir = "./build/"
let packagingRoot = "./"


let authors = ["Neil Stalker"]
let projectName = "ReactiveGpio"
let projectSummary = ""
let projectDescription = "A library to read and write to GPIO ports on linux using Reactive Extensions"
let tags = "Reactive RX GPIO linux RaspberryPi"
let copyright = "Copyright © Neil Stalker 2014"
// Versions
let release =
    File.ReadLines "ReactiveGpio/ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes


// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir]

    RestorePackages()
)

Target "Build" (fun _ ->
    CreateCSharpAssemblyInfo "./ReactiveGpio/Properties/AssemblyInfo.cs"
        [Attribute.Title projectName
         Attribute.Description projectDescription
         Attribute.Product projectName
         Attribute.Version release.AssemblyVersion
         Attribute.FileVersion release.AssemblyVersion
         Attribute.Copyright copyright
         Attribute.Company ""]

    !! "ReactiveGpio/ReactiveGpio.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "Build-Output: "
)

Target "CreatePackage" (fun _ ->

    NuGet (fun p -> 
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription                               
            OutputPath = packagingRoot
            Summary = projectSummary
            WorkingDir = buildDir
            Version = release.NugetVersion
            ReleaseNotes = release.Notes.Head 
            Tags = tags
            Dependencies = ["Rx-Main", GetPackageVersion "./packages/" "Rx-Main"]
            Files = [("ReactiveGpio.dll", Some @"lib\net45", None)
                     ("ReactiveGpio.pdb", Some @"lib\net45", None)]
            SymbolPackage = NugetSymbolPackage.Nuspec
            }) 
            "ReactiveGpio.nuspec"
)


// Dependencies
"Clean"
  ==> "Build"
  ==> "CreatePackage"

// start build
RunTargetOrDefault "CreatePackage"