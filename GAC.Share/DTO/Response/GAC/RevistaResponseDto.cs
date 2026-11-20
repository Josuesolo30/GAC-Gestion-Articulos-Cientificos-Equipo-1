using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record RevistaResponseDto(Guid Id, string Nombre, string NombreEditor, int AnioInicioPublicacion, string FrecuenciaPublicacion);
