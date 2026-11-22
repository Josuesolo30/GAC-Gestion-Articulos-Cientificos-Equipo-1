using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.GAC;

namespace GAC.Application.Contracts;

public interface ICatalogoAvanzado
{
    Task<IReadOnlyList<ResumenArticuloDto>> BuscarArticulosAsync(FiltroArticulosDto filtro, CancellationToken ct = default);
    Task<FichaArticuloDto?> ObtenerFichaArticuloAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<InvestigadorResponseDto>> BuscarInvestigadoresAsync(FiltroInvestigadoresDto filtro, CancellationToken ct = default);
    Task<FichaInvestigadorDto?> ObtenerFichaInvestigadorAsync(Guid id, CancellationToken ct = default);
    Task<GeneralResponseDto> CambiarModalidadAsync(Guid id, CambioModalidadRequestDto solicitud, CancellationToken ct = default);
}
