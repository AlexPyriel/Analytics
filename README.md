# GDF Analytics

Vendor-agnostic analytics facade for Unity. Composite dispatch across pluggable backends
(GameAnalytics, Firebase, mixpanel, PlayFab, custom). Design events, errors, progression,
purchase revenue and ad revenue.

Package name: `com.gdf.analytics`
Assembly name: `GDF.Analytics`

## Features

- `IAnalyticsService` facade: `TrackInfo` (0, 1 or N params), `TrackError`, `TrackProgression`, `TrackPurchaseRevenue`, `TrackAdsRevenue`, `Init`.
- `IAnalyticsBackend` contract for vendor adapters; register one per vendor you ship.
- Composite dispatch: every event fans out to every backend. Per-backend exceptions are swallowed and logged — one vendor outage does not break the facade or neighbouring backends.
- `DebugAnalyticsBackend` default — logs every event into the Unity console (useful during prototyping and playtest).
- DI installers for VContainer, Zenject and Reflex.
- Non-throwing API: empty event names, empty currencies and empty primary progression units are dropped with a warning rather than throwing on the caller.

## Requirements

- Unity `6000.0+`
- One of the supported DI containers (optional, required only when using the matching installer):
  - `jp.hadashikick.vcontainer`
  - `com.mathijsbakker.extenject`
  - `com.gustavopsantos.reflex`

## Installation (UPM via Git)

Add this dependency to `Packages/manifest.json`:

```json
"com.gdf.analytics": "https://github.com/AlexPyriel/Analytics.git#v1.0.0"
```

For development tracking (not recommended for production), use `#main` instead of a tag.

You can also install it from Unity Package Manager:

- `Window -> Package Manager -> + -> Add package from git URL...`
- `https://github.com/AlexPyriel/Analytics.git#v1.0.0`

## Usage

### 1. Register the facade with your backends

VContainer:

```csharp
protected override void Configure(IContainerBuilder builder)
{
    new AnalyticsVContainerInstaller()
        .WithBackend(new DebugAnalyticsBackend())
        .WithBackend(new GameAnalyticsBackend())    // vendor adapter you authored
        .Install(builder);
}
```

Zenject:

```csharp
AnalyticsZenjectInstaller.Install(Container, new IAnalyticsBackend[]
{
    new DebugAnalyticsBackend(),
    new GameAnalyticsBackend(),
});
```

Reflex:

```csharp
public sealed class GameAnalyticsReflexInstaller : AnalyticsReflexInstaller
{
    protected override IReadOnlyList<IAnalyticsBackend> BuildBackends() =>
        new IAnalyticsBackend[] { new DebugAnalyticsBackend(), new GameAnalyticsBackend() };
}
```

### 2. Initialize at startup

```csharp
_analytics.Init();
```

### 3. Fire events anywhere

```csharp
_analytics.TrackInfo("level:completed", "level_id", "1_3");
_analytics.TrackProgression(EAnalyticsProgressionStatus.Complete, "world_1", "level_3");
_analytics.TrackPurchaseRevenue("USD", 4.99f);
_analytics.TrackAdsRevenue("USD", 0.012f);
_analytics.TrackError("checkout_sdk_timeout");
```

### 4. Author a vendor backend

Implement `IAnalyticsBackend` once per vendor. Minimal GameAnalytics shim:

```csharp
public sealed class GameAnalyticsBackend : IAnalyticsBackend
{
    public string Name => "GameAnalytics";

    public void Init() => GameAnalyticsSDK.GameAnalytics.Initialize();

    public void TrackInfo(string eventName, IReadOnlyList<string> _ = null, IReadOnlyList<string> __ = null)
        => GameAnalyticsSDK.GameAnalytics.NewDesignEvent(eventName);

    public void TrackError(string error)
        => GameAnalyticsSDK.GameAnalytics.NewErrorEvent(GAErrorSeverity.Warning, error);

    public void TrackProgression(EAnalyticsProgressionStatus status, string p01, string p02 = null)
        => GameAnalyticsSDK.GameAnalytics.NewProgressionEvent((GAProgressionStatus)status, p01, p02);

    public void TrackPurchaseRevenue(string currency, float price)
        => GameAnalyticsSDK.GameAnalytics.NewBusinessEventIAP(currency, (int)(price * 100), "iap", "receipt", null, null);

    public void TrackAdsRevenue(string currency, float price)
        => GameAnalyticsSDK.GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, "network", "placement");
}
```

## Assembly Definition Notes

`GDF.Analytics` is a dedicated runtime assembly. DI installers live in their own assemblies
(`GDF.Analytics.Installers.VContainer`, `.Zenject`, `.Reflex`) so games that only use one container
do not pull in the others. The internal `AnalyticsService` is accessed by installers through
`InternalsVisibleTo`, keeping the public API free of the orchestrator type.

## License

MIT. See `LICENSE.md`.
