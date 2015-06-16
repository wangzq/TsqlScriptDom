module Tsql

open Microsoft.SqlServer.TransactSql.ScriptDom
open System.IO
open System.Text
open System.Collections.Generic

exception ParseErrors of IList<ParseError>

let parse sql = 
    let parser = TSql120Parser(false)
    let sr = new StringReader(sql)
    let fragment, errors = parser.Parse(sr)
    if errors.Count > 0 then raise (ParseErrors(errors))
    fragment

let parseFile file = File.ReadAllText(file) |> parse

type TSqlFragment with
    member x.DescendantOf(other : #TSqlFragment) = 
        if other = null then false
        else
            if other.FragmentLength < 0 then true
            else
                let otherFrom = other.StartOffset
                let otherTo = other.StartOffset + other.FragmentLength
                let thisFrom = x.StartOffset
                let thisTo = x.StartOffset + x.FragmentLength
                thisFrom >= otherFrom && thisFrom < otherTo && thisTo <= otherTo
    
    member x.ToStringWithPosition() = 
        if x.FragmentLength < 0 then
            x.GetType().Name
        else 
            sprintf "%s (%d - %d)" (x.GetType().Name) x.StartOffset (x.StartOffset + x.FragmentLength - 1)
    member x.GetTypeName() = x.GetType().Name
    member x.ToSql() =
        let generator = Sql100ScriptGenerator()
        let sb = StringBuilder()
        let sw = new StringWriter(sb)
        generator.GenerateScript(x, sw)
        sb.ToString()

type ParseError with
    member x.ToStringEx() =
        sprintf "Line %d: %s" (x.Line) (x.Message)
