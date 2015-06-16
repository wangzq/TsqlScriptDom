module SqlFormLib

open System
open System.Windows.Forms
open System.Drawing
open Microsoft.SqlServer.TransactSql.ScriptDom
open System.IO
open System.Text
open System.Collections.Generic

open Misc
open Tsql
open Winform
open WinformDragDrop
open WinformMenu
open WinformFile
open PgHelper

type private SimpleStack<'a>() =
    let items = List<'a>()
    member x.Add item = items.Add(item) |> ignore
    member x.FindLastIndex f = items.FindLastIndex(f)
    member x.Peek() = items.[items.Count - 1]
    member x.RemoveAfter index = 
        if items.Count > index - 1 then
            items.RemoveRange(index + 1, items.Count - index - 1)
    member x.Get index = items.[index]

type private FragmentTreeViewVisitor(tv : TreeView) = 
    inherit TSqlFragmentVisitor()
    let nodes = SimpleStack<TreeNodeCollection * TSqlFragment>()
    do 
        nodes.Add (tv.Nodes, null)
    override x.Visit(f : TSqlFragment) = 
        let pnodes, _ = 
            if f.FragmentLength < 0 then nodes.Peek()    
            else 
                let index = nodes.FindLastIndex (fun (_, _f) -> f.DescendantOf(_f))
                if index = -1 then nodes.Get 0
                else 
                    nodes.RemoveAfter index
                    nodes.Get index
        let node = pnodes.Add(f.ToStringWithPosition())
        node.Tag <- f
        if f.FragmentLength > 0 then
            nodes.Add (node.Nodes, f)

type SqlForm() as this =
    inherit Form()
    let tvFragments = new TreeView()
    let tbSelected = multilineTextbox true
    let tbAll = multilineTextbox false
    let tbError = multilineTextbox true
    let pgFragment = new PropertyGrid()

    let rec locate (nodes:TreeNodeCollection) pos =
        let result = nodes |> Seq.cast<TreeNode> |> Seq.tryFind (fun node ->
            let fragment = node.Tag :?> TSqlFragment
            if fragment = null then failwith "Unable to find fragment in node"
            else
                if fragment.StartOffset = -1 || fragment.FragmentLength = -1 then false
                else
                    pos >= fragment.StartOffset && pos < fragment.StartOffset + fragment.FragmentLength)
        match result with
        | Some node as x-> 
            match locate node.Nodes pos with
            | Some _node as y -> y
            | None  -> x
        | _ -> None

    let parse sql =
        tvFragments.Nodes.Clear()
        let sql = unix2dos sql // if there are inconsistent line endings then using offset/length to locate fragment or source will not work correctly
        this.Text <- "SQL Parser"
        tbAll.Text <- sql
        tbError.Clear()
        try
            let fragment = parse sql
            // The difference btween Accept and AcceptChildren is that Accept will start from itself, while AcceptChildren
            // will only start from its children
            fragment.Accept(FragmentTreeViewVisitor(tvFragments))
            tbError |> collapse (fun _ -> true)
        with
        | ParseErrors(errors) -> 
            errors |> Seq.iter (fun err -> tbError.AppendText(err.ToStringEx() + Environment.NewLine))
            tbError |> collapse (fun _ -> false)

    let locateInTree() =
        match locate tvFragments.Nodes (tbAll.SelectionStart) with
        | Some node -> 
            tvFragments.SelectedNode <- node
            node.EnsureVisible()
        | _ -> ()

    let parseFile file =
        File.ReadAllText(file) |> parse
        this.Text <- file

    let parseFiles (files:string[]) =
        if files.Length = 1 then
            parseFile files.[0]
        else
            let sb = StringBuilder()
            files |> Array.iter (fun file -> 
                sb.AppendLine(File.ReadAllText(file)) |> ignore
                sb.AppendLine("GO") |> ignore)
            parse (sb.ToString())
        
    do
        let fnt = new Font("Consolas", 10.5f)
        tbAll.Font <- fnt
        tbSelected.Font <- fnt
        pgFragment.Font <- fnt
        tvFragments.Font <- fnt
        this.Text <- "SQL Parser"

        tvFragments.HideSelection <- false
        this.WindowState <- FormWindowState.Maximized

        this.Controls.Add(splitH tvFragments (splitV tbAll (splitH pgFragment (splitV tbSelected tbError))))

        enableFileDragDrop tbAll parseFiles

        tvFragments.AfterSelect.Add(fun e ->
            let fragment = e.Node.Tag :?> TSqlFragment
            pgFragment.SelectedObject <- fragment
            if fragment <> null then
                if fragment.FragmentLength > 0 then
                    tbAll.Select(fragment.StartOffset, fragment.FragmentLength)
                    tbAll.ScrollToCaret()
                tbSelected.Text <- fragment.ToSql())

        tbAll.MouseDoubleClick.Add(fun _ -> locateInTree())

        let mb = menuBar [
                    menuDrop "&File" [
                        menuItem "&Open..." (fun () -> openFile "Sql|*.sql" parseFile);
                        separator();
                        menuItem "E&xit" (this.Close) 
                    ];
                    menuDrop "&Tools" [
                        menuItem "&Parse" (fun () -> parse (tbAll.Text)) |> setShortcut (Keys.F5);
                        menuItem "New Window" (fun () -> 
                            let f = new SqlForm()
                            if tbAll.SelectedText.Length > 0 then
                                f.Parse (tbAll.SelectedText)
                            f.Show() ) |> setShortcut (Keys.Control ||| Keys.N);
                        menuItem "Locate in Tree" locateInTree |> setShortcut (Keys.Control ||| Keys.L)
                    ]
                ]
        this.Controls.Add(mb)

    member this.Parse sql = parse sql
    member this.ParseFile = parseFile 
    member this.ParseFiles = parseFiles
    member this.TreeView = tvFragments
    member this.Locate pos = locate tvFragments.Nodes pos
    
