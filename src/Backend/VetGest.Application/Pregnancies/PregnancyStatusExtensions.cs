using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

internal static class PregnancyStatusExtensions
{
    public static bool IsActive(this Pregnancy pregnancy) =>
        !StringComparer.Ordinal.Equals(pregnancy.Status, "Completed")
        && !StringComparer.Ordinal.Equals(pregnancy.Status, "Cancelled");
}
