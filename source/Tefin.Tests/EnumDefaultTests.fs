module EnumDefaultTests

open System
open System.Threading.Tasks

open Google.Protobuf.Reflection

open Tefin.Core.Reflection

open Xunit

// A proto3 enum's zero member is a sentinel meaning "the caller never set this field". Carrying it
// on the wire tells a validating server the field was omitted, so a generated default must skip it
// and pick a member that carries a real value. Authors spell the sentinel in several ways and only
// the [OriginalName] attribute records which, so every spelling below has to be recognised.
//
// Each enum also declares a real member: an enum with only a sentinel has nothing else to offer,
// which is a separate case covered further down.

type private SuffixedSentinel =
    | [<OriginalName("ADDRESS_TYPE_UNSPECIFIED")>] Unset = 0
    | [<OriginalName("ADDRESS_TYPE_BILLING")>] Billing = 1

type private BareSentinel =
    | [<OriginalName("UNSPECIFIED")>] Unset = 0
    | [<OriginalName("READY")>] Ready = 1

type private BareUndefinedSentinel =
    | [<OriginalName("UNDEFINED")>] Unset = 0
    | [<OriginalName("READY")>] Ready = 1

type private SuffixedUndefinedSentinel =
    | [<OriginalName("STATUS_UNDEFINED")>] Unset = 0
    | [<OriginalName("STATUS_READY")>] Ready = 1

type private LowercaseSentinel =
    | [<OriginalName("undefined")>] Unset = 0
    | [<OriginalName("ready")>] Ready = 1

type private NoUnderscoreSentinel =
    | [<OriginalName("STATUSUNSPECIFIED")>] Unset = 0
    | [<OriginalName("STATUSREADY")>] Ready = 1

type private UnknownSentinel =
    | [<OriginalName("UNKNOWN")>] Unset = 0
    | [<OriginalName("READY")>] Ready = 1

type private MixedCaseSentinel =
    | [<OriginalName("Status_Unspecified")>] Unset = 0
    | [<OriginalName("Status_Ready")>] Ready = 1

// A protobuf enum whose zero member carries a real meaning. A means something, so it must stay
// reachable - filtering every zero would silently drop it.
type private MeaningfulZero =
    | [<OriginalName("A")>] A = 0
    | [<OriginalName("B")>] B = 1

// The prefix of a sentinel is arbitrary - it does not have to be the enum's own type prefix - and a
// member that merely carries a sentinel word is a real value that has to stay reachable.
type private KnownSuffix =
    | [<OriginalName("FOO_UNSPECIFIED")>] Unset = 0
    | [<OriginalName("FOO_KNOWN")>] Known = 1

// Nothing but a sentinel: the safest thing left is to return it rather than throw.
type private AllSentinel =
    | [<OriginalName("X_UNSPECIFIED")>] Unset = 0

// No protobuf attribute anywhere, so its zero is an ordinary value like DayOfWeek.Sunday.
type private PlainEnum =
    | Zero = 0
    | One = 1

// Used only by the concurrency test, so its cache entry is created while several threads race.
type private FirstTouch =
    | [<OriginalName("Z_UNSPECIFIED")>] Unset = 0
    | [<OriginalName("Z_READY")>] Ready = 1

let private defaultOf (t: Type) = TypeBuilder.getDefault "x" t true None 1

let private pick (t: Type) =
    let struct (ok, v) = defaultOf t
    Assert.True(ok, sprintf "%s reported not handled" t.Name)
    Convert.ToInt64 v

let private zeroPicks (t: Type) (runs: int) =
    let mutable hits = 0
    for _ in 1..runs do
        let struct (ok, v) = defaultOf t
        Assert.True(ok, sprintf "%s reported not handled" t.Name)
        if Convert.ToInt64 v = 0L then hits <- hits + 1
    hits

let private sentinelShapes : (string * Type) list =
    [ "suffixed", typeof<SuffixedSentinel>
      "bare", typeof<BareSentinel>
      "suffixed-undefined", typeof<SuffixedUndefinedSentinel>
      "bare-undefined", typeof<BareUndefinedSentinel>
      "lowercase", typeof<LowercaseSentinel>
      "no-underscore", typeof<NoUnderscoreSentinel>
      "unknown", typeof<UnknownSentinel>
      "mixed-case", typeof<MixedCaseSentinel> ]

[<Fact>]
let ``no sentinel spelling is ever picked as the default`` () =
    for (shape, t) in sentinelShapes do
        let hits = zeroPicks t 200
        Assert.True((hits = 0), sprintf "the %s sentinel was picked %d times out of 200" shape hits)

[<Fact>]
let ``a sentinel-shaped enum offers its real members`` () =
    let seen =
        [ for _ in 1..200 -> pick typeof<SuffixedSentinel> ] |> List.distinct

    Assert.True((seen = [ 1L ]), sprintf "picked %A" seen)

[<Fact>]
let ``a protobuf enum whose zero member has a meaning keeps it`` () =
    // A means something, so dropping it would lose a legal choice
    let hits = zeroPicks typeof<MeaningfulZero> 400
    Assert.True((hits > 0), "the meaningful A = 0 member was never picked")

[<Fact>]
let ``a real member is not mistaken for a sentinel`` () =
    let seen =
        [ for _ in 1..400 -> pick typeof<KnownSuffix> ] |> List.distinct

    Assert.True((seen = [ 1L ]), sprintf "picked %A" seen)

[<Fact>]
let ``an enum with nothing but a sentinel still returns a value`` () =
    let struct (ok, v) = defaultOf typeof<AllSentinel>
    Assert.True(ok)
    Assert.Equal(0L, Convert.ToInt64 v)

[<Fact>]
let ``a plain enum keeps its zero member`` () =
    // no [OriginalName] anywhere, so zero is an ordinary value and must not be filtered
    let hits = zeroPicks typeof<PlainEnum> 400
    Assert.True((hits > 0), "the zero member of a plain enum was never picked")

[<Fact>]
let ``concurrent first use of a type never returns a sentinel`` () =
    let runs = 200
    let counts =
        [| for _ in 1..8 -> Task.Run<int>(Func<int>(fun () -> zeroPicks typeof<FirstTouch> runs)) |]
        |> Task.WhenAll
        |> fun t -> t.GetAwaiter().GetResult()

    Assert.Equal(0, Array.sum counts)

[<Fact>]
let ``the cached choice is stable across repeated calls`` () =
    // the cache must not change the distribution into always-returning the same member
    let seen =
        [ for _ in 1..400 -> pick typeof<BareSentinel> ] |> List.distinct

    Assert.True((seen = [ 1L ]), sprintf "picked %A" seen)
