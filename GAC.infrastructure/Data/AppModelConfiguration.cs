using global::GAC.Domain.Entity.Authentication;
using global::GAC.Domain.Entity.GAC;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GAC.Infrastructure.Data;

internal static class AppModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        // TPH: una ficha siempre es uno de estos tres tipos concretos.
        model.Entity<Articulo>().ToTable("Articulos");
        model.Entity<Articulo>()
            .HasDiscriminator(a => a.TipoPublicacion)
            .HasValue<InformeTecnico>(TipoPublicacion.InformeTecnico)
            .HasValue<PublicacionCongreso>(TipoPublicacion.Congreso)
            .HasValue<PublicacionRevista>(TipoPublicacion.Revista);

        model.Entity<InformeTecnico>()
            .Property(x => x.AnioPublicacion).HasColumnName("AnioInforme");
        model.Entity<PublicacionRevista>()
            .Property(x => x.AnioPublicacion).HasColumnName("AnioArticuloRevista");

        model.Entity<Autoria>()
            .HasKey(x => new { x.ArticuloId, x.InvestigadorId });
        model.Entity<Autoria>()
            .HasOne(x => x.Articulo).WithMany(x => x.Autorias)
            .HasForeignKey(x => x.ArticuloId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Autoria>()
            .HasOne(x => x.Investigador).WithMany(x => x.Autorias)
            .HasForeignKey(x => x.InvestigadorId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<ArticuloPalabraClave>()
            .HasKey(x => new { x.ArticuloId, x.PalabraClaveId });
        model.Entity<ArticuloPalabraClave>()
            .HasOne(x => x.Articulo).WithMany(x => x.PalabrasClave)
            .HasForeignKey(x => x.ArticuloId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<ArticuloPalabraClave>()
            .HasOne(x => x.PalabraClave).WithMany(x => x.Articulos)
            .HasForeignKey(x => x.PalabraClaveId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<InvestigadorTema>()
            .HasKey(x => new { x.InvestigadorId, x.TemaId });
        model.Entity<InvestigadorTema>()
            .HasOne(x => x.Investigador).WithMany(x => x.Temas)
            .HasForeignKey(x => x.InvestigadorId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<InvestigadorTema>()
            .HasOne(x => x.Tema).WithMany(x => x.Investigadores)
            .HasForeignKey(x => x.TemaId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<RevistaTema>()
            .HasKey(x => new { x.RevistaId, x.TemaId });
        model.Entity<RevistaTema>()
            .HasOne(x => x.Revista).WithMany(x => x.Temas)
            .HasForeignKey(x => x.RevistaId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<RevistaTema>()
            .HasOne(x => x.Tema).WithMany(x => x.Revistas)
            .HasForeignKey(x => x.TemaId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<EdicionCongreso>()
            .HasOne(x => x.Congreso).WithMany(x => x.Ediciones)
            .HasForeignKey(x => x.CongresoId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<PublicacionCongreso>()
            .HasOne(x => x.EdicionCongreso).WithMany(x => x.Articulos)
            .HasForeignKey(x => x.EdicionCongresoId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<PublicacionRevista>()
            .HasOne(x => x.Revista).WithMany(x => x.Articulos)
            .HasForeignKey(x => x.RevistaId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<Ubicacion>()
            .HasOne(x => x.Articulo).WithOne(x => x.Ubicacion)
            .HasForeignKey<Ubicacion>(x => x.ArticuloId)
            .OnDelete(DeleteBehavior.Cascade);
        model.Entity<Ubicacion>()
            .HasOne(x => x.Investigador).WithMany(x => x.UbicacionesDespacho)
            .HasForeignKey(x => x.InvestigadorId).OnDelete(DeleteBehavior.Restrict);

        model.Entity<RefreshToken>()
            .HasOne(x => x.User).WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        // GRUPOS E INVESTIGADORES

        model.Entity<InvestigadorGrupo>()
            .HasKey(x => new
            {
                x.GrupoInvestigacionId,
                x.InvestigadorId
            });

        model.Entity<InvestigadorGrupo>()
            .HasOne(x => x.GrupoInvestigacion)
            .WithMany(x => x.Miembros)
            .HasForeignKey(x => x.GrupoInvestigacionId)
            .OnDelete(DeleteBehavior.Cascade);

        model.Entity<InvestigadorGrupo>()
            .HasOne(x => x.Investigador)
            .WithMany(x => x.Grupos)
            .HasForeignKey(x => x.InvestigadorId)
            .OnDelete(DeleteBehavior.Restrict);


        // GRUPOS Y TEMAS

        model.Entity<GrupoTema>()
            .HasKey(x => new
            {
                x.GrupoInvestigacionId,
                x.TemaId
            });

        model.Entity<GrupoTema>()
            .HasOne(x => x.GrupoInvestigacion)
            .WithMany(x => x.Temas)
            .HasForeignKey(x => x.GrupoInvestigacionId)
            .OnDelete(DeleteBehavior.Cascade);

        model.Entity<GrupoTema>()
            .HasOne(x => x.Tema)
            .WithMany(x => x.Grupos)
            .HasForeignKey(x => x.TemaId)
            .OnDelete(DeleteBehavior.Restrict);


        // COLABORADORES DE REVISTAS

        model.Entity<ColaboracionRevista>()
            .HasKey(x => new
            {
                x.RevistaId,
                x.InvestigadorId
            });

        model.Entity<ColaboracionRevista>()
            .HasOne(x => x.Revista)
            .WithMany(x => x.Colaboradores)
            .HasForeignKey(x => x.RevistaId)
            .OnDelete(DeleteBehavior.Cascade);

        model.Entity<ColaboracionRevista>()
            .HasOne(x => x.Investigador)
            .WithMany(x => x.ColaboracionesRevista)
            .HasForeignKey(x => x.InvestigadorId)
            .OnDelete(DeleteBehavior.Restrict);


        // CATALOGO DE TIPOS DE INFORME

        model.Entity<InformeTecnico>()
            .HasOne(x => x.TipoInforme)
            .WithMany(x => x.Informes)
            .HasForeignKey(x => x.TipoInformeId)
            .OnDelete(DeleteBehavior.Restrict);


        // GRUPO QUE CONSERVA LA COPIA

        model.Entity<Ubicacion>()
            .HasOne(x => x.GrupoInvestigacion)
            .WithMany(x => x.Ubicaciones)
            .HasForeignKey(x => x.GrupoInvestigacionId)
            .OnDelete(DeleteBehavior.Restrict);


        // EL INVESTIGADOR DEL DESPACHO DEBE PERTENECER AL GRUPO

        model.Entity<Ubicacion>()
            .HasOne<InvestigadorGrupo>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.GrupoInvestigacionId,
                x.InvestigadorId
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
