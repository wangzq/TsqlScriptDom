module WinformFile

open System.Windows.Forms 

let openFile filter f =
    let dlg = new OpenFileDialog()
    dlg.Filter <- filter + "|All|*.*"
    if dlg.ShowDialog() = DialogResult.OK then
        f dlg.FileName
