using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record CongresoResponseDto(Guid Id, string Nombre, int Tipo, string FrecuenciaCelebracion, int AnioPrimeraCelebracion);
