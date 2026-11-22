using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using GAC.Application.Contracts;
using GAC.Infrastructure.Data;
using GAC.Share.DTO.Response;
using Microsoft.EntityFrameworkCore;

namespace GAC.Infrastructure.Repositories;

internal sealed class ErrorCatalogo(int estado, string mensaje, string? campo = null) : Exception(mensaje)
{
    public int Estado { get; } = estado;
    public string? Campo { get; } = campo;
}

internal static class EscrituraEf
{
    public static void Validar(object solicitud)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(solicitud, new ValidationContext(solicitud), errors, true))
        {
            var first = errors[0];
            throw new ErrorCatalogo(400, first.ErrorMessage ?? "Revisa los datos enviados.", first.MemberNames.FirstOrDefault());
        }
    }

    public static async Task<GeneralResponseDto> AplicarAsync(AppDbContext db,
        Func<Task<Guid?>> accion, CancellationToken ct)
    {
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var id = await accion();
            await db.SaveChangesAsync(ct);
            await transaccion.CommitAsync(ct);
            return new GeneralResponseDto(true, Id: id);
        }
        catch (ErrorCatalogo error)
        {
            await transaccion.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return new GeneralResponseDto(false, error.Message, Errors: error.Campo is { } campo
                ? new Dictionary<string, string[]> { [campo] = [error.Message] }
                : null, Estado: error.Estado);
        }
        catch (DbUpdateException)
        {
            await transaccion.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return new GeneralResponseDto(false, "No se pudo guardar porque otra operación cambió un registro relacionado. Recarga la página y vuelve a intentarlo.", Estado: 409);
        }
    }
}

internal class LecturaEf<TEntity, TRespuesta, TClave>(
    Func<IQueryable<TEntity>> consulta,
    Func<TClave, Expression<Func<TEntity, bool>>> porClave,
    Func<TEntity, TRespuesta> proyectar)
    : ICatalogoLectura<TRespuesta, TClave>
    where TEntity : class where TRespuesta : class
{
    protected Expression<Func<TEntity, bool>> FiltroClave(TClave clave) => porClave(clave);

    public async Task<IReadOnlyList<TRespuesta>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await consulta().AsNoTracking().ToListAsync(cancellationToken)).Select(proyectar).ToArray();

    public async Task<TRespuesta?> ObtenerAsync(TClave clave, CancellationToken cancellationToken = default) =>
        await consulta().AsNoTracking().FirstOrDefaultAsync(porClave(clave), cancellationToken) is { } fila
            ? proyectar(fila) : null;
}

internal sealed class CrudEf<TEntity, TSolicitud, TRespuesta>(
    AppDbContext db, Func<IQueryable<TEntity>> consulta, Expression<Func<TEntity, Guid>> idExpr,
    Func<TEntity, TRespuesta> proyectar,
    Func< TEntity, TSolicitud, CancellationToken, Task> asignar,
    Func<TEntity, CancellationToken, Task>? antesDeEliminar = null) :
    ICatalogoCrud<TSolicitud, TRespuesta, Guid>
    where TEntity : class, new() where TSolicitud : class where TRespuesta : class
{
    private readonly Func<TEntity, Guid> _id = idExpr.Compile();

    private Expression<Func<TEntity, bool>> PorId(Guid id) =>
        Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(idExpr.Body, Expression.Constant(id)), idExpr.Parameters);

    private async Task<TEntity> BuscarParaCambiar(Guid id, CancellationToken ct) =>
        await consulta().FirstOrDefaultAsync(PorId(id), ct)
        ?? throw new ErrorCatalogo(404, "El registro seleccionado ya no existe.");

    public async Task<IReadOnlyList<TRespuesta>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await consulta().AsNoTracking().ToListAsync(cancellationToken)).Select(proyectar).ToArray();

    public async Task<TRespuesta?> ObtenerAsync(Guid clave, CancellationToken cancellationToken = default) =>
        await consulta().AsNoTracking().FirstOrDefaultAsync(PorId(clave), cancellationToken) is { } fila
            ? proyectar(fila) : null;

    public Task<GeneralResponseDto> CrearAsync(TSolicitud solicitud, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            EscrituraEf.Validar(solicitud);
            var entidad = new TEntity();
            db.Add(entidad);
            await asignar(entidad, solicitud, cancellationToken);
            return _id(entidad);
        }, cancellationToken);

    public Task<GeneralResponseDto> ActualizarAsync(Guid clave, TSolicitud solicitud, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            EscrituraEf.Validar(solicitud);
            await asignar(await BuscarParaCambiar(clave, cancellationToken), solicitud, cancellationToken);
            return clave;
        }, cancellationToken);

    public Task<GeneralResponseDto> EliminarAsync(Guid clave, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            var entidad = await BuscarParaCambiar(clave, cancellationToken);
            if (antesDeEliminar is not null) await antesDeEliminar(entidad, cancellationToken);
            db.Remove(entidad);
            return clave;
        }, cancellationToken);
}

internal sealed class VinculoEf<TEntity, TSolicitud, TRespuesta>(
    AppDbContext db, Func<(Guid, Guid), Expression<Func<TEntity, bool>>> porClave,
    Func<TSolicitud, (Guid, Guid)> claveDe,
    Func<(Guid, Guid), TEntity> nuevo, Func<TEntity, TRespuesta> proyectar,
    Func<(Guid, Guid), CancellationToken, Task> validarClaves,
    Func<TEntity, CancellationToken, Task>? antesDeEliminar = null)
    : LecturaEf<TEntity, TRespuesta, (Guid, Guid)>(() => db.Set<TEntity>(), porClave, proyectar),
      ICatalogoCrud<TSolicitud, TRespuesta, (Guid, Guid)>
    where TEntity : class where TSolicitud : class where TRespuesta : class
{
    public Task<GeneralResponseDto> CrearAsync(TSolicitud solicitud, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            EscrituraEf.Validar(solicitud);
            var clave = claveDe(solicitud);
            await validarClaves(clave, cancellationToken);
            if (!await db.Set<TEntity>().AnyAsync(FiltroClave(clave), cancellationToken))
                db.Add(nuevo(clave));
            return null;
        }, cancellationToken);

    public Task<GeneralResponseDto> ActualizarAsync((Guid, Guid) clave, TSolicitud solicitud, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            EscrituraEf.Validar(solicitud);
            var actual = await db.Set<TEntity>().FirstOrDefaultAsync(FiltroClave(clave), cancellationToken)
                ?? throw new ErrorCatalogo(404, "La relación seleccionada ya no existe.");
            var otra = claveDe(solicitud);
            if (clave == otra) return null;
            await validarClaves(otra, cancellationToken);
            if (antesDeEliminar is not null) await antesDeEliminar(actual, cancellationToken);
            if (!await db.Set<TEntity>().AnyAsync(FiltroClave(otra), cancellationToken)) db.Add(nuevo(otra));
            db.Remove(actual);
            return null;
        }, cancellationToken);

    public Task<GeneralResponseDto> EliminarAsync((Guid, Guid) clave, CancellationToken cancellationToken = default) =>
        EscrituraEf.AplicarAsync(db, async () =>
        {
            var actual = await db.Set<TEntity>().FirstOrDefaultAsync(FiltroClave(clave), cancellationToken)
                ?? throw new ErrorCatalogo(404, "La relación seleccionada ya no existe.");
            if (antesDeEliminar is not null) await antesDeEliminar(actual, cancellationToken);
            db.Remove(actual);
            return null;
        }, cancellationToken);
}
