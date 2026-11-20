using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record UbicacionResponseDto(Guid Id, Guid ArticuloId, Guid GrupoInvestigacionId, int Tipo, string? Laboratorio, string? Estanteria, Guid? InvestigadorId, string? Despacho);
