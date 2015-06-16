open System
open System.Windows.Forms

open SqlFormLib

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let form = new SqlForm()
    if argv.Length > 0 then
        if argv.[0] = "-" then
            // read from stdin for sql script to parse
            Console.In.ReadToEnd() |> form.Parse
        else
            form.ParseFiles argv
    Application.Run(form)
    0
