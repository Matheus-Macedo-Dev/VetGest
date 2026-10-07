namespace VetGest.Contracts.Pregnancies;

public static class VetConnectionHubGroups
{
    public static string Pregnancy(Guid pregnancyId) => $"pregnancy:{pregnancyId:D}";
}
