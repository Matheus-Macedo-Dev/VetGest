namespace VetGest.Web.Services.Today;

public interface ITodayService
{
    Task<TodayState> GetTodayAsync(CancellationToken cancellationToken = default);
}