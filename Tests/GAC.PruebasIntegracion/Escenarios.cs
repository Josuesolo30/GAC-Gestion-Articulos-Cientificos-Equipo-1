using System.Net;
using System.Net.Http.Json;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.GAC;
using static GAC.PruebasIntegracion.Program;

namespace GAC.PruebasIntegracion;

internal sealed partial class Escenarios(HttpClient api, HttpClient sinSesion, HttpClient web, HttpClient otro,
    string email, string clave)
{
    private readonly HashSet<string> casos = [];
    public int Total => casos.Count;

    private Guid autor, miembro, externa, grupo, tema, palabra, palabra2, tipo, congreso, edicion, revista;
    private Guid informeDemo, congresoDemo, revistaDemo, altaWeb, individual, sinDatos;

    private async Task Caso(string codigo, string descripcion, Func<Task> accion)
    {
        try { await accion(); }
        catch (Exception ex) { throw new InvalidOperationException($"{codigo}: {descripcion}", ex); }
        Comprueba(casos.Add(codigo), "Código de prueba repetido.");
        Console.WriteLine($"OK  {codigo} · {descripcion}");
    }

    private Task<T> Leer<T>(string ruta) => Json<T>(api, ruta);
    private Task<GeneralResponseDto> Post(string ruta, object dto, HttpStatusCode estado = HttpStatusCode.Created) => Enviar(api, HttpMethod.Post, ruta, dto, estado);
    private Task<GeneralResponseDto> Put(string ruta, object dto, HttpStatusCode estado = HttpStatusCode.OK) => Enviar(api, HttpMethod.Put, ruta, dto, estado);
    private Task<GeneralResponseDto> Borrar(string ruta, HttpStatusCode estado = HttpStatusCode.OK) => Enviar(api, HttpMethod.Delete, ruta, codigo: estado);
    private Task<FichaArticuloDto> Ficha(Guid id) => Leer<FichaArticuloDto>($"/api/articulos/{id}/ficha");
    private Task<FichaInvestigadorDto> Persona(Guid id) => Leer<FichaInvestigadorDto>($"/api/investigadores/{id}/ficha");

    private static Guid Id(GeneralResponseDto r) => r.Id ?? throw new InvalidOperationException("La API no devolvió un identificador.");
    private static void Campo(GeneralResponseDto r, string campo) =>
        Comprueba(r.Errors?.Keys.Any(k => string.Equals(k, campo, StringComparison.OrdinalIgnoreCase)) == true,
            "Faltó el error de validación para " + campo);

    private InformeTecnicoRequestDto Informe(string titulo = "Informe de prueba",
        IReadOnlyList<Guid>? autores = null, IReadOnlyList<Guid>? palabras = null, string? correo = null,
        string numero = "IT-PRUEBA", string centro = "UNICDA", int mes = 11, int anio = 2026) => new()
    {
        Titulo = titulo, InvestigadorIds = autores ?? [autor], PalabraClaveIds = palabras ?? [palabra], CorreoContacto = correo,
        NumeroInforme = numero, CentroPublicacion = centro, MesPublicacion = mes, AnioPublicacion = anio, TipoInformeId = tipo
    };

    private PublicacionRevistaRequestDto PublicarRevista(Guid? cual = null, int primero = 1, int ultimo = 2, int anio = 2026,
        string numero = "2026-2") => new()
    {
        Titulo = "Artículo en revista de prueba", InvestigadorIds = [autor], PalabraClaveIds = [palabra],
        RevistaId = cual ?? revista, PaginaInicial = primero, PaginaFinal = ultimo, AnioPublicacion = anio, NumeroRevista = numero
    };

    public async Task<Guid> EjecutarAsync()
    {
        var personas = await Leer<InvestigadorResponseDto[]>("/api/investigadores");
        autor = personas.Single(x => x.Nombre == "Ana Rivera").Id;
        miembro = personas.Single(x => x.Nombre == "Luis Castillo").Id;
        externa = personas.Single(x => x.Nombre == "María Ortiz").Id;
        grupo = (await Leer<GrupoInvestigacionResponseDto[]>("/api/grupos-investigacion")).Single().Id;
        tema = (await Leer<TemaResponseDto[]>("/api/temas")).Single(x => x.Nombre == "Recursos hídricos").Id;
        var claves = await Leer<PalabraClaveResponseDto[]>("/api/palabras-clave");
        palabra = claves.Single(x => x.Texto == "agua").Id;
        palabra2 = claves.Single(x => x.Texto == "medición").Id;
        tipo = (await Leer<TipoInformeResponseDto[]>("/api/tipos-informe")).Single().Id;
        congreso = (await Leer<CongresoResponseDto[]>("/api/congresos")).Single().Id;
        edicion = (await Leer<EdicionCongresoResponseDto[]>("/api/ediciones-congreso")).Single().Id;
        revista = (await Leer<RevistaResponseDto[]>("/api/revistas")).Single().Id;
        var fichas = await Leer<ArticuloResponseDto[]>("/api/articulos");
        informeDemo = fichas.Single(x => x.TipoPublicacion == 1).Id;
        congresoDemo = fichas.Single(x => x.TipoPublicacion == 2).Id;
        revistaDemo = fichas.Single(x => x.TipoPublicacion == 3).Id;

        await AccesoYDirectorio();
        await AltaEdicionYRelaciones();
        await CongresosYRevistas();
        await UbicacionesYBusqueda();
        await DetallesYBajas();
        await ComprobarPantallasDeMantenimiento();
        await CerrarSesion();
        return altaWeb;
    }

    private async Task AccesoYDirectorio()
    {
        await Caso("HU01-E1", "Credenciales válidas y web autenticada", async () =>
        {
            using var r = await Formulario(web, "/Cuenta/Entrar", "/Cuenta/Entrar", ("Datos.Email", email), ("Datos.Password", clave));
            await Estado(r, HttpStatusCode.Redirect);
            Comprueba((await Pagina(web, "/Articulos")).Contains("Calidad del agua"), "No se abrió el catálogo.");
        });
        await Caso("HU01-E2", "Las credenciales inválidas reciben el mismo aviso", async () =>
        {
            using var incorrecta = await Formulario(otro, "/Cuenta/Entrar", "/Cuenta/Entrar", ("Datos.Email", email), ("Datos.Password", "ClaveIncorrecta!3"));
            using var desconocida = await Formulario(otro, "/Cuenta/Entrar", "/Cuenta/Entrar", ("Datos.Email", "desconocido@example.test"), ("Datos.Password", "ClaveIncorrecta!3"));
            foreach (var r in new[] { incorrecta, desconocida })
                Comprueba(r.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync())
                    .Contains("Correo o contraseña incorrectos."), "Mensaje de acceso incorrecto.");
        });
        await Caso("HU01-E3", "Consulta y edición requieren sesión", async () =>
        {
            foreach (var ruta in new[] { "/Articulos", $"/Articulos/Editar/{informeDemo}", "/InformesTecnicos" })
            {
                using var r = await otro.GetAsync(ruta);
                await Estado(r, HttpStatusCode.Redirect);
                Comprueba(r.Headers.Location?.ToString().Contains("/Cuenta/Entrar") == true, "No pidió iniciar sesión.");
            }
            using var a = await sinSesion.GetAsync("/api/informes-tecnicos");
            await Estado(a, HttpStatusCode.Unauthorized);
            using var b = await sinSesion.PostAsJsonAsync("/api/informes-tecnicos", Informe());
            await Estado(b, HttpStatusCode.Unauthorized);
        });

        await Caso("HU03-E1", "Alta con grupos, temas y variante externa", async () =>
        {
            var id = Id(await Post("/api/investigadores", new { nombre = "Investigadora de pruebas", centroTrabajo = "Laboratorio GAC", grupoIds = new[] { grupo }, temaIds = new[] { tema } }));
            var f = await Persona(id);
            Comprueba(f.Grupos.Single().Id == grupo && f.Temas.Single().Id == tema, "No guardó relaciones de investigador.");
            Comprueba((await Persona(externa)).Grupos.Count == 0, "Una investigadora externa aparece como miembro.");
        });
        await Caso("HU03-E2", "Alta sin correo ni temas", async () =>
        {
            sinDatos = Id(await Post("/api/investigadores", new { nombre = "Persona sin datos opcionales", centroTrabajo = "Centro local" }));
            var f = await Persona(sinDatos);
            Comprueba(f.Investigador.CorreoElectronico is null && f.Temas.Count == 0, "Cambió campos opcionales.");
        });
        await Caso("HU03-E3", "Rechazo del nombre, centro o correo inválidos", async () =>
        {
            Campo(await Post("/api/investigadores", new { nombre = "", centroTrabajo = "Centro" }, HttpStatusCode.BadRequest), "Nombre");
            Campo(await Post("/api/investigadores", new { nombre = "Prueba sin centro" }, HttpStatusCode.BadRequest), "CentroTrabajo");
            Campo(await Post("/api/investigadores", new { nombre = "Prueba correo", centroTrabajo = "Centro", correoElectronico = "x" }, HttpStatusCode.BadRequest), "CorreoElectronico");
            Comprueba((await Leer<InvestigadorResponseDto[]>("/api/investigadores/buscar?texto=Prueba%20correo")).Length == 0, "Se guardó un investigador inválido.");
        });
        await Caso("HU04-E1", "Actualización conserva autorías", async () =>
        {
            var antes = (await Persona(autor)).Articulos.Count;
            await Put($"/api/investigadores/{autor}", new { nombre = "Ana Rivera", centroTrabajo = "UNICDA Laboratorio", temaIds = new[] { tema } });
            var despues = await Persona(autor);
            Comprueba(despues.Investigador.CentroTrabajo == "UNICDA Laboratorio" && despues.Articulos.Count == antes, "Perdió autorías al editar.");
        });
        await Caso("HU04-E2", "Distingue investigadores homónimos por ID", async () =>
        {
            var a = Id(await Post("/api/investigadores", new { nombre = "Mismo nombre", centroTrabajo = "Centro A" }));
            var b = Id(await Post("/api/investigadores", new { nombre = "Mismo nombre", centroTrabajo = "Centro B" }));
            await Put($"/api/investigadores/{a}", new { nombre = "Mismo nombre", centroTrabajo = "Centro C" });
            Comprueba((await Persona(a)).Investigador.CentroTrabajo == "Centro C" && (await Persona(b)).Investigador.CentroTrabajo == "Centro B", "Se cambió el homónimo.");
        });
        await Caso("HU04-E3", "El despacho impide retirar la membresía", async () =>
        {
            Campo(await Put($"/api/investigadores/{miembro}", new { nombre = "Luis Castillo", centroTrabajo = "UNICDA", grupoIds = Array.Empty<Guid>() }, HttpStatusCode.BadRequest), "GrupoIds");
            Comprueba((await Persona(miembro)).Grupos.Single().Id == grupo, "Se perdió la membresía.");
        });
    }

    private async Task AltaEdicionYRelaciones()
    {
        await Caso("HU05-E1", "Alta desde Razor Pages con dos autores y palabras", async () =>
        {
            using var r = await Formulario(web, "/Articulos/Editar?tipo=1", "/Articulos/Editar",
                ("Datos.TipoPublicacion","1"), ("Datos.Titulo","Informe desde el formulario"), ("Datos.NumeroInforme","IT-002"),
                ("Datos.CentroPublicacion","UNICDA"), ("Datos.MesPublicacion","11"), ("Datos.AnioPublicacion","2026"),
                ("Datos.TipoInformeId",tipo.ToString()), ("Datos.InvestigadorIds",autor.ToString()), ("Datos.InvestigadorIds",miembro.ToString()),
                ("Datos.PalabraClaveIds",palabra.ToString()), ("Datos.PalabraClaveIds",palabra2.ToString()));
            await Estado(r, HttpStatusCode.Redirect);
            altaWeb = Guid.Parse(r.Headers.Location!.ToString().Split('/').Last());
            var f = await Ficha(altaWeb);
            Comprueba(f.Autores.Count == 2 && f.PalabrasClave.Count == 2 && f.Informe?.NumeroInforme == "IT-002", "El formulario no guardó la ficha completa.");
        });
        await Caso("HU05-E2", "Título, autores y palabras son obligatorios", async () =>
        {
            var antes = (await Leer<ArticuloResponseDto[]>("/api/articulos")).Length;
            foreach (var dto in new[] { Informe(titulo: ""), Informe(autores: []), Informe(palabras: []) })
                await Post("/api/informes-tecnicos", dto, HttpStatusCode.BadRequest);
            Comprueba((await Leer<ArticuloResponseDto[]>("/api/articulos")).Length == antes, "Quedó una ficha parcial.");
        });
        await Caso("HU05-E3", "Guarda un informe sin contacto", async () =>
        {
            individual = Id(await Post("/api/informes-tecnicos", Informe()));
            Comprueba((await Ficha(individual)).Articulo.CorreoContacto is null, "El correo debía ser opcional.");
        });
        await Caso("HU05-E4", "Correo de contacto inválido", async () =>
            Campo(await Post("/api/informes-tecnicos", Informe(correo: "correo-mal"), HttpStatusCode.BadRequest), "CorreoContacto"));

        await Caso("HU06-E1", "Edita una ficha conservando ID, relaciones y copia", async () =>
        {
            var f = await Ficha(informeDemo);
            await Put($"/api/informes-tecnicos/{informeDemo}", Informe("Calidad del agua actualizada", f.Articulo.InvestigadorIds, f.Articulo.PalabraClaveIds, "articulo@example.test", "IT-DEMO-001"));
            var editada = await Ficha(informeDemo);
            Comprueba(editada.Articulo.Id == informeDemo && editada.Articulo.CorreoContacto == "articulo@example.test"
                && editada.Autores.Single().Id == f.Autores.Single().Id && editada.PalabrasClave.Single().Id == f.PalabrasClave.Single().Id
                && editada.Articulo.Ubicacion?.Id == f.Articulo.Ubicacion?.Id, "La edición perdió datos.");
        });
        await Caso("HU06-E2", "Cambia las tres modalidades sin reemplazar la ficha", async () =>
        {
            var f = await Ficha(informeDemo);
            var url = $"/api/articulos/{informeDemo}/modalidad";
            await Put(url, new { tipoPublicacion = 2, edicionCongresoId = edicion });
            var acta = await Ficha(informeDemo);
            Comprueba(acta.Informe is null && acta.PublicacionCongreso?.EdicionCongresoId == edicion && acta.Articulo.Ubicacion?.Id == f.Articulo.Ubicacion?.Id, "No cambió correctamente a congreso.");
            await Put(url, new { tipoPublicacion = 3, revistaId = revista, numeroRevista = "2026-X", paginaInicial = 1, paginaFinal = 2, anioPublicacion = 2026 });
            var revistaCambiada = await Ficha(informeDemo);
            Comprueba(revistaCambiada.PublicacionCongreso is null && revistaCambiada.PublicacionRevista?.RevistaId == revista, "No cambió correctamente a revista.");
            await Put(url, new { tipoPublicacion = 1, tipoInformeId = tipo, numeroInforme = "IT-DEMO-001", centroPublicacion = "UNICDA", mesPublicacion = 11, anioPublicacion = 2026 });
            var informe = await Ficha(informeDemo);
            Comprueba(informe.Articulo.Id == f.Articulo.Id && informe.PublicacionRevista is null && informe.Informe?.NumeroInforme == "IT-DEMO-001"
                && informe.Autores.Single().Id == f.Autores.Single().Id && informe.PalabrasClave.Single().Id == f.PalabrasClave.Single().Id
                && informe.Articulo.Ubicacion?.Id == f.Articulo.Ubicacion?.Id, "No se conservaron las relaciones.");
        });
        await Caso("HU06-E3", "Edición o cambio inválidos preservan los datos", async () =>
        {
            var original = (await Ficha(informeDemo)).Articulo;
            await Put($"/api/informes-tecnicos/{informeDemo}", Informe(titulo: "No guardar", palabras: []), HttpStatusCode.BadRequest);
            await Put($"/api/articulos/{informeDemo}/modalidad", new { tipoPublicacion = 2 }, HttpStatusCode.BadRequest);
            Comprueba((await Ficha(informeDemo)).Articulo.Titulo == original.Titulo
                && (await Ficha(informeDemo)).Articulo.TipoPublicacion == original.TipoPublicacion, "Una edición inválida cambió la ficha.");
        });

        await Caso("HU07-E1", "Asocia a un autor de otros artículos", async () =>
        {
            await Post("/api/autorias", new { articuloId = individual, investigadorId = miembro }, HttpStatusCode.OK);
            Comprueba((await Persona(miembro)).Articulos.Any(x => x.Id == individual) && (await Persona(miembro)).Articulos.Any(x => x.Id == congresoDemo), "El autor no quedó compartido.");
        });
        await Caso("HU07-E2", "Repetir autoría no duplica", async () =>
        {
            await Post("/api/autorias", new { articuloId = individual, investigadorId = miembro }, HttpStatusCode.OK);
            Comprueba((await Ficha(individual)).Autores.Count(x => x.Id == miembro) == 1, "Duplicó la autoría.");
        });
        await Caso("HU07-E3", "No se retira el último autor", async () =>
        {
            await Borrar($"/api/autorias/{individual}/{miembro}");
            await Borrar($"/api/autorias/{individual}/{autor}", HttpStatusCode.Conflict);
            Comprueba((await Ficha(individual)).Autores.Count == 1, "Quedó sin autor.");
        });
        await Caso("HU08-E1", "Varias palabras en la misma ficha", async () =>
        {
            await Post("/api/articulos-palabras-clave", new { articuloId = individual, palabraClaveId = palabra2 }, HttpStatusCode.OK);
            Comprueba((await Ficha(individual)).PalabrasClave.Count == 2, "Perdió palabras.");
        });
        await Caso("HU08-E2", "Repetir palabra no duplica y no se quita la última", async () =>
        {
            await Post("/api/articulos-palabras-clave", new { articuloId = individual, palabraClaveId = palabra2 }, HttpStatusCode.OK);
            Comprueba((await Ficha(individual)).PalabrasClave.Count == 2, "Duplicó la palabra.");
            await Borrar($"/api/articulos-palabras-clave/{individual}/{palabra2}");
            await Borrar($"/api/articulos-palabras-clave/{individual}/{palabra}", HttpStatusCode.Conflict);
            Comprueba((await Ficha(individual)).PalabrasClave.Count == 1, "Quedó sin palabra.");
        });
        await Caso("HU09-E1", "Informe con categoría y metadatos completos", async () =>
        {
            var f = await Ficha(altaWeb);
            Comprueba(f.Informe?.NumeroInforme == "IT-002" && f.Informe.CentroPublicacion == "UNICDA" &&
                f.Informe.MesPublicacion == 11 && f.Informe.AnioPublicacion == 2026 && f.TipoInforme?.Id == tipo, "Metadatos incorrectos.");
        });
        await Caso("HU09-E2", "Rechaza datos propios de informe incompletos", async () =>
        {
            foreach (var dto in new[] { Informe(numero: ""), Informe(centro: ""), Informe(mes: 0), Informe(anio: 0) })
                await Post("/api/informes-tecnicos", dto, HttpStatusCode.BadRequest);
        });
        await Caso("HU09-E3", "Valida mes y año y conserva el formulario", async () =>
        {
            await Post("/api/informes-tecnicos", Informe(mes: 13), HttpStatusCode.BadRequest);
            await Post("/api/informes-tecnicos", Informe(anio: 10_000), HttpStatusCode.BadRequest);
            using var r = await Formulario(web, "/Articulos/Editar?tipo=1", "/Articulos/Editar",
                ("Datos.TipoPublicacion", "1"), ("Datos.Titulo","Prueba de mes inválido"), ("Datos.NumeroInforme","IT-INVALIDO"),
                ("Datos.CentroPublicacion","UNICDA"), ("Datos.MesPublicacion","13"), ("Datos.AnioPublicacion","2026"),
                ("Datos.TipoInformeId",tipo.ToString()), ("Datos.InvestigadorIds",autor.ToString()), ("Datos.PalabraClaveIds",palabra.ToString()));
            await Estado(r, HttpStatusCode.OK);
            var html = WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());
            Comprueba(html.Contains("Prueba de mes inválido") && html.Contains("MesPublicacion")
                && (await Leer<ResumenArticuloDto[]>("/api/articulos/buscar?texto=Prueba%20de%20mes%20inv")).Length == 0, "Se perdió o guardó un formulario inválido.");
        });
    }
}
