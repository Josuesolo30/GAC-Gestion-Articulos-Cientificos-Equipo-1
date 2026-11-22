using System.Net;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using static GAC.PruebasIntegracion.Program;

namespace GAC.PruebasIntegracion;

internal sealed partial class Escenarios
{
    private Guid internacional;

    private EdicionCongresoRequestDto NuevaEdicion(Guid id, string? pais = null, DateTime? inicio = null, DateTime? fin = null,
        string edicionNombre = "Edición de prueba", string ciudad = "Santo Domingo") => new()
    {
        CongresoId = id, Edicion = edicionNombre, Ciudad = ciudad, Pais = pais,
        FechaInicio = inicio ?? new DateTime(2026, 10, 1), FechaFin = fin ?? new DateTime(2026, 10, 2)
    };

    private UbicacionRequestDto Ubicacion(Guid articulo, bool despacho = false, Guid? responsable = null,
        string? estante = "B-10", string? lugar = "Laboratorio de prueba", string? oficina = "D-08") => new()
    {
        ArticuloId = articulo, GrupoInvestigacionId = grupo, Tipo = despacho ? 2 : 1,
        Laboratorio = lugar, Estanteria = estante, InvestigadorId = responsable ?? miembro, Despacho = oficina
    };

    private async Task CongresosYRevistas()
    {
        await Caso("HU10-E1", "Congresos y ediciones nacionales e internacionales", async () =>
        {
            internacional = Id(await Post("/api/congresos", new { nombre = "Evento internacional", tipo = 2,
                frecuenciaCelebracion = "Anual", anioPrimeraCelebracion = 2023 }));
            var ed = Id(await Post("/api/ediciones-congreso", NuevaEdicion(internacional, "República Dominicana")));
            var id = Id(await Post("/api/publicaciones-congreso", new PublicacionCongresoRequestDto
                { Titulo = "Publicación internacional", InvestigadorIds = [autor], PalabraClaveIds = [palabra], EdicionCongresoId = ed }));
            var f = await Ficha(id);
            Comprueba(f.Congreso?.Tipo == 2 && f.Congreso.Nombre == "Evento internacional" && f.Congreso.AnioPrimeraCelebracion == 2023
                && f.Congreso.FrecuenciaCelebracion == "Anual" && f.Edicion?.Pais == "República Dominicana" && f.Edicion.Ciudad == "Santo Domingo"
                && f.Edicion.Edicion == "Edición de prueba" && f.Edicion.FechaInicio <= f.Edicion.FechaFin, "La ficha internacional está incompleta.");
            var nacional = await Ficha(congresoDemo);
            Comprueba(nacional.Congreso?.Tipo == 1 && nacional.Edicion?.Pais is null, "El país nacional era opcional.");
        });
        await Caso("HU10-E2", "País obligatorio al editar o crear una edición internacional", async () =>
        {
            Campo(await Post("/api/ediciones-congreso", NuevaEdicion(internacional), HttpStatusCode.BadRequest), "Pais");
            Campo(await Put($"/api/congresos/{congreso}", new { nombre = "Evento", tipo = 2, frecuenciaCelebracion = "Anual",
                anioPrimeraCelebracion = 2018 }, HttpStatusCode.BadRequest), "Tipo");
            Comprueba((await Ficha(congresoDemo)).Congreso?.Tipo == 1, "Se convirtió a internacional sin país.");
        });
        await Caso("HU10-E3", "Campos obligatorios de congreso, edición y publicación", async () =>
        {
            foreach (var dto in new[]
            {
                new CongresoRequestDto { Nombre = "", Tipo = 1, FrecuenciaCelebracion = "Anual", AnioPrimeraCelebracion = 2020 },
                new CongresoRequestDto { Nombre = "Evento", Tipo = 0, FrecuenciaCelebracion = "Anual", AnioPrimeraCelebracion = 2020 },
                new CongresoRequestDto { Nombre = "Evento", Tipo = 1, FrecuenciaCelebracion = "", AnioPrimeraCelebracion = 2020 },
                new CongresoRequestDto { Nombre = "Evento", Tipo = 1, FrecuenciaCelebracion = "Anual", AnioPrimeraCelebracion = 0 }
            }) await Post("/api/congresos", dto, HttpStatusCode.BadRequest);
            foreach (var dto in new[]
            {
                NuevaEdicion(congreso, edicionNombre: ""), NuevaEdicion(congreso, ciudad: ""),
                NuevaEdicion(congreso, inicio: default(DateTime)), NuevaEdicion(congreso, fin: default(DateTime))
            }) await Post("/api/ediciones-congreso", dto, HttpStatusCode.BadRequest);
            await Post("/api/publicaciones-congreso", new PublicacionCongresoRequestDto
                { Titulo = "Sin edición", InvestigadorIds = [autor], PalabraClaveIds = [palabra] }, HttpStatusCode.BadRequest);
        });
        await Caso("HU10-E4", "Fechas coherentes y edición de un día", async () =>
        {
            await Post("/api/ediciones-congreso", NuevaEdicion(congreso, fin: new DateTime(2026, 1, 1)), HttpStatusCode.BadRequest);
            await Post("/api/ediciones-congreso", NuevaEdicion(congreso, inicio: new DateTime(2017, 1, 1)), HttpStatusCode.BadRequest);
            var dia = new DateTime(2026, 10, 1);
            await Post("/api/ediciones-congreso", NuevaEdicion(congreso, inicio: dia, fin: dia));
        });
        await Caso("HU11-E1", "Revista con editor, temas, colaboradores y páginas", async () =>
        {
            var v = Id(await Post("/api/revistas", new { nombre = "Revista de prueba", nombreEditor = "Equipo editorial",
                anioInicioPublicacion = 2019, frecuenciaPublicacion = "Anual", temaIds = new[] { tema }, colaboradorIds = new[] { externa } }));
            var id = Id(await Post("/api/publicaciones-revista", PublicarRevista(v)));
            var f = await Ficha(id);
            Comprueba(f.Revista?.Nombre == "Revista de prueba" && f.Revista.NombreEditor == "Equipo editorial"
                && f.Revista.FrecuenciaPublicacion == "Anual" && f.Revista.AnioInicioPublicacion == 2019
                && f.TemasRevista.Single().Id == tema && f.ColaboradoresRevista.Single().Id == externa
                && f.PublicacionRevista?.PaginaInicial == 1 && f.PublicacionRevista.PaginaFinal == 2, "Perdió datos de revista.");
        });
        await Caso("HU11-E2", "Revista y publicación requieren sus datos", async () =>
        {
            RevistaRequestDto D(string n = "Revista", string e = "Editor", int a = 2019, string f = "Anual", bool t = true) =>
                new() { Nombre = n, NombreEditor = e, AnioInicioPublicacion = a, FrecuenciaPublicacion = f, TemaIds = t ? [tema] : [] };
            foreach (var dto in new[] { D(n: ""), D(e: ""), D(f: ""), D(a: 0), D(t: false) })
                await Post("/api/revistas", dto, HttpStatusCode.BadRequest);
            await Post("/api/publicaciones-revista", PublicarRevista(numero: ""), HttpStatusCode.BadRequest);
            await Post("/api/publicaciones-revista", PublicarRevista(cual: Guid.Empty), HttpStatusCode.BadRequest);
        });
        await Caso("HU11-E3", "El año del artículo no es anterior al de la revista", async () =>
        {
            var v = await Leer<RevistaResponseDto>($"/api/revistas/{revista}");
            await Post("/api/publicaciones-revista", PublicarRevista(anio: v.AnioInicioPublicacion - 1), HttpStatusCode.BadRequest);
            await Post("/api/publicaciones-revista", PublicarRevista(anio: v.AnioInicioPublicacion));
        });
        await Caso("HU11-E4", "Páginas positivas, ordenadas y de una sola página", async () =>
        {
            await Post("/api/publicaciones-revista", PublicarRevista(primero: 0, ultimo: 1), HttpStatusCode.BadRequest);
            await Post("/api/publicaciones-revista", PublicarRevista(primero: 8, ultimo: 7), HttpStatusCode.BadRequest);
            var id = Id(await Post("/api/publicaciones-revista", PublicarRevista(primero: 7, ultimo: 7)));
            Comprueba((await Ficha(id)).PublicacionRevista?.PaginaFinal == 7, "Rechazó artículo de una página.");
        });
    }

    private async Task UbicacionesYBusqueda()
    {
        Guid copiaLab = default, copiaDespacho = default;
        await Caso("HU12-E1", "Ubicación en laboratorio", async () =>
        {
            copiaLab = Id(await Post("/api/ubicaciones", Ubicacion(individual)));
            var f = await Ficha(individual);
            Comprueba(f.Articulo.Ubicacion?.Id == copiaLab && f.Articulo.Ubicacion.Laboratorio == "Laboratorio de prueba"
                && f.Articulo.Ubicacion.Estanteria == "B-10" && f.GrupoCopia?.Id == grupo, "No guardó el laboratorio.");
        });
        await Caso("HU12-E2", "Ubicación en despacho mediante el formulario", async () =>
        {
            var url = $"/Articulos/Copia/{revistaDemo}";
            using var r = await Formulario(web, url, url, ("Datos.GrupoInvestigacionId", grupo.ToString()), ("Datos.Tipo", "2"),
                ("Datos.InvestigadorId", miembro.ToString()), ("Datos.Despacho", "D-08"));
            await Estado(r, HttpStatusCode.Redirect);
            var f = await Ficha(revistaDemo);
            Comprueba(f.ResponsableDespacho?.Id == miembro && f.Articulo.Ubicacion?.Despacho == "D-08", "No guardó el despacho.");
            copiaDespacho = f.Articulo.Ubicacion!.Id;
        });
        await Caso("HU12-E3", "No guarda copias sin ubicación completa", async () =>
        {
            await Post("/api/ubicaciones", Ubicacion(altaWeb, estante: null), HttpStatusCode.BadRequest);
            await Post("/api/ubicaciones", Ubicacion(altaWeb, lugar: null), HttpStatusCode.BadRequest);
            await Post("/api/ubicaciones", Ubicacion(altaWeb, despacho: true, oficina: null), HttpStatusCode.BadRequest);
            await Post("/api/ubicaciones", new { articuloId = altaWeb, grupoInvestigacionId = grupo }, HttpStatusCode.BadRequest);
            Comprueba((await Ficha(altaWeb)).Articulo.Ubicacion is null, "Una ubicación incompleta quedó guardada.");
        });
        await Caso("HU12-E4", "Un externo no puede tener un despacho del grupo", async () =>
        {
            Campo(await Post("/api/ubicaciones", Ubicacion(altaWeb, despacho: true, responsable: externa), HttpStatusCode.BadRequest), "InvestigadorId");
            Comprueba((await Ficha(altaWeb)).Articulo.Ubicacion is null, "Se asignó despacho al externo.");
        });
        await Caso("HU13-E1", "Traslado conserva una sola ubicación y su ID", async () =>
        {
            await Put($"/api/ubicaciones/{copiaLab}", Ubicacion(individual, despacho: true));
            var f = await Ficha(individual);
            Comprueba(f.Articulo.Ubicacion?.Id == copiaLab && f.Articulo.Ubicacion.Tipo == 2
                && f.Articulo.Ubicacion.Laboratorio is null && f.ResponsableDespacho?.Id == miembro, "No reemplazó la ubicación de laboratorio.");
            await Post("/api/ubicaciones", Ubicacion(individual), HttpStatusCode.Conflict);
            Comprueba((await Leer<UbicacionResponseDto[]>("/api/ubicaciones")).Count(x => x.ArticuloId == individual) == 1, "Dejó dos ubicaciones.");
        });
        await Caso("HU13-E2", "Retirar la copia desde la web conserva la ficha", async () =>
        {
            var url = $"/Articulos/Copia/{revistaDemo}";
            using var r = await Formulario(web, url, url + "?handler=Retirar");
            await Estado(r, HttpStatusCode.Redirect);
            Comprueba((await Ficha(revistaDemo)).Articulo.Ubicacion is null, "La copia todavía existe.");
            using var anterior = await api.GetAsync($"/api/ubicaciones/{copiaDespacho}");
            await Estado(anterior, HttpStatusCode.NotFound);
        });
        await Caso("HU13-E3", "Cancelar traslado o retiro conserva la ubicación", async () =>
        {
            var html = await Pagina(web, $"/Articulos/Copia/{individual}");
            Comprueba(html.Contains("Cancelar") && html.Contains("Confirmar que ya no hay copia"), "Falta el control de confirmación.");
            await Pagina(web, $"/Articulos/Detalle/{individual}");
            Comprueba((await Ficha(individual)).Articulo.Ubicacion?.Id == copiaLab, "Cambió la ubicación solo al abrirla.");
        });
        await Caso("HU14-E1", "Busca por título, autor y palabra sin distinción de mayúsculas", async () =>
        {
            foreach (var (campo,texto) in new[] { ("titulo", "cALIDAd"), ("autor", "aNa rIVERA"), ("palabra", "aGUA") })
            {
                var resultados = await Leer<ResumenArticuloDto[]>($"/api/articulos/buscar?campo={campo}&texto={Uri.EscapeDataString(texto)}");
                Comprueba(resultados.Any(x => x.Id == informeDemo), "No encontró el artículo por " + campo);
            }
        });
        await Caso("HU14-E2", "No hay resultados para texto no encontrado", async () =>
        {
            var html = await Pagina(web, "/Articulos?campo=titulo&texto=zzj_no_existe");
            Comprueba(html.Contains("No se encontraron coincidencias.") && html.Contains("zzj_no_existe"), "Falta estado vacío con el texto buscado.");
        });
        await Caso("HU14-E3", "Limpiar filtros recupera el catálogo", async () =>
        {
            var filtrado = await Pagina(web, "/Articulos?tipo=1");
            var limpio = await Pagina(web,"/Articulos");
            Comprueba(filtrado.Contains("Limpiar filtros") && !filtrado.Contains("Medición de lluvias urbanas")
                && limpio.Contains("Medición de lluvias urbanas") && limpio.Contains("Energía en espacios universitarios"), "El filtro se quedó activo.");
        });
        await Caso("HU15-E1", "Filtrar por cada modalidad", async () =>
        {
            foreach (var t in new[] { 1,2,3 })
            {
                var r = await Leer<ResumenArticuloDto[]>($"/api/articulos/buscar?tipo={t}");
                Comprueba(r.Length > 0 && r.All(x => x.TipoPublicacion == t), "No filtró por modalidad.");
            }
        });
        await Caso("HU15-E2", "Combina texto, modalidad y copia", async () =>
        {
            var con = await Leer<ResumenArticuloDto[]>("/api/articulos/buscar?campo=palabra&texto=agua&tipo=1&tieneCopia=true");
            var sin = await Leer<ResumenArticuloDto[]>("/api/articulos/buscar?campo=palabra&texto=agua&tipo=1&tieneCopia=false");
            Comprueba(con.Any(x => x.Id == informeDemo) && sin.Any(x => x.Id == altaWeb)
                && con.All(x => x.TipoPublicacion == 1 && x.TieneCopia) && sin.All(x => x.TipoPublicacion == 1 && !x.TieneCopia),
                "Falla la combinación de filtros.");
        });
        await Caso("HU15-E3", "Combinación de filtros sin resultados", async () =>
        {
            var antes = (await Leer<ArticuloResponseDto[]>("/api/articulos")).Length;
            Comprueba((await Leer<ResumenArticuloDto[]>("/api/articulos/buscar?campo=titulo&texto=zzj_no_existe&tipo=2&tieneCopia=true")).Length == 0
                && (await Leer<ArticuloResponseDto[]>("/api/articulos")).Length == antes, "Un filtro modificó datos.");
        });
    }
}
