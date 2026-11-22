using System.Net.Http.Headers;

namespace GAC.WebUIPages.Seguridad;

public static class SesionGac
{
    public const string ClaveToken = "Gac.AccessToken";
}

public sealed class CredencialApiHandler(IHttpContextAccessor contexto) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var sesion = contexto.HttpContext;
        if (sesion?.User.Identity?.IsAuthenticated == true)
        {
            await sesion.Session.LoadAsync(ct);
            if (sesion.Session.GetString(SesionGac.ClaveToken) is { } token)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}
