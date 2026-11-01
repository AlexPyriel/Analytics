namespace Analytics
{
    /// <summary>
    /// Progression event status, mirrored on common vendor funnels (GameAnalytics, Firebase).
    /// Report at every meaningful stage of a progression unit — level, boss fight, tutorial step.
    /// </summary>
    public enum EAnalyticsProgressionStatus
    {
        /// <summary>Progression unit has started.</summary>
        Start = 0,

        /// <summary>Progression unit was completed successfully.</summary>
        Complete = 1,

        /// <summary>Progression unit failed before completion.</summary>
        Fail = 2,
    }
}
