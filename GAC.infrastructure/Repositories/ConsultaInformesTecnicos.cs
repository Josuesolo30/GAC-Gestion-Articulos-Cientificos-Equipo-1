using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GAC.Application.Contracts;
using GAC.Infrastructure.Data;
using GAC.Share.DTO.Response.GAC;
using Microsoft.EntityFrameworkCore;

namespace GAC.Infrastructure.Repositories;

public sealed class ConsultaInformesTecnicos(AppDbContext db) : IConsultaInformesTecnicos
{
    public async Task<IReadOnlyList<InformeTecnicoResponseDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var informes = await db.InformesTecnicos
            .AsNoTracking()
            .Include(i => i.Autorias)
            .Include(i => i.PalabrasClave)
            .Include(i => i.Ubicacion)
            .AsSplitQuery()
            .OrderByDescending(i => i.AnioPublicacion)
            .ThenByDescending(i => i.MesPublicacion)
            .ThenBy(i => i.Titulo)
            .ThenBy(i => i.Id)
            .ToListAsync(cancellationToken);

        return informes.Select(i => new InformeTecnicoResponseDto(
            i.Id, i.Titulo, i.CorreoContacto,
            i.Autorias.Select(a => a.InvestigadorId).ToArray(),
            i.PalabrasClave.Select(p => p.PalabraClaveId).ToArray(),
            i.Ubicacion is { } u
                ? new UbicacionResponseDto(u.Id, u.ArticuloId, u.GrupoInvestigacionId,
                    (int)u.Tipo, u.Laboratorio, u.Estanteria, u.InvestigadorId, u.Despacho)
                : null,
            i.NumeroInforme, i.CentroPublicacion, i.MesPublicacion,
            i.AnioPublicacion, i.TipoInformeId)).ToList();
    }
}