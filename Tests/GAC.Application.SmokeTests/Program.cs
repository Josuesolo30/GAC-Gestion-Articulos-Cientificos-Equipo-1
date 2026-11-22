using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GAC.Application.Constants;
using GAC.Application.Extensions;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.GAC;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGacApplication(new Uri("https://gac.test/backend"));
var handler = new ApiSimulada();
services.AddHttpClient(GacApiRoutes.ClientName)
    .ConfigurePrimaryHttpMessageHandler(() => handler);
using var provider = services.BuildServiceProvider();
var gac = provider.GetRequiredService<IGacService>();

var articulo = Guid.NewGuid();
var investigador = Guid.NewGuid();
var palabra = Guid.NewGuid();
var tipo = Guid.NewGuid();
var informe = new InformeTecnicoRequestDto
{
    Titulo = "Calidad del agua",
    InvestigadorIds = [investigador],
    PalabraClaveIds = [palabra],
    NumeroInforme = "IT-001",
    CentroPublicacion = "UNICDA",
    MesPublicacion = 11,
    AnioPublicacion = 2026,
    TipoInformeId = tipo
};

handler.Esperar(HttpMethod.Post, "/backend/api/informes-tecnicos",
    Respuesta(HttpStatusCode.Created, new GeneralResponseDto(true, "Informe registrado.")),
    body =>
    {
        Verifica(body.GetProperty("titulo").GetString() == "Calidad del agua", "Se perdió el título heredado.");
        Verifica(body.GetProperty("numeroInforme").GetString() == "IT-001", "Se perdió el número del informe.");
        Verifica(body.GetProperty("investigadorIds")[0].GetGuid() == investigador, "No se envió el autor.");
        Verifica(body.GetProperty("tipoInformeId").GetGuid() == tipo, "No se envió la categoría.");
    });
Verifica((await gac.InformesTecnicos.CrearAsync(informe)).Success, "El registro debió indicar éxito.");

var informeDevuelto = new InformeTecnicoResponseDto(
    articulo, informe.Titulo, null, [investigador], [palabra], null,
    informe.NumeroInforme, informe.CentroPublicacion, informe.MesPublicacion, informe.AnioPublicacion, tipo);

handler.Esperar(HttpMethod.Get, "/backend/api/informes-tecnicos", Respuesta(HttpStatusCode.OK, new[] { informeDevuelto }));
var lista = await gac.InformesTecnicos.ListarAsync();
Verifica(lista.Single().Id == articulo && lista.Single().TipoInformeId == tipo,
    "El listado no conservó los datos del informe.");

handler.Esperar(HttpMethod.Get, $"/backend/api/informes-tecnicos/{articulo:D}", Respuesta(HttpStatusCode.OK, informeDevuelto));
var detalle = await gac.InformesTecnicos.ObtenerAsync(articulo);
Verifica(detalle?.NumeroInforme == "IT-001", "El detalle cambió el número del informe.");

handler.Esperar(HttpMethod.Put, $"/backend/api/informes-tecnicos/{articulo:D}",
    Respuesta(HttpStatusCode.OK, new GeneralResponseDto(false, "El informe no se puede cambiar.")));
var cambio = await gac.InformesTecnicos.ActualizarAsync(articulo, informe);
Verifica(!cambio.Success && cambio.Message == "El informe no se puede cambiar.",
    "Se perdió el resultado de negocio de la respuesta.");

handler.Esperar(HttpMethod.Delete, $"/backend/api/autorias/{articulo:D}/{investigador:D}",
    new HttpResponseMessage(HttpStatusCode.NoContent));
Verifica((await gac.Autorias.EliminarAsync((articulo, investigador))).Success, "204 debe indicar éxito.");

handler.Esperar(HttpMethod.Delete, $"/backend/api/informes-tecnicos/{articulo:D}",
    new HttpResponseMessage(HttpStatusCode.Forbidden));
Verifica(!(await gac.InformesTecnicos.EliminarAsync(articulo)).Success, "403 no debe indicar éxito.");

handler.Esperar(HttpMethod.Get, $"/backend/api/informes-tecnicos/{articulo:D}",
    new HttpResponseMessage(HttpStatusCode.NotFound));
Verifica(await gac.InformesTecnicos.ObtenerAsync(articulo) is null, "Una consulta por ID inexistente debe retornar null.");

handler.Esperar(HttpMethod.Get, "/backend/api/informes-tecnicos", new HttpResponseMessage(HttpStatusCode.NotFound));
try
{
    await gac.InformesTecnicos.ListarAsync();
    throw new InvalidOperationException("Una ruta de listado inexistente no debe parecer una lista vacía.");
}
catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
{
}

handler.ComprobarQueTermino();
Console.WriteLine("8 comprobaciones superadas: cliente registrado, rutas, verbos, DTO en JSON y respuestas HTTP.");

static HttpResponseMessage Respuesta<T>(HttpStatusCode status, T value) =>
    new(status) { Content = JsonContent.Create(value) };

static void Verifica(bool correcto, string mensaje)
{
    if (!correcto)
        throw new InvalidOperationException(mensaje);
}

sealed class ApiSimulada : HttpMessageHandler
{
    private readonly Queue<(HttpMethod Metodo, string Ruta, HttpResponseMessage Respuesta, Action<JsonElement>? Revisar)> _esperadas = new();

    public void Esperar(HttpMethod metodo, string ruta, HttpResponseMessage respuesta, Action<JsonElement>? revisar = null)
        => _esperadas.Enqueue((metodo, ruta, respuesta, revisar));

    public void ComprobarQueTermino()
    {
        if (_esperadas.Count != 0)
            throw new InvalidOperationException("El cliente no realizó todas las llamadas esperadas.");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var esperado = _esperadas.Dequeue();
        if (request.Method != esperado.Metodo || request.RequestUri?.AbsolutePath != esperado.Ruta)
            throw new InvalidOperationException($"Petición inesperada: {request.Method} {request.RequestUri}.");

        if (esperado.Revisar is not null)
        {
            if (request.Content is null)
                throw new InvalidOperationException("Falta el cuerpo de la solicitud.");
            using var json = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            esperado.Revisar(json.RootElement);
        }

        return esperado.Respuesta;
    }
}
