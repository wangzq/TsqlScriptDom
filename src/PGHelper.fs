module PgHelper

open System
open System.Reflection
open System.ComponentModel

let setExpandable f (asm:Assembly) =
    let a = TypeConverterAttribute(typeof<ExpandableObjectConverter>)
    asm.GetTypes()
    |> Seq.filter (fun t -> t.IsClass && (f t))
    |> Seq.iter (fun t -> TypeDescriptor.AddAttributes(t, a) |> ignore)
