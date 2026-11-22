using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;

namespace GAC.Application.Contracts;

/// <summary>Recursos del negocio y tipos de DTO que acepta cada uno.</summary>
public interface IGacCatalogo
{
    ICatalogoLectura<ArticuloResponseDto, Guid> Articulos { get; }

    ICatalogoCrud<InformeTecnicoRequestDto, InformeTecnicoResponseDto, Guid> InformesTecnicos { get; }

    ICatalogoCrud<PublicacionCongresoRequestDto, PublicacionCongresoResponseDto, Guid> PublicacionesCongreso { get; }

    ICatalogoCrud<PublicacionRevistaRequestDto, PublicacionRevistaResponseDto, Guid> PublicacionesRevista { get; }

    ICatalogoCrud<TipoInformeRequestDto, TipoInformeResponseDto, Guid> TiposInforme { get; }

    ICatalogoCrud<CongresoRequestDto, CongresoResponseDto, Guid> Congresos { get; }

    ICatalogoCrud<EdicionCongresoRequestDto, EdicionCongresoResponseDto, Guid> EdicionesCongreso { get; }

    ICatalogoCrud<RevistaRequestDto, RevistaResponseDto, Guid> Revistas { get; }

    ICatalogoCrud<UbicacionRequestDto, UbicacionResponseDto, Guid> Ubicaciones { get; }

    ICatalogoCrud<InvestigadorRequestDto, InvestigadorResponseDto, Guid> Investigadores { get; }

    ICatalogoCrud<GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto, Guid> GruposInvestigacion { get; }

    ICatalogoCrud<TemaRequestDto, TemaResponseDto, Guid> Temas { get; }

    ICatalogoCrud<PalabraClaveRequestDto, PalabraClaveResponseDto, Guid> PalabrasClave { get; }

    ICatalogoCrud<AutoriaRequestDto, AutoriaResponseDto, (Guid ArticuloId, Guid InvestigadorId)> Autorias { get; }

    ICatalogoCrud<ArticuloPalabraClaveRequestDto, ArticuloPalabraClaveResponseDto, (Guid ArticuloId, Guid PalabraClaveId)> ArticulosPalabrasClave { get; }

    ICatalogoCrud<InvestigadorGrupoRequestDto, InvestigadorGrupoResponseDto, (Guid GrupoInvestigacionId, Guid InvestigadorId)> InvestigadoresGrupos { get; }

    ICatalogoCrud<InvestigadorTemaRequestDto, InvestigadorTemaResponseDto, (Guid InvestigadorId, Guid TemaId)> InvestigadoresTemas { get; }

    ICatalogoCrud<GrupoTemaRequestDto, GrupoTemaResponseDto, (Guid GrupoInvestigacionId, Guid TemaId)> GruposTemas { get; }

    ICatalogoCrud<RevistaTemaRequestDto, RevistaTemaResponseDto, (Guid RevistaId, Guid TemaId)> RevistasTemas { get; }

    ICatalogoCrud<ColaboracionRevistaRequestDto, ColaboracionRevistaResponseDto, (Guid RevistaId, Guid InvestigadorId)> ColaboracionesRevista { get; }
}
