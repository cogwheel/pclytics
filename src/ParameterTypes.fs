(* Parameter value types for serialization, validation, etc. *)

namespace pclytics

open System

(* Would use type providers if this weren't a PCL
   Would use a record if I could provide a constructor
   Would use a module with an embedded type and a constructor if
        use of that constructor could be enforced
   Alas... *)

type TrackingId (t : string) =
    let tid = match t.Split('-') with
              | [| "UA"; tid; pid |] -> t
              | _ -> failwith ("Invalid tid: " + t)
    member internal t.Tid with get() = tid

type Millis = Millis of int

type Dimension = { Width : uint32 ; Height : uint32 }

(* Needed to store System.Uris in a CaseSet *)
[<CustomComparison;CustomEquality>]
type ComparableUri =
    | Uri of Uri

    interface IComparable with
        member u.CompareTo other =
            let (Uri thisUri) = u
            let (Uri otherUri) = other :?> ComparableUri
            thisUri.AbsoluteUri.CompareTo(otherUri.AbsoluteUri)

    override u.Equals other =
        (u :> IComparable).CompareTo(other) = 0

    override u.GetHashCode () =
        let (Uri uri) = u
        uri.GetHashCode()

type HostPath = { Host : string ; Path : string}

type DocumentLocation = 
    | Url of ComparableUri
    | HostPath of HostPath