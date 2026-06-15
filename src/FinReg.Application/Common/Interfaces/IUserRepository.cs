using FinReg.Application.Common.Models;

namespace FinReg.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<UserCredentials?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<UserCredentials?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken ct = default);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct = default);
}
