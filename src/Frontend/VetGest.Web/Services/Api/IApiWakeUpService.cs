namespace VetGest.Web.Services.Api;

public interface IApiWakeUpService
{
    Task WakeAsync(CancellationToken cancellationToken);
}
