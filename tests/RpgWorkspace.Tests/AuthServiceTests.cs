using RpgWorkspace.Application.DTOs.Auth;
using RpgWorkspace.Application.Services;
using RpgWorkspace.Domain.Enums;
using Xunit;

namespace RpgWorkspace.Tests;

public class AuthServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeSubscriptionRepository _subscriptions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeEmailGateway _emails = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _users,
            _unitOfWork,
            new FakeTokenGenerator(),
            new FakePasswordHasher(),
            new FakePasswordResetTokenRepository(),
            _emails,
            _subscriptions);
    }

    [Fact]
    public async Task Register_creates_user_and_trial_subscription_together()
    {
        var response = await _service.RegisterAsync(
            new RegisterRequest("Aventureiro", "novo@teste.com", "SenhaForte1!"));

        var user = Assert.Single(_users.Items);
        var subscription = Assert.Single(_subscriptions.Items);

        Assert.Equal(user.Id, subscription.UserId);
        Assert.Equal(SubscriptionStatus.Trialing, subscription.Status);
        Assert.True(subscription.IsActive());
        Assert.NotNull(subscription.CurrentPeriodEnd);

        var daysLeft = (subscription.CurrentPeriodEnd!.Value - DateTime.UtcNow).TotalDays;
        Assert.InRange(daysLeft, 6.9, 7.1);

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
    }

    [Fact]
    public async Task Register_rejects_duplicate_email()
    {
        await _service.RegisterAsync(new RegisterRequest("A", "dup@teste.com", "SenhaForte1!"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(new RegisterRequest("B", "dup@teste.com", "OutraSenha1!")));

        Assert.Single(_users.Items);
        Assert.Single(_subscriptions.Items);
    }

    [Fact]
    public async Task Login_accepts_correct_password_and_rejects_wrong_one()
    {
        await _service.RegisterAsync(new RegisterRequest("A", "login@teste.com", "SenhaCerta1!"));

        var ok = await _service.LoginAsync(new LoginRequest("login@teste.com", "SenhaCerta1!"));
        Assert.False(string.IsNullOrEmpty(ok.AccessToken));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest("login@teste.com", "SenhaErrada1!")));
    }

    [Fact]
    public async Task Forgot_password_never_reveals_whether_email_exists()
    {
        // Unknown email: silent no-op, nothing sent.
        await _service.RequestPasswordResetAsync(new ForgotPasswordRequest("naoexiste@teste.com"));
        Assert.Empty(_emails.Sent);

        // Known email: one reset email goes out.
        await _service.RegisterAsync(new RegisterRequest("A", "real@teste.com", "SenhaForte1!"));
        await _service.RequestPasswordResetAsync(new ForgotPasswordRequest("real@teste.com"));
        Assert.Single(_emails.Sent);
    }
}
