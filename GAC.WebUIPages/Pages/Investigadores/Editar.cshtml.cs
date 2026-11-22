using System.ComponentModel.DataAnnotations;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAC.WebUIPages.Pages.Investigadores;

public sealed class EditarModel(IGacService gac) : PaginaGac
{
    public Guid? Id { get; private set; }
    [BindProperty] public Entrada Datos { get; set; } = new();
    public IReadOnlyList<SelectListItem> Grupos { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Temas { get; private set; } = [];

    private async Task Opciones(CancellationToken ct)
    {
        Grupos = (await gac.GruposInvestigacion.ListarAsync(ct)).Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToArray();
        Temas = (await gac.Temas.ListarAsync(ct)).Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToArray();
    }

    public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken ct)
    {
        Id = id;
        if (id is Guid k)
        {
            var i = await gac.Investigadores.ObtenerAsync(k, ct);
            if (i is null) return NotFound();
            Datos = new Entrada { Nombre = i.Nombre, CentroTrabajo = i.CentroTrabajo, CorreoElectronico = i.CorreoElectronico,
                GrupoIds = i.GrupoIds?.ToList() ?? [], TemaIds = i.TemaIds?.ToList() ?? [] };
        }
        await Opciones(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id, CancellationToken ct)
    {
        Id = id;
        if (ModelState.IsValid)
        {
            var dto = new InvestigadorRequestDto { Nombre = Datos.Nombre, CentroTrabajo = Datos.CentroTrabajo, CorreoElectronico = Datos.CorreoElectronico,
                GrupoIds = Datos.GrupoIds, TemaIds = Datos.TemaIds };
            var r = id is Guid k ? await gac.Investigadores.ActualizarAsync(k, dto, ct) : await gac.Investigadores.CrearAsync(dto, ct);
            if (r.Success)
            {
                TempData["Aviso"] = id.HasValue ? "Cambios guardados." : "Investigador registrado.";
                return RedirectToPage("./Detalle", new { id = r.Id ?? id });
            }
            if (await RevisarAsync(r) is { } respuesta) return respuesta;
        }
        await Opciones(ct);
        return Page();
    }

    public sealed class Entrada
    {
        [Required(ErrorMessage = "Indica el nombre."), MaxLength(200)] public string Nombre { get; set; } = "";
        [Required(ErrorMessage = "Indica el centro de trabajo."), MaxLength(250)] public string CentroTrabajo { get; set; } = "";
        [EmailAddress(ErrorMessage = "Escribe un correo válido."), MaxLength(254)] public string? CorreoElectronico { get; set; }
        public List<Guid> GrupoIds { get; set; } = [];
        public List<Guid> TemaIds { get; set; } = [];
    }
}
