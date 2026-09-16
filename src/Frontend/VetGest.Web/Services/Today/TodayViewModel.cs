namespace VetGest.Web.Services.Today;

public sealed record TodayViewModel(
    string PetName, string Species, string Breed, decimal WeightKg, int GestationalDay,
    int EstimatedGestationDays, int DaysRemaining, DateOnly EstimatedDueDate, string CurrentPhase,
    string PhaseSummary, string FetalDevelopment, string MaternalChanges, IReadOnlyList<string> CareGuidance,
    string NextReminder, string AlertSeverity, IReadOnlyList<TodayAlertItem> TriggeredAlerts,
    string AlertDisclaimer, string CalculationBasis,
    DateOnly ReferenceDate, string UncertaintyStatement, bool IsOverdue,
    TodayCareContent? CareContent = null)
{
    public int ProgressPercent => Math.Clamp((int)Math.Round(GestationalDay * 100d / EstimatedGestationDays), 0, 100);
}

public sealed record TodayAlertItem(string Name, string Description);

public sealed record TodayCareContent(
    bool IsAvailable, string Phase, string GestationalRange, string FetalDevelopment,
    string MaternalChanges, IReadOnlyList<TodayCareItem> GuidanceItems,
    string PublicationStatus, string ReviewStatus, string? Source, string? Reviewer,
    DateOnly? ReviewDate, string Disclaimer, string UncertaintyStatement);

public sealed record TodayCareItem(string Title, string Guidance);

public sealed record TodayState(bool IsLoading, bool IsDemo, bool IsOffline, TodayViewModel? Data,
    string? ErrorMessage, bool IsUnauthorized = false)
{
    public static TodayState Loading() => new(true, true, true, null, null);
    public static TodayState Demo(TodayViewModel data) => new(false, true, true, data, null);
    public static TodayState Empty() => new(false, true, true, null, null);
    public static TodayState AuthenticatedEmpty() => new(false, false, false, null, null);
    public static TodayState Live(TodayViewModel data) => new(false, false, false, data, null);
    public static TodayState Offline(string message) => new(false, false, true, null, message);
    public static TodayState Unauthorized(string message) => new(false, false, false, null, message, true);
    public static TodayState Error(string message) => new(false, false, false, null, message);
}