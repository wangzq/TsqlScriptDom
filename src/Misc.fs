module Misc

open System.Text.RegularExpressions

let unix2dos text = Regex.Replace(text, @"\r\n?|\n", "\r\n")
let dos2unix text = Regex.Replace(text, @"\r\n?|\n", "\n")