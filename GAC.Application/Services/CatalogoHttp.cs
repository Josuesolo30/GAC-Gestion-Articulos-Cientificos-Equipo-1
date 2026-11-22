using GAC.Application.Contracts;
using GAC.Application.Extensions;
using GAC.Share.DTO.Response;

namespace GAC.Application.Services;

internal class CatalogoLecturaHttp<TRespuesta, TClave>(
    HttpClientService http, string ruta, Func<string, TClave, string> rutaDetalle)
    : ICatalogoLectura<TRespuesta, TClave>
    where TRespuesta : class
{
    protected HttpClientService Http { get; } = http;
    protected string Ruta { get; } = ruta;
    protected string RutaDetalle(TClave clave) => rutaDetalle(Ruta, clave);

    public async Task<IReadOnlyList<TRespuesta>> ListarAsync(CancellationToken cancellationToken = default)
        => (await Http.LeerAsync<TRespuesta[]>(Ruta, false, cancellationToken))!;

    public Task<TRespuesta?> ObtenerAsync(TClave clave, CancellationToken cancellationToken = default)
        => Http.LeerAsync<TRespuesta>(RutaDetalle(clave), true, cancellationToken);
}

internal sealed class CatalogoCrudHttp<TSolicitud, TRespuesta, TClave>(
    HttpClientService http, string ruta, Func<string, TClave, string> rutaDetalle)
    : CatalogoLecturaHttp<TRespuesta, TClave>(http, ruta, rutaDetalle),
      ICatalogoCrud<TSolicitud, TRespuesta, TClave>
    where TSolicitud : class
    where TRespuesta : class
{
    public Task<GeneralResponseDto> CrearAsync(TSolicitud solicitud, CancellationToken cancellationToken = default)
        => Http.EscribirAsync(HttpMethod.Post, Ruta, solicitud, cancellationToken);

    public Task<GeneralResponseDto> ActualizarAsync(TClave clave, TSolicitud solicitud, CancellationToken cancellationToken = default)
        => Http.EscribirAsync(HttpMethod.Put, RutaDetalle(clave), solicitud, cancellationToken);

    public Task<GeneralResponseDto> EliminarAsync(TClave clave, CancellationToken cancellationToken = default)
        => Http.EscribirAsync(HttpMethod.Delete, RutaDetalle(clave), null, cancellationToken);
}
