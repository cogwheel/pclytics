// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

// This is just a scratch pad for REPL-driven development. You should run
// the following command from the git root to prevent pollution:
//
//   git update-index --assume-unchanged project/Script.fsx

#r "System.Net.Http"

#load "../src/Reflection.fs"
#load "../src/Client.fs"

open System
open System.IO
open System.Net

open pclytics
open pclytics.Reflection
