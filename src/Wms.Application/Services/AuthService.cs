using Wms.Application.DTOs.Auth;
using Wms.Application.Interfaces;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();

        if (await userRepo.ExistsAsync(u => u.Username == dto.Username, cancellationToken))
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");

        if (await userRepo.ExistsAsync(u => u.Email == dto.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            IsActive = true
        };

        await userRepo.AddAsync(user, cancellationToken);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);
        user.RefreshTokens.Add(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var users = await userRepo.FindAsync(
            u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail,
            cancellationToken);

        var user = users.FirstOrDefault();
        if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new InvalidOperationException("User account is deactivated.");

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        await tokenRepo.AddAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        var tokens = await tokenRepo.FindAsync(t => t.Token == dto.RefreshToken, cancellationToken);
        var existingToken = tokens.FirstOrDefault();

        if (existingToken == null || !existingToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or inactive.");

        existingToken.IsRevoked = true;
        existingToken.RevokedAtUtc = DateTime.UtcNow;

        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);
        existingToken.ReplacedByToken = newRefreshToken.Token;

        await tokenRepo.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        var tokens = await tokenRepo.FindAsync(t => t.Token == refreshToken, cancellationToken);
        var existingToken = tokens.FirstOrDefault();

        if (existingToken != null && existingToken.IsActive)
        {
            existingToken.IsRevoked = true;
            existingToken.RevokedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}