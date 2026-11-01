using System.Collections.Generic;

namespace Analytics
{
    /// <summary>
    /// Vendor-side implementation of a single analytics backend (GameAnalytics, Firebase, mixpanel,
    /// PlayFab, custom endpoint). Composed into the facade through <see cref="IAnalyticsService"/>;
    /// the service dispatches every event to every registered backend.
    /// </summary>
    /// <remarks>
    /// Implementations are expected to be best-effort and non-throwing: an outage in one vendor must
    /// not disrupt the facade or other backends. Fail-safe behavior is enforced inside the facade.
    /// </remarks>
    public interface IAnalyticsBackend
    {
        /// <summary>Short human-readable name for logs ("GameAnalytics", "Firebase", "Debug").</summary>
        public string Name { get; }

        /// <summary>Initializes the backend (SDK keys, network handshake). Called once from the facade's Init.</summary>
        public void Init();

        /// <summary>
        /// Fires a design/custom event. Parameter arrays are optional and must be the same length
        /// when both provided; implementations should log + skip on mismatch rather than throwing.
        /// </summary>
        /// <param name="eventName">Event id. GA-style <c>namespace:scope:id</c> is recommended.</param>
        /// <param name="paramNames">Optional parameter names.</param>
        /// <param name="paramValues">Optional parameter values aligned with <paramref name="paramNames"/>.</param>
        public void TrackInfo(string eventName, IReadOnlyList<string> paramNames = null, IReadOnlyList<string> paramValues = null);

        /// <summary>Reports a non-fatal error with a human-readable message.</summary>
        /// <param name="error">Error description.</param>
        public void TrackError(string error);

        /// <summary>Reports a progression event: level/boss/tutorial step enter/complete/fail.</summary>
        /// <param name="status">Progression status.</param>
        /// <param name="progression01">Primary unit, e.g. world id.</param>
        /// <param name="progression02">Secondary unit, e.g. level id. Pass <see langword="null"/> when not applicable.</param>
        public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null);

        /// <summary>Reports IAP revenue after a successful purchase.</summary>
        /// <param name="currency">ISO 4217 currency code (e.g. "USD").</param>
        /// <param name="price">Price value in <paramref name="currency"/>.</param>
        public void TrackPurchaseRevenue(string currency, float price);

        /// <summary>Reports eCPM/ad revenue after a successful ad impression (rewarded, interstitial, banner).</summary>
        /// <param name="currency">ISO 4217 currency code.</param>
        /// <param name="price">Price value in <paramref name="currency"/>.</param>
        public void TrackAdsRevenue(string currency, float price);
    }
}
