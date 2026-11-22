namespace GAC.Share.DTO.Response.GAC;

public sealed record ReferenciaDto(Guid Id, string Nombre);
public sealed record ResumenArticuloDto(Guid Id, string Titulo, int TipoPublicacion,
    IReadOnlyList<ReferenciaDto> Autores, IReadOnlyList<ReferenciaDto> PalabrasClave, bool TieneCopia);

public sealed record FichaArticuloDto(
    ArticuloResponseDto Articulo, IReadOnlyList<ReferenciaDto> Autores, IReadOnlyList<ReferenciaDto> PalabrasClave,
    InformeTecnicoResponseDto? Informe, TipoInformeResponseDto? TipoInforme,
    PublicacionCongresoResponseDto? PublicacionCongreso, CongresoResponseDto? Congreso, EdicionCongresoResponseDto? Edicion,
    PublicacionRevistaResponseDto? PublicacionRevista, RevistaResponseDto? Revista,
    IReadOnlyList<ReferenciaDto> TemasRevista, IReadOnlyList<ReferenciaDto> ColaboradoresRevista,
    ReferenciaDto? GrupoCopia, ReferenciaDto? ResponsableDespacho);

public sealed record FichaInvestigadorDto(InvestigadorResponseDto Investigador,
    IReadOnlyList<ReferenciaDto> Grupos, IReadOnlyList<ReferenciaDto> Temas,
    IReadOnlyList<ReferenciaDto> Articulos);
