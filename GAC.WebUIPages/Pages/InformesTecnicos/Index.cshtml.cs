using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages.InformesTecnicos;

public sealed class IndexModel(IGacService gac) : PageModel
{
    public IReadOnlyList<InformeTecnicoResponseDto> Informes { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => Informes = await gac.InformesTecnicos.ListarAsync(ct);
}
