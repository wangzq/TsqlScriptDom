module WinformMenu

open System
open System.Windows.Forms

let menuItem text f =
    let mi = new ToolStripMenuItem()
    mi.Text <- text
    mi.Click.Add (fun _ -> f())
    mi

let menuDrop text (items: ToolStripItem list) =
    let mi = new ToolStripMenuItem()
    mi.Text <- text 
    items |> List.iter (fun item -> mi.DropDownItems.Add(item) |> ignore)
    mi

let separator () =
    new ToolStripSeparator()

let menuBar (items: ToolStripMenuItem list) =
    let mb = new MenuStrip()
    items |> List.iter (fun item -> mb.Items.Add(item) |> ignore)
    mb

let setShortcut (keys:Keys) (item:ToolStripMenuItem) =
    item.ShortcutKeys <- keys
    item