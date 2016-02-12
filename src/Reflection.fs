namespace pclytics

open System
open System.Linq
open System.Net
open Microsoft.FSharp.Reflection

module internal Reflection =

    type internal NameAttribute(name : string) =
        inherit Attribute()
        member a.Name = name

    let private getName (c : UnionCaseInfo) =
        match c.GetCustomAttributes(typeof<NameAttribute>) with
        | [|a|] -> Some (a :?> NameAttribute).Name
        | _ -> None

    let toPayloadString (o: obj) =
        match o with
        | :? bool as b -> Convert.ToInt32(b).ToString()
        | o -> o.ToString()

    let private getCaseInfo v =
        let t = v.GetType()
        FSharpValue.GetUnionFields(v, t)

    let getValue v =
        let case, fields = getCaseInfo v
        match fields with
        | [|o|] -> toPayloadString o
        | _ -> match getName case with
               | Some name -> name
               | None -> case.Name

    let private makePair name value =
        WebUtility.UrlEncode name + "=" + WebUtility.UrlEncode (toPayloadString value)

    let getRecordPairs r =
        let t = r.GetType()
        seq {
              for f in FSharpType.GetRecordFields(t) do
              for a in f.CustomAttributes do
              if a.AttributeType = typeof<NameAttribute> then
                  let name = a.ConstructorArguments.First().Value.ToString()
                  let value = FSharpValue.GetRecordField(r, f)
                  yield makePair name value
            }

    let getKey v =
        let case, _ = getCaseInfo v
        match getName case with
        | Some name -> name
        | None -> case.Name

    let getUnionPairs us = us |> Seq.map (fun u -> makePair (getKey u) (getValue u))

    let typesEqual p q = p.GetType() = q.GetType()

    let typeHash p = p.GetType().GetHashCode()

    let hashEqual p q = (typeHash p).CompareTo (typeHash q)
