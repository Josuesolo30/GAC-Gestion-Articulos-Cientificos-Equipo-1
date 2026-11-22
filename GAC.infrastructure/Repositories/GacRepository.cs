using System.Linq.Expressions;
using GAC.Application.Contracts;
using GAC.Domain.Entity.GAC;
using GAC.Infrastructure.Data;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.EntityFrameworkCore;

namespace GAC.Infrastructure.Repositories;

public sealed partial class GacRepository : IGacRepository
{
    private readonly AppDbContext db;

    public ICatalogoLectura<ArticuloResponseDto, Guid> Articulos { get; }
    public ICatalogoCrud<InformeTecnicoRequestDto, InformeTecnicoResponseDto, Guid> InformesTecnicos { get; }
    public ICatalogoCrud<PublicacionCongresoRequestDto, PublicacionCongresoResponseDto, Guid> PublicacionesCongreso { get; }
    public ICatalogoCrud<PublicacionRevistaRequestDto, PublicacionRevistaResponseDto, Guid> PublicacionesRevista { get; }
    public ICatalogoCrud<TipoInformeRequestDto, TipoInformeResponseDto, Guid> TiposInforme { get; }
    public ICatalogoCrud<CongresoRequestDto, CongresoResponseDto, Guid> Congresos { get; }
    public ICatalogoCrud<EdicionCongresoRequestDto, EdicionCongresoResponseDto, Guid> EdicionesCongreso { get; }
    public ICatalogoCrud<RevistaRequestDto, RevistaResponseDto, Guid> Revistas { get; }
    public ICatalogoCrud<UbicacionRequestDto, UbicacionResponseDto, Guid> Ubicaciones { get; }
    public ICatalogoCrud<InvestigadorRequestDto, InvestigadorResponseDto, Guid> Investigadores { get; }
    public ICatalogoCrud<GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto, Guid> GruposInvestigacion { get; }
    public ICatalogoCrud<TemaRequestDto, TemaResponseDto, Guid> Temas { get; }
    public ICatalogoCrud<PalabraClaveRequestDto, PalabraClaveResponseDto, Guid> PalabrasClave { get; }
    public ICatalogoCrud<AutoriaRequestDto, AutoriaResponseDto, (Guid, Guid)> Autorias { get; }
    public ICatalogoCrud<ArticuloPalabraClaveRequestDto, ArticuloPalabraClaveResponseDto, (Guid, Guid)> ArticulosPalabrasClave { get; }
    public ICatalogoCrud<InvestigadorGrupoRequestDto, InvestigadorGrupoResponseDto, (Guid, Guid)> InvestigadoresGrupos { get; }
    public ICatalogoCrud<InvestigadorTemaRequestDto, InvestigadorTemaResponseDto, (Guid, Guid)> InvestigadoresTemas { get; }
    public ICatalogoCrud<GrupoTemaRequestDto, GrupoTemaResponseDto, (Guid, Guid)> GruposTemas { get; }
    public ICatalogoCrud<RevistaTemaRequestDto, RevistaTemaResponseDto, (Guid, Guid)> RevistasTemas { get; }
    public ICatalogoCrud<ColaboracionRevistaRequestDto, ColaboracionRevistaResponseDto, (Guid, Guid)> ColaboracionesRevista { get; }

    public GacRepository(AppDbContext context)
    {
        db = context;
        Articulos = new LecturaEf<Articulo, ArticuloResponseDto, Guid>(
            () => CargarArticulo(db.Articulos).OrderBy(x => x.Titulo), id => x => x.Id == id, MapeoGac.Articulo);
        InformesTecnicos = Crud<InformeTecnico, InformeTecnicoRequestDto, InformeTecnicoResponseDto>(
            x => x.Id, MapeoGac.Informe, GuardarInforme,
            () => CargarArticulo(db.InformesTecnicos).OrderByDescending(x => x.AnioPublicacion)
                .ThenByDescending(x => x.MesPublicacion).ThenBy(x => x.Titulo));
        PublicacionesCongreso = Crud<PublicacionCongreso, PublicacionCongresoRequestDto, PublicacionCongresoResponseDto>(
            x => x.Id, MapeoGac.PublicacionCongreso, GuardarPublicacionCongreso, () => CargarArticulo(db.PublicacionesCongreso).OrderBy(x => x.Titulo));
        PublicacionesRevista = Crud<PublicacionRevista, PublicacionRevistaRequestDto, PublicacionRevistaResponseDto>(
            x => x.Id, MapeoGac.PublicacionRevista, GuardarPublicacionRevista, () => CargarArticulo(db.PublicacionesRevista).OrderBy(x => x.Titulo));
        TiposInforme = Crud<TipoInforme, TipoInformeRequestDto, TipoInformeResponseDto>(x => x.Id,
            x => new(x.Id, x.Nombre), (x, dto, _) => Asignar(() => x.Nombre = dto.Nombre.Trim()),
            () => db.TiposInforme.OrderBy(x => x.Nombre),
            async (x, ct) => NoReferenciado(await db.InformesTecnicos.AnyAsync(i => i.TipoInformeId == x.Id, ct), "Hay informes con este tipo."));
        Congresos = Crud<Congreso, CongresoRequestDto, CongresoResponseDto>(x => x.Id, MapeoGac.Congreso,
            GuardarCongreso, () => db.Congresos.OrderBy(x => x.Nombre),
            async (x, ct) => NoReferenciado(await db.EdicionesCongreso.AnyAsync(e => e.CongresoId == x.Id, ct), "Este congreso tiene ediciones."));
        EdicionesCongreso = Crud<EdicionCongreso, EdicionCongresoRequestDto, EdicionCongresoResponseDto>(x => x.Id,
            MapeoGac.Edicion, GuardarEdicion, () => db.EdicionesCongreso.OrderByDescending(x => x.FechaInicio),
            async (x, ct) => NoReferenciado(await db.PublicacionesCongreso.AnyAsync(i => i.EdicionCongresoId == x.Id, ct), "Hay publicaciones en esta edición."));
        Revistas = Crud<Revista, RevistaRequestDto, RevistaResponseDto>(x => x.Id, MapeoGac.Revista,
            GuardarRevista, () => CargarRevistas().OrderBy(x => x.Nombre),
            async (x, ct) => NoReferenciado(await db.PublicacionesRevista.AnyAsync(i => i.RevistaId == x.Id, ct), "Hay publicaciones en esta revista."));
        Ubicaciones = Crud<Ubicacion, UbicacionRequestDto, UbicacionResponseDto>(x => x.Id, MapeoGac.Ubicacion, GuardarUbicacion);
        Investigadores = Crud<Investigador, InvestigadorRequestDto, InvestigadorResponseDto>(x => x.Id,
            MapeoGac.Investigador, GuardarInvestigador, () => CargarInvestigadores().OrderBy(x => x.Nombre), AntesDeEliminarInvestigador);
        GruposInvestigacion = Crud<GrupoInvestigacion, GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto>(x => x.Id,
            MapeoGac.Grupo, GuardarGrupo, () => db.GruposInvestigacion.Include(x => x.Temas).OrderBy(x => x.Nombre),
            async (x, ct) => NoReferenciado(await db.Ubicaciones.AnyAsync(u => u.GrupoInvestigacionId == x.Id, ct), "El grupo todavía conserva copias."));
        Temas = Crud<Tema, TemaRequestDto, TemaResponseDto>(x => x.Id, x => new(x.Id, x.Nombre),
            (x, dto, _) => Asignar(() => x.Nombre = dto.Nombre.Trim()), () => db.Temas.OrderBy(x => x.Nombre),
            async (x, ct) => NoReferenciado(
                await db.InvestigadoresTemas.AnyAsync(i => i.TemaId == x.Id, ct)
                || await db.RevistasTemas.AnyAsync(i => i.TemaId == x.Id, ct)
                || await db.GruposTemas.AnyAsync(i => i.TemaId == x.Id, ct), "El tema está asignado a un investigador, grupo o revista."));
        PalabrasClave = Crud<PalabraClave, PalabraClaveRequestDto, PalabraClaveResponseDto>(x => x.Id,
            x => new(x.Id, x.Texto), (x, dto, _) => Asignar(() => x.Texto = dto.Texto.Trim()), () => db.PalabrasClave.OrderBy(x => x.Texto),
            async (x, ct) => NoReferenciado(await db.ArticulosPalabrasClave.AnyAsync(a => a.PalabraClaveId == x.Id, ct), "Esta palabra clave está en uso."));

        Autorias = Vinculo<Autoria, AutoriaRequestDto, AutoriaResponseDto>(
            k => x => x.ArticuloId == k.Item1 && x.InvestigadorId == k.Item2,
            d => (d.ArticuloId.GetValueOrDefault(), d.InvestigadorId.GetValueOrDefault()),
            k => new() { ArticuloId = k.Item1, InvestigadorId = k.Item2 }, x => new(x.ArticuloId, x.InvestigadorId),
            async (k, ct) => { await ValidarId(db.Articulos, k.Item1, "ArticuloId", ct); await ValidarId(db.Investigadores, k.Item2, "InvestigadorId", ct); },
            async (x, ct) => NoReferenciado(await db.Autorias.CountAsync(a => a.ArticuloId == x.ArticuloId, ct) <= 1, "El artículo debe conservar al menos un autor."));
        ArticulosPalabrasClave = Vinculo<ArticuloPalabraClave, ArticuloPalabraClaveRequestDto, ArticuloPalabraClaveResponseDto>(
            k => x => x.ArticuloId == k.Item1 && x.PalabraClaveId == k.Item2,
            d => (d.ArticuloId.GetValueOrDefault(), d.PalabraClaveId.GetValueOrDefault()),
            k => new() { ArticuloId = k.Item1, PalabraClaveId = k.Item2 }, x => new(x.ArticuloId, x.PalabraClaveId),
            async (k, ct) => { await ValidarId(db.Articulos, k.Item1, "ArticuloId", ct); await ValidarId(db.PalabrasClave, k.Item2, "PalabraClaveId", ct); },
            async (x, ct) => NoReferenciado(await db.ArticulosPalabrasClave.CountAsync(a => a.ArticuloId == x.ArticuloId, ct) <= 1, "El artículo debe conservar al menos una palabra clave."));
        InvestigadoresGrupos = Vinculo<InvestigadorGrupo, InvestigadorGrupoRequestDto, InvestigadorGrupoResponseDto>(
            k => x => x.GrupoInvestigacionId == k.Item1 && x.InvestigadorId == k.Item2,
            d => (d.GrupoInvestigacionId.GetValueOrDefault(), d.InvestigadorId.GetValueOrDefault()),
            k => new() { GrupoInvestigacionId = k.Item1, InvestigadorId = k.Item2 }, x => new(x.GrupoInvestigacionId, x.InvestigadorId),
            async (k, ct) => { await ValidarId(db.GruposInvestigacion, k.Item1, "GrupoInvestigacionId", ct); await ValidarId(db.Investigadores, k.Item2, "InvestigadorId", ct); },
            async (x, ct) => NoReferenciado(await db.Ubicaciones.AnyAsync(u => u.GrupoInvestigacionId == x.GrupoInvestigacionId && u.InvestigadorId == x.InvestigadorId, ct), "Primero cambia la ubicación de la copia que conserva este investigador."));
        InvestigadoresTemas = Vinculo<InvestigadorTema, InvestigadorTemaRequestDto, InvestigadorTemaResponseDto>(
            k => x => x.InvestigadorId == k.Item1 && x.TemaId == k.Item2,
            d => (d.InvestigadorId.GetValueOrDefault(), d.TemaId.GetValueOrDefault()),
            k => new() { InvestigadorId = k.Item1, TemaId = k.Item2 }, x => new(x.InvestigadorId, x.TemaId),
            async (k, ct) => { await ValidarId(db.Investigadores, k.Item1, "InvestigadorId", ct); await ValidarId(db.Temas, k.Item2, "TemaId", ct); });
        GruposTemas = Vinculo<GrupoTema, GrupoTemaRequestDto, GrupoTemaResponseDto>(
            k => x => x.GrupoInvestigacionId == k.Item1 && x.TemaId == k.Item2,
            d => (d.GrupoInvestigacionId.GetValueOrDefault(), d.TemaId.GetValueOrDefault()),
            k => new() { GrupoInvestigacionId = k.Item1, TemaId = k.Item2 }, x => new(x.GrupoInvestigacionId, x.TemaId),
            async (k, ct) => { await ValidarId(db.GruposInvestigacion, k.Item1, "GrupoInvestigacionId", ct); await ValidarId(db.Temas, k.Item2, "TemaId", ct); });
        RevistasTemas = Vinculo<RevistaTema, RevistaTemaRequestDto, RevistaTemaResponseDto>(
            k => x => x.RevistaId == k.Item1 && x.TemaId == k.Item2,
            d => (d.RevistaId.GetValueOrDefault(), d.TemaId.GetValueOrDefault()),
            k => new() { RevistaId = k.Item1, TemaId = k.Item2 }, x => new(x.RevistaId, x.TemaId),
            async (k, ct) => { await ValidarId(db.Revistas, k.Item1, "RevistaId", ct); await ValidarId(db.Temas, k.Item2, "TemaId", ct); },
            async (x, ct) => NoReferenciado(await db.RevistasTemas.CountAsync(a => a.RevistaId == x.RevistaId, ct) <= 1, "Una revista debe conservar al menos un tema."));
        ColaboracionesRevista = Vinculo<ColaboracionRevista, ColaboracionRevistaRequestDto, ColaboracionRevistaResponseDto>(
            k => x => x.RevistaId == k.Item1 && x.InvestigadorId == k.Item2,
            d => (d.RevistaId.GetValueOrDefault(), d.InvestigadorId.GetValueOrDefault()),
            k => new() { RevistaId = k.Item1, InvestigadorId = k.Item2 }, x => new(x.RevistaId, x.InvestigadorId),
            async (k, ct) => { await ValidarId(db.Revistas, k.Item1, "RevistaId", ct); await ValidarId(db.Investigadores, k.Item2, "InvestigadorId", ct); });
    }

    private CrudEf<E, S, R> Crud<E, S, R>(Expression<Func<E, Guid>> id, Func<E, R> map,
        Func<E, S, CancellationToken, Task> guardar, Func<IQueryable<E>>? consulta = null,
        Func<E, CancellationToken, Task>? borrar = null)
        where E : class, new() where S : class where R : class =>
        new(db, consulta ?? (() => db.Set<E>()), id, map, guardar, borrar);

    private VinculoEf<E, S, R> Vinculo<E, S, R>(Func<(Guid, Guid), Expression<Func<E, bool>>> porClave,
        Func<S, (Guid, Guid)> clave, Func<(Guid, Guid), E> crear, Func<E, R> map,
        Func<(Guid, Guid), CancellationToken, Task> validar, Func<E, CancellationToken, Task>? borrar = null)
        where E : class where S : class where R : class => new(db, porClave, clave, crear, map, validar, borrar);

    private static IQueryable<T> CargarArticulo<T>(IQueryable<T> consulta) where T : Articulo =>
        consulta.Include(x => x.Autorias).Include(x => x.PalabrasClave).Include(x => x.Ubicacion).AsSplitQuery();

    private IQueryable<Investigador> CargarInvestigadores() =>
        db.Investigadores.Include(x => x.Grupos).Include(x => x.Temas).AsSplitQuery();

    private IQueryable<Revista> CargarRevistas() =>
        db.Revistas.Include(x => x.Temas).Include(x => x.Colaboradores).AsSplitQuery();

    private static Task Asignar(Action accion) { accion(); return Task.CompletedTask; }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static void Exigir(bool valido, string campo, string mensaje)
    {
        if (!valido) throw new ErrorCatalogo(400, mensaje, campo);
    }

    private static void NoReferenciado(bool enUso, string mensaje)
    {
        if (enUso) throw new ErrorCatalogo(409, mensaje);
    }

    private static async Task<Guid> ValidarId<T>(IQueryable<T> tabla, Guid? id, string campo, CancellationToken ct) where T : class
    {
        var clave = id.GetValueOrDefault();
        Exigir(clave != Guid.Empty && await tabla.AnyAsync(x => EF.Property<Guid>(x, "Id") == clave, ct),
            campo, "Selecciona un registro existente para este campo.");
        return clave;
    }

    private static async Task<Guid[]> ValidarIds<T>(IQueryable<T> tabla, IReadOnlyList<Guid>? ids,
        string campo, bool requerido, CancellationToken ct) where T : class
    {
        var claves = ids?.Distinct().ToArray() ?? [];
        Exigir(!requerido || claves.Length > 0, campo, "Selecciona al menos una opción.");
        Exigir(!claves.Contains(Guid.Empty), campo, "Hay una selección inválida.");
        if (claves.Length > 0)
        {
            var existentes = await tabla.CountAsync(x => claves.Contains(EF.Property<Guid>(x, "Id")), ct);
            Exigir(existentes == claves.Length, campo, "Una o más opciones seleccionadas ya no existen.");
        }
        return claves;
    }

    private void Sincronizar<T>(ICollection<T> relaciones, Guid[] nuevos, Func<T, Guid> id, Func<Guid, T> crear) where T : class
    {
        var elegidos = nuevos.ToHashSet();
        var anteriores = relaciones.Select(id).ToHashSet();
        foreach (var fila in relaciones.Where(x => !elegidos.Contains(id(x))).ToArray())
        {
            db.Remove(fila);
            relaciones.Remove(fila);
        }
        foreach (var clave in nuevos.Where(x => !anteriores.Contains(x))) relaciones.Add(crear(clave));
    }
}
