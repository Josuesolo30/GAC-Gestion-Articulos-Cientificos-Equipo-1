using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GAC.Application.Constants;
using GAC.Share.DTO.Response;

namespace GAC.Application.Extensions;

public sealed class HttpClientService(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal async Task<T?> LeerAsync<T>(string ruta, bool permitirNoEncontrado, CancellationToken cancellationToken)
        where T : class
    {
        using var client = httpClientFactory.CreateClient(GacApiRoutes.ClientName);
        using var response = await client.GetAsync(ruta, cancellationToken);
        if (permitirNoEncontrado && response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new JsonException("La API de GAC no devolvió los datos esperados.");
    }

    internal async Task<GeneralResponseDto> EscribirAsync(
        HttpMethod metodo, string ruta, object? solicitud, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient(GacApiRoutes.ClientName);
        using var request = new HttpRequestMessage(metodo, ruta);
        if (solicitud is not null)
            request.Content = JsonContent.Create(solicitud, solicitud.GetType(), options: JsonOptions);

        using var response = await client.SendAsync(request, cancellationToken);
        var contenido = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
            return string.IsNullOrWhiteSpace(contenido)
                ? new GeneralResponseDto(true)
                : JsonSerializer.Deserialize<GeneralResponseDto>(contenido, JsonOptions)
                    ?? throw new JsonException("La API de GAC no devolvió una respuesta de operación válida.");

        var estado = (int)response.StatusCode;
        var mensaje = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "La API rechazó los datos enviados.",
            HttpStatusCode.Unauthorized => "Tu sesión terminó. Inicia sesión para continuar.",
            HttpStatusCode.Forbidden => "No tienes permiso para realizar esta operación.",
            HttpStatusCode.NotFound => "La API no encontró el recurso solicitado.",
            HttpStatusCode.Conflict => "La operación entra en conflicto con los datos existentes.",
            _ => $"La API de GAC respondió con el estado {estado}."
        };

        // Solo usamos los mensajes de validación previstos de la API, nunca el cuerpo de un error 500.
        if (estado is 400 or 404 or 409 && !string.IsNullOrWhiteSpace(contenido))
        {
            try
            {
                var detalle = JsonSerializer.Deserialize<GeneralResponseDto>(contenido, JsonOptions);
                return new GeneralResponseDto(false, detalle?.Message ?? mensaje, Errors: detalle?.Errors, Estado: estado);
            }
            catch (JsonException) { }
        }
        return new GeneralResponseDto(false, mensaje, Estado: estado);
    }
}
