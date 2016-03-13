namespace pclytics

open System
open System.Collections
open System.Collections.Generic
open FSharp.Reflection

open pclytics.Reflection

(* CaseSet stores a set of values of some discriminated union. Their uniqueness
   is determined by the particular case of the union, not by any of the case
   fields (extra values) carried by the type.

   For example, given the following:

       type Foo = Bar of int option | Baz of string
       let fooList = [ Bar None ; Baz "first" ; Bar (Some 1337) ; Baz "second" ]
       let fooCaseSet = CaseSet fooList

   fooCaseSet would contain Bar (Some 1337) and Baz "second"

   Note that the set is constructed in LIFO order. If you add a value with the
   same case but different field values, it will generate a new set with the
   value replaced. This effectively gives CaseSet the semantics of a map from
   case tag to union value *)

type CaseSet<'a when 'a : comparison> private (theMap : Map<int, 'a>) =
    new(theSeq : 'a seq) =
        (* Wish I could express this as a constraint, but it fails early
           enough for my purposes *)
        if not (FSharpType.IsUnion typeof<'a>) then
            failwith "CaseSet value type must be a discriminated union"

        CaseSet(theSeq
                |> Seq.map (fun a -> getCaseTag a, a)
                |> Map.ofSeq)

    member private x.InnerMap = theMap

    member x.Add item = 
        theMap.Add (getCaseTag item, item)
        |> CaseSet

    member x.Contains (item : 'a) =
        theMap.ContainsKey(getCaseTag item)

    member x.Count = theMap.Count

    member x.Remove (item : 'a) =
        theMap.Remove (getCaseTag item)
        |> CaseSet

    override x.Equals other =
        x.InnerMap = (other :?> 'a CaseSet).InnerMap

    override x.GetHashCode () =
        x.InnerMap.GetHashCode()

    interface IComparable with
        member x.CompareTo other =
            (theMap :> IComparable).CompareTo (other :?> 'a CaseSet).InnerMap

    interface ICollection<'a> with
        member x.Contains(item) =
            x.Contains(item)

        member x.CopyTo(array, arrayIndex) =
            theMap
            |> Map.fold (fun i _ v -> array.[i] <- v ; i + 1) arrayIndex
            |> ignore

        member x.Count = x.Count

        member x.GetEnumerator(): IEnumerator<'a> = 
            (theMap
             |> Seq.map (fun (KeyValue(_, v)) -> v)
             :> IEnumerable<'a>)
             .GetEnumerator()

        member x.GetEnumerator(): IEnumerator = 
            (x
             :> IEnumerable<'a>)
             .GetEnumerator()
             :> IEnumerator

        member x.IsReadOnly = true
        member x.Add(item) = raise (NotSupportedException("ReadOnlyCollection"))
        member x.Remove(item) = raise (NotSupportedException("ReadOnlyCollection"))
        member x.Clear() = raise (NotSupportedException("ReadOnlyCollection"))