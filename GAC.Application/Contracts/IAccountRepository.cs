using GAC.Share.DTO.Request.Account;
using GAC.Share.DTO.Response;
using GAC.Share.DTO.Response.Account;

namespace GAC.Application.Contracts;

/// <summary>Contrato para la futura implementación de cuentas en Infraestructura.</summary>
public interface IAccountRepository
{
    Task<GeneralResponseDto> CrearCuentaAsync(CreateAccountRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<LoginResponseDto> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<LoginResponseDto> RenovarTokenAsync(RefreshTokenRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> CrearRolAsync(CreateRoleRequestDto solicitud, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleDto>> ListarRolesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserWithRoleResponseDto>> ListarUsuariosAsync(CancellationToken cancellationToken = default);

    Task<GeneralResponseDto> CambiarRolAsync(ChangeUserRoleRequestDto solicitud, CancellationToken cancellationToken = default);
}
