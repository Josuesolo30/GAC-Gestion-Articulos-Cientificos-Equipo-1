using System.ComponentModel.DataAnnotations;
using GAC.Application.Contracts;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;

namespace GAC.WebUIPages.Pages.Catalogos;

public sealed record ItemCatalogo(Guid Id, string Nombre, string Detalle = "");

public sealed class DatosCatalogo
{
    [MaxLength(250)] public string? Nombre { get; set; }
    [MaxLength(200)] public string? Editor { get; set; }
    [MaxLength(80)] public string? Frecuencia { get; set; }
    [Range(1, 9999)] public int? Anio { get; set; }
    [Range(1, 2)] public int Tipo { get; set; } = 1;
    public Guid? CongresoId { get; set; }
    [MaxLength(80)] public string? Edicion { get; set; }
    [MaxLength(150)] public string? Ciudad { get; set; }
    [MaxLength(150)] public string? Pais { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public List<Guid> TemaIds { get; set; } = [];
    public List<Guid> ColaboradorIds { get; set; } = [];
}

public sealed class GestorCatalogos(IGacService gac)
{
    public static readonly IReadOnlyDictionary<string, string> Nombres = new Dictionary<string, string>
    {
        ["tipos-informe"] = "Tipos de informe",
        ["palabras-clave"] = "Palabras clave",
        ["temas"] = "Temas",
        ["grupos-investigacion"] = "Grupos de investigación",
        ["congresos"] = "Congresos",
        ["ediciones-congreso"] = "Ediciones de congreso",
        ["revistas"] = "Revistas"
    };

    public async Task<IReadOnlyList<ItemCatalogo>> ListarAsync(string catalogo, CancellationToken ct)
    {
        var filas = catalogo switch
        {
            "tipos-informe" => (await gac.TiposInforme.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Nombre)),
            "palabras-clave" => (await gac.PalabrasClave.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Texto)),
            "temas" => (await gac.Temas.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Nombre)),
            "grupos-investigacion" => (await gac.GruposInvestigacion.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Nombre, $"{x.TemaIds?.Count ?? 0} tema(s)")),
            "congresos" => (await gac.Congresos.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Nombre, $"{(x.Tipo == 2 ? "Internacional" : "Nacional")} · {x.FrecuenciaCelebracion}")),
            "ediciones-congreso" => await ListarEdiciones(ct),
            "revistas" => (await gac.Revistas.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id, x.Nombre, $"{x.NombreEditor} · {x.FrecuenciaPublicacion}")),
            _ => Enumerable.Empty<ItemCatalogo>()
        };
        return filas.ToArray();
    }

    private async Task<IEnumerable<ItemCatalogo>> ListarEdiciones(CancellationToken ct)
    {
        var congresos = (await gac.Congresos.ListarAsync(ct)).ToDictionary(x => x.Id, x => x.Nombre);
        return (await gac.EdicionesCongreso.ListarAsync(ct)).Select(x => new ItemCatalogo(x.Id,
            $"{congresos.GetValueOrDefault(x.CongresoId)} · {x.Edicion}", $"{x.Ciudad} · {x.FechaInicio:dd/MM/yyyy}"));
    }

    public Task<DatosCatalogo?> ObtenerAsync(string catalogo, Guid id, CancellationToken ct) => catalogo switch
    {
        "tipos-informe" => Proyectar(gac.TiposInforme, id, x => new() { Nombre = x.Nombre }, ct),
        "palabras-clave" => Proyectar(gac.PalabrasClave, id, x => new() { Nombre = x.Texto }, ct),
        "temas" => Proyectar(gac.Temas, id, x => new() { Nombre = x.Nombre }, ct),
        "grupos-investigacion" => Proyectar(gac.GruposInvestigacion, id, x => new() { Nombre = x.Nombre, TemaIds = x.TemaIds?.ToList() ?? [] }, ct),
        "congresos" => Proyectar(gac.Congresos, id, x => new() { Nombre = x.Nombre, Tipo = x.Tipo, Frecuencia = x.FrecuenciaCelebracion, Anio = x.AnioPrimeraCelebracion }, ct),
        "ediciones-congreso" => Proyectar(gac.EdicionesCongreso, id, x => new() { CongresoId = x.CongresoId, Edicion = x.Edicion, Ciudad = x.Ciudad, Pais = x.Pais, Inicio = x.FechaInicio, Fin = x.FechaFin }, ct),
        "revistas" => Proyectar(gac.Revistas, id, x => new() { Nombre = x.Nombre, Editor = x.NombreEditor, Anio = x.AnioInicioPublicacion, Frecuencia = x.FrecuenciaPublicacion, TemaIds = x.TemaIds?.ToList() ?? [], ColaboradorIds = x.ColaboradorIds?.ToList() ?? [] }, ct),
        _ => Task.FromResult<DatosCatalogo?>(null)
    };

    private static async Task<DatosCatalogo?> Proyectar<R>(ICatalogoLectura<R, Guid> repo, Guid id, Func<R, DatosCatalogo> map, CancellationToken ct) where R : class =>
        await repo.ObtenerAsync(id, ct) is { } x ? map(x) : null;

    public Task<GeneralResponseDto> GuardarAsync(string catalogo, Guid? id, DatosCatalogo f, CancellationToken ct) => catalogo switch
    {
        "tipos-informe" => Guardar(gac.TiposInforme, new TipoInformeRequestDto { Nombre = f.Nombre ?? "" }, id, ct),
        "palabras-clave" => Guardar(gac.PalabrasClave, new PalabraClaveRequestDto { Texto = f.Nombre ?? "" }, id, ct),
        "temas" => Guardar(gac.Temas, new TemaRequestDto { Nombre = f.Nombre ?? "" }, id, ct),
        "grupos-investigacion" => Guardar(gac.GruposInvestigacion, new GrupoInvestigacionRequestDto { Nombre = f.Nombre ?? "", TemaIds = f.TemaIds }, id, ct),
        "congresos" => Guardar(gac.Congresos, new CongresoRequestDto { Nombre = f.Nombre ?? "", Tipo = f.Tipo, FrecuenciaCelebracion = f.Frecuencia ?? "", AnioPrimeraCelebracion = f.Anio.GetValueOrDefault() }, id, ct),
        "ediciones-congreso" => Guardar(gac.EdicionesCongreso, new EdicionCongresoRequestDto { CongresoId = f.CongresoId, Edicion = f.Edicion ?? "", Ciudad = f.Ciudad ?? "", Pais = f.Pais, FechaInicio = f.Inicio.GetValueOrDefault(), FechaFin = f.Fin.GetValueOrDefault() }, id, ct),
        "revistas" => Guardar(gac.Revistas, new RevistaRequestDto { Nombre = f.Nombre ?? "", NombreEditor = f.Editor ?? "", AnioInicioPublicacion = f.Anio.GetValueOrDefault(), FrecuenciaPublicacion = f.Frecuencia ?? "", TemaIds = f.TemaIds, ColaboradorIds = f.ColaboradorIds }, id, ct),
        _ => Task.FromResult(new GeneralResponseDto(false, "Catálogo desconocido.", Estado: 404))
    };

    private static Task<GeneralResponseDto> Guardar<S,R>(ICatalogoCrud<S,R,Guid> repo, S solicitud, Guid? id, CancellationToken ct)
        where S : class where R : class => id is Guid k ? repo.ActualizarAsync(k, solicitud, ct) : repo.CrearAsync(solicitud, ct);

    public Task<GeneralResponseDto> EliminarAsync(string catalogo, Guid id, CancellationToken ct) => catalogo switch
    {
        "tipos-informe" => gac.TiposInforme.EliminarAsync(id, ct),
        "palabras-clave" => gac.PalabrasClave.EliminarAsync(id, ct),
        "temas" => gac.Temas.EliminarAsync(id, ct),
        "grupos-investigacion" => gac.GruposInvestigacion.EliminarAsync(id, ct),
        "congresos" => gac.Congresos.EliminarAsync(id, ct),
        "ediciones-congreso" => gac.EdicionesCongreso.EliminarAsync(id, ct),
        "revistas" => gac.Revistas.EliminarAsync(id, ct),
        _ => Task.FromResult(new GeneralResponseDto(false, "Catálogo desconocido.", Estado: 404))
    };
}
