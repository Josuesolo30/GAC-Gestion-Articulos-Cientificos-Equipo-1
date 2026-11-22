using System.Net;
using GAC.Share.DTO.Response.GAC;
using static GAC.PruebasIntegracion.Program;

namespace GAC.PruebasIntegracion;

internal sealed partial class Escenarios
{
    private async Task DetallesYBajas()
    {
        await Caso("HU16-E1", "Ficha web completa de las tres modalidades", async () =>
        {
            var informe = await Pagina(web, $"/Articulos/Detalle/{informeDemo}");
            var acta = await Pagina(web, $"/Articulos/Detalle/{congresoDemo}");
            var publicacion = await Pagina(web, $"/Articulos/Detalle/{revistaDemo}");
            Comprueba(informe.Contains("IT-DEMO-001") && informe.Contains("11/2026") && informe.Contains("Ana Rivera") && informe.Contains("agua")
                && acta.Contains("Congreso de Investigación Aplicada") && acta.Contains("Edición 2026") && acta.Contains("Santo Domingo")
                && publicacion.Contains("Revista de Estudios Aplicados") && publicacion.Contains("Comité editorial") && publicacion.Contains("Colaboradores de la revista"),
                "Faltan datos en una ficha web.");
        });
        await Caso("HU16-E2", "Ficha indica disponibilidad y ubicación actual", async () =>
        {
            var informe = await Pagina(web, $"/Articulos/Detalle/{informeDemo}");
            var sinCopia = await Pagina(web, $"/Articulos/Detalle/{revistaDemo}");
            Comprueba(informe.Contains("A-02") && informe.Contains("Laboratorio A")
                && sinCopia.Contains("El grupo no tiene una copia registrada."), "La disponibilidad mostrada es incorrecta.");
        });
        await Caso("HU16-E3", "Ficha enlaza autor y distingue el contacto del artículo", async () =>
        {
            var articulo = await Pagina(web, $"/Articulos/Detalle/{informeDemo}");
            var investigador = await Pagina(web, $"/Investigadores/Detalle/{autor}");
            Comprueba(articulo.Contains($"/Investigadores/Detalle/{autor}") && articulo.Contains("articulo@example.test")
                && investigador.Contains("UNICDA Laboratorio") && investigador.Contains("Recursos hídricos")
                && !investigador.Contains("articulo@example.test"), "Confundió la información del autor y del artículo.");
        });
        await Caso("HU17-E1", "El directorio incluye personas sin artículos", async () =>
        {
            var html = await Pagina(web, "/Investigadores");
            Comprueba(html.Contains("Ana Rivera") && html.Contains("María Ortiz") && (await Persona(externa)).Articulos.Count == 0,
                "El directorio omitió una persona externa sin artículos.");
        });
        await Caso("HU17-E2", "Busca investigadores por nombre, centro y tema", async () =>
        {
            foreach (var (campo,texto) in new[] { ("nombre", "ANA"), ("centro", "LABORATORIO"), ("tema", "RECURSOS") })
            {
                var r = await Leer<InvestigadorResponseDto[]>($"/api/investigadores/buscar?campo={campo}&texto={texto}");
                Comprueba(r.Any(x => x.Id == autor), "No encontró investigador por " + campo);
            }
            Comprueba((await Pagina(web, "/Investigadores?campo=nombre&texto=zz-no-existe")).Contains("No se encontraron investigadores."),
                "Falta el estado vacío del directorio.");
        });
        await Caso("HU17-E3", "Muestra correo y temas sin informar", async () =>
        {
            var html = await Pagina(web, $"/Investigadores/Detalle/{sinDatos}");
            Comprueba(html.Contains("No informado") && html.Contains("No informados") && html.Contains("Centro local"), "Ocultó datos de una ficha sin opcionales.");
        });
        await Caso("HU18-E1", "Borrar artículo desde la web elimina relaciones y conserva autores", async () =>
        {
            var anterior = await Ficha(individual);
            var url = $"/Articulos/Eliminar/{individual}";
            using var r = await Formulario(web, url, url);
            await Estado(r, HttpStatusCode.Redirect);
            foreach (var x in new[] { $"/api/articulos/{individual}/ficha", $"/api/autorias/{individual}/{autor}",
                         $"/api/articulos-palabras-clave/{individual}/{palabra}", $"/api/ubicaciones/{anterior.Articulo.Ubicacion!.Id}" })
            {
                using var a = await api.GetAsync(x);
                await Estado(a, HttpStatusCode.NotFound);
            }
            Comprueba((await Persona(autor)).Articulos.Any(a => a.Id == altaWeb), "Eliminar un artículo afectó a su autor.");
        });
        await Caso("HU18-E2", "Abrir y cancelar eliminación conserva el artículo", async () =>
        {
            var original = await Ficha(altaWeb);
            var html = await Pagina(web, $"/Articulos/Eliminar/{altaWeb}");
            Comprueba(html.Contains("Cancelar") && html.Contains(original.Articulo.Titulo), "No identifica el artículo a eliminar.");
            await Pagina(web, $"/Articulos/Detalle/{altaWeb}");
            var actual = await Ficha(altaWeb);
            Comprueba(actual.Autores.Count == original.Autores.Count && actual.PalabrasClave.Count == original.PalabrasClave.Count, "Cambió la ficha sin confirmar.");
        });
        await Caso("HU19-E1", "Baja controlada de investigador sin referencias", async () =>
        {
            await Post("/api/investigadores-temas", new { investigadorId = sinDatos, temaId = tema }, HttpStatusCode.OK);
            var url = $"/Investigadores/Eliminar/{sinDatos}";
            using var r = await Formulario(web, url, url);
            await Estado(r, HttpStatusCode.Redirect);
            foreach (var x in new[] { $"/api/investigadores/{sinDatos}", $"/api/investigadores-temas/{sinDatos}/{tema}" })
            {
                using var a = await api.GetAsync(x);
                await Estado(a, HttpStatusCode.NotFound);
            }
            await Persona(autor);
        });
        await Caso("HU19-E2", "Impide borrar autores o responsables de despacho", async () =>
        {
            await Borrar($"/api/investigadores/{autor}", HttpStatusCode.Conflict);
            var soloDespacho = Id(await Post("/api/investigadores", new { nombre = "Responsable sin autorías",
                centroTrabajo = "UNICDA", grupoIds = new[] { grupo } }));
            await Post("/api/ubicaciones", Ubicacion(altaWeb, despacho: true, responsable: soloDespacho));
            var motivo = await Borrar($"/api/investigadores/{soloDespacho}", HttpStatusCode.Conflict);
            Comprueba(motivo.Message?.Contains("despacho") == true && (await Persona(soloDespacho)).Articulos.Count == 0,
                "No se verificó el bloqueo por despacho sin autoría.");
        });
        await Caso("HU19-E3", "Cancelar baja de investigador preserva su ficha", async () =>
        {
            Comprueba((await Pagina(web, $"/Investigadores/Eliminar/{externa}")).Contains("Cancelar"), "Falta la cancelación.");
            await Pagina(web, $"/Investigadores/Detalle/{externa}");
            Comprueba((await Persona(externa)).Investigador.Nombre == "María Ortiz", "Se quitó el investigador sin confirmación.");
        });
    }

    private async Task CerrarSesion()
    {
        await Caso("HU02-E1", "La sesión web se cierra por POST protegido", async () =>
        {
            using var sinToken = await web.PostAsync("/Cuenta/Salir", null);
            await Estado(sinToken, HttpStatusCode.BadRequest);
            using var r = await Formulario(web, "/Articulos", "/Cuenta/Salir");
            await Estado(r, HttpStatusCode.Redirect);
            Comprueba(r.Headers.Location?.ToString().Contains("/Cuenta/Entrar") == true, "No regresó al acceso.");
        });
        await Caso("HU02-E2", "Al salir ya no se consulta una URL protegida", async () =>
        {
            using var r = await web.GetAsync($"/Articulos/Detalle/{altaWeb}");
            await Estado(r, HttpStatusCode.Redirect);
            Comprueba(r.Headers.Location?.ToString().Contains("/Cuenta/Entrar") == true, "La ficha siguió accesible tras salir.");
        });
    }
}
