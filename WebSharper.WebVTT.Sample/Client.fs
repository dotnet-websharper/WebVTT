namespace WebSharper.WebVTT.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebVTT

[<JavaScript>]
module Client =    
    
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let currentCaption = Var.Create "None"

    let setupVideoWithSubtitles () =
        let video = JS.Document.GetElementById("video") |> As<HTMLVideoElement>
        let captionDisplay = JS.Document.GetElementById("currentCaption")
        
        let track = JS.Document.CreateElement("track")
        track?label <- "English"
        track?kind <- "subtitles"
        track?srclang <- "en"
        track?src <- "subtitles.vtt" 
        track?``default`` <- true
        video.AppendChild(track) |> ignore

        video.OnPlay <- (fun _ -> Console.Log("Video Started Playing"))
        
        track.AddEventListener("load", fun (_: Dom.Event) ->
            let trackData = track?track |> As<TextTrack>
            trackData.Mode <- "hidden"
            
            video.AddEventListener("timeupdate", fun (_: Dom.Event) ->
                let currentTime = video.CurrentTime
                let activeCues = trackData.ActiveCues

                if activeCues.Length > 0 then
                    let cue = activeCues.[0]
                    currentCaption.Value <- cue.Text
                else
                    currentCaption.Value <- "None"
            )
        )

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .init(fun () -> setupVideoWithSubtitles())
            .Subtitles(currentCaption.V)
            .Doc()
        |> Doc.RunById "main"
