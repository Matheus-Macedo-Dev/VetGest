namespace VetGest.Application.Pregnancies;

/// <summary>
/// Resolves display names for user IDs so connection views can show who the counterpart is.
/// </summary>
public interface IUserDirectory
{
    Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken);
}
