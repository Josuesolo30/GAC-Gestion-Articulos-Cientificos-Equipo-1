using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GAC.Application.Constants;
using GAC.Infrastructure.Data;
using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace GAC.PruebasIntegracion;

internal static class Program
{
    public static async Task Main()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "gac-pruebas-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);
        var conexion = "Data Source=" + Path.Combine(carpeta, "gac.db") + ";Foreign Keys=True;Pooling=False";
        var password = "Prueba!3_" + Guid.NewGuid().ToString("N");
        var demo = new Dictionary<string, string?> { ["GacDemo:Email"] = "pruebas@example.test", ["GacDemo:Password"] = password };
        Guid guardado;
        try
        {
            await using (var api = CrearApi(conexion))
            {
                await using (var scope = api.Services.CreateAsyncScope())
                {
                    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
                    await GAC.API.DatosDemo.PrepararAsync(scope.ServiceProvider, new ConfigurationBuilder().AddInMemoryCollection(demo).Build());
                }
                using var sinSesion = api.CreateClient(Opciones());
                using var cliente = api.CreateClient(Opciones());
                await AutenticarApi(cliente, demo["GacDemo:Email"]!, password);

                await using var web = new WebApplicationFactory<GAC.WebUIPages.Program>().WithWebHostBuilder(b =>
                    b.UseEnvironment("Development").ConfigureLogging(log => log.ClearProviders())
                     .ConfigureTestServices(s => s.AddHttpClient(GacApiRoutes.ClientName)
                         .ConfigurePrimaryHttpMessageHandler(() => api.Server.CreateHandler())));
                using var navegador = web.CreateClient(Opciones());
                using var otro = web.CreateClient(Opciones());
                var prueba = new Escenarios(cliente, sinSesion, navegador, otro, demo["GacDemo:Email"]!, password);
                guardado = await prueba.EjecutarAsync();
                Comprueba(prueba.Total == 58, "Se esperaban 58 escenarios HU.");
            }

            await using (var apiReiniciada = CrearApi(conexion))
            {
                using var segunda = apiReiniciada.CreateClient(Opciones());
                await AutenticarApi(segunda, demo["GacDemo:Email"]!, password);
                await Ok(segunda, $"/api/articulos/{guardado}/ficha");
                Console.WriteLine("OK  Persistencia después de reiniciar la API sin volver a cargar datos.");
            }
            Console.WriteLine("58 escenarios de aceptación comprobados por HTTP y Razor Pages con una base SQLite temporal.");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(carpeta, recursive: true);
        }
    }

    private static WebApplicationFactory<GAC.API.Program> CrearApi(string conexion) =>
        new WebApplicationFactory<GAC.API.Program>().WithWebHostBuilder(b =>
            b.UseEnvironment("Development").ConfigureLogging(log => log.ClearProviders())
             .ConfigureTestServices(s =>
             {
                 s.RemoveAll<AppDbContext>();
                 s.RemoveAll<DbContextOptions<AppDbContext>>();
                 s.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                 s.AddDbContext<AppDbContext>(o => o.UseSqlite(conexion));
             }));

    private static WebApplicationFactoryClientOptions Opciones() =>
        new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = true };

    private static async Task AutenticarApi(HttpClient cliente, string email, string password)
    {
        using var r = await cliente.PostAsJsonAsync("/api/cuentas/login", new { email, password });
        await Estado(r, HttpStatusCode.OK);
        var token = await r.Content.ReadFromJsonAsync<Token>();
        Comprueba(!string.IsNullOrWhiteSpace(token?.AccessToken), "La API no devolvió credencial.");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

    internal record Token(string AccessToken);

    internal static void Comprueba(bool ok, string mensaje)
    {
        if (!ok) throw new InvalidOperationException(mensaje);
    }

    internal static async Task Estado(HttpResponseMessage r, HttpStatusCode codigo)
    {
        if (r.StatusCode != codigo)
            throw new InvalidOperationException($"{r.RequestMessage?.Method} {r.RequestMessage?.RequestUri?.PathAndQuery}: se esperaba {(int)codigo} y llegó {(int)r.StatusCode}. {((int)r.StatusCode >= 300 ? await r.Content.ReadAsStringAsync() : "")}");
    }

    internal static async Task Ok(HttpClient cliente, string url)
    {
        using var r = await cliente.GetAsync(url);
        await Estado(r, HttpStatusCode.OK);
    }

    internal static async Task<T> Json<T>(HttpClient c, string url)
    {
        using var r = await c.GetAsync(url);
        await Estado(r, HttpStatusCode.OK);
        return await r.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException($"Respuesta JSON vacía: {url}.");
    }

    internal static async Task<GeneralResponseDto> Enviar(HttpClient c, HttpMethod metodo, string url,
        object? datos = null, HttpStatusCode codigo = HttpStatusCode.OK)
    {
        using var q = new HttpRequestMessage(metodo, url)
            { Content = datos is null ? null : JsonContent.Create(datos, datos.GetType()) };
        using var r = await c.SendAsync(q);
        await Estado(r, codigo);
        var resultado = await r.Content.ReadFromJsonAsync<GeneralResponseDto>()
            ?? throw new InvalidOperationException("No hay un resultado de operación.");
        Comprueba(resultado.Success == ((int)codigo < 300), "El estado de la operación no coincide con HTTP.");
        return resultado;
    }

    internal static async Task<string> Pagina(HttpClient c, string url)
    {
        using var r = await c.GetAsync(url);
        await Estado(r, HttpStatusCode.OK);
        return WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());
    }

    internal static async Task<HttpResponseMessage> Formulario(HttpClient c, string origen, string destino,
        params (string Nombre, string Valor)[] datos)
    {
        var html = await Pagina(c, origen);
        var token = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
        Comprueba(token.Success, "No se encontró el token del formulario: " + origen);
        var valores = datos.Select(x => new KeyValuePair<string, string>(x.Nombre, x.Valor)).ToList();
        valores.Add(new("__RequestVerificationToken", token.Groups[1].Value));
        using var body = new FormUrlEncodedContent(valores);
        return await c.PostAsync(destino, body);
    }
}
