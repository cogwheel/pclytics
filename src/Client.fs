namespace pclytics

open System
open System.Net
open System.Net.Http

open pclytics.Reflection

type HitType = 
    | PageView
    | ScreenView
    | Event
    | Transaction
    | Item
    | Social
    | Exception
    | Timing
    override t.ToString() = (getValue t).ToLower()

type DataSource =
    | Default | App
    | Web
    | Custom of string
    override s.ToString() = match s with
                            | Default | App -> "app"
                            | Web -> "web"
                            | Custom s -> s

[<CustomEquality; CustomComparison>]
type OptionalClientParam =
    | [<Name "uip">]   IpOverride of string
    | [<Name "uid">]   UserId of string
    | [<Name "geoid">] GeographicalId of string
    | [<Name "je">]    JavaEnabled of bool
    | [<Name "fl">]    FlashVersion of string
    override p.ToString() = getValue p

    // OptionalParams are equal by name only, effectively making a set of them
    // into a map of NameAtribute -> case value.
    override p.Equals q = typesEqual p q
    override p.GetHashCode () = typeHash p
    interface IComparable with
        member p.CompareTo(q) = hashEqual p q


type ClientParams = { 
                      [<Name "tid">] TrackingPropertyId : string
                      [<Name "cid">] ClientId : Guid
                      [<Name "aip">] AnonymizeIp : bool
                      [<Name "ds">]  DataSource : DataSource
                      [<Name "ua">]  UserAgent : string
                      [<Name "an">]  ApplicationName : string
                      OptionalParams : OptionalClientParam Set
                    }

type SessionControl = 
    | Start | End
    override t.ToString() = (getValue t).ToLower()

type Dimension (w : uint32, h : uint32) =
    override r.ToString() = String.Join ("x", w.ToString(), h.ToString())

type Bits (b : uint16) =
    override n.ToString() = b.ToString() + "-bits"

[<CustomEquality; CustomComparison>]
type OptionalEventParam = 
    | [<Name "qt">]    QueueTime of DateTime
    | [<Name "sc">]    SessionControl of SessionControl
    | [<Name "dr">]    Referrer of Uri
    | [<Name "cn">]    CampaignName of string
    | [<Name "cs">]    CampaignSource of string
    | [<Name "cm">]    CampaignMedium of string
    | [<Name "ck">]    CampaignKeyword of string
    | [<Name "cc">]    CampaignContent of string
    | [<Name "ci">]    CampaignId of string
    | [<Name "gclid">] AdWordsId of string
    | [<Name "dclid">] DisplayAdsId of string
    | [<Name "sr">]    ScreenResolution of Dimension
    | [<Name "vp">]    ViewportSize of Dimension
    | [<Name "de">]    DocumentEncoding of string
    | [<Name "sd">]    ScreenColorBits of Bits
    | [<Name "ul">]    UserLanguage of string
    | [<Name "ni">]    NonInteractionHit of bool
    | [<Name "dl">]    DocumentLocation of Uri
    | [<Name "cd">]    ScreenName of string
    // dh DocumentHost string
    // dp DocumentPath string
    // dt DocumentTitle string
    // ... TODO

    override p.ToString() = match p with
                            // TimeStamp is reported as milliseconds passed since the occurrance
                            | QueueTime t -> let diff = DateTime.Now - t
                                             let millis = diff.Ticks / TimeSpan.TicksPerMillisecond
                                             millis.ToString()
                            | o -> getValue o


    // Same semantics as OptionalClientParam
    override p.Equals q = typesEqual p q
    override p.GetHashCode () = typeHash p
    interface IComparable with
        member p.CompareTo(q) = hashEqual p q

type EventParams = { [<Name "t">] HitType : HitType
                     OptionalParams : OptionalEventParam Set }

type Client (clientParams : ClientParams) =
    let protocolVersion = "v=1"
    let eventProto = seq { yield protocolVersion
                           yield! getRecordPairs clientParams
                           yield! getUnionPairs clientParams.OptionalParams }

    let endpoint = "https://ssl.google-analytics.com/debug/collect"

    let client = let c = new HttpClient()
                 c.DefaultRequestHeaders.Accept.Add(new Headers.MediaTypeWithQualityHeaderValue("application/json"))
                 c.DefaultRequestHeaders.UserAgent.ParseAdd(clientParams.UserAgent)
                 c

    member c.SendEvent eventParams =
        let payload = seq { yield! eventProto
                            yield! getRecordPairs eventParams
                            yield! getUnionPairs eventParams.OptionalParams }
                      |> Seq.reduce (fun p1 p2 -> p1 + "&" + p2)
        async {
            use! response = client.PostAsync(endpoint, new StringContent(payload)) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }

    interface IDisposable with
        member c.Dispose() =
            client.Dispose()

