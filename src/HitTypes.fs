namespace pclytics

open System

(* Required parameters for a given hit type *)

type PageViewRequired = { Location : DocumentLocation }

type ScreenViewRequired = { ScreenName : string }

type TransactionRequired = { TransactionId : string }

type ItemRequired = { ItemName : string  }

type EventRequired = { Action : string }

type SocialRequired = { Network : string
                        Action : string
                        Target : string }

type TimingRequired = { Category : string
                        VariableName : string
                        Time : Millis }


(* Optional parameters for a given hit type *)

type PageViewOptional =
    | Title of string

type ScreenViewOptional = |S

type TransactionOptional =
    | Affiliation of string
    | Revenue of Decimal
    | Shipping of Decimal
    | Tax of Decimal

type ItemOptional =
    | Price of Decimal
    | Quantity of int
    | Code of string
    | Category of string
    | CurrencyCode of string

type EventOptional = |E
type SocialOptional = |S
type TimingOptional = |T
type ExceptionOptional = |E


(* "Others" allow you to pass parameters from other hit types along with the
   current hit *)

type PageViewOther =
    | Event of EventRequired
    | Item of ItemRequired
    | Social of SocialRequired
    | Timing of TimingRequired

type ScreenViewOther =
    | PageView of PageViewRequired

type TransactionOther =
    | Event of EventRequired
    | Item of ItemRequired
    | Social of SocialRequired
    | Timing of TimingRequired
    | ScreenView of ScreenViewRequired

type ItemOther =
    | Event of EventRequired
    | Social of SocialRequired
    | Timing of TimingRequired 

type EventOther = |E
type SocialOther = |S
type TimingOther = |T
type ExceptionOther = |E


(* Hits *)

type PageViewHit = { Params : PageViewRequired
                     Other : PageViewOther CaseSet }

type ScreenViewHit = { Params: ScreenViewRequired
                       Other : ScreenViewOther CaseSet }

type TransactionHit = { Params : TransactionRequired
                        Optional : TransactionOptional CaseSet
                        Other : TransactionOther CaseSet }

type ItemHit = { Item : ItemRequired
                 Transaction : TransactionRequired
                 Optional : ItemOptional CaseSet
                 Other : ItemOther CaseSet }

type EventHit = | Event
type SocialHit = | Social
type TimingHit = | Timing
type ExceptionHit = | Exception

type HitParams =
    | PageView of PageViewHit
    | ScreenView of ScreenViewHit
    | Event of EventHit
    | Transaction of TransactionHit
    | Item of ItemHit
    | Social of SocialHit
    | Exception of ExceptionHit
    | Timing of TimingHit


(* Universal params, available for any hit type *)

type IpOverride =
    | IpOverride of string
    | AnonymizeIp

type EnvironmentParams =
    | JavaEnabled of bool
    | FlashVersion of string

type DataSource =
    | App of string
    | Web
    | Custom

type SessionControl = Start | End

type HitMeta = { QueueTime : DateTime option
                 SessionControl : SessionControl option
                 Referrer : Uri option }

type CampaignParams = { CampaignName : string option }

type ClientOptional =
    | UserId of string
    | UserAgent of string
    | GeographicalId of string
    | IpOverride of IpOverride

type ClientParams =
    { TrackingId : TrackingId
      ClientId : Guid
      DataSource : DataSource
      Optional : ClientOptional CaseSet }

type Payload = { ClientParams : ClientParams
                 HitParams : HitParams
                 EnvironmentParams : EnvironmentParams CaseSet }
