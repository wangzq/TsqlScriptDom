module WinformDragDrop

open System
open System.Windows.Forms

let enableFileDragDrop (c:#Control) f =
    c.AllowDrop <- true
    c.DragEnter.Add((fun e ->
        if e.Data.GetDataPresent(DataFormats.FileDrop) then
            e.Effect <- DragDropEffects.Copy))
    c.DragDrop.Add((fun e ->
        let files = e.Data.GetData(DataFormats.FileDrop) :?> string[]
        files |> f))
