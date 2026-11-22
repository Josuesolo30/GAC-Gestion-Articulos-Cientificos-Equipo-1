using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.WebUtilities;
using GAC.Share.DTO.Response.GAC;
using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Application.Extensions;
using GAC.Application.Interfaces;

namespace GAC.Application.Services;

/// <summary>Conecta las operaciones de GAC con sus rutas HTTP.</summary>
public sealed class GacService(HttpClientService http) : IGacService
{
    public ICatalogoLectura<ArticuloResponseDto, Guid> Articulos { get; } =
        new CatalogoLecturaHttp<ArticuloResponseDto, Guid>(http, GacApiRoutes.Articulos, GacApiRoutes.ConId);

    public ICatalogoCrud<InformeTecnicoRequestDto, InformeTecnicoResponseDto, Guid> InformesTecnicos { get; } =
        new CatalogoCrudHttp<InformeTecnicoRequestDto, InformeTecnicoResponseDto, Guid>(http, GacApiRoutes.InformesTecnicos, GacApiRoutes.ConId);

    public ICatalogoCrud<PublicacionCongresoRequestDto, PublicacionCongresoResponseDto, Guid> PublicacionesCongreso { get; } =
        new CatalogoCrudHttp<PublicacionCongresoRequestDto, PublicacionCongresoResponseDto, Guid>(http, GacApiRoutes.PublicacionesCongreso, GacApiRoutes.ConId);

    public ICatalogoCrud<PublicacionRevistaRequestDto, PublicacionRevistaResponseDto, Guid> PublicacionesRevista { get; } =
        new CatalogoCrudHttp<PublicacionRevistaRequestDto, PublicacionRevistaResponseDto, Guid>(http, GacApiRoutes.PublicacionesRevista, GacApiRoutes.ConId);

    public ICatalogoCrud<TipoInformeRequestDto, TipoInformeResponseDto, Guid> TiposInforme { get; } =
        new CatalogoCrudHttp<TipoInformeRequestDto, TipoInformeResponseDto, Guid>(http, GacApiRoutes.TiposInforme, GacApiRoutes.ConId);

    public ICatalogoCrud<CongresoRequestDto, CongresoResponseDto, Guid> Congresos { get; } =
        new CatalogoCrudHttp<CongresoRequestDto, CongresoResponseDto, Guid>(http, GacApiRoutes.Congresos, GacApiRoutes.ConId);

    public ICatalogoCrud<EdicionCongresoRequestDto, EdicionCongresoResponseDto, Guid> EdicionesCongreso { get; } =
        new CatalogoCrudHttp<EdicionCongresoRequestDto, EdicionCongresoResponseDto, Guid>(http, GacApiRoutes.EdicionesCongreso, GacApiRoutes.ConId);

    public ICatalogoCrud<RevistaRequestDto, RevistaResponseDto, Guid> Revistas { get; } =
        new CatalogoCrudHttp<RevistaRequestDto, RevistaResponseDto, Guid>(http, GacApiRoutes.Revistas, GacApiRoutes.ConId);

    public ICatalogoCrud<UbicacionRequestDto, UbicacionResponseDto, Guid> Ubicaciones { get; } =
        new CatalogoCrudHttp<UbicacionRequestDto, UbicacionResponseDto, Guid>(http, GacApiRoutes.Ubicaciones, GacApiRoutes.ConId);

    public ICatalogoCrud<InvestigadorRequestDto, InvestigadorResponseDto, Guid> Investigadores { get; } =
        new CatalogoCrudHttp<InvestigadorRequestDto, InvestigadorResponseDto, Guid>(http, GacApiRoutes.Investigadores, GacApiRoutes.ConId);

    public ICatalogoCrud<GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto, Guid> GruposInvestigacion { get; } =
        new CatalogoCrudHttp<GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto, Guid>(http, GacApiRoutes.GruposInvestigacion, GacApiRoutes.ConId);

    public ICatalogoCrud<TemaRequestDto, TemaResponseDto, Guid> Temas { get; } =
        new CatalogoCrudHttp<TemaRequestDto, TemaResponseDto, Guid>(http, GacApiRoutes.Temas, GacApiRoutes.ConId);

    public ICatalogoCrud<PalabraClaveRequestDto, PalabraClaveResponseDto, Guid> PalabrasClave { get; } =
        new CatalogoCrudHttp<PalabraClaveRequestDto, PalabraClaveResponseDto, Guid>(http, GacApiRoutes.PalabrasClave, GacApiRoutes.ConId);

    public ICatalogoCrud<AutoriaRequestDto, AutoriaResponseDto, (Guid ArticuloId, Guid InvestigadorId)> Autorias { get; } =
        new CatalogoCrudHttp<AutoriaRequestDto, AutoriaResponseDto, (Guid ArticuloId, Guid InvestigadorId)>(
            http, GacApiRoutes.Autorias, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.ArticuloId, clave.InvestigadorId));

    public ICatalogoCrud<ArticuloPalabraClaveRequestDto, ArticuloPalabraClaveResponseDto, (Guid ArticuloId, Guid PalabraClaveId)> ArticulosPalabrasClave { get; } =
        new CatalogoCrudHttp<ArticuloPalabraClaveRequestDto, ArticuloPalabraClaveResponseDto, (Guid ArticuloId, Guid PalabraClaveId)>(
            http, GacApiRoutes.ArticulosPalabrasClave, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.ArticuloId, clave.PalabraClaveId));

    public ICatalogoCrud<InvestigadorGrupoRequestDto, InvestigadorGrupoResponseDto, (Guid GrupoInvestigacionId, Guid InvestigadorId)> InvestigadoresGrupos { get; } =
        new CatalogoCrudHttp<InvestigadorGrupoRequestDto, InvestigadorGrupoResponseDto, (Guid GrupoInvestigacionId, Guid InvestigadorId)>(
            http, GacApiRoutes.InvestigadoresGrupos, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.GrupoInvestigacionId, clave.InvestigadorId));

    public ICatalogoCrud<InvestigadorTemaRequestDto, InvestigadorTemaResponseDto, (Guid InvestigadorId, Guid TemaId)> InvestigadoresTemas { get; } =
        new CatalogoCrudHttp<InvestigadorTemaRequestDto, InvestigadorTemaResponseDto, (Guid InvestigadorId, Guid TemaId)>(
            http, GacApiRoutes.InvestigadoresTemas, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.InvestigadorId, clave.TemaId));

    public ICatalogoCrud<GrupoTemaRequestDto, GrupoTemaResponseDto, (Guid GrupoInvestigacionId, Guid TemaId)> GruposTemas { get; } =
        new CatalogoCrudHttp<GrupoTemaRequestDto, GrupoTemaResponseDto, (Guid GrupoInvestigacionId, Guid TemaId)>(
            http, GacApiRoutes.GruposTemas, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.GrupoInvestigacionId, clave.TemaId));

    public ICatalogoCrud<RevistaTemaRequestDto, RevistaTemaResponseDto, (Guid RevistaId, Guid TemaId)> RevistasTemas { get; } =
        new CatalogoCrudHttp<RevistaTemaRequestDto, RevistaTemaResponseDto, (Guid RevistaId, Guid TemaId)>(
            http, GacApiRoutes.RevistasTemas, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.RevistaId, clave.TemaId));

    public ICatalogoCrud<ColaboracionRevistaRequestDto, ColaboracionRevistaResponseDto, (Guid RevistaId, Guid InvestigadorId)> ColaboracionesRevista { get; } =
        new CatalogoCrudHttp<ColaboracionRevistaRequestDto, ColaboracionRevistaResponseDto, (Guid RevistaId, Guid InvestigadorId)>(
            http, GacApiRoutes.ColaboracionesRevista, static (ruta, clave) => GacApiRoutes.ConPar(ruta, clave.RevistaId, clave.InvestigadorId));

    public async Task<IReadOnlyList<ResumenArticuloDto>> BuscarArticulosAsync(FiltroArticulosDto filtro, CancellationToken ct = default) =>
        (await http.LeerAsync<ResumenArticuloDto[]>(QueryHelpers.AddQueryString(GacApiRoutes.Articulos + "/buscar",
            new Dictionary<string, string?> { ["texto"] = filtro.Texto, ["campo"] = filtro.Campo, ["tipo"] = filtro.Tipo?.ToString(), ["tieneCopia"] = filtro.TieneCopia?.ToString() }), false, ct))!;

    public Task<FichaArticuloDto?> ObtenerFichaArticuloAsync(Guid id, CancellationToken ct = default) =>
        http.LeerAsync<FichaArticuloDto>(GacApiRoutes.ConId(GacApiRoutes.Articulos, id) + "/ficha", true, ct);

    public async Task<IReadOnlyList<InvestigadorResponseDto>> BuscarInvestigadoresAsync(FiltroInvestigadoresDto filtro, CancellationToken ct = default) =>
        (await http.LeerAsync<InvestigadorResponseDto[]>(QueryHelpers.AddQueryString(GacApiRoutes.Investigadores + "/buscar",
            new Dictionary<string, string?> { ["texto"] = filtro.Texto, ["campo"] = filtro.Campo }), false, ct))!;

    public Task<FichaInvestigadorDto?> ObtenerFichaInvestigadorAsync(Guid id, CancellationToken ct = default) =>
        http.LeerAsync<FichaInvestigadorDto>(GacApiRoutes.ConId(GacApiRoutes.Investigadores, id) + "/ficha", true, ct);

    public Task<GeneralResponseDto> CambiarModalidadAsync(Guid id, CambioModalidadRequestDto solicitud, CancellationToken ct = default) =>
        http.EscribirAsync(HttpMethod.Put, GacApiRoutes.ConId(GacApiRoutes.Articulos, id) + "/modalidad", solicitud, ct);
}
