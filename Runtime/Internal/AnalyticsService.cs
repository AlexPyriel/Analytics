using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDF.Analytics.Internal
{
    /// <summary>
    /// Default <see cref="IAnalyticsService"/> implementation. Dispatches every call to every
    /// registered <see cref="IAnalyticsBackend"/>, catching per-backend exceptions so a vendor outage
    /// never propagates to the caller.
    /// </summary>
    public sealed class AnalyticsService : IAnalyticsService
    {
        private readonly IReadOnlyList<IAnalyticsBackend> _backends;

        /// <summary>Creates the facade over the given backends; events fan out to each of them in order.</summary>
        /// <param name="backends">Vendor backends; register at least <see cref="DebugAnalyticsBackend"/> when none are configured.</param>
        public AnalyticsService(IReadOnlyList<IAnalyticsBackend> backends)
        {
            _backends = backends ?? throw new ArgumentNullException(nameof(backends));
        }

        /// <inheritdoc />
        public void Init()
        {
            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], static b => b.Init());
        }

        /// <inheritdoc />
        public void TrackInfo(string eventName)
        {
            TrackInfo(eventName, (IReadOnlyList<string>)null, (IReadOnlyList<string>)null);
        }

        /// <inheritdoc />
        public void TrackInfo(string eventName, string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                TrackInfo(eventName, (IReadOnlyList<string>)null, (IReadOnlyList<string>)null);
                return;
            }

            TrackInfo(eventName, (IReadOnlyList<string>)new[] { paramName }, (IReadOnlyList<string>)new[] { paramValue });
        }

        /// <inheritdoc />
        public void TrackInfo(string eventName, IReadOnlyList<string> paramNames, IReadOnlyList<string> paramValues)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[Analytics] TrackInfo called with an empty event name; dropped.");
                return;
            }

            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], (eventName, paramNames, paramValues),
                    static (b, state) => b.TrackInfo(state.eventName, state.paramNames, state.paramValues));
        }

        /// <inheritdoc />
        public void TrackError(string error)
        {
            if (string.IsNullOrEmpty(error))
                return;

            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], error, static (b, e) => b.TrackError(e));
        }

        /// <inheritdoc />
        public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null)
        {
            if (string.IsNullOrEmpty(progression01))
            {
                Debug.LogWarning("[Analytics] TrackProgression called with an empty primary progression; dropped.");
                return;
            }

            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], (status, progression01, progression02),
                    static (b, state) => b.TrackProgression(state.status, state.progression01, state.progression02));
        }

        /// <inheritdoc />
        public void TrackPurchaseRevenue(string currency, float price)
        {
            if (string.IsNullOrEmpty(currency))
            {
                Debug.LogWarning("[Analytics] TrackPurchaseRevenue called with an empty currency; dropped.");
                return;
            }

            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], (currency, price),
                    static (b, state) => b.TrackPurchaseRevenue(state.currency, state.price));
        }

        /// <inheritdoc />
        public void TrackAdsRevenue(string currency, float price)
        {
            if (string.IsNullOrEmpty(currency))
            {
                Debug.LogWarning("[Analytics] TrackAdsRevenue called with an empty currency; dropped.");
                return;
            }

            for (int i = 0; i < _backends.Count; i++)
                SafeInvoke(_backends[i], (currency, price),
                    static (b, state) => b.TrackAdsRevenue(state.currency, state.price));
        }

        private static void SafeInvoke(IAnalyticsBackend backend, Action<IAnalyticsBackend> action)
        {
            if (backend == null)
                return;

            try
            {
                action(backend);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Analytics] Backend '{backend.Name}' threw during dispatch: {exception.Message}\n{exception.StackTrace}");
            }
        }

        private static void SafeInvoke<TState>(IAnalyticsBackend backend, TState state, Action<IAnalyticsBackend, TState> action)
        {
            if (backend == null)
                return;

            try
            {
                action(backend, state);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Analytics] Backend '{backend.Name}' threw during dispatch: {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}
