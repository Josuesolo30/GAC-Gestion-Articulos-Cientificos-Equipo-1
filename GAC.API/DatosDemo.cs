using GAC.Domain.Entity.Authentication;
using GAC.Domain.Entity.GAC;
using GAC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GAC.API;

public static class DatosDemo
{
    public static async Task PrepararAsync(IServiceProvider services, IConfiguration config)
    {
        var email = config["GacDemo:Email"]?.Trim();
        var password = config["GacDemo:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configura GacDemo:Email y GacDemo:Password en los secretos de GAC.API.");

        var db = services.GetRequiredService<AppDbContext>();
        var usuarios = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await usuarios.FindByEmailAsync(email) is null)
        {
            var cuenta = new ApplicationUser { UserName = email, Email = email, NombreCompleto = "Investigador de demostración", EmailConfirmed = true };
            var resultado = await usuarios.CreateAsync(cuenta, password);
            if (!resultado.Succeeded)
                throw new InvalidOperationException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        // Los ejemplos se cargan únicamente cuando solicitas el comando de preparación.
        // Arrancar la aplicación no vuelve a crear artículos que hayas eliminado.
        if (await db.Articulos.AnyAsync())
        {
            Console.WriteLine("La cuenta está disponible. Se conservó el catálogo existente.");
            return;
        }

        var agua = new Tema { Nombre = "Recursos hídricos" };
        var energia = new Tema { Nombre = "Energía" };
        var grupo = new GrupoInvestigacion { Nombre = "Grupo de investigación UNICDA", Temas = [new GrupoTema { Tema = agua }] };
        var ana = new Investigador { Nombre = "Ana Rivera", CentroTrabajo = "UNICDA", CorreoElectronico = "ana@example.test" };
        var luis = new Investigador { Nombre = "Luis Castillo", CentroTrabajo = "UNICDA" };
        var externa = new Investigador { Nombre = "María Ortiz", CentroTrabajo = "Centro de estudios ambientales" };
        ana.Grupos.Add(new InvestigadorGrupo { GrupoInvestigacion = grupo });
        luis.Grupos.Add(new InvestigadorGrupo { GrupoInvestigacion = grupo });
        ana.Temas.Add(new InvestigadorTema { Tema = agua });
        luis.Temas.Add(new InvestigadorTema { Tema = energia });
        externa.Temas.Add(new InvestigadorTema { Tema = agua });

        var palabraAgua = new PalabraClave { Texto = "agua" };
        var palabraMedicion = new PalabraClave { Texto = "medición" };
        var palabraEnergia = new PalabraClave { Texto = "energía" };
        var tipoInforme = new TipoInforme { Nombre = "Informe de investigación" };
        var congreso = new Congreso { Nombre = "Congreso de Investigación Aplicada", Tipo = TipoCongreso.Nacional, FrecuenciaCelebracion = "Anual", AnioPrimeraCelebracion = 2018 };
        var edicion = new EdicionCongreso
        {
            Congreso = congreso, Edicion = "Edición 2026", Ciudad = "Santo Domingo",
            FechaInicio = new DateTime(2026, 7, 10), FechaFin = new DateTime(2026, 7, 11)
        };
        var revista = new Revista
        {
            Nombre = "Revista de Estudios Aplicados", NombreEditor = "Comité editorial",
            AnioInicioPublicacion = 2015, FrecuenciaPublicacion = "Semestral",
            Temas = [new RevistaTema { Tema = energia }], Colaboradores = [new ColaboracionRevista { Investigador = ana }]
        };
        var informe = new InformeTecnico
        {
            Titulo = "Calidad del agua", NumeroInforme = "IT-DEMO-001", CentroPublicacion = "UNICDA",
            AnioPublicacion = 2026, MesPublicacion = 11, TipoInforme = tipoInforme,
            Autorias = [new Autoria { Investigador = ana }],
            PalabrasClave = [new ArticuloPalabraClave { PalabraClave = palabraAgua }],
            Ubicacion = new Ubicacion { GrupoInvestigacion = grupo, Tipo = TipoUbicacion.Laboratorio, Laboratorio = "Laboratorio A", Estanteria = "A-02" }
        };
        var acta = new PublicacionCongreso
        {
            Titulo = "Medición de lluvias urbanas", EdicionCongreso = edicion,
            Autorias = [new Autoria { Investigador = ana }, new Autoria { Investigador = luis }],
            PalabrasClave = [new ArticuloPalabraClave { PalabraClave = palabraMedicion }],
            Ubicacion = new Ubicacion { GrupoInvestigacion = grupo, Tipo = TipoUbicacion.Despacho, Investigador = luis, Despacho = "D-04" }
        };
        var revistaArticulo = new PublicacionRevista
        {
            Titulo = "Energía en espacios universitarios", Revista = revista, NumeroRevista = "2026-1",
            PaginaInicial = 15, PaginaFinal = 24, AnioPublicacion = 2026,
            Autorias = [new Autoria { Investigador = luis }],
            PalabrasClave = [new ArticuloPalabraClave { PalabraClave = palabraEnergia }]
        };
        db.Add(externa);
        db.Articulos.AddRange(informe, acta, revistaArticulo);
        await db.SaveChangesAsync();
        Console.WriteLine("Datos de demostración y cuenta de acceso preparados.");
    }
}
