using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAC.Share.DTO.Response.GAC;

namespace GAC.Application.Contracts;

public interface IConsultaInformesTecnicos
{
    Task<IReadOnlyList<InformeTecnicoResponseDto>> ListarAsync(
        CancellationToken cancellationToken = default);
}