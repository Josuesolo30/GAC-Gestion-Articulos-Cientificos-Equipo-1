using GAC.Share.DTO.Response;

namespace GAC.Application.Contracts;

/// <summary>Operaciones de lectura sobre un recurso del catálogo.</summary>
public interface ICatalogoLectura<TRespuesta, TClave>
    where TRespuesta : class
{
    Task<IReadOnlyList<TRespuesta>> ListarAsync(CancellationToken cancellationToken = default);

    Task<TRespuesta?> ObtenerAsync(TClave clave, CancellationToken cancellationToken = default);
}

/// <summary>Operaciones compartidas por el cliente HTTP y el futuro repositorio.</summary>
public interface ICatalogoCrud<TSolicitud, TRespuesta, TClave> : ICatalogoLectura<TRespuesta, TClave>
    where TSolicitud : class
    where TRespuesta : class
{
    Task<GeneralResponseDto> CrearAsync(TSolicitud solicitud, CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> ActualizarAsync(TClave clave, TSolicitud solicitud, CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> EliminarAsync(TClave clave, CancellationToken cancellationToken = default);
}
