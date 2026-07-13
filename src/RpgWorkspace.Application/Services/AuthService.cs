using System.Security.Cryptography;
using System.Text;
using RpgWorkspace.Application.DTOs.Auth;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class AuthService : IAuthService
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailGateway _emailGateway;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailGateway emailGateway,
        ISubscriptionRepository subscriptionRepository)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailGateway = emailGateway;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailAlreadyExists = await _userRepository.ExistsByEmailAsync(
            request.Email, cancellationToken);

        if (emailAlreadyExists)
            throw new InvalidOperationException("E-mail already in use.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        // Trial starts at signup (not at first character), so the countdown is honest from day one.
        var trial = Subscription.CreateTrial(user.Id, DateTime.UtcNow.AddDays(Subscription.DefaultTrialDays));
        await _subscriptionRepository.AddAsync(trial, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenGenerator.GenerateToken(
            user.Id.ToString(), user.Email, Enumerable.Empty<string>());

        return new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.DefaultCharacterId?.ToString());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _tokenGenerator.GenerateToken(
            user.Id.ToString(), user.Email, Enumerable.Empty<string>());

        return new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.DefaultCharacterId?.ToString());
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return; // Never reveal whether an email is registered.

        var existingTokens = await _passwordResetTokenRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        _passwordResetTokenRepository.RemoveRange(existingTokens);

        var rawToken = GenerateRawToken();
        var resetToken = PasswordResetToken.Create(user.Id, HashToken(rawToken), DateTime.UtcNow.Add(ResetTokenLifetime));
        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailGateway.SendPasswordResetEmailAsync(user.Email, user.Name, rawToken, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(HashToken(request.Token), cancellationToken);
        if (resetToken is null || !resetToken.IsValid(DateTime.UtcNow))
            throw new InvalidPasswordResetTokenException("This reset link is invalid or has expired.");

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken)
            ?? throw new InvalidPasswordResetTokenException("This reset link is invalid or has expired.");

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        resetToken.MarkUsed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
