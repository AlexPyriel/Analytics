using System.Collections.Generic;

namespace GDF.Analytics
{
    /// <summary>
    /// Facade over one or more <see cref="IAnalyticsBackend"/> implementations. Dispatches every
    /// event to every registered backend; the facade is non-throwing, backend-level exceptions are
    /// swallowed and logged so a vendor outage never breaks game code.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>Initializes every registered backend. Call once at app startup, before firing events.</summary>
        public void Init();

        /// <summary>Fires a design/custom event with no extra parameters.</summary>
        /// <param name="eventName">Event id (GA-style <c>namespace:scope:id</c> recommended).</param>
        public void TrackInfo(string eventName);

        /// <summary>Fires a design event with a single <c>name=value</c> parameter pair.</summary>
        public void TrackInfo(string eventName, string paramName, string paramValue);

        /// <summary>Fires a design event with multiple aligned <c>name=value</c> pairs.</summary>
        public void TrackInfo(string eventName, IReadOnlyList<string> paramNames, IReadOnlyList<string> paramValues);

        /// <summary>Reports a non-fatal error.</summary>
        public void TrackError(string error);

        /// <summary>Reports a progression event (level/boss/tutorial step).</summary>
        public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null);

        /// <summary>Reports IAP revenue.</summary>
        public void TrackPurchaseRevenue(string currency, float price);

        /// <summary>Reports eCPM/ad revenue.</summary>
        public void TrackAdsRevenue(string currency, float price);
    }
}
