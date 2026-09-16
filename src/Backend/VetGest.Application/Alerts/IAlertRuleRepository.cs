using VetGest.Domain.Entities;

namespace VetGest.Application.Alerts;

public interface IAlertRuleRepository
{
    Task<IReadOnlyList<AlertRule>> GetActiveForSpeciesAsync(string species, CancellationToken cancellationToken);
}
