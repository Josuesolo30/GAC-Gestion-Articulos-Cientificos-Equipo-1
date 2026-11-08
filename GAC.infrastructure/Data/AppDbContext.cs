using global::GAC.Domain.Entity.Authentication;
using global::GAC.Domain.Entity.GAC;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GAC.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Investigador> Investigadores => Set<Investigador>();
    public DbSet<Autoria> Autorias => Set<Autoria>();
    public DbSet<PalabraClave> PalabrasClave => Set<PalabraClave>();
    public DbSet<ArticuloPalabraClave> ArticulosPalabrasClave => Set<ArticuloPalabraClave>();
    public DbSet<Tema> Temas => Set<Tema>();
    public DbSet<InvestigadorTema> InvestigadoresTemas => Set<InvestigadorTema>();
    public DbSet<RevistaTema> RevistasTemas => Set<RevistaTema>();
    public DbSet<InformeTecnico> InformesTecnicos => Set<InformeTecnico>();
    public DbSet<Congreso> Congresos => Set<Congreso>();
    public DbSet<EdicionCongreso> EdicionesCongreso => Set<EdicionCongreso>();
    public DbSet<PublicacionCongreso> PublicacionesCongreso => Set<PublicacionCongreso>();
    public DbSet<Revista> Revistas => Set<Revista>();
    public DbSet<PublicacionRevista> PublicacionesRevista => Set<PublicacionRevista>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<GrupoInvestigacion> GruposInvestigacion
    => Set<GrupoInvestigacion>();

    public DbSet<InvestigadorGrupo> InvestigadoresGrupos
        => Set<InvestigadorGrupo>();

    public DbSet<GrupoTema> GruposTemas
        => Set<GrupoTema>();

    public DbSet<TipoInforme> TiposInforme
        => Set<TipoInforme>();

    public DbSet<ColaboracionRevista> ColaboracionesRevistas
        => Set<ColaboracionRevista>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        AppModelConfiguration.Configure(builder);
    }
}
