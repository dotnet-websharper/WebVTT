# WebSharper WebVTT API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [WebVTT API](https://developer.mozilla.org/en-US/docs/Web/API/WebVTT_API), enabling WebSharper applications to parse and manipulate Web Video Text Tracks (WebVTT) subtitles and captions.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the WebVTT API.

2. **Sample Project**:
   - Demonstrates how to use the WebVTT API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/WebVTT/).

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.WebVTT
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/WebVTT.git
   cd WebVTT
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.WebVTT/WebSharper.WebVTT.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.WebVTT.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/WebVTT/](https://dotnet-websharper.github.io/WebVTT/)

## Example Usage

Below is an example of how to use the WebVTT API in a WebSharper project:

```fsharp
namespace WebSharper.WebVTT.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.WebVTT

[<JavaScript>]
module Client =

    // Define the connection to the HTML template
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let currentCaption = Var.Create "None"

    // Function to set up video element with subtitles
    let setupVideoWithSubtitles () =
        let video = JS.Document.GetElementById("video") |> As<HTMLVideoElement>
        let captionDisplay = JS.Document.GetElementById("currentCaption")

        // Create a track element for subtitles
        let track = JS.Document.CreateElement("track")
        track?label <- "English"
        track?kind <- "subtitles"
        track?srclang <- "en"
        track?src <- "subtitles.vtt"
        track?``default`` <- true
        video.AppendChild(track) |> ignore

        // Log when video starts playing
        video.OnPlay <- (fun _ -> Console.Log("Video Started Playing"))

        // Load track and monitor active cues
        track.AddEventListener("load", fun (_: Dom.Event) ->
            let trackData = track?track |> As<TextTrack>
            trackData.Mode <- "hidden"

            // Update subtitles based on active cues
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
            // Initialize video subtitles setup
            .init(fun () -> setupVideoWithSubtitles())
            // Bind current subtitle text to UI
            .Subtitles(currentCaption.V)
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to create and manipulate WebVTT cues dynamically using the WebVTT API in WebSharper.

## Important Considerations

- **Browser Compatibility**: Ensure that your target browsers support the WebVTT API; check [MDN WebVTT API](https://developer.mozilla.org/en-US/docs/Web/API/WebVTT_API) for details.
- **Use Cases**: The WebVTT API is mainly used for adding captions, subtitles, and metadata to HTML5 video elements.
- **Performance**: The API is optimized for handling timed text tracks efficiently.
