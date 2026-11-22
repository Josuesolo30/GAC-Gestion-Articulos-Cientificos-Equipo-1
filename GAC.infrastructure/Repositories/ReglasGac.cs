using GAC.Domain.Entity.GAC;
using GAC.Share.DTO.Request.GAC;
using Microsoft.EntityFrameworkCore;

namespace GAC.Infrastructure.Repositories;

public sealed partial class GacRepository
{
    private async Task GuardarArticulo(Articulo x, ArticuloBaseRequestDto dto, CancellationToken ct)
    {
        var autores = await ValidarIds(db.Investigadores, dto.InvestigadorIds, "InvestigadorIds", true, ct);
        var palabras = await ValidarIds(db.PalabrasClave, dto.PalabraClaveIds, "PalabraClaveIds", true, ct);
        x.Titulo = dto.Titulo.Trim();
        x.CorreoContacto = Limpiar(dto.CorreoContacto);
        Sincronizar(x.Autorias, autores, a => a.InvestigadorId, id => new Autoria { ArticuloId = x.Id, InvestigadorId = id });
        Sincronizar(x.PalabrasClave, palabras, p => p.PalabraClaveId, id => new ArticuloPalabraClave { ArticuloId = x.Id, PalabraClaveId = id });
    }

    private async Task GuardarInforme(InformeTecnico x, InformeTecnicoRequestDto dto, CancellationToken ct)
    {
        x.TipoInformeId = await ValidarId(db.TiposInforme, dto.TipoInformeId, "TipoInformeId", ct);
        await GuardarArticulo(x, dto, ct);
        x.NumeroInforme = dto.NumeroInforme.Trim();
        x.CentroPublicacion = dto.CentroPublicacion.Trim();
        x.MesPublicacion = dto.MesPublicacion;
        x.AnioPublicacion = dto.AnioPublicacion;
    }

    private async Task GuardarPublicacionCongreso(PublicacionCongreso x, PublicacionCongresoRequestDto dto, CancellationToken ct)
    {
        x.EdicionCongresoId = await ValidarId(db.EdicionesCongreso, dto.EdicionCongresoId, "EdicionCongresoId", ct);
        await GuardarArticulo(x, dto, ct);
    }

    private async Task GuardarPublicacionRevista(PublicacionRevista x, PublicacionRevistaRequestDto dto, CancellationToken ct)
    {
        x.RevistaId = await ValidarPublicacionRevista(dto.RevistaId, dto.PaginaInicial, dto.PaginaFinal, dto.AnioPublicacion, ct);
        await GuardarArticulo(x, dto, ct);
        x.NumeroRevista = dto.NumeroRevista.Trim();
        x.PaginaInicial = dto.PaginaInicial;
        x.PaginaFinal = dto.PaginaFinal;
        x.AnioPublicacion = dto.AnioPublicacion;
    }

    private async Task<Guid> ValidarPublicacionRevista(Guid? revistaId, int paginaInicio, int paginaFin, int anio, CancellationToken ct)
    {
        var revista = await db.Revistas.AsNoTracking().Include(r => r.Temas)
            .FirstOrDefaultAsync(r => r.Id == revistaId, ct)
            ?? throw new ErrorCatalogo(400, "Selecciona una revista existente.", "RevistaId");
        Exigir(revista.Temas.Count > 0, "RevistaId", "La revista debe tener al menos un tema.");
        Exigir(paginaInicio > 0 && paginaFin >= paginaInicio, "PaginaFinal", "La última página debe ser igual o mayor que la primera, y ambas deben ser positivas.");
        Exigir(anio >= revista.AnioInicioPublicacion, "AnioPublicacion", "El año del artículo no puede ser anterior al inicio de la revista.");
        return revista.Id;
    }

    private async Task GuardarCongreso(Congreso x, CongresoRequestDto dto, CancellationToken ct)
    {
        var ediciones = db.EdicionesCongreso.Where(e => e.CongresoId == x.Id);
        if (dto.Tipo == (int)TipoCongreso.Internacional)
            Exigir(!await ediciones.AnyAsync(e => e.Pais == null || e.Pais.Trim() == "", ct),
                "Tipo", "Completa el país de todas las ediciones antes de cambiar a internacional.");
        Exigir(!await ediciones.AnyAsync(e => e.FechaInicio.Year < dto.AnioPrimeraCelebracion, ct),
            "AnioPrimeraCelebracion", "Ya existe una edición anterior a ese año.");
        x.Nombre = dto.Nombre.Trim();
        x.Tipo = (TipoCongreso)dto.Tipo;
        x.FrecuenciaCelebracion = dto.FrecuenciaCelebracion.Trim();
        x.AnioPrimeraCelebracion = dto.AnioPrimeraCelebracion;
    }

    private async Task GuardarEdicion(EdicionCongreso x, EdicionCongresoRequestDto dto, CancellationToken ct)
    {
        var congreso = await db.Congresos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.CongresoId, ct)
            ?? throw new ErrorCatalogo(400, "Selecciona un congreso existente.", "CongresoId");
        Exigir(dto.FechaInicio != default, "FechaInicio", "Indica la fecha de inicio.");
        Exigir(dto.FechaFin != default && dto.FechaFin.Date >= dto.FechaInicio.Date,
            "FechaFin", "Indica una fecha final igual o posterior a la inicial.");
        Exigir(dto.FechaInicio.Year >= congreso.AnioPrimeraCelebracion, "FechaInicio",
            "La edición no puede ser anterior al primer año del congreso.");
        var pais = Limpiar(dto.Pais);
        Exigir(congreso.Tipo != TipoCongreso.Internacional || pais is not null, "Pais",
            "El país es obligatorio para una edición internacional.");
        x.CongresoId = congreso.Id;
        x.Edicion = dto.Edicion.Trim();
        x.Ciudad = dto.Ciudad.Trim();
        x.Pais = pais;
        x.FechaInicio = dto.FechaInicio.Date;
        x.FechaFin = dto.FechaFin.Date;
    }

    private async Task GuardarRevista(Revista x, RevistaRequestDto dto, CancellationToken ct)
    {
        var temas = await ValidarIds(db.Temas, dto.TemaIds ?? x.Temas.Select(t => t.TemaId).ToArray(), "TemaIds", true, ct);
        Exigir(!await db.PublicacionesRevista.AnyAsync(p => p.RevistaId == x.Id && p.AnioPublicacion < dto.AnioInicioPublicacion, ct),
            "AnioInicioPublicacion", "El nuevo año dejaría publicaciones anteriores al inicio de esta revista.");
        x.Nombre = dto.Nombre.Trim();
        x.NombreEditor = dto.NombreEditor.Trim();
        x.AnioInicioPublicacion = dto.AnioInicioPublicacion;
        x.FrecuenciaPublicacion = dto.FrecuenciaPublicacion.Trim();
        Sincronizar(x.Temas, temas, t => t.TemaId, id => new RevistaTema { RevistaId = x.Id, TemaId = id });
        if (dto.ColaboradorIds is not null)
        {
            var colaboradores = await ValidarIds(db.Investigadores, dto.ColaboradorIds, "ColaboradorIds", false, ct);
            Sincronizar(x.Colaboradores, colaboradores, t => t.InvestigadorId, id => new ColaboracionRevista { RevistaId = x.Id, InvestigadorId = id });
        }
    }

    private async Task GuardarInvestigador(Investigador x, InvestigadorRequestDto dto, CancellationToken ct)
    {
        if (dto.GrupoIds is not null)
        {
            var grupos = await ValidarIds(db.GruposInvestigacion, dto.GrupoIds, "GrupoIds", false, ct);
            var retirados = x.Grupos.Select(g => g.GrupoInvestigacionId).Except(grupos).ToArray();
            if (retirados.Length > 0)
                Exigir(!await db.Ubicaciones.AnyAsync(u => u.InvestigadorId == x.Id && retirados.Contains(u.GrupoInvestigacionId), ct),
                    "GrupoIds", "Primero retira o traslada las copias que conserva en su despacho.");
            Sincronizar(x.Grupos, grupos, g => g.GrupoInvestigacionId,
                id => new InvestigadorGrupo { InvestigadorId = x.Id, GrupoInvestigacionId = id });
        }
        if (dto.TemaIds is not null)
        {
            var temas = await ValidarIds(db.Temas, dto.TemaIds, "TemaIds", false, ct);
            Sincronizar(x.Temas, temas, t => t.TemaId, id => new InvestigadorTema { InvestigadorId = x.Id, TemaId = id });
        }
        x.Nombre = dto.Nombre.Trim();
        x.CentroTrabajo = dto.CentroTrabajo.Trim();
        x.CorreoElectronico = Limpiar(dto.CorreoElectronico);
    }

    private async Task AntesDeEliminarInvestigador(Investigador x, CancellationToken ct)
    {
        NoReferenciado(await db.Autorias.AnyAsync(a => a.InvestigadorId == x.Id, ct),
            "El investigador todavía es autor de uno o más artículos. Resuelve esas autorías primero.");
        NoReferenciado(await db.Ubicaciones.AnyAsync(u => u.InvestigadorId == x.Id, ct),
            "El investigador conserva una copia en su despacho. Retírala o trasládala primero.");
        db.InvestigadoresGrupos.RemoveRange(x.Grupos);
        db.ColaboracionesRevistas.RemoveRange(await db.ColaboracionesRevistas.Where(r => r.InvestigadorId == x.Id).ToListAsync(ct));
    }

    private async Task GuardarGrupo(GrupoInvestigacion x, GrupoInvestigacionRequestDto dto, CancellationToken ct)
    {
        x.Nombre = dto.Nombre.Trim();
        if (dto.TemaIds is not null)
        {
            var temas = await ValidarIds(db.Temas, dto.TemaIds, "TemaIds", false, ct);
            Sincronizar(x.Temas, temas, t => t.TemaId, id => new GrupoTema { GrupoInvestigacionId = x.Id, TemaId = id });
        }
    }

    private async Task GuardarUbicacion(Ubicacion x, UbicacionRequestDto dto, CancellationToken ct)
    {
        var articulo = await ValidarId(db.Articulos, dto.ArticuloId, "ArticuloId", ct);
        var grupo = await ValidarId(db.GruposInvestigacion, dto.GrupoInvestigacionId, "GrupoInvestigacionId", ct);
        Exigir(x.ArticuloId == Guid.Empty || x.ArticuloId == articulo, "ArticuloId", "La ubicación pertenece a otro artículo.");
        NoReferenciado(await db.Ubicaciones.AnyAsync(u => u.ArticuloId == articulo && u.Id != x.Id, ct),
            "Este artículo ya tiene una ubicación. Edita la ubicación existente.");
        x.ArticuloId = articulo;
        x.GrupoInvestigacionId = grupo;
        x.Tipo = (TipoUbicacion)dto.Tipo;
        if (x.Tipo == TipoUbicacion.Laboratorio)
        {
            var laboratorio = Limpiar(dto.Laboratorio);
            var estanteria = Limpiar(dto.Estanteria);
            Exigir(laboratorio is not null, "Laboratorio", "Indica el laboratorio.");
            Exigir(estanteria is not null, "Estanteria", "Indica la estantería.");
            x.Laboratorio = laboratorio;
            x.Estanteria = estanteria;
            x.InvestigadorId = null;
            x.Despacho = null;
        }
        else
        {
            var responsable = await ValidarId(db.Investigadores, dto.InvestigadorId, "InvestigadorId", ct);
            Exigir(await db.InvestigadoresGrupos.AnyAsync(g => g.GrupoInvestigacionId == grupo && g.InvestigadorId == responsable, ct),
                "InvestigadorId", "El investigador debe pertenecer al grupo que conserva la copia.");
            var despacho = Limpiar(dto.Despacho);
            Exigir(despacho is not null, "Despacho", "Indica la referencia del despacho.");
            x.InvestigadorId = responsable;
            x.Despacho = despacho;
            x.Laboratorio = null;
            x.Estanteria = null;
        }
    }
}
