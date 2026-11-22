using GAC.Share.DTO.Request.Account;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.Account;

namespace GAC.Application.Interfaces;

/// <summary>Operaciones de cuentas que podrá consumir la presentación mediante la API.</summary>
public interface IAccountService
{
    Task<GeneralResponseDto> CrearCuentaAsync(CreateAccountRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<LoginResponseDto> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<LoginResponseDto> RenovarTokenAsync(RefreshTokenRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> CrearRolAsync(CreateRoleRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleDto>> ListarRolesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserWithRoleResponseDto>> ListarUsuariosAsync(CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> CambiarRolAsync(ChangeUserRoleRequestDto solicitud, CancellationToken cancellationToken = default);
}
