// include Fake libs
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Git

// Directories
let buildDir = "./build"
let srcBuildDir = buildDir + "/src"
let testDir = buildDir + "./test"
let testDataDir = buildDir + "./testData"
let outputDir = buildDir + "/output"

let appProjects = !!"src/**/*.csproj" ++ "src/**/*.fsproj"
let testProjects = !!"test/**/*.csproj" ++ "test/**/*.fsproj"

// version info
let version = "0.0.1"
let stringVersion = Information.getCurrentSHA1 "."
let asmLibDir = "..\\OCA-AsmLib"

(* ------------------- Download AsmLib ----------------*)

Target "EnsureAsmLib" (fun _ -> 
    if not (directoryExists asmLibDir) then failwith "AsmLib missing")

(* ------------------- Normal targets ----------------*)

Target "Clean" (fun _ -> CleanDirs [ srcBuildDir; testDir; outputDir; testDataDir ])

Target "AssemblyInfo" (fun _ -> 
    CreateCSharpAssemblyInfo "./src/Properties/AssemblyInfo.cs" [ Attribute.Title "OCA-VirtualMachine"
                                                                  Attribute.Description("VirtualMachine for OCA - " + stringVersion)
                                                                  Attribute.Product "OCA-VirtualMachine"
                                                                  Attribute.Version version
                                                                  Attribute.FileVersion version
                                                                  Attribute.InternalsVisibleTo "OCA.VirtualMachine.Test" ])

Target "BuildApp" (fun _ -> MSBuildRelease srcBuildDir "Build" appProjects |> Log "AppBuild-Output: ")
Target "BuildTest" (fun _ -> MSBuildDebug testDir "Build" testProjects |> Log "TestBuild-Output: ")

Target "Test" (fun _ -> 
    !!(testDir + "/*.dll") |> NUnit(fun p -> 
                                  { p with DisableShadowCopy = true
                                           TimeOut = System.TimeSpan.FromMinutes 5.0
                                           Framework = "4.5"
                                           Domain = NUnitDomainModel.DefaultDomainModel
                                           OutputFile = buildDir + "/TestResults.xml" }))

Target "Deploy" (fun _ -> !!(srcBuildDir + "/**/*.*") -- "*.zip" |> Zip buildDir (outputDir + "/VirtualMachine." + stringVersion + ".zip"))

Target "MainBuild" DoNothing

// Build order
"Clean" ==> "EnsureAsmLib" ==> "AssemblyInfo" ==> "BuildApp" ==> "BuildTest" ==> "Test" ==> "Deploy" ==> "MainBuild"

// start build
RunTargetOrDefault "MainBuild"
