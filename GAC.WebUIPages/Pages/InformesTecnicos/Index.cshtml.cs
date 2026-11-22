using System.Text.Json;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages.InformesTecnicos;

public sealed class IndexModel(IGacService gac, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<InformeTecnicoResponseDto> Informes { get; private set; } = [];
    public string? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Informes = await gac.InformesTecnicos.ListarAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogWarning(ex, "No se pudo consultar la API de informes técnicos.");
            Response.StatusCode = StatusCodes.Status502BadGateway;
            Error = "No se pudieron cargar los informes. Comprueba que la API esté en ejecución.";
        }
    }
}