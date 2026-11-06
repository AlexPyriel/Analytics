using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GDF.Analytics.Internal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GDF.Analytics.Tests.EditMode
{
    /// <summary>
    /// Exercises <see cref="AnalyticsService"/> dispatch: every event fans out to every backend,
    /// backend exceptions never bubble to the caller, empty inputs are dropped.
    /// </summary>
    public sealed class AnalyticsServiceTests
    {
        [Test]
        public void TrackInfo_FanOutsToEveryBackend()
        {
            RecordingBackend a = new RecordingBackend("a");
            RecordingBackend b = new RecordingBackend("b");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { a, b });

            service.TrackInfo("event:test");

            Assert.AreEqual(1, a.InfoCalls.Count);
            Assert.AreEqual(1, b.InfoCalls.Count);
            Assert.AreEqual("event:test", a.InfoCalls[0].EventName);
        }

        [Test]
        public void TrackInfo_EmptyEventName_IsDropped()
        {
            RecordingBackend backend = new RecordingBackend("a");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { backend });

            service.TrackInfo(string.Empty);

            Assert.AreEqual(0, backend.InfoCalls.Count);
        }

        [Test]
        public void TrackInfo_SingleParamOverload_WrapsIntoLists()
        {
            RecordingBackend backend = new RecordingBackend("a");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { backend });

            service.TrackInfo("event:param", "slot", "slot1");

            Assert.AreEqual(1, backend.InfoCalls.Count);
            Assert.AreEqual("slot", backend.InfoCalls[0].ParamNames[0]);
            Assert.AreEqual("slot1", backend.InfoCalls[0].ParamValues[0]);
        }

        [Test]
        public void TrackError_FanOutsAndSkipsEmpty()
        {
            RecordingBackend backend = new RecordingBackend("a");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { backend });

            service.TrackError("boom");
            service.TrackError(string.Empty);

            Assert.AreEqual(1, backend.Errors.Count);
            Assert.AreEqual("boom", backend.Errors[0]);
        }

        [Test]
        public void TrackProgression_RequiresPrimaryUnit()
        {
            RecordingBackend backend = new RecordingBackend("a");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { backend });

            service.TrackProgression(EAnalyticsProgressionStatus.Start, "world_1", "level_3");
            service.TrackProgression(EAnalyticsProgressionStatus.Complete, string.Empty);

            Assert.AreEqual(1, backend.Progressions.Count);
            Assert.AreEqual(EAnalyticsProgressionStatus.Start, backend.Progressions[0].Status);
            Assert.AreEqual("world_1", backend.Progressions[0].P01);
            Assert.AreEqual("level_3", backend.Progressions[0].P02);
        }

        [Test]
        public void BackendException_IsSwallowed_OtherBackendsStillReceive()
        {
            ThrowingBackend throwing = new ThrowingBackend();
            RecordingBackend survivor = new RecordingBackend("survivor");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { throwing, survivor });
            LogAssert.Expect(LogType.Error, new Regex(@"\[Analytics\] Backend 'throwing' threw during dispatch"));

            Assert.DoesNotThrow(() => service.TrackInfo("event:x"));
            Assert.AreEqual(1, survivor.InfoCalls.Count, "Backend after a throwing one must still receive the event.");
        }

        [Test]
        public void Init_InvokesEveryBackend()
        {
            RecordingBackend a = new RecordingBackend("a");
            RecordingBackend b = new RecordingBackend("b");
            AnalyticsService service = new AnalyticsService(new IAnalyticsBackend[] { a, b });

            service.Init();

            Assert.AreEqual(1, a.InitCalls);
            Assert.AreEqual(1, b.InitCalls);
        }

        private sealed class RecordingBackend : IAnalyticsBackend
        {
            public RecordingBackend(string name) { Name = name; }

            public string Name { get; }
            public int InitCalls { get; private set; }
            public List<(string EventName, IReadOnlyList<string> ParamNames, IReadOnlyList<string> ParamValues)> InfoCalls { get; } = new();
            public List<string> Errors { get; } = new();
            public List<(EAnalyticsProgressionStatus Status, string P01, string P02)> Progressions { get; } = new();
            public List<(string Currency, float Price)> Purchases { get; } = new();
            public List<(string Currency, float Price)> Ads { get; } = new();

            public void Init() => InitCalls++;
            public void TrackInfo(string eventName, IReadOnlyList<string> paramNames = null, IReadOnlyList<string> paramValues = null)
                => InfoCalls.Add((eventName, paramNames, paramValues));
            public void TrackError(string error) => Errors.Add(error);
            public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null)
                => Progressions.Add((status, progression01, progression02));
            public void TrackPurchaseRevenue(string currency, float price) => Purchases.Add((currency, price));
            public void TrackAdsRevenue(string currency, float price) => Ads.Add((currency, price));
        }

        private sealed class ThrowingBackend : IAnalyticsBackend
        {
            public string Name => "throwing";
            public void Init() => throw new InvalidOperationException("init boom");
            public void TrackInfo(string eventName, IReadOnlyList<string> paramNames = null, IReadOnlyList<string> paramValues = null) => throw new InvalidOperationException("info boom");
            public void TrackError(string error) => throw new InvalidOperationException("error boom");
            public void TrackProgression(EAnalyticsProgressionStatus status, string progression01, string progression02 = null) => throw new InvalidOperationException("progression boom");
            public void TrackPurchaseRevenue(string currency, float price) => throw new InvalidOperationException("purchase boom");
            public void TrackAdsRevenue(string currency, float price) => throw new InvalidOperationException("ads boom");
        }
    }
}
