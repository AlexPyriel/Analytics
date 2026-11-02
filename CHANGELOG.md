# Changelog

## [1.0.1] - 2026-11-02

### Changed

- **Breaking:** root namespace renamed from `Analytics` to `GDF.Analytics`. Consumers must update
  `using Analytics;` → `using GDF.Analytics;` (and the installer sub-namespaces similarly). The
  rename removes a conflict with Unity's built-in `UnityEngine.Analytics` namespace that could cause
  `IAnalyticsService` to silently resolve to the wrong type when both were in scope.

### Fixed

- VContainer installer compilation: added missing `using VContainer.Unity;` so `IInstaller`
  (which lives in `VContainer.Unity`, not `VContainer`) resolves. Without this fix the
  installer assembly failed with `CS0246`.

## [1.0.0] - 2026-11-02

Initial public release.

- `IAnalyticsService` facade exposing the full practical event surface: `TrackInfo` (overloads for 0, 1 and N parameters), `TrackError`, `TrackProgression`, `TrackPurchaseRevenue`, `TrackAdsRevenue`, `Init`.
- `IAnalyticsBackend` contract for vendor-side implementations (GameAnalytics, Firebase, mixpanel, PlayFab, custom endpoints).
- Composite `AnalyticsService` dispatches every event to every registered backend; per-backend exceptions are caught and logged so a vendor outage never breaks game code.
- `DebugAnalyticsBackend` default — logs structured events to the Unity console.
- DI installers for VContainer, Zenject and Reflex. Fluent `WithBackend` on the VContainer installer; subclass hook `BuildBackends` on the Reflex installer; `InstallerArgs` on the Zenject installer.
- EditMode tests cover dispatch, exception isolation, and dropped empty inputs.
