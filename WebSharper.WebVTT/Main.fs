namespace WebSharper.WebVTT

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator
open WebSharper.JavaScript.Dom

module Definition =

    module Enum = 

        let VTTCueVertical = 
            Pattern.EnumStrings "VTTCueVertical" [
                "rl"
                "lr"
            ]

        let VTTCueLineAlign = 
            Pattern.EnumStrings "VTTCueLineAlign" [
                "start"
                "center"
                "end"
            ]

        let VTTCuePositionAlign = 
            Pattern.EnumStrings "VTTCuePositionAlign" [
                "line-left"
                "center"
                "line-right"
                "auto"
            ]

        let VTTCueAlign = 
            Pattern.EnumStrings "VTTCueAlign" [
                "start"
                "center"
                "end"
                "left"
                "right"
            ]

    let TextTrackCue =
        Class "TextTrackCue"

    let VTTRegion =
        Class "VTTRegion"
        |+> Static [
            Constructor T<unit>
        ]
        |+> Instance [
            "id" =@ T<string>
            "width" =@ T<float> 
            "lines" =@ T<int> 
            "regionAnchorX" =@ T<float> 
            "regionAnchorY" =@ T<float> 
            "viewportAnchorX" =@ T<float> 
            "viewportAnchorY" =@ T<float> 
            "scroll" =@ T<string> 
        ]
        
    let VTTCue =
        Class "VTTCue"
        |=> Inherits TextTrackCue 
        |+> Static [
            Constructor (T<float>?startTime * T<float>?endTime * T<string>?text)
        ]
        |+> Instance [
            "region" =@ VTTRegion 
            "vertical" =@ Enum.VTTCueVertical 
            "snapToLines" =@ T<bool>
            "line" =@ T<string> + T<float>
            "lineAlign" =@ Enum.VTTCueLineAlign 
            "position" =@ T<string> + T<float>
            "positionAlign" =@ Enum.VTTCuePositionAlign 
            "size" =@ T<float> 
            "align" =@ Enum.VTTCueAlign 
            "text" =@ T<string>

            "getCueAsHTML" => T<unit> ^-> T<DocumentFragment> 
        ]

    let TextTrackCueList =
        Class "TextTrackCueList"
        |+> Instance [
            "length" =? T<int> 
            "getCueById" => T<string>?id ^-> VTTCue
            "" =@ VTTCue |> Indexed T<int>
        ]

    let TextTrack =
        Class "TextTrack"
        |=> Inherits T<EventTarget> 
        |+> Instance [
            "activeCues" =? TextTrackCueList
            "cues" =? TextTrackCueList
            "id" =? T<string>
            "inBandMetadataTrackDispatchType" =? T<string>
            "kind" =? T<string>
            "label" =? T<string>
            "language" =? T<string>
            "mode" =@ T<string>
            "sourceBuffer" =? T<obj> 

            "addCue" => TextTrackCue?cue ^-> T<unit> 
            "removeCue" => TextTrackCue?cue ^-> T<unit> 

            "oncuechange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnCueChange instead"
            "oncuechange" =@ T<Event> ^-> T<unit>
            |> WithSourceName "OnCueChange"
        ]

    TextTrackCue 
        |=> Inherits T<EventTarget> 
        |+> Instance [
            "track" =? TextTrack
            "id" =@ T<string>
            "startTime" =@ T<float>
            "endTime" =@ T<float>
            "pauseOnExit" =@ T<bool>

            "onenter" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnEnter instead"
            "onenter" =@ T<Event> ^-> T<unit>
            |> WithSourceName "OnEnter"
            "onexit" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnExit instead"
            "onexit" =@ T<Event> ^-> T<unit>
            |> WithSourceName "OnExit"
        ]
    |> ignore

    let TrackEventInit =
        Pattern.Config "TrackEventInit" {
            Required = []
            Optional = [
                ("track", T<obj>)
            ]
        }

    let TrackEvent =
        Class "TrackEvent"
        |=> Inherits T<Event> 
        |+> Static [
            Constructor (T<string>?``type`` * !?TrackEventInit?options) 
        ]
        |+> Instance [
            "track" =? T<obj> 
        ]

    let TextTrackList =
        Class "TextTrackList"
        |=> Inherits T<EventTarget> 
        |+> Instance [
            "length" =? T<int> 
            "getTrackById" => T<string>?id ^-> TextTrack 

            "onaddtrack" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnAddTrack instead"
            "onaddtrack" =@ TrackEvent ^-> T<unit>
            |> WithSourceName "OnAddTrack"
            "onchange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnChange instead"
            "onchange" =@ T<Event> ^-> T<unit>
            |> WithSourceName "OnChange"
            "onremovetrack" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnRemoveTrack instead"
            "onremovetrack" =@ TrackEvent ^-> T<unit>
            |> WithSourceName "OnRemoveTrack"
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.WebVTT" [
                TextTrackList
                TrackEvent
                TrackEventInit
                TextTrack
                TextTrackCueList
                VTTCue
                VTTRegion
                TextTrackCue
                Enum.VTTCueAlign
                Enum.VTTCuePositionAlign
                Enum.VTTCueLineAlign
                Enum.VTTCueVertical
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
