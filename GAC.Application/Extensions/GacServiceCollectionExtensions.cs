using GAC.Application.Constants;
using GAC.Application.Interfaces;
using GAC.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GAC.Application.Extensions;

public static class GacServiceCollectionExtensions
{
    /// <summary>Registra el cliente HTTP y el servicio del catálogo para Razor Pages.</summary>
    public static IServiceCollection AddGacApplication(this IServiceCollection services, Uri apiBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(apiBaseAddress);
        if (!apiBaseAddress.IsAbsoluteUri ||
            (apiBaseAddress.Scheme != Uri.UriSchemeHttp && apiBaseAddress.Scheme != Uri.UriSchemeHttps) ||
            apiBaseAddress.UserInfo.Length != 0 ||
            apiBaseAddress.Query.Length != 0 ||
            apiBaseAddress.Fragment.Length != 0)
        {
            throw new ArgumentException(
                "Indica la URL base absoluta de la API (HTTP o HTTPS), sin credenciales, consulta ni fragmento.",
                nameof(apiBaseAddress));
        }

        var address = new Uri(apiBaseAddress.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
        services.AddHttpClient(GacApiRoutes.ClientName, client => client.BaseAddress = address);
        services.AddTransient<HttpClientService>();
        services.AddTransient<IGacService, GacService>();
        return services;
    }
}
