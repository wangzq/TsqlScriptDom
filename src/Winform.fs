module Winform

open System.Windows.Forms
open System.Drawing

let createSplitContainer (c1:#Control) (c2:#Control) (orientation:Orientation) =
    let splitContainer = new SplitContainer()
    splitContainer.Dock <- DockStyle.Fill
    c1.Dock <- DockStyle.Fill
    splitContainer.Panel1.Controls.Add(c1)
    c2.Dock <- DockStyle.Fill
    splitContainer.Panel2.Controls.Add(c2)
    splitContainer.Orientation <- orientation
    splitContainer

let splitH (c1:#Control) (c2:#Control) = createSplitContainer c1 c2 Orientation.Vertical
let splitV (c1:#Control) (c2:#Control) = createSplitContainer c1 c2 Orientation.Horizontal

/// Given a control that is added to a SplitterContainer, use this method to
/// toggle: c |> collapse not
/// collapsed: c |> collapse (fun _ -> true)
/// not collapsed: c |> collapse (fun _ -> false)
let collapse f (c:#Control) = 
    let panel = c.Parent :?> SplitterPanel
    if panel = null then invalidOp "Control is not in a SplitterPanel"
    let container = panel.Parent :?> SplitContainer
    if container.Panel1 = panel then
        container.Panel1Collapsed <- f container.Panel1Collapsed
    else
        container.Panel2Collapsed <- f container.Panel2Collapsed


let multilineTextbox readonly = 
    let tb = new TextBox()
    tb.Multiline <- true
    tb.WordWrap <- false
    tb.HideSelection <- false
    tb.ReadOnly <- readonly
    tb.ScrollBars <- ScrollBars.Both
    tb
