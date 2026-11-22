namespace GAC.Application.Constants;

/// <summary>Rutas relativas acordadas para los futuros controladores de GAC.</summary>
public static class GacApiRoutes
{
    public const string ClientName = "GacApi";
    public const string ConfigurationKey = "GacApi:BaseUrl";

    public const string Articulos = "api/articulos";
    public const string InformesTecnicos = "api/informes-tecnicos";
    public const string PublicacionesCongreso = "api/publicaciones-congreso";
    public const string PublicacionesRevista = "api/publicaciones-revista";
    public const string TiposInforme = "api/tipos-informe";
    public const string Congresos = "api/congresos";
    public const string EdicionesCongreso = "api/ediciones-congreso";
    public const string Revistas = "api/revistas";
    public const string Ubicaciones = "api/ubicaciones";
    public const string Investigadores = "api/investigadores";
    public const string GruposInvestigacion = "api/grupos-investigacion";
    public const string Temas = "api/temas";
    public const string PalabrasClave = "api/palabras-clave";

    public const string Autorias = "api/autorias";
    public const string ArticulosPalabrasClave = "api/articulos-palabras-clave";
    public const string InvestigadoresGrupos = "api/investigadores-grupos";
    public const string InvestigadoresTemas = "api/investigadores-temas";
    public const string GruposTemas = "api/grupos-temas";
    public const string RevistasTemas = "api/revistas-temas";
    public const string ColaboracionesRevista = "api/colaboraciones-revista";

    public const string Cuentas = "api/cuentas";
    public const string Login = Cuentas + "/login";
    public const string RefreshToken = Cuentas + "/refresh-token";
    public const string Roles = Cuentas + "/roles";
    public const string Usuarios = Cuentas + "/usuarios";
    public const string CambiarRol = Usuarios + "/rol";

    internal static string ConId(string recurso, Guid id) => $"{recurso}/{id:D}";

    internal static string ConPar(string recurso, Guid primero, Guid segundo) =>
        $"{recurso}/{primero:D}/{segundo:D}";
}
