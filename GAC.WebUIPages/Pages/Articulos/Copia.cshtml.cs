using System.ComponentModel.DataAnnotations;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class CopiaModel(IGacService gac) : PaginaGac
{
    public ArticuloResponseDto Articulo { get; private set; } = null!;
    [BindProperty] public Entrada Datos { get; set; } = new();
    public IReadOnlyList<SelectListItem> Grupos { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Investigadores { get; private set; } = [];

    private async Task<bool> CargarArticuloAsync(Guid id, CancellationToken ct)
    {
        if (await gac.Articulos.ObtenerAsync(id, ct) is not { } a) return false;
        Articulo = a;
        return true;
    }

    private async Task CargarOpcionesAsync(CancellationToken ct)
    {
        Grupos = (await gac.GruposInvestigacion.ListarAsync(ct)).Select(g => new SelectListItem(g.Nombre,g.Id.ToString())).ToArray();
        Investigadores = (await gac.Investigadores.ListarAsync(ct))
            .Where(x => x.GrupoIds is { Count: > 0 })
            .Select(i => new SelectListItem($"{i.Nombre} · {i.CentroTrabajo}",i.Id.ToString())).ToArray();
    }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        if (!await CargarArticuloAsync(id, ct)) return NotFound();
        if (Articulo.Ubicacion is { } u)
            Datos = new Entrada { GrupoInvestigacionId = u.GrupoInvestigacionId, Tipo = u.Tipo, Laboratorio = u.Laboratorio,
                Estanteria = u.Estanteria, InvestigadorId = u.InvestigadorId, Despacho = u.Despacho };
        await CargarOpcionesAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        if (!await CargarArticuloAsync(id, ct)) return NotFound();
        if (ModelState.IsValid)
        {
            var dto = new UbicacionRequestDto { ArticuloId = id, GrupoInvestigacionId = Datos.GrupoInvestigacionId,
                Tipo = Datos.Tipo, Laboratorio = Datos.Laboratorio, Estanteria = Datos.Estanteria,
                InvestigadorId = Datos.InvestigadorId, Despacho = Datos.Despacho };
            var r = Articulo.Ubicacion is { } u ? await gac.Ubicaciones.ActualizarAsync(u.Id, dto, ct) : await gac.Ubicaciones.CrearAsync(dto, ct);
            if (r.Success)
            {
                TempData["Aviso"] = "Ubicación guardada.";
                return RedirectToPage("./Detalle", new { id });
            }
            if (await RevisarAsync(r) is { } respuesta) return respuesta;
        }
        await CargarOpcionesAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRetirarAsync(Guid id, CancellationToken ct)
    {
        if (!await CargarArticuloAsync(id, ct)) return NotFound();
        if (Articulo.Ubicacion is { } u)
        {
            var r = await gac.Ubicaciones.EliminarAsync(u.Id, ct);
            if (!r.Success)
            {
                var respuesta = await RevisarAsync(r);
                await CargarOpcionesAsync(ct);
                return respuesta ?? Page();
            }
        }
        TempData["Aviso"] = "La ficha se conserva y ahora figura sin copia física.";
        return RedirectToPage("./Detalle", new { id });
    }

    public sealed class Entrada
    {
        [Required(ErrorMessage = "Selecciona el grupo.")] public Guid? GrupoInvestigacionId { get; set; }
        [Range(1,2)] public int Tipo { get; set; } = 1;
        [MaxLength(150)] public string? Laboratorio { get; set; }
        [MaxLength(100)] public string? Estanteria { get; set; }
        public Guid? InvestigadorId { get; set; }
        [MaxLength(150)] public string? Despacho { get; set; }
    }
}
