using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GDF.Analytics
{
    /// <summary>
    /// Logs every analytics event into the Unity console. Default binding so games can see what the
    /// facade is receiving before plugging in a real vendor backend.
    /// </summary>
    public sealed class DebugAnalyticsBackend : IAnalyticsBackend
    {
        private const string LOG_COLOR = "#4ea3ff";

        /// <inheritdoc />
        public string Name => "Debug";

        /// <inheritdoc />
        public void Init()
        {
            Debug.Log($"<color={LOG_COLOR}>[Analytics:Debug]</color> Init");
        }

        /// <inheritdoc />
        public void TrackInfo(string eventName, IReadOnlyList<string> paramNames = null, IReadOnlyList<string> paramValues = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"<color={LOG_COLOR}>[Analytics:Debug]</color> info: ").Append(eventName);

            if (paramNames != null && paramValues != null && paramNames.Count == paramValues.Count)
            {
                for (int i = 0; i < paramNames.Count; i++)
                    builder.Append("\n  ").Append(paramNames[i]).Append(" = ").Append(paramValues[i]);
            }

            Debug.Log(builder.ToString());
        }

        /// <inheritdoc />
        public void TrackError(string error)
        {
            Debug.LogWarning($"<color={LOG_COLOR}>[Analytics:Debug]</color> error: {error}");
        }

        /// <inheritdoc />
        public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null)
        {
            string suffix = string.IsNullOrEmpty(progression02) ? progression01 : $"{progression01}/{progression02}";
            Debug.Log($"<color={LOG_COLOR}>[Analytics:Debug]</color> progression: {status} — {suffix}");
        }

        /// <inheritdoc />
        public void TrackPurchaseRevenue(string currency, float price)
        {
            Debug.Log($"<color={LOG_COLOR}>[Analytics:Debug]</color> purchase: {price:0.00} {currency}");
        }

        /// <inheritdoc />
        public void TrackAdsRevenue(string currency, float price)
        {
            Debug.Log($"<color={LOG_COLOR}>[Analytics:Debug]</color> ads revenue: {price:0.0000} {currency}");
        }
    }
}
