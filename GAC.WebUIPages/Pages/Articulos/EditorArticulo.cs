using System.ComponentModel.DataAnnotations;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAC.WebUIPages.Pages.Articulos;

public abstract class EditorArticulo(IGacService gac) : PaginaGac
{
    protected IGacService Gac { get; } = gac;
    [BindProperty] public Entrada Datos { get; set; } = new();
    public List<SelectListItem> Autores { get; private set; } = [];
    public List<SelectListItem> Palabras { get; private set; } = [];
    public List<SelectListItem> Tipos { get; private set; } = [];
    public List<SelectListItem> Ediciones { get; private set; } = [];
    public List<SelectListItem> Revistas { get; private set; } = [];

    protected async Task CargarAsync(bool comunes, CancellationToken ct)
    {
        if (comunes)
        {
            Autores = (await Gac.Investigadores.ListarAsync(ct)).Select(x => Opcion(x.Id, $"{x.Nombre} · {x.CentroTrabajo}")).ToList();
            Palabras = (await Gac.PalabrasClave.ListarAsync(ct)).Select(x => Opcion(x.Id, x.Texto)).ToList();
        }
        Tipos = (await Gac.TiposInforme.ListarAsync(ct)).Select(x => Opcion(x.Id, x.Nombre)).ToList();
        var congresos = (await Gac.Congresos.ListarAsync(ct)).ToDictionary(x => x.Id, x => x.Nombre);
        Ediciones = (await Gac.EdicionesCongreso.ListarAsync(ct))
            .Select(x => Opcion(x.Id, $"{congresos.GetValueOrDefault(x.CongresoId)} · {x.Edicion} · {x.FechaInicio:yyyy}")).ToList();
        Revistas = (await Gac.Revistas.ListarAsync(ct)).Select(x => Opcion(x.Id, x.Nombre)).ToList();
    }

    private static SelectListItem Opcion(Guid id, string texto) => new(texto, id.ToString());

    protected void LlenarDesde(FichaArticuloDto ficha)
    {
        var a = ficha.Articulo;
        Datos = new Entrada
        {
            TipoPublicacion = a.TipoPublicacion, Titulo = a.Titulo, CorreoContacto = a.CorreoContacto,
            InvestigadorIds = a.InvestigadorIds.ToList(), PalabraClaveIds = a.PalabraClaveIds.ToList(),
            NumeroInforme = ficha.Informe?.NumeroInforme, CentroPublicacion = ficha.Informe?.CentroPublicacion,
            MesPublicacion = ficha.Informe?.MesPublicacion, AnioPublicacion = ficha.Informe?.AnioPublicacion ?? ficha.PublicacionRevista?.AnioPublicacion,
            TipoInformeId = ficha.Informe?.TipoInformeId, EdicionCongresoId = ficha.PublicacionCongreso?.EdicionCongresoId,
            RevistaId = ficha.PublicacionRevista?.RevistaId, NumeroRevista = ficha.PublicacionRevista?.NumeroRevista,
            PaginaInicial = ficha.PublicacionRevista?.PaginaInicial, PaginaFinal = ficha.PublicacionRevista?.PaginaFinal
        };
    }

    protected Task<GeneralResponseDto> GuardarArticuloAsync(Guid? id, CancellationToken ct)
    {
        var f = Datos;
        var titulo = f.Titulo ?? "";
        if (f.TipoPublicacion == 1)
        {
            var dto = new InformeTecnicoRequestDto
            {
                Titulo = titulo, CorreoContacto = f.CorreoContacto, InvestigadorIds = f.InvestigadorIds, PalabraClaveIds = f.PalabraClaveIds,
                NumeroInforme = f.NumeroInforme ?? "", CentroPublicacion = f.CentroPublicacion ?? "",
                MesPublicacion = f.MesPublicacion.GetValueOrDefault(), AnioPublicacion = f.AnioPublicacion.GetValueOrDefault(), TipoInformeId = f.TipoInformeId
            };
            return id is Guid k ? Gac.InformesTecnicos.ActualizarAsync(k, dto, ct) : Gac.InformesTecnicos.CrearAsync(dto, ct);
        }
        if (f.TipoPublicacion == 2)
        {
            var dto = new PublicacionCongresoRequestDto
            {
                Titulo = titulo, CorreoContacto = f.CorreoContacto, InvestigadorIds = f.InvestigadorIds, PalabraClaveIds = f.PalabraClaveIds,
                EdicionCongresoId = f.EdicionCongresoId
            };
            return id is Guid k ? Gac.PublicacionesCongreso.ActualizarAsync(k, dto, ct) : Gac.PublicacionesCongreso.CrearAsync(dto, ct);
        }
        var revista = new PublicacionRevistaRequestDto
        {
            Titulo = titulo, CorreoContacto = f.CorreoContacto, InvestigadorIds = f.InvestigadorIds, PalabraClaveIds = f.PalabraClaveIds,
            RevistaId = f.RevistaId, NumeroRevista = f.NumeroRevista ?? "", PaginaInicial = f.PaginaInicial.GetValueOrDefault(),
            PaginaFinal = f.PaginaFinal.GetValueOrDefault(), AnioPublicacion = f.AnioPublicacion.GetValueOrDefault()
        };
        return id is Guid r ? Gac.PublicacionesRevista.ActualizarAsync(r, revista, ct) : Gac.PublicacionesRevista.CrearAsync(revista, ct);
    }

    protected CambioModalidadRequestDto NuevoTipo() => new()
    {
        TipoPublicacion = Datos.TipoPublicacion, NumeroInforme = Datos.NumeroInforme, CentroPublicacion = Datos.CentroPublicacion,
        MesPublicacion = Datos.MesPublicacion, AnioPublicacion = Datos.AnioPublicacion, TipoInformeId = Datos.TipoInformeId,
        EdicionCongresoId = Datos.EdicionCongresoId, RevistaId = Datos.RevistaId, NumeroRevista = Datos.NumeroRevista,
        PaginaInicial = Datos.PaginaInicial, PaginaFinal = Datos.PaginaFinal
    };

    public sealed class Entrada
    {
        [Range(1, 3, ErrorMessage = "Elige una modalidad.")] public int TipoPublicacion { get; set; } = 1;
        [MaxLength(500)] public string? Titulo { get; set; }
        [EmailAddress(ErrorMessage = "Escribe un correo válido."), MaxLength(254)] public string? CorreoContacto { get; set; }
        public List<Guid> InvestigadorIds { get; set; } = [];
        public List<Guid> PalabraClaveIds { get; set; } = [];
        [MaxLength(80)] public string? NumeroInforme { get; set; }
        [MaxLength(250)] public string? CentroPublicacion { get; set; }
        public int? MesPublicacion { get; set; }
        public int? AnioPublicacion { get; set; }
        public Guid? TipoInformeId { get; set; }
        public Guid? EdicionCongresoId { get; set; }
        public Guid? RevistaId { get; set; }
        [MaxLength(80)] public string? NumeroRevista { get; set; }
        public int? PaginaInicial { get; set; }
        public int? PaginaFinal { get; set; }
    }
}
