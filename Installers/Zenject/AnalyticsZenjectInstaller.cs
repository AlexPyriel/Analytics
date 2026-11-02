using System.Collections.Generic;
using GDF.Analytics.Internal;
using Zenject;

namespace GDF.Analytics.Installers.Zenject
{
    /// <summary>
    /// Registers <see cref="IAnalyticsService"/> in a Zenject container. Backends are supplied as a
    /// list through <c>WithBackends</c>; a <see cref="DebugAnalyticsBackend"/> is used when none are
    /// provided.
    /// </summary>
    public sealed class AnalyticsZenjectInstaller : Installer<IReadOnlyList<IAnalyticsBackend>, AnalyticsZenjectInstaller>
    {
        [Inject]
        private IReadOnlyList<IAnalyticsBackend> _backends;

        /// <inheritdoc />
        public override void InstallBindings()
        {
            IReadOnlyList<IAnalyticsBackend> source = _backends;
            if (source == null || source.Count == 0)
                source = new IAnalyticsBackend[] { new DebugAnalyticsBackend() };

            Container.BindInstance(source);
            Container
                .Bind<IAnalyticsService>()
                .To<AnalyticsService>()
                .AsSingle();
        }
    }
}
