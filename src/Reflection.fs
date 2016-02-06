namespace pclytics

open System
open System.Linq
open Microsoft.FSharp.Reflection

type internal NameAttribute(name : string) =
    inherit Attribute()
    member a.Name = name

module internal Reflection =
    let private getInfo v =
        let t = v.GetType()
        if not (FSharpType.IsUnion(t)) then
            raise (ArgumentException("Value must be a discriminated union case"))
        FSharpValue.GetUnionFields(v, t)

    let private getName (c : UnionCaseInfo) =
        match c.GetCustomAttributes(typeof<NameAttribute>) with
        | [|a|] -> Some (a :?> NameAttribute).Name
        | _ -> None

    let getKey v = 
        let case, _ = getInfo v
        match getName case with
        | Some name -> name
        | None -> case.Name

    let private getField (fields : obj[]) =
        if fields.Length > 0 then
            let field = fields.[0]
            if field.GetType() = typeof<bool> then
                Some (Convert.ToInt32(field :?> bool).ToString())
            else
                Some (fields.[0].ToString())
        else
            None

    let getValue v =
        let case, fields = getInfo v
        match getField fields with
        | Some name -> name
        | None -> match getName case with
                  | Some name -> name
                  | None -> case.Name

    let private makePair name (value : obj) =
        if value.GetType() = typeof<bool> then
            name, Convert.ToInt32(value :?> bool).ToString()
        else
            name, value.ToString()

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

    let getUnionPairs us = seq {
                                 for u in us do
                                 yield getKey u, getValue u
                               }

    let typesEqual p q = p.GetType() = q.GetType()

    let typeHash p = p.GetType().GetHashCode()

    let hashEqual p q = (typeHash p).CompareTo (typeHash q)
