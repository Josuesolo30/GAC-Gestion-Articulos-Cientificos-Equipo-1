using System.Net;
using GAC.Share.DTO.Response.GAC;
using static GAC.PruebasIntegracion.Program;

namespace GAC.PruebasIntegracion;

internal sealed partial class Escenarios
{
    private async Task ComprobarPantallasDeMantenimiento()
    {
        var reportes = await Pagina(web, "/InformesTecnicos");
        Comprueba(reportes.Contains("IT-DEMO-001") && reportes.Contains("Nuevo informe"), "El listado de informes perdió un acceso.");
        Comprueba((await Pagina(web, "/Catalogos/revistas")).Contains("Revista de Estudios Aplicados"), "No cargó el catálogo de revistas.");
        Comprueba((await Pagina(web, $"/Catalogos/Editar/ediciones-congreso/{edicion}")).Contains("2026-07-10"), "No cargó la fecha de la edición.");
        var formulario = "/Catalogos/Editar/tipos-informe";
        using (var crear = await Formulario(web, formulario, formulario, ("Datos.Nombre", "Tipo de informe web")))
            await Estado(crear, HttpStatusCode.Redirect);
        var id = (await Leer<TipoInformeResponseDto[]>("/api/tipos-informe")).Single(x => x.Nombre == "Tipo de informe web").Id;
        using (var editar = await Formulario(web, formulario + "/" + id, formulario + "/" + id, ("Datos.Nombre", "Tipo de informe actualizado")))
            await Estado(editar, HttpStatusCode.Redirect);
        Comprueba((await Leer<TipoInformeResponseDto>($"/api/tipos-informe/{id}")).Nombre == "Tipo de informe actualizado", "El formulario no actualizó el catálogo.");
        var retirar = $"/Catalogos/Eliminar/tipos-informe/{id}";
        using (var borrar = await Formulario(web, retirar, retirar))
            await Estado(borrar, HttpStatusCode.Redirect);
        using var eliminado = await api.GetAsync($"/api/tipos-informe/{id}");
        await Estado(eliminado, HttpStatusCode.NotFound);
        using var css = await otro.GetAsync("/css/site.css");
        await Estado(css, HttpStatusCode.OK);
        Console.WriteLine("OK  Mantenimiento de catálogos, listado de informes y recursos estáticos de la web.");
    }
}
