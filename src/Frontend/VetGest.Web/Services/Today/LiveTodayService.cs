using VetGest.Web.Services.Api;
using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Today;

public sealed class LiveTodayService(PetService petService, PregnancyService pregnancyService,
    CareContentService careContentService, AlertService alertService, ExaminationService examinationService) : ITodayService
{
    public async Task<TodayState> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var petsResult = await petService.GetPetsAsync(cancellationToken);
        if (!petsResult.Succeeded)
        {
            return petsResult.IsUnauthorized
                ? TodayState.Unauthorized(petsResult.Error ?? "Entre novamente para continuar.")
                : petsResult.IsOffline
                    ? TodayState.Offline(petsResult.Error ?? "Verifique sua conexão e tente novamente.")
                    : TodayState.Error(petsResult.Error ?? "Não foi possível carregar seus pets.");
        }

        var pet = petsResult.Data?.FirstOrDefault(item => item.IsActive);
        if (pet is null)
        {
            return TodayState.AuthenticatedEmpty();
        }

        var pregnancyResult = await pregnancyService.GetCurrentAsync(pet.Id, cancellationToken);
        if (!pregnancyResult.Succeeded)
        {
            return pregnancyResult.IsNotFound
                ? TodayState.AuthenticatedEmpty()
                : pregnancyResult.IsUnauthorized
                    ? TodayState.Unauthorized(pregnancyResult.Error ?? "Entre novamente para continuar.")
                    : pregnancyResult.IsOffline
                        ? TodayState.Offline(pregnancyResult.Error ?? "Verifique sua conexão e tente novamente.")
                        : TodayState.Error(pregnancyResult.Error ?? "Não foi possível carregar a gestação.");
        }

        var pregnancy = pregnancyResult.Data!;
        var calculation = pregnancy.Calculation;
        var careResult = await careContentService.GetAsync(pet.Id, pregnancy.Id, cancellationToken);
        if (careResult.IsUnauthorized)
        {
            return TodayState.Unauthorized(careResult.Error ?? "Você não tem acesso a este conteúdo.");
        }

        if (careResult.IsOffline)
        {
            return TodayState.Offline(careResult.Error ?? "Verifique sua conexão e tente novamente.");
        }

        if (!careResult.Succeeded && !careResult.IsNotFound)
        {
            return TodayState.Error(careResult.Error ?? "Não foi possível carregar o conteúdo educativo.");
        }

        var careContent = careResult.Succeeded && careResult.Data is not null
            ? MapCareContent(careResult.Data)
            : null;

        // Alerts are supplementary to the Today summary; a failure here should not block the whole page.
        var alertsResult = await alertService.GetAsync(pet.Id, pregnancy.Id, cancellationToken);
        var (alertSeverity, triggeredAlerts, alertDisclaimer) = alertsResult.Succeeded && alertsResult.Data is not null
            ? (alertsResult.Data.HighestSeverity.ToString(), MapTriggeredAlerts(alertsResult.Data), alertsResult.Data.Disclaimer)
            : ("Normal", (IReadOnlyList<TodayAlertItem>)Array.Empty<TodayAlertItem>(),
                "Não foi possível carregar os alertas agora. Tente novamente mais tarde.");

        // Examination reminders are supplementary to Today; a failure must not block the summary.
        var examinationsResult = await examinationService.GetAsync(pet.Id, pregnancy.Id, cancellationToken);
        var nextReminder = examinationsResult.Succeeded && examinationsResult.Data is not null
            ? examinationsResult.Data.Where(item => !item.IsPast).FirstOrDefault(item => item.IsDueSoon)
                ?? examinationsResult.Data.FirstOrDefault(item => !item.IsPast)
            : null;

        var estimatedDays = Math.Max((calculation.EstimatedDueDate.Date - calculation.ReferenceDate.Date).Days, 1);
        var daysRemaining = Math.Max((calculation.EstimatedDueDate.Date - DateTime.UtcNow.Date).Days, 0);
        var phase = calculation.Phase ?? "Fase não informada";
        var phaseSummary = careContent is { IsAvailable: true }
            ? $"Janela gestacional estimada: {careContent.GestationalRange}."
            : "O conteúdo educativo desta fase ainda não está disponível.";
        return TodayState.Live(new TodayViewModel(
            pet.Name, pregnancy.Species, pet.Breed ?? "", pet.CurrentWeight ?? 0, calculation.GestationalDay,
            estimatedDays, daysRemaining, DateOnly.FromDateTime(calculation.EstimatedDueDate), phase,
            phaseSummary,
            careContent?.FetalDevelopment ?? "Conteúdo educativo não disponível para esta fase.",
            careContent?.MaternalChanges ?? "Conteúdo educativo não disponível para esta fase.",
            careContent?.GuidanceItems.Select(item => $"{item.Title}: {item.Guidance}").ToArray() ?? [],
            nextReminder?.Title ?? "Planeje a próxima conversa", alertSeverity, triggeredAlerts, alertDisclaimer,
            calculation.Basis == VetGest.Contracts.Pregnancies.PregnancyCalculationBasis.OvulationDate
                ? "Data de ovulação/progesterona"
                : "Data de cobertura",
            DateOnly.FromDateTime(calculation.ReferenceDate), calculation.UncertaintyStatement, calculation.IsOverdue,
            careContent));
    }

    private static IReadOnlyList<TodayAlertItem> MapTriggeredAlerts(TodayAlertsDto dto) =>
        dto.TriggeredAlerts.Select(alert => new TodayAlertItem(alert.Name, alert.Description)).ToArray();

    private static TodayCareContent MapCareContent(CareContentDto content) => new(
        content.IsAvailable,
        content.Phase ?? "Fase não informada",
        FormatGestationalRange(content.StartDayGestation, content.EndDayGestation),
        content.FetalDevelopment ?? "Conteúdo não informado.",
        content.MaternalChanges ?? "Conteúdo não informado.",
        content.GuidanceItems.Select(item => new TodayCareItem(item.Title, item.Guidance)).ToArray(),
        content.PublicationStatus,
        content.ReviewStatus,
        content.Source,
        content.Reviewer,
        content.ReviewDate is { } reviewDate ? DateOnly.FromDateTime(reviewDate) : null,
        content.Disclaimer,
        content.UncertaintyStatement);

    private static string FormatGestationalRange(int? startDay, int? endDay) =>
        startDay is { } start && endDay is { } end ? $"dias {start} a {end}" : "não informada";
}