using GAC.Domain.Entity.GAC;
using GAC.Share.DTO.Response.GAC;

namespace GAC.Infrastructure.Repositories;

internal static class MapeoGac
{
    public static ArticuloResponseDto Articulo(Articulo x) =>
        new(x.Id, x.Titulo, x.CorreoContacto, (int)x.TipoPublicacion, AutorIds(x), PalabraIds(x), UbicacionDe(x));

    public static InformeTecnicoResponseDto Informe(InformeTecnico x) =>
        new(x.Id, x.Titulo, x.CorreoContacto, AutorIds(x), PalabraIds(x), UbicacionDe(x),
            x.NumeroInforme, x.CentroPublicacion, x.MesPublicacion, x.AnioPublicacion, x.TipoInformeId);

    public static PublicacionCongresoResponseDto PublicacionCongreso(PublicacionCongreso x) =>
        new(x.Id, x.Titulo, x.CorreoContacto, AutorIds(x), PalabraIds(x), UbicacionDe(x), x.EdicionCongresoId);

    public static PublicacionRevistaResponseDto PublicacionRevista(PublicacionRevista x) =>
        new(x.Id, x.Titulo, x.CorreoContacto, AutorIds(x), PalabraIds(x), UbicacionDe(x),
            x.RevistaId, x.NumeroRevista, x.PaginaInicial, x.PaginaFinal, x.AnioPublicacion);

    public static UbicacionResponseDto Ubicacion(Ubicacion x) =>
        new(x.Id, x.ArticuloId, x.GrupoInvestigacionId, (int)x.Tipo, x.Laboratorio, x.Estanteria, x.InvestigadorId, x.Despacho);

    public static InvestigadorResponseDto Investigador(Investigador x) =>
        new(x.Id, x.Nombre, x.CentroTrabajo, x.CorreoElectronico,
            x.Temas.Select(t => t.TemaId).ToArray(), x.Grupos.Select(g => g.GrupoInvestigacionId).ToArray());

    public static GrupoInvestigacionResponseDto Grupo(GrupoInvestigacion x) =>
        new(x.Id, x.Nombre, x.Temas.Select(t => t.TemaId).ToArray());

    public static CongresoResponseDto Congreso(Congreso x) =>
        new(x.Id, x.Nombre, (int)x.Tipo, x.FrecuenciaCelebracion, x.AnioPrimeraCelebracion);

    public static EdicionCongresoResponseDto Edicion(EdicionCongreso x) =>
        new(x.Id, x.CongresoId, x.Edicion, x.Ciudad, x.Pais, x.FechaInicio, x.FechaFin);

    public static RevistaResponseDto Revista(Revista x) =>
        new(x.Id, x.Nombre, x.NombreEditor, x.AnioInicioPublicacion, x.FrecuenciaPublicacion,
            x.Temas.Select(t => t.TemaId).ToArray(), x.Colaboradores.Select(i => i.InvestigadorId).ToArray());

    public static ReferenciaDto[] Autores(Articulo a) =>
        a.Autorias.Select(x => new ReferenciaDto(x.InvestigadorId, x.Investigador.Nombre)).OrderBy(x => x.Nombre).ToArray();

    public static ReferenciaDto[] Palabras(Articulo a) =>
        a.PalabrasClave.Select(x => new ReferenciaDto(x.PalabraClaveId, x.PalabraClave.Texto)).OrderBy(x => x.Nombre).ToArray();

    private static Guid[] AutorIds(Articulo a) => a.Autorias.Select(x => x.InvestigadorId).ToArray();
    private static Guid[] PalabraIds(Articulo a) => a.PalabrasClave.Select(x => x.PalabraClaveId).ToArray();
    private static UbicacionResponseDto? UbicacionDe(Articulo a) => a.Ubicacion is { } u ? Ubicacion(u) : null;
}
