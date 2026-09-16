using Microsoft.EntityFrameworkCore;
using VetGest.Application.Alerts;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence;

public sealed class AlertRuleRepository : IAlertRuleRepository
{
    private readonly VetGestDbContext _dbContext;

    public AlertRuleRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<AlertRule>> GetActiveForSpeciesAsync(string species, CancellationToken cancellationToken) =>
        await _dbContext.AlertRules
            .Where(rule => rule.Species == species && rule.IsActive)
            .ToListAsync(cancellationToken);
}
