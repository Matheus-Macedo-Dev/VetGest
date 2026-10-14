using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VetGest.Contracts.Common;

namespace VetGest.Web.Services.Api;

public abstract class ApiClient(HttpClient httpClient, IApiWakeUpService wakeUpService)
{
    protected async Task<ApiCallResult<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        try
        {
            await wakeUpService.WakeAsync(cancellationToken);
            response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return ApiCallResult<T>.Failure("O servidor não retornou dados para esta solicitação. Tente novamente.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken);
            if (envelope?.Success == true && envelope.Data is not null)
            {
                return ApiCallResult<T>.Success(envelope.Data);
            }

            var isUnauthorized = response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
            var isNotFound = response.StatusCode == HttpStatusCode.NotFound;
            var isConflict = response.StatusCode == HttpStatusCode.Conflict;
            var error = response.StatusCode == HttpStatusCode.Forbidden
                ? "Você não tem permissão para acessar este conteúdo."
                : isUnauthorized
                    ? "Sua sessão expirou. Entre novamente para continuar."
                : isNotFound
                    ? "Não encontramos este registro ou você não tem acesso a ele."
                    : isConflict
                        ? "Já existe um registro para esta data. Escolha outro dia."
                        : envelope?.Error ?? "Não foi possível concluir a solicitação.";
            return ApiCallResult<T>.Failure(error, validationErrors: envelope?.ValidationErrors,
                isUnauthorized: isUnauthorized, isNotFound: isNotFound, isConflict: isConflict);
        }
        catch (JsonException)
        {
            var isUnauthorized = response?.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
            var error = response?.StatusCode == HttpStatusCode.Forbidden
                ? "Você não tem permissão para acessar este conteúdo."
                : isUnauthorized
                    ? "Sua sessão expirou. Entre novamente para continuar."
                    : "A resposta do servidor não está em um formato esperado. Tente novamente.";
            return ApiCallResult<T>.Failure(error, isUnauthorized: isUnauthorized);
        }
        catch (HttpRequestException)
        {
            return ApiCallResult<T>.Failure("Não foi possível conectar ao VetGest. Verifique sua conexão e tente novamente.", true);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiCallResult<T>.Failure("A solicitação demorou demais. Tente novamente.", true);
        }
        finally
        {
            response?.Dispose();
        }
    }
}