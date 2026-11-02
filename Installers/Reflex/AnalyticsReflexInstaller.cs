using System.Collections.Generic;
using GDF.Analytics.Internal;
using Reflex.Core;
using UnityEngine;

namespace GDF.Analytics.Installers.Reflex
{
    /// <summary>
    /// Registers <see cref="IAnalyticsService"/> in a Reflex container with a single
    /// <see cref="DebugAnalyticsBackend"/> by default. Clones that need vendor backends subclass
    /// this installer and override <see cref="BuildBackends"/>, or they can replace the installer
    /// entirely with a hand-authored one.
    /// </summary>
    public class AnalyticsReflexInstaller : MonoBehaviour, IInstaller
    {
        /// <inheritdoc />
        public void InstallBindings(ContainerBuilder builder)
        {
            IReadOnlyList<IAnalyticsBackend> backends = BuildBackends();
            if (backends == null || backends.Count == 0)
                backends = new IAnalyticsBackend[] { new DebugAnalyticsBackend() };

            builder.AddSingleton(backends, typeof(IReadOnlyList<IAnalyticsBackend>));
            builder.AddSingleton(
                typeof(AnalyticsService),
                typeof(IAnalyticsService));
        }

        /// <summary>Hook for subclasses to provide concrete backends (vendor SDKs, custom endpoints).</summary>
        protected virtual IReadOnlyList<IAnalyticsBackend> BuildBackends()
        {
            return null;
        }
    }
}
