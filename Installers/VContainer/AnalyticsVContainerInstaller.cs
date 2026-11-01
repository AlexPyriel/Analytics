using System.Collections.Generic;
using VContainer;

namespace Analytics.Installers.VContainer
{
    /// <summary>
    /// Registers <see cref="IAnalyticsService"/> in a VContainer scope. Backends are supplied through
    /// the fluent <see cref="WithBackend"/> method; when none are added, a <see cref="DebugAnalyticsBackend"/>
    /// is registered so events are still visible in the console.
    /// </summary>
    public sealed class AnalyticsVContainerInstaller : IInstaller
    {
        private readonly List<IAnalyticsBackend> _backends = new List<IAnalyticsBackend>();

        /// <summary>Adds a backend to the dispatch list. Order of calls is preserved.</summary>
        public AnalyticsVContainerInstaller WithBackend(IAnalyticsBackend backend)
        {
            if (backend != null)
                _backends.Add(backend);

            return this;
        }

        /// <inheritdoc />
        public void Install(IContainerBuilder builder)
        {
            if (_backends.Count == 0)
                _backends.Add(new DebugAnalyticsBackend());

            IReadOnlyList<IAnalyticsBackend> frozen = _backends.ToArray();
            builder.RegisterInstance(frozen);
            builder
                .Register<Internal.AnalyticsService>(Lifetime.Singleton)
                .As<IAnalyticsService>();
        }
    }
}
