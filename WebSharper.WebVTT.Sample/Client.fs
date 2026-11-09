namespace WebSharper.WebVTT.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebVTT

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let currentCaption = Var.Create "No active cue."
    let trackStatus = Var.Create "Waiting for the video to load..."
    let trackCount = Var.Create "0"
    let lastEvent = Var.Create ""

    // A cue that is not present in subtitles.vtt; it is added with TextTrack.AddCue
    // to demonstrate the binding's VTTCue type.
    let programmaticCue =
        VTTCue(2.0, 2.9, "This cue was added from F# with TextTrack.addCue().")

    // Set by the video's onafterrender callback so the button can reach the element.
    let mutable player = As<HTMLVideoElement> null

    // Probe the global constructor without pulling in an external dependency.
    let vttAvailable =
        let ctor: obj = JS.Global?VTTCue
        not (ctor ===. JS.Undefined)

    let showActiveCue (track: TextTrack) =
        let active = track.ActiveCues
        if active.Length > 0 then
            currentCaption.Value <- active.[0].Text
        else
            currentCaption.Value <- "No active cue."

    let setupVideo (video: HTMLVideoElement) =
        player <- video
        if not vttAvailable then
            trackStatus.Value <- "This browser does not support the WebVTT API."
            currentCaption.Value <- "WebVTT is unavailable in this browser."
        else
            try
                // textTracks is a TextTrackList; observe its add/remove events (TrackEvent).
                let textTracks = video?textTracks |> As<TextTrackList>
                trackCount.Value <- string textTracks.Length

                textTracks.OnAddTrack <- fun (ev: TrackEvent) ->
                    let added = ev.Track |> As<TextTrack>
                    trackCount.Value <- string textTracks.Length
                    lastEvent.Value <- sprintf "addtrack: kind=%s, label=%s" added.Kind added.Label

                textTracks.OnRemoveTrack <- fun (_: TrackEvent) ->
                    trackCount.Value <- string textTracks.Length
                    lastEvent.Value <- "removetrack"

                // Attach a <track> element and drive it through the binding types.
                let trackEl = JS.Document.CreateElement("track")
                trackEl?kind <- "subtitles"
                trackEl?label <- "English"
                trackEl?srclang <- "en"
                trackEl?src <- "subtitles.vtt"
                trackEl?``default`` <- true
                video.AppendChild(trackEl) |> ignore

                let track = trackEl?track |> As<TextTrack>
                track.Mode <- "hidden"

                // React to cue changes instead of polling the video with timeupdate.
                track.OnCueChange <- fun _ -> showActiveCue track

                // Adding the programmatic cue before the .vtt resource finishes loading
                // would be discarded when the fetched cue list replaces it, so wait for
                // the track element's load event (unless it has already loaded).
                let addProgrammaticCue () =
                    track.AddCue(programmaticCue)
                    trackStatus.Value <- "Loaded the WebVTT track and added a programmatic VTTCue."

                let trackReadyState: int = trackEl?readyState
                if trackReadyState = 2 then
                    addProgrammaticCue ()
                else
                    trackEl.AddEventListener("load", fun (_: Dom.Event) -> addProgrammaticCue ())

                trackStatus.Value <- "Loading the WebVTT track..."
            with ex ->
                trackStatus.Value <- "Could not set up the WebVTT track: " + ex.Message
                currentCaption.Value <- "No active cue."

    let playProgrammaticCue () =
        if not (isNull player) then
            player.CurrentTime <- programmaticCue.StartTime
            player.Play()

    [<SPAEntryPoint>]
    let Main () =
        if not vttAvailable then
            trackStatus.Value <- "This browser does not support the WebVTT API."
            currentCaption.Value <- "WebVTT is unavailable in this browser."

        IndexTemplate.Main()
            .VideoAttrs(Attr.OnAfterRender(fun el -> setupVideo (el |> As<HTMLVideoElement>)))
            .caption(currentCaption.V)
            .trackCount(trackCount.V)
            .trackStatus(trackStatus.V)
            .lastEvent(lastEvent.V)
            .playCue(fun _ -> playProgrammaticCue())
            .Doc()
        |> Doc.RunById "main"
