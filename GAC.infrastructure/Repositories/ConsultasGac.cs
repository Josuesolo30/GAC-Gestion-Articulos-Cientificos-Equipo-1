using GAC.Domain.Entity.GAC;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.GAC;
using Microsoft.EntityFrameworkCore;

namespace GAC.Infrastructure.Repositories;

public sealed partial class GacRepository
{
    private IQueryable<Articulo> CargarFichaConRelaciones() => db.Articulos.AsNoTracking()
        .Include(a => a.Autorias).ThenInclude(x => x.Investigador)
        .Include(a => a.PalabrasClave).ThenInclude(x => x.PalabraClave)
        .Include(a => a.Ubicacion).ThenInclude(x => x!.GrupoInvestigacion)
        .Include(a => a.Ubicacion).ThenInclude(x => x!.Investigador)
        .AsSplitQuery();

    public async Task<IReadOnlyList<ResumenArticuloDto>> BuscarArticulosAsync(FiltroArticulosDto filtro, CancellationToken ct = default)
    {
        var query = CargarFichaConRelaciones();
        if (filtro.Tipo is int tipo) query = query.Where(a => (int)a.TipoPublicacion == tipo);
        if (filtro.TieneCopia is bool copia) query = query.Where(a => (a.Ubicacion != null) == copia);
        if (Limpiar(filtro.Texto)?.ToUpperInvariant() is { } texto)
        {
            query = filtro.Campo switch
            {
                "titulo" => query.Where(a => a.Titulo.ToUpper().Contains(texto)),
                "autor" => query.Where(a => a.Autorias.Any(i => i.Investigador.Nombre.ToUpper().Contains(texto))),
                "palabra" => query.Where(a => a.PalabrasClave.Any(p => p.PalabraClave.Texto.ToUpper().Contains(texto))),
                _ => query.Where(a => a.Titulo.ToUpper().Contains(texto)
                    || a.Autorias.Any(i => i.Investigador.Nombre.ToUpper().Contains(texto))
                    || a.PalabrasClave.Any(p => p.PalabraClave.Texto.ToUpper().Contains(texto)))
            };
        }
        return (await query.OrderBy(a => a.Titulo).ThenBy(a => a.Id).ToListAsync(ct)).Select(a =>
            new ResumenArticuloDto(a.Id, a.Titulo, (int)a.TipoPublicacion, MapeoGac.Autores(a),
                MapeoGac.Palabras(a), a.Ubicacion is not null)).ToArray();
    }

    public async Task<FichaArticuloDto?> ObtenerFichaArticuloAsync(Guid id, CancellationToken ct = default)
    {
        var a = await CargarFichaConRelaciones().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (a is null) return null;
        InformeTecnicoResponseDto? informe = null;
        TipoInformeResponseDto? tipoInforme = null;
        PublicacionCongresoResponseDto? publicacionCongreso = null;
        CongresoResponseDto? congreso = null;
        EdicionCongresoResponseDto? edicion = null;
        PublicacionRevistaResponseDto? publicacionRevista = null;
        RevistaResponseDto? revista = null;
        ReferenciaDto[] temas = [], colaboradores = [];
        if (a is InformeTecnico i)
        {
            informe = MapeoGac.Informe(i);
            tipoInforme = await TiposInforme.ObtenerAsync(i.TipoInformeId, ct);
        }
        if (a is PublicacionCongreso c)
        {
            publicacionCongreso = MapeoGac.PublicacionCongreso(c);
            var datos = await db.EdicionesCongreso.AsNoTracking().Include(e => e.Congreso)
                .FirstOrDefaultAsync(e => e.Id == c.EdicionCongresoId, ct);
            if (datos is not null)
            {
                edicion = MapeoGac.Edicion(datos);
                congreso = MapeoGac.Congreso(datos.Congreso);
            }
        }
        if (a is PublicacionRevista r)
        {
            publicacionRevista = MapeoGac.PublicacionRevista(r);
            var datos = await db.Revistas.AsNoTracking()
                .Include(t => t.Temas).ThenInclude(t => t.Tema)
                .Include(x => x.Colaboradores).ThenInclude(x => x.Investigador).AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Id == r.RevistaId, ct);
            if (datos is not null)
            {
                revista = MapeoGac.Revista(datos);
                temas = datos.Temas.Select(t => new ReferenciaDto(t.TemaId, t.Tema.Nombre)).OrderBy(t => t.Nombre).ToArray();
                colaboradores = datos.Colaboradores.Select(c => new ReferenciaDto(c.InvestigadorId, c.Investigador.Nombre)).OrderBy(t => t.Nombre).ToArray();
            }
        }
        var grupo = a.Ubicacion?.GrupoInvestigacion is { } g ? new ReferenciaDto(g.Id, g.Nombre) : null;
        var responsable = a.Ubicacion?.Investigador is { } inv ? new ReferenciaDto(inv.Id, inv.Nombre) : null;
        return new FichaArticuloDto(MapeoGac.Articulo(a), MapeoGac.Autores(a), MapeoGac.Palabras(a),
            informe, tipoInforme, publicacionCongreso, congreso, edicion, publicacionRevista, revista,
            temas, colaboradores, grupo, responsable);
    }

    public async Task<IReadOnlyList<InvestigadorResponseDto>> BuscarInvestigadoresAsync(FiltroInvestigadoresDto filtro, CancellationToken ct = default)
    {
        var query = CargarInvestigadores().AsNoTracking();
        if (Limpiar(filtro.Texto)?.ToUpperInvariant() is { } texto)
        {
            query = filtro.Campo switch
            {
                "nombre" => query.Where(i => i.Nombre.ToUpper().Contains(texto)),
                "centro" => query.Where(i => i.CentroTrabajo.ToUpper().Contains(texto)),
                "tema" => query.Where(i => i.Temas.Any(t => t.Tema.Nombre.ToUpper().Contains(texto))),
                _ => query.Where(i => i.Nombre.ToUpper().Contains(texto) || i.CentroTrabajo.ToUpper().Contains(texto)
                    || i.Temas.Any(t => t.Tema.Nombre.ToUpper().Contains(texto)))
            };
        }
        return (await query.OrderBy(i => i.Nombre).ThenBy(i => i.Id).ToListAsync(ct)).Select(MapeoGac.Investigador).ToArray();
    }

    public async Task<FichaInvestigadorDto?> ObtenerFichaInvestigadorAsync(Guid id, CancellationToken ct = default)
    {
        var i = await db.Investigadores.AsNoTracking()
            .Include(x => x.Grupos).ThenInclude(x => x.GrupoInvestigacion)
            .Include(x => x.Temas).ThenInclude(x => x.Tema)
            .Include(x => x.Autorias).ThenInclude(x => x.Articulo)
            .AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i is null) return null;
        return new FichaInvestigadorDto(MapeoGac.Investigador(i),
            i.Grupos.Select(g => new ReferenciaDto(g.GrupoInvestigacionId, g.GrupoInvestigacion.Nombre)).OrderBy(x => x.Nombre).ToArray(),
            i.Temas.Select(t => new ReferenciaDto(t.TemaId, t.Tema.Nombre)).OrderBy(x => x.Nombre).ToArray(),
            i.Autorias.Select(a => new ReferenciaDto(a.ArticuloId, a.Articulo.Titulo)).OrderBy(x => x.Nombre).ToArray());
    }

    public Task<GeneralResponseDto> CambiarModalidadAsync(Guid id, CambioModalidadRequestDto dto, CancellationToken ct = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            EscrituraEf.Validar(dto);
            var anterior = await db.Articulos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
                ?? throw new ErrorCatalogo(404, "El artículo que vas a cambiar no existe.");
            var origen = (int)anterior.TipoPublicacion;
            var destino = dto.TipoPublicacion;
            Exigir(origen != destino, "TipoPublicacion", "Elige una modalidad distinta a la actual.");

            // La jerarquía TPH vive en una tabla. Cambiamos la misma fila para conservar sus claves foráneas.
            FormattableString sql;
            switch ((TipoPublicacion)destino)
            {
                case TipoPublicacion.InformeTecnico:
                    var tipo = await ValidarId(db.TiposInforme, dto.TipoInformeId, "TipoInformeId", ct);
                    var numero = Limpiar(dto.NumeroInforme);
                    var centro = Limpiar(dto.CentroPublicacion);
                    Exigir(numero is not null, "NumeroInforme", "Indica el número del informe.");
                    Exigir(centro is not null, "CentroPublicacion", "Indica el centro de publicación.");
                    Exigir(dto.MesPublicacion is not null, "MesPublicacion", "Indica el mes de publicación.");
                    Exigir(dto.AnioPublicacion is not null, "AnioPublicacion", "Indica el año de publicación.");
                    sql = $@"UPDATE [Articulos] SET [TipoPublicacion]={destino}, [TipoInformeId]={tipo},
                        [NumeroInforme]={numero}, [CentroPublicacion]={centro},
                        [MesPublicacion]={dto.MesPublicacion.GetValueOrDefault()}, [AnioInforme]={dto.AnioPublicacion.GetValueOrDefault()},
                        [EdicionCongresoId]=NULL, [RevistaId]=NULL, [NumeroRevista]=NULL,
                        [PaginaInicial]=NULL, [PaginaFinal]=NULL, [AnioArticuloRevista]=NULL
                        WHERE [Id]={id} AND [TipoPublicacion]={origen}";
                    break;
                case TipoPublicacion.Congreso:
                    var edicion = await ValidarId(db.EdicionesCongreso, dto.EdicionCongresoId, "EdicionCongresoId", ct);
                    sql = $@"UPDATE [Articulos] SET [TipoPublicacion]={destino}, [EdicionCongresoId]={edicion},
                        [TipoInformeId]=NULL, [NumeroInforme]=NULL, [CentroPublicacion]=NULL, [MesPublicacion]=NULL, [AnioInforme]=NULL,
                        [RevistaId]=NULL, [NumeroRevista]=NULL, [PaginaInicial]=NULL, [PaginaFinal]=NULL, [AnioArticuloRevista]=NULL
                        WHERE [Id]={id} AND [TipoPublicacion]={origen}";
                    break;
                case TipoPublicacion.Revista:
                    var numeroRevista = Limpiar(dto.NumeroRevista);
                    Exigir(numeroRevista is not null, "NumeroRevista", "Indica el número de la revista.");
                    Exigir(dto.PaginaInicial is not null, "PaginaInicial", "Indica la primera página.");
                    Exigir(dto.PaginaFinal is not null, "PaginaFinal", "Indica la última página.");
                    Exigir(dto.AnioPublicacion is not null, "AnioPublicacion", "Indica el año de publicación.");
                    var inicio = dto.PaginaInicial.GetValueOrDefault();
                    var fin = dto.PaginaFinal.GetValueOrDefault();
                    var anio = dto.AnioPublicacion.GetValueOrDefault();
                    var revista = await ValidarPublicacionRevista(dto.RevistaId, inicio, fin, anio, ct);
                    sql = $@"UPDATE [Articulos] SET [TipoPublicacion]={destino}, [RevistaId]={revista}, [NumeroRevista]={numeroRevista},
                        [PaginaInicial]={inicio}, [PaginaFinal]={fin}, [AnioArticuloRevista]={anio},
                        [TipoInformeId]=NULL, [NumeroInforme]=NULL, [CentroPublicacion]=NULL,
                        [MesPublicacion]=NULL, [AnioInforme]=NULL, [EdicionCongresoId]=NULL
                        WHERE [Id]={id} AND [TipoPublicacion]={origen}";
                    break;
                default:
                    throw new ErrorCatalogo(400, "Elige una modalidad válida.", "TipoPublicacion");
            }
            NoReferenciado(await db.Database.ExecuteSqlInterpolatedAsync(sql, ct) != 1, "La modalidad cambió en otra operación. Recarga la ficha.");
            return id;
        }, ct);
}
